#pragma once

#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>
#include "esp_err.h"

typedef esp_err_t (*app_reply_fn_t)(const char *line, void *ctx);
typedef esp_err_t (*app_can_select_bus_fn_t)(int bus);
typedef esp_err_t (*app_can_send_standard_fn_t)(int bus, uint32_t id, const uint8_t *data, size_t len);

enum {
    APP_CMD_CAN_BUS_NONE = 0,
    APP_CMD_CAN_BUS_1 = 1,
    APP_CMD_CAN_BUS_2 = 2,
};

typedef struct {
    bool usb_mounted;
    bool wifi_connected;
    const char *wifi_ip;
    int tcp_port;
    const char *wifi_ssid;
    int active_bus;
    const char *active_bus_name;
    app_can_select_bus_fn_t can_select_bus;
    app_can_send_standard_fn_t can_send_standard;
    size_t can_max_frame_len;
    uint32_t can_std_id_mask;
} app_command_env_t;

esp_err_t app_command_process_line(const char *incoming_line,
                                   app_reply_fn_t reply,
                                   void *ctx,
                                   const app_command_env_t *env);
