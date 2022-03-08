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

#include "listdata.h"
#include "logger.h"

ListData::ListData(QObject *parent /*= 0*/) : QObject(parent)
{
    m_data = new QStandardItemModel;
    clear();
}

ListData::~ListData()
{
    delete m_data;
}

QStandardItemModel* ListData::data() const
{
    return m_data;
}

int ListData::currentRow() const
{
    return m_currentRow;
}

int ListData::min() const
{
    return m_min;
}

int ListData::max() const
{
    return m_max;
}

bool ListData::minValid() const
{
    return m_minValid;
}

bool ListData::maxValid() const
{
    return m_maxValid;
}

bool ListData::updateOnlyOnClose() const
{
    return m_updateOnlyOnClose;
}

int ListData::count() const
{
    return m_data->rowCount();
}

// Looks for the index of the specified value in the data cList
// It first looks for the exact value. If it cannot find it, it'll look for a value that the argument is less than in order to best position the slider handle
int ListData::indexFromValue(const int value) const
{
    int len = count();
    int i;

    for (i = 0; i < len; i++)
    {
        QStandardItem *item = m_data->item(i);

        if (item != 0)
        {
            if (value == item->data(Qt::UserRole).toInt())
            {
                return i;
            }
        }
    }

    for (i = 0; i < len; i++)
    {
        QStandardItem *item = m_data->item(i);

        if (item != 0)
        {
            if (value < item->data(Qt::UserRole).toInt())
            {
                return i;
            }
        }
    }

    Logger::logWarning(QString("Index from value %1 was not found").arg(value), "ListData::indexFromValue(const int)");
    return 0;
}

int ListData::valueFromIndex(const int index) const
{
    QStandardItem *item = m_data->item(index);

    if (item != 0)
    {
        return item->data(Qt::UserRole).toInt();
    }

    Logger::logWarning(QString("Value from index %1 was not found").arg(index), "ListData::valueFromIndex(const int)");
    return 0;
}

void ListData::addItem(const QString &str, const int val)
{
    QStandardItem *item = new QStandardItem(str);
    item->setData(QVariant(val), Qt::UserRole);

    if ((m_minValid && val < m_min) || (m_maxValid && val > m_max))
    {
        item->setData(QVariant(false), Qt::UserRole + 1);
    }
    else
    {
        item->setData(QVariant(true), Qt::UserRole + 1);
    }

    m_data->appendRow(item);
}

void ListData::setCurrentRow(const int row)
{
    m_currentRow = row;
}

void ListData::setMin(const int min)
{
    m_min = min;
}

void ListData::setMax(const int max)
{
    m_max = max;
}

void ListData::setMinValid(const bool minValid)
{
    m_minValid = minValid;
}

void ListData::setMaxValid(const bool maxValid)
{
    m_maxValid = maxValid;
}

void ListData::setUpdateOnlyOnClose(const bool updateOnlyOnClose)
{
    m_updateOnlyOnClose = updateOnlyOnClose;
}

void ListData::setCurrentText(const QString &str)
{
    QStandardItem *item = m_data->item(m_currentRow);

    if (item != 0)
    {
        item->setText(str);
    }
}

void ListData::clear()
{
    m_data->clear();
    m_currentRow = 0;
    m_min = 0;
    m_max = 0;
    m_minValid = false;
    m_maxValid = false;
    m_updateOnlyOnClose = false;
}
