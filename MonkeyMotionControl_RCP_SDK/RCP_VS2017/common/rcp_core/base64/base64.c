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

#include "base64.h"
#include <string.h>

/**
 * characters used for Base64 encoding
 */
static const char * BASE64_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

/**
 * encode three bytes using base64 (RFC 3548)
 *
 * @param triple three bytes that should be encoded
 * @param result buffer of four characters where the result is stored
 */
static void _base64_encode_triple(const unsigned char triple[3], char result[4])
{
    int tripleValue;
    int ii;

    tripleValue = triple[0];
    tripleValue *= 256;
    tripleValue += triple[1];
    tripleValue *= 256;
    tripleValue += triple[2];

    for (ii = 0; ii < 4; ii++)
    {
        result[3 - ii] = BASE64_CHARS[tripleValue % 64];
        tripleValue /= 64;
    }
}

/**
 * encode an array of bytes using Base64 (RFC 3548)
 *
 * @param source the source buffer
 * @param sourcelen the length of the source buffer
 * @param target the target buffer
 * @param targetlen the length of the target buffer
 * @return 1 on success, 0 otherwise
 */
int base64_encode(const unsigned char * source, size_t sourcelen, char * target, size_t targetlen)
{
    if (!source || !target)
    {
        return 0;
    }

    /* check if the result will fit in the target buffer */
    if ((sourcelen + 2) / 3 * 4 > targetlen - 1)
    {
        return 0;
    }

    /* encode all full triples */
    while (sourcelen >= 3)
    {
        _base64_encode_triple(source, target);
        sourcelen -= 3;
        source += 3;
        target += 4;
    }

    /* encode the last one or two characters */
    if (sourcelen > 0)
    {
        unsigned char temp[3];
        memset(temp, 0, sizeof(temp));
        memcpy(temp, source, sourcelen);
        _base64_encode_triple(temp, target);
        target[3] = '=';
        if (sourcelen == 1)
        {
            target[2] = '=';
        }

        target += 4;
    }

    /* terminate the string */
    target[0] = 0;

    return 1;
}

/**
 * determine the value of a base64 encoding character
 *
 * @param base64char the character of which the value is searched
 * @return the value in case of success (0-63), -1 on failure
 */
static int _base64_char_value(char base64char)
{
    if (base64char >= 'A' && base64char <= 'Z')
    {
        return base64char - 'A';
    }
    else if (base64char >= 'a' && base64char <= 'z')
    {
        return base64char - 'a' + 26;
    }
    else if (base64char >= '0' && base64char <= '9')
    {
        return base64char - '0' + 2 * 26;
    }
    else if (base64char == '+')
    {
        return 2 * 26 + 10;
    }
    else if (base64char == '/')
    {
        return 2 * 26 + 11;
    }
    else
    {
        return -1;
    }
}

/**
 * decode a 4 char base64 encoded byte triple
 *
 * @param quadruple the 4 characters that should be decoded
 * @param result the decoded data
 * @return lenth of the result (1, 2 or 3), 0 on failure
 */
static int _base64_decode_triple(const char quadruple[4], unsigned char * result)
{
    int ii;
    int triple_value;
    int bytes_to_decode = 3;
    int only_equals_yet = 1;
    int char_value[4];

    for (ii = 0; ii < 4; ii++)
    {
        char_value[ii] = _base64_char_value(quadruple[ii]);
    }

    /* check if the characters are valid */
    for (ii = 3; ii >= 0; ii--)
    {
        if (char_value[ii] < 0)
        {
            if (only_equals_yet && quadruple[ii] == '=')
            {
                /* we will ignore this character anyway, make it something
                 * that does not break our calculations */
                char_value[ii] = 0;
                bytes_to_decode--;
                continue;
            }
            return 0;
        }
        /* after we got a real character, no other '=' are allowed anymore */
        only_equals_yet = 0;
    }

    /* if we got "====" as input, bytes_to_decode is -1 */
    if (bytes_to_decode < 0)
    {
        bytes_to_decode = 0;
    }

    /* make one big value out of the partial values */
    triple_value = char_value[0];
    triple_value *= 64;
    triple_value += char_value[1];
    triple_value *= 64;
    triple_value += char_value[2];
    triple_value *= 64;
    triple_value += char_value[3];

    /* break the big value into bytes */
    for (ii = bytes_to_decode; ii < 3; ii++)
    {
        triple_value /= 256;
    }
    for (ii = bytes_to_decode - 1; ii >= 0; ii--)
    {
        result[ii] = triple_value % 256;
        triple_value /= 256;
    }

    return bytes_to_decode;
}

/**
 * decode base64 encoded data
 *
 * @param source the encoded data (zero terminated)
 * @param target pointer to the target buffer
 * @param targetlen length of the target buffer
 * @return length of converted data on success, -1 otherwise
 */
size_t base64_decode(const char * source, unsigned char * target, size_t targetlen)
{
    char * src;
    const char * tmpptr;
    char quadruple[4];
    char tmpresult[3];
    int ii;
    size_t tmplen = 3;
    size_t converted = 0;

    if (!source || !target)
    {
        return (size_t) -1;
    }

    /* concatinate '====' to the source to handle unpadded base64 data */
    src =
#ifdef __cplusplus
    (char *)
#endif
    malloc(strlen(source) + 5);

    if (src == NULL)
    {
        return (size_t) -1;
    }
    strcpy(src, source);
    strcat(src, "====");
    tmpptr = src;

    /* convert as long as we get a full result */
    while (tmplen == 3)
    {
        /* get 4 characters to convert */
        for (ii = 0; ii < 4; ii++)
        {
            /* skip invalid characters - we won't reach the end */
            while (*tmpptr != '=' && _base64_char_value(*tmpptr) < 0)
            {
                tmpptr++;
            }

            quadruple[ii] = *tmpptr++;
        }

        /* convert the characters */
        tmplen = _base64_decode_triple(quadruple, (unsigned char *) tmpresult);

        /* check if the fit in the result buffer */
        if (targetlen < tmplen)
        {
            free(src);
            return (size_t) -1;
        }

        /* put the partial result in the result buffer */
        memcpy(target, tmpresult, tmplen);
        target += tmplen;
        targetlen -= tmplen;
        converted += tmplen;
    }

    free(src);
    return converted;
}
