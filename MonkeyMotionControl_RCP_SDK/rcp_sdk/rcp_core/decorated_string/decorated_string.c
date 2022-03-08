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

#include "decorated_string.h"
#include "stringl/stringl.h"
#include <stdio.h>
#include <math.h>
#include <string.h>
#include <ctype.h>
#include "types/rcp_types_public.h"

#if defined(_MSC_VER) || defined(_CVI_)
#if defined(_MSC_VER) && (_MSC_VER < 1900)
#define snprintf _snprintf
#endif
#define roundf(x) (float) floor((x) + 0.5)
#ifdef _CVI_
#define powf(value, power) (float) pow(value, power)
#endif
#endif

#define FPS_DIVIDER 1001
#define INT_TIME_DIVIDER 1000

static void num_to_str(char * dest, size_t dest_size, int32_t num, int32_t divider, int32_t digits);
static void concatenate(char * dest, size_t dest_size, const char * prefix, const char * body, const char * postfix);
static void format_imperial_dist(char * str, size_t size, int32_t dist);
static void format_metric_dist(char * str, size_t size, int32_t dist, int32_t num_dec);
static const char * special_char_lut_1(char sym);
static const char * special_char_lut_2(const char * sym, size_t len, int32_t builtin);
static const char * special_char_lut_1_to_2(char sym);
static int is_word_char(char ch);

/* abbreviation list for use in decorated_string_abbreviate */
typedef struct
{
    const char * full;
    const char * abbr;
} full_abbr_pair_t;

full_abbr_pair_t full_abbr_pairs[] =
{
    {"Maximum", "Max"},
    {"maximum", "max"},
    {"Minimum", "Min"},
    {"minimum", "min"},
    {"Record", "Rec"},
    {"record", "rec"},
    {"Recording", "Rec"},
    {"recording", "rec"},
    {"Preview", "Prev"},
    {"preview", "prev"},
    {"Position", "Pos"},
    {"position", "pos"},
    {"Temperature", "Temp"},
    {"temperature", "temp"},
    {"Integration", "Int"},
    {"integration", "int"},
    {"Exposure", "Exp"},
    {"exposure", "exp"},
    {"Compensation", "Comp"},
    {"compensation", "comp"},
    {"Motion Mount", "MM"},
    {"Absolute", "Abs"},
    {"absolute", "abs"},
    {"Relative", "Rel"},
    {"relative", "rel"},
    {"Distance", "Dist"},
    {"distance", "dist"},
    {"Frequency", "Freq"},
    {"frequency", "freq"},
    {"Resolution", "Res"},
    {"resolution", "res"},
    {"External", "Ext"},
    {"external", "ext"},
    {"Camera", "Cam"},
    {"camera", "cam"},
    {"Number", "No"},
    {"number", "no"},
    {"Volume", "Vol"},
    {"volume", "vol"},
    {"Internal", "Int"},
    {"internal", "int"},
    {"Version", "Ver"},
    {"version", "ver"},
    {"Channel", "Ch"},
    {"channel", "ch"},
    {"Project", "Proj"},
    {"project", "proj"},
    {"Continuous", "Cont"},
    {"continuous", "cont"},
    {"Timecode", "TC"},
    {"timecode", "TC"},
    {"Independent", "Indep"},
    {"independent", "indep"},
    {"Space", "Sp"},
    {"space", "sp"},
    {"Firmware", "FW"},
    {"firmware", "FW"},
    {"Default", "Def"},
    {"default", "def"},
    {"Alternate", "Alt"},
    {"alternate", "alt"},
    {"Calibration", "Cal"},
    {"calibration", "cal"},
    {"Dimensions", "Dims"},
    {"dimensions", "dims"},
    {"Standard", "Std"},
    {"standard", "std"},
    {"Low Light", "LL"},
    {"low light", "ll"},
};

static const char * special_char_lut_2(const char * sym, size_t len, int32_t builtin)
{
    /* converts HTML style special chars to either ascii equivalent or
     * internal font mapping.
     *
     * &SPECIAL; where SPECIAL is one of:
     * RED:
     *   redana2        ANA 2.0 icon
     *   redana125      ANA 1.25 icon
     *   redana13       ANA 1.3 icon
     *   redana15       ANA 1.5 icon
     *   redana165      ANA 1.65 icon
     *   redana18       ANA 1.8 icon
     *   redae          AE icon
     *   rediso         subscript "ISO"
     *   redsec         subscript "sec"
     *   redkelvin      subscript "K"
     *   redformatk     subscript "K"
     *   redfps         subscript "FPS"
     *   red1over       superscript "1/"
     *   redfover       superscript "f/"
     *   redav          Av icon
     *   redcheck       check mark
     *   redsub2        subscript "2"
     *   redll          LL icon
     * Global:
     *   amp, deg, reg, copy, trade
     *
     * */
    switch (len)
    {
        /*
         * NOTE: If a special character is added below make sure to also
         * add it to the characters to compress table in c_list.c
         * */
        case 3:
            if (strncmp(sym, "amp", len) == 0)
                return builtin ? "&" : "&";
            else if (strncmp(sym, "deg", len) == 0)
                return builtin ? "\xb0" : " deg";
            else if (strncmp(sym, "reg", len) == 0)
                return builtin ? "\xae" : "(R)";
            break;

        case 4:
            if (strncmp(sym, "copy", len) == 0)
                return builtin ? "\xa9" : "(C)";
            break;

        case 5:
            if (strncmp(sym, "redae", len) == 0)
                return builtin ? "\x03" : " AE";
            else if (strncmp(sym, "redav", len) == 0)
                return builtin ? "\x0c" : " Av";
            else if (strncmp(sym, "trade", len) == 0)
                return builtin ? "\x0f" : "(TM)";
            else if (strncmp(sym, "redll", len) == 0)
                return builtin ? "\x12" : "LL";
            break;

        case 6:
            if (strncmp(sym, "redfps", len) == 0)
                return builtin ? "\x07" : " FPS";
            else if (strncmp(sym, "rediso", len) == 0)
                return builtin ? "\x04" : "ISO ";
            else if (strncmp(sym, "redsec", len) == 0)
                return builtin ? "\x05" : " sec";
            break;

        case 7:
            if (strncmp(sym, "redana2", len) == 0)
                return builtin ? "\x01" : " ANA 2";
            if (strncmp(sym, "redsub2", len) == 0)
                return builtin ? "\x10" : "2";
            break;

        case 8:
            if (strncmp(sym, "red1over", len) == 0)
                return builtin ? "\x08" : "1/";
            else if (strncmp(sym, "redana13", len) == 0)
                return builtin ? "\x02" : " ANA 1.3";
            else if (strncmp(sym, "redana15", len) == 0)
                return builtin ? "\x15" : " ANA 1.5";
            else if (strncmp(sym, "redana18", len) == 0)
                return builtin ? "\x13" : " ANA 1.8";
            else if (strncmp(sym, "redcheck", len) == 0)
                return builtin ? "\x0e" : " Check";
            else if (strncmp(sym, "redfover", len) == 0)
                return builtin ? "\x0b" : "f/";
            break;

        case 9:
            if (strncmp(sym, "redana125", len) == 0)
                return builtin ? "\x11" : " ANA 1.25";
            else if (strncmp(sym, "redana165", len) == 0)
                return builtin ? "\x14" : " ANA 1.65";
            else if (strncmp(sym, "redkelvin", len) == 0)
                return builtin ? "\x06" : "K";
            break;

        case 10:
            if (strncmp(sym, "redformatk", len) == 0)
                return builtin ? "\x06" : "K";
            break;

        default:
            break;
    }

    return NULL;
}

static const char * special_char_lut_1(char sym)
{
    switch (sym)
    {
        case '$':
            return "1/";

        case '&':
            return "f/";

        case '|':
            return " sec";

        case '{':
            return "ISO ";

        case '}':
            return "K";

        case '^':
            return " deg";

        case '~':
            return " FPS";

        case '!':
            return " ANA 2";

        case '\"':
            return " ANA 1.3";

        default:
            return NULL;
    }
}

static const char * special_char_lut_1_to_2(char sym)
{
    switch (sym)
    {
        case '$':
            return "&red1over;";

        case '&':
            return "&redfover;";

        case '|':
            return "&redsec;";

        case '{':
            return "&rediso;";

        case '}':
            return "&redformatk;";

        case '^':
            return "&deg;";

        case '~':
            return "&redfps;";

        case '!':
            return "&redana2;";

        case '\"':
            return "&redana13;";

        default:
            return NULL;
    }
}

void decorated_string_decode2(const char * src, char * dest, size_t dest_len, int32_t builtin)
{
    if (dest_len == 0)
    {
        return;
    }

    while (dest_len != 1 && *src)
    {
        if (*src == '&')
        {
            const char * end = src + 1;
            while (isalnum((int) *end))
            {
                end++;
            }
            if (*end == ';')
            {
                const char * replacement = special_char_lut_2(src + 1, (end - src) - 1, builtin);
                if (replacement)
                {
                    while (dest_len != 1 && *replacement)
                    {
                        *dest++ = *replacement++;
                        dest_len--;
                    }
                }
                src = end + 1;
            }
            else
            {
                *dest++ = *src++;
                dest_len--;
            }
        }
        else
        {
            *dest++ = *src++;
            dest_len--;
        }
    }

    *dest = '\0';
}

void decorated_string_decode(const char * src, char * dest, size_t dest_len)
{
    decorated_string_decode2(src, dest, dest_len, 0);
}

void decorated_string_decode1(const char * src, char * dest, size_t dest_len)
{
    if (dest_len == 0)
    {
        return;
    }

    while (dest_len != 1 && *src)
    {
        const char * replacement = special_char_lut_1(*src);
        if (!replacement)
        {
            *dest++ = *src++;
            dest_len--;
        }
        else
        {
            while (dest_len != 1 && *replacement)
            {
                *dest++ = *replacement++;
                dest_len--;
            }
            src++;
        }
    }

    *dest = '\0';
}

void decorated_string_1_to_2(const char * src, char * dest, size_t dest_len)
{
    if (dest_len == 0)
    {
        return;
    }

    while (dest_len != 1 && *src)
    {
        const char * replacement = special_char_lut_1_to_2(*src);
        if (!replacement)
        {
            *dest++ = *src++;
            dest_len--;
        }
        else
        {
            while (dest_len != 1 && *replacement)
            {
                *dest++ = *replacement++;
                dest_len--;
            }
            src++;
        }
    }

    *dest = '\0';
}

static void concatenate(char * dest, size_t dest_size, const char * prefix, const char * body, const char * postfix)
{
    if (prefix != NULL)
    {
        strlcpy(dest, prefix, dest_size);
    }
    else
    {
        strlcpy(dest, "", dest_size);
    }

    strlcat(dest, body, dest_size);

    if (postfix != NULL)
    {
        strlcat(dest, postfix, dest_size);
    }
}

static void num_to_str(char * dest, size_t dest_size, int32_t num, int32_t divider, int32_t digits)
{
    if (divider == 0)
    {
        snprintf(dest, dest_size, "---");
    }
    else
    {
        /* snprintf takes care of all rounding to the appropriate decimal point */
        snprintf(dest, dest_size, "%.*f", digits, (float) num / divider);
    }
}

void decorated_string_create(char * dest, size_t dest_size, int32_t num, int32_t divider, int32_t digits, const char * prefix, const char * postfix)
{
    char num_str[20];
    num_to_str(num_str, sizeof(num_str), num, divider, digits);
    concatenate(dest, dest_size, prefix, num_str, postfix);
}

void decorated_string_create_fps_label(char * dest, size_t dest_size, decorated_string_len_t len, int32_t fps)
{
    switch (len)
    {
        default:
        case DECORATED_STRING_LEN_NORMAL:
            if (fps >= FPS_DIVIDER)
            {
                decorated_string_create(dest, dest_size, fps, FPS_DIVIDER, (fps % FPS_DIVIDER) ? 2 : 0, "", "&redfps;");
            }
            else
            {
                decorated_string_create(dest, dest_size, FPS_DIVIDER, fps, (FPS_DIVIDER % fps) ? 2 : 0, "&red1over;", "&redfps;");
            }
            break;

        case DECORATED_STRING_LEN_ABBREVIATED:
            if (fps >= FPS_DIVIDER)
            {
                decorated_string_create(dest, dest_size, fps, FPS_DIVIDER, (fps % FPS_DIVIDER) ? 2 : 0, "", "");
            }
            else
            {
                decorated_string_create(dest, dest_size, FPS_DIVIDER, fps, (FPS_DIVIDER % fps) ? 2 : 0, "&red1over;", "");
            }
            break;
    }
}

void decorated_string_create_int_time_label(char * dest, size_t dest_size, decorated_string_len_t len, int32_t show_angle, int32_t int_time, int32_t int_time_angle, frame_processing_t frameproc, int32_t frame_divider, int32_t ae_control)
{
    char suffix[50];

    if (show_angle)
    {
        if (ae_control)
        {
            decorated_string_create(dest, dest_size, int_time_angle, 1000, 1, "&redae; ", "&deg;");
        }
        else
        {
            decorated_string_create(dest, dest_size, int_time_angle, 1000, 1, "", "&deg;");
        }
    }
    else
    {
        switch (len)
        {
            default:
            case DECORATED_STRING_LEN_NORMAL:
                if (frameproc == FRAME_PROCESSING_SUM)
                {
                    snprintf(suffix, sizeof(suffix), "x%d", frame_divider);
                }
                else
                {
                    suffix[0] = 0;
                }

                if (int_time > INT_TIME_DIVIDER)
                {
                    if (ae_control)
                    {
                        decorated_string_create(dest, dest_size, int_time, INT_TIME_DIVIDER, (int_time % INT_TIME_DIVIDER) ? 2 : 0, "&redae; &red1over;", "");
                    }
                    else
                    {
                        decorated_string_create(dest, dest_size, int_time, INT_TIME_DIVIDER, (int_time % INT_TIME_DIVIDER) ? 2 : 0, "&red1over;", "");
                    }
                }
                else
                {
                    int32_t digits;

                    if (int_time == 0)
                    {
                        digits = 0;
                    }
                    else
                    {
                        digits = (INT_TIME_DIVIDER % int_time) ? 2 : 0;
                    }

                    if (ae_control)
                    {
                        decorated_string_create(dest, dest_size, INT_TIME_DIVIDER, int_time, digits, "&redae; ", "&redsec;");
                    }
                    else
                    {
                        decorated_string_create(dest, dest_size, INT_TIME_DIVIDER, int_time, digits, "", "&redsec;");
                    }
                }

                strlcat(dest, suffix, dest_size);
                break;

            case DECORATED_STRING_LEN_ABBREVIATED:
                if (frameproc == FRAME_PROCESSING_SUM)
                {
                    snprintf(suffix, sizeof(suffix), "x");
                }
                else
                {
                    suffix[0] = 0;
                }

                if (int_time > INT_TIME_DIVIDER)
                {
                    if (ae_control)
                    {
                        decorated_string_create(dest, dest_size, int_time, INT_TIME_DIVIDER, (int_time % INT_TIME_DIVIDER) ? 2 : 0, "&redae; &red1over;", "");
                    }
                    else
                    {
                        decorated_string_create(dest, dest_size, int_time, INT_TIME_DIVIDER, (int_time % INT_TIME_DIVIDER) ? 2 : 0, "&red1over;", "");
                    }
                }
                else
                {
                    int32_t digits;

                    if (int_time == 0)
                    {
                        digits = 0;
                    }
                    else
                    {
                        digits = (INT_TIME_DIVIDER % int_time) ? 2 : 0;
                    }

                    if (ae_control)
                    {
                        decorated_string_create(dest, dest_size, INT_TIME_DIVIDER, int_time, digits, "&redae; ", "&redsec;");
                    }
                    else
                    {
                        decorated_string_create(dest, dest_size, INT_TIME_DIVIDER, int_time, digits, "", "&redsec;");
                    }
                }

                strlcat(dest, suffix, dest_size);
                break;
        }
    }
}

void decorated_string_create_red_gamma(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value)
{
    switch (len)
    {
        default:
        case DECORATED_STRING_LEN_NORMAL:
            switch (value)
            {
                case GAMMACURVE_REDLOGFILM:
                    strlcpy(dest, "REDlogFilm", dest_size);
                    break;

                case GAMMACURVE_REDGAMMA2:
                    strlcpy(dest, "REDgamma2", dest_size);
                    break;

                case GAMMACURVE_REDGAMMA3:
                    strlcpy(dest, "REDgamma3", dest_size);
                    break;

                case GAMMACURVE_REDGAMMA4:
                    strlcpy(dest, "REDgamma4", dest_size);
                    break;

                case GAMMACURVE_ACESPROXY:
                    strlcpy(dest, "ACES Proxy", dest_size);
                    break;

                case GAMMACURVE_ACESCC:
                    strlcpy(dest, "ACEScc", dest_size);
                    break;

                case GAMMACURVE_LOG3G12:
                    strlcpy(dest, "Log3G12", dest_size);
                    break;

                case GAMMACURVE_BT1886:
                    strlcpy(dest, "BT-1886", dest_size);
                    break;

                case GAMMACURVE_HDR2084:
                    strlcpy(dest, "HDR-2084", dest_size);
                    break;

                case GAMMACURVE_LOG3G10:
                    strlcpy(dest, "Log3G10", dest_size);
                    break;

                default:
                    strlcpy(dest, "---", dest_size);
                    break;
            }
            break;

        case DECORATED_STRING_LEN_ABBREVIATED:
            switch (value)
            {
                case GAMMACURVE_REDLOGFILM:
                    strlcpy(dest, "RLF", dest_size);
                    break;

                case GAMMACURVE_REDGAMMA2:
                    strlcpy(dest, "RG2", dest_size);
                    break;

                case GAMMACURVE_REDGAMMA3:
                    strlcpy(dest, "RG3", dest_size);
                    break;

                case GAMMACURVE_REDGAMMA4:
                    strlcpy(dest, "RG4", dest_size);
                    break;

                case GAMMACURVE_ACESPROXY:
                    strlcpy(dest, "ACES", dest_size);
                    break;

                case GAMMACURVE_ACESCC:
                    strlcpy(dest, "ACEScc", dest_size);
                    break;

                case GAMMACURVE_LOG3G12:
                    strlcpy(dest, "Log3G12", dest_size);
                    break;

                case GAMMACURVE_BT1886:
                    strlcpy(dest, "1886", dest_size);
                    break;

                case GAMMACURVE_HDR2084:
                    strlcpy(dest, "2084", dest_size);
                    break;

                case GAMMACURVE_LOG3G10:
                    strlcpy(dest, "Log3G10", dest_size);
                    break;

                default:
                    strlcpy(dest, "---", dest_size);
                    break;
            }
            break;
    }
}

void decorated_string_create_red_color(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value)
{
    switch (len)
    {
        default:
        case DECORATED_STRING_LEN_NORMAL:
            switch (value)
            {
                case COLORSPACE_REDCOLOR2:
                    strlcpy(dest, "REDcolor2", dest_size);
                    break;

                case COLORSPACE_REDCOLOR3:
                    strlcpy(dest, "REDcolor3", dest_size);
                    break;

                case COLORSPACE_REDCOLOR4:
                    strlcpy(dest, "REDcolor4", dest_size);
                    break;

                case COLORSPACE_DRAGONCOLOR:
                    strlcpy(dest, "DRAGONcolor", dest_size);
                    break;

                case COLORSPACE_DRAGONCOLOR2:
                    strlcpy(dest, "DRAGONcolor2", dest_size);
                    break;

                case COLORSPACE_ACES:
                case COLORSPACE_ACES_AP1:
                    strlcpy(dest, "ACES", dest_size);
                    break;

                case COLORSPACE_REC709:
                    strlcpy(dest, "Rec. 709", dest_size);
                    break;

                case COLORSPACE_REC2020:
                    strlcpy(dest, "Rec. 2020", dest_size);
                    break;

                case COLORSPACE_RWGRGB:
                    strlcpy(dest, "REDWideGamutRGB", dest_size);
                    break;

                default:
                    strlcpy(dest, "---", dest_size);
                    break;
            }
            break;

        case DECORATED_STRING_LEN_ABBREVIATED:
            switch (value)
            {
                case COLORSPACE_REDCOLOR2:
                    strlcpy(dest, "RC2", dest_size);
                    break;

                case COLORSPACE_REDCOLOR3:
                    strlcpy(dest, "RC3", dest_size);
                    break;

                case COLORSPACE_REDCOLOR4:
                    strlcpy(dest, "RC4", dest_size);
                    break;

                case COLORSPACE_DRAGONCOLOR:
                    strlcpy(dest, "DC", dest_size);
                    break;

                case COLORSPACE_DRAGONCOLOR2:
                    strlcpy(dest, "DC2", dest_size);
                    break;

                case COLORSPACE_ACES:
                case COLORSPACE_ACES_AP1:
                    strlcpy(dest, "ACES", dest_size);
                    break;

                case COLORSPACE_REC709:
                    strlcpy(dest, "Rec709", dest_size);
                    break;

                case COLORSPACE_REC2020:
                    strlcpy(dest, "Rec2020", dest_size);
                    break;

                case COLORSPACE_RWGRGB:
                    strlcpy(dest, "RWG", dest_size);
                    break;

                default:
                    strlcpy(dest, "---", dest_size);
                    break;
            }
            break;
    }
}

typedef struct
{
    int32_t enum_entry;
    const char * normal;
    const char * abbreviated;
} string_table_t;

const string_table_t main_output_preset_string_table[MAIN_OUTPUT_PRESET_COUNT] =
{
    {MAIN_OUTPUT_PRESET_ACES_PROXY, "ACES PROXY", "ACES"},
    {MAIN_OUTPUT_PRESET_BT1886, "BT-1886", "1886"},
    {MAIN_OUTPUT_PRESET_HDR, "HDR", "HDR"},
    {MAIN_OUTPUT_PRESET_HDR2084, "HDR-2084", "2084"},
    {MAIN_OUTPUT_PRESET_HDR_400, "HDR 400 Nits", "HDR400"},
    {MAIN_OUTPUT_PRESET_HDR_1K, "HDR 1K Nits", "HDR1K"},
    {MAIN_OUTPUT_PRESET_HDR_2K, "HDR 2K Nits", "HDR2K"},
    {MAIN_OUTPUT_PRESET_HDR_4K, "HDR 4K Nits", "HDR4K"},
    {MAIN_OUTPUT_PRESET_LOG3G10, "Log3G10", "Log3G10"},
    {MAIN_OUTPUT_PRESET_REC709, "Rec. 709", "Rec709"},
    {MAIN_OUTPUT_PRESET_REDGAMMA2, "REDgamma2", "RG2"},
    {MAIN_OUTPUT_PRESET_REDGAMMA3, "REDgamma3", "RG3"},
    {MAIN_OUTPUT_PRESET_REDGAMMA4, "REDgamma4", "RG4"},
    {MAIN_OUTPUT_PRESET_REDLOGFILM, "REDlogFilm", "RLF"},
    {MAIN_OUTPUT_PRESET_SDR, "SDR", "SDR"},
    {MAIN_OUTPUT_PRESET_SDR_REC709, "SDR Rec. 709", "SDR709"},
    {MAIN_OUTPUT_PRESET_SDR_REC709_NO_KNEE, "SDR Rec709 No Knee", "SDR709NK"},
    {MAIN_OUTPUT_PRESET_USER, "Log3G10 : 3DLUT", "Log3G10:3D"},
    {MAIN_OUTPUT_PRESET_RLF_TO_3DLUT, "REDlogFilm : 3DLUT", "RLF:3D"},
    {MAIN_OUTPUT_PRESET_IPP2A, "Log3G10 : 3DLUT", "Log3G10:3D"},
};

#define MAIN_OUTPUT_PRESET_STRING_TABLE_COUNT (sizeof(main_output_preset_string_table) / sizeof(string_table_t))

void decorated_string_create_main_output_preset(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value)
{
    const int32_t string_count = MAIN_OUTPUT_PRESET_STRING_TABLE_COUNT;

    if (value != main_output_preset_string_table[value].enum_entry || (value >= string_count))
    {
        strlcpy(dest, "---", dest_size);
        return;
    }

    if (DECORATED_STRING_LEN_ABBREVIATED == len)
    {
        strlcpy(dest, main_output_preset_string_table[value].abbreviated, dest_size);
    }
    else
    {
        strlcpy(dest, main_output_preset_string_table[value].normal, dest_size);
    }
}

const string_table_t display_preset_string_table[DISPLAY_PRESET_COUNT] =
{
    {DISPLAY_PRESET_ACES_PROXY, "ACES Proxy", "ACES"},
    {DISPLAY_PRESET_FLUT_BT1886, "BT-1886", "1886"},
    {DISPLAY_PRESET_FLUT_HDR2084, "HDR-2084", "2084"},
    {DISPLAY_PRESET_FLUT_LOG3G10, "Log3G10", "Log3G10"},
    {DISPLAY_PRESET_FLUT_REDGAMMA2, "REDgamma2", "RG2"},
    {DISPLAY_PRESET_FLUT_REDGAMMA3, "REDgamma3", "RG3"},
    {DISPLAY_PRESET_FLUT_REDGAMMA4, "REDgamma4", "RG4"},
    {DISPLAY_PRESET_FLUT_REDLOGFILM, "REDlogFilm", "RLF"},
    {DISPLAY_PRESET_MAIN_HDR, "HDR", "HDR"},
    {DISPLAY_PRESET_MAIN_HDR_400, "HDR 400 Nits", "HDR400"},
    {DISPLAY_PRESET_MAIN_HDR_1K, "HDR 1K Nits", "HDR1K"},
    {DISPLAY_PRESET_MAIN_HDR_2K, "HDR 2K Nits", "HDR2K"},
    {DISPLAY_PRESET_MAIN_HDR_4K, "HDR 4K Nits", "HDR4K"},
    {DISPLAY_PRESET_MAIN_SDR, "SDR", "SDR"},
    {DISPLAY_PRESET_MAIN_HLG, "HLG", "HLG"},
    {DISPLAY_PRESET_MAIN_SDR_REC709, "SDR Rec. 709", "SDR709"},
    {DISPLAY_PRESET_MAIN_SDR_REC709_NO_KNEE, "SDR Rec. 709", "SDR709NK"},
    {DISPLAY_PRESET_RWGRGB_HDR, "HDR", "HDR"},
    {DISPLAY_PRESET_RWGRGB_HDR_400, "HDR 400 Nits", "HDR400"},
    {DISPLAY_PRESET_RWGRGB_HDR_1K, "HDR 1K Nits", "HDR1K"},
    {DISPLAY_PRESET_RWGRGB_HDR_2K, "HDR 2K Nits", "HDR2K"},
    {DISPLAY_PRESET_RWGRGB_HDR_4K, "HDR 4K Nits", "HDR4K"},
    {DISPLAY_PRESET_RWGRGB_SDR, "SDR", "SDR"},
    {DISPLAY_PRESET_RWGRGB_HLG, "HLG", "HLG"},
    {DISPLAY_PRESET_RWGRGB_SDR_REC709, "SDR Rec. 709", "SDR709"},
    {DISPLAY_PRESET_RWGRGB_SDR_REC709_NO_KNEE, "SDR Rec. 709", "SDR709NK"},
    {DISPLAY_PRESET_USER, "Log3G10:3DLUT", "Log3G10:3D"},
    {DISPLAY_PRESET_RLF_TO_3DLUT, "REDlogFilm:3DLUT", "RLF:3D"}
};

#define DISPLAY_PRESET_STRING_TABLE_COUNT (sizeof(display_preset_string_table) / sizeof(string_table_t))

void decorated_string_create_display_preset(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value)
{
    const int32_t string_count = DISPLAY_PRESET_STRING_TABLE_COUNT;

    if (value != display_preset_string_table[value].enum_entry || (value >= string_count))
    {
        strlcpy(dest, "---", dest_size);
        return;
    }

    if (DECORATED_STRING_LEN_ABBREVIATED == len)
    {
        strlcpy(dest, display_preset_string_table[value].abbreviated, dest_size);
    }
    else
    {
        strlcpy(dest, display_preset_string_table[value].normal, dest_size);
    }
}

const string_table_t output_transform_string_table[OUTPUT_TRANSFORM_COUNT] =
{
    {OUTPUT_TRANSFORM_ACES_AP1, "ACES AP1", "ACES"},
    {OUTPUT_TRANSFORM_DRAGONCOLOR, "DRAGONcolor", "DC"},
    {OUTPUT_TRANSFORM_DRAGONCOLOR2, "DRAGONcolor2", "DC2"},
    {OUTPUT_TRANSFORM_P3, "DCI-P3", "DCI-P3"},
    {OUTPUT_TRANSFORM_REC709, "Rec. 709", "Rec709"},
    {OUTPUT_TRANSFORM_REC2020, "Rec. 2020", "Rec2020"},
    {OUTPUT_TRANSFORM_REDCOLOR2, "REDcolor2", "RC2"},
    {OUTPUT_TRANSFORM_REDCOLOR3, "REDcolor3", "RC3"},
    {OUTPUT_TRANSFORM_REDCOLOR4, "REDcolor4", "RC4"},
    {OUTPUT_TRANSFORM_RWGRGB, "REDWideGamutRGB", "RWGRGB"},
    {OUTPUT_TRANSFORM_NONE, "NONE", "NONE"},
    {OUTPUT_TRANSFORM_CUSTOM, "CUSTOM", "CUSTOM"},    
    {OUTPUT_TRANSFORM_IPP2A, "REDWideGamutRGB", "RWGRGB"},
};

#define OUTPUT_TRANSFORM_STRING_TABLE_COUNT (sizeof(output_transform_string_table) / sizeof(string_table_t))

void decorated_string_create_output_transform(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value)
{
    const int32_t string_count = OUTPUT_TRANSFORM_STRING_TABLE_COUNT;

    if (value != output_transform_string_table[value].enum_entry || (value >= string_count))
    {
        strlcpy(dest, "---", dest_size);
        return;
    }

    if (DECORATED_STRING_LEN_ABBREVIATED == len)
    {
        strlcpy(dest, output_transform_string_table[value].abbreviated, dest_size);
    }
    else
    {
        strlcpy(dest, output_transform_string_table[value].normal, dest_size);
    }
}

void decorated_string_create_iso(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value, int32_t iso_pull, int32_t nd_val, int32_t sensor_gain_supported, int32_t sensor_gain_mode)
{
    if (iso_pull)
    {
        value = (int) roundf(value / powf(2, nd_val / 100.0f));
    }

    switch (len)
    {
        default:
        case DECORATED_STRING_LEN_NORMAL:
            if (sensor_gain_supported && sensor_gain_mode == SENSOR_SENSITIVITY_LOW_LIGHT)
            {
                decorated_string_create(dest, dest_size, value, 1, 0, "&rediso;", " &redll;");
            }
            else
            {
                decorated_string_create(dest, dest_size, value, 1, 0, "&rediso;", "");
            }
            break;

        case DECORATED_STRING_LEN_ABBREVIATED:
            if (sensor_gain_supported && sensor_gain_mode == SENSOR_SENSITIVITY_LOW_LIGHT)
            {
                decorated_string_create(dest, dest_size, value, 1, 0, "", " &redll;");
            }
            else
            {
                decorated_string_create(dest, dest_size, value, 1, 0, "", "");
            }
            break;
    }
}

void decorated_string_create_redcode(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value)
{
    switch (len)
    {
        default:
        case DECORATED_STRING_LEN_NORMAL:
            snprintf(dest, dest_size, "%d:1", (value + 99) / 100);
            break;

        case DECORATED_STRING_LEN_ABBREVIATED:
            snprintf(dest, dest_size, "%d:1", (value + 99) / 100);
            break;
    }
}

void decorated_string_create_file_format(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value)
{
    switch (len)
    {
        default:
        case DECORATED_STRING_LEN_NORMAL:
        case DECORATED_STRING_LEN_ABBREVIATED:
            switch (value)
            {
                case 0:
                    strlcpy(dest, "Redcode", dest_size);
                    break;

                case 1:
                    strlcpy(dest, "QuickTime", dest_size);
                    break;
            }
    }
}

void decorated_string_create_aperture(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value, int32_t ae_control)
{
    const int base = value / 10;
    const int decimal = value % 10;

    if (value <= 0)
    {
        strlcpy(dest, "N/A", dest_size);
    }
    else if (decimal == 0)
    {
        if (ae_control)
        {
            snprintf(dest, dest_size, "&redae; &redfover;%d", base);
        }
        else
        {
            snprintf(dest, dest_size, "&redfover;%d", base);
        }
    }
    else
    {
        if (ae_control)
        {
            snprintf(dest, dest_size, "&redae; &redfover;%d.%d", base, decimal);
        }
        else
        {
            snprintf(dest, dest_size, "&redfover;%d.%d", base, decimal);
        }
    }
}

void decorated_string_create_power(char * dest, size_t dest_size, decorated_string_len_t len, battery_display_mode_t display_mode, int32_t power_level, int32_t runtime)
{
    if (power_level < 101)
    {
        /* Battery */
        if (display_mode == BATTERY_DISPLAY_MODE_PERCENT)
        {
            decorated_string_create_power_percent(dest, dest_size, len, power_level);
        }
        else if (display_mode == BATTERY_DISPLAY_MODE_TOTAL_TIME)
        {
            decorated_string_create_power_runtime(dest, dest_size, len, runtime);
        }
    }
    else
    {
        /* DC Voltage */
        decorated_string_create_power_voltage(dest, dest_size, len, power_level);
    }
}

void decorated_string_create_power2(char * dest, size_t dest_size, decorated_string_len_t len, battery_display_mode_t display_mode, int32_t voltage, int32_t runtime, int32_t percent)
{
    if (percent != BATTERY_INVALID_VALUE && runtime != BATTERY_INVALID_VALUE && display_mode == BATTERY_DISPLAY_MODE_PERCENT)
    {
        decorated_string_create_power_percent(dest, dest_size, len, percent);
    }
    else if (runtime != BATTERY_INVALID_VALUE && display_mode == BATTERY_DISPLAY_MODE_TOTAL_TIME)
    {
        decorated_string_create_power_runtime(dest, dest_size, len, runtime);
    }
    else
    {
        decorated_string_create_power_voltage(dest, dest_size, len, voltage);
    }
}

void decorated_string_create_power_voltage(char * dest, size_t dest_size, decorated_string_len_t len, int32_t voltage)
{
    if (voltage < 0)
    {
        strlcpy(dest, "N/A", dest_size);
    }
    else
    {
        int voltage_deca_volts = voltage / 100;

        snprintf(dest, dest_size, "%d.%dV", voltage_deca_volts / 10, voltage_deca_volts % 10);
    }
}

void decorated_string_create_power_percent(char * dest, size_t dest_size, decorated_string_len_t len, int32_t percent)
{
    if (percent < 0)
    {
        snprintf(dest, dest_size, "N/A");
    }
    else
    {
        snprintf(dest, dest_size, "%d%%", percent);
    }
}

void decorated_string_create_power_runtime(char * dest, size_t dest_size, decorated_string_len_t len, int32_t runtime)
{
    /*  Show batteries with capacity larger then 10 hours as Unknown. */
    if ((runtime < BATTERY_RUNTIME_MAX_MINUTES) && (runtime > 0))
    {
        snprintf(dest, dest_size, "%d:%02d", runtime / 60, runtime % 60);
    }
    else
    {
        snprintf(dest, dest_size, "N/A");
    }
}

static void format_imperial_dist(char * str, size_t size, int32_t dist)
{
    const int32_t total_inches = (int) roundf((float) dist / 2.54);
    const int32_t feet = total_inches / 12;
    const int32_t inches = total_inches % 12;

    if (dist < 0)
    {
        snprintf(str, size, "---");
    }
    else if (dist >= 0xffff)
    {
        snprintf(str, size, "inf");
    }
    else if (feet == 0)
    {
        snprintf(str, size, "%d\"", inches);
    }
    else if (inches == 0)
    {
        snprintf(str, size, "%d'", feet);
    }
    else
    {
        snprintf(str, size, "%d'%d\"", feet, inches);
    }
}

static void format_metric_dist(char * str, size_t size, int32_t dist, int32_t num_dec)
{
    if (dist < 0)
    {
        snprintf(str, size, "---");
    }
    else if (dist >= 0xffff)
    {
        snprintf(str, size, "inf");
    }
    else if (dist < 100)
    {
        snprintf(str, size, "%dcm", dist);
    }
    else
    {
        snprintf(str, size, "%.*fm", num_dec, dist / 100.0);
    }
}

void decorated_string_create_focus_dist(char * dest, size_t dest_size, decorated_string_len_t len, focus_distance_mode_t display_mode, int32_t near_dist, int32_t far_dist)
{
    char near_dist_str[20];
    char far_dist_str[20];

    if ((near_dist == far_dist && near_dist <= 0) || near_dist < 0 || far_dist < 0)
    {
        strlcpy(dest, "N/A", dest_size);
        return;
    }

    switch (display_mode)
    {
        case FOCUS_DISTANCE_MODE_METRIC:
            format_metric_dist(near_dist_str, sizeof(near_dist_str), near_dist, (len == DECORATED_STRING_LEN_NORMAL) ? 2 : 1);
            format_metric_dist(far_dist_str, sizeof(far_dist_str), far_dist, (len == DECORATED_STRING_LEN_NORMAL) ? 2 : 1);
            break;

        case FOCUS_DISTANCE_MODE_IMPERIAL:
        default:
            format_imperial_dist(near_dist_str, sizeof(near_dist_str), near_dist);
            format_imperial_dist(far_dist_str, sizeof(far_dist_str), far_dist);
            break;
    }

    if (strcmp(near_dist_str, far_dist_str) == 0)
    {
        snprintf(dest, dest_size, "%s", near_dist_str);
    }
    else
    {
        switch (len)
        {
            default:
            case DECORATED_STRING_LEN_NORMAL:
                snprintf(dest, dest_size, "%s - %s", near_dist_str, far_dist_str);
                break;

            case DECORATED_STRING_LEN_ABBREVIATED:
                snprintf(dest, dest_size, "%s-%s", near_dist_str, far_dist_str);
                break;
        }
    }
}

void decorated_string_create_focal_length(char * dest, size_t dest_size, decorated_string_len_t len, int32_t value)
{
    switch (len)
    {
        default:
        case DECORATED_STRING_LEN_NORMAL:
            snprintf(dest, dest_size, "%dmm", (value + 50) / 100);
            break;

        case DECORATED_STRING_LEN_ABBREVIATED:
            snprintf(dest, dest_size, "%d", (value + 50) / 100);
            break;
    }
}

void decorated_string_create_media(char * dest, size_t dest_size, decorated_string_len_t len, decorated_string_color_t * color, record_mode_t record_mode, media_display_t display_mode, int32_t media_level, int32_t runtime)
{
    if (record_mode == RECORD_MODE_EXTERNAL)
    {
        strlcpy(dest, "EXT", dest_size);
        if (color)
        {
            *color = DECORATED_STRING_COLOR_WARNING;
        }
    }
    else if (record_mode == RECORD_MODE_STREAM)
    {
        strlcpy(dest, "STR", dest_size);
        if (color)
        {
            *color = DECORATED_STRING_COLOR_WARNING;
        }
    }
    else
    {
        if (media_level < 0)
        {
            if (color)
            {
                *color = DECORATED_STRING_COLOR_DISABLED;
            }
            strlcpy(dest, "", dest_size);
        }
        else
        {
            switch (display_mode)
            {
                case MEDIA_DISPLAY_PERCENTAGE:
                    decorated_string_create(dest, dest_size, media_level, 1, 0, "", "%");
                    break;

                case MEDIA_DISPLAY_TIME:
                    if (runtime == -1)
                    {
                        decorated_string_create(dest, dest_size, media_level, 1, 0, "", "%");
                    }
                    else
                    {
                        snprintf(dest, dest_size, "%d:%02d", runtime / 60, runtime % 60);
                    }
                    break;
            }

            if (color)
            {
                if (media_level > 10)
                {
                    *color = DECORATED_STRING_COLOR_OK;
                }
                else if (media_level > 5)
                {
                    *color = DECORATED_STRING_COLOR_WARNING;
                }
                else
                {
                    *color = DECORATED_STRING_COLOR_ERROR;
                }
            }
        }
    }
}

void decorated_string_create_color_temperature(char * dest, size_t dest_size, decorated_string_len_t len, int32_t color_temp)
{
    if (color_temp == 0)
    {
        snprintf(dest, dest_size, "N/A");
    }
    else
    {
        switch (len)
        {
            default:
            case DECORATED_STRING_LEN_NORMAL:
                decorated_string_create(dest, dest_size, color_temp, 1, 0, "", "&redkelvin;");
                break;

            case DECORATED_STRING_LEN_ABBREVIATED:
                decorated_string_create(dest, dest_size, color_temp, 1, 0, "", "&redkelvin;");
                break;
        }
    }
}

void decorated_string_create_temperature(char * dest, size_t dest_size, decorated_string_len_t len, int32_t temp)
{
    switch (len)
    {
        default:
        case DECORATED_STRING_LEN_NORMAL:
            decorated_string_create(dest, dest_size, temp, 1, 0, "", "&deg;C");
            break;

        case DECORATED_STRING_LEN_ABBREVIATED:
            decorated_string_create(dest, dest_size, temp, 1, 0, "", "");
            break;
    }
}

void decorated_string_create_fan_speed(char * dest, size_t dest_size, decorated_string_len_t len, int32_t fan_speed)
{
    if (fan_speed == -1)
    {
        snprintf(dest, dest_size, "---");
    }
    else
    {
        switch (len)
        {
            default:
            case DECORATED_STRING_LEN_NORMAL:
                decorated_string_create(dest, dest_size, fan_speed, 1, 0, "", "%");
                break;

            case DECORATED_STRING_LEN_ABBREVIATED:
                decorated_string_create(dest, dest_size, fan_speed, 1, 0, "", "");
                break;
        }
    }
}

void decorated_string_create_gyro_angle(char * dest, size_t dest_size, decorated_string_len_t len, int32_t angle)
{
    switch (len)
    {
        default:
        case DECORATED_STRING_LEN_NORMAL:
            decorated_string_create(dest, dest_size, angle, 10000, 1, "", "&deg;");
            break;

        case DECORATED_STRING_LEN_ABBREVIATED:
            decorated_string_create(dest, dest_size, angle, 10000, 0, "", "");
            break;
    }
}

static int is_word_char(char ch)
{
    return isalnum((int) ch) || ch == '\\' || ch == '/';
}

void decorated_string_abbreviate(char * str)
{
    /* TODO: can we make this case-insensitive so the master list
     * doesn't need variations? */
    size_t ii;
    for (ii = 0; ii < sizeof(full_abbr_pairs) / sizeof(full_abbr_pairs[0]); ii++)
    {
        /* cur_pos is not declared as pointing to const char because
         * strstr is overloaded in C++ with a version that takes in
         * const char * and return const char *. */

        /*lint -esym(954, cur_pos) */
        char * cur_pos = str;
        char * find_pos;
        const size_t len_full = strlen(full_abbr_pairs[ii].full);
        const size_t len_abbr = strlen(full_abbr_pairs[ii].abbr);

        do
        {
            /* look for full form of word in the remainder of our string */
            find_pos = strstr(cur_pos, full_abbr_pairs[ii].full);

            if (find_pos)
            {
                /* we only want to find "whole words" that match the
                 * full word.  That is, it only counts if the match is
                 * surrounded by white space, punctuation, etc, or is at
                 * full extent of the string. */

                if (
                    /* first character of match is beginning of string or preceded by a space */
                    (find_pos == str || !is_word_char((*(find_pos - 1))))
                    &&
                    /* last character of match is end of string or followed by a space */
                    (*(find_pos + len_full) == 0 || !is_word_char((*(find_pos + len_full))))
                   )
                {
                    const char * copy_src = find_pos + len_full;
                    char * copy_dest = find_pos + len_abbr;

                    /* we know the abbreviation is shorter than the
                     * full word (otherwise it's not much of an
                     * abbreviation), so we can copy the abbreviation
                     * over at the current location and then copy the
                     * rest of the string. */
                    strncpy(find_pos, full_abbr_pairs[ii].abbr, len_abbr);

                    /* copy the rest of the string.  We can't just call
                     * strcpy because the buffers would be overlapping. */
                    while (*copy_src)
                    {
                        *copy_dest++ = *copy_src++;
                    }
                    *copy_dest = 0;

                    /* continue search after replaced word */
                    cur_pos = find_pos + len_abbr + 1;
                }
                else
                {
                    cur_pos++;
                }
            }
            else
            {
                cur_pos = NULL;
            }
        }
        while (cur_pos && *cur_pos);
    }
}
