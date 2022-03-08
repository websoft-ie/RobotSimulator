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

#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QtWidgets/QMainWindow>
#include <QTcpSocket>
#include <QTimer>
#include <QFrame>
#include <QScrollArea>
#include <QGridLayout>
#include <QCloseEvent>
#include <QMessageBox>
#include "qserialport.h"
#include "rcp_api/rcp_api.h"
#include "connection.h"
#include "controller.h"

class MainWindow : public QMainWindow
{
    Q_OBJECT

    public:
        MainWindow(QWidget *parent = 0);
        ~MainWindow();
        void setAppOutOfDate(const bool outOfDate);
        void stopStateTimer();
        void startExternalTimer();
        void intReceived(const rcp_cur_int_cb_data_t *data);
        void listReceived(const rcp_cur_list_cb_data_t *data);
        void stringReceived(const rcp_cur_str_cb_data_t *data);


    public slots:
        rcp_error_t sendRCP(const char *data, size_t len);
        void dropConnection();
        void rcpGet(rcp_param_t paramID);


    signals:
        void intUpdated(const int value, const rcp_param_t paramID);
        void listUpdated(const rcp_cur_list_cb_data_t *data, const bool updateOnlyOnClose);
        void strUpdated(const QString &str, const rcp_param_status_t status, const rcp_param_t paramID);


    private slots:
        void toggleConnection(const bool connect);
        void connectToCamera();
        void readRCP();
        void socketError(QAbstractSocket::SocketError errorCode);
        void serialProtocolError();
        void externalControlError();
        void rcpSetInt(rcp_param_t paramID, const int value);
        void rcpSetStr(rcp_param_t paramID, const QString &str);
        void rcpGetList(rcp_param_t paramID);


    private:
        static const int READ_TIMEOUT = 1500;
        rcp_camera_connection_t *m_rcpConnection;
        QTcpSocket *m_tcpSocket;
        QSerialPort *m_serialPort;
        QTimer *m_readTimer;
        QTimer *m_stateTimer;
        QTimer *m_externalTimer;

        bool m_connected;
        bool m_appOutOfDate;
        QMessageBox *m_outOfDateWarning;

        QFrame *m_frame;
        QScrollArea *m_scrollArea;
        Connection *m_connection;
        Controller *m_controller;
        QGridLayout *m_layout;

        static rcp_error_t sendData(const char *data, size_t len, void *user_data);
        static void intReceived(const rcp_cur_int_cb_data_t *data, void *user_data);
        static void listReceived(const rcp_cur_list_cb_data_t *data, void *user_data);
        static void histogramReceived(const rcp_cur_hist_cb_data_t *data, void *user_data);
        static void stringReceived(const rcp_cur_str_cb_data_t *data, void *user_data);
        static void stateUpdated(const rcp_state_data_t *data, void *user_data);
        QSize sizeHint() const;
        QString appVersion() const;
        void closeEvent(QCloseEvent *event);
        void sendInitialGets();
        void initializeRCPConnection();
        void updateWindowTitle(const bool connected);
        void constructUI();
};

#endif // MAINWINDOW_H
