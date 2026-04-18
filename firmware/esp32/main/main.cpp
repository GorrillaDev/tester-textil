#include <ctype.h>
#include <errno.h>
#include <inttypes.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <strings.h>
#include "freertos/FreeRTOS.h"
#include "freertos/event_groups.h"
#include "freertos/queue.h"
#include "freertos/semphr.h"
#include "freertos/task.h"
#include "esp_check.h"
#include "esp_err.h"
#include "esp_event.h"
#include "esp_log.h"
#include "esp_mac.h"
#include "esp_netif.h"
#include "esp_wifi.h"
#include "nvs_flash.h"
#include "lwip/inet.h"
#include "lwip/ip4_addr.h"
#include "lwip/sockets.h"
#include "driver/uart.h"
#include "driver/uart_vfs.h"
#include "esp_twai.h"
#include "esp_twai_onchip.h"
#include "tinyusb.h"
#include "tinyusb_default_config.h"
#include "tusb.h"
#include "class/vendor/vendor_device.h"
#include "line_codec.h"
#include "command_processor.h"

#define APP_WIFI_SSID                        "ACURATEX_NET"
#define APP_WIFI_PASS                        "acuratex1"
#define APP_WIFI_STATIC_IP                   "192.168.137.2"
#define APP_WIFI_STATIC_NETMASK              "255.255.255.0"
#define APP_WIFI_STATIC_GW                   "192.168.137.1"

#define APP_TCP_PORT                         3333
#define APP_TCP_RX_BUFFER_SIZE               128
#define APP_LINE_BUFFER_SIZE                 160
#define APP_USB_RX_QUEUE_DEPTH               8

#define APP_CAN_BITRATE                      1000000
#define APP_CAN_TX_QUEUE_DEPTH               5
#define APP_CAN_TX_TIMEOUT_MS                100
#define APP_CAN_SWITCH_WAIT_MS               100

#define APP_USB_VENDOR_ID                    0xCAFE
#define APP_USB_PRODUCT_ID                   0x4030
#define APP_USB_BCD_DEVICE                   0x0100
#define APP_USB_VENDOR_REQUEST_MICROSOFT     0x20
#define APP_USB_MS_OS_20_DESC_LEN            0xB2
#define APP_USB_VENDOR_ITF                   0
#define APP_USB_BULK_OUT_EP                  0x01
#define APP_USB_BULK_IN_EP                   0x81
#define APP_USB_BULK_EP_SIZE                 64
#define APP_USB_TX_RETRY_COUNT               5
#define APP_USB_INTERFACE_GUID               "{D7761D50-5F1B-4D33-95F2-733B0E5F2EED}"
#define APP_USB_CONFIG_TOTAL_LEN             (TUD_CONFIG_DESC_LEN + TUD_VENDOR_DESC_LEN)
#define APP_USB_BOS_TOTAL_LEN                (TUD_BOS_DESC_LEN + TUD_BOS_MICROSOFT_OS_DESC_LEN)

/*
 * Pines de ejemplo para ESP32-S3.
 * Ajustalos al cableado real antes de integrar los transceivers definitivos.
 */
#define CAN1_TX_GPIO                         GPIO_NUM_8
#define CAN1_RX_GPIO                         GPIO_NUM_9
#define CAN2_TX_GPIO                         GPIO_NUM_10
#define CAN2_RX_GPIO                         GPIO_NUM_11

#define WIFI_CONNECTED_BIT                   BIT0

static const char *TAG = "acuratex_fw";

typedef enum {
    APP_CAN_BUS_NONE = 0,
    APP_CAN_BUS_1 = 1,
    APP_CAN_BUS_2 = 2,
} app_can_bus_t;

typedef struct {
    const char *name;
    gpio_num_t tx_gpio;
    gpio_num_t rx_gpio;
} app_can_bus_config_t;

typedef struct {
    char line[APP_LINE_BUFFER_SIZE];
} app_usb_command_t;

static const app_can_bus_config_t s_can_bus_cfg[] = {
    [APP_CAN_BUS_1] = {
        .name = "CAN1",
        .tx_gpio = CAN1_TX_GPIO,
        .rx_gpio = CAN1_RX_GPIO,
    },
    [APP_CAN_BUS_2] = {
        .name = "CAN2",
        .tx_gpio = CAN2_TX_GPIO,
        .rx_gpio = CAN2_RX_GPIO,
    },
};

static EventGroupHandle_t s_wifi_event_group;
static SemaphoreHandle_t s_command_mutex;
static QueueHandle_t s_usb_rx_queue;
static esp_netif_t *s_wifi_sta_netif;
static twai_node_handle_t s_can_node = NULL;
static app_can_bus_t s_active_bus = APP_CAN_BUS_NONE;
static char s_wifi_ip_addr[16] = "0.0.0.0";
static bool s_usb_mounted = false;
static char s_usb_rx_line[APP_LINE_BUFFER_SIZE] = {0};
static size_t s_usb_rx_line_len = 0;
static char s_usb_serial_number[17] = "000000000000";
static uint32_t s_usb_rx_packets = 0;
static uint32_t s_usb_tx_packets = 0;

static const tusb_desc_device_t s_usb_device_descriptor = {
    .bLength = sizeof(tusb_desc_device_t),
    .bDescriptorType = TUSB_DESC_DEVICE,
    .bcdUSB = 0x0210,
    .bDeviceClass = TUSB_CLASS_VENDOR_SPECIFIC,
    .bDeviceSubClass = 0x00,
    .bDeviceProtocol = 0x00,
    .bMaxPacketSize0 = CFG_TUD_ENDPOINT0_SIZE,
    .idVendor = APP_USB_VENDOR_ID,
    .idProduct = APP_USB_PRODUCT_ID,
    .bcdDevice = APP_USB_BCD_DEVICE,
    .iManufacturer = 0x01,
    .iProduct = 0x02,
    .iSerialNumber = 0x03,
    .bNumConfigurations = 0x01,
};

static const char *s_usb_string_descriptor[] = {
    (char[]){0x09, 0x04},
    "Acuratex",
    "Acuratex Control Bridge",
    s_usb_serial_number,
    "Acuratex Vendor Interface",
};

static const uint8_t s_usb_configuration_descriptor[] = {
    TUD_CONFIG_DESCRIPTOR(1, 1, 0, APP_USB_CONFIG_TOTAL_LEN, 0, 100),
    TUD_VENDOR_DESCRIPTOR(APP_USB_VENDOR_ITF, 4, APP_USB_BULK_OUT_EP, APP_USB_BULK_IN_EP, APP_USB_BULK_EP_SIZE),
};

static const uint8_t s_usb_bos_descriptor[] = {
    TUD_BOS_DESCRIPTOR(APP_USB_BOS_TOTAL_LEN, 1),
    TUD_BOS_MS_OS_20_DESCRIPTOR(APP_USB_MS_OS_20_DESC_LEN, APP_USB_VENDOR_REQUEST_MICROSOFT),
};

static const uint8_t s_usb_ms_os_20_descriptor[] = {
    /* Set header */
    U16_TO_U8S_LE(0x000A), U16_TO_U8S_LE(MS_OS_20_SET_HEADER_DESCRIPTOR), U32_TO_U8S_LE(0x06030000), U16_TO_U8S_LE(APP_USB_MS_OS_20_DESC_LEN),

    /* Configuration subset header */
    U16_TO_U8S_LE(0x0008), U16_TO_U8S_LE(MS_OS_20_SUBSET_HEADER_CONFIGURATION), 0x00, 0x00, U16_TO_U8S_LE(APP_USB_MS_OS_20_DESC_LEN - 0x0A),

    /* Function subset header */
    U16_TO_U8S_LE(0x0008), U16_TO_U8S_LE(MS_OS_20_SUBSET_HEADER_FUNCTION), APP_USB_VENDOR_ITF, 0x00, U16_TO_U8S_LE(APP_USB_MS_OS_20_DESC_LEN - 0x0A - 0x08),

    /* Compatible ID feature descriptor -> WINUSB */
    U16_TO_U8S_LE(0x0014), U16_TO_U8S_LE(MS_OS_20_FEATURE_COMPATBLE_ID), 'W', 'I', 'N', 'U', 'S', 'B', 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

    /* Registry property descriptor -> DeviceInterfaceGUIDs */
    U16_TO_U8S_LE(APP_USB_MS_OS_20_DESC_LEN - 0x0A - 0x08 - 0x08 - 0x14), U16_TO_U8S_LE(MS_OS_20_FEATURE_REG_PROPERTY),
    U16_TO_U8S_LE(0x0007), U16_TO_U8S_LE(0x002A),
    'D', 0x00, 'e', 0x00, 'v', 0x00, 'i', 0x00, 'c', 0x00, 'e', 0x00,
    'I', 0x00, 'n', 0x00, 't', 0x00, 'e', 0x00, 'r', 0x00, 'f', 0x00,
    'a', 0x00, 'c', 0x00, 'e', 0x00, 'G', 0x00, 'U', 0x00, 'I', 0x00,
    'D', 0x00, 's', 0x00, 0x00, 0x00,
    U16_TO_U8S_LE(0x0050),
    '{', 0x00, 'D', 0x00, '7', 0x00, '7', 0x00, '6', 0x00, '1', 0x00, 'D', 0x00, '5', 0x00,
    '0', 0x00, '-', 0x00, '5', 0x00, 'F', 0x00, '1', 0x00, 'B', 0x00, '-', 0x00, '4', 0x00,
    'D', 0x00, '3', 0x00, '3', 0x00, '-', 0x00, '9', 0x00, '5', 0x00, 'F', 0x00, '2', 0x00,
    '-', 0x00, '7', 0x00, '3', 0x00, '3', 0x00, 'B', 0x00, '0', 0x00, 'E', 0x00, '5', 0x00,
    'F', 0x00, '2', 0x00, 'E', 0x00, 'E', 0x00, 'D', 0x00, '}', 0x00, 0x00, 0x00, 0x00, 0x00,
};

TU_VERIFY_STATIC(sizeof(s_usb_ms_os_20_descriptor) == APP_USB_MS_OS_20_DESC_LEN, "Descriptor Microsoft OS 2.0 invalido");

static const app_can_bus_config_t *app_can_get_bus_config(app_can_bus_t bus)
{
    if (bus != APP_CAN_BUS_1 && bus != APP_CAN_BUS_2) {
        return NULL;
    }

    return &s_can_bus_cfg[bus];
}

static const char *app_can_get_active_bus_name(void)
{
    if (s_active_bus == APP_CAN_BUS_1 || s_active_bus == APP_CAN_BUS_2) {
        return s_can_bus_cfg[s_active_bus].name;
    }

    return "NONE";
}

static void app_usb_update_serial_string(void)
{
    uint8_t mac[6] = {0};

    ESP_ERROR_CHECK(esp_read_mac(mac, ESP_MAC_WIFI_STA));
    snprintf(s_usb_serial_number, sizeof(s_usb_serial_number),
             "%02X%02X%02X%02X%02X%02X",
             mac[0], mac[1], mac[2], mac[3], mac[4], mac[5]);
}

static esp_err_t app_can_release_active_bus(void)
{
    if (s_can_node == NULL) {
        s_active_bus = APP_CAN_BUS_NONE;
        return ESP_OK;
    }

    ESP_RETURN_ON_ERROR(twai_node_transmit_wait_all_done(s_can_node, APP_CAN_SWITCH_WAIT_MS), TAG, "tx pendiente al cambiar de bus");
    ESP_RETURN_ON_ERROR(twai_node_disable(s_can_node), TAG, "no se pudo deshabilitar el nodo TWAI");
    ESP_RETURN_ON_ERROR(twai_node_delete(s_can_node), TAG, "no se pudo borrar el nodo TWAI");

    s_can_node = NULL;
    s_active_bus = APP_CAN_BUS_NONE;
    return ESP_OK;
}

static esp_err_t app_can_select_bus(app_can_bus_t bus)
{
    const app_can_bus_config_t *cfg = app_can_get_bus_config(bus);
    ESP_RETURN_ON_FALSE(cfg != NULL, ESP_ERR_INVALID_ARG, TAG, "bus CAN invalido");

    if (s_can_node != NULL && s_active_bus == bus) {
        return ESP_OK;
    }

    ESP_RETURN_ON_ERROR(app_can_release_active_bus(), TAG, "no se pudo liberar el bus activo");

    twai_onchip_node_config_t node_config = {
        .io_cfg = {
            .tx = cfg->tx_gpio,
            .rx = cfg->rx_gpio,
            .quanta_clk_out = GPIO_NUM_NC,
            .bus_off_indicator = GPIO_NUM_NC,
        },
        .bit_timing = {
            .bitrate = APP_CAN_BITRATE,
        },
        .fail_retry_cnt = 1,
        .tx_queue_depth = APP_CAN_TX_QUEUE_DEPTH,
    };

    ESP_RETURN_ON_ERROR(twai_new_node_onchip(&node_config, &s_can_node), TAG, "no se pudo crear nodo para %s", cfg->name);
    ESP_RETURN_ON_ERROR(twai_node_enable(s_can_node), TAG, "no se pudo habilitar %s", cfg->name);

    s_active_bus = bus;
    ESP_LOGI(TAG, "bus activo: %s, TX=%d RX=%d, bitrate=%d", cfg->name, cfg->tx_gpio, cfg->rx_gpio, APP_CAN_BITRATE);
    return ESP_OK;
}

static esp_err_t app_can_send_standard(app_can_bus_t bus, uint32_t id, const uint8_t *data, size_t len)
{
    uint8_t tx_data[TWAI_FRAME_MAX_LEN] = {0};

    ESP_RETURN_ON_FALSE(data != NULL, ESP_ERR_INVALID_ARG, TAG, "data nula");
    ESP_RETURN_ON_FALSE(len <= TWAI_FRAME_MAX_LEN, ESP_ERR_INVALID_ARG, TAG, "len invalido");
    ESP_RETURN_ON_ERROR(app_can_select_bus(bus), TAG, "no se pudo seleccionar CAN%u", bus);

    memcpy(tx_data, data, len);

    twai_frame_t tx_frame = {
        .header = {
            .id = id & TWAI_STD_ID_MASK,
            .ide = 0,
            .rtr = 0,
            .fdf = 0,
            .brs = 0,
        },
        .buffer = tx_data,
        .buffer_len = len,
    };

    ESP_RETURN_ON_ERROR(twai_node_transmit(s_can_node, &tx_frame, APP_CAN_TX_TIMEOUT_MS), TAG, "fallo al encolar frame 0x%03" PRIX32, id);
    ESP_RETURN_ON_ERROR(twai_node_transmit_wait_all_done(s_can_node, APP_CAN_TX_TIMEOUT_MS), TAG, "frame 0x%03" PRIX32 " no termino", id);

    ESP_LOGI(TAG, "TX %s -> ID=0x%03" PRIX32 ", DLC=%u", s_can_bus_cfg[bus].name, tx_frame.header.id, (unsigned)len);
    return ESP_OK;
}

static esp_err_t app_reply_stdio(const char *line, void *ctx)
{
    (void)ctx;
    printf("%s\n", line);
    fflush(stdout);
    return ESP_OK;
}

static esp_err_t app_reply_usb_vendor(const char *line, void *ctx)
{
    (void)ctx;
    char buffer[APP_LINE_BUFFER_SIZE + 2];
    size_t len = (size_t)snprintf(buffer, sizeof(buffer), "%s\n", line);
    size_t offset = 0;
    uint32_t retries = 0;

    ESP_RETURN_ON_FALSE(s_usb_mounted, ESP_ERR_INVALID_STATE, TAG, "USB nativo no montado");
    ESP_RETURN_ON_FALSE(tud_mounted(), ESP_ERR_INVALID_STATE, TAG, "USB stack no montado (TinyUSB)");

    while (offset < len) {
        uint32_t available = tud_vendor_n_write_available(APP_USB_VENDOR_ITF);
        if (available == 0) {
            if (retries++ >= APP_USB_TX_RETRY_COUNT) {
                ESP_LOGW(TAG, "USB TX sin espacio (len=%u, sent=%u)", (unsigned)len, (unsigned)offset);
                return ESP_ERR_TIMEOUT;
            }
            vTaskDelay(pdMS_TO_TICKS(1));
            continue;
        }

        uint32_t remaining = (uint32_t)(len - offset);
        uint32_t chunk = (available < remaining) ? available : remaining;
        uint32_t written = tud_vendor_n_write(APP_USB_VENDOR_ITF, &buffer[offset], chunk);
        if (written == 0) {
            if (retries++ >= APP_USB_TX_RETRY_COUNT) {
                ESP_LOGW(TAG, "USB TX write=0 (len=%u, sent=%u)", (unsigned)len, (unsigned)offset);
                return ESP_FAIL;
            }
            vTaskDelay(pdMS_TO_TICKS(1));
            continue;
        }

        offset += written;
        retries = 0;
    }

    tud_vendor_n_write_flush(APP_USB_VENDOR_ITF);
    s_usb_tx_packets++;
    if ((s_usb_tx_packets % 16U) == 0U) {
        ESP_LOGI(TAG, "USB TX acumulado=%lu", (unsigned long)s_usb_tx_packets);
    }
    return ESP_OK;
}

static esp_err_t app_reply_socket(const char *line, void *ctx)
{
    int sock = *((int *)ctx);
    char buffer[APP_LINE_BUFFER_SIZE + 2];
    size_t len = (size_t)snprintf(buffer, sizeof(buffer), "%s\n", line);

    if (send(sock, buffer, len, 0) < 0) {
        return ESP_FAIL;
    }

    return ESP_OK;
}

static esp_err_t app_can_select_bus_adapter(int bus)
{
    switch (bus) {
    case APP_CMD_CAN_BUS_1:
        return app_can_select_bus(APP_CAN_BUS_1);
    case APP_CMD_CAN_BUS_2:
        return app_can_select_bus(APP_CAN_BUS_2);
    default:
        return ESP_ERR_INVALID_ARG;
    }
}

static esp_err_t app_can_send_standard_adapter(int bus, uint32_t id, const uint8_t *data, size_t len)
{
    app_can_bus_t mapped_bus = APP_CAN_BUS_NONE;

    switch (bus) {
    case APP_CMD_CAN_BUS_1:
        mapped_bus = APP_CAN_BUS_1;
        break;
    case APP_CMD_CAN_BUS_2:
        mapped_bus = APP_CAN_BUS_2;
        break;
    default:
        return ESP_ERR_INVALID_ARG;
    }

    return app_can_send_standard(mapped_bus, id, data, len);
}

static esp_err_t app_process_command_line(const char *incoming_line, app_reply_fn_t reply, void *ctx)
{
    bool wifi_connected = (xEventGroupGetBits(s_wifi_event_group) & WIFI_CONNECTED_BIT) != 0;
    int active_bus = APP_CMD_CAN_BUS_NONE;

    if (s_active_bus == APP_CAN_BUS_1) {
        active_bus = APP_CMD_CAN_BUS_1;
    } else if (s_active_bus == APP_CAN_BUS_2) {
        active_bus = APP_CMD_CAN_BUS_2;
    }

    app_command_env_t env = {
        .usb_mounted = s_usb_mounted,
        .wifi_connected = wifi_connected,
        .wifi_ip = s_wifi_ip_addr,
        .tcp_port = APP_TCP_PORT,
        .wifi_ssid = APP_WIFI_SSID,
        .active_bus = active_bus,
        .active_bus_name = app_can_get_active_bus_name(),
        .can_select_bus = app_can_select_bus_adapter,
        .can_send_standard = app_can_send_standard_adapter,
        .can_max_frame_len = TWAI_FRAME_MAX_LEN,
        .can_std_id_mask = TWAI_STD_ID_MASK,
    };

    return app_command_process_line(incoming_line, reply, ctx, &env);
}

static void app_set_static_ip(esp_netif_t *netif)
{
    esp_netif_ip_info_t ip_info = {0};

    if (netif == NULL) {
        return;
    }

    ESP_ERROR_CHECK(esp_netif_dhcpc_stop(netif));
    ip_info.ip.addr = ipaddr_addr(APP_WIFI_STATIC_IP);
    ip_info.netmask.addr = ipaddr_addr(APP_WIFI_STATIC_NETMASK);
    ip_info.gw.addr = ipaddr_addr(APP_WIFI_STATIC_GW);
    ESP_ERROR_CHECK(esp_netif_set_ip_info(netif, &ip_info));
}

static void app_wifi_event_handler(void *arg, esp_event_base_t event_base, int32_t event_id, void *event_data)
{
    esp_netif_t *netif = (esp_netif_t *)arg;

    if (event_base == WIFI_EVENT && event_id == WIFI_EVENT_STA_START) {
        esp_wifi_connect();
        return;
    }

    if (event_base == WIFI_EVENT && event_id == WIFI_EVENT_STA_CONNECTED) {
        app_set_static_ip(netif);
        return;
    }

    if (event_base == WIFI_EVENT && event_id == WIFI_EVENT_STA_DISCONNECTED) {
        strlcpy(s_wifi_ip_addr, "0.0.0.0", sizeof(s_wifi_ip_addr));
        xEventGroupClearBits(s_wifi_event_group, WIFI_CONNECTED_BIT);
        ESP_LOGW(TAG, "wifi desconectado, reintentando...");
        esp_wifi_connect();
        return;
    }

    if (event_base == IP_EVENT && event_id == IP_EVENT_STA_GOT_IP) {
        ip_event_got_ip_t *event = (ip_event_got_ip_t *)event_data;
        snprintf(s_wifi_ip_addr, sizeof(s_wifi_ip_addr), IPSTR, IP2STR(&event->ip_info.ip));
        ESP_LOGI(TAG, "wifi listo, ip=%s", s_wifi_ip_addr);
        xEventGroupSetBits(s_wifi_event_group, WIFI_CONNECTED_BIT);
    }
}

static void app_wifi_init(void)
{
    esp_event_handler_instance_t wifi_any_id;
    esp_event_handler_instance_t wifi_got_ip;

    s_wifi_event_group = xEventGroupCreate();

    ESP_ERROR_CHECK(esp_netif_init());
    ESP_ERROR_CHECK(esp_event_loop_create_default());

    s_wifi_sta_netif = esp_netif_create_default_wifi_sta();
    assert(s_wifi_sta_netif != NULL);

    wifi_init_config_t cfg = WIFI_INIT_CONFIG_DEFAULT();
    ESP_ERROR_CHECK(esp_wifi_init(&cfg));

    ESP_ERROR_CHECK(esp_event_handler_instance_register(WIFI_EVENT,
                                                        ESP_EVENT_ANY_ID,
                                                        &app_wifi_event_handler,
                                                        s_wifi_sta_netif,
                                                        &wifi_any_id));
    ESP_ERROR_CHECK(esp_event_handler_instance_register(IP_EVENT,
                                                        IP_EVENT_STA_GOT_IP,
                                                        &app_wifi_event_handler,
                                                        s_wifi_sta_netif,
                                                        &wifi_got_ip));

    wifi_config_t wifi_config = {
        .sta = {
            .threshold.authmode = WIFI_AUTH_WPA2_PSK,
        },
    };

    strlcpy((char *)wifi_config.sta.ssid, APP_WIFI_SSID, sizeof(wifi_config.sta.ssid));
    strlcpy((char *)wifi_config.sta.password, APP_WIFI_PASS, sizeof(wifi_config.sta.password));

    ESP_ERROR_CHECK(esp_wifi_set_mode(WIFI_MODE_STA));
    ESP_ERROR_CHECK(esp_wifi_set_config(WIFI_IF_STA, &wifi_config));
    ESP_ERROR_CHECK(esp_wifi_start());

    ESP_LOGI(TAG, "wifi sta iniciado ssid=%s ip=%s port=%d", APP_WIFI_SSID, APP_WIFI_STATIC_IP, APP_TCP_PORT);
}

static void app_configure_stdio(void)
{
    setvbuf(stdin, NULL, _IONBF, 0);
    setvbuf(stdout, NULL, _IONBF, 0);

#if CONFIG_ESP_CONSOLE_UART
    if (!uart_is_driver_installed((uart_port_t)CONFIG_ESP_CONSOLE_UART_NUM)) {
        ESP_ERROR_CHECK(uart_driver_install((uart_port_t)CONFIG_ESP_CONSOLE_UART_NUM, 256, 0, 0, NULL, 0));
    }
    uart_vfs_dev_use_driver(CONFIG_ESP_CONSOLE_UART_NUM);
    uart_vfs_dev_port_set_rx_line_endings(CONFIG_ESP_CONSOLE_UART_NUM, ESP_LINE_ENDINGS_CR);
    uart_vfs_dev_port_set_tx_line_endings(CONFIG_ESP_CONSOLE_UART_NUM, ESP_LINE_ENDINGS_CRLF);
#endif
}

static void app_usb_event_handler(tinyusb_event_t *event, void *arg)
{
    (void)arg;

    switch (event->id) {
    case TINYUSB_EVENT_ATTACHED:
        s_usb_mounted = true;
        ESP_LOGI(TAG, "USB vendor montado");
        break;

    case TINYUSB_EVENT_DETACHED:
        s_usb_mounted = false;
        ESP_LOGI(TAG, "USB vendor desmontado");
        break;

#ifdef CONFIG_TINYUSB_SUSPEND_CALLBACK
    case TINYUSB_EVENT_SUSPENDED:
        ESP_LOGI(TAG, "USB vendor suspendido");
        break;
#endif

#ifdef CONFIG_TINYUSB_RESUME_CALLBACK
    case TINYUSB_EVENT_RESUMED:
        ESP_LOGI(TAG, "USB vendor reanudado");
        break;
#endif

    default:
        break;
    }
}

static void app_usb_init(void)
{
    tinyusb_config_t tusb_cfg = TINYUSB_DEFAULT_CONFIG(app_usb_event_handler);

    tusb_cfg.descriptor.device = &s_usb_device_descriptor;
    tusb_cfg.descriptor.full_speed_config = s_usb_configuration_descriptor;
    tusb_cfg.descriptor.string = s_usb_string_descriptor;
    tusb_cfg.descriptor.string_count = sizeof(s_usb_string_descriptor) / sizeof(s_usb_string_descriptor[0]);
#if (TUD_OPT_HIGH_SPEED)
    tusb_cfg.descriptor.high_speed_config = s_usb_configuration_descriptor;
#endif

    ESP_ERROR_CHECK(tinyusb_driver_install(&tusb_cfg));
    ESP_LOGI(TAG, "USB vendor-specific listo VID=0x%04X PID=0x%04X GUID=%s",
             APP_USB_VENDOR_ID, APP_USB_PRODUCT_ID, APP_USB_INTERFACE_GUID);
}

static void app_handle_line_locked(const char *line, app_reply_fn_t reply, void *ctx)
{
    esp_err_t err;

    if (line == NULL || line[0] == '\0') {
        return;
    }

    if (xSemaphoreTake(s_command_mutex, portMAX_DELAY) != pdTRUE) {
        return;
    }

    err = app_process_command_line(line, reply, ctx);
    if (err != ESP_OK) {
        ESP_LOGW(TAG, "fallo procesando linea '%s': %d", line, (int)err);
    }

    xSemaphoreGive(s_command_mutex);
}

static void app_serial_task(void *pvParameters)
{
    char line[APP_LINE_BUFFER_SIZE];

    (void)pvParameters;

    while (1) {
        if (fgets(line, sizeof(line), stdin) == NULL) {
            vTaskDelay(pdMS_TO_TICKS(20));
            continue;
        }

        app_trim_line(line);
        if (line[0] == '\0') {
            continue;
        }

        app_handle_line_locked(line, app_reply_stdio, NULL);
    }
}

static void app_usb_submit_line(const char *line)
{
    app_usb_command_t cmd = {0};

    if (line == NULL || line[0] == '\0') {
        return;
    }

    strlcpy(cmd.line, line, sizeof(cmd.line));
    if (xQueueSend(s_usb_rx_queue, &cmd, 0) != pdTRUE) {
        ESP_LOGW(TAG, "cola USB llena, se descarta: %s", line);
    }
}

static void app_usb_process_rx_bytes(const uint8_t *data, size_t len)
{
    for (size_t i = 0; i < len; i++) {
        char c = (char)data[i];

        if (c == '\r') {
            continue;
        }

        if (c == '\n') {
            s_usb_rx_line[s_usb_rx_line_len] = '\0';
            app_trim_line(s_usb_rx_line);
            if (s_usb_rx_line[0] != '\0') {
                app_usb_submit_line(s_usb_rx_line);
            }
            s_usb_rx_line_len = 0;
            s_usb_rx_line[0] = '\0';
            continue;
        }

        if (s_usb_rx_line_len < (sizeof(s_usb_rx_line) - 1)) {
            s_usb_rx_line[s_usb_rx_line_len++] = c;
        }
    }
}

static void app_usb_vendor_task(void *pvParameters)
{
    app_usb_command_t cmd;

    (void)pvParameters;

    while (1) {
        if (xQueueReceive(s_usb_rx_queue, &cmd, portMAX_DELAY) == pdTRUE) {
            app_handle_line_locked(cmd.line, app_reply_usb_vendor, NULL);
        }
    }
}

static void app_handle_tcp_client(int sock)
{
    char rx_buffer[APP_TCP_RX_BUFFER_SIZE];
    char line_buffer[APP_LINE_BUFFER_SIZE] = {0};
    size_t line_len = 0;

    while (1) {
        int len = recv(sock, rx_buffer, sizeof(rx_buffer), 0);
        if (len < 0) {
            ESP_LOGE(TAG, "tcp recv error errno=%d", errno);
            break;
        }

        if (len == 0) {
            ESP_LOGI(TAG, "tcp cliente cerro la conexion");
            break;
        }

        for (int i = 0; i < len; i++) {
            char c = rx_buffer[i];

            if (c == '\r') {
                continue;
            }

            if (c == '\n') {
                line_buffer[line_len] = '\0';
                app_trim_line(line_buffer);
                if (line_buffer[0] != '\0') {
                    app_handle_line_locked(line_buffer, app_reply_socket, &sock);
                }
                line_len = 0;
                line_buffer[0] = '\0';
                continue;
            }

            if (line_len < (sizeof(line_buffer) - 1)) {
                line_buffer[line_len++] = c;
            }
        }
    }
}

static void app_tcp_server_task(void *pvParameters)
{
    (void)pvParameters;

    while (1) {
        xEventGroupWaitBits(s_wifi_event_group, WIFI_CONNECTED_BIT, pdFALSE, pdFALSE, portMAX_DELAY);

        struct sockaddr_in dest_addr = {
            .sin_family = AF_INET,
            .sin_port = htons(APP_TCP_PORT),
            .sin_addr.s_addr = htonl(INADDR_ANY),
        };

        int listen_sock = socket(AF_INET, SOCK_STREAM, IPPROTO_IP);
        if (listen_sock < 0) {
            ESP_LOGE(TAG, "no se pudo crear socket tcp errno=%d", errno);
            vTaskDelay(pdMS_TO_TICKS(1000));
            continue;
        }

        int opt = 1;
        setsockopt(listen_sock, SOL_SOCKET, SO_REUSEADDR, &opt, sizeof(opt));

        if (bind(listen_sock, (struct sockaddr *)&dest_addr, sizeof(dest_addr)) != 0) {
            ESP_LOGE(TAG, "bind fallo errno=%d", errno);
            close(listen_sock);
            vTaskDelay(pdMS_TO_TICKS(1000));
            continue;
        }

        if (listen(listen_sock, 1) != 0) {
            ESP_LOGE(TAG, "listen fallo errno=%d", errno);
            close(listen_sock);
            vTaskDelay(pdMS_TO_TICKS(1000));
            continue;
        }

        ESP_LOGI(TAG, "tcp server escuchando en %s:%d", s_wifi_ip_addr, APP_TCP_PORT);

        while ((xEventGroupGetBits(s_wifi_event_group) & WIFI_CONNECTED_BIT) != 0) {
            struct sockaddr_in source_addr;
            socklen_t addr_len = sizeof(source_addr);
            int sock = accept(listen_sock, (struct sockaddr *)&source_addr, &addr_len);

            if (sock < 0) {
                ESP_LOGE(TAG, "accept fallo errno=%d", errno);
                break;
            }

            char addr_str[16];
            inet_ntoa_r(source_addr.sin_addr, addr_str, sizeof(addr_str) - 1);
            ESP_LOGI(TAG, "tcp cliente conectado: %s", addr_str);

            app_reply_socket("CONNECTED Acuratex ESP32-S3", &sock);
            app_handle_tcp_client(sock);

            shutdown(sock, 0);
            close(sock);
        }

        close(listen_sock);
        vTaskDelay(pdMS_TO_TICKS(250));
    }
}

extern "C" uint8_t const *tud_descriptor_bos_cb(void)
{
    return s_usb_bos_descriptor;
}

extern "C" bool tud_vendor_control_xfer_cb(uint8_t rhport, uint8_t stage, tusb_control_request_t const *request)
{
    if (stage != CONTROL_STAGE_SETUP) {
        return true;
    }

    if (request->bmRequestType_bit.type == TUSB_REQ_TYPE_VENDOR &&
        request->bRequest == APP_USB_VENDOR_REQUEST_MICROSOFT &&
        request->wIndex == 7) {
        return tud_control_xfer(rhport, request, (void *)(uintptr_t)s_usb_ms_os_20_descriptor, sizeof(s_usb_ms_os_20_descriptor));
    }

    return false;
}
extern "C" void tud_vendor_rx_cb(uint8_t itf, uint8_t const *buffer, uint16_t bufsize)
{
    uint8_t tmp[APP_USB_BULK_EP_SIZE];
    uint32_t pending = tud_vendor_n_available(itf);

    if (buffer != NULL && bufsize > 0) {
        app_usb_process_rx_bytes(buffer, bufsize);
        s_usb_rx_packets++;
        if ((s_usb_rx_packets % 16U) == 0U) {
            ESP_LOGI(TAG, "USB RX acumulado=%lu (pending=%lu)",
                     (unsigned long)s_usb_rx_packets, (unsigned long)pending);
        }
    }

    while (pending > 0) {
        uint32_t to_read = (pending > sizeof(tmp)) ? sizeof(tmp) : pending;
        uint32_t read = tud_vendor_n_read(itf, tmp, to_read);
        if (read == 0) {
            break;
        }

        app_usb_process_rx_bytes(tmp, read);
        s_usb_rx_packets++;
        pending = tud_vendor_n_available(itf);
    }

#if CFG_TUD_VENDOR_RX_BUFSIZE > 0
    // Libera el buffer RX interno de TinyUSB para aceptar el siguiente paquete OUT.
    tud_vendor_n_read_flush(itf);
#endif
}

extern "C" void app_main(void)
{
    esp_err_t ret = nvs_flash_init();
    if (ret == ESP_ERR_NVS_NO_FREE_PAGES || ret == ESP_ERR_NVS_NEW_VERSION_FOUND) {
        ESP_ERROR_CHECK(nvs_flash_erase());
        ret = nvs_flash_init();
    }
    ESP_ERROR_CHECK(ret);

    s_command_mutex = xSemaphoreCreateMutex();
    assert(s_command_mutex != NULL);

    s_usb_rx_queue = xQueueCreate(APP_USB_RX_QUEUE_DEPTH, sizeof(app_usb_command_t));
    assert(s_usb_rx_queue != NULL);

    app_configure_stdio();
    app_usb_update_serial_string();
    app_usb_init();
    ESP_ERROR_CHECK(app_can_select_bus(APP_CAN_BUS_1));
    app_wifi_init();

    printf("ACURATEX FW READY\n");
    printf("USB APP: vendor-specific WinUSB GUID=%s\n", APP_USB_INTERFACE_GUID);
    printf("UART SERVICIO: lineas tipo 'ping', 'status', 'can1', 'can2', 'send 320 07'\n");
    printf("WIFI: SSID=%s IP=%s PORT=%d\n", APP_WIFI_SSID, APP_WIFI_STATIC_IP, APP_TCP_PORT);

    xTaskCreate(app_serial_task, "app_serial_task", 4096, NULL, 5, NULL);
    xTaskCreate(app_usb_vendor_task, "app_usb_vendor_task", 4096, NULL, 5, NULL);
    xTaskCreate(app_tcp_server_task, "app_tcp_server_task", 6144, NULL, 5, NULL);
}
