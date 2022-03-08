/********************************************************************************
 * This file is part of the RCP SDK Release 6.62.0
 *
 * For technical support please email rcpsdk@red.com.
 *
 ********************************************************************************/

/*	$OpenBSD: strlcpy.c,v 1.11 2006/05/05 15:27:38 millert Exp  */

/*
 * Copyright (c) 1998 Todd C. Miller <Todd.Miller@courtesan.com>
 *
 * Permission to use, copy, modify, and distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 * WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 * ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 * WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 * ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 * OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

#ifndef HAVE_STRLCPY

#include <stdlib.h>
#include <string.h>
#include "stringl/stringl.h"
#ifdef STRINGL_USE_LOGGER
#include "utils/diagnostic/log/log.h"
#endif

/*
 * Copy src to string dst of size siz.  At most siz-1 characters
 * will be copied.  Always NUL terminates (unless siz == 0).
 * Returns strlen(src); if retval >= siz, truncation occurred.
 */
size_t strlcpy(char * dst, const char * src, size_t siz)
{
    size_t retval;
    char * d = dst;
    const char * s = src;
    size_t n = siz;

    if (!dst)
    {
#ifdef STRINGL_USE_LOGGER
        log_error("strlcpy called with NULL for destination\n");
#endif
        return 0;
    }

    if (!src)
    {
#ifdef STRINGL_USE_LOGGER
        log_error("strlcpy called with NULL for source\n");
#endif
        if (siz > 0)
        {
            *d = '\0';
        }

        return 0;
    }

    /* Copy as many bytes as will fit */
    if (n != 0)
    {
        while (--n != 0)
        {
            if ((*d++ = *s++) == '\0')
                break;
        }
    }

    /* Not enough room in dst, add NUL and traverse rest of src */
    if (n == 0)
    {
        if (siz != 0)
            *d = '\0';      /* NUL-terminate dst */
        while (*s++)
            ;
    }

    retval = (s - src - 1); /* count does not include NUL */

    if (retval >= siz)
    {
#ifdef STRINGL_USE_LOGGER
        log_warning("truncation occurred in strlcpy; dest size %u; source string: %s\n", siz, src);
#endif
    }

    return retval;
}

#endif
