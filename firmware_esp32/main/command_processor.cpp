#include <inttypes.h>
#include <stdio.h>
#include <string.h>
#include <strings.h>
#include "command_processor.h"
#include "line_codec.h"

static esp_err_t app_send_status(app_reply_fn_t reply, void *ctx, const app_command_env_t *env)
{
    char line[160];

    snprintf(line, sizeof(line),
             "STATUS usb=%s wifi=%s ip=%s tcp_port=%d can=%s ssid=%s",
             env->usb_mounted ? "mounted" : "detached",
             env->wifi_connected ? "connected" : "disconnected",
             env->wifi_ip,
             env->tcp_port,
             env->active_bus_name,
             env->wifi_ssid);

    return reply(line, ctx);
}

static esp_err_t app_process_frame_command(const char *line, app_reply_fn_t reply, void *ctx, const app_command_env_t *env)
{
    uint32_t id = 0;
    uint8_t data[64] = {0};
    size_t len = 0;
    char response[160];
    esp_err_t err;

    if (!app_parse_frame_line(line, &id, data, &len, env->can_max_frame_len, env->can_std_id_mask)) {
        return reply("ERR frame invalido", ctx);
    }

    err = env->can_send_standard((env->active_bus == APP_CMD_CAN_BUS_NONE) ? APP_CMD_CAN_BUS_1 : env->active_bus, id, data, len);
    if (err != ESP_OK) {
        snprintf(response, sizeof(response), "ERR can_send %d", (int)err);
        return reply(response, ctx);
    }

    snprintf(response, sizeof(response), "TX_OK bus=%s id=0x%03" PRIX32 " dlc=%u",
             env->active_bus_name, id, (unsigned)len);
    return reply(response, ctx);
}

esp_err_t app_command_process_line(const char *incoming_line,
                                   app_reply_fn_t reply,
                                   void *ctx,
                                   const app_command_env_t *env)
{
    char line[160];
    esp_err_t err;

    if (incoming_line == NULL || reply == NULL || env == NULL) {
        return ESP_ERR_INVALID_ARG;
    }

    strlcpy(line, incoming_line, sizeof(line));
    app_trim_line(line);

    if (line[0] == '\0') {
        return ESP_OK;
    }

    if (strcasecmp(line, "ping") == 0 || strcasecmp(line, "hello") == 0) {
        return reply("PONG", ctx);
    }

    if (strcasecmp(line, "help") == 0) {
        return reply("OK cmds: ping,status,can1,can2,send <hex>,<hex line>,start,stop,testeo", ctx);
    }

    if (strcasecmp(line, "status") == 0) {
        return app_send_status(reply, ctx, env);
    }

    if (strcasecmp(line, "can1") == 0) {
        err = env->can_select_bus(APP_CMD_CAN_BUS_1);
        if (err != ESP_OK) {
            return reply("ERR no se pudo activar CAN1", ctx);
        }
        return reply("OK CAN1", ctx);
    }

    if (strcasecmp(line, "can2") == 0) {
        err = env->can_select_bus(APP_CMD_CAN_BUS_2);
        if (err != ESP_OK) {
            return reply("ERR no se pudo activar CAN2", ctx);
        }
        return reply("OK CAN2", ctx);
    }

    if (strcasecmp(line, "start") == 0) {
        return reply("ACK start", ctx);
    }

    if (strcasecmp(line, "stop") == 0) {
        return reply("ACK stop", ctx);
    }

    if (strcasecmp(line, "testeo") == 0) {
        return reply("ACK testeo", ctx);
    }

    if (strncasecmp(line, "send ", 5) == 0) {
        return app_process_frame_command(line + 5, reply, ctx, env);
    }

    return app_process_frame_command(line, reply, ctx, env);
}
