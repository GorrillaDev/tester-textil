#pragma once

#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>

void app_trim_line(char *line);
bool app_parse_frame_line(const char *line,
                          uint32_t *id,
                          uint8_t *data,
                          size_t *len,
                          size_t max_data_len,
                          uint32_t max_id);
