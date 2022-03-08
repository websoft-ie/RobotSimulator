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

#include <QTimer>
#include <math.h>
#include "parametergroup.h"
#include "combodelegate.h"
#include "logger.h"
#include "clist/c_list.h"
#include "clist/clist.h"
#include "qtutil.h"

ParameterGroup::ParameterGroup(const param_t param, QWidget *parent /*= 0*/) : QWidget(parent)
{
    m_param = param;
    m_listData = new ListData;
    m_title = new QLabel(this);
    m_layout = new QGridLayout(this);
    m_updateSlider = true;

    const int colCount = 50;
    m_layout->addWidget(m_title, 0, 0, 1, 1);

    QLabel widthLabel("0000.000"); // used to figure out the slider text edit width

    switch (m_param.uiType)
    {
        case UI_TYPE_SLIDER:
            m_slider = new Slider(Qt::Horizontal, this);
            m_sliderEdit = new SliderEdit(this);
            m_combo = 0;
            m_textField = 0;

            m_sliderEdit->setAlignment(Qt::AlignCenter);

            widthLabel.adjustSize();
            m_sliderEdit->setFixedWidth(widthLabel.width());
            m_layout->addWidget(m_slider, 0, 1, 1, colCount - 2);
            m_layout->addWidget(m_sliderEdit, 0, colCount - 1, 1, 1, Qt::AlignRight);

            connect(m_sliderEdit, SIGNAL(valueUpdated(const QString&)), this, SLOT(sendSliderString(const QString&)));
            connect(m_slider, SIGNAL(valueChanged(const int)), this, SLOT(sendInt(const int)));
            connect(m_slider, SIGNAL(sliderReleased()), this, SLOT(disableUpdates()));
            break;

        case UI_TYPE_COMBO:
            m_slider = 0;
            m_sliderEdit = 0;
            m_combo = new ComboBox(this);

            // There is an issue with applying this delegate on the OS X version
#ifdef Q_OS_WIN
            m_combo->setItemDelegate(new ComboDelegate);
#endif
            m_textField = 0;

            m_layout->addWidget(m_combo, 0, 1, 1, colCount - 1);

            connect(m_combo, SIGNAL(currentIndexChanged(const int)), this, SLOT(sendInt(const int)));
            connect(m_combo, SIGNAL(opened()), this, SLOT(openCombo()));
            connect(m_combo, SIGNAL(closed()), this, SLOT(closeCombo()));
            break;

        case UI_TYPE_TEXT_FIELD:
            m_slider = 0;
            m_sliderEdit = 0;
            m_combo = 0;
            m_textField = new QLineEdit(this);

            m_layout->addWidget(m_textField, 0, 1, 1, colCount - 1);

            connect(m_textField, SIGNAL(textChanged(const QString&)), this, SLOT(sendString(const QString&)));
            break;

        default:
            Logger::logError(QString("Invalid UI type %1 specified").arg(m_param.uiType), "ParameterGroup::ParameterGroup(const param_t, QWidget*)");
            break;
    }

    setLayout(m_layout);
}

ParameterGroup::~ParameterGroup()
{
    delete m_listData;
    delete m_title;

    if (m_slider != 0)
    {
        delete m_slider;
    }

    if (m_sliderEdit != 0)
    {
        delete m_sliderEdit;
    }

    if (m_combo != 0)
    {
        delete m_combo;
    }

    delete m_layout;
}

rcp_param_t ParameterGroup::paramID() const
{
    return m_param.paramID;
}

ui_type_t ParameterGroup::uiType() const
{
    return m_param.uiType;
}

bool ParameterGroup::sliding() const
{
    if (m_param.uiType == UI_TYPE_SLIDER)
    {
        return (m_slider->isSliderDown() || m_slider->isKeyDown());
    }

    return false;
}

bool ParameterGroup::updateSlider() const
{
    if (m_param.uiType == UI_TYPE_SLIDER)
    {
        return m_updateSlider;
    }

    return true;
}

void ParameterGroup::setTitle(const QString &title)
{
    m_title->setText(QtUtil::decodedStringFromString(title));
}

void ParameterGroup::setCurrentText(const QString &str, const QString &stylesheet)
{
    switch (m_param.uiType)
    {
        case UI_TYPE_SLIDER:
            m_sliderEdit->setText(str);
            m_sliderEdit->setStyleSheet(stylesheet);
            break;

        case UI_TYPE_COMBO:
            m_listData->setCurrentText(str);
            m_combo->setStyleSheet(stylesheet);
            m_combo->update();
            break;

        case UI_TYPE_TEXT_FIELD:
            m_textField->setText(str);
            m_textField->setStyleSheet(stylesheet);
            break;

        default:
            Logger::logWarning(QString("Invalid UI type specified: %1").arg(m_param.uiType), "ParameterGroup::setCurrentText(const QString&, const QString&)");
            break;
    }
}

void ParameterGroup::setCurrentValue(const int value)
{
    switch (m_param.uiType)
    {
        case UI_TYPE_SLIDER:
            m_slider->blockSignals(true);
            m_slider->setValue(m_listData->indexFromValue(value));
            m_slider->blockSignals(false);
            break;

        case UI_TYPE_COMBO:
            // Don't need to do anything here. Only setCurrentText() is relevant for combo boxes since the list is retrieved and updated whenever it is opened or changed via scroll or directional keys.
            break;

        case UI_TYPE_TEXT_FIELD:
            // Don't need to do anything here. Only setCurrentText() is relevant for text fields.
            break;

        default:
            Logger::logWarning(QString("Invalid UI type specified: %1").arg(m_param.uiType), "ParameterGroup::setCurrentValue(const int)");
            break;
    }
}

void ParameterGroup::setTitleWidth(const int width)
{
    m_title->setFixedWidth(width);
}

// Sets the list data and returns the string value of the current entry
QString ParameterGroup::setListData(const rcp_cur_list_cb_data_t *data, const bool updateOnlyOnClose)
{
    QString currentString = "";
    const bool isCombo = m_param.uiType == UI_TYPE_COMBO;
    const bool comboIsOpen = isCombo ? m_combo->isOpen() : false;
    cList *list = new cList;

    if (isCombo)
    {
        m_combo->blockSignals(true);

        if (comboIsOpen)
        {
#ifdef Q_OS_WIN
            m_combo->hidePopup();
#endif
            m_combo->clear();
        }
    }

    m_listData->clear();

    if (list->importStringListAndDecode(data->list_string) == cList::SUCCESS)
    {
        m_listData->setMin(data->min_val);
        m_listData->setMax(data->max_val);
        m_listData->setMinValid(data->min_val_valid);
        m_listData->setMaxValid(data->max_val_valid);
        m_listData->setUpdateOnlyOnClose(updateOnlyOnClose);

        size_t length = list->length();
        size_t i;

        for (i = 0; i < length; i++)
        {
            cList::Entry entry;

            if (list->getData(i, entry) == cList::SUCCESS)
            {
                m_listData->addItem(tr("%1").arg(entry.str), entry.num);
            }
        }

        list->getIndex(i);
        m_listData->setCurrentRow(i);

        switch (m_param.uiType)
        {
            case UI_TYPE_SLIDER:
                m_slider->setRange(0, m_listData->count() - 1);
                m_slider->setValue(i);
                break;

            case UI_TYPE_COMBO:
                m_combo->setModel(m_listData->data());
                m_combo->setCurrentIndex(m_listData->currentRow());
                break;

            case UI_TYPE_TEXT_FIELD:
                // Don't need to do anything here. Lists are not relevant to text fields.
                break;

            default:
                Logger::logWarning(QString("Invalid UI type specified: %1").arg(m_param.uiType), "ParameterGroup::setListData(const rcp_cur_list_cb_data_t*, const bool)");
                break;
        }

        cList::Entry e;
        if (list->getData(i, e) == cList::SUCCESS)
        {
            currentString = QString("%1").arg(e.str);
        }
    }
    else
    {
        Logger::logError(QString("The list string is invalid: %1").arg(data->list_string), "ParameterGroup::setListData(const rcp_cur_list_cb_data_t*, const bool)");
    }

    if (isCombo)
    {
        if (comboIsOpen)
        {
#ifdef Q_OS_WIN
            m_combo->showPopup();
#endif
            m_combo->setStyleSheet("color: #F0F0F0");
        }

        m_combo->blockSignals(false);
    }

    return currentString;
}

void ParameterGroup::sendSliderString(const QString &value)
{
    double val = value.toDouble();
    val *= 1000.0;

    emit intSent(m_param.paramID, roundf(val));
}

void ParameterGroup::sendString(const QString &value)
{
    emit stringSent(m_param.paramID, value);
}

void ParameterGroup::sendInt(const int index)
{
    if (index >= 0 && index < m_listData->count())
    {
        m_listData->setCurrentRow(index);
        emit intSent(m_param.paramID, m_listData->valueFromIndex(index));
    }
}

void ParameterGroup::openCombo()
{
    emit listNeeded(m_param.paramID);
}

void ParameterGroup::closeCombo()
{
    sendInt(m_combo->currentIndex());

    // Handles the case where the combo box was opened and then closed without the value changing.
    // Since opening the combo makes all the entries white, if the value selected was invalid (yellow), this ensures that the string will still be yellow when closed.
    emit stringNeeded(m_param.paramID);
}

void ParameterGroup::disableUpdates()
{
    m_updateSlider = false;
    QTimer::singleShot(UPDATE_TIMEOUT, this, SLOT(enableUpdates()));
}

void ParameterGroup::enableUpdates()
{
    m_updateSlider = true;
}
