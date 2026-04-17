#include <ctype.h>
#include <stdlib.h>
#include <string.h>
#include "line_codec.h"

void app_trim_line(char *line)
{
    size_t len;

    if (line == NULL) {
        return;
    }

    len = strlen(line);
    while (len > 0 && isspace((unsigned char)line[len - 1])) {
        line[--len] = '\0';
    }

    if (len == 0) {
        return;
    }

    char *start = line;
    while (*start && isspace((unsigned char)*start)) {
        start++;
    }

    if (start != line) {
        memmove(line, start, strlen(start) + 1);
    }
}

static bool app_parse_hex_byte(const char *token, uint8_t *out)
{
    char *endptr = NULL;
    long value;

    if (token == NULL || out == NULL) {
        return false;
    }

    value = strtol(token, &endptr, 16);
    if (endptr == token || *endptr != '\0' || value < 0 || value > 0xFF) {
        return false;
    }

    *out = (uint8_t)value;
    return true;
}

static bool app_parse_hex_id(const char *token, uint32_t *out, uint32_t max_id)
{
    char *endptr = NULL;
    unsigned long value;

    if (token == NULL || out == NULL) {
        return false;
    }

    value = strtoul(token, &endptr, 16);
    if (endptr == token || *endptr != '\0' || value > max_id) {
        return false;
    }

    *out = (uint32_t)value;
    return true;
}

bool app_parse_frame_line(const char *line,
                          uint32_t *id,
                          uint8_t *data,
                          size_t *len,
                          size_t max_data_len,
                          uint32_t max_id)
{
    char buffer[160];
    char *saveptr = NULL;
    char *token;
    size_t count = 0;

    if (line == NULL || id == NULL || data == NULL || len == NULL) {
        return false;
    }

    strlcpy(buffer, line, sizeof(buffer));
    token = strtok_r(buffer, " ", &saveptr);
    if (token == NULL || !app_parse_hex_id(token, id, max_id)) {
        return false;
    }

    while ((token = strtok_r(NULL, " ", &saveptr)) != NULL) {
        if (count >= max_data_len || !app_parse_hex_byte(token, &data[count])) {
            return false;
        }
        count++;
    }

    *len = count;
    return true;
}
