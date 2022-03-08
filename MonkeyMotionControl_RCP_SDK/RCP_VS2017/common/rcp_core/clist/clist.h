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

#ifndef LIST_H_INCLUDED
#define LIST_H_INCLUDED

#include <stdlib.h>
#include <stdint.h>
#include <string>
#include "c_list.h"

class cList {
    public:
        typedef c_list_num_t NumType;
        static const size_t MaxStringLength = C_LIST_MAX_STRING_LEN;

        typedef enum {
            SUCCESS = C_LIST_SUCCESS,                      /* No error */
            INDEX_ERROR = C_LIST_INDEX_ERROR,              /* Index is out of range */
            FIND_ERROR = C_LIST_FIND_ERROR,                /* Could not find item matching search criteria */
            LOAD_ERROR = C_LIST_LOAD_ERROR,                /* Error loading list from file */
            PARAM_ERROR = C_LIST_PARAM_ERROR,              /* Invalid parameter */
            MEM_ERROR = C_LIST_MEM_ERROR,                  /* Out of memory */
            NO_DATA_ERROR = C_LIST_NO_DATA_ERROR,          /* Empty List */
            BUFFER_FULL_ERROR = C_LIST_BUFFER_FULL_ERROR   /* String buffer full Error*/
        } Error;

        typedef enum {
            FIND_EXACT = C_LIST_FIND_EXACT,                /* Match search criteria exactly */
            FIND_CLOSEST = C_LIST_FIND_CLOSEST,            /* Find closest value */
            FIND_NEXT_SMALLER = C_LIST_FIND_NEXT_SMALLER,  /* Find next smaller match */
            FIND_NEXT_BIGGER = C_LIST_FIND_NEXT_BIGGER     /* Find next bigger match */
        } FindOptions;

        struct Entry {
            Entry() : num(0), userDefined()
            {
                str[0] = 0;
                userDefined.ptr = NULL;
                userDefined.int32 = 0;
            }
            NumType num;                /* Number */
            char str[MaxStringLength];  /* String */
            user_defined_t userDefined;         /* User Defined Data */
        };

        cList();
        cList(const cList & rhs);
        cList(const c_list_t * c_list);

        /* Initialize a clist with values.  The first argument in the
         * count followed by that many (int, string) pairs.  E.g.:
         * cList myList(3, 1, "ONE", 2, "TWO", 3, "THREE");
         */
        cList(size_t count, ...);

        /* Initialize a generic clist.  Note, we move the postfix and
         * prefix to the front - rather then the same order as
         * fillListGeneric so that this constructor is not ambiguous
         * with the variadic constructor. */
        cList(const char * prefix, const char * postfix, int min, int max, int step, int divider, int precision);

        ~cList();

#ifdef CLIST_USE_FILEPARSER
        /* Load list from configuration file */
        Error load(const char * filename, const char * group, bool alphabetized = false);
#endif
        /* Import a list based on strings (From an RCP Command for Example) */
        Error importStringList(const char * stringList);
        /* Import a list based on strings and decode each string (From an RCP Command for Example) */
        Error importStringListAndDecode(const char * stringList);
        /* Export a list as a string list */
        Error exportStringList(char * stringList, size_t maxLength, bool force_compression = false) const;
        /* Export a list as a string list */
        Error exportStringList(std::string & stringList) const;

        /* Append item 'e' to end of list */
        Error append(const Entry &e);
        /* Append item with num and str to end of list */
        Error append(NumType num, const char * str);
        /* Insert item 'e' into list at index 'idx' */
        Error insert(size_t idx, const Entry &e);
        /* Add item to list in sorted order.
         * NOTE: list must already be sorted by number in ascending order. */
        Error addItemInSortOrder(const Entry &e);
        /* Add item to list in sorted order.
         * Note: list must already be sorted by number in ascending order. */
        Error addItemInSortOrderUnique(const Entry &e);
        /* Add item to list in alphabetical order.
         * NOTE: list must already be sorted alphabetically. */
        Error addItemInAlphabeticalOrder(const Entry &e);
                /* Add item to list in alphabetical order.
         * NOTE: list must already be sorted alphabetically. */
        Error addItemInAlphabeticalOrder(NumType num, const char * str);

        /* Removed item at index 'idx' from list */
        Error remove(size_t idx);

        /* Clear entire list */
        void clear(void);

        /* Return length of list */
        size_t length(void) const;

        /* Get entire data structure 'e' at index 'idx' */
        Error getData(size_t idx, Entry &e) const;
        /* Get number 'n' from item at index 'idx' */
        Error getNum(size_t idx, NumType &n) const;
        /* Get string 's' from item at index 'idx' */
        /*lint -ecall(545, cList::getStr) */
        Error getStr(size_t idx, char (*s)[MaxStringLength]) const;
        /* Get STL string 's' from item at index 'idx' */
        Error getStr(size_t idx, std::string & s) const;
        /* Get user-defined value 'u' from item at index 'idx' */
        Error getUserDefined(size_t idx, user_defined_t * u) const;

        /* Set entire data structure 'e' for item at index 'idx' */
        Error setData(size_t idx, const Entry &e);

        /* Find item matching number 'n', using find options 'o'.
         * Entry 'e' is returned. Index is stored as current index. */
        Error findNum(NumType n, Entry &e, FindOptions o = FIND_EXACT);
        /* Find item matching string 'str'. Entry 'e' is returned.
         * Index is stored as current index. */
        Error findStr(const char * str, Entry &e);
        /* Find item matching string 'str' with n number of charcters. Entry 'e' is returned.
         * Index is stored as current index. */
        Error findFirstNstr(const size_t n, const char * str, Entry &e);
        /* Find first item in list. Entry 'e' is returned. Index is
         * stored as current index. */
        Error findFirst(Entry &e);
        /* Find last item in list. Entry 'e' is returned. Index is
         * stored as current index. */
        Error findLast(Entry &e);
        /* Find next item (after current index) in list. Entry 'e' is
         * returned. Index is stored as current index. */
        Error findNext(Entry &e);
        /* Find previous item (before current index) in list. Entry 'e' is
         * returned. Index is stored as current index. */
        Error findPrev(Entry &e);

        /* Returned current index */
        Error getIndex(size_t &idx) const;
        /* Sets Current index */
        Error setIndex(size_t idx);

        /* Set entire data structure 'e' for current index */
        Error setCurrentData(const Entry &e);
        /* Get entire data structure 'e' for current index */
        Error getCurrentData(Entry &e) const;
        /* Get number 'n' for current index */
        Error getCurrentNum(NumType &n) const;
        /* Get string 's' for current index */
        /*lint -ecall(545, cList::getCurrentStr) */
        Error getCurrentStr(char (*s)[MaxStringLength]) const;
        /* Get user-defined value 'u' for current index */
        Error getCurrentUserDefined(user_defined_t * u) const;
        //Fill list with a generic range of values
        Error fillListGeneric(int min, int max, int step, int divider, int precision, const char * postfix = NULL, const char * prefix = NULL);

        void createArray(Entry * * array, size_t & len) const;
        void destroyArray(Entry * * array) const;

        Error decode(void);

        cList & operator=(const cList & rhs);
        bool operator==(const cList & rhs) const;
        bool operator!=(const cList & rhs) const;

    private:
        static void copyEntry(Entry & dst, const c_list_entry_t & src);
        static void copyEntry(c_list_entry_t & dst, const Entry & src);
        c_list_t * m_c_list;
};

#endif
