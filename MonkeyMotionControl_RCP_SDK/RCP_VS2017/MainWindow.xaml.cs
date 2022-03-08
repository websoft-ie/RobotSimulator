using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace rcp_wpf
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum ConnectionStatus
        {
            CONNECTED = 0,
            CONNECTING,
            DISCONNECTED,
            ERROR,
        }

        public static int TCP_PORT = 1111;
        public static int UDP_PORT = 1112;
        static int READ_TIMEOUT = 1500;

        ConnectionStatus m_status = ConnectionStatus.DISCONNECTED;
        bool m_connected = false;
        bool m_appOutOfDate = false;
        bool m_searching = false;
        int m_broadcastCount = 0;
        bool m_serialConnectionFound = false;

        DispatcherTimer m_broadcastTimer = new DispatcherTimer();
        DispatcherTimer m_readTimer = new DispatcherTimer();
        DispatcherTimer m_stateTimer = new DispatcherTimer();
        DispatcherTimer m_externalTimer = new DispatcherTimer();

        TcpClient m_tcpSocket = new TcpClient();
        TcpListener tcpServer = null;
        Thread tcpThread = null;

        Thread udpThread = null;
        UdpClient m_udpSocket = new UdpClient(UDP_PORT);
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, UDP_PORT);
        SerialPort m_serialPort = new SerialPort();

        bool ConnectionIsTCP
        {
            get
            {
                return ConnectionType.SelectedIndex == 0 ? true : false;
            }
        }

        #region DLL definitions
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void ProgressCallback(int value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate string GetFilePathCallback(string filter);



        [DllImport("rcp_dll.dll")]
        static extern void DoWork([MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallback callbackPointer);

        [DllImport("rcp_dll.dll")]
        static extern void ProcessFile([MarshalAs(UnmanagedType.FunctionPtr)] GetFilePathCallback callbackPointer);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate int SendRCPCallback(string data, int len);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void IntReceivedCallback(int id, int cur_val_valid, int cur_val, int display_str_valid,
            string display_str_decoded, int display_str_status, int display_str_in_list);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void ListReceivedCallback(int id, int list_string_valid, string list_string, int min_val, int max_val,
            int min_val_valid, int max_val_valid, int display_str_in_list);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void StringReceivedCallback(int id, string display_str_abbr_decoded, int display_str_status);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void StopStateTimerCallback();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void SetAppOutOfDateCallback(bool outOfDate);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void StartExternalTimerCallback();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void DropConnectionCallback();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void WriteDatagramCallback(string data, int size);

        [DllImport("rcp_dll.dll")]
        static extern void CreateCameraConnection(
            [MarshalAs(UnmanagedType.FunctionPtr)] SendRCPCallback sendRCPCallback,
            [MarshalAs(UnmanagedType.FunctionPtr)] IntReceivedCallback intReceivedCallback,
            [MarshalAs(UnmanagedType.FunctionPtr)] ListReceivedCallback listReceivedCallback,
            [MarshalAs(UnmanagedType.FunctionPtr)] StringReceivedCallback stringReceivedCallback,
            [MarshalAs(UnmanagedType.FunctionPtr)] StopStateTimerCallback stopStateTimerCallback,
            [MarshalAs(UnmanagedType.FunctionPtr)] SetAppOutOfDateCallback setAppOutOfDateCallback,
            [MarshalAs(UnmanagedType.FunctionPtr)] StartExternalTimerCallback startExternalTimerCallback,
            [MarshalAs(UnmanagedType.FunctionPtr)] DropConnectionCallback dropConnectionCallback
        );

        [DllImport("rcp_dll.dll")]
        static extern void RCPDiscoveryStart([MarshalAs(UnmanagedType.FunctionPtr)] WriteDatagramCallback writeDatagramCallback);

        #endregion


        WriteDatagramCallback writeDatagramCallback;


        [DllImport("rcp_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetCharacterPositions();


        public MainWindow()
        {
            InitializeComponent();

            DllInterface.InitMutex();
            //Test();

            //int length;
            //IntPtr characterPositions = DllInterface.RCPDiscoveryGetList(out length);
            //var structSize = Marshal.SizeOf(typeof(CamInfoList));
            //for (int i = 0; i < length; ++i)
            //{
            //    var data = new IntPtr(characterPositions.ToInt64() + structSize * i);
            //    var characterInformation = (CamInfoList)Marshal.PtrToStructure(data, typeof(CamInfoList));
            //}

            ConnectionList.Items.Add("dd");
            ConnectionList.Items.Add("dd");

            m_readTimer.Tick += ReadTimerEvent;
            m_stateTimer.Interval = TimeSpan.FromMilliseconds(READ_TIMEOUT);
            m_stateTimer.Tick += SerialProtocolError;
            m_externalTimer.Interval = TimeSpan.FromMilliseconds(READ_TIMEOUT);
            m_externalTimer.Tick += ExternalControlError;
            m_broadcastTimer.Interval = TimeSpan.FromMilliseconds(300);
            m_broadcastTimer.Tick += M_broadcastTimer_Tick;

            SearchForCameras();
        }

        private void Test()
        {
            ProgressCallback callback =
                (value) =>
                {
                    Console.WriteLine("Progress = {0}", value);
                };
            DoWork(callback);

            // define a get file path callback delegate
            GetFilePathCallback getPath =
                (filter) =>
                {
                    string path = default(string);

                    OpenFileDialog ofd =
                        new OpenFileDialog()
                        {
                            Filter = filter
                        };

                    if (ofd.ShowDialog().Value == true)
                    {
                        path = ofd.FileName;
                    }

                    return path;
                };

            // call ProcessFile in C code
            ProcessFile(getPath);

        }

        void ReadTimerEvent(object sender, EventArgs e)
        {
            DropConnection();
        }

        void SerialProtocolError(object sender, EventArgs e)
        {
            DropConnection();
            m_stateTimer.Stop();

            if (ConnectionIsTCP)
            {
                MessageBox.Show("An error occurred while connecting");
                //Logger::logError(QString("An error occurred while connecting to %1").arg(m_connection->connectionID()), "MainWindow::serialProtocolError()");
                //QMessageBox::critical(this, tr("Error"), tr("An error occurred while connecting to your camera. Please ensure that the specified IP address belongs to your camera."));
            }
            else
            {
                MessageBox.Show("Serial protocol is not set to RED Control Protocol");
                //Logger::logError("Serial protocol is not set to RED Control Protocol", "MainWindow::serialProtocolError()");
                //QMessageBox::critical(this, tr("Error"), tr("An error occurred while connecting to your camera. Please ensure that the camera is connected to the specified serial port and that the Serial Protocol is set to RED Control Protocol (Menu > Settings > Setup > Communication > Serial)"));
            }
        }

        void ExternalControlError(object sender, EventArgs e)
        {
            DropConnection();
            m_externalTimer.Stop();

            if (ConnectionIsTCP)
            {
                MessageBox.Show("External control is disabled");
                //Logger::logError("External control is disabled", "MainWindow::externalControlError()");
                //QMessageBox::critical(this, tr("Error"), tr("An error occurred while connecting to your camera. Please ensure that external control is enabled (Menu > Settings > Setup > Communication > Network)"));
            }
            else
            {
                MessageBox.Show("An error occurred while connecting to %1");
                //Logger::logError(QString("An error occurred while connecting to %1").arg(m_connection->connectionID()), "MainWindow::externalControlError()");
                //QMessageBox::critical(this, tr("Error"), tr("An error occurred while connecting to your camera. Please ensure that the specified serial port belongs to your camera."));
            }

        }

        private void CreateUDPSocket()
        {
            DestroyUDPSocket();
            m_udpSocket = new UdpClient(UDP_PORT);
            //m_udpSocket.BeginReceive(DataReceived, m_udpSocket);

            udpThread = new Thread(new ParameterizedThreadStart(UDPServerProc));
            udpThread.IsBackground = true;
            udpThread.Name = "UDP server thread";
            udpThread.Start(m_udpSocket);

        }

        private void DataReceived(IAsyncResult ar)
        {
            UdpClient c = (UdpClient)ar.AsyncState;
            IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Byte[] receivedBytes = c.EndReceive(ar, ref receivedIpEndPoint);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.ff tt") + " (" + receivedBytes.Length + " bytes)");
            Console.WriteLine("UDP: " + Encoding.ASCII.GetString(receivedBytes));

            c.BeginReceive(DataReceived, ar.AsyncState);
        }
        private void TCPServerProc(object arg)
        {
            Console.WriteLine("TCP server thread started");

            try
            {
                TcpListener server = (TcpListener)arg;
                byte[] buffer = new byte[2048];
                int count;

                server.Start();

                for (; ; )
                {
                    TcpClient client = server.AcceptTcpClient();

                    using (var stream = client.GetStream())
                    {
                        while ((count = stream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            string data = Encoding.ASCII.GetString(buffer, 0, count);
                            Console.WriteLine("TCP: " + data);
                            DllInterface.RCPProcessData(data, data.Length);
                        }
                    }
                    client.Close();
                }
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode != 10004) // unexpected
                    Console.WriteLine("TCPServerProc exception: " + ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("TCPServerProc exception: " + ex);
            }

            Console.WriteLine("TCP server thread finished");
        }
        private void UDPServerProc(object arg)
        {
            Console.WriteLine("UDP server thread started");

            try
            {
                UdpClient server = (UdpClient)arg;
                IPEndPoint remoteEP;
                byte[] buffer;

                for (; ; )
                {
                    remoteEP = null;
                    buffer = server.Receive(ref remoteEP);

                    if (buffer != null && buffer.Length > 0)
                    {
                        Console.WriteLine("UDP: " + Encoding.ASCII.GetString(buffer));
                        DllInterface.RCPDiscoveryProcessData(Encoding.ASCII.GetString(buffer), buffer.Length, remoteEP.ToString());
                    }
                }
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode != 10004) // unexpected
                    Console.WriteLine("UDPServerProc exception: " + ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("UDPServerProc exception: " + ex);
            }

            Console.WriteLine("UDP server thread finished");
        }

        private void DestroyUDPSocket()
        {
            if (udpThread != null && udpThread.IsAlive)
            {
                udpThread.Abort();
                udpThread = null;
            }
            if (m_udpSocket != null)
                m_udpSocket.Close();
            //m_udpSocket = null;
        }

        public void UDP_listening()
        {
            bool done = false;
            try
            {
                while (!done)
                {
                    byte[] pdata = m_udpSocket.Receive(ref groupEP);
                    string price = Encoding.ASCII.GetString(pdata);
                    int Rawdata1 = int.Parse(price);
                    //Rawdata_str1 = Rawdata1.ToString();
                    //UpdateXY(Rawdata_str1);
                }
            }
            finally
            {
                m_udpSocket.Close();
            }
        }
        
        private void DiscoveryStepAndFinalize()
        {
            DllInterface.RCPDiscoveryStep();
            m_broadcastCount++;

            if (m_broadcastCount >= 5) // RCP_DISCOVERY_STEP_LOOP_COUNT
            {
                //bool connectionFound = false;
                m_broadcastTimer.Stop();
                m_broadcastCount = 0;
                m_searching = false;

                if (m_status != ConnectionStatus.CONNECTING && m_status != ConnectionStatus.CONNECTED)
                {
                    ConnectionList.IsEnabled = true;
                }

                btnRefreshList.Content = "Refresh List";
                DestroyUDPSocket();

                ConnectionList.Items.Clear();
                int length;
                IntPtr cameraInfoList = DllInterface.RCPDiscoveryGetList(out length);
                var structSize = Marshal.SizeOf(typeof(CamInfo));
                for (int i = 0; i < length; ++i)
                {
                    var data = new IntPtr(cameraInfoList.ToInt64() + structSize * i);
                    var camInfo = (CamInfo)Marshal.PtrToStructure(data, typeof(CamInfo));
                    ConnectionList.Items.Add(camInfo.ip_address);
                }

                DllInterface.RCPDiscoveryFreeList();
                DllInterface.RCPDiscoveryEnd();

                //if (!connectionFound)
                if (length <= 0)
                {
                    lbConnectionStatus.Content = "No cameras found";
                    //Console.WriteLine("No cameras found");
                }
                else
                {
                    lbConnectionStatus.Content = "Not Connected";
                }

                btnRefreshList.IsEnabled = true;
            }
        }

        private void UpdateConnectionStatus(ConnectionStatus status)
        {
            m_status = status;
            switch (m_status)
            {
                case ConnectionStatus.CONNECTED:
                    break;
                case ConnectionStatus.CONNECTING:
                    m_connected = false;
                    m_broadcastTimer.Stop();
                    m_broadcastCount = 0;
                    m_searching = false;
                    btnRefreshList.Content = "Refresh List";
                    DestroyUDPSocket();
                    lbConnectionStatus.Content = "Connecting...";
                    lbConnectionStatus.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0, 0xFF, 0xFF));
                    btnConnect.IsEnabled = false;
                    ConnectionType.IsEnabled = false;
                    tbxIPAddress.IsEnabled = false;
                    ConnectionList.IsEnabled = false;
                    btnRefreshList.IsEnabled = false;
                    break;
                case ConnectionStatus.DISCONNECTED:
                    break;
                case ConnectionStatus.ERROR:
                    break;
                default:
                    Console.WriteLine("INFO: Invalid connection status {0} specified", status);
                    break;
            }
        }

        private void SendInitialGets()
        {
            int i, count = 0, width;
            int maxWidth = 0;
            int[] _params = new int[5]; // m_controller->params(&count);

            string label;

            for (i = 0; i < count; i++)
            {
                label = DllInterface.RCPGetLabel(_params[i]);

                //m_controller->setParamTitle(tr("%1").arg(label), _params[i]);

                DllInterface.RCPGetList(_params[i]);
                DllInterface.RCPGet(_params[i]);
            }

            //m_controller->setParamTitleWidth(maxWidth);

            DllInterface.RCPGet(27); /// RCP_PARAM_RECORD_STATE
        }

        private int SendRCP(string data, int len)
        {
            int err = 0;

            if (ConnectionIsTCP)
            {
                Byte[] sendBytes = Encoding.ASCII.GetBytes(data);
                try
                {
                    Byte[] bytedata = System.Text.Encoding.ASCII.GetBytes(data);
                    // Get a client stream for reading and writing.
                    //  Stream stream = client.GetStream();
                    NetworkStream stream = m_tcpSocket.GetStream();
                    // Send the message to the connected TcpServer.
                    Console.WriteLine("=============== TcpSocket : write data ==============\n");
                    stream.Write(bytedata, 0, bytedata.Length);
                    //m_udpSocket.Send(sendBytes, sendBytes.Length, new IPEndPoint(IPAddress.Parse(tbxIPAddress.Text), UDP_PORT));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    err = 8;
                }
            }
            else
            {
                try
                {
                    m_serialPort.Write(data);
                }
                catch (Exception ex)
                {
                    err = 8;
                }
            }

            return err;
        }

        private void IntReceived(int id, int cur_val_valid, int cur_val, int display_str_valid,
            string display_str_decoded, int display_str_status, int display_str_in_list)
        {
            if (id == 27 && !m_connected) /// id == RCP_PARAM_RECORD_STATE
            {
                m_connected = true;
                m_externalTimer.Stop();
                //m_controller->constructUI();
                //m_controller->setEnabled(true);
                UpdateConnectionStatus(ConnectionStatus.CONNECTED);
                SendInitialGets();

                if (m_appOutOfDate)
                {
                    // Using the static QMessageBox::warning() won't work well here because it uses the QMessageBox::exec() function rather than QMessageBox::show(). Using exec() freezes up the main event loop and stops the application from reading incoming RCP messages, thus dropping the connection.
                    MessageBox.Show("This version of the %1 is not fully supported by your camera's firmware. The application will still work but there may be features on the camera that it cannot support.");
                }
            }

            if (cur_val_valid == 1)
            {
                //emit intUpdated(data->cur_val, data->id);
            }

            if (display_str_valid == 1)
            {
                //emit strUpdated(QString("%1").arg(data->display_str_decoded), data->display_str_status, data->id);
            }

            if (display_str_in_list == 1)
            {
                DllInterface.RCPGetList(id);
            }
        }

        private void ListReceived(int id, int list_string_valid, string list_string, int min_val, int max_val,
            int min_val_valid, int max_val_valid, int display_str_in_list)
        {
            if (list_string_valid == 1)
            {
                //emit listUpdated(data, rcp_get_update_list_only_on_close(m_rcpConnection, data->id));
            }
            else
            {
                Console.WriteLine("INFO: The list string is invalid: {0}", list_string, "MainWindow::listReceived(const rcp_cur_list_cb_data_t*)");
            }
        }

        private void InitializeRCPConnection()
        {
            SendRCPCallback sendRCPCallback =
                (data, len) =>
                {
                    Console.WriteLine("Data = {0}", data);
                    SendRCP(data, len);
                    return 0;
                };

            IntReceivedCallback intReceivedCallback =
                (id, cur_val_valid, cur_val, display_str_valid, display_str_decoded, display_str_status, display_str_in_list) =>
                {
                    IntReceived(id, cur_val_valid, cur_val, display_str_valid, display_str_decoded, display_str_status, display_str_in_list);
                };

            ListReceivedCallback listReceivedCallback =
                (id, list_string_valid, list_string, min_val, max_val, min_val_valid, max_val_valid, display_str_in_list) =>
                {
                    ListReceived(id, list_string_valid, list_string, min_val, max_val, min_val_valid, max_val_valid, display_str_in_list);
                };

            StringReceivedCallback stringReceivedCallback =
                (id, display_str_abbr_decoded, display_str_status) =>
                {
                    //emit strUpdated(QString("%1").arg(data->display_str_abbr_decoded), data->display_str_status, data->id);
                };

            StopStateTimerCallback stopStateTimerCallback =
                () =>
                {
                    m_stateTimer.Stop();
                };

            SetAppOutOfDateCallback setAppOutOfDateCallback =
                (outOfDate) =>
                {
                    m_appOutOfDate = outOfDate;
                };

            StartExternalTimerCallback startExternalTimerCallback =
                () =>
                {
                    m_externalTimer.Start();
                };

            DropConnectionCallback dropConnectionCallback =
                () =>
                {
                    DropConnection();
                };

            m_stateTimer.Start();

            CreateCameraConnection(sendRCPCallback, intReceivedCallback, listReceivedCallback, stringReceivedCallback,
                stopStateTimerCallback, setAppOutOfDateCallback, startExternalTimerCallback, dropConnectionCallback);
        }

        private void ConnectCamera()
        {
            Console.WriteLine("INFO: Connecting to {0}", this.tbxIPAddress.Text);
            UpdateConnectionStatus(ConnectionStatus.CONNECTING);
            DllInterface.DeleteCameraConnection();

            //m_udpSocket.Close();
            if (m_tcpSocket.Connected)
                m_tcpSocket.Close();

            if (m_serialPort.IsOpen)
                m_serialPort.Close();

            if (ConnectionIsTCP)
            {
                try
                {
                    //m_udpSocket.Connect(new IPEndPoint(IPAddress.Parse(tbxIPAddress.Text), UDP_PORT));
                    m_tcpSocket.Connect(tbxIPAddress.Text, TCP_PORT);
                    tcpServer = new TcpListener(IPAddress.Any, TCP_PORT);
                    tcpThread = new Thread(new ParameterizedThreadStart(TCPServerProc));
                    tcpThread.IsBackground = true;
                    tcpThread.Name = "TCP server thread";
                    tcpThread.Start(tcpServer);
                    InitializeRCPConnection();
                }
                catch (Exception ex)
                {
                    DropConnection();
                    UpdateConnectionStatus(ConnectionStatus.ERROR);
                    Console.WriteLine("INFO: Unable to connect to {0}", tbxIPAddress.Text);
                }
            }
            else
            {
                m_serialPort.PortName = tbxIPAddress.Text;

                try
                {
                    m_serialPort.Open();
                }
                catch (Exception ex)
                {
                    DropConnection();
                    UpdateConnectionStatus(ConnectionStatus.ERROR);
                    MessageBox.Show("Unable to open serial port %1. Error: %2");
                    //Logger::logError(QString("Unable to open serial port %1. Error: %2").arg(m_connection->connectionID()).arg(m_serialPort->errorString()), "MainWindow::connectToCamera()");
                    return;
                }

                try
                {
                    m_serialPort.BaudRate = 115200;
                }
                catch (Exception ex)
                {
                    DropConnection();
                    UpdateConnectionStatus(ConnectionStatus.ERROR);
                    MessageBox.Show("Unable to set baud rate to 115200 on serial port %1. Error: %2");
                    //Logger::logError(QString("Unable to set baud rate to 115200 on serial port %1. Error: %2").arg(m_connection->connectionID()).arg(m_serialPort->errorString()), "MainWindow::connectToCamera()");
                    return;
                }

                try
                {
                    m_serialPort.DataBits = 8;
                }
                catch (Exception ex)
                {
                    DropConnection();
                    UpdateConnectionStatus(ConnectionStatus.ERROR);
                    MessageBox.Show("Unable to set data bits to 8 on serial port %1. Error: %2");
                    //Logger::logError(QString("Unable to set data bits to 8 on serial port %1. Error: %2").arg(m_connection->connectionID()).arg(m_serialPort->errorString()), "MainWindow::connectToCamera()");
                    return;
                }

                try
                {
                    m_serialPort.Parity = Parity.None;
                }
                catch (Exception ex)
                {
                    DropConnection();
                    UpdateConnectionStatus(ConnectionStatus.ERROR);
                    MessageBox.Show("Unable to set no parity to serial port %1. Error: %2");
                    //Logger::logError(QString("Unable to set no parity to serial port %1. Error: %2").arg(m_connection->connectionID()).arg(m_serialPort->errorString()), "MainWindow::connectToCamera()");
                    return;
                }

                try
                {
                    m_serialPort.StopBits = StopBits.One;
                }
                catch (Exception ex)
                {
                    DropConnection();
                    UpdateConnectionStatus(ConnectionStatus.ERROR);
                    MessageBox.Show("Unable to set one stop bit on serial port %1. Error: %2");
                    //Logger::logError(QString("Unable to set one stop bit on serial port %1. Error: %2").arg(m_connection->connectionID()).arg(m_serialPort->errorString()), "MainWindow::connectToCamera()");
                    return;
                }

                Console.WriteLine("Serial connection established");
                //Logger::logInfo("Serial connection established", "MainWindow::connectToCamera()");
                InitializeRCPConnection();
            }
        }

        private void DropConnection()
        {
            DllInterface.DeleteCameraConnection();

            if (m_tcpSocket.Connected)
            {
                Console.WriteLine("INFO: TCP connection dropped", "MainWindow::dropConnection()");
                m_tcpSocket.Close();
            }

            if (m_serialPort.IsOpen)
            {
                Console.WriteLine("INFO: Serial connection dropped", "MainWindow::dropConnection()");
                m_serialPort.Close();
            }

            m_connected = false;
            m_appOutOfDate = false;
            m_readTimer.Stop();
            //m_controller->initialize();
            UpdateConnectionStatus(ConnectionStatus.DISCONNECTED);
        }

        private void SearchForCameras()
        {
            if (ConnectionList == null) return;

            ConnectionList.Items.Clear();

            if (ConnectionIsTCP)
            {
                if (!m_searching)
                {
                    m_searching = true;
                    btnRefreshList.IsEnabled = false;
                    btnRefreshList.Content = "Searching...";
                    ConnectionList.Focus();
                    m_broadcastTimer.Stop();
                    m_broadcastCount = 0;

                    CreateUDPSocket();

                    writeDatagramCallback =
                         (data, size) =>
                         {
                             Byte[] sendBytes = Encoding.ASCII.GetBytes(data);
                             try
                             {
                                 m_udpSocket.Send(sendBytes, sendBytes.Length, new IPEndPoint(IPAddress.Broadcast, UDP_PORT));
                             }
                             catch (Exception e)
                             {
                                 Console.WriteLine(e.ToString());
                             }
                         };
                    RCPDiscoveryStart(writeDatagramCallback);
                    m_broadcastTimer.Start();
                }
            }
            else
            {
                m_searching = false;
                btnRefreshList.IsEnabled = true;
                m_broadcastTimer.Stop();
                m_broadcastCount = 0;

                string[] ports = SerialPort.GetPortNames();

                foreach (string port in ports)
                {
                    //addConnectionToList(i->portName(), i->portName(), i->manufacturer(), i->description());
                    ConnectionList.Items.Add(port);
                    m_serialConnectionFound = true;
                }

                if (!m_serialConnectionFound)
                {
                    MessageBox.Show("No available serial ports");
                    //m_connectionListStatus->setText(tr("No available serial ports"));
                }
            }

        }

        private void M_broadcastTimer_Tick(object sender, EventArgs e)
        {
            DiscoveryStepAndFinalize();
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton btn = sender as ToggleButton;
            bool isChecked = btn.IsChecked.HasValue && btn.IsChecked.Value;

            if (isChecked)
            {
                btn.Content = "Disconnect";
                ConnectCamera();
            }
            else
            {
                btn.Content = "Connect";
                DropConnection();
            }
        }

        private void ConnectionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //tbxIPAddress.Text = "";

            if (ConnectionIsTCP)
            {
                if (ConnectionListTitle != null)
                    ConnectionListTitle.Content = "Cameras";
                //m_connectionId->setPlaceholderText(tr("IP Address"));
            }
            else
            {
                if (ConnectionListTitle != null)
                    ConnectionListTitle.Content = "Serial Ports";
                //m_connectionId->setPlaceholderText(tr("Port Name"));
            }

            SearchForCameras();
        }

        private void ConnectionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem lbi = ((sender as ListBox).SelectedItem as ListBoxItem);
            //tb.Text = "   You selected " + lbi.Content.ToString() + ".";
        }

        private void btnRefreshList_Click(object sender, RoutedEventArgs e)
        {
            SearchForCameras();
        }
    }
}
