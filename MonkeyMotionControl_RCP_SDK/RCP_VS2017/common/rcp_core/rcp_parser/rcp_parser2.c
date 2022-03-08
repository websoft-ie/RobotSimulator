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

#include "rcp_parser2.h"
#include <stdlib.h>
#include <string.h>

/* Convert two ASCII characters to number (ASCIIHEX to short) */
static unsigned char ascii2hex16b(const char * valstr);

/* Convert number to two chars */
static void hex2ascii16b(unsigned short num, char * valstr);

static int _RCP_get_and_parse_packet_ext(tRCPParsedPacketState * packetState, char nextChar, int destructive);

#ifdef RCP2_CHECK_BUFFER_OVERFLOW
static int RCP_noBufferRoom(const tRCP * pRcpData, int increase);
static int RCP_getStringSize(const char * pBuffer);
#endif

char * RCP_copyStringChar(char * dst, const char * src, const char * end);

/* Receive Functions */
/* ----------------- */
int RCP_prepareForParsing(tRCP * pRcpData, char * pBuffer, int bufferLength)
{
    /* Setup RCP Structure */
    pRcpData->pBufferPos = pBuffer;
    pRcpData->pParserPos = pBuffer;
    pRcpData->bufferLength = bufferLength;
    pRcpData->pTargetID = 0;
    pRcpData->pSourceID = 0;

    return RCP2_PARSER_OK;
}

/* WARNING: checksum characters are not properly escaped. A sequence of
 * ':*' in a list will be */
/* misinterpreted as a checksum. Just don't do it ;-) */
int RCP_validateChecksum(const tRCP * pRcpData)
{
    unsigned char result = 0;
    unsigned char checksum;
    const char * pScanPos = (char *) (pRcpData->pBufferPos + 1);

    /*  -1 as an index is nasty but it is guaranteed that there is
     *  always a start of message marker before */
    /*  the first character in our array. saves a bunch of code.... */
    while (!((pScanPos[-1] == RCP2_MSG_SEPARATOR_SYMBOL) && (pScanPos[0] == RCP2_MSG_CKSUM_SYMBOL)))
    {
        /* End of Message? Too many characters? No way out.... */
        if (*pScanPos == RCP2_MSG_END1_SYMBOL)
            return RCP2_PARSER_NO_CHECKSUM;

#ifdef RCP2_CHECK_BUFFER_OVERFLOW
        if (pScanPos == (char *) (pRcpData->pBufferPos + pRcpData->bufferLength))
            return RCP2_PARSER_BUFFER_END;
#endif

        result = result ^ *pScanPos;
        pScanPos++;
    }
    /* skip checksum marker */
    pScanPos++;

    /* we got our checksum... now we interpret the next two characters
     * as ASCII hex */
    checksum = ascii2hex16b(pScanPos);

    /* Validate checksum */
    if (checksum != result)
        return RCP2_PARSER_CHECKSUM_MISSMATCH;
    else
        return RCP2_PARSER_OK;
}

int RCP_validateChecksumBinary(const tRCP * pRcpData)
{
    unsigned char result = 0;
    unsigned char checksum;
    const char * pScanPos = (char *) (pRcpData->pBufferPos + 1);

    /*  skip last 4 bytes as it consists of 1 byte of checksum marker,
     *  2 bytes of checksum, and the last byte of end symbol */
    int checksumdatalen = pRcpData->bufferLength - 4;

    /* check checksum marker */
    if (RCP2_MSG_CKSUM_SYMBOL == (pRcpData->pBufferPos[checksumdatalen]))
    {
        /*  Decrease by one as Start symbol is not used for checksum
         *  calculation */
        checksumdatalen--;
        /*  calculate checksum */
        while (0 < checksumdatalen--)
        {
            result = result ^ *pScanPos;
            pScanPos++;
        }
        /* skip checksum marker */
        pScanPos++;

        /* we got our checksum... now we interpret the next two
         * characters as ASCII hex */
        checksum = ascii2hex16b(pScanPos);

        /* Validate checksum */
        if (checksum != result)
            return RCP2_PARSER_CHECKSUM_MISSMATCH;
        else
            return RCP2_PARSER_OK;
    }
    else
    {
        return RCP2_PARSER_NO_CHECKSUM;
    }
}

int RCP_parseHeader(tRCP * pRcpData, const char * pDeviceID, char * pCmd)
{
    const char * pScanPos;
    const char * pCmdToken;

    /* Validate valid Marker */
    if (*pRcpData->pParserPos != RCP2_MSG_START_SYMBOL)
        return RCP2_PARSER_NO_HEADER;

    /* If the first parameter is a target ID we set the pointer */
    pRcpData->pParserPos++;
    if (*pRcpData->pParserPos == RCP2_MSG_TARGET_SYMBOL)
        pRcpData->pTargetID = pRcpData->pParserPos + 1;

    /* Look for start of source id */
    /* TODO: we should check for end of message characters so we do not
     * loop endlessly on data... */
    while (*pRcpData->pParserPos != RCP2_MSG_SOURCE_SYMBOL)
    {
        pRcpData->pParserPos++;

#ifdef RCP2_CHECK_BUFFER_OVERFLOW
        if (pRcpData->pParserPos == (pRcpData->pBufferPos + pRcpData->bufferLength))
            return RCP2_PARSER_BUFFER_END;
#endif
    }

    /* Replace symbol with end of string */
    *pRcpData->pParserPos = '\0';

    /* Extract Source ID */
    pRcpData->pSourceID = RCP_getNextToken(pRcpData);

    /* Extract Command */
    if (0 == (pCmdToken = RCP_getNextToken(pRcpData)))
    {
        return RCP2_PARSER_NO_COMMAND;
    }
    *pCmd = *pCmdToken;

    /* Validate Device ID */
    if (pRcpData->pTargetID)
    {
        pScanPos = pRcpData->pTargetID;
        /* Compare with Device ID */
        while (*pDeviceID != '\0')
        {
            if (*pScanPos != *pDeviceID)
                return RCP2_PARSER_WRONG_ID;
            pScanPos++;
            pDeviceID++;
        }
    }
    return RCP2_PARSER_OK;
}

char * RCP_getNextToken(tRCP * pRcpData)
{
    char * pCopyPos;
    char * pStringPos;

    /* Check for end of Token list */
    if ((*pRcpData->pParserPos == RCP2_MSG_END1_SYMBOL) || (*pRcpData->pParserPos == RCP2_MSG_CKSUM_SYMBOL))
        return 0;

    /* Skip first character as it is a separator */
    pRcpData->pParserPos++;

    /* Init Copy Position */
    pCopyPos = pRcpData->pParserPos;
    pStringPos = pRcpData->pParserPos;

    /* Loop through token until we hit a separator */
    do
    {
        /* Deal with Escape characters */
        if (*pRcpData->pParserPos == RCP2_MSG_ESCAPE_SYMBOL)
            pRcpData->pParserPos++;

        /* Copy character */
        *pCopyPos = *pRcpData->pParserPos;

        /* Advance one character */
        pRcpData->pParserPos++;
        pCopyPos++;

#ifdef RCP2_CHECK_BUFFER_OVERFLOW
        if (pRcpData->pParserPos == (pRcpData->pBufferPos + pRcpData->bufferLength))
            return 0;
#endif
    }
    while (*pRcpData->pParserPos != RCP2_MSG_SEPARATOR_SYMBOL);

    /* Add string terminator to in-buffer string */
    *pCopyPos = '\0';
    return pStringPos;
}

/* Send Functions */
/* -------------- */

int RCP_buildHeader(tRCP * pRcpData, char * pBuffer, int bufferLength, const char * pSourceID, const char * pTargetID, char Cmd)
{
    /* Setup RCP Structure */
    pRcpData->pBufferPos = pBuffer;
    pRcpData->pParserPos = pBuffer;
    pRcpData->bufferLength = bufferLength;

    /* Fill in Header into Buffer */
#ifdef RCP2_CHECK_BUFFER_OVERFLOW
    /* Check if we have room for the header */
    if (RCP_noBufferRoom(pRcpData, 10))
        return RCP2_PARSER_BUFFER_FULL;
#endif

    /* Add Header Start */
    *pRcpData->pParserPos++ = RCP2_MSG_START_SYMBOL;

    /* Add optional Target ID */
    if (pTargetID && *pTargetID)
    {
#ifdef RCP2_CHECK_BUFFER_OVERFLOW
        /* Check if we have room for the header */
        if (RCP_noBufferRoom(pRcpData, RCP_getStringSize(pTargetID) + 1))
            return RCP2_PARSER_BUFFER_FULL;
#endif
        /* Add Target Marker */
        *pRcpData->pParserPos++ = RCP2_MSG_TARGET_SYMBOL;
        /* Add Target ID */
        pRcpData->pParserPos = RCP_copyStringChar(pRcpData->pParserPos, pTargetID, pBuffer + bufferLength);
    }

    /* Add Source Marker */
#ifdef RCP2_CHECK_BUFFER_OVERFLOW
    /* Check if we have room for the header */
    if (RCP_noBufferRoom(pRcpData, RCP_getStringSize(pSourceID) + 1))
        return RCP2_PARSER_BUFFER_FULL;
#endif
    /* Add Source Marker */
    *pRcpData->pParserPos++ = RCP2_MSG_SOURCE_SYMBOL;
    /* Add Target ID */
    pRcpData->pParserPos = RCP_copyStringChar(pRcpData->pParserPos, pSourceID, pBuffer + bufferLength);

    /* Add Command */
#ifdef RCP2_CHECK_BUFFER_OVERFLOW
    /* Check if we have room for the header */
    if (RCP_noBufferRoom(pRcpData, 2))
        return RCP2_PARSER_BUFFER_FULL;
#endif
    *pRcpData->pParserPos++ = RCP2_MSG_SEPARATOR_SYMBOL;
    *pRcpData->pParserPos++ = Cmd;
    *pRcpData->pParserPos++ = RCP2_MSG_SEPARATOR_SYMBOL;

    return RCP2_PARSER_OK;
}

int RCP_addToken(tRCP * pRcpData, const char * pToken)
{
    /* Loop through token we are adding */
    while (*pToken != '\0')
    {
        /* Deal with Escape Characters */
        if (
            (*pToken == RCP2_MSG_START_SYMBOL) ||
            (*pToken == RCP2_MSG_TARGET_SYMBOL) ||
            (*pToken == RCP2_MSG_SOURCE_SYMBOL) ||
            (*pToken == RCP2_MSG_SEPARATOR_SYMBOL) ||
            (*pToken == RCP2_MSG_CKSUM_SYMBOL) ||
            (*pToken == RCP2_MSG_ESCAPE_SYMBOL)
           )
        {
#ifdef RCP2_CHECK_BUFFER_OVERFLOW
            /* Check if we have room for escape character */
            if (RCP_noBufferRoom(pRcpData, 1))
                return RCP2_PARSER_BUFFER_FULL;
#endif
            *(pRcpData->pParserPos++) = RCP2_MSG_ESCAPE_SYMBOL;
        }
#ifdef RCP2_CHECK_BUFFER_OVERFLOW
        /* Check if we have room for character */
        if (RCP_noBufferRoom(pRcpData, 1))
            return RCP2_PARSER_BUFFER_FULL;
#endif
        /* Add character */
        *(pRcpData->pParserPos++) = *pToken;

        /* Move us forward... */
        pToken++;
    }

    /* Add Separator */
#ifdef RCP2_CHECK_BUFFER_OVERFLOW
    /* Check if we have room for the header */
    if (RCP_noBufferRoom(pRcpData, 1))
        return RCP2_PARSER_BUFFER_FULL;
#endif
    *(pRcpData->pParserPos++) = RCP2_MSG_SEPARATOR_SYMBOL;

    return RCP2_PARSER_OK;
}

int RCP_addChecksum(tRCP * pRcpData)
{
    unsigned char result = 0;
    const char * pBufPos = pRcpData->pBufferPos + 1;

#ifdef RCP2_CHECK_BUFFER_OVERFLOW
    /* Check if we have room for checksum */
    if (RCP_noBufferRoom(pRcpData, 3))
        return RCP2_PARSER_BUFFER_FULL;
#endif

    /* Calculate Checksum */
    while (pBufPos != pRcpData->pParserPos)
    {
        result = result ^ *pBufPos;
        pBufPos++;
    }
    /* Append Checksum marker */
    *(pRcpData->pParserPos++) = RCP2_MSG_CKSUM_SYMBOL;

    /* Append Checksum */
    hex2ascii16b(result, pRcpData->pParserPos++);

    pRcpData->pParserPos++;
    return RCP2_PARSER_OK;
}

int RCP_finalizeMessage(tRCP * pRcpData)
{
#ifdef RCP2_CHECK_BUFFER_OVERFLOW
    /* Check if we have room for termination */
    if (RCP_noBufferRoom(pRcpData, 2))
        return RCP2_PARSER_BUFFER_FULL;
#endif

    /* Append termination character */
    *(pRcpData->pParserPos++) = RCP2_MSG_END1_SYMBOL;

    /* Just to make life easier we terminate the string here... */
    *(pRcpData->pParserPos++) = '\0';

    /* Return final size */
    return (int) ((pRcpData->pParserPos - 1) - pRcpData->pBufferPos);  /* While calculating size, do not include '\0' at the end of RCP message. */
}

/* Wrapper Function */
/* ---------------- */

int RCP_buildMessage(char * pBuffer, int maxSize, const char * pSource, const char * pTarget, char cmd, const char * pParam, const char * pValue, char addChecksum)
{
    tRCP rcpBuffer;
    int returnVal;

    /* Lets create a header */
    returnVal = RCP_buildHeader(&rcpBuffer, pBuffer, maxSize, pSource, pTarget, cmd);
    if (returnVal != RCP2_PARSER_OK)
        return returnVal;

    /* Add Param */
    returnVal = RCP_addToken(&rcpBuffer, pParam);
    if (returnVal != RCP2_PARSER_OK)
        return returnVal;

    /* Add Value */
    returnVal = RCP_addToken(&rcpBuffer, pValue);
    if (returnVal != RCP2_PARSER_OK)
        return returnVal;

    /* Add Optional checksum */
    if (addChecksum)
    {
        returnVal = RCP_addChecksum(&rcpBuffer);
        if (returnVal != RCP2_PARSER_OK)
            return returnVal;
    }

    /* Finalize */
    return RCP_finalizeMessage(&rcpBuffer);
}

int RCP_parseMessage(char * pBuffer, int bufferSize, const char * myDeviceId, char * * senderID, char * pCmd, char * * pParam, char * * pValue)
{
    tRCP rcpBuffer;
    int returnVal;

    /* Prepare for Parsing */
    returnVal = RCP_prepareForParsing(&rcpBuffer, pBuffer, bufferSize);
    if (returnVal != RCP2_PARSER_OK)
        return returnVal;

    /* Validate checksum */
    returnVal = RCP_validateChecksumBinary(&rcpBuffer);
    if ((returnVal != RCP2_PARSER_NO_CHECKSUM) && (returnVal != RCP2_PARSER_OK))
        return returnVal;

    /* Parse Header */
    returnVal = RCP_parseHeader(&rcpBuffer, myDeviceId, pCmd);
    if (returnVal != RCP2_PARSER_OK)
        return returnVal;

    *senderID = rcpBuffer.pSourceID;
    /* Extract Parameter */
    if (0 == (*pParam = RCP_getNextToken(&rcpBuffer)))
    {
        *pValue = 0;
        return RCP2_PARSER_NO_PARAMETER;
    }

    /* Extract Value */
    if (0 == (*pValue = RCP_getNextToken(&rcpBuffer)))
        return RCP2_PARSER_NO_VALUE;

    return RCP2_PARSER_OK;
}

/* Helper functions */
/* ---------------- */
int RCP_getVersion(void)
{
    return (int) RCP2_VERSION;
}

static unsigned char ascii2hex16b(const char * valstr)
{
    unsigned char val;

    if (valstr[0] <= '9')
        val = valstr[0] - '0';
    else
        val = 10 + (valstr[0] - 'A');

    val *= 16;

    if (valstr[1] <= '9')
        val += valstr[1] - '0';
    else
        val += 10 + (valstr[1] - 'A');

    return val;
}

static void hex2ascii16b(unsigned short num, char * valstr)
{
    /* Append as string */
    int digit = num / 16;
    if (digit < 10)
        valstr[0] = (char) ('0' + digit);
    else
        valstr[0] = (char) ('A' + (digit - 10));

    digit = num % 16;
    if (digit < 10)
        valstr[1] = (char) ('0' + digit);
    else
        valstr[1] = (char) ('A' + (digit - 10));
}

/* Specific copy.... Copy string characters (Not terminator) */
/* up to a specific length.... */
char * RCP_copyStringChar(char * dst, const char * src, const char * end)
{
    do
    {
        *dst = *src;
        src++;
        dst++;
    }
    while ((*src != '\0') && (dst <= end));
    return dst;
}

#ifdef RCP2_CHECK_BUFFER_OVERFLOW
static int RCP_getStringSize(const char * pBuffer)
{
    int ii = 0;
    while (pBuffer[ii] != '\0')
        ii++;

    return ii;
}

static int RCP_noBufferRoom(const tRCP * pRcpData, int increase)
{
    if (((pRcpData->pParserPos - pRcpData->pBufferPos) + increase) > pRcpData->bufferLength)
        return 1;
    else
        return 0;
}
#endif

const char * RCP_get_packet(tRCPPacketState * packetState, char nextChar, int * numBytes)
{
    if (!packetState)
    {
        return 0;
    }

    if (packetState->len >= (packetState->buf_len - 2))
    {
        /*  RCP receive buffer full */
        packetState->len = packetState->buf_len - 2;
    }

    if (packetState->escaped && packetState->in_packet)
    {
        /* This character has no special meaning */
        packetState->buf[packetState->len++] = nextChar;
        packetState->escaped = 0;
        if (numBytes)
        {
            *numBytes = 0;
        }
        return 0;
    }

    if (packetState->skip_bytes && packetState->in_packet)
    {
        packetState->buf[packetState->len++] = nextChar;
        packetState->skip_bytes--;
        if (numBytes)
        {
            *numBytes = 0;
        }
        return 0;
    }

    switch (nextChar)
    {
        case RCP2_MSG_START_SYMBOL:
            packetState->len = 0;
            packetState->buf[packetState->len++] = nextChar;
            packetState->in_packet = 1;
            packetState->is_binary = 0;
            packetState->skip_bytes = 0;
            packetState->parsed.pCmd = 0;
            packetState->parsed.pTarget = 0;
            packetState->parsed.pSource = 0;
            packetState->parsed.pParam = 0;
            packetState->parsed.pArg = 0;
            packetState->parsed.pExtra = 0;
            break;

        case RCP2_MSG_END1_SYMBOL:
        case RCP2_MSG_END2_SYMBOL:
            if (packetState->in_packet)
            {
                packetState->buf[packetState->len++] = RCP2_MSG_END1_SYMBOL;
                packetState->buf[packetState->len++] = 0;
                if (numBytes)
                {
                    *numBytes = packetState->len - 1;
                }
                packetState->len = 0;
                packetState->in_packet = 0;
                return packetState->buf;
            }
            break;

        case RCP2_MSG_ESCAPE_SYMBOL:
            if (packetState->in_packet)
            {
                packetState->buf[packetState->len++] = nextChar;
                packetState->escaped = 1;
            }
            break;

        default:
            if (packetState->in_packet)
            {
                packetState->buf[packetState->len++] = nextChar;

                switch (nextChar)
                {
                    case RCP2_MSG_TARGET_SYMBOL:
                        packetState->parsed.pTarget = &(packetState->buf[packetState->len]);
                        break;

                    case RCP2_MSG_SOURCE_SYMBOL:
                        packetState->parsed.pSource = &(packetState->buf[packetState->len]);
                        break;

                    case RCP2_MSG_SEPARATOR_SYMBOL:
                        if (!packetState->parsed.pCmd)
                        {
                            packetState->parsed.pCmd = &(packetState->buf[packetState->len]);
                        }
                        else if (!packetState->parsed.pParam)
                        {
                            packetState->parsed.pParam = &(packetState->buf[packetState->len]);
                        }
                        else if (!packetState->parsed.pArg)
                        {
                            packetState->parsed.pArg = &(packetState->buf[packetState->len]);

                            if (packetState->parsed.pCmd[0] == 'S' &&
                                packetState->parsed.pParam[0] == 'B' &&
                                packetState->parsed.pParam[1] == 'I' &&
                                packetState->parsed.pParam[2] == 'N' &&
                                packetState->parsed.pParam[3] == ':')
                            {
                                /* This is a binary packet */
                                packetState->is_binary = 1;
                            }
                            break;
                        }
                        else if (!packetState->parsed.pExtra)
                        {
                            packetState->parsed.pExtra = &(packetState->buf[packetState->len]);

                            if (packetState->is_binary)
                            {
                                int digits = 0;
                                packetState->skip_bytes = 0;
                                while ((packetState->parsed.pArg[digits] >= '0') && (packetState->parsed.pArg[digits] <= '9'))
                                {
                                    packetState->skip_bytes *= 10;
                                    packetState->skip_bytes += packetState->parsed.pArg[digits] - '0';
                                    digits++;
                                }
                            }
                            break;
                        }
                        break;
                }
            }
            break;
    }
    if (numBytes)
    {
        *numBytes = 0;
    }
    return 0;
}

int RCP_strcmp(const char * s1, const char * s2)
{
    for (;; )
    {
        if (
            (!*s1 || *s1 == RCP2_MSG_SEPARATOR_SYMBOL || *s1 == RCP2_MSG_SOURCE_SYMBOL) &&
            (!*s2 || *s2 == RCP2_MSG_SEPARATOR_SYMBOL || *s2 == RCP2_MSG_SOURCE_SYMBOL)
           )
        {
            return 0;
        }

        if (*s1 == RCP2_MSG_ESCAPE_SYMBOL)
        {
            s1++;
        }

        if (*s2 == RCP2_MSG_ESCAPE_SYMBOL)
        {
            s2++;
        }

        if (*s1 != *s2)
        {
            return *(const unsigned char *) s1 - *(const unsigned char *) s2;
        }

        s1++;
        s2++;
    }
}

size_t RCP_strlcpy(char * dst, const char * src, size_t siz)
{
    size_t retval = 0;
    char * d = dst;
    const char * s = src;
    size_t n = siz;

    if (!dst)
    {
        return 0;
    }

    if (!src)
    {
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
            if (*s == '\0' || *s == RCP2_MSG_SEPARATOR_SYMBOL || *s == RCP2_MSG_SOURCE_SYMBOL)
            {
                *d++ = '\0';
                break;
            }

            if (*s == RCP2_MSG_ESCAPE_SYMBOL)
            {
                s++;
            }

            *d++ = *s++;
            retval++;
        }
    }

    /* Not enough room in dst, add NUL and traverse rest of src */
    if (n == 0)
    {
        if (siz != 0)
        {
            *d = '\0';      /* NUL-terminate dst */
        }
        while (*s != '\0' && *s != RCP2_MSG_SEPARATOR_SYMBOL && *s != RCP2_MSG_SOURCE_SYMBOL)
        {
            if (*s == RCP2_MSG_ESCAPE_SYMBOL)
            {
                s++;
            }

            s++;
            retval++;
        }
    }

    return retval;
}

int RCP_get_and_parse_packet(tRCPParsedPacketState * packetState, char nextChar)
{
    return _RCP_get_and_parse_packet_ext(packetState, nextChar, 1);
}

int RCP_get_and_parse_packet_non_destructive(tRCPParsedPacketState * packetState, char nextChar)
{
    return _RCP_get_and_parse_packet_ext(packetState, nextChar, 0);
}

static int _RCP_get_and_parse_packet_ext(tRCPParsedPacketState * packetState, char nextChar, int destructive)
{
    tRCPState next_state;
    int delim_start = 0;
    int delim_target = 0;
    int delim_source = 0;
    int delim_separator = 0;
    int delim_checksum = 0;
    int delim_end = 0;
    int delim = 0;
    int successful = 0;

    /* check for valid packet state pointer */
    if (!packetState)
    {
        return RCP2_PARSER_INCOMPLETE_PACKET;
    }

    /* initialize packet state the first time */
    if (!packetState->initialized)
    {
        packetState->initialized = 1;
        packetState->len = 0;
        packetState->is_escaped = 0;
        packetState->is_binary = 0;
        packetState->last_error = RCP2_PARSER_INCOMPLETE_PACKET;
        packetState->cur_state = RCP2_STATE_IDLE;
    }

    if (packetState->len >= packetState->buf_len - 2)
    {
        /* RCP receive buffer full */
        packetState->len = 0;
        packetState->last_error = RCP2_PARSER_BUFFER_FULL;
        /* we reset the length and continue parsing in this case.  This
         * packet is a lost cause but we don't want to miss the start
         * of the next packet.  We don't want to reset our parsing
         * flags since we want to retain the current state.  Just
         * notify the user that the buffer was full on move on. */
    }

    /* check for escape character */
    if (
        !packetState->is_escaped &&
        packetState->cur_state != RCP2_STATE_BIN_DATA &&
        nextChar == RCP2_MSG_ESCAPE_SYMBOL)
    {
        if (!destructive)
        {
            packetState->buf[packetState->len++] = nextChar;
        }
        packetState->checksum ^= nextChar;
        packetState->is_escaped = 1;
        return packetState->last_error;
    }

    /* check for delimiters */
    if (!packetState->is_escaped && packetState->cur_state != RCP2_STATE_BIN_DATA)
    {
        switch (nextChar)
        {
            case RCP2_MSG_START_SYMBOL:
                delim_start = 1;
                delim = 1;
                break;

            case RCP2_MSG_TARGET_SYMBOL:
                delim_target = 1;
                delim = 1;
                break;

            case RCP2_MSG_SOURCE_SYMBOL:
                delim_source = 1;
                delim = 1;
                break;

            case RCP2_MSG_SEPARATOR_SYMBOL:
                delim_separator = 1;
                delim = 1;
                break;

            case RCP2_MSG_CKSUM_SYMBOL:
                delim_checksum = 1;
                delim = 1;
                break;

            case RCP2_MSG_END1_SYMBOL:
            case RCP2_MSG_END2_SYMBOL:
                delim_end = 1;
                delim = 1;
                break;

            default:
                break;
        }
    }
    else
    {
        packetState->is_escaped = 0;
    }

    /* state handlers */
    next_state = packetState->cur_state;
    switch (packetState->cur_state)
    {
        case RCP2_STATE_IDLE:
            if (delim_start)
            {
                if (!destructive)
                {
                    packetState->buf[packetState->len++] = nextChar;
                }
                next_state = RCP2_STATE_START;
            }
            break;

        case RCP2_STATE_START:
            if (delim_target)
            {
                if (!destructive)
                {
                    packetState->buf[packetState->len++] = nextChar;
                }
                packetState->checksum ^= nextChar;
                next_state = RCP2_STATE_TARGET;
            }
            else if (delim_source)
            {
                if (!destructive)
                {
                    packetState->buf[packetState->len++] = nextChar;
                }
                packetState->checksum ^= nextChar;
                next_state = RCP2_STATE_SOURCE;
            }
            else if (delim_start)
            {
                /* stay in this state */
            }
            else
            {
                next_state = RCP2_STATE_IDLE;
            }

            break;

        case RCP2_STATE_TARGET:
            if (delim_source)
            {
                if (destructive)
                {
                    packetState->buf[packetState->len++] = 0;
                }
                else
                {
                    packetState->buf[packetState->len++] = nextChar;
                }
                packetState->checksum ^= nextChar;
                next_state = RCP2_STATE_SOURCE;
            }
            else if (delim_start)
            {
                packetState->last_error = RCP2_PARSER_MALFORMED;
                next_state = RCP2_STATE_START;
            }
            else if (delim)
            {
                packetState->last_error = RCP2_PARSER_MALFORMED;
                next_state = RCP2_STATE_IDLE;
            }
            else
            {
                packetState->checksum ^= nextChar;
                packetState->buf[packetState->len++] = nextChar;
            }
            break;

        case RCP2_STATE_SOURCE:
        case RCP2_STATE_CMD:
        case RCP2_STATE_PARAM:
        case RCP2_STATE_BIN_LEN:
            if (delim_separator)
            {
                if (destructive)
                {
                    packetState->buf[packetState->len++] = 0;
                }
                else
                {
                    packetState->buf[packetState->len++] = nextChar;
                }
                packetState->checksum ^= nextChar;

                /* taking advantage of the fact that these states are
                 * in the same order as in the packet */
                next_state = (tRCPState) ((int) packetState->cur_state + 1);
            }
            else if (delim_start)
            {
                packetState->last_error = RCP2_PARSER_MALFORMED;
                next_state = RCP2_STATE_START;
            }
            else if (delim)
            {
                packetState->last_error = RCP2_PARSER_MALFORMED;
                next_state = RCP2_STATE_IDLE;
            }
            else
            {
                packetState->checksum ^= nextChar;
                packetState->buf[packetState->len++] = nextChar;
            }
            break;

        case RCP2_STATE_ARG:
            if (delim_separator)
            {
                if (destructive)
                {
                    packetState->buf[packetState->len++] = 0;
                }
                else
                {
                    packetState->buf[packetState->len++] = nextChar;
                }
                packetState->checksum ^= nextChar;

                packetState->parsed.argc++;

                if (packetState->parsed.argc == RCP2_MAX_ARG_COUNT)
                {
                    /* This is the last allowed argument, don't try to set the next argument
                     * pointer. */
                }
                else if (packetState->parsed.argc > RCP2_MAX_ARG_COUNT)
                {
                    /* We've seen too many arguments, return an error. */
                    packetState->parsed.argc = RCP2_MAX_ARG_COUNT;
                    packetState->last_error = RCP2_PARSER_TOO_MANY_ARGS;
                }
                else
                {
                    /* Set the next argument pointer */
                    packetState->parsed.argv[packetState->parsed.argc] = &packetState->buf[packetState->len];
                }
            }
            else if (delim_start)
            {
                packetState->last_error = RCP2_PARSER_MALFORMED;
                next_state = RCP2_STATE_START;
            }
            else if (delim_checksum)
            {
                if (!destructive)
                {
                    packetState->buf[packetState->len++] = nextChar;
                }
                next_state = RCP2_STATE_CHKSUM;
            }
            else if (delim_end)
            {
                if (!destructive)
                {
                    packetState->buf[packetState->len++] = nextChar;
                }
                next_state = RCP2_STATE_END;
            }
            else if (delim)
            {
                packetState->last_error = RCP2_PARSER_MALFORMED;
                next_state = RCP2_STATE_IDLE;
            }
            else
            {
                packetState->checksum ^= nextChar;
                packetState->buf[packetState->len++] = nextChar;
            }
            break;

        case RCP2_STATE_BIN_DATA:
            packetState->checksum ^= nextChar;
            packetState->buf[packetState->len++] = nextChar;
            if (0 == --packetState->skip_bytes)
            {
                next_state = RCP2_STATE_BIN_DONE;
            }

            break;

        case RCP2_STATE_BIN_DONE:
            if (delim_start)
            {
                packetState->last_error = RCP2_PARSER_MALFORMED;
                next_state = RCP2_STATE_START;
            }
            else if (delim_checksum)
            {
                if (!destructive)
                {
                    packetState->buf[packetState->len++] = nextChar;
                }
                next_state = RCP2_STATE_CHKSUM;
            }
            else if (delim_end)
            {
                if (!destructive)
                {
                    packetState->buf[packetState->len++] = nextChar;
                }
                next_state = RCP2_STATE_END;
            }
            else if (nextChar == 0)
            {
                /* this is just padding */
                if (!destructive)
                {
                    packetState->buf[packetState->len++] = nextChar;
                }
            }
            else
            {
                packetState->last_error = RCP2_PARSER_MALFORMED;
                next_state = RCP2_STATE_IDLE;
            }
            break;

        case RCP2_STATE_CHKSUM:
            if (delim_end)
            {
                if (destructive)
                {
                    packetState->buf[packetState->len++] = 0;
                }
                else
                {
                    packetState->buf[packetState->len++] = nextChar;
                }
                next_state = RCP2_STATE_END;
            }
            else if (delim_start)
            {
                packetState->last_error = RCP2_PARSER_MALFORMED;
                next_state = RCP2_STATE_START;
            }
            else if (delim)
            {
                packetState->last_error = RCP2_PARSER_MALFORMED;
                next_state = RCP2_STATE_IDLE;
            }
            else
            {
                packetState->buf[packetState->len++] = nextChar;
            }
            break;

        case RCP2_STATE_END:
            break;
    }

    /* handle state enter conditions */
    while (packetState->cur_state != next_state)
    {
        packetState->cur_state = next_state;

        /* handle enter condition */
        switch (packetState->cur_state)
        {
            case RCP2_STATE_IDLE:
                break;

            case RCP2_STATE_START:
                packetState->parsed.pTarget = 0;
                packetState->parsed.pSource = 0;
                packetState->is_binary = 0;
                packetState->checksum = 0;
                if (destructive)
                {
                    packetState->len = 0;
                }
                else
                {
                    packetState->len = 1;
                }
                packetState->parsed.argc = 0;
                packetState->parsed.pChecksum = 0;
                packetState->parsed.pParam = 0;
                packetState->parsed.pCmd = 0;
                packetState->parsed.argv[0] = 0;
                packetState->last_error = RCP2_PARSER_INCOMPLETE_PACKET;
                break;

            case RCP2_STATE_TARGET:
                packetState->parsed.pTarget = &packetState->buf[packetState->len];
                break;

            case RCP2_STATE_SOURCE:
                packetState->parsed.pSource = &packetState->buf[packetState->len];
                break;

            case RCP2_STATE_CMD:
                packetState->parsed.pCmd = &packetState->buf[packetState->len];
                break;

            case RCP2_STATE_PARAM:
                packetState->parsed.pParam = &packetState->buf[packetState->len];
                break;

            case RCP2_STATE_ARG:
                if (packetState->parsed.pCmd[0] == 'S' &&
                    (RCP_strcmp(packetState->parsed.pParam, "BIN") == 0 ||
                     RCP_strcmp(packetState->parsed.pParam, "BINDONE") == 0 ||
                     RCP_strcmp(packetState->parsed.pParam, "FUILCD") == 0))
                {
                    next_state = RCP2_STATE_BIN_LEN;
                }
                else
                {
                    packetState->parsed.argv[packetState->parsed.argc] = &packetState->buf[packetState->len];
                }
                break;

            case RCP2_STATE_BIN_LEN:
                packetState->parsed.argv[0] = &packetState->buf[packetState->len];
                break;

            case RCP2_STATE_BIN_DATA:
                packetState->skip_bytes = atoi(packetState->parsed.argv[0]);
                packetState->parsed.argv[1] = &packetState->buf[packetState->len];

                if (!packetState->skip_bytes)
                {
                    next_state = RCP2_STATE_BIN_DONE;
                }
                break;

            case RCP2_STATE_BIN_DONE:
                if (packetState->parsed.pCmd[0] == 'S' &&
                    (RCP_strcmp(packetState->parsed.pParam, "BIN") == 0 ||
                     RCP_strcmp(packetState->parsed.pParam, "FUILCD") == 0))
                {
                    packetState->parsed.argc = 2;
                }
                else                    /* BINDONE */
                {
                    packetState->parsed.argc = 1;
                }
                break;

            case RCP2_STATE_CHKSUM:
                packetState->parsed.pChecksum = &packetState->buf[packetState->len];
                break;

            case RCP2_STATE_END:
                next_state = RCP2_STATE_IDLE;

                if (packetState->parsed.pChecksum)
                {
                    if (packetState->checksum != ascii2hex16b(packetState->parsed.pChecksum))
                    {
                        packetState->last_error = RCP2_PARSER_CHECKSUM_MISSMATCH;
                        break;
                    }
                }

                if (destructive)
                {
                    if (!packetState->parsed.pCmd || !packetState->parsed.pCmd[0])
                    {
                        packetState->last_error = RCP2_PARSER_NO_COMMAND;
                        break;
                    }
                }
                else
                {
                    if (!packetState->parsed.pCmd || packetState->parsed.pCmd[0] == RCP2_MSG_SEPARATOR_SYMBOL)
                    {
                        packetState->last_error = RCP2_PARSER_NO_COMMAND;
                        break;
                    }
                }

                if (destructive)
                {
                    if (!packetState->parsed.pParam || !packetState->parsed.pParam[0])
                    {
                        packetState->last_error = RCP2_PARSER_NO_PARAMETER;
                        break;
                    }
                }
                else
                {
                    if (!packetState->parsed.pParam || packetState->parsed.pParam[0] == RCP2_MSG_SEPARATOR_SYMBOL)
                    {
                        packetState->last_error = RCP2_PARSER_NO_PARAMETER;
                        break;
                    }
                }

                if (packetState->last_error == RCP2_PARSER_INCOMPLETE_PACKET)
                {
                    /* if the only error up until now is an incomplete
                     * packet then we have successfully parsed a packet */
                    successful = 1;
                }
                break;
        }
    }

    if (successful)
    {
        return RCP2_PARSER_OK;
    }
    else
    {
        return packetState->last_error;
    }
}
