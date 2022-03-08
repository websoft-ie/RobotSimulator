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
#include "rcpdata.h"
#include "rcp.h"
#include "logger.h"

RCPData::RCPData(QObject *parent /*= 0*/) : QObject(parent)
{
    initialize();
}

bool RCPData::connectionEstablished() const
{
    return m_connectionEstablished;
}

bool RCPData::connectionIsValid() const
{
    return m_connectionIsValid;
}

int RCPData::rcpVersion() const
{
    return m_rcpVersion;
}

int RCPData::systemMajorVersion() const
{
    return m_systemMajorVersion;
}

int RCPData::systemMinorVersion() const
{
    return m_systemMinorVersion;
}

int RCPData::recordStatus() const
{
    return m_recordStatus;
}

void RCPData::setConnectionEstablished(const bool connectionEstablished)
{
    m_connectionEstablished = connectionEstablished;
}

void RCPData::setConnectionIsValid(const bool connectionIsValid)
{
    m_connectionIsValid = connectionIsValid;
}

void RCPData::initialize()
{
    m_connectionEstablished = false;
    m_connectionIsValid = false;

    m_rcpVersion = -1;
    m_systemMajorVersion = -1;
    m_systemMinorVersion = -1;
    m_recordStatus = -1; // needs to be initialized to -1; this is used as a flag to verify that external control is enabled

    emit recordUpdated(RECORD_STATE_NOT_RECORDING);
}

void RCPData::updateValues(const char *rcp)
{
    tRCPParsedPacketState state = {0};
    const bool rcpIsValid = RCP::parseRCP(&state, rcp);

    if (rcpIsValid && *(state.parsed.pCmd) == RCP2_CMD_CURRENT)
    {
        if (m_connectionEstablished)
        {
            if (strcmp(state.parsed.pParam, "RECORD") == 0)
            {
                m_recordStatus = atoi(state.parsed.argv[0]);
                emit recordUpdated(m_recordStatus);
            }
            else if (strcmp(state.parsed.pParam, "SENSFPS") == 0)
            {
                emit sensorFPSUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "PROJFPS") == 0)
            {
                emit projectFPSUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "ISO") == 0)
            {
                emit isoUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "APRTR") == 0)
            {
                emit apertureUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "SHTIME") == 0)
            {
                emit shutterTimeUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "SHTIMET") == 0)
            {
                emit shutterTimeTargetUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "COLTMP") == 0)
            {
                emit colorTempUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "FORMAT2") == 0)
            {
                emit formatUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "REDCODE") == 0)
            {
                emit redcodeUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "RCTARGET") == 0)
            {
                emit redcodeTargetUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "SATURAT") == 0)
            {
                emit saturationUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "CONTRST") == 0)
            {
                emit contrastUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "BRIGHT") == 0)
            {
                emit brightnessUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "EXPCOMP") == 0)
            {
                emit exposureUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "REDG") == 0)
            {
                emit rgradeUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "GREENG") == 0)
            {
                emit ggradeUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "BLUEG") == 0)
            {
                emit bgradeUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "FLUT") == 0)
            {
                emit flutUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "SHADOW") == 0)
            {
                emit shadowUpdated(atoi(state.parsed.argv[0]));
            }
            else if (strcmp(state.parsed.pParam, "SENSFPSL") == 0)
            {
                emit sensorFPSListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "ISOL") == 0)
            {
                emit isoListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "APRTRL") == 0)
            {
                emit apertureListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "SHTIMEL") == 0)
            {
                emit shutterTimeListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "COLTMPL") == 0)
            {
                emit colorTempListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "FORMATL") == 0)
            {
                emit formatListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "REDCODEL") == 0)
            {
                emit redcodeListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "SATURATL") == 0)
            {
                emit saturationListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "CONTRSTL") == 0)
            {
                emit contrastListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "BRIGHTL") == 0)
            {
                emit brightnessListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "EXPCOMPL") == 0)
            {
                emit exposureListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "RGRADEL") == 0)
            {
                emit rgradeListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "GREENGL") == 0)
            {
                emit ggradeListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "BLUEGL") == 0)
            {
                emit bgradeListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "FLUTL") == 0)
            {
                emit flutListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "SHADOWL") == 0)
            {
                emit shadowListReceived(state.parsed.argv[0]);
            }
        }
        else
        {
            if (strcmp(state.parsed.pParam, "RCPVER") == 0)
            {
                m_rcpVersion = atoi(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "RCPPSVER") == 0)
            {
                QString hex;
                QString major;
                QString minor;

                // Converts the argument to hex and stores the 4 high order bits as the major version and the 4 low order bits as the minor version
                hex.sprintf("%08X", (unsigned int)atol(state.parsed.argv[0]));
                major = hex.mid(0, 4);
                minor = hex.mid(4, 4);

                // Converts the hex values to longs
                m_systemMajorVersion = strtol(major.toUtf8().constData(), 0, 16);
                m_systemMinorVersion = strtol(minor.toUtf8().constData(), 0, 16);
            }
            else if (strcmp(state.parsed.pParam, "SENSFPSL") == 0)
            {
                emit sensorFPSListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "ISOL") == 0)
            {
                emit isoListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "APRTRL") == 0)
            {
                emit apertureListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "SHTIMEL") == 0)
            {
                emit shutterTimeListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "COLTMPL") == 0)
            {
                emit colorTempListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "FORMATL") == 0)
            {
                emit formatListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "REDCODEL") == 0)
            {
                emit redcodeListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "SATURATL") == 0)
            {
                emit saturationListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "CONTRSTL") == 0)
            {
                emit contrastListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "BRIGHTL") == 0)
            {
                emit brightnessListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "EXPCOMPL") == 0)
            {
                emit exposureListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "RGRADEL") == 0)
            {
                emit rgradeListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "GREENGL") == 0)
            {
                emit ggradeListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "BLUEGL") == 0)
            {
                emit bgradeListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "FLUTL") == 0)
            {
                emit flutListReceived(state.parsed.argv[0]);
            }
            else if (strcmp(state.parsed.pParam, "SHADOWL") == 0)
            {
                emit shadowListReceived(state.parsed.argv[0]);
            }
        }
    }
}
