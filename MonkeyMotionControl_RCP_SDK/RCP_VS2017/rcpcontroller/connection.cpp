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

#include <QCommonStyle>
#include <QApplication>
#include <QPainter>
#include "qserialportinfo.h"
#include "logger.h"
#include "connection.h"
#include "rcp_api/rcp_api.h"

Connection::Connection(QWidget *parent) : QWidget(parent)
{
    m_status = DISCONNECTED;
    m_connected = false;
    m_searching = false;
    m_udpSocket = 0;
    m_broadcastTimer = new QTimer(this);
    m_sessionTimer = new QTimer(this);
    m_sessionDuration = 0;
    m_serialConnectionFound = false;
    m_broadcastCount = 0;

    m_title = new QLabel(this);
    m_connectionType = new ComboBox(this);
    m_connectionId = new QLineEdit(this);
    m_connect = new QPushButton(this);
    m_connectionStatusTitle = new QLabel(this);
    m_connectionDurationTitle = new QLabel(this);
    m_connectionStatus = new QLabel(this);
    m_connectionDuration = new QLabel(this);
    m_connectionListTitle = new QLabel(this);
    m_connectionList = new ListWidget(this);
    m_connectionListStatus = new QLabel(this);
    m_refreshConnectionList = new QPushButton(this);
    m_layout = new QGridLayout(this);

    constructUI();

    connect(m_connectionList, SIGNAL(itemSelected(QListWidgetItem*)), this, SLOT(setConnectionId(QListWidgetItem*)));
    connect(m_connectionList, SIGNAL(itemConfirmed(QListWidgetItem*)), this, SLOT(connectFromList(QListWidgetItem*)));
    connect(m_refreshConnectionList, SIGNAL(clicked()), this, SLOT(searchForCameras()));
    connect(m_broadcastTimer, SIGNAL(timeout()), this, SLOT(discoveryStepAndFinalize()));
    connect(m_sessionTimer, SIGNAL(timeout()), this, SLOT(incrementSessionDuration()));
    connect(m_connectionType, SIGNAL(currentIndexChanged(int)), this, SLOT(updateConnectionType(int)));
    connect(m_connectionId, SIGNAL(textChanged(const QString &)), this, SLOT(enableOrDisableConnectButton(const QString &)));
    connect(m_connect, SIGNAL(clicked()), this, SLOT(toggleConnection()));
}

Connection::~Connection()
{
    destroyUDPSocket();
    delete m_broadcastTimer;
    delete m_sessionTimer;
    delete m_title;
    delete m_connectionType;
    delete m_connectionId;
    delete m_connect;
    delete m_connectionStatusTitle;
    delete m_connectionDurationTitle;
    delete m_connectionStatus;
    delete m_connectionDuration;
    delete m_connectionListTitle;
    delete m_connectionList;
    delete m_connectionListStatus;
    delete m_refreshConnectionList;
    delete m_layout;
}

bool Connection::connectionIsTCP() const
{
    return m_connectionType->itemData(m_connectionType->currentIndex()).toBool();
}

QString Connection::connectionID() const
{
    return m_connectionId->text();
}

void Connection::writeDatagram(const char *data, size_t len)
{
    if (m_udpSocket != 0)
    {
        m_udpSocket->writeDatagram(data, len, QHostAddress::Broadcast, UDP_PORT);
    }
}

void Connection::updateConnectionStatus(ConnectionStatus status)
{
    QCommonStyle icon;

    m_status = status;

    switch (m_status)
    {
        case CONNECTED:
            m_connected = true;
            m_broadcastTimer->stop();
            m_broadcastCount = 0;
            m_searching = false;
            m_refreshConnectionList->setText(tr("Refresh List"));
            destroyUDPSocket();
            m_sessionTimer->start(SESSION_TIMEOUT);
            m_connect->setText(tr("Disconnect"));
            m_connect->setIcon(icon.standardIcon(QStyle::SP_DialogCancelButton));
            m_connectionStatus->setStyleSheet("color: #00FF00");
            m_connectionStatus->setText(tr("Connected"));
            m_connectionStatus->setToolTip(tr("Connected to %1").arg(connectionID()));
            m_connect->setEnabled(true);
            m_connectionList->setEnabled(false);
            m_refreshConnectionList->setEnabled(false);
            break;

        case CONNECTING:
            m_connected = false;
            m_broadcastTimer->stop();
            m_broadcastCount = 0;
            m_searching = false;
            m_refreshConnectionList->setText(tr("Refresh List"));
            destroyUDPSocket();
            m_connectionStatus->setStyleSheet("color: #00FFFF");
            m_connectionStatus->setText(tr("Connecting..."));
            m_connectionStatus->setToolTip(m_connectionStatus->text());
            m_connect->setEnabled(false);
            m_connectionType->setEnabled(false);
            m_connectionId->setEnabled(false);
            m_connectionList->setEnabled(false);
            m_refreshConnectionList->setEnabled(false);
            break;

        case DISCONNECTED:
            m_connected = false;
            m_sessionTimer->stop();
            m_sessionDuration = 0;
            m_connectionDuration->setText("00:00:00");
            m_connect->setText(tr("  Connect "));
            m_connect->setIcon(icon.standardIcon(QStyle::SP_DialogOkButton));
            m_connectionStatus->setStyleSheet("color: #FF0000");
            m_connectionStatus->setText(tr("Not connected"));
            m_connectionStatus->setToolTip(m_connectionStatus->text());
            m_connect->setEnabled(true);
            m_connectionType->setEnabled(true);
            m_connectionId->setEnabled(true);
            m_connectionList->setEnabled(true);
            searchForCameras();
            break;

        case ERROR:
            m_connected = false;
            m_sessionTimer->stop();
            m_sessionDuration = 0;
            m_connectionStatus->setStyleSheet("color: #FF0000");
            m_connectionStatus->setText(tr("Failed to connect"));
            m_connectionStatus->setToolTip(tr("Failed to connect to '%1'").arg(connectionID()));
            m_connect->setEnabled(true);
            m_connectionType->setEnabled(true);
            m_connectionId->setEnabled(true);
            m_connectionList->setEnabled(true);
            searchForCameras();
            break;

        default:
            Logger::logError(QString("Invalid connection status '%1' specified").arg(status), "Connection::updateConnectionStatus(ConnectionStatus)");
            break;
    }

    QApplication::processEvents(); // forces the color changes of the connection status message
}

void Connection::updateConnectionType(int index /*= 0*/)
{
    m_connectionId->setText("");

    if (connectionIsTCP())
    {
        m_connectionListTitle->setText(tr("Cameras"));
        m_connectionId->setPlaceholderText(tr("IP Address"));
    }
    else
    {
        m_connectionListTitle->setText("Serial Ports");
        m_connectionId->setPlaceholderText(tr("Port Name"));
    }

    searchForCameras();
}

void Connection::setConnectionId(QListWidgetItem *connectionListItem)
{
    if (connectionListItem == 0)
    {
        m_connectionId->setText("");
    }
    else
    {
        m_connectionId->setText(connectionListItem->data(Qt::UserRole).toString());
    }
}

void Connection::connectFromList(QListWidgetItem* /*connectionListItem*/)
{
    emit connectClicked(true);
}

void Connection::searchForCameras()
{
    m_connectionList->clear();
    m_connectionListStatus->setText(tr("Searching..."));

    if (connectionIsTCP())
    {
        if (!m_searching)
        {
            m_searching = true;
            m_refreshConnectionList->setEnabled(false);
            m_refreshConnectionList->setText(m_connectionListStatus->text());
            m_connectionList->setFocus();
            m_broadcastTimer->stop();
            m_broadcastCount = 0;

            createUDPSocket();

            rcp_discovery_start(broadcastDatagram, (void*)this);
            m_broadcastTimer->start(RCP_DISCOVERY_STEP_SLEEP_MS);
        }
    }
    else
    {
        m_searching = false;
        m_refreshConnectionList->setEnabled(true);
        m_refreshConnectionList->setText(tr("Refresh List"));
        m_broadcastTimer->stop();
        m_broadcastCount = 0;

        QList<QSerialPortInfo> ports = QSerialPortInfo::availablePorts();
        QList<QSerialPortInfo>::const_iterator i;

        for (i = ports.begin(); i != ports.end(); i++)
        {
            addConnectionToList(i->portName(), i->portName(), i->manufacturer(), i->description());
            m_serialConnectionFound = true;
        }

        if (!m_serialConnectionFound)
        {
            m_connectionListStatus->setText(tr("No available serial ports"));
        }
    }
}

// Reads the responses (if any) from broadcastDatagram() by the cameras on the network
void Connection::processPendingDatagrams()
{
    if (m_udpSocket != 0)
    {
        if (m_udpSocket->state() != QAbstractSocket::UnconnectedState)
        {
            while (m_udpSocket->hasPendingDatagrams())
            {
                QHostAddress address;
                QByteArray datagram;
                datagram.resize(m_udpSocket->pendingDatagramSize());
                char *data = (char*)malloc(datagram.size());

                if (m_udpSocket->readDatagram(data, datagram.size(), &address, 0) > -1)
                {
                    char *addr = (char*)calloc(256, sizeof(char));
                    strncpy(addr, address.toString().toStdString().c_str(), 256);

                    rcp_discovery_process_data(data, datagram.size(), addr);

                    free(addr);
                }

                free(data);
            }
        }
    }
}

void Connection::discoveryStepAndFinalize()
{
    rcp_discovery_step();
    m_broadcastCount++;

    if (m_broadcastCount >= RCP_DISCOVERY_STEP_LOOP_COUNT)
    {
        bool connectionFound = false;
        m_broadcastTimer->stop();
        m_broadcastCount = 0;
        m_searching = false;

        if (m_status != CONNECTING && m_status != CONNECTED)
        {
            m_refreshConnectionList->setEnabled(true);
        }

        m_refreshConnectionList->setText(tr("Refresh List"));
        destroyUDPSocket();

        rcp_discovery_cam_info_list_t *cam_list = rcp_discovery_get_list();
        const rcp_discovery_cam_info_list_t *cur = cam_list;

        while (cur)
        {
            connectionFound = true;

            QString camInterface;

            switch (cur->info.rcp_interface)
            {
                case RCP_INTERFACE_UNKNOWN:
                    camInterface = "???";
                    break;

                case RCP_INTERFACE_BRAIN_SERIAL:
                    camInterface = "Brain Serial";
                    break;

                case RCP_INTERFACE_BRAIN_GIGABIT_ETHERNET:
                    camInterface = "Brain GigE";
                    break;

                case RCP_INTERFACE_REDLINK_BRIDGE:
                    camInterface = "REDLINK BRIDGE";
                    break;

                default:
                    break;
            }

            addConnectionToList(QString("%1").arg(cur->ip_address), QString("%1").arg(cur->info.id), camInterface, QString("%1").arg(cur->info.pin));
            cur = cur->next;
        }

        rcp_discovery_free_list(cam_list);
        rcp_discovery_end();

        if (!connectionFound)
        {
            m_connectionListStatus->setText(tr("No cameras found"));
        }
    }
}

void Connection::incrementSessionDuration()
{
    m_sessionDuration += SESSION_TIMEOUT;

    QString seconds, minutes, hours;

    int val = m_sessionDuration / 1000;
    const int s = val % 60;

    val /= 60;
    const int m = val % 60;

    val /= 60;
    const int h = val % 60;

    // Pads the seconds/minutes/hours strings with a 0 if any of them are only one digit long
    seconds.sprintf((s < 10) ? "0%d" : "%d", s);
    minutes.sprintf((m < 10) ? "0%d" : "%d", m);
    hours.sprintf((h < 10) ? "0%d" : "%d", h);

    m_connectionDuration->setText(QString("%1:%2:%3").arg(hours).arg(minutes).arg(seconds));
}

void Connection::enableOrDisableConnectButton(const QString &connectionId)
{
    if (connectionId.trimmed().isEmpty())
    {
        m_connect->setEnabled(false);
    }
    else
    {
        m_connect->setEnabled(true);
    }
}

void Connection::toggleConnection()
{
    if (m_connected)
    {
        emit connectClicked(false);
    }
    else
    {
        emit connectClicked(true);
    }
}

// Broadcasts a "GET CAMINFO" RCP message over UDP so cameras on the network can respond with their information
void Connection::broadcastDatagram(const char *data, size_t len, void *user_data)
{
    Connection *connection = (Connection*)user_data; // refers to "this"
    connection->writeDatagram(data, len);
}

QSize Connection::sizeHint() const
{
    return QSize(450, 240);
}

void Connection::paintEvent(QPaintEvent* /*event*/)
{
    QPainter painter;
    painter.begin(this);
    painter.fillRect(rect(), QColor("#404040"));
}

void Connection::addConnectionToList(const QString &id, const QString &property1, const QString &property2, const QString &property3)
{
    int i;
    const int total = m_connectionList->count();
    QString connectionString = property1.trimmed();
    QString prop2 = property2.trimmed();
    QString prop3 = property3.trimmed();

    if (!prop2.isEmpty())
    {
        connectionString.append("\n     ");
        connectionString.append(prop2);
    }

    if (!prop3.isEmpty())
    {
        connectionString.append("\n     ");
        connectionString.append(prop3);
    }

    for (i = 0; i < total; i++)
    {
        QListWidgetItem *item = m_connectionList->item(i);

        if (item != 0)
        {
            // If the connection already exists, ignore it
            if (item->text().compare(connectionString) == 0)
            {
                return;
            }
        }
    }

    m_connectionListStatus->setText("");

    QListWidgetItem *connection = new QListWidgetItem(connectionString);
    connection->setData(Qt::UserRole, id);
    m_connectionList->addItem(connection);
}

void Connection::createUDPSocket()
{
    destroyUDPSocket();
    m_udpSocket = new QUdpSocket;
    m_udpSocket->bind(UDP_PORT, QUdpSocket::DefaultForPlatform);
    connect(m_udpSocket, SIGNAL(readyRead()), this, SLOT(processPendingDatagrams()));
}

void Connection::destroyUDPSocket()
{
    if (m_udpSocket != 0)
    {
        m_udpSocket->abort();
        delete m_udpSocket;
        m_udpSocket = 0;
    }
}

void Connection::constructUI()
{
    m_title->setObjectName("title");
    m_connectionListTitle->setObjectName("subtitle");

    m_title->setText(tr("Connection"));
    m_connectionStatusTitle->setText(tr("Status:"));
    m_connectionDurationTitle->setText(tr("Duration:"));

    QCommonStyle icon;
    m_refreshConnectionList->setIcon(icon.standardIcon(QStyle::SP_BrowserReload));

    // The connect button temporarily receives the longest string it can contain ("Disconnect") in order to find its maximum size; once that size is applied, it will not change width as its text changes
    QString connectText = m_connect->text();
    QIcon connectIcon = m_connect->icon();
    m_connect->setIcon(icon.standardIcon(QStyle::SP_DialogCancelButton));
    m_connect->setText(tr("Disconnect"));
    m_connect->adjustSize();
    m_connect->setFixedWidth(m_connect->width());
    m_connect->setText(connectText);
    m_connect->setIcon(connectIcon);

    // The second boolean argument is used to determine the connection type (TCP or serial) without relying on string comparisons
    m_connectionType->addItem(tr("GigE Ethernet (TCP)"), true);
    m_connectionType->addItem(tr("Serial Port"), false);

#ifdef Q_OS_MAC
    m_connectionId->setAttribute(Qt::WA_MacShowFocusRect, false);
    m_connectionList->setAttribute(Qt::WA_MacShowFocusRect, false);
#endif

    // The UI is set up so the connection type/connection id/connection status etc. section takes up 2/3 of the layout while the connection list takes up 1/3 of the layout
    int row = 0;
    int listRow = 0;
    const int colCount = 6; // needs to be divisible by 3

    m_layout->addWidget(m_title, row++, 0, 1, colCount, Qt::AlignHCenter);
    m_layout->addWidget(m_connectionListTitle, row++, (colCount / 3) * 2, 1, colCount / 3, Qt::AlignHCenter);
    listRow = row;
    m_layout->addWidget(m_connectionType, row++, 0, 1, (colCount / 3) * 2);
    m_layout->addWidget(m_connectionId, row++, 0, 1, (colCount / 3) * 2);
    m_layout->addWidget(m_connect, row++, colCount / 3, 1, colCount / 3, Qt::AlignRight);

    row += 4; // forces the status/duration to the bottom

    m_layout->addWidget(m_connectionStatusTitle, row, 0, 1, colCount / 3, Qt::AlignLeft);
    m_layout->addWidget(m_connectionStatus, row++, colCount / 3, 1, colCount / 3, Qt::AlignRight);
    m_layout->addWidget(m_connectionDurationTitle, row, 0, 1, colCount / 3, Qt::AlignLeft);
    m_layout->addWidget(m_connectionDuration, row, colCount / 3, 1, colCount / 3, Qt::AlignRight);

    m_layout->addWidget(m_connectionList, listRow, (colCount / 3) * 2, row - 1, colCount / 3);
    m_layout->addWidget(m_connectionListStatus, listRow, (colCount / 3) * 2, row - 1, colCount / 3, Qt::AlignCenter);
    m_layout->addWidget(m_refreshConnectionList, listRow + row, (colCount / 3) * 2, 1, colCount / 3);

    setLayout(m_layout);
    adjustSize();
    setSizePolicy(QSizePolicy::Minimum, QSizePolicy::Fixed);
}
