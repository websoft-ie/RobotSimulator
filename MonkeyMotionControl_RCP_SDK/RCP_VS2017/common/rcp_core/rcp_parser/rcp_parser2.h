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

#ifndef RCP_PARSER_2_H_
#define RCP_PARSER_2_H_

#include <stdlib.h>

/*
 *  RCP Message Format:
 *  #@[targetID]$[sourceID]:cmd:param:value[:value...]:*cksum<cr>
 *
 */

/* Options */
#define RCP2_CHECK_BUFFER_OVERFLOW

#define RCP2_VERSION   2

/* Field Length */
#define RCP2_MAX_ID_LENGTH 8        /* Maximum length of a source or target ID */
#define RCP2_MAX_CMD_LENGTH 1       /* Maximum length of a command */
#define RCP2_MAX_PARAM_LENGTH 8     /* Maximum length of the parameter name */
#define RCP2_MAX_VALUE_LENGTH (8 * 1024)  /* Maximum length of all concatenated values (including separators between values, but not trailing separator) */
#define RCP2_MAX_HEADER_LENGTH (1 + 1 + RCP2_MAX_ID_LENGTH + 1 + RCP2_MAX_ID_LENGTH + 1) /* Maximum length of header (up to, but not including separator after source id) */
#define RCP2_MAX_PACKET_LENGTH (RCP2_MAX_HEADER_LENGTH + 1 + RCP2_MAX_CMD_LENGTH + 1 + RCP2_MAX_VALUE_LENGTH + 1 + 1 + 2 + 1)

#define RCP2_MAX_ARG_COUNT 30

/* Symbol offsets */
#define RCP2_MSG_START_SYMBOL '#'
#define RCP2_MSG_TARGET_SYMBOL '@'
#define RCP2_MSG_SOURCE_SYMBOL '$'
#define RCP2_MSG_SEPARATOR_SYMBOL ':'
#define RCP2_MSG_CKSUM_SYMBOL '*'
#define RCP2_MSG_END1_SYMBOL  '\n'
#define RCP2_MSG_END2_SYMBOL  '\r'
#define RCP2_MSG_ESCAPE_SYMBOL '\\'

/* RCP Errors */
#define RCP2_PARSER_OK                  0
#define RCP2_PARSER_NO_HEADER           -1
#define RCP2_PARSER_NO_CHECKSUM         -2
#define RCP2_PARSER_CHECKSUM_MISSMATCH  -3
#define RCP2_PARSER_WRONG_ID            -4
#define RCP2_PARSER_NO_PARAMETER        -5
#define RCP2_PARSER_NO_VALUE            -6
#define RCP2_PARSER_BUFFER_FULL         -7
#define RCP2_PARSER_BUFFER_END          -8
#define RCP2_PARSER_NO_COMMAND          -9
#define RCP2_PARSER_INCOMPLETE_PACKET   -10
#define RCP2_PARSER_TOO_MANY_ARGS       -11
#define RCP2_PARSER_MALFORMED           -12
#define RCP2_PARSER_FIELD_TOO_LARGE     -13
#define RCP2_PARSER_FIELD_NO_BYTES      -14

/* RCP Commands */
#define RCP2_CMD_SET                'S'     /* Set command */
#define RCP2_CMD_GET                'G'     /* Get command */
#define RCP2_CMD_CURRENT            'C'     /* Current command */
#define RCP2_CMD_SET_LIST           'T'     /* Set List command */
#define RCP2_CMD_GET_LIST           'H'     /* Get List command */
#define RCP2_CMD_CURRENT_LIST       'D'     /* Current List command */
#define RCP2_CMD_SET_RELATIVE       'U'     /* Set Relative command */
#define RCP2_CMD_SET_LIST_RELATIVE  'V'     /* Set List Relative command */
#define RCP2_CMD_GET_PERIODIC       'I'     /* Get Periodic command - used to turn periodic updates on/off */
#define RCP2_CMD_GET_DEFAULT        'J'     /* Get default command */
#define RCP2_CMD_CURRENT_DEFAULT    'E'     /* Current default command */
#define RCP2_CMD_FAIL               'F'     /* Fail */

typedef struct
{
    char * pSourceID;            /* Pointer to Sender ID */
    char * pTargetID;            /* Pointer to Target ID or NULL for a broadcast */
    char * pBufferPos;           /* Position of Buffer */
    char * pParserPos;           /* Current Parser position in Buffer */
    int bufferLength;            /* Maximum buffer length */
} tRCP;

/* for get_packet */
typedef struct
{
    char * pCmd;
    char * pTarget;
    char * pSource;
    char * pParam;

    char * pArg;
    char * pExtra;
} tRCPParsedPacket;

typedef struct
{
    unsigned int len;
    char * buf;                         /* must be provided by caller */
    unsigned int buf_len;               /* must be provided by caller */
    int escaped;
    int in_packet;

    tRCPParsedPacket parsed;
    int is_binary;
    int skip_bytes;
} tRCPPacketState;

/* for get_and_parse_packet */
typedef struct
{
    char * pCmd;
    char * pTarget;
    char * pSource;
    char * pParam;

    char * argv[RCP2_MAX_ARG_COUNT];
    char * pChecksum;
    int argc;
} tRCPParsedPacket2;

/* for get_and_parse_packet */
typedef enum
{
    RCP2_STATE_IDLE,
    RCP2_STATE_START,
    RCP2_STATE_TARGET,
    RCP2_STATE_SOURCE,
    RCP2_STATE_CMD,
    RCP2_STATE_PARAM,
    RCP2_STATE_ARG,
    RCP2_STATE_BIN_LEN,
    RCP2_STATE_BIN_DATA,
    RCP2_STATE_BIN_DONE,
    RCP2_STATE_CHKSUM,
    RCP2_STATE_END
} tRCPState;

typedef struct
{
    char * buf;                         /* must be provided by caller */
    unsigned int buf_len;               /* must be provided by caller */
    int initialized;                    /* must be zero on first call */

    unsigned int len;
    int is_escaped;
    int is_binary;

    unsigned char checksum;
    int last_error;

    int skip_bytes;

    tRCPParsedPacket2 parsed;

    tRCPState cur_state;
} tRCPParsedPacketState;

#ifdef __cplusplus
extern "C"
{
#endif

/* Receive Functions
 * ----------------- */

/* Set initial values for Parsing (We need to initialize the parser so we can run
 * the checksum validation.
 */
int RCP_prepareForParsing(tRCP * pRcpData, char * pBuffer, int bufferLength);

/* Validate checksum: calculates the checksum for a message and compares it to the message
 * checksum if one is present. Returns 0 on success or an error.
 * Checksum check needs to be run before we parse the data as that modifies the buffer.
 */
int RCP_validateChecksum(const tRCP * pRcpData);

/* Validate binary checksum: calculates the binary checksum for a message and compares it to the message
 * checksum if one is present. Returns 0 on success or an error.
 * Checksum check needs to be run before we parse the data as that modifies the buffer.
 */
int RCP_validateChecksumBinary(const tRCP * pRcpData);

/* Parses the header and fills in the RCP structure. This structure needs to be kept and
 * Passed on to the next functions.
 */
int RCP_parseHeader(tRCP * pRcpData, const char * pDeviceID, char * pCmd);

/* Get the next token. Returns a string or NULL if it was the last one
 * Notice that we do not distinguish between parameters and values, it is
 * up to the caller to determine what he expects next
 */
char * RCP_getNextToken(tRCP * pRcpData);

/* Send Functions
 * -------------- */

/* Build a header in the buffer provided based on the data provided.
 * The Data will be filled in to the RCP structure which needs to be passed on to the next functions
 */
int RCP_buildHeader(tRCP * pRcpData, char * pBuffer, int bufferLength, const char * pSourceID, const char * pTargetID, char Cmd);

/* Add a token to the message. Escape characters will be automatically added. */
int RCP_addToken(tRCP * pRcpData, const char * pToken);

/* Add the optional checksum */
int RCP_addChecksum(tRCP * pRcpData);

/* Finalize message and add a checksum to the message if addChecksum is true.
 * Returns the final size of the message
 */
int RCP_finalizeMessage(tRCP * pRcpData);

/* Convenient Wrapper Functions
 * ---------------------------- */

/* Create a Param/Value Pair Message */
int RCP_buildMessage(char * pBuffer, int maxSize, const char * pSource, const char * pTarget, char cmd, const char * pParam, const char * pValue, char addChecksum);

/* Parse a Param/Value Pair Message */
int RCP_parseMessage(char * pBuffer, int bufferSize, const char * myDeviceId, char * * senderID, char * pCmd, char * * pParam, char * * pValue);

/* Helper Routines
 * --------------- */

/* designed to be used with RCP_get_and_parse_packet_non_destructive.
 * Behaves like strcmp but handles RCP escaping and non-NULL
 * terminatation (strings can be
 * RCP2_MSG_SEPARATOR_SYMBOL/RCP2_MSG_SOURCE_SYMBOL terminated. */
int RCP_strcmp(const char * s1, const char * s2);

/* designed to be used with RCP_get_and_parse_packet_non_destructive.
 * Behaves like strlcpy but handles RCP escaping and non-NULL
 * terminatation for src (strings can be
 * RCP2_MSG_SEPARATOR_SYMBOL/RCP2_MSG_SOURCE_SYMBOL terminated. */
size_t RCP_strlcpy(char *dst, const char *src, size_t siz);

/* call this function with each char received.   It will return a
 * pointer to RCP packet that has been detected in the stream (and its
 * length) or NULL if no complete packet has been received yet.
 * Note that packetState should be memset to all 0s before the first call
 * and state members buf and buf_len should be set to valid values
 */
const char * RCP_get_packet(tRCPPacketState * packetState, char nextChar, int * numBytes);

/* call this function with each char received.  It will return
 * RCP2_PARSER_OK when a packet has been detected and parsed.  The
 * parsed data can be found in the packetState structure.  Before the
 * first call the state members buf, buf_len, and initialized should be set.
 */
int RCP_get_and_parse_packet(tRCPParsedPacketState * packetState, char nextChar);

/* same as RCP_get_and_parse_packet, but the original stream sent into
 * the parse is left intact.  That is, when RCP2_PARSER_OK is returned,
 * packetState->buf / packetState->len can be used to inspect the
 * actual packet received.  Note: the pointers in packetState->parsed,
 * such as cCmd, pTarget, pSource, pParam, argv, etc. will not be NULL
 * terminated, they will be RCP2_MSG_SEPARATOR_SYMBOL terminated.
 */
int RCP_get_and_parse_packet_non_destructive(tRCPParsedPacketState * packetState, char nextChar);

/* Get the RCP Version */
int RCP_getVersion(void);

#ifdef __cplusplus
}
#endif

#endif
