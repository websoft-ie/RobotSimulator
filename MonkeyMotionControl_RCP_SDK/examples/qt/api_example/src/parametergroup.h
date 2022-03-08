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

#ifndef PARAMETERGROUP_H
#define PARAMETERGROUP_H

#include <QLabel>
#include <QGridLayout>
#include "listdata.h"
#include "slider.h"
#include "slideredit.h"
#include "combobox.h"
#include "rcp_api/rcp_api.h"

typedef enum
{
    UI_TYPE_SLIDER,
    UI_TYPE_COMBO,
    UI_TYPE_TEXT_FIELD
} ui_type_t;

typedef struct
{
    rcp_param_t paramID;
    ui_type_t uiType;
} param_t;

class ParameterGroup : public QWidget
{
    Q_OBJECT

    public:
        explicit ParameterGroup(const param_t param, QWidget *parent = 0);
        ~ParameterGroup();
        rcp_param_t paramID() const;
        ui_type_t uiType() const;
        bool sliding() const;
        bool updateSlider() const;
        void setTitle(const QString &title);
        void setCurrentText(const QString &str, const QString &stylesheet);
        void setCurrentValue(const int value);
        void setTitleWidth(const int width);
        QString setListData(const rcp_cur_list_cb_data_t *data, const bool updateOnlyOnClose);


    signals:
        void stringSent(rcp_param_t paramID, const QString &value);
        void intSent(rcp_param_t paramID, const int value);
        void listNeeded(rcp_param_t paramID);
        void stringNeeded(rcp_param_t paramID);


    private slots:
        void sendSliderString(const QString &value);
        void sendString(const QString &value);
        void sendInt(const int index);
        void openCombo();
        void closeCombo();
        void disableUpdates();
        void enableUpdates();


    private:
        static const int UPDATE_TIMEOUT = 500;
        param_t m_param;
        ListData *m_listData;
        QLabel *m_title;
        Slider *m_slider;
        SliderEdit *m_sliderEdit;
        bool m_updateSlider;
        ComboBox *m_combo;
        QLineEdit *m_textField;
        QGridLayout *m_layout;
};

#endif // PARAMETERGROUP_H
