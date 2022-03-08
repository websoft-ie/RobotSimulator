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

#ifndef KEYDEFINITION_H_INCLUDED
#define KEYDEFINITION_H_INCLUDED

/* Unique Key-Codes are defined as 24-bit numbers
 * Lower 8 Bit = Code
 * Middle 8 Bit = Flags
 * Upper 8 Bit = Source
 * */

/* Event sources */
#define BRAIN                       0
#define SIDE_HANDLE                 1
#define BOTTOM_HANDLE               2
#define SIDE_SSD                    3
#define REDMOTE                     4
#define LCD                         5
#define LENS                        6
#define LCD_PROMODULE               7
#define EVF                         8
#define EVF_PROMODULE               9
#define PRO_IO                      10
#define UI_SOFTKEY                  11
#define SIDE_MODULE                 12
#define FLAT_UI                     13
#define TOP_HANDLE                  14
#define WEAPON_SIDE_HANDLE          15
#define LCD3                        16
#define EVF3                        17
#define XLFIZ                       18  /* No longer supported... */
#define SIDE_UI_LEFT                19
#define SIDE_UI_RIGHT               20

/* Key flags */
#define KEY_DOWN                    1   /**< Key press down event (should always be followed eventually by a @ref KEY_UP event. */
#define KEY_UP                      2   /**< Key released */
#define CW                          4   /**< Clockwise */
#define CCW                         8   /**< Counter Clockwise */

/* Key ID's */
#define USER_KEY_A                  1
#define USER_KEY_B                  2
#define USER_KEY_C                  3
#define USER_KEY_D                  4
#define FUNCTION_KEY_1              5
#define FUNCTION_KEY_2              6
#define FUNCTION_KEY_3              7
#define FUNCTION_KEY_4              8
#define FUNCTION_KEY_5              9
#define FUNCTION_KEY_6              10
#define FUNCTION_KEY_7              11
#define FUNCTION_KEY_8              12
#define STILL_SELECT_KEY            13
#define MOVIE_SELECT_KEY            14
#define RECORD_HALF_KEY             15
#define RECORD_FULL_KEY             16
#define FRONT_ROTARY_1              26
#define FRONT_ROTARY_2              27
#define BACK_ROTARY_JOG             28
#define BACKLIGHT_KEY               29
#define MENU_KEY                    30
#define NORTH_KEY                   31
#define SOUTH_KEY                   32
#define EAST_KEY                    33
#define WEST_KEY                    34
#define CENTER_KEY                  35
#define LOCK_KEY                    40
#define UNLOCK_KEY                  41
#define ROCKER_PLUS                 42
#define ROCKER_MINUS                43
#define MODE_KEY                    44

/* Utility Macros */

/** Create keycode from @p source, @p flags, and @p id. */
#define MAKEKEYCODE(source, flags, id) ((((source) & 0xFF) << 16) | (((flags) & 0xFF) << 8) | ((id) & 0xFF))

/** Extract source from @p keycode. */
#define KEYCODE_SOURCE(keycode) (((keycode) & 0xFF0000) >> 16)

/** Extract flags from @p keycode. */
#define KEYCODE_FLAGS(keycode) (((keycode) & 0x00FF00) >> 8)

/** Extract id from @p keycode. */
#define KEYCODE_ID(keycode) ((keycode) & 0x0000FF)
#endif
