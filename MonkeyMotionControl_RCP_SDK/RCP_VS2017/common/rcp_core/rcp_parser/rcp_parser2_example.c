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

#include <stdio.h>
#include <stdlib.h>
#if _CVI_
#include <ansi_c.h>
#include <formatio.h>
#endif
#include "rcp_parser2.h"

static void test_RCP_get_and_parse_packet2(const char * rcp, size_t len)
{
    size_t ii;
    char buf[RCP2_MAX_PACKET_LENGTH];
    tRCPParsedPacketState state = {0};
    state.buf = buf;
    state.buf_len = sizeof(buf);

    printf("Testing RCP_get_and_parse_packet:\n");
    printf("  RCP Packet: %s\n", rcp);

    for (ii = 0; ii < len; ii++)
    {
        int res;

        switch (res = RCP_get_and_parse_packet(&state, rcp[ii]))
        {
            case RCP2_PARSER_OK:
            {
                int arg;
                if (state.parsed.pTarget)
                    printf("  Target: %s\n", state.parsed.pTarget);
                if (state.parsed.pSource)
                    printf("  Source: %s\n", state.parsed.pSource);
                if (state.parsed.pCmd)
                    printf("  Cmd: %s\n", state.parsed.pCmd);
                if (state.parsed.pParam)
                    printf("  Param: %s\n", state.parsed.pParam);
                if (
                      state.parsed.pCmd &&
                      state.parsed.pCmd[0] == 'S' &&
                      state.parsed.pParam &&
                      strcmp(state.parsed.pParam, "BIN") == 0
                   )
                {
                    int i;
                    int len = atoi(state.parsed.argv[0]);
                    printf("  Binary data (length = %d)\n  ", len);
                    for (i = 0; i < len; i++)
                    {
                        printf(" 0x%02x", state.parsed.argv[1][i]);
                    }
                    printf("\n");
                }
                else
                {
                    for (arg = 0; arg < state.parsed.argc; arg++)
                    {
                        printf("  Argv[%d]: %s\n", arg, state.parsed.argv[arg]);
                    }
                }
                if (state.parsed.pChecksum)
                    printf("  Checksum: %s (%2x)\n", state.parsed.pChecksum, state.checksum);

                return;
            }

            case RCP2_PARSER_INCOMPLETE_PACKET:
                /* not an error, just need to keep feeding it data */
                break;

            default:
                printf("  RCP2 PARSING ERROR: %d\n", res);
                if (res == RCP2_PARSER_CHECKSUM_MISSMATCH)
                {
                    if (state.parsed.pChecksum)
                        printf("  Checksum: %s (%2x)\n", state.parsed.pChecksum, state.checksum);
                }
                break;
        }
    }

    printf("\n");
}

static void test_RCP_get_and_parse_packet(const char * rcp)
{
    test_RCP_get_and_parse_packet2(rcp, strlen(rcp));
}

int main(void) {
    //Validate RCP Parser...
    tRCP rcpBuffer;
    char outputBuffer[1024];
    char * source = "SOURCE";
    char * target = "TARGET";
    int returnVal;
    char * pToken;
    char cmd;

    //Lets create a header
    printf("Creating Simple Header\n");
    returnVal = RCP_buildHeader(&rcpBuffer, outputBuffer, sizeof(outputBuffer), source, 0, 'S');
    if (returnVal != RCP2_PARSER_OK)
        printf("ERROR: %d\n", returnVal);

    //And Terminate it immidatly
    returnVal = RCP_finalizeMessage(&rcpBuffer);
    printf("Final Size: %d Bytes\n", returnVal);

    //Print result
    printf("Message: %s\n", outputBuffer);
    test_RCP_get_and_parse_packet(outputBuffer);

    //Lets Create another header
    printf("Creating Complex Header\n");
    returnVal = RCP_buildHeader(&rcpBuffer, outputBuffer, sizeof(outputBuffer), source, target, 'S');
    if (returnVal != RCP2_PARSER_OK)
        printf("ERROR: %d\n", returnVal);

    //Add a Param and Value
    returnVal = RCP_addToken(&rcpBuffer, "TEST1");
    if (returnVal != RCP2_PARSER_OK)
        printf("ERROR: %d\n", returnVal);

    //Add Another value
    returnVal = RCP_addToken(&rcpBuffer, "1000");
    if (returnVal != RCP2_PARSER_OK)
        printf("ERROR: %d\n", returnVal);

    //Calculate Checksum
    returnVal = RCP_addChecksum(&rcpBuffer);
    if (returnVal != RCP2_PARSER_OK)
        printf("ERROR: %d\n", returnVal);

    returnVal = RCP_finalizeMessage(&rcpBuffer);
    printf("Final Size: %d Bytes\n", returnVal);

    //Print Result
    printf("Message: %s\n", outputBuffer);
    test_RCP_get_and_parse_packet(outputBuffer);

    //Parse the Last Header
    printf("Prepare for Parsing\n");
    returnVal = RCP_prepareForParsing(&rcpBuffer, outputBuffer, strlen(outputBuffer));
    if (returnVal != RCP2_PARSER_OK)
        printf("ERROR: %d\n", returnVal);

    //Validate checksum
    printf("Validating checksum\n");
    returnVal = RCP_validateChecksum(&rcpBuffer);
    if (returnVal != RCP2_PARSER_OK)
        printf("ERROR: %d\n", returnVal);

    printf("Parsing Header\n");
    returnVal = RCP_parseHeader(&rcpBuffer, "TARGET", &cmd);
    if (returnVal != RCP2_PARSER_OK)
        printf("ERROR: %d\n", returnVal);

    //Extract Parameters and print until we run out of data
    while ((pToken = RCP_getNextToken(&rcpBuffer)) != 0)
        printf("TOKEN: %s\n", pToken);

    printf("Done Parsing Message\n");
    printf("\n");

    //Create another Header
    printf("Creating  Header\n");
    returnVal = RCP_buildHeader(&rcpBuffer, outputBuffer, sizeof(outputBuffer), source, target, 'S');
    if (returnVal != RCP2_PARSER_OK)
        printf("ERROR: %d\n", returnVal);

    //Add a Param and Value
    returnVal = RCP_addToken(&rcpBuffer, "ESCAPE");
    if (returnVal != RCP2_PARSER_OK)
        printf("ERROR: %d\n", returnVal);

    //Add PArameter with Escaping strings
    returnVal = RCP_addToken(&rcpBuffer, "BLA:BLA\\BLA");
    if (returnVal != RCP2_PARSER_OK)
        printf("ERROR: %d\n", returnVal);

    //Finalize
    returnVal = RCP_finalizeMessage(&rcpBuffer);
    printf("Final Size: %d Bytes\n", returnVal);

    //Print Result
    printf("Message: %s\n", outputBuffer);
    test_RCP_get_and_parse_packet(outputBuffer);

    //Parse the same header
    printf("Prepare for Parsing\n");
    returnVal = RCP_prepareForParsing(&rcpBuffer, outputBuffer, strlen(outputBuffer));
    if (returnVal != RCP2_PARSER_OK)
        printf("ERROR: %d\n", returnVal);

    printf("Parsing Header\n");
    returnVal = RCP_parseHeader(&rcpBuffer, "TARGET", &cmd);
    if (returnVal != RCP2_PARSER_OK)
        printf("ERROR: %d\n", returnVal);

    //Extract Parameters and print until we run out of data
    while ((pToken = RCP_getNextToken(&rcpBuffer)) != 0)
        printf("TOKEN: %s\n", pToken);

    printf("Done Parsing Message\n");
    printf("\n");

    /* Test RCP_get_and_parse_packet function */
    test_RCP_get_and_parse_packet("#@target$source:c:param:value1:value2:\r");

    test_RCP_get_and_parse_packet("#$source:c:param:value1:value2:\r");

    test_RCP_get_and_parse_packet("#@target$source:c:param:value1:\r");

    test_RCP_get_and_parse_packet("#@target$source:S:BIN:05:HE:\\O\r");

    test_RCP_get_and_parse_packet2("#@target$source:S:BIN:05:12345" "\x00" "\x00" "\x00" "*4B\r", 37);

    test_RCP_get_and_parse_packet("#@target$source:S:BIN:05:HE:\\Oextra\r");

    test_RCP_get_and_parse_packet2("#@target$source:S:BINDONE:00:" "\x00" "\x00" "\x00" "\r", 33);

    test_RCP_get_and_parse_packet("#@target$source:c:param:\r");

    test_RCP_get_and_parse_packet("#@target$:c:param:\r");

    test_RCP_get_and_parse_packet("#@target$source::param:\r");

    test_RCP_get_and_parse_packet("#@target$source:c::\r");

    test_RCP_get_and_parse_packet("#@target$source:c:param:1:2:3:4:5:6:7:8:9:10:11:12:13:14:15:16:17:18:19:20:\r");

    test_RCP_get_and_parse_packet("#@target$source:c:param:*00\r");

    test_RCP_get_and_parse_packet("#@target$source:c:param\r");

    return EXIT_SUCCESS;
}
