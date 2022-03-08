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

#include <QPainter>
#include "controller.h"
//#include "logger.h"

Controller::Controller(QWidget *parent /*= 0*/) : QWidget(parent)
{
    m_title = new QLabel(tr("Controller"), this);
    m_title->setObjectName("title");

    m_record = new QPushButton(tr("PRE-RECORDING"), this);
    m_record->setObjectName("record");
    m_record->adjustSize();
    m_record->setFixedWidth(m_record->width());
    m_record->setText(tr("RECORD"));

    m_layout = new QGridLayout(this);

    m_layout->addWidget(m_title, 0, 0, 1, 1, Qt::AlignHCenter | Qt::AlignTop);
    m_layout->addWidget(m_record, 1, 0, 1, 1, Qt::AlignHCenter | Qt::AlignTop);
    m_layout->addItem(new QSpacerItem(1, 1), 2, 0, 100, 1);

    setSizePolicy(QSizePolicy::Minimum, QSizePolicy::MinimumExpanding);
    m_paramGroups = 0;
    setLayout(m_layout);

    connect(m_record, SIGNAL(clicked()), this, SLOT(toggleRecord()));
}

Controller::~Controller()
{
    delete m_title;

    int i;

    if (m_paramGroups != 0)
    {
        for (i = 0; i < m_paramCount; i++)
        {
            delete m_paramGroups[i];
        }

        delete m_paramGroups;
    }

    delete m_layout;
}

rcp_param_t* Controller::params(int *count)
{
    int i;
    rcp_param_t *params = new rcp_param_t[m_paramCount]; // it is the calling function's responsibility to free this array

    for (i = 0; i < m_paramCount; i++)
    {
        params[i] = m_paramGroups[i]->paramID();
    }

    *count = m_paramCount;
    return params;
}

void Controller::setParamTitle(const QString &text, rcp_param_t paramID)
{
    ParameterGroup *group = paramGroup(paramID);

    if (group != 0)
    {
        group->setTitle(text);
    }
}

void Controller::setParamTitleWidth(const int width)
{
    int i;
    for (i = 0; i < m_paramCount; i++)
    {
        m_paramGroups[i]->setTitleWidth(width);
    }
}

void Controller::initialize()
{
    int i;

    if (m_paramGroups != 0)
    {
        for (i = 0; i < m_paramCount; i++)
        {
            m_layout->removeWidget(m_paramGroups[i]);
            delete m_paramGroups[i];
        }

        delete m_paramGroups;
    }

    m_paramCount = 0;


    i = 0;
    param_t params[100];
    params[i++] = {RCP_PARAM_SENSOR_FRAME_RATE, UI_TYPE_COMBO};
    params[i++] = {RCP_PARAM_ISO, UI_TYPE_COMBO};
    params[i++] = {RCP_PARAM_APERTURE, UI_TYPE_COMBO};
    params[i++] = {RCP_PARAM_EXPOSURE_INTEGRATION_TIME, UI_TYPE_COMBO};
    params[i++] = {RCP_PARAM_COLOR_TEMPERATURE, UI_TYPE_COMBO};
    params[i++] = {RCP_PARAM_RECORD_FORMAT, UI_TYPE_COMBO};
    params[i++] = {RCP_PARAM_REDCODE, UI_TYPE_COMBO};
    params[i++] = {RCP_PARAM_SATURATION, UI_TYPE_SLIDER};
    params[i++] = {RCP_PARAM_CONTRAST, UI_TYPE_SLIDER};
    params[i++] = {RCP_PARAM_BRIGHTNESS, UI_TYPE_SLIDER};
    params[i++] = {RCP_PARAM_EXPOSURE_COMPENSATION, UI_TYPE_SLIDER};
    params[i++] = {RCP_PARAM_RED_GAIN, UI_TYPE_SLIDER};
    params[i++] = {RCP_PARAM_GREEN_GAIN, UI_TYPE_SLIDER};
    params[i++] = {RCP_PARAM_BLUE_GAIN, UI_TYPE_SLIDER};
    params[i++] = {RCP_PARAM_FLUT, UI_TYPE_SLIDER};
    params[i++] = {RCP_PARAM_SHADOW, UI_TYPE_SLIDER};
    params[i++] = {RCP_PARAM_SLATE_SCENE, UI_TYPE_TEXT_FIELD};

    m_paramCount = i;


    m_paramGroups = new ParameterGroup*[m_paramCount];
    for (i = 0; i < m_paramCount; i++)
    {
        m_paramGroups[i] = new ParameterGroup(params[i]);

        connect(m_paramGroups[i], SIGNAL(stringSent(rcp_param_t, const QString&)), this, SIGNAL(stringSent(rcp_param_t, const QString&)));
        connect(m_paramGroups[i], SIGNAL(intSent(rcp_param_t, const int)), this, SIGNAL(intSent(rcp_param_t, const int)));
        connect(m_paramGroups[i], SIGNAL(listNeeded(rcp_param_t)), this, SIGNAL(listNeeded(rcp_param_t)));
        connect(m_paramGroups[i], SIGNAL(stringNeeded(rcp_param_t)), this, SIGNAL(stringNeeded(rcp_param_t)));
    }

    updateInt(RECORD_STATE_NOT_RECORDING, RCP_PARAM_RECORD_STATE);
}

void Controller::constructUI()
{
    int i;
    for (i = 0; i < m_paramCount; i++)
    {
        // The + 2 accommodates for the title and the record button
        m_layout->addWidget(m_paramGroups[i], i + 2, 0, 1, 1);
    }

    adjustSize();
}

void Controller::updateInt(const int value, const rcp_param_t paramID)
{
    ParameterGroup *group = paramGroup(paramID);

    if (paramID == RCP_PARAM_RECORD_STATE)
    {
        switch (value)
        {
            case RECORD_STATE_NOT_RECORDING:
                m_record->setStyleSheet("background-color: #808080;");
                m_record->setText(tr("RECORD"));
                m_record->setEnabled(true);
                break;

            case RECORD_STATE_RECORDING:
                m_record->setStyleSheet("background-color: #FF0000;");
                m_record->setText(tr("RECORDING"));
                m_record->setEnabled(true);
                break;

            case RECORD_STATE_FINALIZING:
                m_record->setStyleSheet("background-color: #FFFF00;");
                m_record->setText(tr("FINALIZING"));
                m_record->setEnabled(false);
                break;

            case RECORD_STATE_PRE_RECORDING:
                m_record->setStyleSheet("background-color: #FFFF00;");
                m_record->setText(tr("PRE-RECORDING"));
                m_record->setEnabled(true);
                break;

            default:
                //Logger::logWarning(QString("Invalid record status %1 specified").arg(value), "Controller::updateInt(const int, const rcp_param_t)");
                m_record->setEnabled(true);
                break;
        }
    }
    else if (group != 0)
    {
        switch (group->uiType())
        {
            case UI_TYPE_SLIDER:
                if (!group->sliding() && group->updateSlider())
                {
                    group->setCurrentValue(value);
                }
                break;

            case UI_TYPE_COMBO:
                // Don't need to do anything here. The combo lists are reloaded each time they're opened or each time a value changes through scrolling/pressing directional keys.
                // Only the display value (which is obtained in updateString) of the combo needs to update.
                break;

            case UI_TYPE_TEXT_FIELD:
                // Don't need to do anything here. updateString() handles text fields.
                break;

            default:
                //Logger::logWarning(QString("Invalid UI type specified: %1").arg(group->uiType()), "Controller::updateInt(const int, const rcp_param_t)");
                break;
        }
    }
}

void Controller::updateString(const QString &str, const rcp_param_status_t status, const rcp_param_t paramID)
{
    QString color = "color: #";

    switch (status)
    {
        case RCP_PARAM_DISPLAY_STATUS_NORMAL:
            color.append("F0F0F0");
            break;

        case RCP_PARAM_DISPLAY_STATUS_GOOD:
            color.append("00FF00");
            break;

        case RCP_PARAM_DISPLAY_STATUS_WARNING:
            color.append("FFFF00");
            break;

        case RCP_PARAM_DISPLAY_STATUS_ERROR:
            color.append("FF0000");
            break;

        case RCP_PARAM_DISPLAY_STATUS_DISABLED:
            color.append("A0A0A0");
            break;

        case RCP_PARAM_DISPLAY_STATUS_RECORDING:
            color.append("FF0000");
            break;

        case RCP_PARAM_DISPLAY_STATUS_FINALIZING:
            color.append("FFFF00");
            break;

        default:
            color.append("F0F0F0");
            break;
    }

    ParameterGroup *group = paramGroup(paramID);

    if (group != 0)
    {
        group->setCurrentText(str, color);
    }
}

void Controller::updateList(const rcp_cur_list_cb_data_t *data, const bool updateOnlyOnClose)
{
    ParameterGroup *group = paramGroup(data->id);

    if (group != 0)
    {
        QString currentString = group->setListData(data, updateOnlyOnClose);

        if (data->display_str_in_list && !currentString.isEmpty())
        {
            updateString(currentString, RCP_PARAM_DISPLAY_STATUS_NORMAL, data->id);
        }
    }
}

void Controller::toggleRecord()
{
    emit intSent(RCP_PARAM_RECORD_STATE, SET_RECORD_STATE_TOGGLE);
}

QSize Controller::sizeHint() const
{
    return QSize(450, 400);
}

void Controller::paintEvent(QPaintEvent* /*event*/)
{
    QPainter painter;
    painter.begin(this);
    painter.fillRect(rect(), QColor("#404040"));
}

ParameterGroup* Controller::paramGroup(const rcp_param_t paramID)
{
    int i;
    for (i = 0; i < m_paramCount; i++)
    {
        if (paramID == m_paramGroups[i]->paramID())
        {
            return m_paramGroups[i];
        }
    }

    return 0;
}
