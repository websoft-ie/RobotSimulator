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

/*line -strong(AJX,c_list_error_t,c_list_find_t,c_list_string_type_t) */
#ifndef C_LIST_H_INCLUDED
#define C_LIST_H_INCLUDED

#include <stdlib.h>
#include <stdint.h>

/** @file c_list.h
 * c_list is a library for maintaining lists used in-camera.  There is a
 * C++ wrapper for this library called cList.
 *
 * Lists contain any number of entries.  Each entry contains a numeric
 * value, string representation, and option user defined data.
 *
 * Lists also have a "current" entry.  That is, a entry in the list
 * that is the denoted as being the currently selected item.
 *
 * Lists can be converted to strings for transfer between two devices.
 * Normally, this is to send lists out from the camera to a external
 * device using RCP.
 * */

/*
 * Stringify format:  There are three different stringify formats
 * including: normal (C_LIST_STRING_TYPE_NORMAL), compressed
 * (C_LIST_STRING_TYPE_COMPRESSED), and min/max/step
 * (C_LIST_STRING_TYPE_MIN_MAX_STEP).
 *
 * C_LIST_STRING_TYPE_NORMAL:
 *     format: TYPE|CURRENT_INDEX|[NUM|STRING|]*
 *
 * C_LIST_STRING_TYPE_COMPRESSED:
 *     format: TYPE|CURRENT_INDEX|[NUM|COMPRESSED_STRING|]*{[UNCOMPRESSED_STRING:COMPRESSED_CHARACTER|]*}
 *
 *     Note: Compression is only used when the normal format generates a
 *     string too large to fit the 8K buffer.
 *
 *     This format replaces the STRING portion of the
 *     C_LIST_STRING_TYPE_NORMAL format with a COMPRESSED_STRING.  The
 *     COMPRESSED_STRING can contain both COMPRESSED_CHARACTERs and
 *     non-compressed portions of the original STRING. The mapping
 *     between UNCOMPRESSED_STRINGs and COMPRESSED_CHARACTERs is added
 *     at the end of the stringified list in a dictionary (denoted by
 *     curly braces).  To uncompress the stringified list use the
 *     dictionary to replace all COMPRESSED_CHARACTERs in each
 *     COMPRESSED_STRING with the corresponding UNCOMPRESSED_STRING.
 *
 * C_LIST_STRING_TYPE_MIN_MAX_STEP:
 *     format: TYPE|CURRENT_INDEX|MIN|MAX|STEP|DIVIDER|PRECISION|PREFIX|POSTFIX|
 *
 * TYPE: type of string (see c_list_string_type_t)
 * CURRENT_INDEX: zero based index denoting which entry in the list is
 *     the "current" entry; -1, if the list is empty.
 * NUM: integer portion of list entry
 * STRING: string portion of list entry
 * COMPRESSED_STRING: see above
 * UNCOMPRESSED_STRING: see above
 * COMPRESSED_CHARACTER: see above
 * MIN: minimum value for NUM (value for first entry in list)
 * MAX: maximum value for NUM
 * STEP: each entry in list is calculated as previous entry plus STEP
 * DIVIDER/PRECISION/PREFIX/POSTFIX: used to create string for entry
 *     based on NUM.  The format is: "PREFIX VALUE POSTFIX" where VALUE
 *     is NUM/DIVIDER shown to PRECISION decimal places.
 */

typedef int32_t c_list_num_t;
#define C_LIST_MAX_STRING_LEN 255
typedef void * (* c_list_malloc_t) (size_t);
typedef void (* c_list_free_t) (void *);

#ifdef __cplusplus
extern "C"
{
#endif

typedef enum {
    C_LIST_SUCCESS,                    /**< No error */
    C_LIST_INDEX_ERROR,                /**< Index is out of range */
    C_LIST_FIND_ERROR,                 /**< Could not find item matching search criteria */
    C_LIST_LOAD_ERROR,                 /**< Error loading list from file */
    C_LIST_PARAM_ERROR,                /**< Invalid parameter */
    C_LIST_MEM_ERROR,                  /**< Out of memory */
    C_LIST_NO_DATA_ERROR,              /**< Empty List */
    C_LIST_BUFFER_FULL_ERROR           /**< String buffer full Error*/
} c_list_error_t;

typedef enum {
    C_LIST_FIND_EXACT,                 /**< Match search criteria exactly */
    C_LIST_FIND_CLOSEST,               /**< Find closest value */
    C_LIST_FIND_NEXT_SMALLER,          /**< Find next smaller match */
    C_LIST_FIND_NEXT_BIGGER            /**< Find next bigger match */
} c_list_find_t;

typedef enum {
    C_LIST_STRING_TYPE_NORMAL,
    C_LIST_STRING_TYPE_MIN_MAX_STEP,
    C_LIST_STRING_TYPE_COMPRESSED
} c_list_string_type_t;

/** INTERNAL USE ONLY */
typedef struct
{
    int valid;
    int min;
    int max;
    int step;
    int divider;
    int precision;
    char prefix[C_LIST_MAX_STRING_LEN];
    char postfix[C_LIST_MAX_STRING_LEN];
} c_list_step_options_t;

typedef union
{
    void * ptr;
    int32_t int32;
} user_defined_t;

/** Structure defining each entry in the list */
typedef struct c_list_entry_tag {
    c_list_num_t num;                   /**< Number */
    char str[C_LIST_MAX_STRING_LEN];    /**< String */
    user_defined_t user_defined;        /**< User Defined Data */
    struct c_list_entry_tag * next;     /**< INTERNAL USE ONLY */
    struct c_list_entry_tag * prev;     /**< INTERNAL USE ONLY */
} c_list_entry_t;

/** INTERNAL USE ONLY */
typedef struct {
    c_list_entry_t * head;
    c_list_entry_t * tail;
    c_list_entry_t * cur;
    c_list_step_options_t step_options;
    c_list_malloc_t malloc;
    c_list_free_t free;
} c_list_t;

/** @brief Create list.
 *
 * @param[in] c_list_malloc    allocator function to use for all
 * dynamic memory allocations (typically this will be malloc)
 * @param[in] c_list_free      de-allocator function to use for all
 * dynamic memory de-allocations (typically this will be free)
 *
 * @returns pointer to newly allocated list (must be deleted by
 * calling @ref c_list_delete) or NULL on error.
 * */
c_list_t * c_list_create(c_list_malloc_t c_list_malloc, c_list_free_t c_list_free);

/** @brief Delete list.
 *
 * @param[in] c_list    pointer of list to delete (returned by @ref c_list_create)
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_delete(c_list_t * c_list);

/** @brief Copy list.
 *
 * Will copy list @p src to @p dest.  Both @p src and @p must be valid
 * lists allocated by @ref c_list_create.  @note If the @a user_defined
 * field is a pointer to an external object it will not be deep copied.
 *
 * @param dest  destination list
 * @param src   source list
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_copy(c_list_t * dest, const c_list_t * src);

/** @brief Append item to list.
 *
 * @param[in] c_list    list to add item to
 * @param[in] num       integer value of item to add
 * @param[in] str       string value of item to add
 * @param[in] user_defined   user defined value of item to add
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_append(c_list_t * c_list, c_list_num_t num, const char * str, user_defined_t user_defined);

/** @brief Insert item in list at specified location.
 *
 * @param[in] c_list    list to add item to
 * @param[in] idx       0-based index of where to add item
 * @param[in] num       integer value of item to add
 * @param[in] str       string value of item to add
 * @param[in] user_defined   user defined value of item to add
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_insert(c_list_t * c_list, size_t idx, c_list_num_t num, const char * str, user_defined_t user_defined);

/** @brief Insert item in list in sorted order.
 *
 * Inserts item in list in sorted order.
 *
 * @note The list must already be in sorted order
 *
 * @param[in] c_list    list to add item to
 * @param[in] num       integer value of item to add
 * @param[in] str       string value of item to add
 * @param[in] user_defined   user defined value of item to add
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_insert_in_sort_order(c_list_t * c_list, c_list_num_t num, const char * str, user_defined_t user_defined);

/** @brief Insert item in list in sorted order (unique).
 *
 * Inserts item in list in sorted order.  If an item with the same
 * numeric value already exists in the list it will be replaced with
 * the values provided.
 *
 * @note The list must already be in sorted order
 *
 * @param[in] c_list    list to add item to
 * @param[in] num       integer value of item to add
 * @param[in] str       string value of item to add
 * @param[in] user_defined   user defined value of item to add
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_insert_in_sort_order_unique(c_list_t * c_list, c_list_num_t num, const char * str, user_defined_t user_defined);

/** @brief Insert item in list in alphabetical order.
 *
 * @note The list must already be in alphabetical order
 *
 * @param[in] c_list    list to add item to
 * @param[in] num       integer value of item to add
 * @param[in] str       string value of item to add
 * @param[in] user_defined   user defined value of item to add
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_insert_in_alphabetical_order(c_list_t * c_list, c_list_num_t num, const char * str, user_defined_t user_defined);

/** @brief Remove item from list.
 *
 * Remove item from list specified by the index @p idx
 *
 * @param[in] c_list    list object
 * @param[in] idx   index of item to remove
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_remove(c_list_t * c_list, size_t idx);

/** @brief Clear list.
 *
 * Clears all items from list
 *
 * @param[in] c_list    list object
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_clear(c_list_t * c_list);

/** @brief Length of list
 *
 * @param[in] c_list    list object
 *
 * @returns the number of items in the list.
 * */
size_t c_list_get_length(const c_list_t * c_list);

/** @brief Find first entry in the list
 *
 * Sets the current item index to the first item in the list and returns the
 * contents of the item.
 *
 * @param[in] c_list    list object
 * @param[out] c_list_entry pointer to list entry object that found
 * data will be copied to
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_find_first(c_list_t * c_list, c_list_entry_t * c_list_entry);

/** @brief Find last entry in the list
 *
 * Sets the current item index to the last item in the list and returns the
 * contents of the item.
 *
 * @param[in] c_list    list object
 * @param[out] c_list_entry pointer to list entry object that found
 * data will be copied to
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_find_last(c_list_t * c_list, c_list_entry_t * c_list_entry);

/** @brief Find next entry in the list
 *
 * Sets the current item index to the next item in the list (based on
 * the current item index) and returns the contents of the item.
 *
 * @param[in] c_list    list object
 * @param[out] c_list_entry pointer to list entry object that found
 * data will be copied to
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_find_next(c_list_t * c_list, c_list_entry_t * c_list_entry);

/** @brief Find previous entry in the list
 *
 * Sets the current item index to the previous item in the list (based on
 * the current item index) and returns the contents of the item.
 *
 * @param[in] c_list    list object
 * @param[out] c_list_entry pointer to list entry object that found
 * data will be copied to
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_find_prev(c_list_t * c_list, c_list_entry_t * c_list_entry);

/** @brief Get entry details by index.
 *
 * Get entry details for an entry by index
 *
 * @param[in] c_list            list object
 * @param[in] idx               index of item to get
 * @param[out] c_list_entry     pointer to list entry object to copy
 * data to
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_get_entry(const c_list_t * c_list, size_t idx, c_list_entry_t * c_list_entry);

/** @brief Set entry details by index.
 *
 * Replace the given entry (specified by the index @p idx) with the
 * data contained in @p c_list_entry.
 *
 * @param[in] c_list        list_object
 * @param[in] idx           index of item to replace
 * @param[in] c_list_entry  pointer to entry object containing new data
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_set_entry(c_list_t * c_list, size_t idx, const c_list_entry_t * c_list_entry);

/** @brief Get current entry details.
 *
 * Get current entry details
 *
 * @param[in] c_list            list object
 * @param[out] c_list_entry     pointer to list entry object to copy
 * data to
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_get_current_entry(const c_list_t * c_list, c_list_entry_t * c_list_entry);

/** @brief Set details of current entry.
 *
 * Replace the current entry with the data contained in @p c_list_entry.
 *
 * @param[in] c_list        list object
 * @param[in] c_list_entry  pointer to entry object containing new data
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_set_current_entry(c_list_t * c_list, const c_list_entry_t * c_list_entry);

/** @brief Get index of current entry.
 *
 * @param[in] c_list    list object
 * @param[out] idx      index of current object
 *
 * @returns C_LIST_SUCCESS on success.*/
c_list_error_t c_list_get_index(const c_list_t * c_list, size_t * idx);

/** @brief Set current entry by index;
 *
 * @param[in] c_list    list object
 * @param[in] idx       index of new current object
 *
 * @returns C_LIST_SUCCESS on success.*/
c_list_error_t c_list_set_index(c_list_t * c_list, size_t idx);

/** @brief Find entry by number.
 *
 * Finds the entry in the list that matches the specified number @p num (using
 * the criteria set by @p find).  The entry, if found, is set as the
 * current entry.
 *
 * @param[in] c_list            list object
 * @param[in] num               number to find
 * @param[out] c_list_entry     pointer to entry object to copy data to
 * @param[in] find              search criteria
 *
 * @returns C_LIST_SUCCESS if a match is found.
 * */
c_list_error_t c_list_find_num(c_list_t * c_list, c_list_num_t num, c_list_entry_t * c_list_entry, c_list_find_t find);

/** @brief Find entry by string.
 *
 * Finds the entry in the list that matches the specified string @p
 * str. The entry, if found, is set as the current entry.
 *
 * @param[in] c_list            list object
 * @param[in] str               string to find
 * @param[out] c_list_entry     pointer to entry object to copy data to
 *
 * @returns C_LIST_SUCCESS if a match is found.
 * */
c_list_error_t c_list_find_str(c_list_t * c_list, const char * str, c_list_entry_t * c_list_entry);

/** @brief Find entry by partial string.
 *
 * Finds the entry in the list that matches the specified string @p
 * str up to the length @p len. The entry, if found, is set as the current entry.
 *
 * @param[in] c_list            list object
 * @param[in] str               string to find
 * @param[in] len               length of characters to compare
 * @param[out] c_list_entry     pointer to entry object to copy data to
 *
 * @returns C_LIST_SUCCESS if a match is found.
 * */
c_list_error_t c_list_find_strn(c_list_t * c_list, const char * str, size_t len, c_list_entry_t * c_list_entry);

/** @brief Create list using min, max, step.
 *
 * Create a list given a @p min value, @p max value and @p step size.
 * The string for each entry will be generated using the @p divider and
 * @p precision parameters.  For example, if @p divider is 1000 and @p
 * precision is 3, the value 1000 will be represented as "1.000".
 * Furthermore, each string will have the @p prefix and @p postfix
 * added to the string.
 *
 * @note: The list will be cleared of any existing data before being
 * filled with new data.
 *
 * @param[in] c_list        list object
 * @param[in] min           minimum value in list
 * @param[in] max           maximum value in list
 * @param[in] step          step size used to create values between @p
 * min and @p max
 * @param[in] divider       divider used to create string
 * @param[in] precision     precision (number of digits after decimal
 * point) used to create string
 * @param[in] postfix       string to the appended to end of string
 * @param[in] prefix        string to be prepended to beginning of
 * string
 *
 * @return C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_fill_generic(c_list_t * c_list, int min, int max, int step, int divider, int precision, const char * postfix, const char * prefix);

/** @brief Decode all decorated strings in list.
 *
 *  Decorated string contain special characters encoded using the HTML
 *  Entities syntax.  That is, \&special;.  This function will decode
 *  all the strings and replace the special characters with ASCII
 *  equivalents.  For example, \&red1over; will be replaced with "1/".
 *
 *  @param[in] c_list   list object
 *
 *  @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_decode(const c_list_t * c_list);

/** @brief Convert string to list.
 *
 * This function will import a list from a string previously created by
 * @ref c_list_export_to_string.
 *
 * @note All contents of the list will be cleared before importing form
 * the string.
 *
 * @param[in] c_list    list object
 * @param[in] str       string to import
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_import_from_string(c_list_t * c_list, const char * str);

/** @brief Convert list to a single string.
 *
 * This function will convert the list into a single string that can be
 * converted back into a list using @ref c_list_import_from_string.
 *
 * @note The user_defined data for each entry is not part of the created
 * string.
 *
 * @param[in] c_list    list_object
 * @param[out] str      buffer to hold output string
 * @param[in] size      size of @p str
 *
 * @returns C_LIST_SUCCESS on success.
 * */
c_list_error_t c_list_export_to_string(const c_list_t * c_list, char * str, size_t size);
c_list_error_t c_list_export_to_string_ext(const c_list_t * c_list, char * str, size_t size, int force_compression);

/** @brief Compare two lists.
 *
 * Two lists are considered equal if all their entries are identical
 * and they have the same "current" entry.
 *
 * @param[in] c_list_1  first list to compare
 * @param[in] c_list_2  second list to compare
 *
 * @return 1 if two lists are equal, 0 otherwise.
 * */
int c_list_compare(const c_list_t * c_list_1, const c_list_t * c_list_2);

#ifdef __cplusplus
}
#endif

#endif
