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

/* c_list.c
 *
 * This is a very simple implementation of a list, using a doubly
 * linked list.  Each element is allocated from a block memory pool to
 * ensure there is no heap fragmentation.  However, this means that
 * care must be taken to ensure the correct size pool is initialized.
 * If it is too small there will be run-time out of memory errors.  If
 * it is too large that memory can not be used by any other part of the
 * software as it is reserved in the memory pool.
 *
 * Compile time options:
 * CLIST_USE_THREADX: use threadX memory pool instead of new/delete
 * CLIST_USE_FILEPARSER: use fileparser (this adds the load method)
 * CLIST_USE_LOGGER: use logging functions
 *
 * Compression
 *
 * Compression is achieved by replacing a pre-defined catalog of
 * strings to compress with a single unused (in the original list)
 * character.  A dictionary is created that maps found catalog strings
 * with a unique unused character.  Once the entire stringified list
 * has been compressed the dictionary that was used for the compression
 * is appended to the end of the string.  Including the dictionary
 * allows for self contained uncompressing.  That is the uncompressor
 * does not need any additional information to uncompress the
 * stringified list.
 *
 * Potential future compression
 *
 * The existing compression only uses characters that are unused in the
 * original clist strings.  It's possible that a list uses a large
 * amount of the usable characters thus limiting the available
 * characters to use for compression.  The following describes another
 * compression scheme that may fix this problem.
 *
 * We may still be able to use a character for compression that is used
 * in the original cList if the act of compression removes all
 * occurrences of that character.  This approach requires us to keep
 * track of the number of times each usable character occurs in the
 * original cList strings.  Then determine the number of times each
 * string to compress from the catalog occurs and adjust the character
 * counts assuming that the strings to compress will be removed.
 * This will potentially free characters that can be used for
 * compression that were previously unavailable.
 *
 * If it assumed that all found strings to compress will be removed its
 * possible that we still do not have enough characters remaining and
 * the compression will fail.  This is an all or nothing approach that
 * would be fairly easy to implement.  An iterative approach could be
 * used to remove a single string to compress at a time and see if
 * there are any characters that can be used.  Further compression
 * would not be possible if all strings to compress are attempted but
 * none of them resule in a free character.  This allows for partial
 * compression.
 *
 */

#include "c_list.h"
#include <stdio.h>
#ifdef CLIST_USE_THREADX
#include "bsp/os/red/red_block_pool.h"
#endif
#ifdef CLIST_USE_LOGGER
#include "utils/diagnostic/log/log.h"
#endif
#include "stringl/stringl.h"
#include "decorated_string/decorated_string.h"
#include "rcp_parser/rcp_parser2.h"
#include <string.h>
#include <ctype.h>

#if defined(_MSC_VER) && (_MSC_VER < 1900)
#define snprintf _snprintf
#endif

#define ESCAPE_CHAR '%'
#define SEPARATOR_CHAR '|'
#define COMPRESSED_STRING_DICTIONARY_START_CHAR '{'
#define COMPRESSED_STRING_DICTIONARY_END_CHAR '}'
#define COMPRESSED_STRING_DICTIONARY_SEPARATOR_CHAR ':'

/* Note - Make sure the table below is from longest to shortest to
 * gurantee unique matches */
const char * c_list_strings_to_compress[] =
{
    "&redformatk; ",
    "&redformatk;",
    "&redkelvin;",
    "&redana125;",
    "&redana165;",
    "&red1over;",
    "&redana13;",
    "&redana15;",
    "&redana18;",
    "&redcheck;",
    "&redfover;",
    "&redana2;",
    "&redsub2;",
    "&redfps;",
    "&rediso;",
    "&redsec;",
    "&redae;",
    "&redav;",
    "&redll;",
    "&trade;",
    "&copy;",
    "&amp;",
    "&deg;",
    "&reg;",
    "2%:1",
    "3%:2",
    "6%:5",
    "4%:1",
    "8%:1",
    "5%:4",
    "4%:3",
    "5.5",
    "4.5",
    "3.5",
    "2.5",
    "HD",
    "WS"
};

size_t c_list_strings_to_compress_size(void)
{
    return sizeof(c_list_strings_to_compress) / sizeof(c_list_strings_to_compress[0]);
}

#define MAX_COMPRESSED_STRINGS 25
#define MAX_UNCOMPRESSED_STRING_LENGTH 20
/* {uncompressed0:compressed0|uncompressed1:compressed1|...} */
#define MAX_DICTIONARY_STRING_LENGTH (1 + (MAX_UNCOMPRESSED_STRING_LENGTH + 1 + 1 + 1) * MAX_COMPRESSED_STRINGS + 1 + 1)

typedef struct {
    char uncompressed_string[MAX_UNCOMPRESSED_STRING_LENGTH];
    char compressed_character;
} c_list_compressed_string_dictionary_entry_t;

#define FIRST_USABLE_CHARACER_FOR_COMPRESSION '0'
#define LAST_USABLE_CHARACTER_FOR_COMPRESSION 'z'

static c_list_entry_t * _c_list_get_entry_by_index(const c_list_t * c_list, size_t idx);
static int _c_list_convert_string_token(char * pBuffer, const char * pToken, size_t maxLength, const char * characters_to_compress);
static const char _c_list_get_compressed_character(c_list_compressed_string_dictionary_entry_t * compressed_string_dictionary, size_t * compressed_character_count, const char * src, size_t src_size, const unsigned char * used_characters, char * current_unused_character);
static char _c_list_create_compressed_character(const unsigned char * used_characters, char * current_unused_character);
static void _c_list_compress_string_token(c_list_compressed_string_dictionary_entry_t * compressed_string_dictionary, size_t * compressed_string_count, char * pBuffer, const char * pToken, size_t maxLength, const unsigned char * used_characters, char * current_unused_character);
static size_t _c_list_get_token(const char * pBuffer, char * pToken, size_t maxLength, int remove_escape_characters);
static size_t _c_list_remove_escape_characters(const char * src, char * dest, size_t dest_length);
static c_list_error_t _c_list_import_normal_string(c_list_t * c_list, const char * str, int compress);
static c_list_error_t _c_list_import_min_max_step_string(c_list_t * c_list, const char * str);
static c_list_error_t _c_list_export_min_max_step_string(const c_list_t * c_list, char * str, size_t size);
static void _c_list_parse_compressed_string_dictionary(c_list_compressed_string_dictionary_entry_t * compressed_string_dictionary, size_t * compressed_string_count, const char * str_dictionary);
static void _c_list_get_used_characters(const c_list_t * c_list, unsigned char * used_characters);
static c_list_error_t _c_list_export_normal_string(const c_list_t * c_list, char * str, size_t size, int compress);
static c_list_entry_t * _c_list_entry_create(const c_list_t * c_list, c_list_num_t num, const char * str, user_defined_t user_defined);
static c_list_error_t _c_list_entry_delete(const c_list_t * c_list, c_list_entry_t * entry);
static c_list_error_t _c_list_entry_copy(c_list_entry_t * dest, const c_list_entry_t * src);

c_list_t * c_list_create(c_list_malloc_t c_list_malloc, c_list_free_t c_list_free)
{
    c_list_t * const c_list = (c_list_t *) c_list_malloc(sizeof(c_list_t));

    if (c_list)
    {
        c_list->head = NULL;
        c_list->tail = NULL;
        c_list->cur = NULL;
        c_list->step_options.valid = 0;
        c_list->malloc = c_list_malloc;
        c_list->free = c_list_free;
    }
    return c_list;
}

c_list_error_t c_list_delete(c_list_t * c_list)
{
    if (!c_list)
    {
        return C_LIST_PARAM_ERROR;
    }
    else
    {
        const c_list_error_t err = c_list_clear(c_list);
        c_list->free(c_list);
        return err;
    }
}

static c_list_entry_t * _c_list_entry_create(const c_list_t * c_list, c_list_num_t num, const char * str, user_defined_t user_defined)
{
#ifndef CLIST_USE_THREADX
    c_list_entry_t * c_list_entry = NULL;

    if (!c_list)
    {
        return NULL;
    }
    c_list_entry = (c_list_entry_t *) c_list->malloc(sizeof(c_list_entry_t));

#else
    c_list_entry_t * c_list_entry = red_block_pool_allocate(&_red_block_pool_c_list_entry_t);

    if (!c_list_entry)
    {
#ifdef CLIST_USE_LOGGER
        log_error("cList block pool full\n");
#endif
        return NULL;
    }
#endif

    c_list_entry->num = num;
    strlcpy(c_list_entry->str, str, sizeof(c_list_entry->str));
    c_list_entry->user_defined = user_defined;
    c_list_entry->next = NULL;
    c_list_entry->prev = NULL;
    return c_list_entry;
}

static c_list_error_t _c_list_entry_delete(const c_list_t * c_list, c_list_entry_t * entry)
{
#ifndef CLIST_USE_THREADX
    if (!c_list || !entry)
    {
        return C_LIST_PARAM_ERROR;
    }
    else
    {
        c_list->free(entry);
        return C_LIST_SUCCESS;
    }
#else
    red_block_pool_release((void *) entry);

    return C_LIST_SUCCESS;
#endif
}

c_list_error_t c_list_copy(c_list_t * dest, const c_list_t * src)
{
    c_list_error_t err;
    const c_list_entry_t * cur = NULL;

    if (!dest || !src)
    {
        return C_LIST_PARAM_ERROR;
    }

    err = c_list_clear(dest);
    if (C_LIST_SUCCESS != err)
    {
        return err;
    }

    cur = src->head;
    while (cur)
    {
        c_list_entry_t entry;
        err = _c_list_entry_copy(&entry, cur);
        if (C_LIST_SUCCESS != err)
        {
            return err;
        }
        err = c_list_append(dest, entry.num, entry.str, entry.user_defined);
        if (C_LIST_SUCCESS != err)
        {
            return err;
        }

        if (cur == src->cur)
        {
            dest->cur = dest->tail;
        }

        cur = cur->next;
    }

    dest->step_options = src->step_options;
    return C_LIST_SUCCESS;
}

c_list_error_t c_list_append(c_list_t * c_list, c_list_num_t num, const char * str, user_defined_t user_defined)
{
    c_list_entry_t * c_list_entry = NULL;
    if (!c_list)
    {
        return C_LIST_PARAM_ERROR;
    }

    c_list->step_options.valid = 0;

    c_list_entry = _c_list_entry_create(c_list, num, str, user_defined);
    if (!c_list_entry)
    {
        return C_LIST_MEM_ERROR;
    }

    if (!c_list->head)
    {
        c_list->head = c_list_entry;
        c_list->tail = c_list_entry;
        c_list->cur = c_list_entry;
    }
    else
    {
        c_list->tail->next = c_list_entry;
        c_list_entry->prev = c_list->tail;
        c_list->tail = c_list_entry;
    }

    return C_LIST_SUCCESS;
}

c_list_error_t c_list_insert(c_list_t * c_list, size_t idx, c_list_num_t num, const char * str, user_defined_t user_defined)
{
    c_list_entry_t * cur = NULL;
    c_list_entry_t * c_list_entry = NULL;

    if (!c_list)
    {
        return C_LIST_PARAM_ERROR;
    }

    c_list->step_options.valid = 0;

    cur = _c_list_get_entry_by_index(c_list, idx);
    if (!cur)
    {
        if (idx == c_list_get_length(c_list))
        {
            return c_list_append(c_list, num, str, user_defined);
        }
        else
        {
            return C_LIST_INDEX_ERROR;
        }
    }

    c_list_entry = _c_list_entry_create(c_list, num, str, user_defined);

    if (cur == c_list->head)
    {
        c_list_entry->next = c_list->head;
        c_list->head->prev = c_list_entry;
        c_list->head = c_list_entry;
    }
    else
    {
        c_list_entry->next = cur;
        c_list_entry->prev = cur->prev;
        if (cur->prev)
        {
            cur->prev->next = c_list_entry;
        }
        cur->prev = c_list_entry;
    }
    return C_LIST_SUCCESS;
}

c_list_error_t c_list_insert_in_sort_order(c_list_t * c_list, c_list_num_t num, const char * str, user_defined_t user_defined)
{
    c_list_entry_t * cur = NULL;
    c_list_entry_t * c_list_entry = NULL;

    if (!c_list)
    {
        return C_LIST_PARAM_ERROR;
    }

    c_list->step_options.valid = 0;

    /* If the list is empty add this item at the beginning */
    if (!c_list->head)
    {
        return c_list_append(c_list, num, str, user_defined);
    }

    cur = c_list->head;
    c_list_entry = _c_list_entry_create(c_list, num, str, user_defined);
    c_list->cur = c_list_entry;

    /* We will linearly search through the entries until we find the
     * location where this new entry belongs.  Note that if an entry
     * with the same value exists this entry will be inserted before it
     * in the list.*/
    while (cur)
    {
        if (num > cur->num)
        {
            cur = cur->next;
        }
        else
        {
            if (cur == c_list->head)
            {
                c_list_entry->next = c_list->head;
                c_list->head->prev = c_list_entry;
                c_list->head = c_list_entry;
            }
            else
            {
                c_list_entry->next = cur;
                c_list_entry->prev = cur->prev;
                if (cur->prev)
                {
                    cur->prev->next = c_list_entry;
                }
                cur->prev = c_list_entry;
            }
            break;
        }
    }

    if (!cur)
    {
        if (C_LIST_SUCCESS != _c_list_entry_delete(c_list, c_list_entry))
        {
            return C_LIST_MEM_ERROR;
        }
        return c_list_append(c_list, num, str, user_defined);
    }

    return C_LIST_SUCCESS;
}

c_list_error_t c_list_insert_in_sort_order_unique(c_list_t * c_list, c_list_num_t num, const char * str, user_defined_t user_defined)
{
    c_list_entry_t list_entry;

    if (!c_list)
    {
        return C_LIST_PARAM_ERROR;
    }

    /* If the list is empty add this item at the beginning */
    if (c_list->head == NULL)
    {
        return c_list_append(c_list, num, str, user_defined);
    }

    if (C_LIST_SUCCESS == c_list_find_num(c_list, num, &list_entry, C_LIST_FIND_EXACT))
    {
        /* we will replace the current version */

        list_entry.num = num;
        strlcpy(list_entry.str, str, sizeof(list_entry.str));
        list_entry.user_defined = user_defined;
        return c_list_set_current_entry(c_list, &list_entry);
    }

    return c_list_insert_in_sort_order(c_list, num, str, user_defined);
}

c_list_error_t c_list_insert_in_alphabetical_order(c_list_t * c_list, c_list_num_t num, const char * str, user_defined_t user_defined)
{
    c_list_entry_t * cur = NULL;
    c_list_entry_t * c_list_entry = NULL;

    if (!c_list)
    {
        return C_LIST_PARAM_ERROR;
    }

    c_list->step_options.valid = 0;

    if (c_list->head == NULL)
    {
        return c_list_append(c_list, num, str, user_defined);
    }

    cur = c_list->head;
    c_list_entry = _c_list_entry_create(c_list, num, str, user_defined);
    c_list->cur = c_list_entry;

    /* We will linearly search through the entries until we find the
     * location where this new entry belongs.  Note that if an entry
     * with the same value exists this entry will be inserted before it
     * in the list.*/
    while (cur)
    {
        if (strcmp(str, cur->str) > 0)
        {
            cur = cur->next;
        }
        else
        {
            if (cur == c_list->head)
            {
                c_list_entry->next = c_list->head;
                c_list->head->prev = c_list_entry;
                c_list->head = c_list_entry;
            }
            else
            {
                c_list_entry->next = cur;
                c_list_entry->prev = cur->prev;
                if (cur->prev)
                {
                    cur->prev->next = c_list_entry;
                }
                cur->prev = c_list_entry;
            }
            break;
        }
    }

    if (NULL == cur)
    {
        if (C_LIST_SUCCESS != _c_list_entry_delete(c_list, c_list_entry))
        {
            return C_LIST_MEM_ERROR;
        }
        return c_list_append(c_list, num, str, user_defined);
    }

    return C_LIST_SUCCESS;
}

c_list_error_t c_list_remove(c_list_t * c_list, size_t idx)
{
    int removing_currently_selected_entry = 0;
    c_list_entry_t * cur = NULL;

    if (!c_list)
    {
        return C_LIST_PARAM_ERROR;
    }

    c_list->step_options.valid = 0;

    cur = _c_list_get_entry_by_index(c_list, idx);

    if (!cur)
    {
        return C_LIST_INDEX_ERROR;
    }

    if (cur == c_list->cur)
    {
        removing_currently_selected_entry = 1;
    }

    if (idx == 0)
    {
        c_list->head = cur->next;
        if (C_LIST_SUCCESS != _c_list_entry_delete(c_list, cur))
        {
            return C_LIST_MEM_ERROR;
        }
        if (c_list->head)
        {
            c_list->head->prev = NULL;
        }
    }
    else
    {
        cur->prev->next = cur->next;
        if (cur->next)
        {
            cur->next->prev = cur->prev;
        }
        else
        {
            c_list->tail = cur->prev;
        }
        if (C_LIST_SUCCESS != _c_list_entry_delete(c_list, cur))
        {
            return C_LIST_MEM_ERROR;
        }
    }

    if (removing_currently_selected_entry)
    {
        c_list->cur = NULL;
    }

    return C_LIST_SUCCESS;
}

c_list_error_t c_list_clear(c_list_t * c_list)
{
    if (!c_list)
    {
        return C_LIST_PARAM_ERROR;
    }

    c_list->step_options.valid = 0;

    c_list->cur = c_list->head;

    while (c_list->cur)
    {
        c_list_entry_t * const next = c_list->cur->next;
        if (C_LIST_SUCCESS != _c_list_entry_delete(c_list, c_list->cur))
        {
            return C_LIST_MEM_ERROR;
        }

        c_list->cur = next;
    }

    c_list->head = NULL;
    c_list->cur = NULL;
    c_list->tail = NULL;

    return C_LIST_SUCCESS;
}

c_list_error_t c_list_find_first(c_list_t * c_list, c_list_entry_t * c_list_entry)
{
    c_list_error_t err;

    if (!c_list || !c_list_entry)
    {
        return C_LIST_PARAM_ERROR;
    }

    if (!c_list->head)
    {
        return C_LIST_FIND_ERROR;
    }
    c_list->cur = c_list->head;
    err = _c_list_entry_copy(c_list_entry, c_list->cur);
    if (C_LIST_SUCCESS != err)
    {
        return err;
    }
    c_list_entry->next = NULL;
    c_list_entry->prev = NULL;
    return C_LIST_SUCCESS;
}

c_list_error_t c_list_find_last(c_list_t * c_list, c_list_entry_t * c_list_entry)
{
    c_list_error_t err;

    if (!c_list || !c_list_entry)
    {
        return C_LIST_PARAM_ERROR;
    }

    if (NULL == c_list->tail)
    {
        return C_LIST_FIND_ERROR;
    }

    c_list->cur = c_list->tail;
    err = _c_list_entry_copy(c_list_entry, c_list->cur);
    if (err != C_LIST_SUCCESS)
    {
        return err;
    }
    c_list_entry->next = NULL;
    c_list_entry->prev = NULL;
    return C_LIST_SUCCESS;
}

c_list_error_t c_list_find_next(c_list_t * c_list, c_list_entry_t * c_list_entry)
{
    c_list_error_t err;

    if (!c_list || !c_list_entry)
    {
        return C_LIST_PARAM_ERROR;
    }

    if (NULL == c_list->cur)
    {
        return C_LIST_FIND_ERROR;
    }

    if (NULL == c_list->cur->next)
    {
        return C_LIST_FIND_ERROR;
    }
    c_list->cur = c_list->cur->next;
    err = _c_list_entry_copy(c_list_entry, c_list->cur);
    if (err != C_LIST_SUCCESS)
    {
        return err;
    }
    c_list_entry->next = NULL;
    c_list_entry->prev = NULL;
    return C_LIST_SUCCESS;
}

c_list_error_t c_list_find_prev(c_list_t * c_list, c_list_entry_t * c_list_entry)
{
    c_list_error_t err;

    if (!c_list || !c_list_entry)
    {
        return C_LIST_PARAM_ERROR;
    }

    if (NULL == c_list->cur)
    {
        return C_LIST_FIND_ERROR;
    }

    if (NULL == c_list->cur->prev)
    {
        return C_LIST_FIND_ERROR;
    }
    c_list->cur = c_list->cur->prev;
    err = _c_list_entry_copy(c_list_entry, c_list->cur);
    if (err != C_LIST_SUCCESS)
    {
        return err;
    }
    c_list_entry->next = NULL;
    c_list_entry->prev = NULL;
    return C_LIST_SUCCESS;
}

c_list_error_t c_list_get_entry(const c_list_t * c_list, size_t idx, c_list_entry_t * c_list_entry)
{
    const c_list_entry_t * c_list_found_entry = NULL;
    c_list_error_t err;

    if (!c_list || !c_list_entry)
    {
        return C_LIST_PARAM_ERROR;
    }

    c_list_found_entry = _c_list_get_entry_by_index(c_list, idx);
    if (!c_list_found_entry)
    {
        return C_LIST_INDEX_ERROR;
    }

    err = _c_list_entry_copy(c_list_entry, c_list_found_entry);
    if (err != C_LIST_SUCCESS)
    {
        return err;
    }
    c_list_entry->next = NULL;
    c_list_entry->prev = NULL;
    return C_LIST_SUCCESS;
}

c_list_error_t c_list_set_entry(c_list_t * c_list, size_t idx, const c_list_entry_t * c_list_entry)
{
    c_list_entry_t * c_list_found_entry = NULL;

    if (!c_list || !c_list_entry)
    {
        return C_LIST_PARAM_ERROR;
    }

    c_list->step_options.valid = 0;

    c_list_found_entry = _c_list_get_entry_by_index(c_list, idx);
    if (!c_list_found_entry)
    {
        return C_LIST_INDEX_ERROR;
    }
    return _c_list_entry_copy(c_list_found_entry, c_list_entry);
}

c_list_error_t c_list_get_current_entry(const c_list_t * c_list, c_list_entry_t * c_list_entry)
{
    c_list_error_t err;
    if (!c_list || !c_list_entry)
    {
        return C_LIST_PARAM_ERROR;
    }

    if (!c_list->cur)
    {
        return C_LIST_INDEX_ERROR;
    }

    err = _c_list_entry_copy(c_list_entry, c_list->cur);
    if (err != C_LIST_SUCCESS)
    {
        return err;
    }
    c_list_entry->next = NULL;
    c_list_entry->prev = NULL;
    return C_LIST_SUCCESS;
}

c_list_error_t c_list_set_current_entry(c_list_t * c_list, const c_list_entry_t * c_list_entry)
{
    if (!c_list || !c_list_entry)
    {
        return C_LIST_PARAM_ERROR;
    }

    c_list->step_options.valid = 0;

    if (!c_list->cur)
    {
        return C_LIST_INDEX_ERROR;
    }
    return _c_list_entry_copy(c_list->cur, c_list_entry);
}

c_list_error_t c_list_get_index(const c_list_t * c_list, size_t * idx)
{
    const c_list_entry_t * cur = NULL;
    if (!c_list || !idx)
    {
        return C_LIST_PARAM_ERROR;
    }

    cur = c_list->head;
    *idx = 0;

    while (cur)
    {
        if (cur == c_list->cur)
        {
            break;
        }
        cur = cur->next;
        (*idx)++;
    }

    if (!cur)
    {
        return C_LIST_INDEX_ERROR;
    }

    return C_LIST_SUCCESS;
}

c_list_error_t c_list_set_index(c_list_t * c_list, size_t idx)
{
    if (!c_list)
    {
        return C_LIST_PARAM_ERROR;
    }

    c_list->cur = c_list->head;
    while (idx--)
    {
        if (c_list->cur)
        {
            c_list->cur = c_list->cur->next;
        }
        else
        {
            break;
        }
    }

    if (!c_list->cur)
    {
        return C_LIST_INDEX_ERROR;
    }

    return C_LIST_SUCCESS;
}

c_list_error_t c_list_find_num(c_list_t * c_list, c_list_num_t num, c_list_entry_t * c_list_entry, c_list_find_t find)
{
    c_list_entry_t * smaller = NULL;
    c_list_num_t smallerDelta = 0;
    c_list_entry_t * bigger = NULL;
    c_list_num_t biggerDelta = 0;
    c_list_error_t err;

    if (!c_list)
    {
        return C_LIST_PARAM_ERROR;
    }

    c_list->cur = c_list->head;

    while (c_list->cur)
    {
        const c_list_num_t delta = c_list->cur->num - num;

        if (delta == 0)
        {
            /* we found an exact match, return now */
            err = _c_list_entry_copy(c_list_entry, c_list->cur);
            if (err != C_LIST_SUCCESS)
            {
                return err;
            }
            c_list_entry->next = NULL;
            c_list_entry->prev = NULL;
            return C_LIST_SUCCESS;
        }
        else if (delta > 0)
        {
            if (!bigger || delta < biggerDelta)
            {
                bigger = c_list->cur;
                biggerDelta = delta;
            }
        }
        else if (delta < 0)
        {
            if (!smaller || -delta < smallerDelta)
            {
                smaller = c_list->cur;
                smallerDelta = -delta;
            }
        }

        c_list->cur = c_list->cur->next;
    }

    /* we did not find an exact match */
    switch (find)
    {
        case C_LIST_FIND_EXACT:
            return C_LIST_FIND_ERROR;

        case C_LIST_FIND_NEXT_BIGGER:
            if (!bigger)
            {
                return C_LIST_FIND_ERROR;
            }

            c_list->cur = bigger;
            break;

        case C_LIST_FIND_NEXT_SMALLER:
            if (!smaller)
            {
                return C_LIST_FIND_ERROR;
            }

            c_list->cur = smaller;
            break;

        case C_LIST_FIND_CLOSEST:
            if (smaller)
            {
                if (bigger)
                {
                    /* we found an item both bigger than and smaller
                     * than what we were searching for */
                    if (biggerDelta < smallerDelta)
                    {
                        c_list->cur = bigger;
                    }
                    else
                    {
                        c_list->cur = smaller;
                    }
                }
                else
                {
                    /* we only found an item smaller than we were
                     * searching for */
                    c_list->cur = smaller;
                }
            }
            else if (bigger)
            {
                /* we only found an item bigger than we were searching
                 * for */
                c_list->cur = bigger;
            }
            else
            {
                /* we didn't find any items either bigger or smaller
                 * than we were searching for */
                return C_LIST_FIND_ERROR;
            }
            break;
    }

    if (c_list->cur)
    {
        err = _c_list_entry_copy(c_list_entry, c_list->cur);
        if (err != C_LIST_SUCCESS)
        {
            return err;
        }
        c_list_entry->next = NULL;
        c_list_entry->prev = NULL;
        return C_LIST_SUCCESS;
    }

    return C_LIST_FIND_ERROR;
}

c_list_error_t c_list_find_str(c_list_t * c_list, const char * str, c_list_entry_t * c_list_entry)
{
    c_list_error_t err;

    if (!c_list || !str)
    {
        return C_LIST_PARAM_ERROR;
    }

    c_list->cur = c_list->head;
    while (c_list->cur)
    {
        if (strcmp(c_list->cur->str, str) == 0)
        {
            err = _c_list_entry_copy(c_list_entry, c_list->cur);
            if (err != C_LIST_SUCCESS)
            {
                return err;
            }
            c_list_entry->next = NULL;
            c_list_entry->prev = NULL;
            return C_LIST_SUCCESS;
        }
        c_list->cur = c_list->cur->next;
    }
    return C_LIST_FIND_ERROR;
}

c_list_error_t c_list_find_strn(c_list_t * c_list, const char * str, size_t len, c_list_entry_t * c_list_entry)
{
    c_list_error_t err;

    if (!c_list || !str)
    {
        return C_LIST_PARAM_ERROR;
    }

    if (len > C_LIST_MAX_STRING_LEN)
    {
        return C_LIST_FIND_ERROR;
    }

    c_list->cur = c_list->head;
    while (c_list->cur)
    {
        if (strncmp(c_list->cur->str, str, len) == 0)
        {
            err = _c_list_entry_copy(c_list_entry, c_list->cur);
            if (err != C_LIST_SUCCESS)
            {
                return err;
            }
            c_list_entry->next = NULL;
            c_list_entry->prev = NULL;
            return C_LIST_SUCCESS;
        }
        c_list->cur = c_list->cur->next;
    }
    return C_LIST_FIND_ERROR;
}

c_list_error_t c_list_fill_generic(c_list_t * c_list, int min, int max, int step, int divider, int precision, const char * postfix, const char * prefix)
{
    c_list_error_t err;
    int ii;

    if (!c_list)
    {
        return C_LIST_PARAM_ERROR;
    }

    err = c_list_clear(c_list);
    if (C_LIST_SUCCESS != err)
    {
        return err;
    }

    if (divider < 1)
    {
        return C_LIST_PARAM_ERROR;
    }

    if (precision < 0)
    {
        precision = 0;
    }

    /* Save params used to create list.  We can use these later to
     * create a compressed string to represent the list. */
    c_list->step_options.min = min;
    c_list->step_options.max = max;
    c_list->step_options.step = step;
    c_list->step_options.divider = divider;
    c_list->step_options.precision = precision;
    if (prefix)
    {
        strlcpy(c_list->step_options.prefix, prefix, sizeof(c_list->step_options.prefix));
    }
    else
    {
        strlcpy(c_list->step_options.prefix, "", sizeof(c_list->step_options.prefix));
    }
    if (postfix)
    {
        strlcpy(c_list->step_options.postfix, postfix, sizeof(c_list->step_options.postfix));
    }
    else
    {
        strlcpy(c_list->step_options.postfix, "", sizeof(c_list->step_options.postfix));
    }

    for (ii = min; ii <= max; ii += step)
    {
        char num[32];
        c_list_entry_t listEntry;
        listEntry.num = ii;
        listEntry.str[0] = 0;
        listEntry.user_defined.ptr = NULL;
        listEntry.user_defined.int32 = 0;

        if (prefix)
        {
            strlcat(listEntry.str, prefix, sizeof(listEntry.str));
        }

        snprintf(num, sizeof(num), "%.*f", precision, (float) ii / divider);
        strlcat(listEntry.str, num, sizeof(listEntry.str));

        if (postfix)
        {
            strlcat(listEntry.str, postfix, sizeof(listEntry.str));
        }

        err = c_list_append(c_list, listEntry.num, listEntry.str, listEntry.user_defined);
        if (C_LIST_SUCCESS != err)
        {
            return err;
        }
    }

    /* we do this at the end since calls to append above will clear
     * this flag */
    c_list->step_options.valid = 1;

    return C_LIST_SUCCESS;
}

static c_list_entry_t * _c_list_get_entry_by_index(const c_list_t * c_list, size_t idx)
{
    c_list_entry_t * cur = NULL;
    if (!c_list)
    {
        return NULL;
    }

    cur = c_list->head;

    while (idx--)
    {
        if (cur)
        {
            cur = cur->next;
        }
        else
        {
            break;
        }
    }

    return cur;
}

static c_list_error_t _c_list_entry_copy(c_list_entry_t * dest, const c_list_entry_t * src)
{
    if (!dest || !src)
    {
        return C_LIST_PARAM_ERROR;
    }

    dest->num = src->num;
    strlcpy(dest->str, src->str, sizeof(dest->str));
    dest->user_defined = src->user_defined;

    return C_LIST_SUCCESS;
}

size_t c_list_get_length(const c_list_t * c_list)
{
    size_t len = 0;
    const c_list_entry_t * cur = NULL;

    if (!c_list)
    {
        return 0;
    }

    cur = c_list->head;
    while (cur)
    {
        cur = cur->next;
        len++;
    }
    return len;
}

c_list_error_t c_list_decode(const c_list_t * c_list)
{
    c_list_entry_t * cur = NULL;
    char decoded_str[C_LIST_MAX_STRING_LEN];

    if (!c_list)
    {
        return C_LIST_PARAM_ERROR;
    }

    cur = c_list->head;
    while (cur)
    {
        decorated_string_decode(cur->str, decoded_str, sizeof(decoded_str));
        strlcpy(cur->str, decoded_str, sizeof(cur->str));
        cur = cur->next;
    }

    return C_LIST_SUCCESS;
}

/* Import a list based on strings (From an RCP Command for Example)
 * String lists are used to transmit lists through the RCP protocol.
 * To pass them as RCP data the lists is put into one long parameter
 * The separator for fields inside the lists is '|'
 * The first field in a list has to be a number for the currently active
 * element in the list. After that the list number / list string pairs
 * are following. Pointers in lists are ignored.
 */

static int _c_list_convert_string_token(char * pBuffer, const char * pToken, size_t maxLength, const char * characters_to_escape)
{
    size_t ii = 0;

    if (!characters_to_escape)
    {
#ifdef CLIST_USE_LOGGER
        log_error("NULL characters to escape\n");
#endif
        return 0;
    }

    /* Loop through token we are adding */
    while (*pToken != '\0')
    {
        /* Deal with Escape Characters */
        size_t jj = 0;
        while (characters_to_escape[jj] != '\0')
        {
            if (*pToken == characters_to_escape[jj])
            {
                pBuffer[ii++] = ESCAPE_CHAR;
                break;
            }

            jj++;
        }

        /* Add character */
        pBuffer[ii++] = *pToken++;

        if (ii >= maxLength)
        {
            return 0;
        }
    }

    pBuffer[ii++] = 0;

    return 1;
}

const char _c_list_get_compressed_character(c_list_compressed_string_dictionary_entry_t * compressed_string_dictionary, size_t * compressed_string_count, const char * src, size_t src_size, const unsigned char * used_characters, char * current_unused_character)
{
    size_t ii = 0;
    size_t idx;
    char tmp;

    if (!src)
    {
#ifdef CLIST_USE_LOGGER
        log_error("NULL source\n");
#endif
        return 0;
    }

    for (ii = 0; ii < *compressed_string_count; ii++)
    {
        if (strcmp(compressed_string_dictionary[ii].uncompressed_string, src) == 0)
        {
            return compressed_string_dictionary[ii].compressed_character;
        }
    }

    if (*compressed_string_count == MAX_COMPRESSED_STRINGS)
    {
        return 0;
    }

    /* Add string to the compressed dictionary */
    idx = *compressed_string_count;

    if (src_size > sizeof(compressed_string_dictionary[idx].uncompressed_string))
    {
#ifdef CLIST_USE_LOGGER
        log_error("Uncompressed string too large to store: %s\n", src);
#endif
        return 0;
    }

    tmp = _c_list_create_compressed_character(used_characters, current_unused_character);
    if (tmp)
    {
        (*compressed_string_count)++;
        strncpy(compressed_string_dictionary[idx].uncompressed_string, src, src_size);

        compressed_string_dictionary[idx].compressed_character = tmp;
    }

    return tmp;
}

char _c_list_create_compressed_character(const unsigned char * used_characters, char * current_unused_character)
{
    /* Find next available unused character */
    char compressed_character = 0;
    while (*current_unused_character <= LAST_USABLE_CHARACTER_FOR_COMPRESSION)
    {
        /* Since the dictionary separator character falls in the range
         * of compressed characters that can be used it must be skipped
         * */
        if (*current_unused_character != COMPRESSED_STRING_DICTIONARY_SEPARATOR_CHAR)
        {
            if (!used_characters[*current_unused_character - FIRST_USABLE_CHARACER_FOR_COMPRESSION])
            {
                compressed_character = *current_unused_character;
                (*current_unused_character)++;
                break;
            }
        }
        (*current_unused_character)++;
    }
    return compressed_character;
}

/* For lists that are too long attempt to compress the string by
 * generating a dictionary that will be passed with the stringified
 * list */
static void _c_list_compress_string_token(c_list_compressed_string_dictionary_entry_t * compressed_string_dictionary, size_t * compressed_string_count, char * dest, const char * src, size_t dest_size, const unsigned char * used_characters, char * current_unused_character)
{
    if (dest_size == 0)
    {
        return;
    }

    while (dest_size != 1 && *src)
    {
        int string_to_compress_found = 0;
        size_t ii = 0;

        if (*src == ESCAPE_CHAR)
        {
            *dest++ = *src++;
            dest_size--;
        }
        else
        {
            for (ii = 0; ii < sizeof(c_list_strings_to_compress) / sizeof(c_list_strings_to_compress[0]); ii++)
            {
                const size_t length = strlen(c_list_strings_to_compress[ii]);
                if (strncmp(src, c_list_strings_to_compress[ii], length) == 0)
                {
                    /* Find string in dictionary or add it */
                    const char compressed_character = _c_list_get_compressed_character(compressed_string_dictionary, compressed_string_count, c_list_strings_to_compress[ii], length, used_characters, current_unused_character);
                    if (compressed_character)
                    {
                        *dest++ = compressed_character;
                        src += length;
                        dest_size--;
                        string_to_compress_found = 1;
                        break;
                    }
                }
            }
        }

        if (!string_to_compress_found)
        {
            *dest++ = *src++;
            dest_size--;
        }
    }

    *dest = '\0';
}

static size_t _c_list_get_token(const char * pBuffer, char * pToken, size_t maxLength, int remove_escape_characters)
{
    size_t count = 0;

    /* Check for end of list */
    if (*pBuffer == '\0')
    {
        return 0;
    }

    /* Loop through token until we hit a separator */
    while (*pBuffer != SEPARATOR_CHAR && *pBuffer != 0)
    {
        /* Deal with Escape characters */
        if (*pBuffer == ESCAPE_CHAR)
        {
            if (remove_escape_characters)
            {
                pBuffer++;
                count++;
            }
            else
            {
                *pToken++ = *pBuffer++;
                count++;

                if (count >= maxLength || *pBuffer == 0)
                {
                    return 0;
                }
            }
        }

        *pToken++ = *pBuffer++;
        count++;

        if (count >= maxLength)
        {
            return 0;
        }
    }

    /* Add string terminator to in-buffer string */
    *pToken = '\0';
    return count;
}

static size_t _c_list_remove_escape_characters(const char * src, char * dest, size_t dest_length)
{
    size_t count = 0;

    /* Check for end of list */
    if (*src == '\0')
    {
        return 0;
    }

    while (*src != 0)
    {
        /* Skip Escape characters */
        if (*src == ESCAPE_CHAR)
        {
            src++;
            count++;
        }

        *dest++ = *src++;
        count++;

        if (count >= dest_length)
        {
            return 0;
        }
    }

    /* Add string terminator to in-buffer string */
    *dest = '\0';
    return count;
}

c_list_error_t c_list_import_from_string(c_list_t * c_list, const char * str)
{
    c_list_string_type_t type;
    size_t len;
    c_list_error_t err;
    char buffer1[C_LIST_MAX_STRING_LEN];

    if (!c_list)
    {
        return C_LIST_PARAM_ERROR;
    }

    err = c_list_clear(c_list);
    if (err != C_LIST_SUCCESS)
    {
        return err;
    }

    /* Get string type */
    len = _c_list_get_token(str, buffer1, sizeof(buffer1), 1);
    if (len == 0)
    {
        return C_LIST_NO_DATA_ERROR;
    }
    str = str + len + 1;
    type = (c_list_string_type_t) atoi(buffer1);

    switch (type)
    {
        case C_LIST_STRING_TYPE_NORMAL:
            return _c_list_import_normal_string(c_list, str, 0);

        case C_LIST_STRING_TYPE_COMPRESSED:
            return _c_list_import_normal_string(c_list, str, 1);

        case C_LIST_STRING_TYPE_MIN_MAX_STEP:
            return _c_list_import_min_max_step_string(c_list, str);

        default:
            return C_LIST_PARAM_ERROR;
    }
}

/* Export a list as a string list */
c_list_error_t c_list_export_to_string(const c_list_t * c_list, char * str, size_t size)
{
    return c_list_export_to_string_ext(c_list, str, size, 0);
}

c_list_error_t c_list_export_to_string_ext(const c_list_t * c_list, char * str, size_t size, int force_compression)
{
    if (!c_list)
    {
        return C_LIST_PARAM_ERROR;
    }

    if (size == 0)
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }

    if (c_list->step_options.valid)
    {
        return _c_list_export_min_max_step_string(c_list, str, size);
    }
    else
    {
        if (force_compression)
        {
            return _c_list_export_normal_string(c_list, str, size, 1);
        }
        else
        {
            c_list_error_t err = _c_list_export_normal_string(c_list, str, size, 0);
            if (C_LIST_BUFFER_FULL_ERROR == err)
            {
                err = _c_list_export_normal_string(c_list, str, size, 1);
            }
            return err;
        }
    }
}

/* Fill in table of used characters in the list.  This table is used by
 * the compressor to reduce matching strings to a single unused
 * character */
static void _c_list_get_used_characters(const c_list_t * c_list, unsigned char * used_characters)
{
    const c_list_entry_t * cur_entry;

    if (!c_list)
    {
#ifdef CLIST_USE_LOGGER
        log_error("NULL clist\n");
#endif

        return;
    }

    if (!used_characters)
    {
#ifdef CLIST_USE_LOGGER
        log_error("NULL used characters\n");
#endif

        return;
    }

    cur_entry = c_list->head;
    while (cur_entry)
    {
        const char * tmp = cur_entry->str;
        while (*tmp != '\0')
        {
            /* Make sure the character is in the range of usable
             * characters for compression */
            if (*tmp >= FIRST_USABLE_CHARACER_FOR_COMPRESSION && *tmp <= LAST_USABLE_CHARACTER_FOR_COMPRESSION)
            {
                /* Mark character as used */
                used_characters[*tmp - FIRST_USABLE_CHARACER_FOR_COMPRESSION] = 1;
            }
            tmp++;
        }

        cur_entry = cur_entry->next;
    }
}

static c_list_error_t _c_list_export_normal_string(const c_list_t * c_list, char * str, size_t size, int compress)
{
    size_t cur;
    int cur_int;
    char buff[2 * C_LIST_MAX_STRING_LEN + 1];     /* we use 2*C_LIST_MAX_STRING_LEN in case each char needs to be escaped */
    const char separator[2] = {SEPARATOR_CHAR, '\0'};
    size_t ii;
    c_list_compressed_string_dictionary_entry_t compressed_string_dictionary[MAX_COMPRESSED_STRINGS] = {0};
    const c_list_entry_t * cur_entry;
    size_t offset = 0;
    unsigned char used_characters[LAST_USABLE_CHARACTER_FOR_COMPRESSION - FIRST_USABLE_CHARACER_FOR_COMPRESSION + 1] = {0};
    char current_unused_character = FIRST_USABLE_CHARACER_FOR_COMPRESSION;

    size_t compressed_string_count = 0;

    if (!c_list || !str)
    {
        return C_LIST_PARAM_ERROR;
    }

    str[0] = 0;

    /* Add list type */
    if (compress)
    {
        snprintf(buff, sizeof(buff), "%d", C_LIST_STRING_TYPE_COMPRESSED);

        _c_list_get_used_characters(c_list, used_characters);
    }
    else
    {
        snprintf(buff, sizeof(buff), "%d", C_LIST_STRING_TYPE_NORMAL);
    }

    offset += strlcat(str + offset, buff, size - offset);
    if (size <= offset)
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }
    offset += strlcat(str + offset, separator, size - offset);
    if (size <= offset)
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }

    /* Add current index */
    if (C_LIST_SUCCESS != c_list_get_index(c_list, &cur))
    {
        cur_int = -1;
    }
    else
    {
        cur_int = (int) cur;
    }

    snprintf(buff, sizeof(buff), "%d", cur_int);
    offset += strlcat(str + offset, buff, size - offset);
    if (size <= offset)
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }
    offset += strlcat(str + offset, separator, size - offset);
    if (size <= offset)
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }

    /* Loop through all entries */
    cur_entry = c_list->head;
    while (cur_entry)
    {
        /* Add number */
        snprintf(buff, sizeof(buff), "%d", cur_entry->num);
        offset += strlcat(str + offset, buff, size - offset);
        if (size <= offset)
        {
            return C_LIST_BUFFER_FULL_ERROR;
        }
        offset += strlcat(str + offset, separator, size - offset);
        if (size <= offset)
        {
            return C_LIST_BUFFER_FULL_ERROR;
        }

        /* Add string */
        if (compress)
        {
            char tmp[2 * C_LIST_MAX_STRING_LEN + 1] = {0};
            char tmp2[2 * C_LIST_MAX_STRING_LEN + 1] = {0};
            const char characters_to_compress_1[] = {SEPARATOR_CHAR, ESCAPE_CHAR, COMPRESSED_STRING_DICTIONARY_START_CHAR, COMPRESSED_STRING_DICTIONARY_END_CHAR, COMPRESSED_STRING_DICTIONARY_SEPARATOR_CHAR, '\0'};
            const char characters_to_compress_2[] = {SEPARATOR_CHAR, ESCAPE_CHAR, COMPRESSED_STRING_DICTIONARY_START_CHAR, '\0'};

            /* Escape characters in string compressing.
             * Uncompressed values in the dictionary must be escaped
             * in order to parse the dicationary correctly on import.
             * Escape characters are removed when uncompressing */
            if (!_c_list_convert_string_token(tmp, cur_entry->str, sizeof(tmp), characters_to_compress_1))
            {
                return C_LIST_MEM_ERROR;
            }

            /* Create dictionary and generate compressed version of the
             * stringified list */
            _c_list_compress_string_token(compressed_string_dictionary, &compressed_string_count, buff, tmp, sizeof(buff), used_characters, &current_unused_character);

            /* The uncompressed values in the dictionary should escape
             * both the characters used for the stringified clist
             * syntax (SEPARATOR_CHAR) as well as the characters in the
             * dictionary syntax (COMPRESSED_STRING_DICTIONARY_...)
             * The strings in the stringified clist should only escape
             * the characters used for the stingified clist syntax
             * (SEPARATOR_CHAR). Thus we remove the dictionary specific
             * characters here and then add clist specific  ones to
             * create the stringified list */
            _c_list_remove_escape_characters(buff, tmp2, sizeof(tmp2));

            if (!_c_list_convert_string_token(buff, tmp2, sizeof(buff), characters_to_compress_2))
            {
                return C_LIST_MEM_ERROR;
            }
        }
        else
        {
            const char characters_to_compress[] = {SEPARATOR_CHAR, ESCAPE_CHAR, '\0'};
            if (!_c_list_convert_string_token(buff, cur_entry->str, sizeof(buff), characters_to_compress))
            {
                return C_LIST_MEM_ERROR;
            }
        }

        offset += strlcat(str + offset, buff, size - offset);
        if (size <= offset)
        {
            return C_LIST_BUFFER_FULL_ERROR;
        }
        offset += strlcat(str + offset, separator, size - offset);
        if (size <= offset)
        {
            return C_LIST_BUFFER_FULL_ERROR;
        }

        cur_entry = cur_entry->next;
    }

    /* For compressed lists add dictionary */
    if (compress)
    {
        offset += strlcat(str + offset, "{", size - offset);
        if (size <= offset)
        {
            return C_LIST_BUFFER_FULL_ERROR;
        }

        for (ii = 0; ii < compressed_string_count; ii++)
        {
            const char dictionary_separator[] = {COMPRESSED_STRING_DICTIONARY_SEPARATOR_CHAR, '\0'};

            /* Strings in the dictionary were already escaped when they were
             * added, they should not be escaped again */
            offset += strlcat(str + offset, compressed_string_dictionary[ii].uncompressed_string, size - offset);
            if (size <= offset)
            {
                return C_LIST_BUFFER_FULL_ERROR;
            }

            offset += strlcat(str + offset, dictionary_separator, size - offset);
            if (size <= offset)
            {
                return C_LIST_BUFFER_FULL_ERROR;
            }

            /* Compressed strings in the dicationary were already
             * escaped when they were added, they should not be escaped
             * again */
            str[offset] = compressed_string_dictionary[ii].compressed_character;
            offset++;
            if (size <= offset)
            {
                return C_LIST_BUFFER_FULL_ERROR;
            }
            str[offset] = '\0';

            offset += strlcat(str + offset, separator, size - offset);
            if (size <= offset)
            {
                return C_LIST_BUFFER_FULL_ERROR;
            }
        }

        offset += strlcat(str + offset, "}", size - offset);
        if (size <= offset)
        {
            return C_LIST_BUFFER_FULL_ERROR;
        }
    }

    return C_LIST_SUCCESS;
}

static c_list_error_t _c_list_export_min_max_step_string(const c_list_t * c_list, char * str, size_t size)
{
    size_t cur;
    int cur_int;
    char buff[2 * C_LIST_MAX_STRING_LEN + 1];     /* we use 2*C_LIST_MAX_STRING_LEN in case each char needs to be escaped */
    const char separator[2] = {SEPARATOR_CHAR, 0};
    const char characters_to_compress[] = {SEPARATOR_CHAR, ESCAPE_CHAR, '\0'};

    if (!c_list || !str)
    {
        return C_LIST_PARAM_ERROR;
    }

    str[0] = 0;

    /* Add list type */
    snprintf(buff, sizeof(buff), "%d", C_LIST_STRING_TYPE_MIN_MAX_STEP);
    if (size <= strlcat(str, buff, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }
    if (size <= strlcat(str, separator, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }

    /* Add current index */
    if (C_LIST_SUCCESS != c_list_get_index(c_list, &cur))
    {
        cur_int = -1;
    }
    else
    {
        cur_int = (int) cur;
    }

    snprintf(buff, sizeof(buff), "%d", cur_int);
    if (size <= strlcat(str, buff, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }
    if (size <= strlcat(str, separator, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }

    /* Add Min */
    snprintf(buff, sizeof(buff), "%d", c_list->step_options.min);
    if (size <= strlcat(str, buff, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }
    if (size <= strlcat(str, separator, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }

    /* Add Max */
    snprintf(buff, sizeof(buff), "%d", c_list->step_options.max);
    if (size <= strlcat(str, buff, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }
    if (size <= strlcat(str, separator, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }

    /* Add Step */
    snprintf(buff, sizeof(buff), "%d", c_list->step_options.step);
    if (size <= strlcat(str, buff, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }
    if (size <= strlcat(str, separator, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }

    /* Add Divider */
    snprintf(buff, sizeof(buff), "%d", c_list->step_options.divider);
    if (size <= strlcat(str, buff, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }
    if (size <= strlcat(str, separator, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }

    /* Add Precision */
    snprintf(buff, sizeof(buff), "%d", c_list->step_options.precision);
    if (size <= strlcat(str, buff, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }
    if (size <= strlcat(str, separator, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }

    /* Add Prefix */
    if (!_c_list_convert_string_token(buff, c_list->step_options.prefix, sizeof(buff), characters_to_compress))
    {
        return C_LIST_MEM_ERROR;
    }
    if (size <= strlcat(str, buff, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }
    if (size <= strlcat(str, separator, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }

    /* Add Postfix */
    if (!_c_list_convert_string_token(buff, c_list->step_options.postfix, sizeof(buff), characters_to_compress))
    {
        return C_LIST_MEM_ERROR;
    }
    if (size <= strlcat(str, buff, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }
    if (size <= strlcat(str, separator, size))
    {
        return C_LIST_BUFFER_FULL_ERROR;
    }

    return C_LIST_SUCCESS;
}

void _c_list_parse_compressed_string_dictionary(c_list_compressed_string_dictionary_entry_t * compressed_string_dictionary, size_t * compressed_string_count, const char * str_dictionary)
{
    if (!str_dictionary)
    {
#ifdef CLIST_USE_LOGGER
        log_error("NULL compressed string dictionary\n");
#endif
        return;
    }

    while (*compressed_string_count < MAX_COMPRESSED_STRINGS)
    {
        int src_index = 0;
        int uncompressed_string_index = 0;

        /* Each token in the dictionary contains both the compressed
         * and uncompressed string separated by a colon */
        char token[MAX_UNCOMPRESSED_STRING_LENGTH + 1 + 1];

        /* Get the next token from the string.  Do not remove escape
         * characters here since they are sill needed to parse the
         * token correctly.  The escape characters will be removed when
         * decompressing for import  */
        const size_t len = _c_list_get_token(str_dictionary, token, sizeof(token), 0);
        if (len == 0)
        {
            break;
        }
        str_dictionary = str_dictionary + len + 1;

        /* Separate the uncompressed and compressed values from the
         * token */
        while (token[src_index] != '\0')
        {
            if (token[src_index] != ESCAPE_CHAR)
            {
                /* The character following the separator is the
                 * compressed character */
                if (token[src_index] == COMPRESSED_STRING_DICTIONARY_SEPARATOR_CHAR)
                {
                    compressed_string_dictionary[*compressed_string_count].uncompressed_string[uncompressed_string_index] = '\0';
                    src_index++;
                    compressed_string_dictionary[*compressed_string_count].compressed_character = token[src_index];
                    break;
                }
                else
                {
                    compressed_string_dictionary[*compressed_string_count].uncompressed_string[uncompressed_string_index++] = token[src_index++];
                }
            }
            else
            {
                compressed_string_dictionary[*compressed_string_count].uncompressed_string[uncompressed_string_index++] = token[src_index++];
                if (token[src_index] != '\0')
                {
                    compressed_string_dictionary[*compressed_string_count].uncompressed_string[uncompressed_string_index++] = token[src_index++];
                }
            }
        }

        (*compressed_string_count)++;
    }
}

static c_list_error_t _c_list_import_normal_string(c_list_t * c_list, const char * str, int compress)
{
    int idx;
    size_t len;
    char buffer1[2 * C_LIST_MAX_STRING_LEN + 1]; /* we use 2*C_LIST_MAX_STRING_LEN in case each char needs to be escaped */
    c_list_entry_t listEntry;
    c_list_error_t err;
    c_list_compressed_string_dictionary_entry_t compressed_string_dictionary[MAX_COMPRESSED_STRINGS] = {0};
    size_t compressed_string_count = 0;

    if (!c_list || !str)
    {
        return C_LIST_PARAM_ERROR;
    }

    /* Parse the compressed string dictionary */
    if (compress)
    {
        int ii = 0;
        int dictionary_start = -1;
        int dictionary_end = -1;

        /* Find the start and end of the dictionary */
        while (str[ii] != '\0')
        {
            if (str[ii] == ESCAPE_CHAR)
            {
                if (str[ii + 1] == '\0')
                {
                    break;
                }

                ii += 2;
                continue;
            }

            if (str[ii] == COMPRESSED_STRING_DICTIONARY_START_CHAR)
            {
                dictionary_start = ii + 1;
            }
            else if (dictionary_start != -1 && str[ii] == COMPRESSED_STRING_DICTIONARY_END_CHAR)
            {
                dictionary_end = ii - 2;
                break;
            }
            ii++;
        }

        /* Valid dicationary was found so parse it and fill in the
         * dictionary structure */
        if (dictionary_start > 0 && dictionary_end > 0 && dictionary_start < dictionary_end)
        {
            const size_t str_dictionary_size = dictionary_end - dictionary_start + 1;
            if (str_dictionary_size <= MAX_DICTIONARY_STRING_LENGTH)
            {
                char str_dictionary[MAX_DICTIONARY_STRING_LENGTH] = {0};
                strncpy(str_dictionary, str + dictionary_start, str_dictionary_size);
                _c_list_parse_compressed_string_dictionary(compressed_string_dictionary, &compressed_string_count, str_dictionary);
            }
            else
            {
#ifdef CLIST_USE_LOGGER
                log_error("String dictionary too large to fit in buffer\n");
#endif
                return C_LIST_BUFFER_FULL_ERROR;
            }
        }
    }

    /* Get current index */
    len = _c_list_get_token(str, buffer1, sizeof(buffer1), 1);
    if (len == 0)
    {
        return C_LIST_NO_DATA_ERROR;
    }
    str = str + len + 1;
    idx = atoi(buffer1);

    for (;; )
    {
        user_defined_t user_defined;

        /* Do not attempt to uncompress the dictionary.  This assumes
         * the dictionary is at the very end of the string */
        if (*str == COMPRESSED_STRING_DICTIONARY_START_CHAR)
        {
            break;
        }

        /* Get Value */
        len = _c_list_get_token(str, buffer1, sizeof(buffer1), 1);
        if (len == 0)
            break;
        str = str + len + 1;
        listEntry.num = atoi(buffer1);

        /* Get String */
        len = _c_list_get_token(str, buffer1, sizeof(buffer1), compress ? 0 : 1);
        str = str + len;
        if (*str == SEPARATOR_CHAR)
            str += 1;

        /* Expand compressed strings */
        if (compress)
        {
            size_t src_index = 0;
            int dest_index = 0;
            memset(listEntry.str, 0, sizeof(listEntry.str));

            /* Iterate over each character in the buffer and compare
             * it to each of the compressed characters in the
             * dictionary.  If a match is found copy the corresponding
             * uncompressed string otherwise copy the character itself */
            while (src_index < len)
            {
                int compressed_string_found = 0;
                size_t ii = 0;

                /* Ignore escaped characters */
                if (buffer1[src_index] == ESCAPE_CHAR)
                {
                    src_index++;
                    if (src_index < len)
                    {
                        listEntry.str[dest_index++] = buffer1[src_index++];
                    }
                    continue;
                }

                /* Attempt to find a matching compressed character */
                for (ii = 0; ii < compressed_string_count; ii++)
                {
                    /* If a match is found copy the corresponding
                     * uncompressed value.  This assumes that only one
                     * compressed string will match.  To make this
                     * generic it must be assumed that more than one
                     * compressed string can match and the longest
                     * matching string must be found (requires
                     * iterating over all of the entries) */
                    if (buffer1[src_index] == compressed_string_dictionary[ii].compressed_character)
                    {
                        /* Remove any escape characters that were added
                         * to the dictionary */
                        char tmp[MAX_UNCOMPRESSED_STRING_LENGTH] = {0};
                        _c_list_remove_escape_characters(compressed_string_dictionary[ii].uncompressed_string, tmp, sizeof(tmp));

                        strncpy(listEntry.str + dest_index, tmp, strlen(tmp));
                        dest_index += strlen(tmp);
                        src_index++;
                        compressed_string_found = 1;
                        break;
                    }
                }

                if (!compressed_string_found)
                {
                    listEntry.str[dest_index++] = buffer1[src_index++];
                }
            }
        }
        else
        {
            strlcpy(listEntry.str, buffer1, sizeof(listEntry.str));
        }

        /* Add to List */
        user_defined.ptr = NULL;
        user_defined.int32 = 0;
        listEntry.user_defined.ptr = NULL;
        err = c_list_append(c_list, listEntry.num, listEntry.str, user_defined);
        if (err != C_LIST_SUCCESS)
        {
            return err;
        }
    }

    /* Set index */
    if (idx >= 0)
    {
        err = c_list_set_index(c_list, idx);
        if (err != C_LIST_SUCCESS)
        {
            return err;
        }
    }

    return C_LIST_SUCCESS;
}

static c_list_error_t _c_list_import_min_max_step_string(c_list_t * c_list, const char * str)
{
    int idx;
    size_t len;
    char buffer1[2 * C_LIST_MAX_STRING_LEN + 1]; /* we use 2*C_LIST_MAX_STRING_LEN in case each char needs to be escaped */
    c_list_error_t err;
    int min;
    int max;
    int step;
    int divider;
    int precision;
    char prefix[C_LIST_MAX_STRING_LEN];
    char postfix[C_LIST_MAX_STRING_LEN];

    if (!c_list || !str)
    {
        return C_LIST_PARAM_ERROR;
    }

    /* Get current index */
    len = _c_list_get_token(str, buffer1, sizeof(buffer1), 1);
    if (len == 0)
        return C_LIST_NO_DATA_ERROR;
    str = str + len + 1;
    idx = atoi(buffer1);

    /* Get min */
    len = _c_list_get_token(str, buffer1, sizeof(buffer1), 1);
    if (len == 0)
        return C_LIST_NO_DATA_ERROR;
    str = str + len + 1;
    min = atoi(buffer1);

    /* Get max */
    len = _c_list_get_token(str, buffer1, sizeof(buffer1), 1);
    if (len == 0)
        return C_LIST_NO_DATA_ERROR;
    str = str + len + 1;
    max = atoi(buffer1);

    /* Get step */
    len = _c_list_get_token(str, buffer1, sizeof(buffer1), 1);
    if (len == 0)
        return C_LIST_NO_DATA_ERROR;
    str = str + len + 1;
    step = atoi(buffer1);

    /* Get divider */
    len = _c_list_get_token(str, buffer1, sizeof(buffer1), 1);
    if (len == 0)
        return C_LIST_NO_DATA_ERROR;
    str = str + len + 1;
    divider = atoi(buffer1);

    /* Get precision */
    len = _c_list_get_token(str, buffer1, sizeof(buffer1), 1);
    if (len == 0)
        return C_LIST_NO_DATA_ERROR;
    str = str + len + 1;
    precision = atoi(buffer1);

    /* Get prefix */
    len = _c_list_get_token(str, prefix, sizeof(prefix), 1);
    str = str + len + 1;

    /* Get postfix */
    (void) _c_list_get_token(str, postfix, sizeof(postfix), 1);

    err = c_list_fill_generic(c_list, min, max, step, divider, precision, postfix, prefix);
    if (err != C_LIST_SUCCESS)
    {
        return err;
    }

    /* Set index */
    if (idx >= 0)
    {
        err = c_list_set_index(c_list, idx);
        if (err != C_LIST_SUCCESS)
        {
            return err;
        }
    }

    return C_LIST_SUCCESS;
}

int c_list_compare(const c_list_t * c_list_1, const c_list_t * c_list_2)
{
    /* if they are pointing to the same list, they are equal */
    if (c_list_1 == c_list_2)
    {
        return 1;
    }

    /* if either list is NULL, they are not equal */
    if (!c_list_1 || !c_list_2)
    {
        return 0;
    }
    else
    {
        const c_list_entry_t * cur_1 = c_list_1->head;
        const c_list_entry_t * cur_2 = c_list_2->head;

        while (cur_1 && cur_2)
        {
            if (cur_1->num != cur_2->num)
            {
                return 0;
            }

            if (strcmp(cur_1->str, cur_2->str) != 0)
            {
                return 0;
            }

            if (c_list_1->cur == cur_1)
            {
                if (c_list_2->cur != cur_2)
                {
                    /* the lists don't have the same current element */
                    return 0;
                }
            }

            cur_1 = cur_1->next;
            cur_2 = cur_2->next;
        }

        /* the lists are different lengths */
        if (cur_1 || cur_2)
        {
            return 0;
        }

        return 1;
    }
}
