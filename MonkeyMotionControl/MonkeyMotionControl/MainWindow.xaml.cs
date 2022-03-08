using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MonkeyMotionControl.Properties;
//using MonkeyMotionControl.RobotArm;
using MonkeyMotionControl.UI;
using System.Windows.Data;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MonkeyMotionControl
{
    public enum RobotPlanningMode { SIMULATOR, COMMANDS, EVENT };

    public enum RobotLiveMoveMode { CARTESIAN, JOINTS };

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region VARIABLES

        private const string LOG_FILE_NAME = "log.txt";
        private string LogPath
        {
            get
            {
                return System.IO.Path.Combine(App.LOGS_PATH, LOG_FILE_NAME);
            }
        }

        // MAIN TIMER
        private DispatcherTimer mainTimer;
        public Stopwatch MainStopwatch = new Stopwatch();
        public bool IsVideoCaptureStarted = false;

        #endregion

        #region CONSTRUCTOR

        public MainWindow()
        {
            App.Win = this;
            InitializeComponent();
            DataContext = this;
            Title = "Monkey Motion Control v0.981";
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            ContentRendered += MainWindow_ContentRendered;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UI_HideAllContent();
            UI_DisableAppFunctions();
            AddLog("Loaded");
            //Timeline_Initialize();
            Simulator_Initialize();
            //Initialize_Player();
            //Robot_Initialize();
            //Camera_Initialize();
            //ToolController_Initialize();
            //MotionDevices_Initialize(); //KSMotion EtherCAT
            //SpaceNavigator_Initialize();
            //UserControls_Initialize();
            //GamePad_Initialize();
            //VideoCapture_Initialize();
            //CountdownTimer_Initialize();
            //StreamTable_Initialize();
            //CommandsTable_Initialize();
            //RobotPresetsTable_Initialize();
            //RobotMovesTable_Initialize();
            MainTimer_Initialize();
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            text_LogPath.Text = LogPath;
            UI_ShowAllContent();
            App.SplashScreen.Close(TimeSpan.FromSeconds(4));
            //((MainVM)DataContext).DetectIfSubsystemAlreadyStarted();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e) // TODO: Check if data is saved
        {
            if (true) //this.isDataSaved
            {
                if (ShowDialogMessage("CLOSE", "Are you sure you want to Close?"))
                {
                    MainStopwatch.Stop();
                    //Robot_Close();
                    //ToolController_Close();
                    //VideoCapture_Close();
                    //Camera_Close();
                    //GamePad_Close();
                    //SpaceNavigator_Close();
                    //MotionDevices_Close();
                    //RobotPresetsTable_Close();
                    //RobotMovesTable_Close();
                    Settings.Default.Save();
                }
                else
                {
                    e.Cancel = true; // If user doesn't want to close, cancel closure
                }
            }
        }

        private void CloseApp()
        {
            this.Close();
        }

        private void UI_ShowAllContent()
        {
            //grid_Timeline.Visibility = Visibility.Visible;
            grid_Controls.Visibility = Visibility.Visible;
            //grid_Videofeed.Visibility = Visibility.Visible;
            grid_Simulator.Visibility = Visibility.Visible;
        }

        private void UI_HideAllContent()
        {
            //grid_Timeline.Visibility = Visibility.Collapsed;
            grid_Controls.Visibility = Visibility.Collapsed;
            //grid_Videofeed.Visibility = Visibility.Collapsed;
            grid_Simulator.Visibility = Visibility.Collapsed;
        }

        private void Btn_CloseApp_Click(object sender, RoutedEventArgs e)
        {
            CloseApp();
        }

        #endregion

        #region SESSION

        //private async void Btn_RobotConnectionToggle_Click(object sender, EventArgs e)
        //{
        //    if (RobotConnectionStatus())
        //    {
        //        // === DISCONNECT
        //        Robot_Disconnect(); //TODO: await Task.Run(Robot_Disconnect); //TODO: Fix to be ThreadSafe -> AddLog
        //        Session_DisconnectedState();
        //    }
        //    else
        //    {
        //        // === CONNECT
        //        if (await Robot_Connect(cb_IPAddress.Text))
        //        {
        //            Session_ConnectedState();
        //            Settings.Default.ROBOT_TCP_IPADDRESS = cb_IPAddress.Text;
        //        }
        //        else
        //        {
        //            Btn_RobotConnectionToggle.IsChecked = false;
        //            ShowErrorMessageAndLog("Robot Error", $"Connect to {cb_IPAddress.Text} error.");
        //        }
        //    }
        //}

        private void UI_EnableAppFunctions()
        {
            //Robot_UI_EnableFunctions();
            //UserControls_UIEnabled();
        }

        private void UI_DisableAppFunctions()
        {
            //Robot_UI_DisableFunctions();
            //UserControls_UIDisabled();
            Simulator_UIClear();
            //Robot_UI_Clear();
        }

        private void Session_ConnectedState()
        {
            //Robot_UI_Connected();
            Simulator_SyncEnable();
            UI_EnableAppFunctions();
        }

        private void Session_DisconnectedState()
        {
            //Robot_UI_Disconected();
            Simulator_SyncDisable();
            UI_DisableAppFunctions();

            //if (GamePad.IsGamePadEnabled) GamePad.DisableGamepad();
            //if (IsLiveControlEnabled) IsLiveControlEnabled = false;
        }

        #endregion

        #region MAIN TIMER

        private void MainTimer_Initialize()
        {
            mainTimer = new DispatcherTimer(DispatcherPriority.Normal);
            mainTimer.Tick += new EventHandler(MainTimer_Tick);
            mainTimer.Interval = new TimeSpan(0, 0, 0, 0, 100); // 100ms
            mainTimer.IsEnabled = true;
            mainTimer.Start();
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                //if (Camera.PhantomCamera_IsOnline() && !Camera.PhantomCamera_IsSimulated() && !VideoCapture.IsStarted && Settings.Default.VIDEOCAPTURE_AUTOSTART)
                //{
                //    if (VideoCapture_GetSelectedDeviceName().Contains(Settings.Default.VIDEOCAPTURE_SELECTED_NAME))
                //    {
                //        VideoCapture_Start();
                //    }
                //}

                //Robot_TimerUpdate();

                ToggleIndicatorMainTimer();
            }
            catch (Exception l_e)
            {
                ShowErrorMessageAndLog("Main Timer Tick Error", l_e.Message);
            }
        }

        private void ToggleIndicatorMainTimer()
        {
            text_indicator_maintimer.Background = text_indicator_maintimer.Background == Brushes.DarkRed ? Brushes.Tomato : Brushes.DarkRed;
        }

        #endregion

        #region LOG, UI & MESSAGE ALERTS

        private bool LogFirstEntryFlag = true;

        public void AddLog(string logText)
        {
            Dispatcher.Invoke(() =>
            {
                if (LogFirstEntryFlag) // Log current date and time entry each time start of application
                {
                    tb_MainLog.AppendText($"{DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")} > App {logText}{Environment.NewLine}");
                    string str1 = Stopwatch.IsHighResolution ? "Log Timer using High-Resolution counter." : "Log Timer using Low-Resolution DateTime class.";
                    long frequency = Stopwatch.Frequency;
                    long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;
                    string str2 = $"Log Timer frequency in ticks per second = {frequency}, accurate within {nanosecPerTick} nanoseconds.";
                    tb_MainLog.AppendText($"> {str1}{Environment.NewLine}");
                    tb_MainLog.AppendText($"> {str2}{Environment.NewLine}");
                    LogFirstEntryFlag = false;
                    MainStopwatch.Start();
                }
                else
                {
                    TimeSpan ts = MainStopwatch.Elapsed;
                    string timestamp = string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D2}:{4:D3}", ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
                    tb_MainLog.AppendText($"{timestamp} > {logText}{Environment.NewLine}");
                }
                tb_MainLog.ScrollToEnd();
                tb_MainLog.ScrollToEnd();
                LogFile(logText);
            });
        }

        public static void LogFile(string message)
        {
            if (!Directory.Exists(App.LOGS_PATH))
                Directory.CreateDirectory(App.LOGS_PATH);
            File.AppendAllText(System.IO.Path.Combine(App.LOGS_PATH, LOG_FILE_NAME), Environment.NewLine + DateTime.Now.ToString() + " - ");
            File.AppendAllText(System.IO.Path.Combine(App.LOGS_PATH, LOG_FILE_NAME), message);
        }

        public void Debug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        private void Btn_ClearMotionLog_Click(object sender, EventArgs e)
        {
            tb_MainLog.Text = string.Empty;
        }

        //CustomProgressWindow progressWindow;

        //public void CustomProgressWindow_Show(double max, string text)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        progressWindow = new CustomProgressWindow(max, text);
        //        progressWindow.Show();
        //    });
        //}

        //public void CustomProgressWindow_UpdateProgress(double progress)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        progressWindow.UpdateProgress(progress);
        //    });
        //}

        //public void CustomProgressWindow_Close()
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        progressWindow.FastClose();
        //    });
        //}

        public void ShowShortNotificationMessage(string message)
        {
            CustomMessageBox.ShowShortNotification(message);
        }

        private void ShowShortNotificationDialogMessage(string message)
        {
            CustomMessageBox.ShowShortNotificationDialog(message);
        }

        public bool ShowDialogMessage(string title, string message)
        {
            return CustomMessageBox.ShowDialog(message, title);
        }

        private void ShowInfoMessage(string title, string message)
        {
            CustomMessageBox.ShowInfo(message, title);
        }

        private void ShowInfoMessageAndLog(string title, string message)
        {
            AddLog(message);
            CustomMessageBox.ShowInfo(message, title);
        }

        private bool ShowAlertMessage(string title, string message)
        {
            return CustomMessageBox.ShowAlert(message, title);
        }

        private bool ShowWarningMessage(string title, string message)
        {
            return CustomMessageBox.ShowWarning(message, title);
        }

        private void ShowErrorMessage(string title, string message)
        {
            CustomMessageBox.ShowError(message, title);
        }

        public void ShowErrorMessageAndLog(string title, string message)
        {
            AddLog(message);
            Dispatcher.Invoke(() =>
            {
                ShowErrorMessage(title, message);
            });
        }

        private bool ValidateField(System.Windows.Controls.TextBox tb)
        {
            bool result = true;
            if (string.IsNullOrEmpty(tb.Text)) result = false;
            return result;
        }

        private bool ValidateField_DecimalString(System.Windows.Controls.TextBox tb)
        {
            double double_num;
            return Double.TryParse(tb.Text, out double_num);
        }

        #endregion

        #region ROBOT CONTROLLER

        #region RobotPlanMode Dependency Property

        public static readonly RoutedEvent RobotPlanModeChangedRoutedEvent = EventManager.RegisterRoutedEvent(
            nameof(RobotPlanModeChangedEvent), RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<RobotPlanningMode>), typeof(MainWindow));

        public event RoutedPropertyChangedEventHandler<RobotPlanningMode> RobotPlanModeChangedEvent
        {
            add { AddHandler(RobotPlanModeChangedRoutedEvent, value); }
            remove { RemoveHandler(RobotPlanModeChangedRoutedEvent, value); }
        }

        public static readonly DependencyProperty RobotPlanModeProperty = DependencyProperty.Register(
            nameof(RobotPlanMode), typeof(RobotPlanningMode), typeof(MainWindow),
            new FrameworkPropertyMetadata(RobotPlanningMode.COMMANDS, new PropertyChangedCallback(RobotPlanModeChanged)));

        /// <summary>
        /// RobotPlanMode Dependency Property.
        /// </summary>
        public RobotPlanningMode RobotPlanMode
        {
            get { return (RobotPlanningMode)GetValue(RobotPlanModeProperty); }
            set
            {
                SetValue(RobotPlanModeProperty, value);
                OnPropertyChanged(nameof(RobotPlanMode));
            }
        }

        private static void RobotPlanModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as MainWindow).OnRobotPlanModeChanged(
                new RoutedPropertyChangedEventArgs<RobotPlanningMode>((RobotPlanningMode)args.OldValue, (RobotPlanningMode)args.NewValue, RobotPlanModeChangedRoutedEvent));
        }

        private void OnRobotPlanModeChanged(RoutedPropertyChangedEventArgs<RobotPlanningMode> e)
        {
            //Debug("OnRobotPlanModeChanged()");
            if (robotPlanMode_Current != e.NewValue)
            {
                if (ShowWarningMessage("Switch Mode", $"Are you sure you want to switch to {RobotPlanMode} Mode?\nAll current data will be erased."))
                {
                    // TODO: Reset Simulator
                    if (e.NewValue == RobotPlanningMode.COMMANDS)
                    {
                        //RobotSetMotionModeCommands(); // Set Robot Controller to Commands Mode
                        //Timeline.RobotPlanMode = e.NewValue; // Change Timeline to standard config
                    }
                    else if (e.NewValue == RobotPlanningMode.EVENT)
                    {
                        //RobotSetMotionModeCommands(); // Set Robot Controller to Commands Mode
                        //Timeline.RobotPlanMode = e.NewValue; // Change Timeline to events config
                        //                                     //TODO: Set Rehearsed Focus 1,2,3,4,5 = false
                        //                                     //Autofocus performance?
                        //RobotEventMode_Initialize();
                    }
                    else if (e.NewValue == RobotPlanningMode.SIMULATOR)
                    {
                        //RobotSetMotionModeStream(); // Set Robot Controller to Stream Mode
                        //Timeline.RobotPlanMode = e.NewValue; // Change Timeline to Stream Mode
                    }
                    robotPlanMode_Current = e.NewValue;
                    //Robot_UI_UpdatePlanMode(); // Update UI 
                }
                else
                {
                    RobotPlanMode = e.OldValue;
                }
            }
            RaiseEvent(e);
        }

        private RobotPlanningMode robotPlanMode_Current = RobotPlanningMode.COMMANDS; // Match default to RobotPlanMode DP

        #endregion

        #region RobotLiveMoveMode Dependency Property

        public static readonly RoutedEvent RobotLiveMoveModeChangedRoutedEvent = EventManager.RegisterRoutedEvent(
            nameof(RobotLiveMoveModeChangedEvent), RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<RobotLiveMoveMode>), typeof(MainWindow));

        public event RoutedPropertyChangedEventHandler<RobotLiveMoveMode> RobotLiveMoveModeChangedEvent
        {
            add { AddHandler(RobotLiveMoveModeChangedRoutedEvent, value); }
            remove { RemoveHandler(RobotLiveMoveModeChangedRoutedEvent, value); }
        }

        public static readonly DependencyProperty RobotLiveMoveModeProperty = DependencyProperty.Register(
            nameof(RobotLiveMoveMode), typeof(RobotLiveMoveMode), typeof(MainWindow),
            new FrameworkPropertyMetadata(RobotLiveMoveMode.CARTESIAN, new PropertyChangedCallback(RobotLiveMoveModeChanged)));

        /// <summary>
        /// RobotLiveMoveMode Dependency Property.
        /// </summary>
        public RobotLiveMoveMode RobotLiveMoveMode
        {
            get { return (RobotLiveMoveMode)GetValue(RobotLiveMoveModeProperty); }
            set
            {
                SetValue(RobotLiveMoveModeProperty, value);
                OnPropertyChanged(nameof(RobotLiveMoveMode));
            }
        }

        private static void RobotLiveMoveModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as MainWindow).OnRobotLiveMoveModeChanged(
                new RoutedPropertyChangedEventArgs<RobotLiveMoveMode>((RobotLiveMoveMode)args.OldValue, (RobotLiveMoveMode)args.NewValue, RobotLiveMoveModeChangedRoutedEvent));
        }

        private void OnRobotLiveMoveModeChanged(RoutedPropertyChangedEventArgs<RobotLiveMoveMode> e)
        {
            //System.Diagnostics.Debug.WriteLine("OnRobotLiveMoveModeChanged()");
            switch (e.NewValue)
            {
                case RobotLiveMoveMode.CARTESIAN:
                    //robotLiveMoveCartesian = true;
                    break;
                case RobotLiveMoveMode.JOINTS:
                    //robotLiveMoveCartesian = false;
                    break;
            }
            RaiseEvent(e);
        }

        #endregion

        #endregion

        #region SIMULATOR

        private double Simulator_UIWidth;
        private double Simulator_UIHeight;
        private bool Simulator_FocusUpdatingLock = false;

        private void Simulator_Initialize()
        {
            Simulator.LogEvent += Simulator_LogHandler;
            Simulator.ErrorLogEvent += Simulator_ErrorLogHandler;
            Simulator.StreamTable_AddRowEvent += Simulator_StreamTableAddRowHandler;
            Simulator.StreamTable_UpdateRowEvent += Simulator_StreamTableUpdateRowHandler;
            Simulator.StreamTable_ClearEvent += Simulator_StreamTableClearHandler;
            Simulator.CommandsTable_AddRowEvent += Simulator_CommandsTableAddRowHandler;
            Simulator.CommandsTable_ClearEvent += Simulator_CommandsTableClearHandler;
            Simulator.RobotToolOffsetUpdatedEvent += Simulator_RobotToolOffsetUpdatedHandler;
            Simulator.TrackingEnabledChangedEvent += Simulator_TrackingEnabledChangedHandler;
            Simulator.FocusDistanceChangedEvent += Simulator_FocusDistanceChangedHandler;
            Simulator.TargetDistanceChangedEvent += Simulator_TargetDistanceChangedHandler;
            Simulator.SyncEnabledChangedEvent += Simulator_SyncEnabledChangedHandler;
            Simulator.CameraViewModeChangedEvent += Simulator_CameraViewModeChanged;
            Simulator.FullscreenModeChangedEvent += Simulator_FullscreenModeChanged;
            Simulator.UIGoToStreamTableEvent += Simulator_UIGoToStreamTable;
            Simulator.UIGoToCommandsTableEvent += Simulator_UIGoToCommandsTable;
            Simulator.UIGoToLiveControlsEvent += Simulator_UIGoToLiveControls;
        }

        public void Simulator_LogHandler(object o, LogEventArgs a)
        {
            AddLog($"SIMULATOR: {a.LogMsg}");
        }

        public void Simulator_ErrorLogHandler(object o, LogEventArgs a)
        {
            ShowErrorMessageAndLog("Simulator Error", $"SIMULATOR: {a.LogMsg}");
        }

        private void Simulator_CameraViewModeChanged(object o, RoutedPropertyChangedEventArgs<bool> cameraview)
        {
            if (!Simulator.FullscreenMode)
            {
                if (cameraview.NewValue)
                {
                    grid_Simulator.Width = 1920;
                    //grid_Videofeed.Visibility = Visibility.Collapsed;
                    //if (VideoCapture.IsStarted)
                    //{
                    //    VideoCapture_Stop();
                    //}
                }
                else
                {
                    grid_Simulator.Width = 1026;
                    //grid_Videofeed.Visibility = Visibility.Visible;
                    //if (!VideoCapture.IsStarted)
                    //{
                    //    VideoCapture_Start();
                    //}
                }
            }
        }

        public void Simulator_FullscreenModeChanged(object o, RoutedPropertyChangedEventArgs<bool> fullscreen)
        {
            if (fullscreen.NewValue)
            {
                Simulator_UIWidth = grid_Simulator.ActualWidth; // Save Current Width
                Simulator_UIHeight = grid_Simulator.ActualHeight; // Save Current Height
                grid_Simulator.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
                grid_Simulator.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
                //grid_Timeline.Visibility = Visibility.Collapsed;
                grid_Controls.Visibility = Visibility.Collapsed;
                //grid_Videofeed.Visibility = Visibility.Collapsed;
                //grid_Player.Margin = Player_UIControls_MarginFullScreenSimulator;
                //grid_Player.HorizontalAlignment = HorizontalAlignment.Center;
                //grid_Player.VerticalAlignment = VerticalAlignment.Bottom;
            }
            else
            {
                grid_Simulator.Width = Simulator_UIWidth;
                grid_Simulator.Height = Simulator_UIHeight;
                //grid_Timeline.Visibility = Visibility.Visible;
                grid_Controls.Visibility = Visibility.Visible;
                //grid_Player.Margin = Player_UIControls_MarginOriginal;
                //grid_Player.HorizontalAlignment = HorizontalAlignment.Left;
                //grid_Player.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        public void Simulator_UIGoToStreamTable(object o, EventArgs a)
        {
            tabControl_Main.SelectedIndex = 0;
            //tabControl_Robot.SelectedIndex = 1;
        }

        public void Simulator_UIGoToCommandsTable(object o, EventArgs a)
        {
            tabControl_Main.SelectedIndex = 0;
            //tabControl_Robot.SelectedIndex = 0;
        }

        public void Simulator_UIGoToLiveControls(object o, EventArgs a)
        {
            //UserControls_UIGoToLiveControls();
        }

        public void Simulator_UIClear()
        {
            Simulator.tb_robotPos_J1.Text = "0.00";
            Simulator.tb_robotPos_J2.Text = "0.00";
            Simulator.tb_robotPos_J3.Text = "0.00";
            Simulator.tb_robotPos_J4.Text = "0.00";
            Simulator.tb_robotPos_J5.Text = "0.00";
            Simulator.tb_robotPos_J6.Text = "0.00";
            Simulator.tb_robotPos_X.Text = "0.00";
            Simulator.tb_robotPos_Y.Text = "0.00";
            Simulator.tb_robotPos_Z.Text = "0.00";
            Simulator.tb_robotPos_RX.Text = "0.00";
            Simulator.tb_robotPos_RY.Text = "0.00";
            Simulator.tb_robotPos_RZ.Text = "0.00";
        }

        private void Simulator_Reset()
        {
            Simulator.ResetAll();
        }

        private void Simulator_AddBezierPoint()
        {
        }

        private void Simulator_AddBezierPoint(string label, double x, double y, double z, double rx, double ry, double rz)
        {
        }

        private void Simulator_ChangeDuration(double duration)
        {
            Simulator.Duration = duration;
        }

        private void Simulator_SyncEnable()
        {
            Simulator.btn_Sync.IsEnabled = true;
            if (!Simulator.IsSyncEnabled && !Simulator.IsCalculating && !Simulator.IsPlaying)
            {
                Simulator.IsSyncEnabled = true;
            }
        }

        private void Simulator_SyncDisable()
        {
            Simulator.IsSyncEnabled = false;
            Simulator.btn_Sync.IsEnabled = false;
        }

        private void Simulator_UpdateRobotJoints(double j1, double j2, double j3, double j4, double j5, double j6)
        {
            Simulator.UpdateRobotJoints(j1, j2, j3, j4, j5, j6);
        }

        /// <summary>
        /// Update Simulator Focus Distance
        /// </summary>
        /// <param name="dist">Focus Distance</param>
        //public void Simulator_UpdateSimFocusDistance(float dist)
        //{
        //    //ToolController.FocusLink == ToolControllerLinkType.SIM 
        //    //&& Simulator.IsSyncEnabled && !robotStreamMode_RunThreadRunning 
        //    if (!Simulator_FocusUpdatingLock && !IsLiveFocusTargetEnabled)
        //    {
        //        Simulator_FocusUpdatingLock = true;
        //        Simulator.UpdateFocusDistance(dist);
        //        Simulator_FocusUpdatingLock = false;
        //    }
        //}

        public void Simulator_TrackingEnabledChangedHandler(object o, EventArgs a)
        {

        }

        /// <summary>
        /// Simulator Focus Distance Changed Handler updates ToolController Focus Distance.
        /// </summary>
        public void Simulator_FocusDistanceChangedHandler(object o, EventArgs a)
        {
            ////&& Simulator.IsSyncEnabled && !robotStreamMode_RunThreadRunning
            //if (ToolController.FocusLink == ToolControllerLinkType.SIMULATOR
            //     && !Simulator_FocusUpdatingLock && !Simulator.IsCalculating)
            //{
            //    Simulator_FocusUpdatingLock = true;
            //    ToolController.FocusPosition = ToolController.Focus_GetPosition(Simulator.FocusDistance);
            //    Simulator_FocusUpdatingLock = false;
            //}
        }

        public void Simulator_TargetDistanceChangedHandler(object o, EventArgs a)
        {
            ////TODO: Move into Simulator -> IsFocusTargetEnabled ?
            //if (Simulator.IsSyncEnabled && IsLiveFocusTargetEnabled)
            //{
            //    Simulator.FocusDistance = Simulator.TargetDistance;
            //}
        }

        public void Simulator_SyncEnabledChangedHandler(object o, EventArgs a)
        {
            //if (Simulator.IsSyncEnabled)
            //{
            //    UserControls_UITargetControlsEnabled();
            //    ToolController.FocusPosition = ToolController.Focus_GetPosition(Simulator.FocusDistance);
            //}
            //else
            //{
            //    UserControls_UITargetControlsDisabled();
            //}
        }

        public void Simulator_RobotToolOffsetUpdatedHandler(object o, EventArgs a)
        {
            //RobotSetToolOffset(Convert.ToInt16(Simulator.CameraOffset.X), Convert.ToInt16(Simulator.CameraOffset.Y), Convert.ToInt16(Simulator.CameraOffset.Z));
        }

        public void Simulator_StreamTableClearHandler(object o, EventArgs a)
        {
            //Simulator.IsStreamTableClear = StreamTable_Clear();
        }

        public void Simulator_StreamTableUpdateRowHandler(object o, StreamTableRowEventArgs a)
        {
            //StreamTable_UpdateRow(a.RowID);
        }

        /// <summary>
        /// Handle Export Mode 2 (Joints Sequence) Insert Commands to Stream Sequence Table
        /// </summary>
        /// <param name="o"></param>
        /// <param name="a"></param>
        public void Simulator_StreamTableAddRowHandler(object o, StreamTableRowEventArgs a)
        {
            StreamTable_AddRow(a.JointValues, a.Vel, a.Acc, a.Dec, a.Leave, a.Reach, a.DistToTarget);
        }

        public void Simulator_CommandsTableClearHandler(object o, EventArgs a)
        {
            //Simulator.IsCommandsTableClear = CommandsTable.ClearTable();
        }

        /// <summary>
        /// Handle Export Mode 1 (Command Sequence) Insert Commands to Commands Sequence Table
        /// </summary>
        /// <param name="o"></param>
        /// <param name="rowData"></param>
        public void Simulator_CommandsTableAddRowHandler(object o, CommandsTableRowEventArgs rowData)
        {
            // Check and set first point 
            if (rowData.MoveType == CommandsMoveType.START && !CommandsTable.IsStartPointSet)
            {
                CommandsTable.SetStartPoint(
                    new CommandsTablePoint(
                        rowData.FirstPoint.X,
                        rowData.FirstPoint.Y,
                        rowData.FirstPoint.Z,
                        rowData.FirstPoint.RX,
                        rowData.FirstPoint.RY,
                        rowData.FirstPoint.RZ,
                        rowData.Focus,
                        rowData.Iris,
                        rowData.Zoom,
                        rowData.AuxMotor
                    )
                );
            }
            else if (rowData.MoveType == CommandsMoveType.LINEAR)
            {
                CommandsTable.AddLinearMove(
                    new CommandsTablePoint(
                        rowData.LastPoint.X,
                        rowData.LastPoint.Y,
                        rowData.LastPoint.Z,
                        rowData.LastPoint.RX,
                        rowData.LastPoint.RY,
                        rowData.LastPoint.RZ,
                        rowData.Focus,
                        rowData.Iris,
                        rowData.Zoom,
                        rowData.AuxMotor
                    ),
                    rowData.Velocity,
                    rowData.Acceleration,
                    rowData.Deceleration,
                    Settings.Default.SIM_EXP1_LEAVE,
                    Settings.Default.SIM_EXP1_REACH
                );
            }
            else if (rowData.MoveType == CommandsMoveType.CIRCULAR)
            {
                CommandsTable.AddCircularMove(
                    new CommandsTablePoint(
                        rowData.MidPoint.X,
                        rowData.MidPoint.Y,
                        rowData.MidPoint.Z,
                        rowData.MidPoint.RX,
                        rowData.MidPoint.RY,
                        rowData.MidPoint.RZ,
                        rowData.MidFocus,
                        rowData.MidIris,
                        rowData.MidZoom,
                        rowData.MidAuxMotor
                    ),
                    new CommandsTablePoint(
                        rowData.LastPoint.X,
                        rowData.LastPoint.Y,
                        rowData.LastPoint.Z,
                        rowData.LastPoint.RX,
                        rowData.LastPoint.RY,
                        rowData.LastPoint.RZ,
                        rowData.Focus,
                        rowData.Iris,
                        rowData.Zoom,
                        rowData.AuxMotor
                    ),
                    rowData.Velocity,
                    rowData.Acceleration,
                    rowData.Deceleration,
                    Settings.Default.SIM_EXP1_LEAVE,
                    Settings.Default.SIM_EXP1_REACH
                );
            }
            else
            {
                Simulator.Log($"Export 1 Add Row Error invalid move type.");
            }
        }


        #endregion

        #region TABLES

        #region STREAM TABLE

        private void StreamTable_Initialize()
        {
            StreamTable.LogEvent += StreamTable_LogHandler;
            StreamTable.AddPointEvent += StreamTable_AddPointHandler;
            StreamTable.GotoSelPointEvent += StreamTable_GotoSelPointHandler;
            StreamTable.ClearStackEvent += StreamTable_ClearStackHandler;
            StreamTable.RunEvent += StreamTable_RunHandler;
        }

        private void StreamTable_LogHandler(object sender, LogEventArgs e)
        {
            AddLog($"STREAM TABLE: {e.LogMsg}");
        }

        private void StreamTable_AddPointHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                //Simulator.UpdateTargetDistance();
                StreamTable_AddRow(
                    new double[] {
                        Simulator.slider_J1.Value,
                        Simulator.slider_J2.Value,
                        Simulator.slider_J3.Value,
                        Simulator.slider_J4.Value,
                        Simulator.slider_J5.Value,
                        Simulator.slider_J6.Value
                    },
                    Settings.Default.ROBOT_DEFAULT_VEL,
                    Settings.Default.ROBOT_DEFAULT_ACC,
                    Settings.Default.ROBOT_DEFAULT_DEC,
                    Settings.Default.ROBOT_DEFAULT_LEAVE,
                    Settings.Default.ROBOT_DEFAULT_REACH,
                    Simulator.TargetDistance
                );
            }
            catch (Exception ex)
            {
                ShowErrorMessageAndLog("Stream Table Error", ex.Message);
            }
        }

        public bool StreamTable_Clear()
        {
            return StreamTable.ClearTable();
        }

        public bool StreamTable_AddRow(double[] joints, double vel, double acc, double dec, double leave, double reach, double distToTarget)
        {
            return StreamTable.AddRow(
                joints,
                distToTarget,
                9999, //ToolController.Focus_GetPosition(distToTarget),
                0,
                0,
                0,
                vel,
                acc,
                dec,
                leave,
                reach
            );
        }

        public bool StreamTable_UpdateRow(int nRow)
        {
            return StreamTable.UpdateRow(
                nRow,
                Convert.ToDouble(Simulator.TargetDistance),                 /// Distance
                9999, //ToolController.Focus_GetPosition(Simulator.TargetDistance), /// Focus
                0,      /// Iris
                0,      /// Zoom
                0,      /// Aux
                new double[]
                {
                    Convert.ToDouble(Simulator.tb_PointData_J1.Text),   /// J1
                    Convert.ToDouble(Simulator.tb_PointData_J2.Text),   /// J2
                    Convert.ToDouble(Simulator.tb_PointData_J3.Text),   /// J3
                    Convert.ToDouble(Simulator.tb_PointData_J4.Text),   /// J4
                    Convert.ToDouble(Simulator.tb_PointData_J5.Text),   /// J5
                    Convert.ToDouble(Simulator.tb_PointData_J6.Text)    /// J6
                },
                Convert.ToDouble(Simulator.tb_pointdata_vel.Text),      /// Velocity
                Convert.ToDouble(Simulator.tb_pointdata_acc.Text),      /// Acceleration
                Convert.ToDouble(Simulator.tb_pointdata_dec.Text),      /// Deceleration
                Convert.ToDouble(Simulator.tb_pointdata_leave.Text),    /// Leave
                Convert.ToDouble(Simulator.tb_pointdata_reach.Text)     /// Reach
            );
        }

        private void StreamTable_GotoSelPointHandler(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    if (Robot_UI_PopUp_MotionAlert())
            //    {
            //        RobotStreamMode_GoToPoint();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ShowErrorMessageAndLog("Stream Table Error", ex.Message);
            //}
        }

        private async void StreamTable_ClearStackHandler(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    //robotStreamMode_RunThreadRunning = false; // Terminate Run Stream Thread 
            //    //robotIsSequenceSyncMotion = false; // Terminate Run Stream Motion Sync Thread 
            //    await Task.Delay(500);
            //    RobotReset();
            //    robotStreamMode_MotionStreamBuffer.Clear();
            //    robotStreamMode_MotionStreamBufferReverse.Clear();
            //    StreamTable.UpdateProgressBar(0);
            //    StreamTable.SetProgressBar(0);
            //    Player_UpdateSlider(0);
            //    ShowInfoMessage("Stream Table", "Stream Motion Buffers Cleared!");
            //}
            //catch (Exception ex)
            //{
            //    ShowErrorMessageAndLog("Stream Table Error", ex.Message);
            //}
        }

        private void StreamTable_RunHandler(object sender, RoutedEventArgs e)
        {
           // RobotStreamMode_RunExecute();
        }

        //private void StreamTable_UpdateTotalTime(TimeSpan totaltime)
        //{
        //    StreamTable.UpdateTotalTime(totaltime);
        //}

        //private void StreamTable_UIEnableFunctions()
        //{
        //    StreamTable.EnableMotionFunctions();
        //}

        //private void StreamTable_UIDisableFunctions()
        //{
        //    StreamTable.DisableMotionFunctions();
        //}

        #endregion

        #region COMMANDS TABLE

        private void CommandsTable_Initialize()
        {
            CommandsTable.LogEvent += CommandsTable_LogHandler;
            CommandsTable.SetStartPointEvent += CommandsTable_SetStartPointHandler;
            //CommandsTable.AddLinearMoveEvent += CommandsTable_AddLinearMoveHandler;
            //CommandsTable.EditMoveEvent += CommandsTable_EditMoveHandler;
            //CommandsTable.AddCircularMoveEvent += CommandsTable_AddCircularMoveHandler;
            //CommandsTable.GotoSelPointEvent += CommandsTable_GotoSelPointHandler;
            //CommandsTable.TableLoadedEvent += CommandsTable_TableLoadedHandler;
            //CommandsTable.ClearEvent += CommandsTable_ClearHandler;
            //CommandsTable.DeleteLastEvent += CommandsTable_DeleteLastHandler;
            //CommandsTable.NavCurrentRowChangedEvent += CommandsTable_NavCurrentRowChangedHandler;
            //CommandsTable.NavStartEvent += CommandsTable_NavStartHandler;
            //CommandsTable.NavPrevEvent += CommandsTable_NavPrevHandler;
            //CommandsTable.NavNextEvent += CommandsTable_NavNextHandler;
            //CommandsTable.NavEndEvent += CommandsTable_NavEndHandler;
        }

        private void CommandsTable_LogHandler(object sender, LogEventArgs e)
        {
            AddLog($"COMMANDS TABLE: {e.LogMsg}");
        }

        //public void CommandsTable_SetProgressBar(double total)
        //{
        //    progBar_CommandsSequence_RunProgress.Maximum = total;
        //    tb_CommandsSequence_RunItemsTotal.Text = total.ToString();
        //    progBar_CommandsSequence_RunProgress.Value = 0;
        //    tb_CommandsSequence_RunProgress.Text = "0.00";
        //}

        //public void CommandsTable_UpdateProgressBar(double prog)
        //{
        //    progBar_CommandsSequence_RunProgress.Value = prog;
        //    tb_CommandsSequence_RunProgress.Text = Math.Round(prog, 2).ToString();
        //}

        //private void CommandsTable_UpdateTotalTime(TimeSpan totaltime)
        //{
        //    tb_CommandsSequence_totalTime.Text = totaltime.ToString(@"hh\:mm\:ss\.fff");
        //}

        //private void CommandsTable_ResetTotalTime()
        //{
        //    var totaltime = new TimeSpan(0, 0, 0);
        //    tb_CommandsSequence_totalTime.Text = totaltime.ToString(@"hh\:mm\:ss\.fff");
        //}

        private void CommandsTable_SetStartPoint()
        {
            //CommandsTable.SetStartPoint(
            //    new CommandsTablePoint(
            //        Simulator.slider_X.Value,
            //        Simulator.slider_Y.Value,
            //        Simulator.slider_Z.Value,
            //        Simulator.slider_RX.Value,
            //        Simulator.slider_RY.Value,
            //        Simulator.slider_RZ.Value,
            //        ToolController.FocusPosition,
            //        ToolController.IrisPosition,
            //        ToolController.ZoomPosition,
            //        ToolController.AuxPosition
            //    )
            //);
            //Simulator_AddBezierPoint();
            //UserControls_UISequenceState_Ready();
            //robotCommandsMode_IsRehearsed = false;
        }

        //private void CommandsTable_AddLinearMove(CommandsTablePoint destPoint)
        //{
        //    CommandsTable.AddLinearMove(
        //        destPoint,
        //        Settings.Default.ROBOT_DEFAULT_VEL,
        //        Settings.Default.ROBOT_DEFAULT_ACC,
        //        Settings.Default.ROBOT_DEFAULT_DEC,
        //        Settings.Default.ROBOT_DEFAULT_LEAVE,
        //        Settings.Default.ROBOT_DEFAULT_REACH
        //    );
        //    Simulator_AddBezierPoint();
        //    robotCommandsMode_IsRehearsed = false;
        //}

        //private void CommandsTable_AddCircularMove(CommandsTablePoint midpt, CommandsTablePoint endpt)
        //{
        //    CommandsTable.AddCircularMove(
        //        midpt,
        //        endpt,
        //        Settings.Default.ROBOT_DEFAULT_VEL,
        //        Settings.Default.ROBOT_DEFAULT_ACC,
        //        Settings.Default.ROBOT_DEFAULT_DEC,
        //        Settings.Default.ROBOT_DEFAULT_LEAVE,
        //        Settings.Default.ROBOT_DEFAULT_REACH
        //    );
        //    //Simulator_AddArc(midpt, endpt);
        //    Simulator_AddBezierPoint("MidPt", midpt.X, midpt.Y, midpt.Z, midpt.RX, midpt.RY, midpt.RZ);
        //    Simulator_AddBezierPoint("EndPt", endpt.X, endpt.Y, endpt.Z, endpt.RX, endpt.RY, endpt.RZ);
        //    UserControls_UISequenceState_Ready();
        //    robotCommandsMode_IsRehearsed = false;
        //}

        //private double CommandsTable_GetFocusValue(int index)
        //{
        //    return CommandsTable.GetFocusValue(index);
        //}

        //private double CommandsTable_GetIrisValue(int index)
        //{
        //    return CommandsTable.GetIrisValue(index);
        //}

        //private double CommandsTable_GetZoomValue(int index)
        //{
        //    return CommandsTable.GetZoomValue(index);
        //}

        //private double CommandsTable_GetAuxValue(int index)
        //{
        //    return CommandsTable.GetAuxValue(index);
        //}

        //private void CommandsTable_NavStart() // TODO FIX WaitEndMove=False or Implement MoveIDMonitor for end of move.
        //{
        //    var index = CommandsTable.NavStart();
        //    if (index > -1)
        //    {
        //        for (int i = CommandsTable.GetCurrentRowIndex(); i >= index; i--) // Sequential move from current point to Start
        //        {
        //            RobotCommandsMode_GoToPoint(index, false, true);
        //            UserControls_Tb_SeqNavCurPoint.Text = CommandsTable.Tb_NavCurPoint.Text;
        //        }
        //    }
        //}

        //private int CommandsTable_NavPrev()
        //{
        //    var index = CommandsTable.NavPrev();
        //    if (index > -1)
        //    {
        //        RobotCommandsMode_GoToPoint(index + 1, true, true); // REVERSE from current index
        //        UserControls_Tb_SeqNavCurPoint.Text = CommandsTable.Tb_NavCurPoint.Text;
        //    }
        //    return index;
        //}

        //private int CommandsTable_NavNext()
        //{
        //    var index = CommandsTable.NavNext();
        //    if (index > -1)
        //    {
        //        RobotCommandsMode_GoToPoint(index, false, true);
        //        UserControls_Tb_SeqNavCurPoint.Text = CommandsTable.Tb_NavCurPoint.Text;
        //    }
        //    return index;
        //}

        //// TODO Sequence Thread to Navigate through all the in between points
        //// TODO CommandsTable.SelectedRow(index) ----> Visual update selected row when navigating.
        //private void CommandsTable_NavEnd() // TODO FIX WaitEndMove=False or Implement MoveIDMonitor for end of move.
        //{
        //    var index = CommandsTable.NavEnd();
        //    if (index > -1)
        //    {
        //        for (int i = CommandsTable.GetCurrentRowIndex(); i <= index; i++)  // Sequential move from current point to End
        //        {
        //            RobotCommandsMode_GoToPoint(index, false, true);
        //            UserControls_Tb_SeqNavCurPoint.Text = CommandsTable.Tb_NavCurPoint.Text;
        //        }
        //    }
        //}

        //private void CommandsTable_EditStart()
        //{
        //    try
        //    {
        //        var selectedIndex = CommandsTable.GetSelectedRowIndex();
        //        if (selectedIndex == 0)
        //        {
        //            CommandsTable.EditStart(
        //                new CommandsTablePoint(
        //                    Convert.ToDouble(Simulator.tb_simPos_x.Text),
        //                    Convert.ToDouble(Simulator.tb_simPos_y.Text),
        //                    Convert.ToDouble(Simulator.tb_simPos_z.Text),
        //                    Convert.ToDouble(Simulator.tb_simPos_rx.Text),
        //                    Convert.ToDouble(Simulator.tb_simPos_ry.Text),
        //                    Convert.ToDouble(Simulator.tb_simPos_rz.Text),
        //                    ToolController.FocusPosition,
        //                    ToolController.IrisPosition,
        //                    ToolController.ZoomPosition,
        //                    ToolController.AuxPosition
        //                )
        //            );
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowErrorMessageAndLog("Robot Circular Move Error", $"Error Editing Circular Move: {ex}");
        //    }
        //}

        private void CommandsTable_SetStartPointHandler(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                CommandsTable_SetStartPoint();
            });
        }

        //private void CommandsTable_AddLinearMoveHandler(object sender, RoutedEventArgs e)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        CommandsTablePoint lastPoint = CommandsTable.GetLastMovePoint(); // Get Last Point from Last Move in Commands Sequence
        //        tb_RobotLinearMove_PA_X.Text = lastPoint.X.ToString();
        //        tb_RobotLinearMove_PA_Y.Text = lastPoint.Y.ToString();
        //        tb_RobotLinearMove_PA_Z.Text = lastPoint.Z.ToString();
        //        tb_RobotLinearMove_PA_RX.Text = lastPoint.RX.ToString();
        //        tb_RobotLinearMove_PA_RY.Text = lastPoint.RY.ToString();
        //        tb_RobotLinearMove_PA_RZ.Text = lastPoint.RZ.ToString();
        //        tb_RobotLinearMove_PA_Focus.Text = lastPoint.Focus.ToString();
        //        tb_RobotLinearMove_PA_Iris.Text = lastPoint.Iris.ToString();
        //        tb_RobotLinearMove_PA_Zoom.Text = lastPoint.Z.ToString();
        //        tb_RobotLinearMove_PA_Aux.Text = lastPoint.Aux.ToString();

        //        tb_RobotLinearMove_PB_X.Text = Simulator.slider_X.Value.ToString(); // Current Robot Point
        //        tb_RobotLinearMove_PB_Y.Text = Simulator.slider_Y.Value.ToString();
        //        tb_RobotLinearMove_PB_Z.Text = Simulator.slider_Z.Value.ToString();
        //        tb_RobotLinearMove_PB_RX.Text = Simulator.slider_RX.Value.ToString();
        //        tb_RobotLinearMove_PB_RY.Text = Simulator.slider_RY.Value.ToString();
        //        tb_RobotLinearMove_PB_RZ.Text = Simulator.slider_RZ.Value.ToString();
        //        tb_RobotLinearMove_PB_Focus.Text = ToolController.FocusPosition.ToString();
        //        tb_RobotLinearMove_PB_Iris.Text = ToolController.IrisPosition.ToString();
        //        tb_RobotLinearMove_PB_Zoom.Text = ToolController.ZoomPosition.ToString();
        //        tb_RobotLinearMove_PB_Aux.Text = ToolController.AuxPosition.ToString();

        //        RobotLinearMove_UIAddMoveMode();
        //    });
        //}

        //private void CommandsTable_AddCircularMoveHandler(object sender, RoutedEventArgs e)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        CommandsTablePoint lastPoint = CommandsTable.GetLastMovePoint(); // Get Last Point from Last Move in Commands Sequence
        //        tb_RobotCircularMove_PA_X.Text = lastPoint.X.ToString();
        //        tb_RobotCircularMove_PA_Y.Text = lastPoint.Y.ToString();
        //        tb_RobotCircularMove_PA_Z.Text = lastPoint.Z.ToString();
        //        tb_RobotCircularMove_PA_RX.Text = lastPoint.RX.ToString();
        //        tb_RobotCircularMove_PA_RY.Text = lastPoint.RY.ToString();
        //        tb_RobotCircularMove_PA_RZ.Text = lastPoint.RZ.ToString();
        //        tb_RobotCircularMove_PA_Focus.Text = lastPoint.Focus.ToString();
        //        tb_RobotCircularMove_PA_Iris.Text = lastPoint.Iris.ToString();
        //        tb_RobotCircularMove_PA_Zoom.Text = lastPoint.Zoom.ToString();
        //        tb_RobotCircularMove_PA_Aux.Text = lastPoint.Aux.ToString();

        //        tb_RobotCircularMove_PB_X.Text = Simulator.slider_X.Value.ToString(); // Current Robot Point
        //        tb_RobotCircularMove_PB_Y.Text = Simulator.slider_Y.Value.ToString();
        //        tb_RobotCircularMove_PB_Z.Text = Simulator.slider_Z.Value.ToString();
        //        tb_RobotCircularMove_PB_RX.Text = Simulator.slider_RX.Value.ToString();
        //        tb_RobotCircularMove_PB_RY.Text = Simulator.slider_RY.Value.ToString();
        //        tb_RobotCircularMove_PB_RZ.Text = Simulator.slider_RZ.Value.ToString();
        //        tb_RobotCircularMove_PB_Focus.Text = ToolController.FocusPosition.ToString();
        //        tb_RobotCircularMove_PB_Iris.Text = ToolController.IrisPosition.ToString();
        //        tb_RobotCircularMove_PB_Zoom.Text = ToolController.ZoomPosition.ToString();
        //        tb_RobotCircularMove_PB_Aux.Text = ToolController.AuxPosition.ToString();

        //        RobotCircularMove_UIAddMoveMode();
        //    });
        //}

        //private void CommandsTable_EditMoveHandler(object sender, RoutedEventArgs e) // TODO Implement: Simulator->EditPoint, Timeline->EditKeyframe 
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        // Check if a row is selected and valid
        //        var selectedIndex = CommandsTable.GetSelectedRowIndex();
        //        if (selectedIndex < 0) return;

        //        CommandsTableRow row = CommandsTable.GetRowData(selectedIndex);
        //        if (row == null) return;

        //        // SELECTED MOVE IS START
        //        if (row.MoveType == CommandsMoveType.START)
        //        {
        //            if (ShowDialogMessage("Update Start Position", "Update START position to current Robot position?"))
        //            {
        //                CommandsTable_EditStart();
        //            }
        //            CommandsTable.UI_ExitAddEditMode();
        //            return;
        //        }

        //        // SELECTED MOVE IS CIRCULAR
        //        if (row.MoveType == CommandsMoveType.CIRCULAR)
        //        {
        //            tb_RobotCircularMove_RowNumber.Text = row.Id.ToString();
        //            tb_RobotCircularMove_RowName.Text = row.Name;

        //            // Get Selected Move Point B (Middle)
        //            tb_RobotCircularMove_PB_X.Text = row.MidPoint.X.ToString();
        //            tb_RobotCircularMove_PB_Y.Text = row.MidPoint.Y.ToString();
        //            tb_RobotCircularMove_PB_Z.Text = row.MidPoint.Z.ToString();
        //            tb_RobotCircularMove_PB_RX.Text = row.MidPoint.RX.ToString();
        //            tb_RobotCircularMove_PB_RY.Text = row.MidPoint.RY.ToString();
        //            tb_RobotCircularMove_PB_RZ.Text = row.MidPoint.RZ.ToString();
        //            tb_RobotCircularMove_PB_Focus.Text = row.MidPoint.Focus.ToString();
        //            tb_RobotCircularMove_PB_Iris.Text = row.MidPoint.Iris.ToString();
        //            tb_RobotCircularMove_PB_Zoom.Text = row.MidPoint.Zoom.ToString();
        //            tb_RobotCircularMove_PB_Aux.Text = row.MidPoint.Aux.ToString();

        //            // Get Selected Move Point C
        //            tb_RobotCircularMove_PC_X.Text = row.TargetPoint.X.ToString();
        //            tb_RobotCircularMove_PC_Y.Text = row.TargetPoint.Y.ToString();
        //            tb_RobotCircularMove_PC_Z.Text = row.TargetPoint.Z.ToString();
        //            tb_RobotCircularMove_PC_RX.Text = row.TargetPoint.RX.ToString();
        //            tb_RobotCircularMove_PC_RY.Text = row.TargetPoint.RY.ToString();
        //            tb_RobotCircularMove_PC_RZ.Text = row.TargetPoint.RZ.ToString();
        //            tb_RobotCircularMove_PC_Focus.Text = row.TargetPoint.Focus.ToString();
        //            tb_RobotCircularMove_PC_Iris.Text = row.TargetPoint.Iris.ToString();
        //            tb_RobotCircularMove_PC_Zoom.Text = row.TargetPoint.Zoom.ToString();
        //            tb_RobotCircularMove_PC_Aux.Text = row.TargetPoint.Aux.ToString();

        //            // Get Selected Move Vel, Acc, Dec
        //            numbox_RobotCircularMove_Vel.Value = (int)row.Vel;
        //            numbox_RobotCircularMove_Accel.Value = (int)row.Acc;
        //            numbox_RobotCircularMove_Decel.Value = (int)row.Dec;

        //            // Get Selected Move Point A (from previous move last point)
        //            row = CommandsTable.GetRowData(selectedIndex - 1);
        //            tb_RobotCircularMove_PA_X.Text = row.TargetPoint.X.ToString();
        //            tb_RobotCircularMove_PA_Y.Text = row.TargetPoint.Y.ToString();
        //            tb_RobotCircularMove_PA_Z.Text = row.TargetPoint.Z.ToString();
        //            tb_RobotCircularMove_PA_RX.Text = row.TargetPoint.RX.ToString();
        //            tb_RobotCircularMove_PA_RY.Text = row.TargetPoint.RY.ToString();
        //            tb_RobotCircularMove_PA_RZ.Text = row.TargetPoint.RZ.ToString();
        //            tb_RobotCircularMove_PA_Focus.Text = row.TargetPoint.Focus.ToString();
        //            tb_RobotCircularMove_PA_Iris.Text = row.TargetPoint.Iris.ToString();
        //            tb_RobotCircularMove_PA_Zoom.Text = row.TargetPoint.Zoom.ToString();
        //            tb_RobotCircularMove_PA_Aux.Text = row.TargetPoint.Aux.ToString();

        //            RobotCircularMove_UIEditMoveMode();
        //        }

        //        // SELECTED MOVE IS LINEAR
        //        else if (row.MoveType == CommandsMoveType.LINEAR)
        //        {
        //            tb_RobotLinearMove_RowNumber.Text = row.Id.ToString();
        //            tb_RobotLinearMove_RowName.Text = row.Name;

        //            // Get Selected Move Point B 
        //            tb_RobotLinearMove_PB_X.Text = row.TargetPoint.X.ToString();
        //            tb_RobotLinearMove_PB_Y.Text = row.TargetPoint.Y.ToString();
        //            tb_RobotLinearMove_PB_Z.Text = row.TargetPoint.Z.ToString();
        //            tb_RobotLinearMove_PB_RX.Text = row.TargetPoint.RX.ToString();
        //            tb_RobotLinearMove_PB_RY.Text = row.TargetPoint.RY.ToString();
        //            tb_RobotLinearMove_PB_RZ.Text = row.TargetPoint.RZ.ToString();
        //            tb_RobotLinearMove_PB_Focus.Text = row.TargetPoint.Focus.ToString();
        //            tb_RobotLinearMove_PB_Iris.Text = row.TargetPoint.Iris.ToString();
        //            tb_RobotLinearMove_PB_Zoom.Text = row.TargetPoint.Zoom.ToString();
        //            tb_RobotLinearMove_PB_Aux.Text = row.TargetPoint.Aux.ToString();

        //            // Get Selected Move Vel, Acc, Dec
        //            numbox_RobotLinearMove_Vel.Value = (double)row.Vel;
        //            numbox_RobotLinearMove_Accel.Value = (double)row.Acc;
        //            numbox_RobotLinearMove_Decel.Value = (double)row.Dec;

        //            // Get Selected Move Point A (from previous move last point)
        //            row = CommandsTable.GetRowData(selectedIndex - 1);
        //            tb_RobotLinearMove_PA_X.Text = row.TargetPoint.X.ToString();
        //            tb_RobotLinearMove_PA_Y.Text = row.TargetPoint.Y.ToString();
        //            tb_RobotLinearMove_PA_Z.Text = row.TargetPoint.Z.ToString();
        //            tb_RobotLinearMove_PA_RX.Text = row.TargetPoint.RX.ToString();
        //            tb_RobotLinearMove_PA_RY.Text = row.TargetPoint.RY.ToString();
        //            tb_RobotLinearMove_PA_RZ.Text = row.TargetPoint.RZ.ToString();
        //            tb_RobotLinearMove_PA_Focus.Text = row.TargetPoint.Focus.ToString();
        //            tb_RobotLinearMove_PA_Iris.Text = row.TargetPoint.Iris.ToString();
        //            tb_RobotLinearMove_PA_Zoom.Text = row.TargetPoint.Zoom.ToString();
        //            tb_RobotLinearMove_PA_Aux.Text = row.TargetPoint.Aux.ToString();

        //            RobotLinearMove_UIEditMoveMode();
        //        }

        //        // SELECTED MOVE NO MATCH
        //        else
        //        {
        //            ShowErrorMessageAndLog("Commands Table Error", $"ROBOT Commands Sequence EDIT Command Invalid: {row.Name}.");
        //        }

        //    });
        //}

        //private void CommandsTable_GotoSelPointHandler(object sender, RoutedEventArgs e)
        //{
        //    RobotCommandsMode_GoToSelectedPoint();
        //}

        //private void CommandsTable_NavCurrentRowChangedHandler(object sender, RoutedEventArgs e)
        //{
        //    // Current Nav Point Changed Handler
        //}

        //private void CommandsTable_NavStartHandler(object sender, RoutedEventArgs e)
        //{
        //    CommandsTable_NavStart();
        //}

        //private void CommandsTable_NavPrevHandler(object sender, RoutedEventArgs e)
        //{
        //    CommandsTable_NavPrev();
        //}

        //private void CommandsTable_NavNextHandler(object sender, RoutedEventArgs e)
        //{
        //    CommandsTable_NavNext();
        //}

        //private void CommandsTable_NavEndHandler(object sender, RoutedEventArgs e)
        //{
        //    CommandsTable_NavEnd();
        //}

        //private async void CommandsTable_TableLoadedHandler(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        AddLog($"COMMANDS TABLE: Load Table Data");
        //        ShowShortNotificationMessage("Loading ...");
        //        Simulator_Reset();
        //        // TODO Set duration
        //        Timeline_Clear();
        //        UserControls_Tb_SeqNavCurPoint.Text = CommandsTable.Tb_NavCurPoint.Text;
        //        UserControls_UISequenceState_Ready();
        //        robotCommandsMode_IsRehearsed = false;
        //        IsTargetSet = false;

        //        // Load points to Simulator
        //        for (int i = 0; i < CommandsTable.Count; i++)
        //        {
        //            CommandsTableRow row = CommandsTable.GetRowData(i);
        //            var name = "" + row.Id; //row.Name

        //            if (row.MoveType == CommandsMoveType.START)
        //            {
        //                Simulator_AddBezierPoint(
        //                    name,
        //                    row.TargetPoint.X,
        //                    row.TargetPoint.Y,
        //                    row.TargetPoint.Z,
        //                    row.TargetPoint.RX,
        //                    row.TargetPoint.RY,
        //                    row.TargetPoint.RZ
        //                );
        //                var firstKeyframe = Simulator.GetKeyframeData();
        //                firstKeyframe.FOCUS = row.TargetPoint.Focus;
        //                firstKeyframe.IRIS = row.TargetPoint.Iris;
        //                firstKeyframe.ZOOM = row.TargetPoint.Zoom;
        //                firstKeyframe.AUX = row.TargetPoint.Aux;
        //                Timeline_AddKeyframe(firstKeyframe);
        //            }
        //            else if (row.MoveType == CommandsMoveType.CIRCULAR)
        //            {
        //                // TODO Implement Simulator.AddArc(row.MidPoint,row.TargetPoint) to replace code below

        //                // Add Mid Point 
        //                Simulator_AddBezierPoint(
        //                    name,
        //                    row.MidPoint.X,
        //                    row.MidPoint.Y,
        //                    row.MidPoint.Z,
        //                    row.MidPoint.RX,
        //                    row.MidPoint.RY,
        //                    row.MidPoint.RZ
        //                );
        //                var keyframe_mid = Simulator.GetKeyframeData();
        //                keyframe_mid.FOCUS = row.MidPoint.Focus;
        //                keyframe_mid.IRIS = row.MidPoint.Iris;
        //                keyframe_mid.ZOOM = row.MidPoint.Zoom;
        //                keyframe_mid.AUX = row.MidPoint.Aux;
        //                //Timeline_AddKeyframe(keyframe_mid); // Not Need ?

        //                // Add Last Point  
        //                Simulator_AddBezierPoint(
        //                    name + "mid",
        //                    row.TargetPoint.X,
        //                    row.TargetPoint.Y,
        //                    row.TargetPoint.Z,
        //                    row.TargetPoint.RX,
        //                    row.TargetPoint.RY,
        //                    row.TargetPoint.RZ
        //                );
        //                var keyframe_last = Simulator.GetKeyframeData();
        //                keyframe_last.FOCUS = row.TargetPoint.Focus;
        //                keyframe_last.IRIS = row.TargetPoint.Iris;
        //                keyframe_last.ZOOM = row.TargetPoint.Zoom;
        //                keyframe_last.AUX = row.TargetPoint.Aux;
        //                Timeline_AddKeyframe(keyframe_last);
        //            }
        //            else if (row.MoveType == CommandsMoveType.LINEAR)
        //            {
        //                Simulator_AddBezierPoint(
        //                    name,
        //                    row.TargetPoint.X,
        //                    row.TargetPoint.Y,
        //                    row.TargetPoint.Z,
        //                    row.TargetPoint.RX,
        //                    row.TargetPoint.RY,
        //                    row.TargetPoint.RZ
        //                );
        //                var keyframe = Simulator.GetKeyframeData();
        //                keyframe.FOCUS = row.TargetPoint.Focus;
        //                keyframe.IRIS = row.TargetPoint.Iris;
        //                keyframe.ZOOM = row.TargetPoint.Zoom;
        //                keyframe.AUX = row.TargetPoint.Aux;
        //                Timeline_AddKeyframe(keyframe);
        //            }
        //            else
        //            {
        //                ShowErrorMessageAndLog("COMMANDS MODE TABLE", "Error Loading new table data to Simulator and Timeline.");
        //            }

        //            //row.Vel
        //            //row.Acc
        //            //row.Dec
        //            //row.Leave
        //            //row.Reach

        //        }

        //        if (RobotConnectionStatus())
        //        {
        //            RobotCommandsMode_NavGoToStart();
        //        }
        //        await Task.Delay(20);
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowErrorMessageAndLog("Commands Table Error", ex.Message);
        //    }
        //}

        //private void CommandsTable_DeleteLastHandler(object sender, RoutedEventArgs e) // TODO Implement
        //{
        //    try
        //    {
        //        // TODO Delete Last Point from Simulator
        //        //Simulator_DeleteLastPoint();
        //        // TODO Delete Last Keyframe from Simulator
        //        //Timeline_DeleteLastKeyframe();
        //        robotCommandsMode_IsRehearsed = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowErrorMessageAndLog("Commands Table Delete Last Error", ex.Message);
        //    }
        //}

        //private void CommandsTable_ClearTable()
        //{
        //    try
        //    {
        //        CommandsTable.ClearTable(); // Triggers Event and Calls ClearHandler below
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowErrorMessageAndLog("Commands Table Clear Error", ex.Message);
        //    }
        //}

        //private void CommandsTable_ClearHandler(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        // TODO Add Settings Option or DialogBoxes
        //        Simulator_Reset();
        //        Timeline_Clear();
        //        UserControls_Tb_SeqNavCurPoint.Text = "-";
        //        UserControls_UISequenceState_Initial();
        //        robotCommandsMode_IsRehearsed = false;
        //        IsTargetSet = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowErrorMessageAndLog("Commands Table Clear Error", ex.Message);
        //    }
        //}

        //private void CommandsTable_UIEnableFunctions()
        //{
        //    grpbox_CommandsSequence_MotionFunctions.IsEnabled = true;
        //}

        //private void CommandsTable_UIDisableFunctions()
        //{
        //    grpbox_CommandsSequence_MotionFunctions.IsEnabled = false;
        //}

        #endregion

        #endregion

    }

}

namespace MonkeyMotionControl.UI 
{ 
    /// <summary>
    /// This converter is used to bind a value to a group of radio buttons, and can be used with an enum, bool, int, etc.
    /// It depends on the converter parameter, which maps a radio button to a specific value.
    /// Note: this converter is not designed to work with flag enums (e.g. multiple checkboxes scenario).
    /// Here is an example of the bidning for an enum value:
    /// <RadioButton Content="Option 1" IsChecked="{Binding EnumValue, Converter={StaticResource EnumToRadioBoxCheckedConverter}, 
    ///     ConverterParameter={x:Static local:TestEnum.Option1}}" GroupName="EnumGroup"/>
    /// <RadioButton Content="Option 2" IsChecked="{Binding EnumValue, Converter={StaticResource EnumToRadioBoxCheckedConverter}, 
    ///     ConverterParameter={x:Static local:TestEnum.Option2}}" GroupName="EnumGroup"/>
    /// </summary>
    public class RadioButtonCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }

    public class EnumBindingSourceExtension : System.Windows.Markup.MarkupExtension
    {
        private Type _enumType;
        public Type EnumType
        {
            get { return this._enumType; }
            set
            {
                if (value != this._enumType)
                {
                    if (null != value)
                    {
                        Type enumType = Nullable.GetUnderlyingType(value) ?? value;
                        if (!enumType.IsEnum)
                            throw new ArgumentException("Type must be for an Enum.");
                    }

                    this._enumType = value;
                }
            }
        }

        public EnumBindingSourceExtension() { }

        public EnumBindingSourceExtension(Type enumType)
        {
            EnumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (null == this._enumType)
                throw new InvalidOperationException("The EnumType must be specified.");

            Type actualEnumType = Nullable.GetUnderlyingType(this._enumType) ?? this._enumType;
            Array enumValues = Enum.GetValues(actualEnumType);

            if (actualEnumType == this._enumType)
                return enumValues;

            Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            return tempArray;
        }

    }
}