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

#ifndef CONNECTION_H
#define CONNECTION_H

#include <QWidget>
#include <QLabel>
#include <QLineEdit>
#include <QPushButton>
#include <QGridLayout>
#include <QPaintEvent>
#include <QUdpSocket>
#include <QTimer>
#include "combobox.h"
#include "listwidget.h"

static const int TCP_PORT = 1111;
static const int UDP_PORT = 1112;

enum ConnectionStatus
{
    CONNECTED,
    CONNECTING,
    DISCONNECTED,
    ERROR
};

class Connection : public QWidget
{
    Q_OBJECT

    public:
        explicit Connection(QWidget *parent = 0);
        ~Connection();
        bool connectionIsTCP() const;
        QString connectionID() const;
        void writeDatagram(const char *data, size_t len);


    public slots:
        void updateConnectionStatus(ConnectionStatus status);
        void updateConnectionType(int index = 0);


    signals:
        void connectClicked(const bool connect);


    private slots:
        void setConnectionId(QListWidgetItem *connectionListItem);
        void connectFromList(QListWidgetItem *connectionListItem);
        void searchForCameras();
        void processPendingDatagrams();
        void discoveryStepAndFinalize();
        void incrementSessionDuration();
        void enableOrDisableConnectButton(const QString &connectionId);
        void toggleConnection();


    private:
        static const int MAX_BROADCAST_COUNT = 5;
        static const int BROADCAST_TIMEOUT = 500;
        static const int SESSION_TIMEOUT = 1000;

        ConnectionStatus m_status;
        bool m_connected;
        bool m_searching;
        QUdpSocket *m_udpSocket;
        QTimer *m_broadcastTimer;
        QTimer *m_sessionTimer;
        unsigned long m_sessionDuration;
        bool m_serialConnectionFound;
        int m_broadcastCount;

        QLabel *m_title;
        ComboBox *m_connectionType;
        QLineEdit *m_connectionId;
        QPushButton *m_connect;
        QLabel *m_connectionStatusTitle;
        QLabel *m_connectionDurationTitle;
        QLabel *m_connectionStatus;
        QLabel *m_connectionDuration;
        QLabel *m_connectionListTitle;
        ListWidget *m_connectionList;
        QLabel *m_connectionListStatus;
        QPushButton *m_refreshConnectionList;
        QGridLayout *m_layout;

        static void broadcastDatagram(const char *data, size_t len, void *user_data);
        QSize sizeHint() const;
        void paintEvent(QPaintEvent *event);
        void addConnectionToList(const QString &id, const QString &property1, const QString &property2, const QString &property3);
        void createUDPSocket();
        void destroyUDPSocket();
        void constructUI();
};

#endif // CONNECTION_H
