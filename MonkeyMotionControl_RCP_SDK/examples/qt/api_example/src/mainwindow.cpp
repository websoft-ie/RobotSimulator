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

#include <QCoreApplication>
#include "mainwindow.h"
#include "qtutil.h"
#include "logger.h"

MainWindow::MainWindow(QWidget *parent /*= 0*/) : QMainWindow(parent)
{
    m_rcpConnection = 0;
    m_tcpSocket = new QTcpSocket;
    m_serialPort = new QSerialPort;
    m_readTimer = new QTimer;
    m_stateTimer = new QTimer;
    m_externalTimer = new QTimer;

    m_connected = false;
    m_appOutOfDate = false;
    m_outOfDateWarning = new QMessageBox(QMessageBox::Warning, tr("Warning"), tr("This version of the %1 is not fully supported by your camera's firmware. The application will still work but there may be features on the camera that it cannot support.").arg(QCoreApplication::applicationName()), QMessageBox::Ok, this);

    m_frame = new QFrame(this);
    m_scrollArea = new QScrollArea(m_frame);
    m_connection = new Connection(m_frame);
    m_controller = new Controller(m_scrollArea);
    m_layout = new QGridLayout(m_frame);

    constructUI();

    connect(m_tcpSocket, SIGNAL(readyRead()), this, SLOT(readRCP()));
    connect(m_tcpSocket, SIGNAL(disconnected()), this, SLOT(dropConnection()));
    connect(m_tcpSocket, SIGNAL(error(QAbstractSocket::SocketError)), this, SLOT(socketError(QAbstractSocket::SocketError)));
    connect(m_serialPort, SIGNAL(readyRead()), this, SLOT(readRCP()));
    connect(m_readTimer, SIGNAL(timeout()), this, SLOT(dropConnection()));
    connect(m_stateTimer, SIGNAL(timeout()), this, SLOT(serialProtocolError()));
    connect(m_externalTimer, SIGNAL(timeout()), this, SLOT(externalControlError()));
    connect(m_connection, SIGNAL(connectClicked(const bool)), this, SLOT(toggleConnection(const bool)));

    connect(this, SIGNAL(intUpdated(const int, const rcp_param_t)), m_controller, SLOT(updateInt(const int, const rcp_param_t)));
    connect(this, SIGNAL(listUpdated(const rcp_cur_list_cb_data_t*, const bool)), m_controller, SLOT(updateList(const rcp_cur_list_cb_data_t*, const bool)));
    connect(this, SIGNAL(strUpdated(const QString&, const rcp_param_status_t, const rcp_param_t)), m_controller, SLOT(updateString(const QString&, const rcp_param_status_t, const rcp_param_t)));

    connect(m_controller, SIGNAL(stringSent(rcp_param_t, const QString&)), this, SLOT(rcpSetStr(rcp_param_t, const QString&)));
    connect(m_controller, SIGNAL(intSent(rcp_param_t, const int)), this, SLOT(rcpSetInt(rcp_param_t, const int)));
    connect(m_controller, SIGNAL(listNeeded(rcp_param_t)), this, SLOT(rcpGetList(rcp_param_t)));
    connect(m_controller, SIGNAL(stringNeeded(rcp_param_t)), this, SLOT(rcpGet(rcp_param_t)));

    m_connection->updateConnectionType();
    dropConnection();
}

MainWindow::~MainWindow()
{
    if (m_rcpConnection != 0)
    {
        rcp_delete_camera_connection(m_rcpConnection);
    }

    delete m_tcpSocket;
    delete m_serialPort;
    delete m_readTimer;
    delete m_stateTimer;
    delete m_externalTimer;
    delete m_outOfDateWarning;
    delete m_connection;
    delete m_controller;
    delete m_layout;
    delete m_frame;
}

void MainWindow::setAppOutOfDate(const bool outOfDate)
{
    m_appOutOfDate = outOfDate;
}

void MainWindow::stopStateTimer()
{
    m_stateTimer->stop();
}

void MainWindow::startExternalTimer()
{
    m_externalTimer->start(READ_TIMEOUT);
}

void MainWindow::intReceived(const rcp_cur_int_cb_data_t *data)
{
    if (data->id == RCP_PARAM_RECORD_STATE && !m_connected)
    {
        m_connected = true;
        m_externalTimer->stop();
        m_controller->constructUI();
        m_controller->setEnabled(true);
        m_connection->updateConnectionStatus(CONNECTED);
        sendInitialGets();
        updateWindowTitle(true);

        if (m_appOutOfDate)
        {
            // Using the static QMessageBox::warning() won't work well here because it uses the QMessageBox::exec() function rather than QMessageBox::show(). Using exec() freezes up the main event loop and stops the application from reading incoming RCP messages, thus dropping the connection.
            m_outOfDateWarning->show();
        }
    }

    if (data->cur_val_valid)
    {
        emit intUpdated(data->cur_val, data->id);
    }

    if (data->display_str_valid)
    {
        emit strUpdated(QString("%1").arg(data->display_str_decoded), data->display_str_status, data->id);
    }

    if (data->display_str_in_list)
    {
        rcp_get_list(m_rcpConnection, data->id);
    }
}

void MainWindow::listReceived(const rcp_cur_list_cb_data_t *data)
{
    if (data->list_string_valid)
    {
        emit listUpdated(data, rcp_get_update_list_only_on_close(m_rcpConnection, data->id));
    }
    else
    {
        Logger::logError(QString("The list string is invalid: %1").arg(data->list_string), "MainWindow::listReceived(const rcp_cur_list_cb_data_t*)");
    }
}

void MainWindow::stringReceived(const rcp_cur_str_cb_data_t *data)
{
    emit strUpdated(QString("%1").arg(data->display_str_abbr_decoded), data->display_str_status, data->id);
}

rcp_error_t MainWindow::sendRCP(const char *data, size_t len)
{
    rcp_error_t err = RCP_SUCCESS;
    char *msg = (char*)calloc(len + 1, sizeof(char));
    strncpy(msg, data, len);

    if (m_connection->connectionIsTCP())
    {
        if (m_tcpSocket->write(msg) == -1)
        {
            Logger::logError("Failed to write to TCP socket", "MainWindow::sendRCP(const char*, size_t)");
            err = RCP_ERROR_SEND_DATA_TO_CAM_FAILED;
        }

        m_tcpSocket->flush();
    }
    else
    {
        if (m_serialPort->write(msg) == -1)
        {
            Logger::logError("Failed to write to serial port", "MainWindow::sendRCP(const char*, size_t)");
            err = RCP_ERROR_SEND_DATA_TO_CAM_FAILED;
        }

        m_serialPort->flush();
    }

    free(msg);
    return err;
}

void MainWindow::dropConnection()
{
    if (m_rcpConnection != 0)
    {
        rcp_delete_camera_connection(m_rcpConnection);
        m_rcpConnection = 0;
    }

    if (m_tcpSocket->isOpen())
    {
        Logger::logInfo("TCP connection dropped", "MainWindow::dropConnection()");
        m_tcpSocket->close();
    }

    if (m_serialPort->isOpen())
    {
        Logger::logInfo("Serial connection dropped", "MainWindow::dropConnection()");
        m_serialPort->close();
    }

    m_connected = false;
    m_appOutOfDate = false;
    m_readTimer->stop();
    m_controller->initialize();
    m_controller->setEnabled(false);
    m_connection->updateConnectionStatus(DISCONNECTED);
    updateWindowTitle(false);
}

void MainWindow::rcpGet(rcp_param_t paramID)
{
    rcp_get(m_rcpConnection, paramID);
}

void MainWindow::toggleConnection(const bool connect)
{
    if (connect)
    {
        connectToCamera();
    }
    else
    {
        dropConnection();
    }
}

void MainWindow::connectToCamera()
{
    Logger::logInfo(QString("Connecting to %1").arg(m_connection->connectionID()), "MainWindow::connectToCamera()");

    m_connection->updateConnectionStatus(CONNECTING);

    if (m_rcpConnection != 0)
    {
        rcp_delete_camera_connection(m_rcpConnection);
        m_rcpConnection = 0;
    }

    if (m_tcpSocket->isOpen())
    {
        Logger::logInfo("TCP connection dropped", "MainWindow::dropConnection()");
        m_tcpSocket->close();
    }

    if (m_serialPort->isOpen())
    {
        Logger::logInfo("Serial connection dropped", "MainWindow::dropConnection()");
        m_serialPort->close();
    }

    if (m_connection->connectionIsTCP())
    {
        m_tcpSocket->connectToHost(m_connection->connectionID(), TCP_PORT, QIODevice::ReadWrite);

        if (m_tcpSocket->waitForConnected(5000))
        {
            Logger::logInfo("TCP connection established", "MainWindow::connectToCamera()");
            initializeRCPConnection();
        }
        else
        {
            dropConnection();
            m_connection->updateConnectionStatus(ERROR);
            Logger::logError(QString("Unable to connect to %1").arg(m_connection->connectionID()), "MainWindow::connectToCamera()");
        }
    }
    else
    {
        m_serialPort->setPortName(m_connection->connectionID());

        if (!m_serialPort->open(QIODevice::ReadWrite))
        {
            dropConnection();
            m_connection->updateConnectionStatus(ERROR);
            Logger::logError(QString("Unable to open serial port %1. Error: %2").arg(m_connection->connectionID()).arg(m_serialPort->errorString()), "MainWindow::connectToCamera()");
            return;
        }

        if (!m_serialPort->setBaudRate(QSerialPort::Baud115200))
        {
            dropConnection();
            m_connection->updateConnectionStatus(ERROR);
            Logger::logError(QString("Unable to set baud rate to 115200 on serial port %1. Error: %2").arg(m_connection->connectionID()).arg(m_serialPort->errorString()), "MainWindow::connectToCamera()");
            return;
        }

        if (!m_serialPort->setDataBits(QSerialPort::Data8))
        {
            dropConnection();
            m_connection->updateConnectionStatus(ERROR);
            Logger::logError(QString("Unable to set data bits to 8 on serial port %1. Error: %2").arg(m_connection->connectionID()).arg(m_serialPort->errorString()), "MainWindow::connectToCamera()");
            return;
        }

        if (!m_serialPort->setParity(QSerialPort::NoParity))
        {
            dropConnection();
            m_connection->updateConnectionStatus(ERROR);
            Logger::logError(QString("Unable to set no parity to serial port %1. Error: %2").arg(m_connection->connectionID()).arg(m_serialPort->errorString()), "MainWindow::connectToCamera()");
            return;
        }

        if (!m_serialPort->setStopBits(QSerialPort::OneStop))
        {
            dropConnection();
            m_connection->updateConnectionStatus(ERROR);
            Logger::logError(QString("Unable to set one stop bit on serial port %1. Error: %2").arg(m_connection->connectionID()).arg(m_serialPort->errorString()), "MainWindow::connectToCamera()");
            return;
        }

        if (!m_serialPort->setFlowControl(QSerialPort::NoFlowControl))
        {
            dropConnection();
            m_connection->updateConnectionStatus(ERROR);
            Logger::logError(QString("Unable to set no flow control to serial port %1. Error: %2").arg(m_connection->connectionID()).arg(m_serialPort->errorString()), "MainWindow::connectToCamera()");
            return;
        }

        Logger::logInfo("Serial connection established", "MainWindow::connectToCamera()");
        initializeRCPConnection();
    }
}

void MainWindow::readRCP()
{
    // The timer is reset each time a message is received
    m_readTimer->start(READ_TIMEOUT);

    QByteArray data = m_connection->connectionIsTCP() ? m_tcpSocket->readAll() : m_serialPort->readAll();

    if (m_rcpConnection)
    {
        rcp_process_data(m_rcpConnection, data.data(), data.length());
    }
}

void MainWindow::socketError(QAbstractSocket::SocketError errorCode)
{
    switch (errorCode)
    {
        case QAbstractSocket::ConnectionRefusedError:
            Logger::logError("The connection was refused by the peer (or timed out).", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::RemoteHostClosedError:
            Logger::logError("The remote host closed the connection. Note that the client socket (i.e., this socket) will be closed after the remote close notification has been sent.", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::HostNotFoundError:
            Logger::logError("The host address was not found.", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::SocketAccessError:
            Logger::logError("The socket operation failed because the application lacked the required privileges.", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::SocketResourceError:
            Logger::logError("The local system ran out of resources (e.g., too many sockets).", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::SocketTimeoutError:
            Logger::logError("The socket operation timed out.", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::DatagramTooLargeError:
            Logger::logError("The datagram was larger than the operating system's limit (which can be as low as 8192 bytes).", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::NetworkError:
            Logger::logError("An error occurred with the network (e.g., the network cable was accidentally plugged out).", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::AddressInUseError:
            Logger::logError("The address specified to QUdpSocket::bind() is already in use and was set to be exclusive.", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::SocketAddressNotAvailableError:
            Logger::logError("The address specified to QUdpSocket::bind() does not belong to the host.", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::UnsupportedSocketOperationError:
            Logger::logError("The requested socket operation is not supported by the local operating system (e.g., lack of IPv6 support).", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::ProxyAuthenticationRequiredError:
            Logger::logError("The socket is using a proxy, and the proxy requires authentication.", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::SslHandshakeFailedError:
            Logger::logError("The SSL/TLS handshake failed, so the connection was closed (only used in QSslSocket).", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::UnfinishedSocketOperationError:
            Logger::logError("Used by QAbstractSocketEngine only, the last operation attempted has not finished yet (still in progress in the background).", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::ProxyConnectionRefusedError:
            Logger::logError("Could not contact the proxy server because the connection to that server was denied.", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::ProxyConnectionClosedError:
            Logger::logError("The connection to the proxy server was closed unexpectedly (before the connection to the final peer was established).", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::ProxyConnectionTimeoutError:
            Logger::logError("The connection to the proxy server timed out or the proxy server stopped responding in the authentication phase.", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::ProxyNotFoundError:
            Logger::logError("The proxy address set with setProxy() (or the application proxy) was not found.", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::ProxyProtocolError:
            Logger::logError("The connection negotiation with the proxy server because the response from the proxy server could not be understood.", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        case QAbstractSocket::UnknownSocketError:
            Logger::logError("An unidentified error occurred.", "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;

        default:
            Logger::logError(QString("An undefined error code [%1] was specified.").arg(errorCode), "MainWindow::socketError(QAbstractSocket::SocketError)");
            break;
    }
}

void MainWindow::serialProtocolError()
{
    dropConnection();
    m_stateTimer->stop();

    if (m_connection->connectionIsTCP())
    {
        Logger::logError(QString("An error occurred while connecting to %1").arg(m_connection->connectionID()), "MainWindow::serialProtocolError()");
        QMessageBox::critical(this, tr("Error"), tr("An error occurred while connecting to your camera. Please ensure that the specified IP address belongs to your camera."));
    }
    else
    {
        Logger::logError("Serial protocol is not set to RED Control Protocol", "MainWindow::serialProtocolError()");
        QMessageBox::critical(this, tr("Error"), tr("An error occurred while connecting to your camera. Please ensure that the camera is connected to the specified serial port and that the Serial Protocol is set to RED Control Protocol (Menu > Settings > Setup > Communication > Serial)"));
    }
}

void MainWindow::externalControlError()
{
    dropConnection();
    m_externalTimer->stop();

    if (m_connection->connectionIsTCP())
    {
        Logger::logError("External control is disabled", "MainWindow::externalControlError()");
        QMessageBox::critical(this, tr("Error"), tr("An error occurred while connecting to your camera. Please ensure that external control is enabled (Menu > Settings > Setup > Communication > Network)"));
    }
    else
    {
        Logger::logError(QString("An error occurred while connecting to %1").arg(m_connection->connectionID()), "MainWindow::externalControlError()");
        QMessageBox::critical(this, tr("Error"), tr("An error occurred while connecting to your camera. Please ensure that the specified serial port belongs to your camera."));
    }
}

void MainWindow::rcpSetInt(rcp_param_t paramID, const int value)
{
    rcp_set_int(m_rcpConnection, paramID, value);
}

void MainWindow::rcpSetStr(rcp_param_t paramID, const QString &str)
{
    rcp_set_str(m_rcpConnection, paramID, str.toUtf8().constData());
}

void MainWindow::rcpGetList(rcp_param_t paramID)
{
    rcp_get_list(m_rcpConnection, paramID);
}

rcp_error_t MainWindow::sendData(const char *data, size_t len, void *user_data)
{
    MainWindow *window = (MainWindow*)user_data; // refers to "this"
    return window->sendRCP(data, len);
}

void MainWindow::intReceived(const rcp_cur_int_cb_data_t *data, void *user_data)
{
    MainWindow *window = (MainWindow*)user_data; // refers to "this"
    window->intReceived(data);
}

void MainWindow::listReceived(const rcp_cur_list_cb_data_t *data, void *user_data)
{
    MainWindow *window = (MainWindow*)user_data; // refers to "this"
    window->listReceived(data);
}

void MainWindow::histogramReceived(const rcp_cur_hist_cb_data_t *data, void *user_data)
{

}

void MainWindow::stringReceived(const rcp_cur_str_cb_data_t *data, void *user_data)
{
    MainWindow *window = (MainWindow*)user_data; // refers to "this"
    window->stringReceived(data);
}

void MainWindow::stateUpdated(const rcp_state_data_t *data, void *user_data)
{
    MainWindow *window = (MainWindow*)user_data; // refers to "this"
    window->stopStateTimer();

    switch (data->state)
    {
        case RCP_CONNECTION_STATE_INIT:
            break;

        case RCP_CONNECTION_STATE_CONNECTED:
            Logger::logInfo("RCP connection established", "MainWindow::stateUpdated(const rcp_state_data_t*, void*)");

            window->setAppOutOfDate(data->parameter_set_version_valid && data->parameter_set_newer);

            // Sending a GET RECORD and then verifying if external control is enabled based on whether or not a CURRENT RECORD is sent back
            window->rcpGet(RCP_PARAM_RECORD_STATE);
            window->startExternalTimer();
            break;

        case RCP_CONNECTION_STATE_ERROR_RCP_VERSION_MISMATCH:
            Logger::logError("The RCP version is invalid", "MainWindow::stateUpdated(const rcp_state_data_t*, void*)");
            window->dropConnection();
            QMessageBox::critical(window, "Error", "The RCP version of your camera is invalid. This application only supports RCP version 2.");
            break;

        case RCP_CONNECTION_STATE_ERROR_RCP_PARAMETER_SET_VERSION_MISMATCH:
            Logger::logError("The RCP parameter set version is invalid", "MainWindow::stateUpdated(const rcp_state_data_t*, void*)");
            window->dropConnection();
            QMessageBox::critical(window, "Error", "The RCP parameter set version of your camera is invalid.");
            break;

        case RCP_CONNECTION_STATE_COMMUNICATION_ERROR:
            Logger::logError("Communication with camera was lost", "MainWindow::stateUpdated(const rcp_state_data_t*, void*)");
            window->dropConnection();
            QMessageBox::critical(window, "Error", "Communication with your camera was lost.");
            break;

        default:
            break;
    }
}

QSize MainWindow::sizeHint() const
{
    return QSize(450, 700);
}

QString MainWindow::appVersion() const
{
    QString ver = QString("%1").arg(VER_STR);

    if (ver.isEmpty())
    {
        if (QString("%1").arg(VER_NO).isEmpty())
        {
            ver = "Local Engineering Build";
        }
        else
        {
            ver = QString("Engineering Build %1").arg(VER_NO);
        }
    }

    return ver;
}

void MainWindow::closeEvent(QCloseEvent *event)
{
    dropConnection();
    QMainWindow::closeEvent(event);
}

void MainWindow::sendInitialGets()
{
    int i, count, width;
    int maxWidth = 0;
    rcp_param_t *params = m_controller->params(&count);

    const char *label;
    QLabel widthLabel;

    for (i = 0; i < count; i++)
    {
        label = rcp_get_label(m_rcpConnection, params[i]);

        m_controller->setParamTitle(tr("%1").arg(label), params[i]);

        widthLabel.setText(tr("%1").arg(label));
        widthLabel.adjustSize();
        width = widthLabel.width();
        if (width > maxWidth)
        {
            maxWidth = width;
        }

        rcp_get_list(m_rcpConnection, params[i]);
        rcp_get(m_rcpConnection, params[i]);
    }

    free(params);

    m_controller->setParamTitleWidth(maxWidth);

    rcp_get(m_rcpConnection, RCP_PARAM_RECORD_STATE);
}

void MainWindow::initializeRCPConnection()
{
    QString appName = QCoreApplication::applicationName();
    QString ver = appVersion();

    rcp_camera_connection_info_t info =
    {
        appName.toUtf8().constData(),
        ver.toUtf8().constData(),
        NULL,
        sendData, (void*)this,
        intReceived, (void*)this,
        0, 0,                               // uint
        listReceived, (void*)this,
        histogramReceived, (void*)this,
        stringReceived, (void*)this,
        0, 0,                               // clip list
        0, 0,                               // frame tagged
        0, 0,                               // status
        0, 0,                               // notification
        0, 0,                               // audio vu
        0, 0,                               // menu tree
        0, 0,                               // menu tree node status
        0, 0,                               // rftp status
        0, 0,                               // user set
        0, 0,                               // user get
        0, 0,                               // user current
        0, 0,                               // user metadata
        0, 0,                               // default int
        0, 0,                               // default uint
        0, 0,                               // action list
        0, 0,                               // key mapping
        stateUpdated, (void*)this,
    };

    m_stateTimer->start(READ_TIMEOUT);
    m_rcpConnection = rcp_create_camera_connection(&info);
}

void MainWindow::updateWindowTitle(const bool connected)
{
    QString connection = connected ? QString(" - Connected to %1").arg(m_connection->connectionID()) : "";
    setWindowTitle(tr("%1%2 (%3)").arg(QCoreApplication::applicationName()).arg(connection).arg(appVersion()));
}

void MainWindow::constructUI()
{
    m_scrollArea->setWidget(m_controller);
    m_scrollArea->setWidgetResizable(true);

    m_layout->addWidget(m_connection, 0, 0, 1, 1);
    m_layout->addWidget(m_scrollArea, 1, 0, 1, 1);

    updateWindowTitle(false);
#if defined(Q_OS_WIN)
    setWindowIcon(QIcon("redlink.png"));
#elif defined(Q_OS_MAC)
    setWindowIcon(QIcon("../Resources/redlink.png"));
#endif
    setCentralWidget(m_frame);
    adjustSize();
    resize(size().width() + 20, size().height());
    move(QtUtil::desktopCenter(size()));
}
