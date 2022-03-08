/********************************************************************************
 * This file is part of the RCP SDK Release 6.62.0
 * Copyright (C) 2009-2018 RED.COM, LLC.  All rights reserved.
 *
 * For technical support please email rcpsdk@red.com.
 *
 * "Source Code" means the accompanying software in any form preferred for making
 * modifications. "Source Code" does not include the accompanying strlcat.c and
 * strlcpy.c software and examples/qt/common/qt/serial software.
 * 
 * "Binary Code" means machine-readable Source Code in binary form.
 * 
 * "Approved Recipients" means only those recipients of the Source Code who have
 * entered into the RCP SDK License Agreement with RED.COM, LLC. All
 * other recipients are not authorized to possess, modify, use, or distribute the
 * Source Code.
 *
 * RED.COM, LLC hereby grants Approved Recipients the rights to modify this
 * Source Code, create derivative works based on this Source Code, and distribute
 * the modified/derivative works only as Binary Code in its binary form. Approved
 * Recipients may not distribute the Source Code or any modification or derivative
 * work of the Source Code. Redistributions of Binary Code must reproduce this
 * copyright notice, this list of conditions, and the following disclaimer in the
 * documentation or other materials provided with the distribution. RED.COM, LLC
 * may not be used to endorse or promote Binary Code redistributions without
 * specific prior written consent from RED.COM, LLC. 
 *
 * The only exception to the above licensing requirements is any recipient may use,
 * copy, modify, and distribute in any format the strlcat.c and strlcpy.c software
 * in accordance with the provisions required by the license associated therewith;
 * provided, however, that the modifications are solely to the strlcat.c and
 * strlcpy.c software and do not include any other portion of the Source Code.
 * 
 * THE ACCOMPANYING SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, TITLE, AND NON-INFRINGEMENT.
 * IN NO EVENT SHALL THE RED.COM, LLC, ANY OTHER COPYRIGHT HOLDERS, OR ANYONE
 * DISTRIBUTING THE SOFTWARE BE LIABLE FOR ANY DAMAGES OR OTHER LIABILITY, WHETHER
 * IN CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 ********************************************************************************/

#ifndef DECORATED_STRING_H
#define DECORATED_STRING_H

#include "types/rcp_types_public.h"
#include <stdlib.h>

#ifdef __cplusplus
extern "C"
{
#endif

typedef enum
{
    DECORATED_STRING_LEN_NORMAL,
    DECORATED_STRING_LEN_ABBREVIATED,
    DECORATED_STRING_LEN_COUNT
} decorated_string_len_t;

typedef enum
{
    DECORATED_STRING_COLOR_OK,
    DECORATED_STRING_COLOR_WARNING,
    DECORATED_STRING_COLOR_ERROR,
    DECORATED_STRING_COLOR_DISABLED
} decorated_string_color_t;

void decorated_string_decode(const char * src, char * dest, size_t dest_len);
void decorated_string_decode1(const char * src, char * dest, size_t dest_len);
void decorated_string_decode2(const char * src, char * dest, size_t dest_len, int32_t builtin);
void decorated_string_1_to_2(const char * src, char * dest, size_t dest_len);

void decorated_string_create(char * dest, size_t dest_size, int32_t num, int32_t divider, int32_t digits, const char * prefix, const char * postfix);

void decorated_string_abbreviate(char * str);

void decorated_string_create_fps_label(char * dest, size_t dest_size, decorated_string_len_t len, int32_t fps);
void decorated_string_create_int_time_label(char * dest, size_t dest_size, decorated_string_len_t len, int32_t show_angle, int32_t int_time, int32_t int_time_angle, frame_processing_t frame_processing, int32_t frame_divider, int32_t ae_control);
void decorated_string_create_red_gamma(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value);
void decorated_string_create_red_color(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value);
void decorated_string_create_main_output_preset(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value);
void decorated_string_create_display_preset(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value);
void decorated_string_create_output_transform(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value);
void decorated_string_create_iso(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value, int32_t iso_pull, int32_t nd_val, int32_t sensor_gain_supported, int32_t sensor_gain_mode);
void decorated_string_create_redcode(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value);
void decorated_string_create_file_format(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value);
void decorated_string_create_aperture(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value, int32_t ae_control);
void decorated_string_create_power(char * dest, size_t dest_size, decorated_string_len_t len, battery_display_mode_t display_mode, int32_t power_level, int32_t runtime);
void decorated_string_create_power2(char * dest, size_t dest_size, decorated_string_len_t len, battery_display_mode_t display_mode, int32_t voltage, int32_t runtime, int32_t percent);
void decorated_string_create_power_voltage(char * dest, size_t dest_size, decorated_string_len_t len, int32_t voltage);
void decorated_string_create_power_percent(char * dest, size_t dest_size, decorated_string_len_t len, int32_t percent);
void decorated_string_create_power_runtime(char * dest, size_t dest_size, decorated_string_len_t len, int32_t runtime);
void decorated_string_create_focus_dist(char * dest, size_t dest_size, decorated_string_len_t len, focus_distance_mode_t display_mode, int32_t near_dist, int32_t far_dist);
void decorated_string_create_focal_length(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value);
void decorated_string_create_media(char * dest, size_t dest_size, decorated_string_len_t len, decorated_string_color_t * color, record_mode_t record_mode, media_display_t display_mode, int32_t media_level, int32_t runtime);
void decorated_string_create_color_temperature(char * dest, size_t dest_size, decorated_string_len_t len, int32_t color_temp);
void decorated_string_create_temperature(char * dest, size_t dest_size, decorated_string_len_t len, int32_t temp);
void decorated_string_create_fan_speed(char * dest, size_t dest_size, decorated_string_len_t len, int32_t fan_speed);
void decorated_string_create_gyro_angle(char * dest, size_t dest_size, decorated_string_len_t len, int32_t angle);

#ifdef __cplusplus
}
#endif

#endif
