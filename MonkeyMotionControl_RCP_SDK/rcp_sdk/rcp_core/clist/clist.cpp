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

/* clist.cpp
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
 * CLIST_USE_THREADX: use threadX memory pool instead of malloc/free
 *                    for list entries
 * CLIST_USE_FILEPARSER: use fileparser (this adds the load method)
 * CLIST_USE_LOGGER: use logging functions
 */

#include "clist/clist.h"
#include <iostream>
#include <assert.h>
#include <string.h>
#include <stdio.h>
#include <stdarg.h>
#ifdef CLIST_USE_FILEPARSER
#include "app_engine/file_format/config_file/fileParser.h"
#endif
#include "stringl/stringl.h"
#include "decorated_string/decorated_string.h"

#if defined(_MSC_VER) && (_MSC_VER < 1900)
#define snprintf _snprintf
#endif

/* Note that a deep copy of the userDefined void * is not performed. */
cList & cList::operator =(const cList &rhs)
{
    if (this == &rhs)
    {
        return *this;
    }

    /*lint -esym(1539, cList::m_c_list) */
    (void) c_list_copy(m_c_list, rhs.m_c_list);
    return *this;
}

bool cList::operator==(const cList &rhs) const
{
    return c_list_compare(m_c_list, rhs.m_c_list) == 1;
}

bool cList::operator!=(const cList &rhs) const
{
    return !operator==(rhs);
}

cList::cList(const cList & rhs) : m_c_list(NULL)
{
    m_c_list = c_list_create(malloc, free);
    *this = rhs;
}

cList::cList() : m_c_list(NULL)
{
    m_c_list = c_list_create(malloc, free);
}

cList::cList(const c_list_t * c_list) : m_c_list(NULL)
{
    m_c_list = c_list_create(malloc, free);
    (void) c_list_copy(m_c_list, c_list);
}

cList::cList(size_t count, ...) : m_c_list(NULL)
{
    m_c_list = c_list_create(malloc, free);

    va_list arg_list;
    va_start(arg_list, count);
    for (size_t ii = 0; ii < count; ii++)
    {
        Entry e;
        user_defined_t user_defined;

        e.num = va_arg(arg_list, NumType);
        strlcpy(e.str, va_arg(arg_list, const char *), sizeof(e.str));
        user_defined.ptr = NULL;
        user_defined.int32 = 0;
        (void) c_list_append(m_c_list, e.num, e.str, user_defined);
    }
    va_end(arg_list);
}

cList::cList(const char * prefix, const char * postfix, int min, int max, int step, int divider, int precision) : m_c_list(NULL)
{
    m_c_list = c_list_create(malloc, free);

    (void) c_list_fill_generic(m_c_list, min, max, step, divider, precision, postfix, prefix);
}

cList::~cList()
{
    (void) c_list_delete(m_c_list);
    m_c_list = NULL;
}

#ifdef CLIST_USE_FILEPARSER
cList::Error cList::load(const char * filename, const char * group, bool alphabetized)
{
    fileParser parser;
    cList::Entry entry;
    int error;

    if (m_c_list)
    {
        m_c_list->step_options.valid = 0;
    }

    /* Open file */
    error = parser.initFile(filename, true);
    if (error < 0)
    {
        return LOAD_ERROR;
    }

    /* Find group */
    error = parser.findGroup((char *) group, NULL);
    if (error < 0)
    {
        return LOAD_ERROR;
    }
    else
    {
        /* Load data */
        while (parser.loadList(&entry.num, entry.str, sizeof(entry.str)) >= 0)
        {
            cList::Error err;
            if (alphabetized)
            {
                err = addItemInAlphabeticalOrder(entry);
            }
            else
            {
                err = append(entry);
            }
            if (err != SUCCESS)
            {
                return err;
            }
        }
    }
    return setIndex(0);
}
#endif

cList::Error cList::importStringList(const char * stringList)
{
    return (Error) c_list_import_from_string(m_c_list, stringList);
}

cList::Error cList::importStringListAndDecode(const char * stringList)
{
    Error err;
    err = (Error) c_list_import_from_string(m_c_list, stringList);
    if (err != SUCCESS)
    {
        return err;
    }

    return (Error) c_list_decode(m_c_list);
}

cList::Error cList::exportStringList(std::string & stringList) const
{
    char * buffer = NULL;
    size_t len = 8096;

    for (;; )
    {
        buffer = (char *) malloc(len);
        if (!buffer)
        {
            return MEM_ERROR;
        }

        const Error err = exportStringList(buffer, len);

        if (err == SUCCESS)
        {
            stringList = buffer;
            free(buffer);
            return SUCCESS;
        }
        else if (err == BUFFER_FULL_ERROR)
        {
            free(buffer);
            len *= 2;
        }
        else
        {
            free(buffer);
            return err;
        }
    }
}

/* Export a list as a string list */
cList::Error cList::exportStringList(char * stringList, size_t maxLength, bool force_compression) const
{
    return (Error) c_list_export_to_string_ext(m_c_list, stringList, maxLength, force_compression);
}

cList::Error cList::append(NumType num, const char * str)
{
    user_defined_t user_defined;
    user_defined.ptr = NULL;
    user_defined.int32 = 0;
    return (Error) c_list_append(m_c_list, num, str, user_defined);
}

cList::Error cList::append(const Entry &e)
{
    return (Error) c_list_append(m_c_list, e.num, e.str, e.userDefined);
}

cList::Error cList::insert(size_t idx, const Entry &e)
{
    return (Error) c_list_insert(m_c_list, idx, e.num, e.str, e.userDefined);
}

cList::Error cList::remove(size_t idx)
{
    return (Error) c_list_remove(m_c_list, idx);
}

void cList::clear(void)
{
    (void) c_list_clear(m_c_list);
}

void cList::copyEntry(Entry & dst, const c_list_entry_t & src)
{
    dst.num = src.num;
    strlcpy(dst.str, src.str, sizeof(dst.str));
    dst.userDefined = src.user_defined;
}

void cList::copyEntry(c_list_entry_t & dst, const Entry & src)
{
    dst.num = src.num;
    strlcpy(dst.str, src.str, sizeof(dst.str));
    dst.user_defined = src.userDefined;
}

cList::Error cList::getData(size_t idx, Entry &e) const
{
    Error err;
    c_list_entry_t c_list_entry;
    err = (Error) c_list_get_entry(m_c_list, idx, &c_list_entry);
    if (err == SUCCESS)
    {
        copyEntry(e, c_list_entry);
    }
    return err;
}

cList::Error cList::setData(size_t idx, const Entry &e)
{
    c_list_entry_t c_list_entry;
    copyEntry(c_list_entry, e);
    return (Error) c_list_set_entry(m_c_list, idx, &c_list_entry);
}

size_t cList::length(void) const
{
    return (Error) c_list_get_length(m_c_list);
}

cList::Error cList::getIndex(size_t &idx) const
{
    return (Error) c_list_get_index(m_c_list, &idx);
}

cList::Error cList::setIndex(size_t idx)
{
    return (Error) c_list_set_index(m_c_list, idx);
}

cList::Error cList::findNum(cList::NumType n, Entry &e, FindOptions o)
{
    Error err;
    c_list_entry_t c_list_entry;
    err = (Error) c_list_find_num(m_c_list, n, &c_list_entry, (c_list_find_t) o);
    if (err == SUCCESS)
    {
        copyEntry(e, c_list_entry);
    }
    return err;
}

cList::Error cList::findStr(const char * str, Entry &e)
{
    Error err;
    c_list_entry_t c_list_entry;
    err = (Error) c_list_find_str(m_c_list, str, &c_list_entry);
    if (err == SUCCESS)
    {
        copyEntry(e, c_list_entry);
    }
    return err;
}

cList::Error cList::findFirstNstr(const size_t n, const char * str, Entry &e)
{
    Error err;
    c_list_entry_t c_list_entry;
    err = (Error) c_list_find_strn(m_c_list, str, n, &c_list_entry);
    if (err == SUCCESS)
    {
        copyEntry(e, c_list_entry);
    }
    return err;
}

cList::Error cList::findFirst(Entry &e)
{
    Error err;
    c_list_entry_t c_list_entry;
    err = (Error) c_list_find_first(m_c_list, &c_list_entry);
    if (err == SUCCESS)
    {
        copyEntry(e, c_list_entry);
    }
    return err;
}

cList::Error cList::findLast(Entry &e)
{
    Error err;
    c_list_entry_t c_list_entry;
    err = (Error) c_list_find_last(m_c_list, &c_list_entry);
    if (err == SUCCESS)
    {
        copyEntry(e, c_list_entry);
    }
    return err;
}

cList::Error cList::findNext(Entry &e)
{
    Error err;
    c_list_entry_t c_list_entry;
    err = (Error) c_list_find_next(m_c_list, &c_list_entry);
    if (err == SUCCESS)
    {
        copyEntry(e, c_list_entry);
    }
    return err;
}

cList::Error cList::findPrev(Entry &e)
{
    Error err;
    c_list_entry_t c_list_entry;
    err = (Error) c_list_find_prev(m_c_list, &c_list_entry);
    if (err == SUCCESS)
    {
        copyEntry(e, c_list_entry);
    }
    return err;
}

cList::Error cList::getNum(size_t idx, NumType &n) const
{
    c_list_entry_t e;
    const Error err = (Error) c_list_get_entry(m_c_list, idx, &e);

    if (err != SUCCESS)
    {
        return err;
    }
    n = e.num;
    return SUCCESS;
}

cList::Error cList::getStr(size_t idx, char (*s)[MaxStringLength]) const
{
    c_list_entry_t e;
    const Error err = (Error) c_list_get_entry(m_c_list, idx, &e);

    if (err != SUCCESS)
    {
        return err;
    }
    if (s == NULL)
    {
        return PARAM_ERROR;
    }

    strlcpy(*s, e.str, MaxStringLength);
    return SUCCESS;
}

cList::Error cList::getStr(size_t idx, std::string & s) const
{
    c_list_entry_t e;
    const Error err = (Error) c_list_get_entry(m_c_list, idx, &e);

    s.clear();

    if (err != SUCCESS)
    {
        return err;
    }

    s = e.str;
    return SUCCESS;
}

cList::Error cList::getUserDefined(size_t idx, user_defined_t * u) const
{
    c_list_entry_t e;
    const Error err = (Error) c_list_get_entry(m_c_list, idx, &e);
    if (err != SUCCESS)
    {
        return err;
    }
    if (u == NULL)
    {
        return PARAM_ERROR;
    }
    *u = e.user_defined;
    return SUCCESS;
}

cList::Error cList::getCurrentNum(NumType &n) const
{
    c_list_entry_t e;
    const Error err = (Error) c_list_get_current_entry(m_c_list, &e);
    if (err != SUCCESS)
    {
        return err;
    }
    n = e.num;
    return SUCCESS;
}

cList::Error cList::getCurrentStr(char (*s)[MaxStringLength]) const
{
    c_list_entry_t e;
    const Error err = (Error) c_list_get_current_entry(m_c_list, &e);
    if (err != SUCCESS)
    {
        return err;
    }
    if (s == NULL)
    {
        return PARAM_ERROR;
    }

    strlcpy(*s, e.str, MaxStringLength);
    return SUCCESS;
}

cList::Error cList::getCurrentUserDefined(user_defined_t * u) const
{
    c_list_entry_t e;
    const Error err = (Error) c_list_get_current_entry(m_c_list, &e);
    if (err != SUCCESS)
    {
        return err;
    }
    if (u == NULL)
    {
        return PARAM_ERROR;
    }
    *u = e.user_defined;
    return SUCCESS;
}

cList::Error cList::setCurrentData(const Entry &e)
{
    c_list_entry_t c_list_entry;
    copyEntry(c_list_entry, e);
    return (Error) c_list_set_current_entry(m_c_list, &c_list_entry);
}

cList::Error cList::getCurrentData(Entry &e) const
{
    Error err;
    c_list_entry_t c_list_entry;
    err = (Error) c_list_get_current_entry(m_c_list, &c_list_entry);
    if (err == SUCCESS)
    {
        copyEntry(e, c_list_entry);
    }
    return err;
}

cList::Error cList::addItemInAlphabeticalOrder(const Entry &e)
{
    return (Error) c_list_insert_in_alphabetical_order(m_c_list, e.num, e.str, e.userDefined);
}

cList::Error cList::addItemInAlphabeticalOrder(NumType num, const char * str)
{
    user_defined_t user_defined;
    user_defined.ptr = NULL;
    user_defined.int32 = 0;
    return (Error) c_list_insert_in_alphabetical_order(m_c_list, num, str, user_defined);
}

cList::Error cList::addItemInSortOrder(const Entry &e)
{
    return (Error) c_list_insert_in_sort_order(m_c_list, e.num, e.str, e.userDefined);
}

cList::Error cList::addItemInSortOrderUnique(const Entry &e)
{
    return (Error) c_list_insert_in_sort_order_unique(m_c_list, e.num, e.str, e.userDefined);
}

cList::Error cList::fillListGeneric(int min, int max, int step, int divider, int precision, const char * postfix, const char * prefix)
{
    return (Error) c_list_fill_generic(m_c_list, min, max, step, divider, precision, postfix, prefix);
}

void cList::createArray(Entry * * array, size_t & len) const
{
    const size_t list_len = length();
    *array = new Entry[list_len];
    len = 0;

    if (!array)
    {
        return;
    }

    for (len = 0; len < list_len; len++)
    {
        c_list_entry_t c_list_entry;
        if (SUCCESS == (Error) c_list_get_entry(m_c_list, len, &c_list_entry))
        {
            copyEntry((*array)[len], c_list_entry);
        }
    }
}

void cList::destroyArray(Entry * * array) const
{
    if (*array)
    {
        delete [] (*array);
        *array = NULL;
    }
}
