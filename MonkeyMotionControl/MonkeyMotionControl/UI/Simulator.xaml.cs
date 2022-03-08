using SharpDX;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using HelixToolkit.Wpf.SharpDX.Assimp;
using HelixToolkit.Wpf.SharpDX.Controls;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using System;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Transform3D = System.Windows.Media.Media3D.Transform3D;
using Transform3DGroup = System.Windows.Media.Media3D.Transform3DGroup;
using RotateTransform3D = System.Windows.Media.Media3D.RotateTransform3D;
using AxisAngleRotation3D = System.Windows.Media.Media3D.AxisAngleRotation3D;
using TranslateTransform3D = System.Windows.Media.Media3D.TranslateTransform3D;
using Colors = System.Windows.Media.Colors;
using Brushes = System.Windows.Media.Brushes;
using MonkeyMotionControl.Simulator;
using System.ComponentModel;
using Microsoft.Win32;
using System.Windows.Input;
using MonkeyMotionControl.Properties;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;

namespace MonkeyMotionControl.UI
{
    /// <summary>
    /// Interaction logic for Simulator.xaml
    /// </summary>
    public partial class Simulator : UserControl, INotifyPropertyChanged
    {

        #region EVENTS

        public delegate void LogEventHandler(object sender, LogEventArgs args);
        public event LogEventHandler LogEvent;

        public delegate void ErrorLogEventHandler(object sender, LogEventArgs args);
        public event ErrorLogEventHandler ErrorLogEvent;

        public delegate void UIGoToStreamTableEventHandler(object sender, EventArgs args);
        public event UIGoToStreamTableEventHandler UIGoToStreamTableEvent;

        public delegate void UIGoToCommandsTableEventHandler(object sender, EventArgs args);
        public event UIGoToCommandsTableEventHandler UIGoToCommandsTableEvent;

        public delegate void UIGoToLiveControlsEventHandler(object sender, EventArgs args);
        public event UIGoToLiveControlsEventHandler UIGoToLiveControlsEvent;

        public delegate void StreamTable_AddRowEventHandler(object sender, StreamTableRowEventArgs args);
        public event StreamTable_AddRowEventHandler StreamTable_AddRowEvent;

        public delegate void StreamTableUpdateRowEventHandler(object sender, StreamTableRowEventArgs args);
        public event StreamTableUpdateRowEventHandler StreamTable_UpdateRowEvent;

        public delegate void StreamTableClearEventHandler(object sender, EventArgs args);
        public event StreamTableClearEventHandler StreamTable_ClearEvent;

        public delegate void CommandsTable_AddRowEventHandler(object sender, CommandsTableRowEventArgs args);
        public event CommandsTable_AddRowEventHandler CommandsTable_AddRowEvent;

        public delegate void CommandsTableClearEventHandler(object sender, EventArgs args);
        public event CommandsTableClearEventHandler CommandsTable_ClearEvent;

        public delegate void RobotToolOffsetUpdatedEventHandler(object sender, EventArgs args);
        public event RobotToolOffsetUpdatedEventHandler RobotToolOffsetUpdatedEvent;

        // Other Properties Inside Code:
        // TrackingEnabledChangedEvent
        // FocusDistanceChangedEvent
        // SyncEnabledChangedEvent

        #endregion

        #region PROPERTIES

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Duration Dependency Property

        public static readonly RoutedEvent DurationChangedRoutedEvent = EventManager.RegisterRoutedEvent(
            nameof(DurationChangedEvent), RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<double>), typeof(Simulator));

        public event RoutedPropertyChangedEventHandler<double> DurationChangedEvent
        {
            add { AddHandler(DurationChangedRoutedEvent, value); }
            remove { RemoveHandler(DurationChangedRoutedEvent, value); }
        }

        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(
            nameof(Duration), typeof(double), typeof(Simulator),
            new FrameworkPropertyMetadata(Settings.Default.DURATION, new PropertyChangedCallback(DurationChanged)));

        /// <summary>
        /// Duration Dependency Property.
        /// </summary>
        public double Duration
        {
            get { return (double)GetValue(DurationProperty); }
            set
            {
                SetValue(DurationProperty, value);
                OnPropertyChanged(nameof(Duration));
            }
        }

        private static void DurationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as Simulator).OnDurationChanged(
                new RoutedPropertyChangedEventArgs<double>((double)args.OldValue, (double)args.NewValue, DurationChangedRoutedEvent));
        }

        private void OnDurationChanged(RoutedPropertyChangedEventArgs<double> e)
        {
            System.Diagnostics.Debug.WriteLine($"Simulator OnDurationChanged() {e.NewValue}");
            //SimPlayer.Duration = value;
            Sim_Duration.Value = e.NewValue;
            RaiseEvent(e);
        }

        #endregion

        #region IsTrackingEnabled Dependency Property

        public static readonly RoutedEvent IsTrackingEnabledChangedRoutedEvent = EventManager.RegisterRoutedEvent(
            nameof(TrackingEnabledChangedEvent), RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(Simulator));

        public event RoutedPropertyChangedEventHandler<bool> TrackingEnabledChangedEvent
        {
            add { AddHandler(IsTrackingEnabledChangedRoutedEvent, value); }
            remove { RemoveHandler(IsTrackingEnabledChangedRoutedEvent, value); }
        }

        public static readonly DependencyProperty IsTrackingEnabledProperty = DependencyProperty.Register(
            nameof(IsTrackingEnabled), typeof(bool), typeof(Simulator), new FrameworkPropertyMetadata(false,
                new PropertyChangedCallback(IsTrackingEnabledChanged)));

        /// <summary>
        /// Simulator Tracking Enabled Property.
        /// </summary>
        public bool IsTrackingEnabled
        {
            get { return (bool)GetValue(IsTrackingEnabledProperty); }
            set
            {
                SetValue(IsTrackingEnabledProperty, value);
                OnPropertyChanged(nameof(IsTrackingEnabled));
            }
        }

        private static void IsTrackingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as Simulator).OnTrackingEnabledChanged(
                new RoutedPropertyChangedEventArgs<bool>((bool)args.OldValue, (bool)args.NewValue, IsTrackingEnabledChangedRoutedEvent));
        }

        private void OnTrackingEnabledChanged(RoutedPropertyChangedEventArgs<bool> e)
        {
            //System.Diagnostics.Debug.WriteLine("OnTrackingEnabledChanged()");
            if (e.NewValue)
            {
                slider_RX.IsEnabled = false;
                slider_RY.IsEnabled = false;
                slider_RZ.IsEnabled = false;
                tb_simPos_rx.IsEnabled = false;
                tb_simPos_ry.IsEnabled = false;
                tb_simPos_rz.IsEnabled = false;
                IsFKControlEnabled = false;
                HidePrismRef();
                // Update the camera to track the target point
                OnPlanningMode_MoveCameraPoint(
                    new Point3D(
                        Convert.ToDouble(tb_simPos_x.Text),
                        Convert.ToDouble(tb_simPos_y.Text),
                        Convert.ToDouble(tb_simPos_z.Text)
                    )
                );
            }
            else
            {
                slider_RX.IsEnabled = true;
                slider_RY.IsEnabled = true;
                slider_RZ.IsEnabled = true;
                tb_simPos_rx.IsEnabled = true;
                tb_simPos_ry.IsEnabled = true;
                tb_simPos_rz.IsEnabled = true;
                ShowPrismRef();
            }
            RaiseEvent(e);
        }

        #endregion

        #region IsSyncEnabled Dependency Property

        public static readonly RoutedEvent IsSyncEnabledChangedRoutedEvent = EventManager.RegisterRoutedEvent(
            nameof(SyncEnabledChangedEvent), RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(Simulator));

        public event RoutedPropertyChangedEventHandler<bool> SyncEnabledChangedEvent
        {
            add { AddHandler(IsSyncEnabledChangedRoutedEvent, value); }
            remove { RemoveHandler(IsSyncEnabledChangedRoutedEvent, value); }
        }

        public static readonly DependencyProperty IsSyncEnabledProperty = DependencyProperty.Register(
            nameof(IsSyncEnabled), typeof(bool), typeof(Simulator), new FrameworkPropertyMetadata(false,
                new PropertyChangedCallback(IsSyncEnabledChanged)));

        /// <summary>
        /// Simulator Sync Enabled Property.
        /// </summary>
        public bool IsSyncEnabled
        {
            get { return (bool)GetValue(IsSyncEnabledProperty); }
            set
            {
                SetValue(IsSyncEnabledProperty, value);
                OnPropertyChanged(nameof(IsSyncEnabled));
            }
        }

        private static void IsSyncEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as Simulator).OnSyncEnabledChanged(
                new RoutedPropertyChangedEventArgs<bool>((bool)args.OldValue, (bool)args.NewValue, IsSyncEnabledChangedRoutedEvent));
        }

        private void OnSyncEnabledChanged(RoutedPropertyChangedEventArgs<bool> e)
        {
            //System.Diagnostics.Debug.WriteLine("OnSyncEnabledChanged()");
            if (e.NewValue)
            {
                //Enable Sync
                grid_PointData.IsEnabled = false;
                grid_SimJointSliders.IsEnabled = false;
                grid_SimCartesianSliders.IsEnabled = false;
                chk_EnableTracking.IsEnabled = false;
                btn_TrackingToggle.IsEnabled = false;
                Btn_AllowFKIK.IsEnabled = false;
                Btn_GoHome.IsEnabled = false;
                HideCamManipulator();
                IsTrackingEnabled = false;
                ClearSelection();
            }
            else
            {
                //Disable Sync
                grid_PointData.IsEnabled = true;
                grid_SimJointSliders.IsEnabled = true;
                grid_SimCartesianSliders.IsEnabled = true;
                chk_EnableTracking.IsEnabled = true;
                btn_TrackingToggle.IsEnabled = true;
                Btn_AllowFKIK.IsEnabled = true;
                Btn_GoHome.IsEnabled = true;
                ShowCamManipulator();
                ClearSelection();
            }

            RaiseEvent(e);
        }

        #endregion

        #region IsPathTracingEnabled Dependency Property

        public static readonly RoutedEvent IsPathTracingEnabledChangedRoutedEvent = EventManager.RegisterRoutedEvent(
            nameof(IsPathTracingEnabledChangedEvent), RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(Simulator));

        public event RoutedPropertyChangedEventHandler<bool> IsPathTracingEnabledChangedEvent
        {
            add { AddHandler(IsPathTracingEnabledChangedRoutedEvent, value); }
            remove { RemoveHandler(IsPathTracingEnabledChangedRoutedEvent, value); }
        }

        public static readonly DependencyProperty IsPathTracingEnabledProperty = DependencyProperty.Register(
            nameof(IsPathTracingEnabled), typeof(bool), typeof(Simulator),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(IsPathTracingEnabledChanged)));

        /// <summary>
        /// IsPathTracingEnabled Dependency Property.
        /// </summary>
        public bool IsPathTracingEnabled
        {
            get { return (bool)GetValue(IsPathTracingEnabledProperty); }
            set
            {
                SetValue(IsPathTracingEnabledProperty, value);
                OnPropertyChanged(nameof(IsPathTracingEnabled));
            }
        }

        private static void IsPathTracingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as Simulator).OnIsPathTracingEnabledChanged(
                new RoutedPropertyChangedEventArgs<bool>((bool)args.OldValue, (bool)args.NewValue, IsPathTracingEnabledChangedRoutedEvent));
        }

        private void OnIsPathTracingEnabledChanged(RoutedPropertyChangedEventArgs<bool> e)
        {
            //System.Diagnostics.Debug.WriteLine("OnIsPathTracingEnabledChanged()");
            //e.NewValue
            RaiseEvent(e);
        }

        #endregion

        #region IsPathTracingMarkerEnabled Dependency Property

        public static readonly RoutedEvent IsPathTracingMarkerEnabledChangedRoutedEvent = EventManager.RegisterRoutedEvent(
            nameof(IsPathTracingMarkerEnabledChangedEvent), RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(Simulator));

        public event RoutedPropertyChangedEventHandler<bool> IsPathTracingMarkerEnabledChangedEvent
        {
            add { AddHandler(IsPathTracingMarkerEnabledChangedRoutedEvent, value); }
            remove { RemoveHandler(IsPathTracingMarkerEnabledChangedRoutedEvent, value); }
        }

        public static readonly DependencyProperty IsPathTracingMarkerEnabledProperty = DependencyProperty.Register(
            nameof(IsPathTracingMarkerEnabled), typeof(bool), typeof(Simulator),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(IsPathTracingMarkerEnabledChanged)));

        /// <summary>
        /// IsPathTracingMarkerEnabled Dependency Property.
        /// </summary>
        public bool IsPathTracingMarkerEnabled
        {
            get { return (bool)GetValue(IsPathTracingMarkerEnabledProperty); }
            set
            {
                SetValue(IsPathTracingMarkerEnabledProperty, value);
                OnPropertyChanged(nameof(IsPathTracingMarkerEnabled));
            }
        }

        private static void IsPathTracingMarkerEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as Simulator).OnIsPathTracingMarkerEnabledChanged(
                new RoutedPropertyChangedEventArgs<bool>((bool)args.OldValue, (bool)args.NewValue, IsPathTracingMarkerEnabledChangedRoutedEvent));
        }

        private void OnIsPathTracingMarkerEnabledChanged(RoutedPropertyChangedEventArgs<bool> e)
        {
            //System.Diagnostics.Debug.WriteLine("OnIsPathTracingMarkerEnabledChanged()");
            pathTracer.MarkerEnable = e.NewValue;
            RaiseEvent(e);
        }

        #endregion

        #region FocusDistance Dependency Property

        public static readonly RoutedEvent FocusDistanceChangedRoutedEvent = EventManager.RegisterRoutedEvent(
            nameof(FocusDistanceChangedEvent), RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<float>), typeof(Simulator));

        public event RoutedPropertyChangedEventHandler<float> FocusDistanceChangedEvent
        {
            add { AddHandler(FocusDistanceChangedRoutedEvent, value); }
            remove { RemoveHandler(FocusDistanceChangedRoutedEvent, value); }
        }

        public static readonly DependencyProperty FocusDistanceProperty = DependencyProperty.Register(
            nameof(FocusDistance), typeof(float), typeof(Simulator), new FrameworkPropertyMetadata(1000.0f,
                new PropertyChangedCallback(FocusDistanceChanged)));

        /// <summary>
        /// Simulator Focus Distance Dependency Property.
        /// </summary>
        public float FocusDistance
        {
            get { return (float)GetValue(FocusDistanceProperty); }
            set
            {
                SetValue(FocusDistanceProperty, value);
                OnPropertyChanged(nameof(FocusDistance));
            }
        }

        private static void FocusDistanceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as Simulator).OnFocusDistanceChanged(
                new RoutedPropertyChangedEventArgs<float>((float)args.OldValue, (float)args.NewValue, FocusDistanceChangedRoutedEvent));
        }

        private void OnFocusDistanceChanged(RoutedPropertyChangedEventArgs<float> e)
        {
            //System.Diagnostics.Debug.WriteLine("OnFocusDistanceChanged()");
            //e.NewValue
            RaiseEvent(e);
        }

        #endregion

        #region TargetDistance Dependency Property

        public static readonly RoutedEvent TargetDistanceChangedRoutedEvent = EventManager.RegisterRoutedEvent(
            nameof(TargetDistanceChangedEvent), RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<float>), typeof(Simulator));

        public event RoutedPropertyChangedEventHandler<float> TargetDistanceChangedEvent
        {
            add { AddHandler(TargetDistanceChangedRoutedEvent, value); }
            remove { RemoveHandler(TargetDistanceChangedRoutedEvent, value); }
        }

        public static readonly DependencyProperty TargetDistanceProperty = DependencyProperty.Register(
            nameof(TargetDistance), typeof(float), typeof(Simulator), new FrameworkPropertyMetadata(0.0f,
                new PropertyChangedCallback(TargetDistanceChanged)));

        /// <summary>
        /// Simulator Target Distance Dependency Property.
        /// </summary>
        public float TargetDistance
        {
            get { return (float)GetValue(TargetDistanceProperty); }
            set
            {
                SetValue(TargetDistanceProperty, value);
                OnPropertyChanged(nameof(TargetDistance));
            }
        }

        private static void TargetDistanceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as Simulator).OnTargetDistanceChanged(
                new RoutedPropertyChangedEventArgs<float>((float)args.OldValue, (float)args.NewValue, TargetDistanceChangedRoutedEvent));
        }

        private void OnTargetDistanceChanged(RoutedPropertyChangedEventArgs<float> e)
        {
            //System.Diagnostics.Debug.WriteLine("OnTargetDistanceChanged()");
            //e.NewValue
            RaiseEvent(e);
        }

        #endregion

        #region CameraMount Dependency Property

        public static readonly RoutedEvent CameraMountChangedRoutedEvent = EventManager.RegisterRoutedEvent(
            nameof(CameraMountChangedEvent), RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<ToolPlacement>), typeof(Simulator));

        public event RoutedPropertyChangedEventHandler<ToolPlacement> CameraMountChangedEvent
        {
            add { AddHandler(CameraMountChangedRoutedEvent, value); }
            remove { RemoveHandler(CameraMountChangedRoutedEvent, value); }
        }

        public static readonly DependencyProperty CameraMountProperty = DependencyProperty.Register(
            nameof(CameraMount), typeof(ToolPlacement), typeof(Simulator),
            new FrameworkPropertyMetadata(ToolPlacement.BOTTOM, new PropertyChangedCallback(CameraMountChanged)));

        /// <summary>
        /// CameraMount Dependency Property.
        /// </summary>
        public ToolPlacement CameraMount
        {
            get { return (ToolPlacement)GetValue(CameraMountProperty); }
            set
            {
                SetValue(CameraMountProperty, value);
                OnPropertyChanged(nameof(CameraMount));
            }
        }

        private static void CameraMountChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as Simulator).OnCameraMountChanged(
                new RoutedPropertyChangedEventArgs<ToolPlacement>((ToolPlacement)args.OldValue, (ToolPlacement)args.NewValue, CameraMountChangedRoutedEvent));
        }

        private void OnCameraMountChanged(RoutedPropertyChangedEventArgs<ToolPlacement> e)
        {
            //System.Diagnostics.Debug.WriteLine("OnCameraMountChanged()");
            if (e.OldValue != e.NewValue)
            {
                IsTrackingEnabled = false;
                tb_cam_mount.Text = e.NewValue.ToString();
                ReplaceCamera(e.NewValue);
                Reset();
                RaiseEvent(e);
            }
        }

        #endregion

        #region CameraOffset Dependency Property

        public static readonly RoutedEvent CameraOffsetChangedRoutedEvent = EventManager.RegisterRoutedEvent(
            nameof(CameraOffsetChangedEvent), RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<Vector3>), typeof(Simulator));

        public event RoutedPropertyChangedEventHandler<Vector3> CameraOffsetChangedEvent
        {
            add { AddHandler(CameraOffsetChangedRoutedEvent, value); }
            remove { RemoveHandler(CameraOffsetChangedRoutedEvent, value); }
        }

        public static readonly DependencyProperty CameraOffsetProperty = DependencyProperty.Register(
            nameof(CameraOffset), typeof(Vector3), typeof(Simulator),
            new FrameworkPropertyMetadata(Vector3.Zero, new PropertyChangedCallback(CameraOffsetChanged)));

        /// <summary>
        /// CameraOffset Dependency Property.
        /// </summary>
        public Vector3 CameraOffset
        {
            get { return (Vector3)GetValue(CameraOffsetProperty); }
            set
            {
                SetValue(CameraOffsetProperty, value);
                OnPropertyChanged(nameof(CameraOffset));
            }
        }

        private static void CameraOffsetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as Simulator).OnCameraOffsetChanged(
                new RoutedPropertyChangedEventArgs<Vector3>((Vector3)args.OldValue, (Vector3)args.NewValue, CameraOffsetChangedRoutedEvent));
        }

        private void OnCameraOffsetChanged(RoutedPropertyChangedEventArgs<Vector3> e)
        {
            //System.Diagnostics.Debug.WriteLine("OnCameraOffsetChanged()");
            if (e.NewValue != e.OldValue)
            {
                if (CameraMount == ToolPlacement.BOTTOM)
                {
                    Settings.Default.CAMERA_BOTTOM_OFFSET_X = e.NewValue.X;
                    Settings.Default.CAMERA_BOTTOM_OFFSET_Y = e.NewValue.Y;
                    Settings.Default.CAMERA_BOTTOM_OFFSET_Z = e.NewValue.Z;
                }
                else if (CameraMount == ToolPlacement.FRONT)
                {
                    Settings.Default.CAMERA_FRONT_OFFSET_X = e.NewValue.X;
                    Settings.Default.CAMERA_FRONT_OFFSET_Y = e.NewValue.Y;
                    Settings.Default.CAMERA_FRONT_OFFSET_Z = e.NewValue.Z;
                }
                else { }
                tb_OffsetLocalX.Text = Math.Round(e.NewValue.X, 2).ToString();
                tb_OffsetLocalY.Text = Math.Round(e.NewValue.Y, 2).ToString();
                tb_OffsetLocalZ.Text = Math.Round(e.NewValue.Z, 2).ToString();
                RobotToolOffsetUpdatedEvent?.Invoke(this, new EventArgs()); // Triggers robot offset update -> RobotSetToolOffset();
                RobotKinematics.CameraCenterZOffset = e.NewValue.Z; // TODO: RobotKinematics add X,Y offsets -> change or modify kinematics library
                UpdateAll(
                    new JointPos(
                        slider_J1.Value,
                        slider_J2.Value,
                        slider_J3.Value,
                        slider_J4.Value,
                        slider_J5.Value,
                        slider_J6.Value
                    )
                );
                RaiseEvent(e);
            }
        }

        #endregion

        #region FullscreenMode Dependency Property

        public static readonly RoutedEvent FullscreenModeChangedRoutedEvent = EventManager.RegisterRoutedEvent(
            nameof(FullscreenModeChangedEvent), RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(Simulator));

        public event RoutedPropertyChangedEventHandler<bool> FullscreenModeChangedEvent
        {
            add { AddHandler(FullscreenModeChangedRoutedEvent, value); }
            remove { RemoveHandler(FullscreenModeChangedRoutedEvent, value); }
        }

        public static readonly DependencyProperty FullscreenModeProperty = DependencyProperty.Register(
            nameof(FullscreenMode), typeof(bool), typeof(Simulator),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(FullscreenModeChanged)));

        /// <summary>
        /// FullscreenMode Dependency Property.
        /// </summary>
        public bool FullscreenMode
        {
            get { return (bool)GetValue(FullscreenModeProperty); }
            set
            {
                SetValue(FullscreenModeProperty, value);
                OnPropertyChanged(nameof(FullscreenMode));
            }
        }

        private static void FullscreenModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as Simulator).OnFullscreenModeChanged(
                new RoutedPropertyChangedEventArgs<bool>((bool)args.OldValue, (bool)args.NewValue, FullscreenModeChangedRoutedEvent));
        }

        private void OnFullscreenModeChanged(RoutedPropertyChangedEventArgs<bool> e)
        {
            //System.Diagnostics.Debug.WriteLine("OnFullscreenModeChanged()");
            UIRefreshView();
            RaiseEvent(e);
        }

        #endregion

        #region CameraViewMode Dependency Property

        public static readonly RoutedEvent CameraViewModeChangedRoutedEvent = EventManager.RegisterRoutedEvent(
            nameof(CameraViewModeChangedEvent), RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(Simulator));

        public event RoutedPropertyChangedEventHandler<bool> CameraViewModeChangedEvent
        {
            add { AddHandler(CameraViewModeChangedRoutedEvent, value); }
            remove { RemoveHandler(CameraViewModeChangedRoutedEvent, value); }
        }

        public static readonly DependencyProperty CameraViewModeProperty = DependencyProperty.Register(
            nameof(CameraViewMode), typeof(bool), typeof(Simulator),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(CameraViewModeChanged)));

        /// <summary>
        /// CameraViewMode Dependency Property.
        /// </summary>
        public bool CameraViewMode
        {
            get { return (bool)GetValue(CameraViewModeProperty); }
            set
            {
                SetValue(CameraViewModeProperty, value);
                OnPropertyChanged(nameof(CameraViewMode));
            }
        }

        private static void CameraViewModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as Simulator).OnCameraViewModeChanged(
                new RoutedPropertyChangedEventArgs<bool>((bool)args.OldValue, (bool)args.NewValue, CameraViewModeChangedRoutedEvent));
        }

        private void OnCameraViewModeChanged(RoutedPropertyChangedEventArgs<bool> e)
        {
            //System.Diagnostics.Debug.WriteLine("OnCameraViewModeChanged()");
            if (e.NewValue)
                UpdateCameraViewport();

            UIRefreshView();
            RaiseEvent(e);
        }

        #endregion

        private double FLANGE_HEIGHT = 0.0;
        public bool IsSimInitialized { get; set; } = false;
        private bool IsCalculatingTracking { get; set; } = false;
        public bool IsLoading { get; private set; } = true;
        public int StepSize { get; set; } = 1;
        private Vector3 RobotFlangeOffset { get; set; } = new Vector3(0, 0, 550); // Staubli Robot Z position relative to Axis 2 above 550mm
        public bool IsPlayForward { get; set; } = true;
        public bool IsLastPoint { get; set; } = false;
        public bool IsFirstPoint { get; set; } = false;
        public bool IsStreamTableClear { get; set; } = false;
        public bool IsCommandsTableClear { get; set; } = false;
        public bool IsPanControlEnabled { get; set; } = false;
        public bool IsNavigating { get; set; } = false;
        private bool IsPlaybackPaused { get; set; } = false;
        private bool IsHoming { get; set; } = false;

        public static readonly DependencyProperty IsCalculateUpdateUIProperty = DependencyProperty.Register(
            nameof(IsCalculateUpdateUI), typeof(bool), typeof(Simulator), new FrameworkPropertyMetadata(true));
        public bool IsCalculateUpdateUI
        {
            get { return (bool)GetValue(IsCalculateUpdateUIProperty); }
            set
            {
                SetValue(IsCalculateUpdateUIProperty, value);
                OnPropertyChanged(nameof(IsCalculateUpdateUI));
            }
        }

        public bool IsCalculateThreadRunFlag { get; set; } = false;

        public bool IsCalculating
        {
            get
            {
                if (IsCalculateThreadRunFlag)
                {
                    return _isCalculating;
                }
                else
                {
                    if (timerCalc != null && timerCalc.IsEnabled)
                        return true;
                    else
                        return false;
                }
            }
            set
            {
                _isCalculating = value;
            }
        }
        private bool _isCalculating = false;

        public bool IsPlaying
        {
            get
            {
                if (PlaybackTimer != null && PlaybackTimer.IsEnabled)
                    return true;
                else
                    return false;
            }
        }

        private Vector3 CameraPosOffset
        {
            get
            {
                Matrix toolTransformMatrix = new Matrix();
                foreach (var node in RobotJoints[7].model.Traverse())
                {
                    if (node is MeshNode m)
                    {
                        toolTransformMatrix = m.ModelMatrix;
                        break;
                    }
                }
                return Vector3.Transform(
                    new Vector3(CameraOffset.X, CameraOffset.Y, 0), //Z=0 TEMP fix, Kinematic Function pre-adjusted with ZOffsetFromFlange, TODO: Fix kinematic Function to add X and Y offsets? 
                    new Matrix3x3(toolTransformMatrix.M11, toolTransformMatrix.M12, toolTransformMatrix.M13,
                                toolTransformMatrix.M21, toolTransformMatrix.M22, toolTransformMatrix.M23,
                                toolTransformMatrix.M31, toolTransformMatrix.M32, toolTransformMatrix.M33)
                    );
            }
        }

        private bool _highlightSelection = false;
        public bool HighlightSelection
        {
            get
            {
                return _highlightSelection;
            }
            set
            {
                _highlightSelection = value;
                HighlightSelectionFunc(MainScene, value);
            }
        }

        private bool _showWireframe = false;
        public bool ShowWireframe
        {
            get
            {
                return _showWireframe;
            }
            set
            {
                _showWireframe = value;
                ShowWireframeFunc(MainScene, value);
            }
        }

        private bool _renderFlat = false;
        public bool RenderFlat
        {
            get
            {
                return _renderFlat;
            }
            set
            {
                _renderFlat = value;
                RenderFlatFunc(MainScene, value);
            }
        }

        private bool _renderEnvironmentMap = false;
        public bool RenderEnvironmentMap
        {
            get => _renderEnvironmentMap;
            set
            {
                _renderEnvironmentMap = value;
                environmentMap.IsRendering = value;
                var scene = MainScene.GroupNode;
                if (scene != null)
                {
                    foreach (var node in scene.Traverse())
                    {
                        if (node is MaterialGeometryNode m && m.Material is PBRMaterialCore material)
                        {
                            material.RenderEnvironmentMap = value;
                        }
                    }
                }
            }
        }

        private double _totalLength = 0;
        public double TotalLength
        {
            get { return _totalLength; }
            set
            {
                _totalLength = value;
                tb_total_length.Text = Math.Round(value, 2).ToString();
            }
        }

        private bool _controlPointsEnabled = true;
        public bool IsControlPointsEnabled
        {
            get { return _controlPointsEnabled; }
            set
            {
                _controlPointsEnabled = value;
                chk_ShowControlPoints.IsChecked = value;
            }
        }

        private bool _trackingLineEnabled = false;
        public bool IsTrackingLineEnabled
        {
            get { return _trackingLineEnabled; }
            set
            {
                _trackingLineEnabled = value;
                chk_ShowTrackingLine.IsChecked = value;
            }
        }

        private bool _allowFK = false;
        public bool IsFKControlEnabled
        {
            get { return _allowFK; }
            set
            {
                _allowFK = value;
                if (value)
                {
                    UIState_SimSliders_IKMode();
                }
                else
                {
                    UIState_SimSliders_FKMode();
                }
            }
        }

        private bool _linkHandle = false;
        public bool IsLinkHandle
        {
            get { return _linkHandle; }
            set
            {
                _linkHandle = value;
                chk_LinkHandle.IsChecked = value;
            }
        }

        private bool _showRobotArm = true;
        public bool IsShowRobotArm
        {
            get { return _showRobotArm; }
            set
            {
                _showRobotArm = value;
                chk_ShowRobotArm.IsChecked = value;
            }
        }

        private bool _boundingBoxEnabled = false;
        public bool IsBoundingBoxEnabled
        {
            get { return _boundingBoxEnabled; }
            set
            {
                _boundingBoxEnabled = value;
                chk_ShowBoundingBox.IsChecked = value;
            }
        }

        private bool _prismEnabled = true;
        public bool IsPrismEnabled
        {
            get { return _prismEnabled; }
            set
            {
                _prismEnabled = value;
                chk_ShowPrism.IsChecked = value;
            }
        }

        private bool _bezierLabelEnabled = true;
        public bool IsBezierLabelEnabled
        {
            get { return _bezierLabelEnabled; }
            set
            {
                _bezierLabelEnabled = value;
                chk_ShowBezierLabel.IsChecked = value;
            }
        }

        private bool _limitSphereEnabled = false;
        public bool IsLimitSphereEnabled
        {
            get { return _limitSphereEnabled; }
            set
            {
                _limitSphereEnabled = value;
                chk_ShowLimitSphere.IsChecked = value;
            }
        }

        private bool _playRealtime = true;
        public bool IsPlayRealtime
        {
            get { return _playRealtime; }
            set
            {
                _playRealtime = value;
                chk_PlayRealtime.IsChecked = value;
            }
        }

        #endregion

        #region VARIABLES

        private CancellationTokenSource cts = new CancellationTokenSource();
        private SynchronizationContext context = SynchronizationContext.Current;
        private CompositionTargetEx compositeHelper = new CompositionTargetEx();

        /// Viewport Pan
        private bool panFirstMove = true;
        private Vector2 panLastPos;
        private Vector3D panTargetPos;

        /// Robot Kinematics
        public RobotKinematics RobotKinematics;

        /// Robot Joint Data
        static public List<RobotModel> RobotJoints = null;

        /// Motion Path Data
        private List<Trajectory> lineBezier = new List<Trajectory>();           /// list of curves (list number is curve number)
        public List<CustomCurve> movePaths = new List<CustomCurve>();           /// list of curve points (list number is curve number)
        public List<CustomCurve> movePathsSync = new List<CustomCurve>();       /// list of curve points (list number is curve number)
        public int nSelected = -1;                                              /// number of selected curve
        public Point3D ptSelected = new Point3D();                              /// position of selected point
        public bool bPointSelected = false;                                          /// true when selected point
        SelectedVisualType selectedVisual = SelectedVisualType.None;            /// string of the selected curve("point" or "control")
        public bool bCurveSelected = false;                                     /// true when selected curve
        public int nCurveSelected = -1;                                         /// number of the selected curve
        public bool jointChanged = false;                                       /// true when changed the location of bezier or control point
        public int nPoint = -1;

        /// Trajectory Path Data
        private TargetPath targetPath { get; set; }

        private MotionPath robotMotionPath = new MotionPath();
        private MotionPath targetMotionPath = new MotionPath();


        public struct TrajectoryPoint
        {
            public int index;
            public double[] jointAngles;
            //public double velValue;
            //public double accelValue;
            //public double decelValue;
            public double distToTarget;
            public Point3D targetPosition;
            public TrajectoryPoint(int ind, double[] jointAngles, double distToTarget, Point3D targetPos)
            {
                this.index = ind;
                this.jointAngles = jointAngles;
                this.distToTarget = distToTarget;
                this.targetPosition = targetPos;
            }
        }
        public List<List<TrajectoryPoint>> trajectory = new List<List<TrajectoryPoint>>(); /// Calculated Motion Trajectory (Point data of segments in the whole Path)
        public int trajCurCurve = 0;                                            /// Number of curve when calculating and playing
        public int trajCurCurvePoint = 0;                                       /// Number of point on trajCurCurve-th curve when calculating and playing
        public int trajCurPoint = -1;                                           /// Number of bezier point when calculate, playing and navigating "Next, Prev" buttons
        // List of bezier points data (for point to point information or Commands points?) TODO: Put into a Class or merge with trajectory
        private List<double[]> jointArray = new List<double[]>();               /// List of joint angles(6) on bezier points
        public List<double> velArray = new List<double>();                      /// List of velocity on bezier points
        public List<double> accelArray = new List<double>();                    /// List of acceleration on bezier points
        public List<double> decelArray = new List<double>();                    /// List of deceleration on bezier points
        //public List<Vector3> trackPts = new List<Vector3>();
        public int trajOldTotalCount = 0;
        public int trajTotalCount = 0;

        /// Tracer
        private PathTracer pathTracer;

        /// Playback
        private DispatcherTimer PlaybackTimer;
        private int PlaybackTimer_Interval = 33;
        public float total_time_origin = 0.0f;
        public int play_step = 0;
        public int timeCount_Playback = 0;
        public double curPlayheadTime = 0;
        public double curPlayheadPos = 0;

        /// Limit Sphere
        public bool bInSphere = true;
        private float limitSphereRadius;
        private Point3D limitSpherePoint = new Point3D();

        /// Calculate
        public DispatcherTimer timerCalc;
        public Stopwatch calculateStopwatch;

        /// Simulator
        private bool wasSyncEnabled = false;
        private DispatcherTimer timerSceneTest;
        private int modelCount = 0;

        /// CAMERA ///////////////////
        private double POV_AnimationTime = 1000;
        private Point3D POV_Home_Pos = new Point3D(3132, 1488, 1955);
        private Vector3D POV_Home_Dir = new Vector3D(-2800, -2140, -1680);
        private Vector3D POV_Home_UpDir = new Vector3D(0, 0, 1);
        private Point3D POV_Front_Pos = new Point3D(3600, 0, 800);
        private Vector3D POV_Front_Dir = new Vector3D(-7100, -180, 0);
        private Vector3D POV_Front_UpDir = new Vector3D(0, 0, 1);
        private Point3D POV_Back_Pos = new Point3D(-3600, 0, 800);
        private Vector3D POV_Back_Dir = new Vector3D(7100, 0, 0);
        private Vector3D POV_Back_UpDir = new Vector3D(0, 0, 1);
        private Point3D POV_Left_Pos = new Point3D(500, 3600, 800);
        private Vector3D POV_Left_Dir = new Vector3D(0, -7000, 0);
        private Vector3D POV_Left_UpDir = new Vector3D(0, 0, 1);
        private Point3D POV_Right_Pos = new Point3D(500, -3600, 800);
        private Vector3D POV_Right_Dir = new Vector3D(0, 7000, 0);
        private Vector3D POV_Right_UpDir = new Vector3D(0, 0, 1);
        private Point3D POV_Top_Pos = new Point3D(350, 0, 5600);
        private Vector3D POV_Top_Dir = new Vector3D(0, 0, -7000);
        private Vector3D POV_Top_UpDir = new Vector3D(0, -1, 0);
        private Point3D POV_Bottom_Pos = new Point3D(500, 0, -3600);
        private Vector3D POV_Bottom_Dir = new Vector3D(0, 0, 7000);
        private Vector3D POV_Bottom_UpDir = new Vector3D(0, 1, 0);


        /// VISUAL ///////////////////
        public SceneNodeGroupModel3D MainScene { get; set; }
        public SceneNodeGroupModel3D CameraScene { get; set; }
        private ModelContainer3DX SharedContainer;
        private TextureModel EnvironmentMapTexture;
        private EnvironmentMap3D environmentMap;
        private EnvironmentMap3D environmentMap_CamPOV; // Camera POV Viewport
        private IEffectsManager EffectsManager;
        private IEffectsManager EffectsManager_CamPOV; // Camera POV Viewport
        private AmbientLight3D ambientLight;
        private AmbientLight3D ambientLight_CamPOV; // Camera POV Viewport 
        private PointLight3D pointLight;
        private PointLight3D pointLight_CamPOV; // Camera POV Viewport
        private LightSphere3D pointLightSphereModel;
        private Transform3D pointLightTransform;
        private DirectionalLight3D directionalLight1;
        private DirectionalLight3D directionalLight1_CamPOV;
        private DirectionalLight3D directionalLight2;
        private DirectionalLight3D directionalLight2_CamPOV;
        private DirectionalLight3D directionalLight3;
        private DirectionalLight3D directionalLight3_CamPOV;
        private LightBox lightBox;
        private Camera Camera;
        private Camera Camera2;
        private PlaneGrid3D planeGrid;
        private PlaneGrid3D planeGrid_CamPOV;
        private PlaneFloor3D planeFloor;
        private PlaneFloor3D planeFloor_CamPOV;
        private GroupNode roboticArm;
        private RobotFlangePoint3D robotFlangePoint;
        private LimitSphere3D limitSphere;
        private CameraPoint3D cameraPoint;
        private CameraManipulator3D cameraManipulator;
        private CameraViewPrism3D cameraViewPrism;
        private CameraBoundingBox3D cameraBoundingBox;
        private PrismReferencePoint3D prismRefPoint;
        private CameraManipulator3D prismRefManipulator;
        public TargetPoint3D targetPoint;
        public TargetPoint3D targetPointCameraPOV;

        #endregion

        #region CONSTRUCTOR

        public Simulator()
        {
            IsSimInitialized = false;

            InitializeComponent();
            InitializeViewport3DX();
            DataContext = this;

            Sim_Duration.Value = Duration;
            Sim_StepSize.Value = StepSize;
            IsControlPointsEnabled = true;
            IsTrackingLineEnabled = false;
            IsTrackingEnabled = false;
            IsLinkHandle = false;
            IsShowRobotArm = true;
            IsBoundingBoxEnabled = false;
            IsPrismEnabled = true;
            IsBezierLabelEnabled = true;
            IsLimitSphereEnabled = false;
            IsPanControlEnabled = false;
            IsPathTracingEnabled = false;
            UIState_Initial();
            Initialize_SimPlayer();

            IsLoading = false;
            IsSimInitialized = true;

            GoHomePosition();
            //CameraHome(3000);
        }

        private void InitializeViewport3DX()
        {
            SharedContainer = new ModelContainer3DX();
            MainScene = new SceneNodeGroupModel3D();
            CameraScene = new SceneNodeGroupModel3D();

            /// Effects Manager
            EffectsManager = new DefaultEffectsManager();
            SharedContainer.EffectsManager = EffectsManager; //Viewport.EffectsManager = EffectsManager;

            /// Effects Manager Camera POV
            EffectsManager_CamPOV = new DefaultEffectsManager();
            ViewportCameraPOV.EffectsManager = EffectsManager_CamPOV; // Camera POV Viewport

            /// Post Effect Mesh Highlight
            PostEffectMeshBorderHighlight postEffect_MeshBorderHightlight = new PostEffectMeshBorderHighlight();
            postEffect_MeshBorderHightlight.EffectName = "highlight";
            SharedContainer.Items.Add(postEffect_MeshBorderHightlight);

            /// Post Effect Mesh Outline Glow
            PostEffectMeshOutlineBlur postEffect_MeshOutlineBlur = new PostEffectMeshOutlineBlur();
            postEffect_MeshOutlineBlur.EffectName = "glow";
            postEffect_MeshOutlineBlur.NumberOfBlurPass = 12;
            SharedContainer.Items.Add(postEffect_MeshOutlineBlur);

            /// Shadow Map 
            ShadowMap3D shadowMap = new ShadowMap3D();
            SharedContainer.Items.Add(shadowMap); //Viewport.Items.Add(shadowMap);

            /// Shadow Map Camera POV
            ShadowMap3D shadowMap_CamPOV = new ShadowMap3D();
            ViewportCameraPOV.Items.Add(shadowMap_CamPOV); // Camera POV Viewport 

            /// Environment Map
            EnvironmentMapTexture = Texture.LoadFileToMemory("./Resources/Sim_EnvironmentMap.dds");
            environmentMap = new EnvironmentMap3D();
            environmentMap.Texture = EnvironmentMapTexture;
            environmentMap.IsRendering = false;
            environmentMap.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90));
            SharedContainer.Items.Add(environmentMap);

            /// Environment Map Camera POV
            //EnvironmentMapTexture_CamPOV = Texture.LoadFileToMemory("./Resources/Sim_EnvironmentMap.dds");
            environmentMap_CamPOV = new EnvironmentMap3D();
            environmentMap_CamPOV.Texture = EnvironmentMapTexture;
            environmentMap_CamPOV.IsRendering = false;
            ViewportCameraPOV.Items.Add(environmentMap_CamPOV); // Camera POV Viewport 

            /// Ambient Light
            ambientLight = new AmbientLight3D();
            ambientLight.Color = Colors.LightGray;
            ambientLight.IsRendering = true;
            SharedContainer.Items.Add(ambientLight);

            /// Ambient Light Camera POV
            ambientLight_CamPOV = new AmbientLight3D();
            ambientLight_CamPOV.Color = Colors.LightGray;
            ambientLight_CamPOV.IsRendering = true;
            ViewportCameraPOV.Items.Add(ambientLight_CamPOV); // Camera POV Viewport

            /// Point Light
            pointLight = new PointLight3D();
            pointLight.Attenuation = new Vector3D(100.1f, 100.4f, 10.05f);
            pointLight.Color = Colors.White;
            pointLight.IsRendering = true;
            pointLight.Transform = new TranslateTransform3D(new Vector3D(500, 0, 1000));
            pointLightTransform = pointLight.Transform;
            SharedContainer.Items.Add(pointLight);

            /// Point Light Camera POV
            pointLight_CamPOV = new PointLight3D();
            pointLight_CamPOV.Attenuation = new Vector3D(100.1f, 100.4f, 10.05f);
            pointLight_CamPOV.Color = Colors.White;
            pointLight_CamPOV.IsRendering = true;
            pointLight_CamPOV.Transform = new TranslateTransform3D(new Vector3D(500, 0, 1000));
            ViewportCameraPOV.Items.Add(pointLight_CamPOV); // Camera POV Viewport

            /// Directional Light 1
            directionalLight1 = new DirectionalLight3D();
            directionalLight1.Direction = new Vector3D(-1, -3, -2);
            directionalLight1.Color = Colors.White;
            directionalLight1.IsRendering = true;
            SharedContainer.Items.Add(directionalLight1);

            /// Directional Light 1 Camera POV
            directionalLight1_CamPOV = new DirectionalLight3D();
            directionalLight1_CamPOV.Direction = new Vector3D(-1, -3, -2);
            directionalLight1_CamPOV.Color = Colors.White;
            directionalLight1_CamPOV.IsRendering = true;
            ViewportCameraPOV.Items.Add(directionalLight1_CamPOV); // Camera POV Viewport

            /// Directional Light 2
            directionalLight2 = new DirectionalLight3D();
            directionalLight2.Direction = new Vector3D(-2, 2, -4);
            directionalLight2.Color = Colors.Gray;
            directionalLight2.IsRendering = true;
            SharedContainer.Items.Add(directionalLight2);

            /// Directional Light 2 Camera POV
            directionalLight2_CamPOV = new DirectionalLight3D();
            directionalLight2_CamPOV.Direction = new Vector3D(-2, 2, -4);
            directionalLight2_CamPOV.Color = Colors.Gray;
            directionalLight2_CamPOV.IsRendering = true;
            ViewportCameraPOV.Items.Add(directionalLight2_CamPOV); // Camera POV Viewport

            /// Directional Light 3
            directionalLight3 = new DirectionalLight3D();
            directionalLight3.Direction = new Vector3D(2, 1, -1);
            directionalLight3.Color = Colors.DimGray;
            directionalLight3.IsRendering = true;
            SharedContainer.Items.Add(directionalLight3);

            /// Directional Light 3 Camera POV
            directionalLight3_CamPOV = new DirectionalLight3D();
            directionalLight3_CamPOV.Direction = new Vector3D(2, 1, -1);
            directionalLight3_CamPOV.Color = Colors.DimGray;
            directionalLight3_CamPOV.IsRendering = true;
            ViewportCameraPOV.Items.Add(directionalLight3_CamPOV); // Camera POV Viewport

            /// Camera for Viewport
            Camera = new PerspectiveCamera
            {
                //Position = new Point3D(0, 0, 0),
                //LookDirection = new Vector3D(-1, 0, 0),
                //UpDirection = new Vector3D(0, 0, 1),
                Position = POV_Home_Pos,
                LookDirection = POV_Home_Dir,
                UpDirection = POV_Home_UpDir,
                NearPlaneDistance = 1,
                FarPlaneDistance = 50000
            };

            /// Viewport Settings
            Viewport.Camera = Camera;
            Viewport.EnableSharedModelMode = true;
            Viewport.EnableDesignModeRendering = false;
            Viewport.EnableSwapChainRendering = false;
            Viewport.FXAALevel = FXAALevel.Ultra;
            Viewport.MSAA = MSAALevel.Eight;
            Viewport.IsShadowMappingEnabled = true;
            Viewport.IsInertiaEnabled = true;
            Viewport.ModelUpDirection = new Vector3D(0, 0, 1);
            Viewport.ShowFrameRate = false;
            Viewport.ShowCameraInfo = false;
            Viewport.ShowCameraTarget = false;
            Viewport.ShowFrameDetails = false;
            Viewport.ShowTriangleCountInfo = false;
            Viewport.ShowCoordinateSystem = true;
            Viewport.IsCoordinateSystemMoverEnabled = false;
            Viewport.InfoForeground = Brushes.WhiteSmoke;
            Viewport.CoordinateSystemLabelForeground = Colors.White;
            Viewport.CoordinateSystemHorizontalPosition = 0.88;
            Viewport.CoordinateSystemVerticalPosition = -0.82;
            Viewport.ShowViewCube = true;
            Viewport.IsViewCubeMoverEnabled = false;
            Viewport.IsViewCubeEdgeClicksEnabled = true;
            Viewport.ViewCubeHorizontalPosition = -0.9;
            Viewport.ViewCubeVerticalPosition = -0.9;

            /// Viewport Event Handlers
            Viewport.MouseDown += Viewport_MouseDown;
            Viewport.MouseMove += Viewport_MouseMove;
            Viewport.MouseUp += Viewport_MouseUp;
            //Viewport.MouseDown3D += OnMouseDown;
            //Viewport.MouseMove3D += OnMouseMove;
            //Viewport.MouseUp3D += OnMouseUp;

            /// Camera2 for Viewport Camera POV
            Camera2 = new PerspectiveCamera
            {
                Position = new Point3D(0, 0, 0),
                LookDirection = new Vector3D(1, 0, 0),
                UpDirection = new Vector3D(0, 0, 1),
                NearPlaneDistance = 1,
                FarPlaneDistance = 50000
            };

            /// Viewport Camera POV Settings
            ViewportCameraPOV.Title = "Camera POV";
            ViewportCameraPOV.Camera = Camera2;
            ViewportCameraPOV.EnableSharedModelMode = false;
            ViewportCameraPOV.EnableDesignModeRendering = false;
            ViewportCameraPOV.EnableSwapChainRendering = false;
            ViewportCameraPOV.FXAALevel = FXAALevel.Ultra;
            ViewportCameraPOV.MSAA = MSAALevel.Eight;
            ViewportCameraPOV.IsShadowMappingEnabled = true;
            ViewportCameraPOV.IsInertiaEnabled = true;
            ViewportCameraPOV.ModelUpDirection = new Vector3D(0, 0, 1);
            ViewportCameraPOV.InfoForeground = Brushes.WhiteSmoke;
            ViewportCameraPOV.ShowFrameRate = false;
            ViewportCameraPOV.ShowCameraInfo = false;
            ViewportCameraPOV.ShowCameraTarget = true;
            ViewportCameraPOV.ShowFrameDetails = false;
            ViewportCameraPOV.ShowTriangleCountInfo = false;
            ViewportCameraPOV.ShowCoordinateSystem = false;
            ViewportCameraPOV.CoordinateSystemLabelForeground = Colors.White;
            ViewportCameraPOV.CoordinateSystemHorizontalPosition = 0.88;
            ViewportCameraPOV.CoordinateSystemVerticalPosition = -0.82;
            ViewportCameraPOV.ShowViewCube = false;
            ViewportCameraPOV.IsEnabled = false; // View Only

            /// Target Path
            targetPath = new TargetPath(this);

            robotMotionPath = new MotionPath(this, PathType.MOTIONPATH, Color.Green);
            targetMotionPath = new MotionPath(this, PathType.TARGETPATH, Color.Yellow);

            /// Tracer
            pathTracer = new PathTracer(MainScene);

            /// Robotic Arm
            Initialize_RoboticArm(ref RobotJoints);

            /// Robotic Arm Limit Boundary Sphere
            var limitSphereCenterPoint = new Point3D(RobotJoints[1].joint.rotPointX, RobotJoints[1].joint.rotPointY, RobotJoints[1].joint.rotPointZ);
            limitSphereRadius = (float)CalculateDistance(limitSphereCenterPoint.ToVector3(), new Vector3(2200, 0, 0));
            limitSphere = new LimitSphere3D(limitSphereCenterPoint, limitSphereRadius);
            if (IsLimitSphereEnabled) ShowLimitSphere();

            /// Camera Position Point
            var initialCameraPos = new Vector3(1000, 0, 1000);
            cameraPoint = new CameraPoint3D(initialCameraPos);
            //cameraPoint.PositionChangedEvent += CameraPoint_PositionChangedEvent;
            cameraPoint.TransformChanged += CameraPoint_TransformChanged;
            cameraPoint.Tag = new AttachedNodeViewModel(cameraPoint);
            MainScene.AddNode(cameraPoint);

            /// Camera Manipulator (X,Y,Z,PIP)
            cameraManipulator = new CameraManipulator3D();
            cameraManipulator.Position = cameraPoint.Position;
            cameraManipulator.Target = cameraPoint;
            cameraManipulator.Tag = new AttachedNodeViewModel(cameraManipulator.SceneNode);
            MainScene.AddNode(cameraManipulator.SceneNode);

            /// Target Mark Point
            var initialTargetPos = new Vector3(Settings.Default.TARGET_POINT_X, Settings.Default.TARGET_POINT_Y, Settings.Default.TARGET_POINT_Z);
            targetPoint = new TargetPoint3D(initialTargetPos);
            targetPoint.TransformChanged += TargetPoint_TransformChanged;
            targetPoint.Tag = new AttachedNodeViewModel(targetPoint);
            MainScene.AddNode(targetPoint);

            /// Target Mark Point Camera POV Viewport
            targetPointCameraPOV = new TargetPoint3D(initialTargetPos);
            targetPointCameraPOV.Tag = new AttachedNodeViewModel(targetPointCameraPOV);
            CameraScene.AddNode(targetPointCameraPOV); // Camera POV Scene

            /// Camera View Prism
            cameraViewPrism = new CameraViewPrism3D(cameraPoint.Position);
            cameraViewPrism.SceneNode.Tag = new AttachedNodeViewModel(cameraViewPrism.SceneNode);
            MainScene.AddNode(cameraViewPrism.SceneNode);

            /// Camera Visual Reference Point of Prism
            prismRefPoint = new PrismReferencePoint3D(targetPoint.Position);
            prismRefPoint.TransformChanged += PrismRefPoint_TransformChanged;
            prismRefPoint.Tag = new AttachedNodeViewModel(prismRefPoint);
            MainScene.AddNode(prismRefPoint);

            /// Camera Visual Reference Point Manipulator (X,Y,Z,PIP)
            prismRefManipulator = new CameraManipulator3D();
            prismRefManipulator.Position = prismRefPoint.Position;
            prismRefManipulator.Target = prismRefPoint;
            prismRefManipulator.SceneNode.Name = "PrismRef Manipulator";
            prismRefManipulator.Tag = new AttachedNodeViewModel(prismRefManipulator.SceneNode);
            //MainScene.AddNode(prismRefManipulator.SceneNode);

            //if (IsTrackingLineEnabled) HelixViewport3D.Children.Add(trackLine);

            /// Model Manipulator Test
            //ModelManipulator modelManipulator = new ModelManipulator();
            //modelManipulator.CenterOffset = targetPoint.Position;
            //modelManipulator.Target = prismRefPoint;
            //MainScene.AddNode(modelManipulator.SceneNode);

            /// Plane Grid
            planeGrid = new PlaneGrid3D();
            planeGrid.SceneNode.Tag = new AttachedNodeViewModel(planeGrid.SceneNode);
            MainScene.AddNode(planeGrid.SceneNode);

            /// Plane Grid for Camera POV Viewport
            planeGrid_CamPOV = new PlaneGrid3D();
            planeGrid_CamPOV.SceneNode.Tag = new AttachedNodeViewModel(planeGrid_CamPOV.SceneNode);
            CameraScene.AddNode(planeGrid_CamPOV.SceneNode); // Camera POV Scene

            /// Plane Floor
            planeFloor = new PlaneFloor3D(Vector3.Zero);
            planeFloor.SceneNode.Tag = new AttachedNodeViewModel(planeFloor.SceneNode);
            MainScene.AddNode(planeFloor.SceneNode);

            /// Plane Floor for Camera POV Viewport
            planeFloor_CamPOV = new PlaneFloor3D(Vector3.Zero);
            planeFloor_CamPOV.SceneNode.Tag = new AttachedNodeViewModel(planeFloor_CamPOV.SceneNode);
            CameraScene.AddNode(planeFloor_CamPOV.SceneNode); // Camera POV Scene

            /// Light Box
            lightBox = new LightBox(new Vector3(0, 0, 2000));
            lightBox.SceneNode.Tag = new AttachedNodeViewModel(lightBox.SceneNode);
            MainScene.AddNode(lightBox.SceneNode);

            /// Point Light Source Sphere
            pointLightSphereModel = new LightSphere3D(pointLightTransform.ToMatrix().TranslationVector.ToPoint3D(), 30);
            pointLightSphereModel.Tag = new AttachedNodeViewModel(pointLightSphereModel.SceneNode);
            MainScene.AddNode(pointLightSphereModel.SceneNode);

            /// Test Animate Point Light and Model
            pointLightTransform = CreateAnimatedTransform1(new Vector3D(1000, 0, 2000), new Vector3D(0, 0, 1), 6);
            pointLight.Transform = pointLightTransform;
            pointLightSphereModel.Transform = pointLightTransform;

            UpdateCameraOffset();

            /// Move Robot to Home Position
            GoHomePosition();

            SharedContainer.Items.Add(MainScene);
            Viewport.SharedModelContainer = SharedContainer;
            //Viewport.Items.Add(MainScene);

            //SharedContainer.Items.Add(LoadedModel);
            //ViewportCameraPOV.SharedModelContainer = SharedContainer; // TODO?
            ViewportCameraPOV.Items.Add(CameraScene);

            //compositeHelper.Rendering += CompositeHelper_Rendering;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void CompositeHelper_Rendering(object sender, System.Windows.Media.RenderingEventArgs e)
        {

        }

        #endregion

        #region METHODS

        public void SceneTest(SceneNode node = null)
        {
            Vector3 pos = new Vector3(1000, 1000, 1000);
            timerSceneTest = new DispatcherTimer();
            timerSceneTest.Interval = TimeSpan.FromMilliseconds(50);
            timerSceneTest.Tick += async (s, e) =>
            {
                //timer_test(s, e);

                var trans = new TranslateTransform3D(pos.ToVector3D());
                pointLight.Transform = trans;
                pointLightSphereModel.Transform = trans;

                if (pos.Y < 1000)
                {
                    pos.Y += 100;
                    //timerSceneTest.Stop();
                }
                else
                {
                    pos.Y -= 100;
                }
            };
            timerSceneTest.IsEnabled = true;
            timerSceneTest.Start();
        }

        private Transform3D CreateAnimatedTransform1(Vector3D translate, Vector3D axis, double speed = 4)
        {
            var lightTrafo = new Transform3DGroup();
            lightTrafo.Children.Add(new TranslateTransform3D(translate));

            var rotateAnimation = new System.Windows.Media.Animation.Rotation3DAnimation
            {
                RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever,
                By = new AxisAngleRotation3D(axis, 90),
                Duration = TimeSpan.FromSeconds(speed / 4),
                IsCumulative = true,
            };

            var rotateTransform = new RotateTransform3D();
            rotateTransform.BeginAnimation(RotateTransform3D.RotationProperty, rotateAnimation);
            lightTrafo.Children.Add(rotateTransform);

            return lightTrafo;
        }

        private void Initialize_RoboticArm(ref List<RobotModel> jointModels)
        {
            try
            {
                jointModels = new List<RobotModel>();
                roboticArm = new GroupNode();
                RobotKinematics = new RobotKinematics();
                List<string> robotModelFilepaths = RobotKinematics.GetRobot3DFilesPath(CameraMount);
                List<JointDH> robotJointsDH = RobotKinematics.GetRobotDHKinematics();

                var importer = new Importer();
                importer.Configuration.GlobalScale = 1.0f;

                for (int i = 0; i < robotModelFilepaths.Count; i++)
                {
                    //Log($"{robotModelFilepaths[i]}");
                    var RAHelixScene = importer.Load("./3DModel/" + robotModelFilepaths[i]);
                    var first = true;
                    var _tmp = RAHelixScene.Root.Traverse();
                    foreach (var mesh in RAHelixScene.Root.Traverse())
                    {
                        if (first)
                        {
                            mesh.Name = $"Joint {i + 1}";
                            first = false;
                        }
                        mesh.Tag = new AttachedNodeViewModel(mesh);

                        //Environment Map
                        if (mesh is MaterialGeometryNode m)
                        {
                            if (m.Material is PBRMaterialCore pbr)
                            {
                                pbr.RenderEnvironmentMap = RenderEnvironmentMap;
                            }
                            else if (m.Material is PhongMaterialCore phong)
                            {
                                phong.RenderEnvironmentMap = RenderEnvironmentMap;
                            }
                        }

                        if (i == 7) /// Camera Mount
                            FLANGE_HEIGHT = mesh.Bounds.Size.Z;
                    }

                    if (i < 6) // Add Joints 1 to 6
                    {
                        // Add Robot Joints
                        jointModels.Add(new RobotModel(robotJointsDH[i], RAHelixScene.Root, $"Joint {i + 1}"));

                        // Add Robotic Arm Flange Point
                        if (i == 5)
                        {
                            robotFlangePoint = new RobotFlangePoint3D(Vector3.Zero);
                            robotFlangePoint.Tag = new AttachedNodeViewModel(robotFlangePoint);
                            MainScene.AddNode(robotFlangePoint);
                        }
                    }
                    else if (i == 6)
                    {
                        // Add Robot Camera Base
                        jointModels.Add(new RobotModel(null, RAHelixScene.Root, "Base"));
                    }
                    else if (i == 7)
                    {
                        // Add Robot Camera Mount
                        jointModels.Add(new RobotModel(null, RAHelixScene.Root, "Camera " + CameraMount.ToString()));
                    }
                    else if (i == 8)
                    {
                        // Add Robot Camera
                        jointModels.Add(new RobotModel(null, RAHelixScene.Root, "Camera RED")); //+ CameraType.ToString()));
                        jointModels[8].model.TransformChanged += RobotCamera_TransformChanged;

                        // Camera Bounding Box 
                        cameraBoundingBox = new CameraBoundingBox3D(jointModels[8].model);
                        cameraBoundingBox.SceneNode.Tag = new AttachedNodeViewModel(cameraBoundingBox.SceneNode);
                        if (IsBoundingBoxEnabled) MainScene.AddNode(cameraBoundingBox.SceneNode);
                    }

                    roboticArm.AddChildNode(RAHelixScene.Root);
                    MainScene.AddNode(roboticArm);
                }
                roboticArm.Name = "Robotic Arm";
                roboticArm.Tag = new AttachedNodeViewModel(roboticArm);

                // Set Robot Colors
                Color robotColor = Color.DimGray;
                Color robotFlangeColor = Color.LightGray;
                Color mountColor = Color.DimGray;
                Color cameraColor = Color.YellowGreen;
                cameraColor.A = 90;
                ChangeModelColor(jointModels[0].model, robotColor);
                ChangeModelColor(jointModels[1].model, robotColor);
                ChangeModelColor(jointModels[2].model, robotColor);
                ChangeModelColor(jointModels[3].model, robotColor);
                ChangeModelColor(jointModels[4].model, robotColor);
                ChangeModelColor(jointModels[5].model, robotFlangeColor);
                ChangeModelColor(jointModels[6].model, robotColor);
                ChangeModelColor(jointModels[7].model, mountColor);
                ChangeModelColor(jointModels[8].model, cameraColor);

                UIJointSliders_SetLimits();
                //addSphere(new Point3D(joints[0].rotPointX, joints[0].rotPointY, joints[0].rotPointZ), 20);
                //addSphere(new Point3D(joints[1].rotPointX, joints[1].rotPointY, joints[1].rotPointZ), 20);
                //addSphere(new Point3D(joints[2].rotPointX, joints[2].rotPointY, joints[2].rotPointZ), 20);
                //addSphere(new Point3D(joints[3].rotPointX, joints[3].rotPointY, joints[3].rotPointZ), 20);
                //addSphere(new Point3D(joints[4].rotPointX, joints[4].rotPointY, joints[4].rotPointZ), 20);
                //addSphere(new Point3D(joints[5].rotPointX, joints[5].rotPointY, joints[5].rotPointZ), 20);

            }
            catch (Exception e)
            {
                ErrorLog($"Initialize Robotic Arm Error: {e.Message}");
            }
        }

        private void UpdateRoboticArm(double J1, double J2, double J3, double J4, double J5, double J6, 
            double camOffsetX = 0, double camOffsetY = 0, double camOffsetZ = 0, 
            double camRotAxisX = 0, double camRotAxisY = 0, double camRotAxisZ = 0)
        {
            // TODO: Check new joint angles within range limits

            // Update Joints Angles
            RobotJoints[0].joint.angle = J1;
            RobotJoints[1].joint.angle = J2;
            RobotJoints[2].joint.angle = J3;
            RobotJoints[3].joint.angle = J4;
            RobotJoints[4].joint.angle = J5;
            RobotJoints[5].joint.angle = J6;

            // Update Robot Kinematics Joint Angles (Previous Joints)
            RobotKinematics.CurJointPos.J1 = J1;
            RobotKinematics.CurJointPos.J2 = J2;
            RobotKinematics.CurJointPos.J3 = J3;
            RobotKinematics.CurJointPos.J4 = J4;
            RobotKinematics.CurJointPos.J5 = J5;
            RobotKinematics.CurJointPos.J6 = J6;

            // Setup Transformations
            Transform3DGroup F1;
            Transform3DGroup F2;
            Transform3DGroup F3;
            Transform3DGroup F4;
            Transform3DGroup F5;
            Transform3DGroup F6;
            Transform3DGroup F7;
            Transform3DGroup F8;
            Transform3DGroup F9;
            RotateTransform3D R;
            TranslateTransform3D T;
            F1 = new Transform3DGroup();
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D((double)RobotJoints[0].joint.rotAxisX, (double)RobotJoints[0].joint.rotAxisY, (double)RobotJoints[0].joint.rotAxisZ), J1), new Point3D((double)RobotJoints[0].joint.rotPointX, (double)RobotJoints[0].joint.rotPointY, (double)RobotJoints[0].joint.rotPointZ));
            F1.Children.Add(R);
            F2 = new Transform3DGroup();
            T = new TranslateTransform3D(0.0, 0.0, 0.0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D((double)RobotJoints[1].joint.rotAxisX, (double)RobotJoints[1].joint.rotAxisY, (double)RobotJoints[1].joint.rotAxisZ), J2), new Point3D((double)RobotJoints[1].joint.rotPointX, (double)RobotJoints[1].joint.rotPointY, (double)RobotJoints[1].joint.rotPointZ));
            F2.Children.Add(T);
            F2.Children.Add(R);
            F2.Children.Add(F1);
            F3 = new Transform3DGroup();
            T = new TranslateTransform3D(0.0, 0.0, 0.0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D((double)RobotJoints[2].joint.rotAxisX, (double)RobotJoints[2].joint.rotAxisY, (double)RobotJoints[2].joint.rotAxisZ), J3), new Point3D((double)RobotJoints[2].joint.rotPointX, (double)RobotJoints[2].joint.rotPointY, (double)RobotJoints[2].joint.rotPointZ));
            F3.Children.Add(T);
            F3.Children.Add(R);
            F3.Children.Add(F2);
            F4 = new Transform3DGroup();
            T = new TranslateTransform3D(0.0, 0.0, 0.0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D((double)RobotJoints[3].joint.rotAxisX, (double)RobotJoints[3].joint.rotAxisY, (double)RobotJoints[3].joint.rotAxisZ), J4), new Point3D((double)RobotJoints[3].joint.rotPointX, (double)RobotJoints[3].joint.rotPointY, (double)RobotJoints[3].joint.rotPointZ));
            F4.Children.Add(T);
            F4.Children.Add(R);
            F4.Children.Add(F3);
            F5 = new Transform3DGroup();
            T = new TranslateTransform3D(0.0, 0.0, 0.0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D((double)RobotJoints[4].joint.rotAxisX, (double)RobotJoints[4].joint.rotAxisY, (double)RobotJoints[4].joint.rotAxisZ), J5), new Point3D((double)RobotJoints[4].joint.rotPointX, (double)RobotJoints[4].joint.rotPointY, (double)RobotJoints[4].joint.rotPointZ));
            F5.Children.Add(T);
            F5.Children.Add(R);
            F5.Children.Add(F4);
            F6 = new Transform3DGroup();
            T = new TranslateTransform3D(0.0, 0.0, 0.0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D((double)RobotJoints[5].joint.rotAxisX, (double)RobotJoints[5].joint.rotAxisY, (double)RobotJoints[5].joint.rotAxisZ), J6), new Point3D((double)RobotJoints[5].joint.rotPointX, (double)RobotJoints[5].joint.rotPointY, (double)RobotJoints[5].joint.rotPointZ));
            F6.Children.Add(T);
            F6.Children.Add(R);
            F6.Children.Add(F5);

            //F7 = new Transform3DGroup();
            //T = new TranslateTransform3D(camOffsetX, camOffsetY, camOffsetZ);
            //R = new RotateTransform3D(
            //    new AxisAngleRotation3D(
            //        new Vector3D((double)RobotJoints[5].joint.rotAxisX, (double)RobotJoints[5].joint.rotAxisY, (double)RobotJoints[5].joint.rotAxisZ),
            //        camRotAxisZ),
            //    new Point3D((double)RobotJoints[5].joint.rotPointX, (double)RobotJoints[5].joint.rotPointY, (double)RobotJoints[5].joint.rotPointZ));
            //F7.Children.Add(R);
            //R = new RotateTransform3D(
            //    new AxisAngleRotation3D(
            //        new Vector3D(1, 0, 0),
            //        camRotAxisX),
            //    new Point3D((double)0, (double)0, (double)RobotJoints[5].joint.rotPointZ + 180));
            //F7.Children.Add(R);
            //R = new RotateTransform3D(
            //    new AxisAngleRotation3D(
            //        new Vector3D(0, 1, 0), 
            //        camRotAxisY),
            //    new Point3D((double)130, (double)0, (double)RobotJoints[5].joint.rotPointZ + 180));
            //F7.Children.Add(R);
            //F7.Children.Add(T);
            //F7.Children.Add(F6);

            /// Test ///
            F7 = new Transform3DGroup();
            T = new TranslateTransform3D(camOffsetX, camOffsetY, camOffsetZ);
            R = new RotateTransform3D(
                new AxisAngleRotation3D(
                    new Vector3D(0, 0, 1), camRotAxisZ),
                new Point3D((double)RobotJoints[5].joint.rotPointX, (double)RobotJoints[5].joint.rotPointY, (double)RobotJoints[5].joint.rotPointZ + cameraBoundingBox.BoundingBox.SizeZ / 2 + FLANGE_HEIGHT));
            F7.Children.Add(R);
            R = new RotateTransform3D(
                new AxisAngleRotation3D(
                    new Vector3D(1, 0, 0), camRotAxisX),
                new Point3D((double)RobotJoints[5].joint.rotPointX, (double)RobotJoints[5].joint.rotPointY, (double)RobotJoints[5].joint.rotPointZ + cameraBoundingBox.BoundingBox.SizeZ / 2 + FLANGE_HEIGHT));
            F7.Children.Add(R);
            R = new RotateTransform3D(
                new AxisAngleRotation3D(
                    new Vector3D(0, 1, 0), camRotAxisY),
                new Point3D((double)RobotJoints[5].joint.rotPointX, (double)RobotJoints[5].joint.rotPointY, (double)RobotJoints[5].joint.rotPointZ + cameraBoundingBox.BoundingBox.SizeZ / 2 + FLANGE_HEIGHT));
            F7.Children.Add(R);
            F7.Children.Add(T);
            F7.Children.Add(F6);

            /// End of Test ///

            // Apply Transformations
            foreach (var node in RobotJoints[0].model.Traverse()) // Joint 1
            {
                if (node is MeshNode m)
                {
                    m.ModelMatrix = F1.ToMatrix();
                }
            }
            foreach (var node in RobotJoints[1].model.Traverse()) // Joint 2
            {
                if (node is MeshNode m)
                {
                    m.ModelMatrix = F2.ToMatrix();
                }
            }
            foreach (var node in RobotJoints[2].model.Traverse()) // Joint 3
            {
                if (node is MeshNode m)
                {
                    m.ModelMatrix = F3.ToMatrix();
                }
            }
            foreach (var node in RobotJoints[3].model.Traverse()) // Joint 4
            {
                if (node is MeshNode m)
                {
                    m.ModelMatrix = F4.ToMatrix();
                }
            }
            foreach (var node in RobotJoints[4].model.Traverse()) // Joint 5
            {
                if (node is MeshNode m)
                {
                    m.ModelMatrix = F5.ToMatrix();
                }
            }
            foreach (var node in RobotJoints[5].model.Traverse()) // Joint 6
            {
                if (node is MeshNode m)
                {
                    m.ModelMatrix = F6.ToMatrix();
                }
            }
            foreach (var node in RobotJoints[7].model.Traverse()) // Camera Mount
            {
                if (node is MeshNode m)
                {
                    m.ModelMatrix = F6.ToMatrix();
                }
            }
            foreach (var node in RobotJoints[8].model.Traverse()) // Camera
            {
                if (node is MeshNode m)
                {                    
                    m.ModelMatrix = F7.ToMatrix();
                }
            }

            // Flange Point
            double flangePointOffsetX = RobotJoints[5].joint.rotPointX;
            double flangePointOffsetY = 0;
            double flangePointOffsetZ = RobotJoints[5].joint.rotPointZ;
            F8 = new Transform3DGroup(); // Transform Group for Flange Point
            T = new TranslateTransform3D(flangePointOffsetX, flangePointOffsetY, flangePointOffsetZ);
            F8.Children.Add(T);
            F8.Children.Add(F7);
            robotFlangePoint.ModelMatrix = F8.ToMatrix();

            // Camera Bounding Box
            double boxOffsetX = 0;
            double boxOffsetY = 0;
            double boxOffsetZ = RobotJoints[5].joint.rotPointZ + (cameraBoundingBox.BoundingBox.SizeZ / 2);
            if (CameraMount == ToolPlacement.BOTTOM)
            {
                boxOffsetX = cameraBoundingBox.BoundingBox.SizeX / 2;
            }
            else if (CameraMount == ToolPlacement.FRONT)
            {
                boxOffsetX = cameraBoundingBox.BoundingBox.SizeX - 20;
            }
            else { }
            F9 = new Transform3DGroup(); // Transform Group for Camera Bounding Box 
            T = new TranslateTransform3D(boxOffsetX, boxOffsetY, boxOffsetZ + FLANGE_HEIGHT);
            F9.Children.Add(T);
            F9.Children.Add(F7);
            cameraBoundingBox.SceneNode.ModelMatrix = F9.ToMatrix();

            if (IsPathTracingEnabled)
            {
                var toolPosition = new Vector3(
                    (float)slider_X.Value,
                    (float)slider_Y.Value,
                    (float)slider_Z.Value + RobotFlangeOffset.Z
                );
                Tracer_AddPoint(toolPosition);
            }
        }

        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            Vector2 position = new Vector2((float)e.GetPosition(Viewport).X, (float)e.GetPosition(Viewport).Y);
            if (!Viewport.UnProject(position, out var ray)) return;

            List<HitTestResult> hits = new List<HitTestResult>();
            var result = Viewport.FindHits(position, ref hits);
            if (result)
            {
                //for (int i = 0; i < hits.Count; i++)
                //{
                //    Log($"Viewport MouseDown hit: {hits[i].ModelHit}");
                //}
                //Log($"Viewport MouseDown hit: {hits[0].ModelHit}"); // Read only the closest hit.
                OnMouseDown(hits[0].ModelHit);
            }
            else
            {
                OnMouseDown(null);
            }
        }

        private void Viewport_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            panFirstMove = true;
        }

        private void Viewport_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                ViewportCamera_Rotate(sender, e);
            }
            else if (e.MiddleButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                ViewportCamera_Pan(sender, e);
            }
        }

        private void ViewportCamera_Pan(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var mouse = e.GetPosition(this);

            if (panFirstMove)
            {
                panLastPos = new Vector2((float)mouse.X, (float)mouse.Y);
                panFirstMove = false;
            }
            else
            {
                var deltaX = mouse.X - panLastPos.X;
                var deltaY = mouse.Y - panLastPos.Y;
                panLastPos = new Vector2((float)mouse.X, (float)mouse.Y);

                var _front = Viewport.Camera.LookDirection;
                _front.Normalize();
                var _right = Vector3D.CrossProduct(_front, new Vector3D(0, 0, 1));
                _right.Normalize();
                var _up = Vector3D.CrossProduct(_right, _front);
                _up.Normalize();

                Viewport.Camera.Position += (_right * -deltaX) * 5;  /// PAN Left & Right
                Viewport.Camera.Position += _up * deltaY * 5;        /// PAN Up & Down
                panTargetPos += (_right * -deltaX) * 5;  /// PAN Left & Right
                panTargetPos += _up * deltaY * 5;        /// PAN Up & Down
            }
        }

        private void ViewportCamera_Rotate(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var frontDirection = Viewport.Camera.LookDirection;
            var target = frontDirection + Viewport.Camera.Position;
            Viewport.Camera.Position = new Point3D(-frontDirection.X + panTargetPos.X, -frontDirection.Y + panTargetPos.Y, -frontDirection.Z + panTargetPos.Z);
        }

        public void OnMouseDown(object modelhit)
        {
            /// Hit Nothing
            if (ReferenceEquals(modelhit, null))
            {
                Log($"ModelHit None.");
                ClearSelection();
                selectedVisual = SelectedVisualType.None;
                return;
            }

            /// Hit PlaneFloor
            else if (modelhit is PlaneFloor3D)
            {
                Log($"ModelHit PlaneFloor3D.");
                ClearSelection();
                selectedVisual = SelectedVisualType.None;
                return;
            }

            /// Hit PlaneGrid
            else if (modelhit is PlaneGrid3D)
            {
                //Log($"ModelHit PlaneGrid3D.");
                ClearSelection();
                selectedVisual = SelectedVisualType.None;
                return;
            }

            /// Hit Camera Manipulator
            else if (modelhit is CameraXAxisManipulator3D || modelhit is CameraYAxisManipulator3D || modelhit is CameraZAxisManipulator3D || modelhit is CameraPIPAxisManipulator3D)
            {
                // Is PrismRefPoint Manipulator
                if (selectedVisual == SelectedVisualType.PrismReferencePoint)
                {
                    return;
                }
                else
                {
                    //Log($"ModelHit CameraManipulator3D.");
                    ClearSelection();
                    selectedVisual = SelectedVisualType.None;
                    return;
                }
            }

            /// Hit Point Manipulator
            else if (modelhit is PointManipulator3D)
            {
                //if (selectedVisual == SelectedVisualType.TargetPoint)
                //{
                //    return;
                //}
                return;
            }

            ///////////

            /// Scene List Selection
            if (modelhit is SceneNode node && node.Tag is AttachedNodeViewModel vm)
            {
                vm.Selected = !vm.Selected;
                Log($"ModelHit Attached SceneNode | Type: {node.GetType().Name} | Name: {node.Name}");
            }
            /// Hit Item not in list
            else
            {
                //Log($"ModelHit Not Attached SceneNode | Type: {modelhit.GetType().Name}");
            }

            //////////   

            /// Hit Point Light Source
            if (modelhit is LightSphere3D)
            {
                //Log($"ModelHit LightSphere3D.");
                ClearSelection();
                selectedVisual = SelectedVisualType.None;

                var plpt = (LightSphere3D)modelhit;

                // Add Point Manipulator
                PointManipulator3D man = new PointManipulator3D();
                man.Position = plpt.SceneNode.ModelMatrix.TranslationVector;
                man.Target = plpt.SceneNode;
                MainScene.AddNode(man.SceneNode);
            }

            /// Hit Visual Reference Point
            else if (modelhit is PrismReferencePoint3D)
            {
                //Log($"ModelHit PrismReferencePoint3D.");
                ClearSelection();
                selectedVisual = SelectedVisualType.PrismReferencePoint;
                //var vpt = (PrismReferencePoint3D)modelhit;
                if (IsSyncEnabled) ShowPrismRefManipulator(true);
                else ShowPrismRefManipulator(false);
            }

            /// Hit Target Point
            else if (modelhit is TargetPoint3D)
            {
                //Log($"ModelHit TargetPoint3D.");
                ClearSelection();
                selectedVisual = SelectedVisualType.TargetPoint;
                bPointSelected = true;

                var tpt = (TargetPoint3D)modelhit;

                // Add Point Manipulator
                PointManipulator3D man = new PointManipulator3D();
                man.Position = tpt.Position;
                man.Target = tpt;
                tpt.Manipulator = man;
                MainScene.AddNode(man.SceneNode);
            }

            /// Hit Bezier Points
            else if (modelhit is BezierMarkPoint3D)
            {
                //Log($"ModelHit BezierMarkPoint3D.");
                ClearSelection();

                var bpt = (BezierMarkPoint3D)modelhit;
                if ((modelhit as BezierMarkPoint3D).PathType is PathType.MOTIONPATH)
                {
                    robotMotionPath.OnMouseDown(modelhit);
                }
                else
                {
                    targetMotionPath.OnMouseDown(modelhit);
                    return;
                }

                /// Display info on UI
                Point3D tt = new Point3D();
                tt.X = targetPoint.Position.X - ptSelected.X;
                tt.Y = targetPoint.Position.Y - ptSelected.Y;
                tt.Z = targetPoint.Position.Z - ptSelected.Z;
                double radius = Math.Sqrt(tt.X * tt.X + tt.Y * tt.Y + tt.Z * tt.Z);
                double theta = Math.Atan2(tt.Y, tt.X) * 57.3;
                double fai = Math.Atan2(Math.Sqrt(tt.X * tt.X + tt.Y * tt.Y), tt.Z) * 57.3;
                tb_PointData_Id.Text = Convert.ToString(nSelected + 1);
                if (nSelected == 0 && nPoint == 0)
                    tb_PointData_Id.Text = Convert.ToString(nSelected);
            }

            /// Hit Control Points
            else if (modelhit is ControlPoint3D)
            {
                //Log($"ModelHit ControlPoint3D.");
                ClearSelection();
                selectedVisual = SelectedVisualType.ControlPoint;

                var cpt = (ControlPoint3D)modelhit;

                //if (cpt.PathType is "TargetPath")
                //{
                //    targetPath.OnMouseDown(sender, args);
                //    return;
                //}

                //Control Point Data
                int num2 = 0;
                while (true)
                {
                    if (num2 < movePaths[nSelected].ptAdjust.Count)
                    {
                        if (!(ptSelected == movePaths[nSelected].ptAdjust[num2].point))
                        {
                            num2++;
                            continue;
                        }
                        nPoint = num2;
                    }
                    break;
                }

                // Hit Arc control point
                if (movePaths[nSelected].curveShape == CurveType.ARC)
                {
                    Vector3D directRadius = new Vector3D();
                    int i = 0, j = 0;
                    if (nSelected == 0)
                        i = 1;
                    if (nSelected == 1)
                        j = 1;
                    directRadius.X = (movePaths[nSelected].ptCurve[i].point.X + movePaths[nSelected + i - 1].ptCurve[j].point.X) / 2 -
                        ptSelected.X;
                    directRadius.Y = (movePaths[nSelected].ptCurve[i].point.Y + movePaths[nSelected + i - 1].ptCurve[j].point.Y) / 2 -
                        ptSelected.Y;
                    directRadius.Z = (movePaths[nSelected].ptCurve[i].point.Z + movePaths[nSelected + i - 1].ptCurve[j].point.Z) / 2 -
                        ptSelected.Z;
                    directRadius.Normalize();

                    //PointTranslateManipulator2D man = new PointTranslateManipulator2D();
                    //PointTranslateManipulator man_y = new PointTranslateManipulator();

                    //man_x.Direction = directRadius.ToVector3();
                    //man_x.Length = 200;
                    //man_x.Diameter = 20;
                    //man_x.Color = Color.Red;
                    //man_x.Transform = cpt.Transform;
                    //man_x.TargetTransform = cpt.Transform; 
                    //man_x.Bind(cpt);
                    //MainScene.AddNode(man_x); CHANGE to SceneNode

                    Vector3D dd = new Vector3D();
                    dd.X = movePaths[nSelected].ptCurve[i].point.X - movePaths[nSelected + i - 1].ptCurve[j].point.X;
                    dd.Y = movePaths[nSelected].ptCurve[i].point.Y - movePaths[nSelected + i - 1].ptCurve[j].point.Y;
                    dd.Z = movePaths[nSelected].ptCurve[i].point.Z - movePaths[nSelected + i - 1].ptCurve[j].point.Z;
                    directRadius = Vector3D.CrossProduct(directRadius, dd);
                    directRadius.Normalize();
                    //man_y.Direction = directRadius.ToVector3();
                    //man_y.Length = 200;
                    //man_y.Diameter = 20;
                    //man_y.Color = Color.Blue;
                    //man_y.Transform = cpt.Transform;
                    //man_y.TargetTransform = cpt.Transform;
                    //man_y.Bind(cpt);
                    //MainScene.AddNode(man_y); CHANGE to SceneNode
                }
                // Hit Bezier control point
                else
                {
                    // Add Point Manipulator
                    PointManipulator3D man = new PointManipulator3D();
                    man.Position = cpt.Position;
                    man.Target = cpt;
                    MainScene.AddNode(man.SceneNode);
                }

            }

            /// Hit Trajectory Path
            else if (modelhit is Trajectory j)
            {
                //Log($"ModelHit Trajectory.");
                ClearSelection();
                selectedVisual = SelectedVisualType.Trajectory;

                if (j.Name is "MOTIONPATH")
                    robotMotionPath.OnMouseDown(modelhit);
                else
                    targetMotionPath.OnMouseDown(modelhit);

                bPointSelected = false;
                bCurveSelected = true;
                nCurveSelected = j.number;
                lineBezier[nCurveSelected].Color = Color.Red;
                btn_Add_Arc.IsEnabled = true;
                btn_Add_Linear.IsEnabled = true;
            }

            /// Hit MeshGeometryModel3D
            else if (modelhit is MeshGeometryModel3D mesh)
            {
                Log($"ModelHit MeshGeometryModel3D: {mesh.Name} | {mesh.SceneNode.Name}");
            }

            /// Hit Item not registered
            else
            {
                Log($"ModelHit Not Registered | Type: {modelhit.GetType().Name}");
            }

        }

        private void ClearSelection()
        {
            /// Select point
            bPointSelected = false;
            nSelected = -1;

            /// Select curve
            if (bCurveSelected)
                lineBezier[nCurveSelected].Color = Color.Green;
            bCurveSelected = false;
            nCurveSelected = -1;

            btn_Add_Arc.IsEnabled = false;
            btn_Add_Linear.IsEnabled = false;

            ClearManipulators();
        }

        private void ClearManipulators()
        {
            List<SceneNode> removeNodes = new List<SceneNode>();
            foreach (var node in MainScene.GroupNode.Traverse())
            {
                var name = node.Name;
                //Log($"ForEach: {node.Name}");
                if (node.Name == "Camera Manipulator")
                    continue;
                if (node.Name == "Point Manipulator" || node.Name == "PrismRef Manipulator" || node.Name == "Target Manipulator")
                    removeNodes.Add(node);
            }
            for (int i = 0; i < removeNodes.Count; i++)
            {
                MainScene.GroupNode.RemoveChildNode(removeNodes[i]); //Remove all other manipulators
                //Log($"Cleared {i + 1} Manipulators.");
            }
        }

        private void CameraPoint_TransformChanged(object sender, TransformArgs e)
        {
            try
            {
                if (!IsSimInitialized || activeSlider != SliderName.NONE || IsCalculating || IsPlaying) return;

                if (cameraManipulator.IsCaptured)
                {
                    Vector3 pos = cameraPoint.Position - CameraPosOffset - RobotFlangeOffset;
                    OnPlanningMode_MoveCameraPoint(pos.ToPoint3D());
                }
                UpdateCameraViewport();
            }
            catch (Exception ex)
            {
                ErrorLog($"Camera Point TransformChanged Error: {ex.Message}");
            }
        }

        private void PrismRefPoint_TransformChanged(object sender, TransformArgs e)
        {
            try
            {
                if (selectedVisual == SelectedVisualType.PrismReferencePoint && prismRefManipulator.IsCaptured)
                {
                    OnPlanningMode_MovePrismRefPoint();
                    UpdateCameraViewport();
                }
            }
            catch (Exception ex)
            {
                ErrorLog($"PrismRef Point TransformChanged Error: {ex.Message}");
            }
        }

        private void TargetPoint_TransformChanged(object sender, TransformArgs e)
        {
            try
            {
                if (IsCalculating || IsPlaying) return;
                if (IsTrackingEnabled)
                {
                    cameraManipulator.PIPDirection = CalcTargetDirection();
                    Vector3 pos = cameraPoint.Position - CameraPosOffset - RobotFlangeOffset;
                    OnPlanningMode_MoveCameraPoint(pos.ToPoint3D());
                }
                else
                {
                    UpdateTargetDistance();
                }

                // Update Camera POV Target Point Position
                targetPointCameraPOV.Position = targetPoint.Position;
                UpdateCameraViewport();

                // Update Target Point Sliders
                slider_targetX.Value = targetPoint.Position.X;
                slider_targetY.Value = targetPoint.Position.Y;
                slider_targetZ.Value = targetPoint.Position.Z;
            }
            catch (Exception ex)
            {
                ErrorLog($"Target Point TransformChanged Error: {ex.Message}");
            }
        }

        private void RobotCamera_TransformChanged(object sender, TransformArgs e)
        {
            try
            {
                Log("RobotCamera_TransformChanged");
            }
            catch (Exception ex)
            {
                ErrorLog($"Target Point TransformChanged Error: {ex.Message}");
            }
        }

        private void UpdateCameraViewport()
        {
            if (CameraViewMode)
            {
                ViewportCameraPOV.Camera.Position = cameraPoint.Position.ToPoint3D();
                ViewportCameraPOV.Camera.LookDirection = cameraPoint.Direction.ToVector3D();
                //ViewportCameraPOV.LookAt(prismRefPoint.Position.ToPoint3D(), CalcPrismRefDistance(), 1);
                //TODO Fix Roll Pan Tilt - Use UpDirection???
            }
        }

        private void OnPlanningMode_MovePrismRefPoint()
        {
            try
            {
                if (!IsSimInitialized) return;
                //Log("OnPlanningMode_MovePrismRefPoint");

                double[] rotation = CalcRotation_MovePrismRef();

                slider_X.Value = cameraPoint.Position.X;
                slider_Y.Value = cameraPoint.Position.Y;
                slider_Z.Value = cameraPoint.Position.Z - RobotFlangeOffset.Z;
                slider_RX.Value = rotation[0];
                slider_RY.Value = rotation[1];
                slider_RZ.Value = rotation[2];

                double[] arrIK = RobotKinematics.InverseKinematics(cameraPoint.Position.X, cameraPoint.Position.Y, cameraPoint.Position.Z, rotation[0], rotation[1], rotation[2]);

                slider_J1.Value = arrIK[0];
                slider_J2.Value = arrIK[1];
                slider_J3.Value = arrIK[2];
                slider_J4.Value = arrIK[3];
                slider_J5.Value = arrIK[4];
                slider_J6.Value = arrIK[5];

                UpdateRoboticArm(arrIK[0], arrIK[1], arrIK[2], arrIK[3], arrIK[4], arrIK[5]);

                UpdateTargetDistance();

                FocusDistance = (float)CalculateDistance(prismRefPoint.Position, cameraPoint.Position);
                cameraManipulator.PIPDirection = CalculateDirection(prismRefPoint.Position, cameraPoint.Position);
                cameraPoint.Direction = cameraManipulator.PIPDirection;
                prismRefManipulator.PIPDirection = cameraManipulator.PIPDirection;

                List<Vector3D> directions = cameraBoundingBox.GetAxisVectors();

                cameraViewPrism.Update(
                    cameraPoint.Position,
                    FocusDistance,
                    directions[0].ToVector3(),
                    directions[1].ToVector3(),
                    directions[2].ToVector3(),
                    cameraManipulator.PIPDirection,
                    CameraMount
                ); //TODO: Merge to UpdateCameraViewPrism() ???

            }
            catch (Exception e)
            {
                ErrorLog($"OnPlanningMode MovePrismRefPoint Error: {e.Message}");
            }
        }

        private void OnPlanningMode_MoveCameraPoint(Point3D point)
        {
            if (!IsSimInitialized) return;

            double[] rotation = CalcRotation_MoveCamera();

            UpdateAll(new CartesianPos(point.X, point.Y, point.Z, rotation[0], rotation[1], rotation[2]));

        }

        private void OnPlanningMode_MoveSliderCartesian(double X, double Y, double Z, double RX, double RY, double RZ)
        {
            if (!IsSimInitialized) return;

            double[] rotation = CalcRotation_MoveCamera();

            UpdateAll(new CartesianPos(X, Y, Z, rotation[0], rotation[1], rotation[2]));

        }

        private void OnPlanningMode_MoveSliderJoints(double J1, double J2, double J3, double J4, double J5, double J6, double camOffsetX = 0, double camOffsetY = 0, double camOffsetZ = 0, double camRotAxisX = 0, double camRotAxisY = 0, double camRotAxisZ = 0)
        {
            if (!IsSimInitialized) return;

            if (IsTrackingEnabled)
            {
                double[] joints = new double[] { J1, J2, J3, J4, J5, J6 }; //TEMP
                //double[] joints = CalcRotation_MoveJoints(); //TODO
                UpdateAll(new JointPos(joints[0], joints[1], joints[2], joints[3], joints[4], joints[5]), 
                    new CartesianPos(camOffsetX, camOffsetY, camOffsetZ, camRotAxisX, camRotAxisY, camRotAxisZ));
            }
            else
            {
                UpdateAll(new JointPos(J1, J2, J3, J4, J5, J6),
                    new CartesianPos(camOffsetX, camOffsetY, camOffsetZ, camRotAxisX, camRotAxisY, camRotAxisZ));
            }
        }

        /// <summary>
        /// Update Robot Joints Position
        /// </summary>
        /// <param name="j1">J1</param>
        /// <param name="j2">J2</param>
        /// <param name="j3">J3</param>
        /// <param name="j4">J4</param>
        /// <param name="j5">J5</param>
        /// <param name="j6">J6</param>
        public void UpdateRobotJoints(double j1, double j2, double j3, double j4, double j5, double j6)
        {
            // TODO Check Limits?
            slider_J1.Value = j1;
            slider_J2.Value = j2;
            slider_J3.Value = j3;
            slider_J4.Value = j4;
            slider_J5.Value = j5;
            slider_J6.Value = j6;
            OnPlanningMode_MoveSliderJoints(j1, j2, j3, j4, j5, j6);
        }

        private void UpdateAll(CartesianPos cartPos)
        {
            double[] arrIK;

            if (!IsSyncEnabled || IsPlaying)
            {
                arrIK = RobotKinematics.InverseKinematics(cartPos.X, cartPos.Y, cartPos.Z + RobotFlangeOffset.Z, cartPos.RX, cartPos.RY, cartPos.RZ);

                slider_J1.Value = arrIK[0];
                slider_J2.Value = arrIK[1];
                slider_J3.Value = arrIK[2];
                slider_J4.Value = arrIK[3];
                slider_J5.Value = arrIK[4];
                slider_J6.Value = arrIK[5];
            }
            else //Sync Mode
            {
                arrIK = new double[] {
                    slider_J1.Value,
                    slider_J2.Value,
                    slider_J3.Value,
                    slider_J4.Value,
                    slider_J5.Value,
                    slider_J6.Value
                };
            }

            UpdateRoboticArm(arrIK[0], arrIK[1], arrIK[2], arrIK[3], arrIK[4], arrIK[5]);

            UpdateCamera(cartPos.X, cartPos.Y, cartPos.Z, cartPos.RX, cartPos.RY, cartPos.RZ);

            slider_X.Value = cartPos.X;
            slider_Y.Value = cartPos.Y;
            slider_Z.Value = cartPos.Z;
            slider_RX.Value = cartPos.RX;
            slider_RY.Value = cartPos.RY;
            slider_RZ.Value = cartPos.RZ;
        }

        private void UpdateAll(JointPos jointPos, CartesianPos camConfig = null)
        {
            double[] arrFK;

            if (!IsSyncEnabled || IsPlaying)
            {
                arrFK = RobotKinematics.ForwardKinematics(jointPos.J1, jointPos.J2, jointPos.J3, jointPos.J4, jointPos.J5, jointPos.J6);
                arrFK[2] -= RobotFlangeOffset.Z; //FIX for Staubli z offset problem //TODO Fix Kinematic Output
                slider_X.Value = arrFK[0];
                slider_Y.Value = arrFK[1];
                slider_Z.Value = arrFK[2];
                slider_RX.Value = arrFK[3];
                slider_RY.Value = arrFK[4];
                slider_RZ.Value = arrFK[5];
            }
            else // Sync Mode
            {
                arrFK = new double[] {
                    slider_X.Value,
                    slider_Y.Value,
                    slider_Z.Value,
                    slider_RX.Value,
                    slider_RY.Value,
                    slider_RZ.Value
                };
            }

            double camOffsetX = camConfig != null ? camConfig.X : 0;
            double camOffsetY = camConfig != null ? camConfig.Y : 0;
            double camOffsetZ = camConfig != null ? camConfig.Z : 0;
            double camRotAxisX = camConfig != null ? camConfig.RX : 0;
            double camRotAxisY = camConfig != null ? camConfig.RY : 0;
            double camRotAxisZ = camConfig != null ? camConfig.RZ : 0;
            UpdateRoboticArm(jointPos.J1, jointPos.J2, jointPos.J3, jointPos.J4, jointPos.J5, jointPos.J6, 
                camOffsetX, camOffsetY, camOffsetZ, 
                camRotAxisX, camRotAxisY, camRotAxisZ);
            UpdateCamera(arrFK[0] - camOffsetX, arrFK[1] + camOffsetY, arrFK[2] - camOffsetZ, arrFK[3], arrFK[4], arrFK[5]);


            slider_J1.Value = jointPos.J1;
            slider_J2.Value = jointPos.J2;
            slider_J3.Value = jointPos.J3;
            slider_J4.Value = jointPos.J4;
            slider_J5.Value = jointPos.J5;
            slider_J6.Value = jointPos.J6;
        }

        // TODO FIX Bounding Box and Prism
        private void UpdateAll_Sync(JointPos jointPos, CartesianPos cartPos)
        {
            slider_J1.Value = jointPos.J1;
            slider_J2.Value = jointPos.J2;
            slider_J3.Value = jointPos.J3;
            slider_J4.Value = jointPos.J4;
            slider_J5.Value = jointPos.J5;
            slider_J6.Value = jointPos.J6;

            slider_X.Value = cartPos.X;
            slider_Y.Value = cartPos.Y;
            slider_Z.Value = cartPos.Z;
            slider_RX.Value = cartPos.RX;
            slider_RY.Value = cartPos.RY;
            slider_RZ.Value = cartPos.RZ;

            UpdateRoboticArm(jointPos.J1, jointPos.J2, jointPos.J3, jointPos.J4, jointPos.J5, jointPos.J6);

            UpdateCamera(cartPos.X, cartPos.Y, cartPos.Z, cartPos.RX, cartPos.RY, cartPos.RZ);
        }

        private void UpdateCamera(double X, double Y, double Z, double RX, double RY, double RZ)
        {
            if (IsSyncEnabled || IsHoming || IsCalculating || IsPlaying || IsNavigating || activeSlider != SliderName.NONE ||
                (!cameraManipulator.IsCaptured && !targetPoint.IsPositionChanged && !prismRefPoint.IsPositionChanged && !prismRefManipulator.IsCaptured))
            {
                //TODO Update RoboticArm pose based on Flange position = camerapos - CameraPosOffset
                UpdateCameraPosition(X, Y, Z, RX, RY, RZ);
            }

            //cameraManipulator.PIPDirection = CalculateDirection(prismRefPoint.Position, cameraPoint.Position);

            // Update Camera PIP Direction
            if (IsTrackingEnabled)
            {
                var targetDirection = CalcTargetDirection();
                cameraPoint.Direction = targetDirection;
                cameraManipulator.PIPDirection = cameraPoint.Direction;
                prismRefManipulator.PIPDirection = cameraManipulator.PIPDirection;
            }
            else
            {
                List<Vector3D> directions = cameraBoundingBox.GetAxisVectors();
                if (CameraMount == ToolPlacement.BOTTOM)
                {
                    cameraPoint.Direction = -directions[0].ToVector3();
                    cameraManipulator.PIPDirection = cameraPoint.Direction;
                    prismRefManipulator.PIPDirection = cameraManipulator.PIPDirection;
                }
                else if (CameraMount == ToolPlacement.FRONT)
                {
                    cameraPoint.Direction = directions[2].ToVector3();
                    cameraManipulator.PIPDirection = cameraPoint.Direction;
                    prismRefManipulator.PIPDirection = cameraManipulator.PIPDirection;
                }
                else
                {

                }
            }
            ////////////

            UpdateTargetDistance();

            if (!prismRefManipulator.IsCaptured)
            {
                UpdateCameraViewPrism();
            }
        }

        public (JointPos jointPos, bool result) CalculateCameraLookAtTargetPosition()
        {
            if (!IsSimInitialized) return (new JointPos(0, 0, 0, 0, 0, 0), false);
            Vector3 pos = cameraPoint.Position - CameraPosOffset - RobotFlangeOffset;
            //Log($"CalculateCameraLookAtTarget: Camera Pos X={pos.X:F2} Y={pos.Y:F2} Z={pos.Z:F2}");
            IsCalculatingTracking = true;
            double[] rotation = CalcRotation_MoveCamera();
            Log($"CalculateCameraLookAtTarget: Rotated Camera Pos X={pos.X:F2} Y={pos.Y:F2} Z={pos.Z + RobotFlangeOffset.Z:F2} RX={rotation[0]:F2} RY={rotation[1]:F2} RZ={rotation[2]:F2}");
            double[] arrIK = RobotKinematics.InverseKinematics(pos.X, pos.Y, pos.Z + RobotFlangeOffset.Z, rotation[0], rotation[1], rotation[2]);
            Log($"CalculateCameraLookAtTarget: Rotated Camera Joint Pos J1={arrIK[0]:F2} J2={arrIK[1]:F2} J3={arrIK[2]:F2} J4={arrIK[3]:F2} J5={arrIK[4]:F2} J6={arrIK[5]:F2}");
            IsCalculatingTracking = false;
            return (new JointPos(arrIK[0], arrIK[1], arrIK[2], arrIK[3], arrIK[4], arrIK[5]), true);
        }

        /// <summary>
        /// Calculate Camera Look At Target Position (Optimized for streaming).
        /// New camera X,Y,Z values should come from robot feedback (with Camera Offset and Staubli Offset)
        /// </summary>
        /// <param name="posX">New camera X position</param>
        /// <param name="posY">New camera Y position</param>
        /// <param name="posZ">New camera Z position</param>
        /// <returns>JointPos value and Result</returns>
        public (JointPos jointPos, bool result) CalculateCameraLookAtTargetPosition_Move(double posX, double posY, double posZ)
        {
            Vector3 camPos = new Vector3((float)posX, (float)posY, (float)posZ + RobotFlangeOffset.Z);
            if (!IsSimInitialized) return (new JointPos(0, 0, 0, 0, 0, 0), false);
            //Vector3 pos = cameraPoint.Position - CameraPosOffset - StaubliOffset;
            //System.Diagnostics.Debug.WriteLine($"Camera Position X [sim={pos.X:F2} | new={posX}], Y [sim={pos.Y:F2} | new={posY}], Z [sim={pos.Z:F2} | new={posZ}]");
            IsCalculatingTracking = true;
            double[] rotation = CalcRotation_MoveCamera(camPos);
            //System.Diagnostics.Debug.WriteLine($"Rotated Camera Cart Pos X={posX:F2} Y={posY:F2} Z={posZ + StaubliOffset.Z:F2} RX={rotation[0]:F2} RY={rotation[1]:F2} RZ={rotation[2]:F2}");
            double[] arrIK = RobotKinematics.InverseKinematics(posX, posY, posZ + RobotFlangeOffset.Z, rotation[0], rotation[1], rotation[2]);
            //System.Diagnostics.Debug.WriteLine($"Rotated Camera Joint Pos J1={arrIK[0]:F2} J2={arrIK[1]:F2} J3={arrIK[2]:F2} J4={arrIK[3]:F2} J5={arrIK[4]:F2} J6={arrIK[5]:F2}");
            IsCalculatingTracking = false;
            return (new JointPos(arrIK[0], arrIK[1], arrIK[2], arrIK[3], arrIK[4], arrIK[5]), true);
        }

        /// <summary>
        /// Update Prism point & geometry
        /// </summary>
        /// <param name="focusDistance">Updated focus distance value</param>
        public void UpdateFocusDistance(float focusDistance)
        {
            if (IsTrackingEnabled || IsCalculating || IsPlaying || prismRefManipulator.IsCaptured)
                return;

            FocusDistance = focusDistance;
            UpdateCameraViewPrism();
        }

        public void UpdateCameraPosition(double X, double Y, double Z, double RX, double RY, double RZ)
        {
            Vector3 position = new Vector3(
                (float)X,
                (float)Y,
                (float)Z + RobotFlangeOffset.Z // Staubli system Z offset fix
            );

            /// Sync values from robot arm already have Camera Offset
            if (!IsSyncEnabled)
            {
                /// ##### TODO Use Vector3 offset_origin = robotFlangePoint.Position; 
                position = position + CameraPosOffset;
            }

            tb_OffsetGlobalX.Text = position.X.ToString();
            tb_OffsetGlobalY.Text = position.Y.ToString();
            tb_OffsetGlobalZ.Text = position.Z.ToString();

            cameraPoint.Position = position;
            cameraManipulator.Position = position;
            cameraManipulator.LookAtTarget = prismRefPoint.Position;
            Console.WriteLine("Camera Manipulation : {0}, {1}, {2}", position.X, position.Y, position.Z);
        }

        private void UpdateTotalLength()
        {
            double tempTot = 0;
            for (int i = 0; i < movePaths.Count; i++)
                tempTot += movePaths[i].lenCurve;
            TotalLength = tempTot;
        }

        private void UpdateTargetDistance()
        {
            TargetDistance = (float)CalcTargetDistance();
        }

        public void UpdateTrackingLine(Vector3 toolPos)
        {
            //trackLine.Points[0] = targetPoint.Position;
            //trackLine.Points[1] = toolPos.ToPoint3D();
        }

        public void UpdateCameraViewPrism()
        {
            Vector3 ptCenter = cameraPoint.Position;

            // Update Prism Reference Point
            if (IsTrackingEnabled)
            {
                prismRefPoint.Position = targetPoint.Position;
                prismRefManipulator.Position = targetPoint.Position;
                prismRefManipulator.LookAtTarget = targetPoint.Position;
                prismRefManipulator.PIPDirection = cameraManipulator.PIPDirection;
                FocusDistance = (float)CalcPrismRefDistance();
            }
            else
            {
                Vector3 pos = ptCenter + FocusDistance * cameraManipulator.PIPDirection;
                prismRefPoint.Position = pos;
                prismRefManipulator.Position = pos;
                prismRefManipulator.LookAtTarget = pos;
                prismRefManipulator.PIPDirection = cameraManipulator.PIPDirection;
            }

            List<Vector3D> axisVectors = cameraBoundingBox.GetAxisVectors();

            // Update Camera View Prism
            cameraViewPrism.Update(
                ptCenter,
                FocusDistance,
                axisVectors[0].ToVector3(), // X axis
                axisVectors[1].ToVector3(), // Y axis
                axisVectors[2].ToVector3(), // Z axis
                cameraManipulator.PIPDirection,
                CameraMount
            );
        }

        private void UpdateCameraOffset()
        {
            if (CameraMount == ToolPlacement.BOTTOM)
                CameraOffset = new Vector3(Settings.Default.CAMERA_BOTTOM_OFFSET_X, Settings.Default.CAMERA_BOTTOM_OFFSET_Y, Settings.Default.CAMERA_BOTTOM_OFFSET_Z);
            else if (CameraMount == ToolPlacement.FRONT)
                CameraOffset = new Vector3(Settings.Default.CAMERA_FRONT_OFFSET_X, Settings.Default.CAMERA_FRONT_OFFSET_Y, Settings.Default.CAMERA_FRONT_OFFSET_Z);
        }

        public void SetCameraMountBottom()
        {
            CameraMount = ToolPlacement.BOTTOM;
        }

        public void SetCameraMountFront()
        {
            CameraMount = ToolPlacement.FRONT;
        }

        public void SetTargetPoint_Position(Vector3 position)
        {
            IsTargetPointUpdateLock = true;

            targetPoint.Transform = new TranslateTransform3D(position.ToVector3D());
            targetPoint.Position = position;

            // Update Target Point Sliders
            slider_targetX.Value = targetPoint.Position.X;
            slider_targetY.Value = targetPoint.Position.Y;
            slider_targetZ.Value = targetPoint.Position.Z;

            IsTargetPointUpdateLock = false;
        }

        public void SetTargetPoint_ToPrismRefPosition()
        {
            SetTargetPoint_Position(prismRefPoint.Position);
        }

        private double[] CalcRotation_MovePrismRef()
        {
            Vector3 direction = CalcPrismRefDirection();
            double rotX = 0;
            double rotY = (Math.Acos(direction.Z) - Math.PI / 2) * 180 / Math.PI;
            double rotZ = Math.Atan(direction.Y / direction.X) * 180 / Math.PI;
            double distPrism = CalcPrismRefDistance();

            switch (CameraMount)
            {
                case ToolPlacement.BOTTOM:
                    //double delRotY = Math.Asin(CameraOffset.Z / distPrism); //ORIGINAL when using OffsetFromFlange
                    double delRotY = Math.Asin(0 / distPrism);
                    rotY -= delRotY * 180 / Math.PI;
                    break;
                case ToolPlacement.FRONT:
                    rotY -= 90;
                    break;
                default:
                    break;
            }

            return new double[] { rotX, rotY, rotZ };
        }

        private double[] CalcRotation_MoveCamera()
        {
            double rotX, rotY, rotZ;

            if (IsTrackingEnabled || IsCalculatingTracking)
            {
                Vector3 direction = CalcTargetDirection();
                rotX = 180;
                rotY = (Math.Acos(direction.Z)) * 180 / Math.PI;
                rotZ = Math.Atan(direction.Y / direction.X) * 180 / Math.PI;

                switch (CameraMount)
                {
                    case ToolPlacement.BOTTOM:
                        rotX = 0;
                        rotY -= 90;
                        break;
                    case ToolPlacement.FRONT:
                        UpdateTargetDistance();
                        double delRotY = Math.Asin(CameraOffset.Z / TargetDistance);
                        rotY -= delRotY * 180 / Math.PI;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                rotX = slider_RX.Value;
                rotY = slider_RY.Value;
                rotZ = slider_RZ.Value;
            }

            return new double[] { rotX, rotY, rotZ };
        }

        private double[] CalcRotation_MoveCamera(Vector3 cameraPos)
        {
            double rotX, rotY, rotZ;

            Vector3 direction = CalculateDirection(targetPoint.Position, cameraPos);
            rotX = 180;
            rotY = (Math.Acos(direction.Z)) * 180 / Math.PI;
            rotZ = Math.Atan(direction.Y / direction.X) * 180 / Math.PI;

            switch (CameraMount)
            {
                case ToolPlacement.BOTTOM:
                    rotX = 0;
                    rotY -= 90;
                    break;
                case ToolPlacement.FRONT:
                    break;
                default:
                    break;
            }

            return new double[] { rotX, rotY, rotZ };
        }

        private double CalculateDistance(Vector3 pA, Vector3 pB)
        {
            //return (pA - pB).Length();
            return CustomSpline.GetDistance(pA.ToPoint3D(), pB.ToPoint3D());
        }

        private Vector3 CalculateDirection(Vector3 dest, Vector3 src)
        {
            // Calculate direction between Source point and Target point
            Vector3 direction = dest - src;
            direction.Normalize();
            return direction;
        }

        private Vector3 CalcTargetDirection()
        {
            // Calculate direction between Camera point and Target point
            //Vector3 direction = targetPoint.Position - cameraPoint.Position;
            //direction.Normalize();
            return CalculateDirection(targetPoint.Position, cameraPoint.Position);
        }

        private double CalcTargetDistance()
        {
            return CalculateDistance(targetPoint.Position, cameraPoint.Position);
        }

        private Vector3 CalcPrismRefDirection()
        {
            // Calculate direction between Camera point and Prism Reference point
            return CalculateDirection(prismRefPoint.Position, cameraPoint.Position);
        }

        private double CalcPrismRefDistance()
        {
            //return (prismRefPoint.Position - cameraPoint.Position).Length();
            return CalculateDistance(prismRefPoint.Position, cameraPoint.Position);
        }

        private void ReplaceCamera(ToolPlacement camPlacement)
        {
            // Replace Robotic Arm 
            MainScene.RemoveNode(roboticArm);
            MainScene.RemoveNode(robotFlangePoint);
            MainScene.RemoveNode(cameraBoundingBox.SceneNode);
            Initialize_RoboticArm(ref RobotJoints);
            UpdateCameraOffset();
            GoHomePosition();
            OnPlanningMode_MovePrismRefPoint(); // Refresh
        }

        //////////////////////////////////////////////////////




        private string OpenFileDialog(string filter)
        {
            var d = new OpenFileDialog();
            d.CustomPlaces.Clear();

            d.Filter = filter;

            if (!d.ShowDialog().Value)
            {
                return null;
            }

            return d.FileName;
        }

        private HelixToolkitScene MainScene_LoadedModel_Scene { get; set; }
        private HelixToolkitScene CameraPOV_LoadedModel_Scene { get; set; }
        public SceneNodeGroupModel3D MainScene_LoadedModel_GroupModel { get; } = new SceneNodeGroupModel3D();
        public SceneNodeGroupModel3D CameraPOV_LoadedModel_GroupModel { get; } = new SceneNodeGroupModel3D();

        private TransformManipulator3D modelManipulator = new TransformManipulator3D();

        private bool isModelLoading = false;
        public bool IsModelLoading
        {
            private set { isModelLoading = value; }
            get => isModelLoading;
        }

        public void ModelLoadFile(string filepath, bool cameraViewflag = false)
        {
            // Check if have filepath, else openFileDialog
            try
            {
                string OpenFileFilter = $"{HelixToolkit.Wpf.SharpDX.Assimp.Importer.SupportedFormatsString}";

                if (isModelLoading)
                {
                    return;
                }

                string path = OpenFileDialog(OpenFileFilter);
                if (path == null)
                {
                    return;
                }

                //StopAnimation();

                IsModelLoading = true;

                // ========= Main Scene Model Loading
                Task.Run(() =>
                {
                    var loader = new Importer();
                    return loader.Load(path);
                }
                ).ContinueWith((result) =>
                {
                    IsModelLoading = false;
                    if (result.IsCompleted)
                    {
                        MainScene_LoadedModel_Scene = result.Result;
                        //Animations.Clear();
                        MainScene_LoadedModel_GroupModel.Clear();
                        if (MainScene_LoadedModel_Scene != null)
                        {
                            if (MainScene_LoadedModel_Scene.Root != null)
                            {
                                foreach (var node in MainScene_LoadedModel_Scene.Root.Traverse())
                                {
                                    if (node is MaterialGeometryNode m)
                                    {
                                        //m.Geometry.SetAsTransient();
                                        if (m.Material is PBRMaterialCore pbr)
                                        {
                                            pbr.RenderEnvironmentMap = RenderEnvironmentMap;
                                        }
                                        else if (m.Material is PhongMaterialCore phong)
                                        {
                                            phong.RenderEnvironmentMap = RenderEnvironmentMap;
                                        }
                                    }
                                }
                            }

                            MainScene_LoadedModel_GroupModel.AddNode(MainScene_LoadedModel_Scene.Root);
                            MainScene_LoadedModel_GroupModel.SceneNode.Name = "Loaded Model";

                            //if (ModelScene.HasAnimation)
                            //{
                            //    var dict = ModelScene.Animations.CreateAnimationUpdaters();
                            //    foreach (var ani in dict.Values)
                            //    {
                            //        Animations.Add(ani);
                            //    }
                            //}

                            foreach (var n in MainScene_LoadedModel_Scene.Root.Traverse())
                            {
                                n.Tag = new AttachedNodeViewModel(n);
                            }

                            MainScene_LoadedModel_GroupModel.Transform = new TranslateTransform3D(targetPoint.Position.ToVector3D());

                            //SharedContainer.Items.Add(LoadModel_GroupModel.SceneNode); TODO ?
                            MainScene.AddNode(MainScene_LoadedModel_GroupModel.SceneNode);

                            modelManipulator.SizeScale = 2;
                            modelManipulator.EnableScaling = false;
                            modelManipulator.EnableXRayGrid = false;
                            //modelManipulator.CenterOffset = MainScene_LoadedModel_GroupModel.SceneNode.Bounds.Center; //targetPoint.Position;
                            //modelManipulator.Target = MainScene_LoadedModel_GroupModel;
                            MainScene.AddNode(modelManipulator.SceneNode);
                        }
                    }
                    else if (result.IsFaulted && result.Exception != null)
                    {
                        ErrorLog(result.Exception.Message);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
                // =========

                // ========= Camera POV Scene Model Loading (Copy of above)
                Task.Run(() =>
                {
                    var loader = new Importer();
                    return loader.Load(path);
                }
                ).ContinueWith((result) =>
                {
                    IsModelLoading = false;
                    if (result.IsCompleted)
                    {
                        CameraPOV_LoadedModel_Scene = result.Result;
                        //Animations.Clear();
                        CameraPOV_LoadedModel_GroupModel.Clear();
                        if (CameraPOV_LoadedModel_Scene != null)
                        {
                            if (CameraPOV_LoadedModel_Scene.Root != null)
                            {
                                foreach (var node in CameraPOV_LoadedModel_Scene.Root.Traverse())
                                {
                                    if (node is MaterialGeometryNode m)
                                    {
                                        //m.Geometry.SetAsTransient();
                                        if (m.Material is PBRMaterialCore pbr)
                                        {
                                            pbr.RenderEnvironmentMap = RenderEnvironmentMap;
                                        }
                                        else if (m.Material is PhongMaterialCore phong)
                                        {
                                            phong.RenderEnvironmentMap = RenderEnvironmentMap;
                                        }
                                    }
                                }
                            }

                            CameraPOV_LoadedModel_GroupModel.AddNode(CameraPOV_LoadedModel_Scene.Root);
                            CameraPOV_LoadedModel_GroupModel.SceneNode.Name = "Loaded Model";

                            //if (ModelScene.HasAnimation)
                            //{
                            //    var dict = ModelScene.Animations.CreateAnimationUpdaters();
                            //    foreach (var ani in dict.Values)
                            //    {
                            //        Animations.Add(ani);
                            //    }
                            //}

                            foreach (var n in CameraPOV_LoadedModel_Scene.Root.Traverse())
                            {
                                n.Tag = new AttachedNodeViewModel(n);
                            }

                            CameraPOV_LoadedModel_GroupModel.Transform = new TranslateTransform3D(targetPointCameraPOV.Position.ToVector3D());
                            CameraScene.AddNode(CameraPOV_LoadedModel_GroupModel.SceneNode);
                        }
                    }
                    else if (result.IsFaulted && result.Exception != null)
                    {
                        ErrorLog(result.Exception.Message);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
                // =========

            }
            catch (Exception ex)
            {
                ErrorLog($"Load 3D model file error: {ex.StackTrace}");
            }
        }

        private void ModelMove_TargetPoint()
        {
            MainScene_LoadedModel_GroupModel.Transform = new TranslateTransform3D(targetPoint.Position.ToVector3D());
            CameraPOV_LoadedModel_GroupModel.Transform = new TranslateTransform3D(targetPointCameraPOV.Position.ToVector3D());
        }

        private void ModelMove_RotateZ()
        {
            MainScene_LoadedModel_GroupModel.Transform = new RotateTransform3D((new AxisAngleRotation3D(new Vector3D(0, 0, 1), 90)), targetPoint.Position.ToPoint3D());
            CameraPOV_LoadedModel_GroupModel.Transform = new RotateTransform3D((new AxisAngleRotation3D(new Vector3D(0, 0, 1), 90)), targetPointCameraPOV.Position.ToPoint3D());
        }

        private void Reset()
        {
            removeVisual();
            lineBezier.Clear();
            movePaths.Clear();
            jointArray.Clear();
            trajectory.Clear();
        }

        private void removeVisual()
        {
            if (movePaths.Count == 0)
                return;
            else if (movePaths.Count == 1 && movePaths[0].ptCurve.Count == 1)
            {
                MainScene.RemoveNode(movePaths[0].ptCurve[0]);
                MainScene.RemoveNode(movePaths[0].lblVisual3D[0].SceneNode);
                return;
            }

            for (int i = 0; i < movePaths.Count; i++)
            {
                MainScene.RemoveNode(lineBezier[i].SceneNode);
                MainScene.RemoveNode(movePaths[i].ptCurve[0]);
                MainScene.RemoveNode(movePaths[i].lblVisual3D[0].SceneNode);
                if (i == 0)
                {
                    MainScene.RemoveNode(movePaths[i].ptCurve[1]);
                    MainScene.RemoveNode(movePaths[i].lblVisual3D[1].SceneNode);
                }
                if (movePaths[i].curveShape == CurveType.BEZIER)
                {
                    MainScene.RemoveNode(movePaths[i].ptAdjust[0]);
                    MainScene.RemoveNode(movePaths[i].ptAdjust[1]);
                    MainScene.RemoveNode(movePaths[i].ctrlLine[0].SceneNode);
                    MainScene.RemoveNode(movePaths[i].ctrlLine[1].SceneNode);
                }
                else if (movePaths[i].curveShape == CurveType.ARC)
                {
                    MainScene.RemoveNode(movePaths[i].ptAdjust[0]);
                }
            }
        }

        private void addVisual()
        {
            for (int i = trajOldTotalCount; i < movePaths.Count; i++)
            {
                MainScene.AddNode(movePaths[i].ptCurve[0]);
                MainScene.AddNode(movePaths[i].lblVisual3D[0].SceneNode);
                if (i == 0)
                {
                    MainScene.AddNode(movePaths[i].ptCurve[1]);
                    MainScene.AddNode(movePaths[i].lblVisual3D[1].SceneNode);
                }
                if (trajOldTotalCount > 0 && i == trajOldTotalCount)
                    continue;
                if (IsControlPointsEnabled)
                {
                    if (movePaths[i].curveShape == CurveType.BEZIER)
                    {
                        MainScene.AddNode(movePaths[i].ptAdjust[0]);
                        MainScene.AddNode(movePaths[i].ptAdjust[1]);
                    }
                    else if (movePaths[i].curveShape == CurveType.ARC)
                    {
                        MainScene.AddNode(movePaths[i].ptAdjust[0]);
                    }
                }
            }
        }

        private void deleteAndUpdate()
        {
            CustomCurve curve = new CustomCurve();
            BezierMarkPoint3D tempPt = new BezierMarkPoint3D();

            if (robotMotionPath.CurveCount == 1)
            {
                if (movePaths[0].ptCurve.Count == 1) Reset();
                else
                {
                    MainScene.RemoveNode(lineBezier[0].SceneNode);
                    if (movePaths[0].curveShape == CurveType.BEZIER)
                    {
                        MainScene.RemoveNode(movePaths[0].ctrlLine[0].SceneNode);
                        MainScene.RemoveNode(movePaths[0].ctrlLine[1].SceneNode);
                        MainScene.RemoveNode(movePaths[0].ptAdjust[0]);
                        MainScene.RemoveNode(movePaths[0].ptAdjust[1]);
                    }
                    else if (movePaths[0].curveShape == CurveType.ARC)
                    {
                        MainScene.RemoveNode(movePaths[0].ptAdjust[0]);
                    }
                    MainScene.RemoveNode(movePaths[0].ptCurve[nPoint]);
                    MainScene.RemoveNode(movePaths[0].lblVisual3D[nPoint].SceneNode);

                    curve.curveShape = CurveType.BEZIER;
                    curve.ptCurve = new List<BezierMarkPoint3D>();
                    curve.ptCurve.Add(movePaths[0].ptCurve[1 - nPoint]);
                    curve.lblVisual3D = new List<BillboardText>();
                    curve.lblVisual3D.Add(movePaths[0].lblVisual3D[1 - nPoint]);
                    movePaths[0] = curve;

                    double[] temp = jointArray[1 - nPoint];
                    jointArray.Clear();
                    jointArray.Add(temp);

                    lineBezier.Clear();
                }
                renameLbl();
                return;
            }


            for (int i = 0; i < 2; i++)
            {
                MainScene.RemoveNode(lineBezier[nSelected + i].SceneNode);
                if (movePaths[nSelected + i].curveShape == CurveType.BEZIER)
                {
                    MainScene.RemoveNode(movePaths[nSelected + i].ctrlLine[0].SceneNode);
                    MainScene.RemoveNode(movePaths[nSelected + i].ctrlLine[1].SceneNode);
                    MainScene.RemoveNode(movePaths[nSelected + i].ptAdjust[0]);
                    MainScene.RemoveNode(movePaths[nSelected + i].ptAdjust[1]);
                }
                else if (movePaths[nSelected + i].curveShape == CurveType.ARC)
                {
                    MainScene.RemoveNode(movePaths[nSelected + i].ptAdjust[0]);
                }

                if (nSelected + 1 == robotMotionPath.CurveCount || (nSelected == 0 && nPoint == 0))
                    break;
            }

            if (nSelected == 0 && nPoint == 0)
            {
                tempPt = movePaths[nSelected].ptCurve[1 - nPoint];
                var textVis3d = movePaths[nSelected].lblVisual3D[1];
                textVis3d.Text = "1";

                MainScene.RemoveNode(movePaths[nSelected].ptCurve[0]);
                MainScene.RemoveNode(movePaths[nSelected].ptCurve[1]);
                MainScene.RemoveNode(movePaths[nSelected].lblVisual3D[0].SceneNode);

                if (nPoint == 0)
                {
                    curve = movePaths[nSelected + 1];
                    curve.ptCurve.Add(tempPt);
                    tempPt = curve.ptCurve[0];
                    curve.ptCurve[0] = curve.ptCurve[1];
                    curve.ptCurve[1] = tempPt;

                    curve.lblVisual3D.Add(textVis3d);
                    textVis3d = curve.lblVisual3D[0];
                    curve.lblVisual3D[0] = curve.lblVisual3D[1];
                    curve.lblVisual3D[1] = textVis3d;

                    MainScene.AddNode(curve.ptCurve[0]);
                    movePaths[nSelected + 1] = curve;

                    for (int i = 0; i < robotMotionPath.CurveCount - 1; i++)
                    {
                        movePaths[i] = movePaths[i + 1];
                        movePaths[i].ptAdjust[0].curveNum = i;
                        movePaths[i].ptAdjust[1].curveNum = i;
                        movePaths[i].ptCurve[0].curveNum = i;
                        if (i == 0)
                            movePaths[i].ptCurve[1].curveNum = i;
                        lineBezier[i] = lineBezier[i + 1];
                        lineBezier[i].number = i;
                    }

                    for (int i = 0; i < jointArray.Count - 1; i++)
                        jointArray[i] = jointArray[i + 1];
                }
            }
            else if (nSelected == robotMotionPath.CurveCount - 1)
            {
                MainScene.RemoveNode(movePaths[nSelected].ptCurve[0]);
                MainScene.RemoveNode(movePaths[nSelected].lblVisual3D[0].SceneNode);
            }
            else
            {
                int k = 0;
                if (nSelected == 0)
                    k = 1;
                MainScene.RemoveNode(movePaths[nSelected].ptCurve[k]);
                MainScene.RemoveNode(movePaths[nSelected].lblVisual3D[k].SceneNode);
                curve = movePaths[nSelected];
                curve.ptCurve[k] = movePaths[nSelected + 1].ptCurve[0];
                curve.lblVisual3D[k] = movePaths[nSelected + 1].lblVisual3D[0];

                movePaths[nSelected] = curve;
                movePaths[nSelected].ptCurve[0].curveNum = nSelected;
                robotMotionPath.MakeBezier(nSelected, true);
                for (int i = nSelected; i < robotMotionPath.CurveCount - 1; i++)
                {
                    if (i != nSelected)
                    {
                        movePaths[i] = movePaths[i + 1];
                        lineBezier[i] = lineBezier[i + 1];
                    }
                    movePaths[i].ptAdjust[0].curveNum = i;
                    movePaths[i].ptAdjust[1].curveNum = i;
                    movePaths[i].ptCurve[0].curveNum = i;
                    if (i == 0)
                        movePaths[i].ptCurve[1].curveNum = i;
                    lineBezier[i].number = i;
                }
                for (int i = nSelected + 1; i < jointArray.Count - 1; i++)
                    jointArray[i] = jointArray[i + 1];
            }

            renameLbl();
            jointArray.RemoveAt(jointArray.Count - 1);
            lineBezier.RemoveAt(robotMotionPath.CurveCount - 1);
            movePaths.RemoveAt(robotMotionPath.CurveCount - 1);
        }

        private void renameLbl()
        {
            if (movePaths.Count == 0) return;

            movePaths[0].lblVisual3D[0].Text = "1";

            if (movePaths.Count == 1 && movePaths[0].ptCurve.Count == 1) return;

            for (int i = 0; i < robotMotionPath.CurveCount; i++)
            {
                int jj = 0;
                if (i == 0)
                {
                    jj = 1;
                }
                movePaths[i].lblVisual3D[jj].Text = string.Format("{0}", i + 2);
            }
        }

        public void Sync_Update(JointPos jointPos, CartesianPos cartPos)
        {
            tb_robotPos_J1.Text = jointPos.J1.ToString("F");
            tb_robotPos_J2.Text = jointPos.J2.ToString("F");
            tb_robotPos_J3.Text = jointPos.J3.ToString("F");
            tb_robotPos_J4.Text = jointPos.J4.ToString("F");
            tb_robotPos_J5.Text = jointPos.J5.ToString("F");
            tb_robotPos_J6.Text = jointPos.J6.ToString("F");
            tb_robotPos_X.Text = cartPos.X.ToString("F");
            tb_robotPos_Y.Text = cartPos.Y.ToString("F");
            tb_robotPos_Z.Text = cartPos.Z.ToString("F");
            tb_robotPos_RX.Text = cartPos.RX.ToString("F");
            tb_robotPos_RY.Text = cartPos.RY.ToString("F");
            tb_robotPos_RZ.Text = cartPos.RZ.ToString("F");

            if (IsSyncEnabled)
            {
                UpdateAll_Sync(jointPos, cartPos);
            }
        }

        //////////////////////////////////////////////////////

        private void ChangeModelColor(SceneNode sceneNode, Color newColor)
        {
            try
            {
                //Log("Model Material: " + pModel.Material.GetType());
                foreach (var node in sceneNode.Traverse())
                {
                    if (node is MaterialGeometryNode m)
                    {
                        if (m.Material is PBRMaterialCore material)
                        {
                            material.RenderEnvironmentMap = RenderEnvironmentMap;
                        }
                        m.Material = new PhongMaterial()
                        {
                            DiffuseColor = newColor
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Change Color Exception Error: " + ex.StackTrace);
            }
        }

        private void HighlightSelectionFunc(SceneNodeGroupModel3D groupmodel, bool flag)
        {
            foreach (var node in groupmodel.GroupNode.Items.PreorderDFT((node) => { return node.IsRenderable; }))
            {
                if (node is MeshNode m)
                {
                    m.WireframeColor = Color.GreenYellow;
                    if (m.Tag is AttachedNodeViewModel a)
                    {
                        a.HighlightEnable = flag;
                    }
                }
            }
        }

        private void ShowWireframeFunc(SceneNodeGroupModel3D groupmodel, bool show)
        {
            foreach (var node in groupmodel.GroupNode.Items.PreorderDFT((node) => { return node.IsRenderable; }))
            {
                if (node is MeshNode m)
                {
                    m.WireframeColor = Color.GreenYellow;
                    m.RenderWireframe = show;
                }
            }
        }

        private void RenderFlatFunc(SceneNodeGroupModel3D groupmodel, bool show)
        {
            foreach (var node in groupmodel.GroupNode.Items.PreorderDFT((node) => { return node.IsRenderable; }))
            {
                if (node is MeshNode m)
                {
                    if (m.Material is PhongMaterialCore phong)
                    {
                        phong.EnableFlatShading = show;
                    }
                    else if (m.Material is PBRMaterialCore pbr)
                    {
                        pbr.EnableFlatShading = show;
                    }
                }
            }
        }

        public void ShowControlPoints()
        {
            for (int i = 0; i < movePaths.Count; i++)
            {
                if (movePaths[i].curveShape == CurveType.BEZIER)
                    for (int j = 0; j < 2; j++)
                    {
                        if (!MainScene.GroupNode.Items.Contains(movePaths[i].ctrlLine[j].SceneNode))
                            MainScene.AddNode(movePaths[i].ctrlLine[j].SceneNode);
                        if (!MainScene.GroupNode.Items.Contains(movePaths[i].ptAdjust[j]))
                            MainScene.AddNode(movePaths[i].ptAdjust[j]);
                    }
                else if (movePaths[i].curveShape == CurveType.ARC && !MainScene.GroupNode.Items.Contains(movePaths[i].ptAdjust[0]))
                    MainScene.AddNode(movePaths[i].ptAdjust[0]);
            }
        }

        public void HideControlPoints()
        {
            for (int i = 0; i < movePaths.Count; i++)
            {
                if (movePaths[i].curveShape == CurveType.BEZIER)
                    for (int j = 0; j < 2; j++)
                    {
                        if (MainScene.GroupNode.Items.Contains(movePaths[i].ctrlLine[j].SceneNode))
                            MainScene.GroupNode.RemoveChildNode(movePaths[i].ctrlLine[j].SceneNode);
                        if (MainScene.GroupNode.Items.Contains(movePaths[i].ptAdjust[j]))
                            MainScene.GroupNode.RemoveChildNode(movePaths[i].ptAdjust[j]);
                    }
                else if (movePaths[i].curveShape == CurveType.ARC && MainScene.GroupNode.Items.Contains(movePaths[i].ptAdjust[0]))
                    MainScene.RemoveNode(movePaths[i].ptAdjust[0]);
            }
        }

        public void ShowTrackingLine()
        {
            //if (!Viewport.Items.Contains(trackLine))
            //    MainScene.AddNode(trackLine);
        }

        public void HideTrackingLine()
        {
            //if (Viewport.Items.Contains(trackLine))
            //    Viewport.Items.Remove(trackLine);
        }

        public void ShowRobotArm()
        {
            if (!MainScene.GroupNode.Items.Contains(roboticArm))
                MainScene.AddNode(roboticArm);
        }

        public void HideRobotArm()
        {
            if (MainScene.GroupNode.Items.Contains(roboticArm))
                MainScene.GroupNode.RemoveChildNode(roboticArm);
        }

        public void ShowCamManipulator()
        {
            if (!MainScene.GroupNode.Items.Contains(cameraManipulator.SceneNode))
                MainScene.AddNode(cameraManipulator.SceneNode);
        }

        public void HideCamManipulator()
        {
            if (MainScene.GroupNode.Items.Contains(cameraManipulator.SceneNode))
                MainScene.GroupNode.RemoveChildNode(cameraManipulator.SceneNode);
        }

        public void ShowPointLabels()
        {
            foreach (var curve in movePaths)
                foreach (var lblVisual in curve.lblVisual3D)
                    if (!MainScene.GroupNode.Items.Contains(lblVisual.SceneNode))
                    {
                        lblVisual.Tag = new AttachedNodeViewModel(lblVisual.SceneNode);
                        MainScene.AddNode(lblVisual.SceneNode);
                    }
        }

        public void HidePointLabels()
        {
            foreach (var curve in movePaths)
                foreach (var lblVisual in curve.lblVisual3D)
                    if (MainScene.GroupNode.Items.Contains(lblVisual.SceneNode))
                        MainScene.GroupNode.RemoveChildNode(lblVisual.SceneNode);
        }

        public void ShowLimitSphere()
        {
            if (!MainScene.GroupNode.Items.Contains(limitSphere))
            {
                limitSphere.Tag = new AttachedNodeViewModel(limitSphere);
                MainScene.AddNode(limitSphere);
            }
        }

        public void HideLimitSphere()
        {
            if (MainScene.GroupNode.Items.Contains(limitSphere))
                MainScene.GroupNode.RemoveChildNode(limitSphere);
        }

        public void ShowCamBounds()
        {
            if (!MainScene.GroupNode.Items.Contains(cameraBoundingBox.SceneNode))
            {
                cameraBoundingBox.Tag = new AttachedNodeViewModel(cameraBoundingBox.SceneNode);
                MainScene.AddNode(cameraBoundingBox.SceneNode);
            }
        }

        public void HideCamBounds()
        {
            if (MainScene.GroupNode.Items.Contains(cameraBoundingBox.SceneNode))
                MainScene.GroupNode.RemoveChildNode(cameraBoundingBox.SceneNode);
        }

        public void ShowPrism()
        {
            if (!MainScene.GroupNode.Items.Contains(cameraViewPrism.SceneNode))
            {
                cameraViewPrism.Tag = new AttachedNodeViewModel(cameraViewPrism.SceneNode);
                MainScene.AddNode(cameraViewPrism.SceneNode);
            }
        }

        public void HidePrism()
        {
            if (MainScene.GroupNode.Items.Contains(cameraViewPrism.SceneNode))
                MainScene.GroupNode.RemoveChildNode(cameraViewPrism.SceneNode);
        }

        public void ShowPrismRef()
        {
            if (!MainScene.GroupNode.Items.Contains(prismRefPoint))
            {
                prismRefPoint.Tag = new AttachedNodeViewModel(prismRefPoint);
                MainScene.AddNode(prismRefPoint);
            }
        }

        public void HidePrismRef()
        {
            if (MainScene.GroupNode.Items.Contains(prismRefPoint))
                MainScene.GroupNode.RemoveChildNode(prismRefPoint);
            HidePrismRefManipulator(); //Also Hide Manipulator
        }

        /// <summary>
        /// Show Prism Reference Point 3D Manipulator
        /// </summary>
        /// <param name="showOnlyPIP">Show Only PIP Manipulator</param>
        public void ShowPrismRefManipulator(bool showOnlyPIP)
        {
            if (!MainScene.GroupNode.Items.Contains(prismRefManipulator.SceneNode))
            {
                prismRefManipulator.IsShowPIPOnly = showOnlyPIP;
                prismRefManipulator.Tag = new AttachedNodeViewModel(prismRefManipulator.SceneNode);
                MainScene.AddNode(prismRefManipulator.SceneNode);
            }
        }

        public void HidePrismRefManipulator()
        {
            if (MainScene.GroupNode.Items.Contains(prismRefManipulator.SceneNode))
                MainScene.GroupNode.RemoveChildNode(prismRefManipulator.SceneNode);
        }

        //TODO: Use when loading saved Tracer Data?
        //private void Tracer_PlotData()
        //{
        //    Log($"Trace Points Count: {tracerPoints.Count}");
        //    if (tracerPoints.Count == 1)
        //    {
        //        TracePoint3D pt;
        //        lock (tracerPoints)
        //        {
        //            pt = tracerPoints[0];
        //            tracerPoints.Clear();
        //        }
        //        pathTracer.AddPoint(pt.point, System.Windows.Media.Color.FromArgb(pt.color.A, pt.color.R, pt.color.G, pt.color.B), pt.thickness);
        //    }
        //    else
        //    {
        //        TracePoint3D[] pointsArray;
        //        lock (tracerPoints)
        //        {
        //            pointsArray = tracerPoints.ToArray();
        //            tracerPoints.Clear();
        //        }
        //        foreach (TracePoint3D pt in pointsArray)
        //            pathTracer.AddPoint(pt.point, System.Windows.Media.Color.FromArgb(pt.color.A, pt.color.R, pt.color.G, pt.color.B), pt.thickness);
        //    }
        //}

        private void Tracer_AddPoint(Vector3 pt)
        {
            var color = Color.Red;
            var thickness = 1.0;
            //tracerPoints.Add(new TracePoint3D(pt, color, thickness));
            //Tracer_PlotData();
            pathTracer.AddPoint(pt, System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B), thickness);
        }

        public void Tracer_Clear()
        {
            pathTracer.ClearTrace();
        }

        //////////

        public void Log(string msg)
        {
            LogEvent?.Invoke(this, new LogEventArgs(msg));
        }

        public void ErrorLog(string msg)
        {
            ErrorLogEvent?.Invoke(this, new LogEventArgs(msg));
        }

        private void StreamTable_AddRow(double[] joints, double vel, double acc, double dec, double leave, double reach, double distToTarget)
        {
            StreamTable_AddRowEvent?.Invoke(this, new StreamTableRowEventArgs(joints, vel, acc, dec, leave, reach, Math.Round(distToTarget, 2)));
        }

        private void StreamTable_UpdateRow(int id)
        {
            StreamTable_UpdateRowEvent?.Invoke(this, new StreamTableRowEventArgs(id));
        }

        private void StreamTable_Clear()
        {
            StreamTable_ClearEvent?.Invoke(this, new EventArgs());
        }

        private void CommandsTable_AddRow(CommandsMoveType curveShape, CartesianPos firstPt, CartesianPos lastPt, CartesianPos midPt, double vel, double acc, double dec, double focusPos, double irisPos, double zoomPos, double auxMotPos)
        {
            CommandsTable_AddRowEvent?.Invoke(this, new CommandsTableRowEventArgs(curveShape, firstPt, lastPt, midPt, vel, acc, dec, focusPos, irisPos, zoomPos, auxMotPos));
        }

        private void CommandsTable_Clear()
        {
            CommandsTable_ClearEvent?.Invoke(this, new EventArgs());
        }

        public void Enable3DPanControl()
        {
            //HelixViewport3D.PanGesture = new MouseGesture(MouseAction.LeftClick);
            //IsPanControlEnabled = true;
            //btn_Pan3DEnable.Background = Brushes.LightSalmon;
            //btn_Pan3DEnable.Content = "Pan ON";
        }

        public void Disable3DPanControl()
        {
            //HelixViewport3D.PanGesture = new MouseGesture(MouseAction.None);
            //IsPanControlEnabled = false;
            //btn_Pan3DEnable.Background = Brushes.LightGray;
            //btn_Pan3DEnable.Content = "Pan OFF";
        }

        public void CameraHome(double anim_time = -1)
        {
            if (anim_time < 0)
                anim_time = POV_AnimationTime;
            Viewport.Camera.AnimateTo(POV_Home_Pos, POV_Home_Dir, POV_Home_UpDir, anim_time);
            Viewport.Camera.Position = POV_Home_Pos;
            Viewport.Camera.LookDirection = POV_Home_Dir;
            Viewport.Camera.UpDirection = POV_Home_UpDir;
            //ViewportCameraPOV.Camera.AnimateTo(POV_Home_Pos, POV_Home_Dir, POV_Home_UpDir, anim_time);
        }

        public void CameraFront()
        {
            Viewport.Camera.AnimateTo(POV_Front_Pos, POV_Front_Dir, POV_Front_UpDir, POV_AnimationTime);
        }

        public void CameraBack()
        {
            Viewport.Camera.AnimateTo(POV_Back_Pos, POV_Back_Dir, POV_Back_UpDir, POV_AnimationTime);
        }

        public void CameraLeft()
        {
            Viewport.Camera.AnimateTo(POV_Left_Pos, POV_Left_Dir, POV_Left_UpDir, POV_AnimationTime);
        }

        public void CameraRight()
        {
            Viewport.Camera.AnimateTo(POV_Right_Pos, POV_Right_Dir, POV_Right_UpDir, POV_AnimationTime);
        }

        public void CameraTop()
        {
            Viewport.Camera.AnimateTo(POV_Top_Pos, POV_Top_Dir, POV_Top_UpDir, POV_AnimationTime);
        }

        public void CameraBottom()
        {
            Viewport.Camera.AnimateTo(POV_Bottom_Pos, POV_Bottom_Dir, POV_Bottom_UpDir, POV_AnimationTime);
        }

        #endregion

        #region CALCULATE

        private void Calculate_SyncTarget(ref List<CustomCurve> paths)
        {
            try
            {
                for (int index = 0; index < paths.Count; index++)
                {
                    List<Point3D> pts = new List<Point3D>();
                    if (paths[index].curveShape == CurveType.BEZIER)
                        pts = CustomSpline.GetBezierPoint(ref paths, index);
                    else if (paths[index].curveShape == CurveType.ARC)
                        pts = CustomSpline.GetArcPoints(ref paths, index, false);
                    else /// shape == CurveType.LINE
                    {
                        float t = 0f;
                        float dist = (float)paths[index].lenCurve;
                        float step = CustomSpline.spacing / dist;
                        Point3D item = new Point3D();
                        while (true)
                        {
                            if (t > 1f)
                            {
                                break;
                            }
                            item = CustomSpline.GetSegmentPoint(paths, index, t);
                            pts.Add(item);
                            t += step;
                        }
                    }
                    CustomCurve cur = paths[index];
                    cur.curvePath = pts;
                    paths[index] = cur;
                }
            }
            catch (Exception e)
            {
                ErrorLog($"Sync Motion Error: {e.Message}");
            }
        }

        private void Calculate_SyncMotion(ref List<CustomCurve> paths)
        {
            try
            {
                for (int index = 0; index < paths.Count; index++)
                {
                    double firstVel = velArray[index];
                    if (index != 0)
                    {
                        firstVel += velArray[index - 1];
                        firstVel /= 2;
                    }
                    double lastVel = (velArray[index] + velArray[index + 1]) / 2;
                    double accelVel = accelArray[index];
                    if (velArray[index] <= firstVel) accelVel = -accelVel;
                    double decelVel = decelArray[index];
                    if (velArray[index + 1] <= lastVel) decelVel = -decelVel;

                    List<Point3D> pts = new List<Point3D>();
                    if (paths[index].curveShape == CurveType.BEZIER)
                        pts = CustomSpline.GetBezierPoint(ref paths, index, firstVel, accelVel, velArray[index], lastVel, decelVel);
                    else if (paths[index].curveShape == CurveType.ARC)
                        pts = CustomSpline.GetArcPoints(ref paths, index, firstVel, accelVel, velArray[index], lastVel, decelVel);
                    else /// shape == CurveType.LINE
                    {
                        double t = 0f;
                        double lenCurve = paths[index].lenCurve;
                        double step = 0;
                        Point3D item = new Point3D();
                        double totTime = 0;
                        double tempSpacing = 0;
                        double tFirst = 0, tInterval = 0, tLast = 0;
                        CustomSpline.EvalTime(lenCurve, firstVel, accelVel, velArray[index], lastVel, decelVel,
                            ref tFirst, ref tInterval, ref tLast);
                        while (true)
                        {
                            if (t > 1f) break;

                            item = CustomSpline.GetSegmentPoint(paths, index, (float)t);
                            pts.Add(item);
                            if (totTime <= tFirst)
                            {
                                tempSpacing = firstVel + accelVel * totTime;
                                tempSpacing *= 10;
                            }
                            else if (totTime <= tFirst + tInterval)
                            {
                                tempSpacing = velArray[index];
                                tempSpacing *= 10;
                            }
                            else if (totTime <= tFirst + tInterval + tLast)
                            {
                                tempSpacing = velArray[index] + decelVel * (totTime - tFirst - tInterval);
                                tempSpacing *= 10;
                            }
                            //Log($"Line shape interval : {tempSpacing}\n");
                            step = tempSpacing / lenCurve;
                            t += step;
                            totTime += 10;
                        }
                    }
                    CustomCurve cur = paths[index];
                    cur.curvePath = pts;
                    paths[index] = cur;
                }
            }
            catch (Exception e)
            {
                ErrorLog($"Sync Motion Error: {e.Message}");
            }
        }

        private void Calculate_SyncPaths()
        {
            try
            {
                if (IsTrackingEnabled)
                {
                    /// Total distances of Motion and Target paths
                    double totMotion = TotalLength;
                    double totTarget = Convert.ToDouble(tb_total_length_target.Text);

                    /// Total simulation time(milliseconds)
                    TimeSpan ts = TimeSpan.FromSeconds(Duration);
                    double totMiliSeconds = ts.TotalMilliseconds;

                    float resMotion = (float)(totMotion / (totMiliSeconds / 10));
                    CustomSpline.spacing = resMotion;
                    movePathsSync = movePaths;
                    Calculate_SyncMotion(ref movePathsSync);

                    int count = 0;
                    foreach (var curve in movePathsSync)
                        count += curve.curvePath.Count;

                    float resTarget = (float)(totTarget / count);
                    CustomSpline.spacing = resTarget;
                    targetPath.targetPathsSync = targetPath.targetPaths;
                    Calculate_SyncTarget(ref targetPath.targetPathsSync);

                    /// Reset point resolution as default value(5.0)
                    CustomSpline.spacing = 5.0f;
                }
                else
                    movePathsSync = movePaths;

                Log("Paths Synchronized.");
            }
            catch (Exception e)
            {
                ErrorLog($"Sync Paths Error: {e.Message}");
            }
        }

        private struct JointPoint
        {
            public int index;
            public double[] joint;
            public JointPoint(int ind, double[] jnt)
            {
                index = ind;
                joint = jnt;
            }
        }

        public void Calculate_Start()
        {
            try
            {
                if (IsCalculateUpdateUI)
                {
                    Thread th1 = new Thread(Calculate_Thread);
                    th1.Name = "SimCalculateThread";
                    //th1.Priority = ThreadPriority.Highest;
                    th1.Start();
                }
                else
                {
                    Thread th1 = new Thread(Calculate_ParallelCompute_Thread);
                    th1.Name = "SimCalculateParallelComputeThread";
                    //th1.Priority = ThreadPriority.Highest;
                    th1.Start();
                }
            }
            catch (Exception e)
            {
                ErrorLog($"Calculate Error: {e.Message}");
            }
        }

        public void Calculate_Stop()
        {

        }

        private void Calculate_Thread()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    Log($"Calculate Thread Start");
                    UIState_BeforeCalculate();

                    // Calculate Progress Bar
                    prog_CalculateStatus.Value = 0;
                    int tempCounter = 0;
                    foreach (var segment in movePathsSync)
                    {
                        tempCounter += segment.curvePath.Count();
                    }
                    prog_CalculateStatus.Maximum = tempCounter;
                    Log($"Calculate Progress Total Points = {tempCounter}");
                });

                IsCalculateThreadRunFlag = true;
                IsCalculating = true;
                jointArray.Clear();     // Clear Trajectory Points Array (Joint) 
                trajectory.Clear();     // Clear Trajectory Points Data List (Joint, Distance, Target Positon)
                trajTotalCount = 0;

                Stopwatch sw = new Stopwatch();
                int counter = 0;
                double totalLength = 0;
                int curvesTotal = movePathsSync.Count;
                //int targetCurvesTotal = targetPath.targetPathsSync.Count;
                int targetCurvesTotal = targetMotionPath.PathsSync.Count;
                int targetCurSegment = 0;         // Current Target Trajectory Curve 
                int targetCurSegmentPoint = 0;    // Current Target Trajectory Curve Point
                Vector3 targetPtStatic = targetPoint.Position;

                sw.Start();

                // Loop through each curve segment in path
                for (int i = 0; i < curvesTotal; i++)
                {
                    int trajSegmentPointsTotal = movePathsSync[i].curvePath.Count;

                    // Loop through each point on the curve path
                    for (int j = 0; j < trajSegmentPointsTotal; j++)
                    {
                        //Dispatcher.Invoke(() =>
                        //{
                        //    Log($"Calculate Trajectory: Curves[{i}/{curvesTotal}] | Points[{j}/{trajSegmentPointsTotal}]");
                        //});

                        // Advance to target position 
                        // TODO: FIX skipped last 2 points
                        Vector3 targetPt;
                        if (targetCurvesTotal > 0) //Target is a trajectory path
                        {
                            // If reach last point of current segment
                            if (targetCurSegmentPoint == targetPath.targetPathsSync[targetCurSegment].curvePath.Count)
                            {
                                targetCurSegmentPoint = 0; // Reset back to first point of Target segment
                                targetCurSegment++; // Next Segment in Trajectory
                            }
                            // If reach last Segment
                            if (targetCurSegment > targetPath.targetPathsSync.Count - 1)
                            {
                                targetCurSegment = targetPath.targetPathsSync.Count - 1;
                                targetCurSegmentPoint = targetPath.targetPathsSync[targetCurSegment].curvePath.Count;
                            }
                            //Dispatcher.Invoke(() =>
                            //{
                            //    Log($"Target Path [{targetCurSegment}/{targetPath.targetPathsSync.Count}] - [{targetCurSegmentPoint}/{targetPath.targetPathsSync[targetCurSegment].curvePath.Count}]");
                            //});
                            targetPt = targetPath.targetPathsSync[targetCurSegment].curvePath[targetCurSegmentPoint].ToVector3();
                            targetCurSegmentPoint++; // Next Point in current segment
                        }
                        else // Target is Static Point
                        {
                            targetPt = targetPtStatic;
                        }

                        // Calculate and add to Total Path Length
                        if (j > 0)
                        {
                            totalLength += CalculateDistance(movePathsSync[i].curvePath[j].ToVector3(), movePathsSync[i].curvePath[j - 1].ToVector3());
                        }

                        Vector3 current_position = new Vector3(
                            (float)movePathsSync[i].curvePath[j].X, /// Current Point on Trajectory Path X Value 
                            (float)movePathsSync[i].curvePath[j].Y, /// Current Point on Trajectory Path Y Value
                            (float)movePathsSync[i].curvePath[j].Z  /// Current Point on Trajectory Path Z Value
                        );

                        // Calculate Rotation - Extracted from CalcRotation_MoveCamera()
                        double rotX, rotY, rotZ;
                        Vector3 direction = CalculateDirection(targetPt, movePathsSync[i].curvePath[j].ToVector3());
                        rotX = 180; // Camera Front
                        rotY = (Math.Acos(direction.Z)) * 180 / Math.PI; // Camera Front
                        rotZ = Math.Atan(direction.Y / direction.X) * 180 / Math.PI;
                        //if (CameraMount == ToolPlacement.BOTTOM){
                        rotX = 0; // Camera Bottom
                        rotY -= 90; // Camera Bottom

                        // Calculate Point to Joints 
                        double[] angles = RobotKinematics.InverseKinematics(current_position.X, current_position.Y, current_position.Z, rotX, rotY, rotZ);

                        // If end of current curve segment points list
                        if (j == trajSegmentPointsTotal - 1)
                            jointArray.Add(angles);

                        // Add current point data to the trajectory list
                        if (trajectory.Count - 1 < i)
                            trajectory.Add(new List<TrajectoryPoint>());
                        trajectory[i].Add(
                            new TrajectoryPoint(
                                    j,
                                    angles,
                                    CalculateDistance(movePathsSync[i].curvePath[j].ToVector3(), targetPt), // Calculate distance between current path point and current target point 
                                    targetPt.ToPoint3D()
                            )
                        );

                        counter++;

                        Dispatcher.Invoke(() =>
                        {
                            // Update Simulator UI
                            TotalLength = totalLength;
                            UpdateRoboticArm(angles[0], angles[1], angles[2], angles[3], angles[4], angles[5]);
                            UpdateCamera(current_position.X, current_position.Y, current_position.Z - RobotFlangeOffset.Z, rotX, rotY, rotZ);
                            slider_J1.Value = angles[0];
                            slider_J2.Value = angles[1];
                            slider_J3.Value = angles[2];
                            slider_J4.Value = angles[3];
                            slider_J5.Value = angles[4];
                            slider_J6.Value = angles[5];
                            slider_X.Value = current_position.X;
                            slider_Y.Value = current_position.Y;
                            slider_Z.Value = current_position.Z;
                            slider_RX.Value = rotX;
                            slider_RY.Value = rotY;
                            slider_RZ.Value = rotZ;
                            SetTargetPoint_Position(targetPt);
                            prog_CalculateStatus.Value = counter; // Update Calculate progress status
                        });
                    }
                }

                IsCalculating = false;

                sw.Stop();

                trajTotalCount = counter;
                jointChanged = false;
                trajCurPoint = jointArray.Count - 1;

                Dispatcher.Invoke(() =>
                {
                    // Calculate Interval Frequency
                    intctrl_ExportMode2_StreamFrequency.Value = Calculate_Interval(Settings.Default.ROBOT_EXPORT2_INTERVAL);

                    UIState_AfterCalculate();
                    Log($"trajTotalCount={trajTotalCount}, trajectory list count={TrajectoryJointsCount()}.");
                    Log($"Calculate (Thread) total time: {sw.Elapsed.TotalSeconds:F2} s.");
                });

                IsCalculateThreadRunFlag = false;

            }
            catch (Exception e)
            {
                Dispatcher.Invoke(() =>
                {
                    ErrorLog($"Calculate thread exception: {e}");
                });
            }
        }

        public void Calculate_ParallelCompute_Thread()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    Log($"Calculate Thread Start");
                    UIState_BeforeCalculate();

                    // Calculate Progress Bar
                    prog_CalculateStatus.Value = 0;
                    int tempCounter = 0;
                    foreach (var segment in movePathsSync) tempCounter += segment.curvePath.Count();
                    prog_CalculateStatus.Maximum = tempCounter;
                    Log($"Calculate Progress Total Points = {tempCounter}");

                });

                jointArray.Clear();     // Clear Trajectory Points Array (Joint) 
                trajectory.Clear();     // Clear Trajectory Points Data List (Joint, Distance, Target Positon)

                IsCalculateThreadRunFlag = true;

                int counter = 0;        // Count Number of Points in Trajectory and progress status use
                double totalLength = 0; // Total length accumulator

                // Sync Trajectory Path (Segment, SegPoint) to Index Mapping Array
                Dictionary<(int, int), int> indexedTrajectory = new Dictionary<(int, int), int>();
                for (int i = 0; i < movePathsSync.Count; i++)
                {
                    for (int j = 0; j < movePathsSync[i].curvePath.Count; j++)
                    {
                        indexedTrajectory.Add((i, j), counter);
                        counter++;
                    }
                }
                trajectory = new List<List<TrajectoryPoint>>(counter); // Create List of the Size of the Trajectory Points

                counter = 0; // Reset Counter

                // Sync Target Path Index to (Segment, SegPoint) Mapping Array
                Vector3 targetPtStatic = targetPoint.Position;
                int targetCurvesTotal = targetPath.targetPathsSync.Count;
                List<(int, int)> targetSyncMap = new List<(int, int)>();
                if (targetCurvesTotal > 0) //Target is a trajectory path
                {
                    // Enumerate / Index Target Path
                    for (int u = 0; u < targetPath.targetPathsSync.Count; u++)
                    {
                        for (int v = 0; v < targetPath.targetPathsSync[u].curvePath.Count; v++)
                        {
                            targetSyncMap.Add((u, v));
                        }
                    }
                }

                Stopwatch sw = new Stopwatch();
                sw.Start();

                // Loop through each curve segment in path
                for (int i = 0; i < movePathsSync.Count; i++)
                {
                    int trajSegmentPointsTotal = movePathsSync[i].curvePath.Count;
                    ConcurrentBag<JointPoint> unsortedJointArray = new ConcurrentBag<JointPoint>();
                    ConcurrentBag<TrajectoryPoint> unsortedSegmentPoints = new ConcurrentBag<TrajectoryPoint>();

                    try
                    {
                        // Parallel Task Loop through each point on the curve path
                        Parallel.For(0, trajSegmentPointsTotal, j =>
                        {
                            //Dispatcher.Invoke(() =>
                            //{
                            //    Log($"Calculate (Parallel) Trajectory: Curves[{i}/{curvesTotal}] | Points[{j}/{trajSegmentPointsTotal}]");
                            //});

                            // Get Sync target position 
                            Vector3 targetPt;
                            if (targetCurvesTotal > 0) //Target is a trajectory path
                            {
                                (int, int) targetIndex = targetSyncMap[indexedTrajectory[(i, j)]];
                                //Dispatcher.InvokeAsync(() =>
                                //{
                                //    Log($"Target Path [{targetIndex.Item1}/{targetPath.targetPathsSync.Count}] - [{targetIndex.Item2}/{targetPath.targetPathsSync[targetIndex.Item1].curvePath.Count}]");
                                //});
                                targetPt = targetPath.targetPathsSync[targetIndex.Item1].curvePath[targetIndex.Item2].ToVector3();
                            }
                            else // Target is Static Point
                            {
                                targetPt = targetPtStatic;
                            }

                            // Calculate and add to Total Path Length
                            if (j > 0)
                                totalLength += CalculateDistance(movePathsSync[i].curvePath[j].ToVector3(), movePathsSync[i].curvePath[j - 1].ToVector3());

                            // Calculate Rotation - Extracted from CalcRotation_MoveCamera()
                            double rotX, rotY, rotZ;
                            Vector3 direction = CalculateDirection(targetPt, movePathsSync[i].curvePath[j].ToVector3());
                            rotX = 180; // Camera Front
                            rotY = (Math.Acos(direction.Z)) * 180 / Math.PI; // Camera Front
                            rotZ = Math.Atan(direction.Y / direction.X) * 180 / Math.PI;
                            //if (CameraMount == ToolPlacement.BOTTOM){
                            rotX = 0; // Camera Bottom
                            rotY -= 90; // Camera Bottom

                            // Calculate Point to Joints 
                            double[] angles = RobotKinematics.InverseKinematics((float)movePathsSync[i].curvePath[j].X,
                                                                                 (float)movePathsSync[i].curvePath[j].Y,
                                                                                 (float)movePathsSync[i].curvePath[j].Z,
                                                                                 rotX,
                                                                                 rotY,
                                                                                 rotZ);

                            // If end of current curve segment points list
                            if (j == trajSegmentPointsTotal - 1)
                                unsortedJointArray.Add(new JointPoint(j, angles));

                            // Add current point data to the unsorted Segment Points list
                            unsortedSegmentPoints.Add(
                                new TrajectoryPoint(
                                        j,
                                        angles,
                                        CalculateDistance(movePathsSync[i].curvePath[j].ToVector3(), targetPt), // Calculate distance between current path point and current target point 
                                        targetPt.ToPoint3D()
                                )
                            );

                            Dispatcher.InvokeAsync(() =>
                            {
                                prog_CalculateStatus.Value++;
                            });

                            counter++;

                        });
                    }
                    catch (AggregateException ae)
                    {
                        var ignoredExceptions = new List<Exception>();
                        foreach (var ex in ae.Flatten().InnerExceptions)
                        {
                            if (ex is ArgumentException)
                                Dispatcher.Invoke(() =>
                                {
                                    ErrorLog($"Calculate (Parallel) Error: {ex.Message}");
                                });
                            else
                                ignoredExceptions.Add(ex);
                        }
                        if (ignoredExceptions.Count > 0)
                            throw new AggregateException(ignoredExceptions);
                    }

                    // Sort and add current segment point data to global jointArray list
                    foreach (var sortedList in unsortedJointArray.OrderBy(o => o.index).ToList())
                    {
                        jointArray.Add(sortedList.joint);
                    }

                    // Sort and add current segment point data to global trajectory list
                    trajectory.Add(unsortedSegmentPoints.OrderBy(o => o.index).ToList());

                }

                // Debuging Show trajectory ordered index
                //Dispatcher.InvokeAsync(() =>
                //{
                //    Log($"Trajectory ordered indexes:");
                //    int segNum = 0;
                //    foreach (var segment in trajectory)
                //    {
                //        foreach (var point in segment)
                //        {
                //            Log($"Segment={segNum} => Point={point.index}");
                //        }
                //        segNum++;
                //    }
                //});

                sw.Stop();
                trajTotalCount = counter;
                jointChanged = false;
                trajCurPoint = jointArray.Count - 1;

                Dispatcher.Invoke(() =>
                {
                    TotalLength = totalLength;

                    //Update Calculate progress status
                    prog_CalculateStatus.Value = counter;

                    // Calculate Interval Frequency
                    intctrl_ExportMode2_StreamFrequency.Value = Calculate_Interval(Settings.Default.ROBOT_EXPORT2_INTERVAL);

                    UIState_AfterCalculate();
                    Log($"trajTotalCount={trajTotalCount}, trajectory list count={TrajectoryJointsCount()}.");
                    Log($"Calculate (Parallel) total time: {sw.Elapsed.TotalSeconds:F2} s");
                });

                IsCalculateThreadRunFlag = false;
            }
            catch (Exception e)
            {
                ErrorLog($"Calculate (Parallel) Error: {e.Message} - {e.StackTrace}");
            }
        }

        private int Calculate_TargetInterval(int timerMotionInterv)
        {
            int totMotionCount = 0, totTargetCount = 0;
            foreach (var curve in movePaths)
                totMotionCount += curve.curvePath.Count;
            foreach (var curve in targetPath.targetPaths)
                totTargetCount += curve.curvePath.Count;
            int retVal = (int)(totMotionCount * timerMotionInterv * 1.52) / totTargetCount;
            if (retVal == 0) retVal = 1;
            return retVal;
        }

        private int Calculate_Interval(int timerInterv)
        {
            int totCount = 0;
            foreach (var segment in trajectory)
                totCount += segment.Count;
            double retVal = totCount / (Duration * 1000 / timerInterv);
            if (retVal == 0) retVal = 1;
            return (int)Math.Round(retVal, 0);
        }

        #endregion

        #region PLAYBACK

        public void GoHomePosition()
        {
            IsHoming = true;
            switch (CameraMount)
            {
                case ToolPlacement.BOTTOM:
                    ClearSelection();
                    UpdateAll(RobotKinematics.HomeBottomJointPos, RobotKinematics.CameraOffsetConfig);
                    break;
                case ToolPlacement.FRONT:
                    ClearSelection();
                    UpdateAll(RobotKinematics.HomeFrontJointPos, RobotKinematics.CameraOffsetConfig);
                    break;
            }
            IsHoming = false;
        }

        public void GoPreviousPathPoint()
        {
            if (jointArray.Count < 1)
                return;

            trajCurPoint--;

            if (trajCurPoint <= 0)
            {
                trajCurPoint = 0;
                IsFirstPoint = true;
            }
            else
            {
                IsFirstPoint = false;
            }

            IsNavigating = true;
            UpdateAll(
                new JointPos(
                    jointArray[trajCurPoint][0],
                    jointArray[trajCurPoint][1],
                    jointArray[trajCurPoint][2],
                    jointArray[trajCurPoint][3],
                    jointArray[trajCurPoint][4],
                    jointArray[trajCurPoint][5]
                )
            );
            IsNavigating = false;
        }

        public void GoNextPathPoint()
        {
            if (jointArray.Count < 1)
                return;

            trajCurPoint++;

            if (trajCurPoint >= jointArray.Count - 1)
            {
                trajCurPoint = jointArray.Count - 1;
                IsLastPoint = true;
            }
            else
            {
                IsLastPoint = false;
            }

            IsNavigating = true;
            UpdateAll(
                new JointPos(
                    jointArray[trajCurPoint][0],
                    jointArray[trajCurPoint][1],
                    jointArray[trajCurPoint][2],
                    jointArray[trajCurPoint][3],
                    jointArray[trajCurPoint][4],
                    jointArray[trajCurPoint][5]
                )
            );
            IsNavigating = false;
        }

        public void GoFirstPathPoint()
        {
            trajCurPoint = 0;
            IsNavigating = true;
            UpdateAll(
                new JointPos(
                    jointArray[trajCurPoint][0],
                    jointArray[trajCurPoint][1],
                    jointArray[trajCurPoint][2],
                    jointArray[trajCurPoint][3],
                    jointArray[trajCurPoint][4],
                    jointArray[trajCurPoint][5]
                )
            );
            IsNavigating = false;
        }

        public void GoLastPathPoint()
        {
            trajCurPoint = jointArray.Count - 1;
            IsNavigating = true;
            UpdateAll(
                new JointPos(
                    jointArray[trajCurPoint][0],
                    jointArray[trajCurPoint][1],
                    jointArray[trajCurPoint][2],
                    jointArray[trajCurPoint][3],
                    jointArray[trajCurPoint][4],
                    jointArray[trajCurPoint][5]
                )
            );
            IsNavigating = false;
        }

        public void PlaybackStepForward()
        {
            timeCount_Playback += StepSize;
            trajCurCurvePoint += StepSize;
            if (trajCurCurvePoint > trajectory[trajCurCurve].Count - 1)
            {
                trajCurCurvePoint -= trajectory[trajCurCurve].Count;
                trajCurCurve++;
            }
            if (trajCurCurve > trajectory.Count - 1)
            {
                trajCurCurve = trajectory.Count - 1;
                trajCurCurvePoint = trajectory[trajCurCurve].Count - 1;
                //SimPlayer.State_ReachEnd();
            }

            double[] angles = trajectory[trajCurCurve][trajCurCurvePoint].jointAngles;

            IsNavigating = true;
            UpdateAll(new JointPos(angles[0], angles[1], angles[2], angles[3], angles[4], angles[5]));
            IsNavigating = false;

            //SimPlayer.PlayingPosition = timeCount_Playback * 100 / trajTotalCount;
        }

        public void PlaybackStepReverse()
        {
            timeCount_Playback -= StepSize;
            trajCurCurvePoint -= StepSize;
            if (trajCurCurvePoint < 0)
            {
                trajCurCurve--;
                if (trajCurCurve >= 0)
                    trajCurCurvePoint = trajectory[trajCurCurve].Count + trajCurCurvePoint;
            }
            if (trajCurCurve < 0)
            {
                trajCurCurve = 0;
                trajCurCurvePoint = 0;
                //SimPlayer.State_ReachStart();
            }

            double[] angles = trajectory[trajCurCurve][trajCurCurvePoint].jointAngles;

            IsNavigating = true;
            UpdateAll(new JointPos(angles[0], angles[1], angles[2], angles[3], angles[4], angles[5]));
            IsNavigating = false;

            //SimPlayer.PlayingPosition = timeCount_Playback * 100 / trajTotalCount;
        }

        public void PlaybackStop()
        {
            if (!IsPlaying || PlaybackTimer == null)
                return;

            UIState_ActionsEnabled();
            //SimPlayer.State_PlaybackStop();
            trajCurPoint = trajCurCurve;
            PlaybackTimer.IsEnabled = false;
            PlaybackTimer.Stop();
            //timerPlay.Tick -= new System.EventHandler(timer_play);
            PlaybackTimer = null;

        }

        public void PlaybackPause()
        {
            if (!IsPlaying || PlaybackTimer == null)
                return;

            //if (IsPlayForward)
            //    SimPlayer.State_PlaybackPauseForward();
            //else
            //    SimPlayer.State_PlaybackPauseBackward();

            if (PlaybackTimer.IsEnabled)
            {
                PlaybackTimer.IsEnabled = false;
                IsPlaybackPaused = true;
            }
        }

        public void PlaybackForward()
        {
            if (Duration <= 0)
                return;
            UIState_ActionsDisabled();
            //SimPlayer.State_PlaybackForward();

            if (IsSyncEnabled) IsSyncEnabled = false;
            IsPlayForward = true;

            if (PlaybackTimer != null)
            {
                if (!PlaybackTimer.IsEnabled)
                {
                    PlaybackTimer.IsEnabled = true;
                }
            }
            else
            {
                trajCurCurvePoint = 0;
                trajCurCurve = 0;

                if (!IsPlaybackPaused)
                {
                    curPlayheadPos = 0;
                    curPlayheadTime = 0;
                    timeCount_Playback = 0;
                }
                //SimPlayer.PlayingPosition = curPlayheadTime;

                // TODO: CHANGE TO THREADING TIMER
                // https://stackoverflow.com/questions/10317088/why-there-are-5-versions-of-timer-classes-in-net
                PlaybackTimer = new DispatcherTimer();
                PlaybackTimer.Interval = TimeSpan.FromMilliseconds(PlaybackTimer_Interval);
                PlaybackTimer.Tick += async (s, e) =>
                {
                    PlaybackTimer_Tick(s, e);
                };
                PlaybackTimer.IsEnabled = true;

                total_time_origin = (float)(PlaybackTimer_Interval * (trajTotalCount - 1)) * 0.001f;
                play_step = (int)(Math.Round(total_time_origin / Duration));
                if (total_time_origin / Duration < 1)
                {
                    PlaybackTimer.Interval = TimeSpan.FromMilliseconds((int)(PlaybackTimer_Interval * (float)(Duration / total_time_origin)));
                    play_step = 1;
                }

                PlaybackTimer.Start();
            }
        }

        public void PlaybackBackward()
        {
            if (Duration == 0)
                return;
            UIState_ActionsDisabled();
            //SimPlayer.State_PlaybackBackward();

            if (IsSyncEnabled) IsSyncEnabled = false;
            IsPlayForward = false;

            if (PlaybackTimer != null)
            {
                if (!PlaybackTimer.IsEnabled)
                {
                    PlaybackTimer.IsEnabled = true;
                }
            }
            else
            {
                trajCurCurve = trajectory.Count - 1;
                trajCurCurvePoint = trajectory[trajCurCurve].Count - 1;

                if (!IsPlaybackPaused)
                {
                    curPlayheadPos = 100;
                    curPlayheadTime = 0;
                    timeCount_Playback = trajTotalCount;
                }
                //SimPlayer.PlayingPosition = curPlayheadTime;

                PlaybackTimer = new DispatcherTimer();
                PlaybackTimer.Interval = TimeSpan.FromMilliseconds(PlaybackTimer_Interval);
                PlaybackTimer.Tick += async (s, e) =>
                {
                    PlaybackTimer_Tick(s, e);
                };
                PlaybackTimer.IsEnabled = true;

                total_time_origin = (float)(PlaybackTimer_Interval * (trajTotalCount - 1)) * 0.001f;
                play_step = (int)(Math.Round(total_time_origin / Duration));
                if (total_time_origin / Duration < 1)
                {
                    PlaybackTimer.Interval = TimeSpan.FromMilliseconds((int)(PlaybackTimer_Interval * (float)(Duration / total_time_origin)));
                    play_step = 1;
                }

                PlaybackTimer.Start();
            }
        }

        private double playbackPrevTime = 0;
        public void PlaybackMoveTo(double time)
        {
            //UIState_ActionsDisabled();

            var totalCount = TrajectoryJointsCount();

            if (playbackPrevTime < time) // FORWARD 
            {
                //trajCurCurvePoint = 0;
                //trajCurCurve = 0; 
                //curPlayheadPos = 0;
                //curPlayheadTime = 0;
                //timeCount_Playback = 0;
            }
            else // BACKWARD (time < playbackPrevTime)
            {
                //trajCurCurve = trajectory.Count - 1;
                //trajCurCurvePoint = trajectory[trajCurCurve].Count - 1;
                //curPlayheadPos = 100;
                //curPlayheadTime = 0;
                //timeCount_Playback = trajTotalCount;
            }

            trajCurCurve = 0;
            trajCurCurvePoint = 0;

            /// Update the position of target point
            targetPoint.Transform = new TranslateTransform3D(
                new Vector3D(
                    trajectory[trajCurCurve][trajCurCurvePoint].targetPosition.X,
                    trajectory[trajCurCurve][trajCurCurvePoint].targetPosition.Y,
                    trajectory[trajCurCurve][trajCurCurvePoint].targetPosition.Z
                )
            );

            /// Update the position of camera 
            UpdateAll(
                new JointPos(
                    trajectory[trajCurCurve][trajCurCurvePoint].jointAngles[0],
                    trajectory[trajCurCurve][trajCurCurvePoint].jointAngles[1],
                    trajectory[trajCurCurve][trajCurCurvePoint].jointAngles[2],
                    trajectory[trajCurCurve][trajCurCurvePoint].jointAngles[3],
                    trajectory[trajCurCurve][trajCurCurvePoint].jointAngles[4],
                    trajectory[trajCurCurve][trajCurCurvePoint].jointAngles[5]
                )
            );

        }

        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            var stepsize = 1;
            if (IsPlayRealtime)
            {
                stepsize = play_step;
                curPlayheadTime += PlaybackTimer.Interval.TotalSeconds;
            }
            //SimPlayer.PlayingPosition = curPlayheadTime;

            if (IsPlayForward)
            {
                timeCount_Playback++;
                trajCurCurvePoint += stepsize;
                if (trajCurCurvePoint > trajectory[trajCurCurve].Count - 1)
                {
                    trajCurCurvePoint = 0;
                    trajCurCurve++;
                }
                if (trajCurCurve > trajectory.Count - 1)
                {
                    trajCurPoint = jointArray.Count - 1;
                    trajCurCurve = trajectory.Count - 1;
                    trajCurCurvePoint = trajectory[trajCurCurve].Count - 1;
                    PlaybackTimer.IsEnabled = false;
                    PlaybackTimer.Stop();
                    PlaybackTimer = null;
                    btn_PointData_First.IsEnabled = true;
                    btn_PointData_Previous.IsEnabled = true;
                    btn_PointData_Next.IsEnabled = false;
                    btn_PointData_Last.IsEnabled = false;
                    btn_Add_Default.IsEnabled = true;
                    //SimPlayer.State_ReachEnd();
                    IsPlaybackPaused = false;
                    timeCount_Playback = trajTotalCount;
                }
                curPlayheadPos = (double)timeCount_Playback / trajTotalCount * 100;
            }
            else
            {
                timeCount_Playback--;
                trajCurCurvePoint -= stepsize;
                if (trajCurCurvePoint < 0)
                {
                    trajCurCurve--;
                    if (trajCurCurve >= 0)
                        trajCurCurvePoint = trajectory[trajCurCurve].Count - 1;
                }
                if (trajCurCurve < 0)
                {
                    trajCurPoint = 0;
                    trajCurCurve = 0;
                    trajCurCurvePoint = 0;
                    PlaybackTimer.IsEnabled = false;
                    PlaybackTimer.Stop();
                    PlaybackTimer = null;
                    btn_PointData_First.IsEnabled = false;
                    btn_PointData_Previous.IsEnabled = false;
                    btn_PointData_Next.IsEnabled = true;
                    btn_PointData_Last.IsEnabled = true;
                    btn_Add_Default.IsEnabled = true;
                    //SimPlayer.State_ReachStart();
                    IsPlaybackPaused = false;
                    timeCount_Playback = 0;
                }
                curPlayheadPos = (double)timeCount_Playback / trajTotalCount * 100;
            }

            //SimPlayer.PlayingPosition = curPlayheadPos;
            //Log($"TimerPlay Percent: {SimPlayer.PlayPosition}");
            //Log($"nCurve={nCurve} nCount={nCount}");

            /// Update the position of target point
            targetPoint.Transform = new TranslateTransform3D(
                new Vector3D(
                    trajectory[trajCurCurve][trajCurCurvePoint].targetPosition.X,
                    trajectory[trajCurCurve][trajCurCurvePoint].targetPosition.Y,
                    trajectory[trajCurCurve][trajCurCurvePoint].targetPosition.Z
                )
            );

            UpdateAll(
                new JointPos(
                    trajectory[trajCurCurve][trajCurCurvePoint].jointAngles[0],
                    trajectory[trajCurCurve][trajCurCurvePoint].jointAngles[1],
                    trajectory[trajCurCurve][trajCurCurvePoint].jointAngles[2],
                    trajectory[trajCurCurve][trajCurCurvePoint].jointAngles[3],
                    trajectory[trajCurCurve][trajCurCurvePoint].jointAngles[4],
                    trajectory[trajCurCurve][trajCurCurvePoint].jointAngles[5]
                )
            );

        }

        private int TrajectoryJointsCount()
        {
            int count = 0;
            for (int i = 0; i < trajectory.Count; i++)
            {
                for (int j = 0; j < trajectory[i].Count; j++)
                {
                    count += 1;
                }
            }
            return count;
        }

        public Keyframe GetKeyframeData()
        {
            var keyframe = new Keyframe();
            keyframe.X = slider_X.Value;
            keyframe.Y = slider_Y.Value;
            keyframe.Z = slider_Z.Value;
            keyframe.RX = slider_RX.Value;
            keyframe.RY = slider_RY.Value;
            keyframe.RZ = slider_RZ.Value;
            keyframe.FOCUS = FocusDistance;
            keyframe.IRIS = 0;
            keyframe.ZOOM = 0;
            keyframe.TX = slider_targetX.Value;
            keyframe.TY = slider_targetY.Value;
            keyframe.TZ = slider_targetZ.Value;
            return keyframe;
        }


        #endregion

        #region FILE HANDLING

        public void UpdateOpenData()
        {
            addVisual();
            //lineBezier = new List<Trajectory>();
            for (int i = trajOldTotalCount; i < movePaths.Count; i++)
            {
                if (i != 0 && i == trajOldTotalCount)
                {
                    //makeDefaultBezier(i, false);
                    continue;
                }
                if (movePaths[i].curveShape == CurveType.BEZIER)
                {
                    //drawBezierCurveSegment(i, false);
                    //drawControlCurveSegment(i, false);
                }
                //else if (movePaths[i].curveShape == CurveType.ARC)
                //    //drawArc(i);
                //else if (movePaths[i].curveShape == CurveType.LINE)
                //    drawSegment(i);
            }
        }

        public void OpenFile()
        {
            //CustomCurve curve = new CustomCurve();
            //CustomCurve curve0 = new CustomCurve();
            //curve0.lblVisual3D = new List<BillboardText>();

            //try
            //{
            //    OpenFileDialog openFileDialog = new OpenFileDialog();
            //    openFileDialog.Filter = "Motion Path File|*.xml";
            //    string fileName;
            //    if (openFileDialog.ShowDialog() == true)
            //    {
            //        if (movePaths.Count == 1 && movePaths[0].ptCurve.Count == 1)
            //            Reset();

            //        trajOldTotalCount = movePaths.Count;

            //        fileName = openFileDialog.FileName;

            //        System.Xml.Linq.XDocument xDoc = System.Xml.Linq.XDocument.Load(fileName);
            //        var items = xDoc.Descendants("PathData");

            //        int i = trajOldTotalCount;
            //        if (trajOldTotalCount > 0)
            //            i++;

            //        //movePaths = new List<CustomCurve>();

            //        var ptMainColor = Color.LimeGreen;
            //        var ptControlColor = Color.Blue;

            //        int text_offset = 60;
            //        foreach (var item in items)
            //        {
            //            ///////////  shape of curve  //////////////
            //            curve.curveShape = item.Element("shape").Value;
            //            if (trajOldTotalCount > 0)
            //                curve0.curveShape = item.Element("shape").Value;

            //            ////////// pt1 ////////////
            //            string ptStr = item.Element("pt1").Value;
            //            string[] ptsStr = ptStr.Split(',');
            //            Point3D pt = new Point3D(Convert.ToInt32(Convert.ToDouble(ptsStr[0])), Convert.ToInt32(Convert.ToDouble(ptsStr[1])), Convert.ToInt32(Convert.ToDouble(ptsStr[2])));
            //            BezierMarkPoint3D mark = new BezierMarkPoint3D(i, PathType.MOTIONPATH, PointType.POINT, pt, ptMainColor);
            //            mark.Transform = new TranslateTransform3D(new Vector3D(pt.X, pt.Y, pt.Z));
            //            curve.ptCurve = new List<BezierMarkPoint3D>();
            //            curve.ptCurve.Add(mark);
            //            curve.lblVisual3D = new List<BillboardText>();
            //            var textVis3d1 = new BillboardText("0", new Point3D(pt.X, pt.Y, pt.Z + text_offset));
            //            curve.lblVisual3D.Add(textVis3d1);
            //            if (trajOldTotalCount > 0 && i == trajOldTotalCount + 1)
            //            {
            //                curve0.ptCurve = new List<BezierMarkPoint3D>();
            //                mark.curveNum -= 1;
            //                curve0.ptCurve.Add(mark);
            //                var textVis3d2 = new BillboardText("0", new Point3D(pt.X, pt.Y, pt.Z + text_offset));
            //                curve0.lblVisual3D.Add(textVis3d2);
            //            }

            //            ////////// pt2 //////////
            //            if (i == 0)
            //            {
            //                ptStr = item.Element("pt2").Value;
            //                ptsStr = ptStr.Split(',');
            //                pt = new Point3D(Convert.ToInt32(Convert.ToDouble(ptsStr[0])), Convert.ToInt32(Convert.ToDouble(ptsStr[1])), Convert.ToInt32(Convert.ToDouble(ptsStr[2])));
            //                mark = new BezierMarkPoint3D(i, PathType.MOTIONPATH, PointType.POINT, pt, ptMainColor);
            //                mark.Transform = new TranslateTransform3D(new Vector3D(pt.X, pt.Y, pt.Z));
            //                curve.ptCurve.Add(mark);
            //                var textVis3d3 = new BillboardText("0", new Point3D(pt.X, pt.Y, pt.Z + text_offset));
            //                curve.lblVisual3D.Add(textVis3d3);
            //            }
            //            if (trajOldTotalCount > 0 && i == trajOldTotalCount + 1)
            //            {
            //                ptStr = item.Element("pt2").Value;
            //                ptsStr = ptStr.Split(',');
            //                pt = new Point3D(Convert.ToInt32(Convert.ToDouble(ptsStr[0])), Convert.ToInt32(Convert.ToDouble(ptsStr[1])), Convert.ToInt32(Convert.ToDouble(ptsStr[2])));
            //                mark = new BezierMarkPoint3D(i, PathType.MOTIONPATH, PointType.POINT, pt, ptMainColor);
            //                mark.Transform = new TranslateTransform3D(new Vector3D(pt.X, pt.Y, pt.Z));
            //                curve.ptCurve[0] = mark;
            //                var textVis3d4 = new BillboardText("0", new Point3D(pt.X, pt.Y, pt.Z + text_offset));
            //                curve.lblVisual3D[0] = textVis3d4;
            //            }

            //            ///////// pt3 ///////////
            //            ptStr = item.Element("pt3").Value;
            //            ptsStr = ptStr.Split(',');
            //            pt = new Point3D(Convert.ToInt32(Convert.ToDouble(ptsStr[0])), Convert.ToInt32(Convert.ToDouble(ptsStr[1])), Convert.ToInt32(Convert.ToDouble(ptsStr[2])));
            //            mark = new BezierMarkPoint3D(i, PathType.MOTIONPATH, PointType.CONTROL, pt, ptControlColor);
            //            mark.Transform = new TranslateTransform3D(new Vector3D(pt.X, pt.Y, pt.Z));
            //            curve.ptAdjust = new List<BezierMarkPoint3D>();
            //            curve.ptAdjust.Add(mark);

            //            ////////// pt4 ///////////
            //            ptStr = item.Element("pt4").Value;
            //            ptsStr = ptStr.Split(',');
            //            pt = new Point3D(Convert.ToInt32(Convert.ToDouble(ptsStr[0])), Convert.ToInt32(Convert.ToDouble(ptsStr[1])), Convert.ToInt32(Convert.ToDouble(ptsStr[2])));
            //            mark = new BezierMarkPoint3D(i, PathType.MOTIONPATH, PointType.CONTROL, pt, ptControlColor);
            //            mark.Transform = new TranslateTransform3D(new Vector3D(pt.X, pt.Y, pt.Z));
            //            curve.ptAdjust.Add(mark);

            //            if (trajOldTotalCount != 0 && i == trajOldTotalCount + 1)
            //            {
            //                movePaths.Add(curve0);
            //            }
            //            movePaths.Add(curve);

            //            /////////// joint data ///////////
            //            ptStr = item.Element("jointChanged").Value;
            //            jointChanged &= Convert.ToBoolean(ptStr);
            //            double[] temp = { 0, 0, 0, 0, 0, 0 };
            //            if (i == 0 || (trajOldTotalCount > 0 && i == trajOldTotalCount + 1))
            //            {
            //                ptStr = item.Element("joint0").Value;
            //                ptsStr = ptStr.Split(',');
            //                for (int k = 0; k < 6; k++)
            //                    temp[k] = Convert.ToDouble(ptsStr[k]);
            //                jointArray.Add(temp);
            //            }

            //            double[] tempp = { 0, 0, 0, 0, 0, 0 };
            //            ptStr = item.Element("joint1").Value;
            //            ptsStr = ptStr.Split(',');
            //            for (int k = 0; k < 6; k++)
            //                tempp[k] = Convert.ToDouble(ptsStr[k]);
            //            jointArray.Add(tempp);

            //            i++;
            //        }

            //        renameLbl();
            //        UpdateOpenData();

            //        btn_PointData_First.IsEnabled = true;
            //        btn_PointData_Last.IsEnabled = true;
            //        btn_Save.IsEnabled = true;
            //        btn_SyncPaths.IsEnabled = true;
            //        //btn_Calculate.IsEnabled = true;
            //        btn_Reset.IsEnabled = true;
            //        if (!jointChanged)
            //        {
            //            btn_PointData_Previous.IsEnabled = true;
            //            btn_PointData_Next.IsEnabled = true;
            //        }

            //    }
            //}
            //catch (Exception e)
            //{
            //    MessageBox.Show(e.Message, "Simulator Open File Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }

        public void SaveFile()
        {
            try
            {
                if (movePaths.Count == 0)
                    return;
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Path file (*.xml)|*.xml";
                string time = DateTime.Now.ToString("hhmmss"); // includes leading zeros
                string date = DateTime.Now.ToString("yyMMdd"); // includes leading zeros
                saveFileDialog.FileName = $"motionpath_{date}_{time}.xml";
                string fileName;
                if (saveFileDialog.ShowDialog() == true)
                {
                    fileName = saveFileDialog.FileName;
                    // ... add items...
                    System.Data.DataTable dt = new System.Data.DataTable();
                    dt.TableName = "PathData";
                    dt.Columns.Add("shape");
                    dt.Columns.Add("pt1");
                    dt.Columns.Add("pt2");
                    dt.Columns.Add("pt3");
                    dt.Columns.Add("pt4");
                    dt.Columns.Add("jointChanged");
                    dt.Columns.Add("joint0");
                    dt.Columns.Add("joint1");
                    string tempp = "";
                    for (int i = 0; i < movePaths.Count; i++)
                    {
                        ///////////  point data  /////////////
                        dt.Rows.Add();
                        dt.Rows[dt.Rows.Count - 1]["shape"] = movePaths[i].curveShape;
                        dt.Rows[dt.Rows.Count - 1]["pt1"] = movePaths[i].ptCurve[0].point;
                        if (i == 0)
                            dt.Rows[dt.Rows.Count - 1]["pt2"] = movePaths[i].ptCurve[1].point;
                        dt.Rows[dt.Rows.Count - 1]["pt3"] = movePaths[i].ptAdjust[0].point;
                        dt.Rows[dt.Rows.Count - 1]["pt4"] = movePaths[i].ptAdjust[1].point;

                        ///////////  joint data  ////////////
                        dt.Rows[dt.Rows.Count - 1]["jointChanged"] = jointChanged;
                        if (i == 0)
                        {
                            tempp = Convert.ToString(jointArray[0][0]);
                            for (int k = 1; k < 6; k++)
                                tempp += ("," + Convert.ToString(jointArray[0][k]));
                            dt.Rows[dt.Rows.Count - 1]["joint0"] = tempp;
                        }
                        tempp = Convert.ToString(jointArray[i + 1][0]);
                        for (int k = 1; k < 6; k++)
                            tempp += ("," + Convert.ToString(jointArray[i + 1][k]));
                        dt.Rows[dt.Rows.Count - 1]["joint1"] = tempp;
                    }

                    dt.WriteXml(fileName);

                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Simulator Save File Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region UI METHODS & HANDLERS

        private void UIRefreshView()
        {
            if (FullscreenMode)
            {
                grid_SimulatorRobotView.Width = System.Windows.SystemParameters.PrimaryScreenWidth;

                grid_SimulatorCameraView.Margin = new Thickness(10, 650, 0, 0);
                grid_SimulatorCameraView.Width = 320;
                grid_SimulatorCameraView.Height = 240;
                if (CameraViewMode)
                {
                    grid_SimulatorCameraView.Visibility = Visibility.Visible;
                    ViewportCameraPOV.Visibility = Visibility.Visible;
                }
                else
                {
                    grid_SimulatorCameraView.Visibility = Visibility.Hidden;
                    ViewportCameraPOV.Visibility = Visibility.Hidden;
                }

                Expander_MotionPath.IsExpanded = true;
                Expander_TargetPath.IsExpanded = true;
                Expander_Calculate.IsExpanded = true;
                Expander_Simulator.IsExpanded = true;
                Expander_Settings.IsExpanded = false;
                Expander_Scene.IsExpanded = true;
                Btn_Fullscreen.Content = "Exit";

            }
            else // Not Fullscreen
            {
                grid_SimulatorRobotView.Width = 1026;

                grid_SimulatorCameraView.Margin = new Thickness(1026, 0, 0, 0);
                grid_SimulatorCameraView.Width = 894;
                grid_SimulatorCameraView.Height = 504;
                if (CameraViewMode)
                {
                    grid_SimulatorCameraView.Visibility = Visibility.Visible;
                    ViewportCameraPOV.Visibility = Visibility.Visible;
                }
                else
                {
                    grid_SimulatorCameraView.Visibility = Visibility.Hidden;
                    ViewportCameraPOV.Visibility = Visibility.Hidden;
                }

                Expander_MotionPath.IsExpanded = true;
                Expander_TargetPath.IsExpanded = false;
                Expander_Calculate.IsExpanded = true;
                Expander_Simulator.IsExpanded = true;
                Expander_Settings.IsExpanded = false;
                Expander_Scene.IsExpanded = false;
                Btn_Fullscreen.Content = "Fullscreen";
            }

        }

        private void UIGoToStreamTable()
        {
            UIGoToStreamTableEvent?.Invoke(this, new EventArgs());
        }

        private void UIGoToCommandsTable()
        {
            UIGoToCommandsTableEvent?.Invoke(this, new EventArgs());
        }

        private void UIGoToLiveControls()
        {
            UIGoToLiveControlsEvent?.Invoke(this, new EventArgs());
        }

        private void UIPointData_Clear()
        {
            tb_PointData_Id.Text = "0";
            tb_PointData_J1.Text = "0.000";
            tb_PointData_J2.Text = "0.000";
            tb_PointData_J3.Text = "0.000";
            tb_PointData_J4.Text = "0.000";
            tb_PointData_J5.Text = "0.000";
            tb_PointData_J6.Text = "0.000";
            tb_pointdata_acc.Text = "100";
            tb_pointdata_dec.Text = "100";
            tb_pointdata_vel.Text = "100";
            tb_pointdata_leave.Text = "100";
            tb_pointdata_reach.Text = "100";
            btn_PointData_Previous.IsEnabled = false;
            btn_PointData_Next.IsEnabled = false;
            btn_PointData_First.IsEnabled = false;
            btn_PointData_Last.IsEnabled = false;
        }

        private void UIState_Initial()
        {
            UIPointData_Clear();

            /// Motion Path
            btn_Add_Arc.IsEnabled = false;
            btn_Add_Linear.IsEnabled = false;
            btn_Save.IsEnabled = false;
            btn_Reset.IsEnabled = false;
            btn_SyncPaths.IsEnabled = false;
            btn_Calculate.IsEnabled = false;
            btn_ExportMode1.IsEnabled = false;
            btn_ExportMode2.IsEnabled = false;
            intctrl_ExportMode2_StreamFrequency.IsEnabled = false;

            /// Target Path
            btn_Target_AddDefault.IsEnabled = false;
            btn_Target_AddArc.IsEnabled = false;
            btn_Target_Reset.IsEnabled = false;
            btn_Target_Save.IsEnabled = false;
            btn_Target_Delete.IsEnabled = false;

            /// Offset parameter to camera configuration
            tb_OffsetGlobalX.Text = "0";
            tb_OffsetGlobalY.Text = "0";
            tb_OffsetGlobalZ.Text = "0";

            UIState_SimSliders_FKMode();
        }

        public void UIState_ActionsEnabled()
        {
            btn_PointData_Previous.IsEnabled = true;
            btn_PointData_Next.IsEnabled = true;
            btn_PointData_First.IsEnabled = true;
            btn_PointData_Last.IsEnabled = true;
            btn_Add_Default.IsEnabled = true;
        }

        public void UIState_ActionsDisabled()
        {
            btn_PointData_Previous.IsEnabled = false;
            btn_PointData_Next.IsEnabled = false;
            btn_PointData_First.IsEnabled = false;
            btn_PointData_Last.IsEnabled = false;
            btn_Add_Default.IsEnabled = false;
        }

        public void UIState_BeforeCalculate()
        {
            // Disable unnecessary buttons during calculating the trajectory info
            chk_EnableTracking.IsEnabled = false;
            btn_TrackingToggle.IsEnabled = false;
            btn_PointData_Previous.IsEnabled = false;
            btn_PointData_Next.IsEnabled = false;
            btn_PointData_First.IsEnabled = false;
            btn_PointData_Last.IsEnabled = false;
            btn_Add_Default.IsEnabled = false;
            btn_Calculate.Background = Brushes.Green;
            //btn_Calculate.IsEnabled = false;
            //SimPlayer.State_AllDisabled();
            //SimPlayer.PlayingPosition = 0;
            TotalLength = 0;
            if (IsSyncEnabled)
            {
                wasSyncEnabled = true;
                IsSyncEnabled = false;
            }
            if (IsPrismEnabled)
            {
                ShowPrism();
            }
        }

        public void UIState_AfterCalculate()
        {
            chk_EnableTracking.IsEnabled = true;
            btn_TrackingToggle.IsEnabled = true;
            btn_PointData_First.IsEnabled = true;
            btn_PointData_Previous.IsEnabled = true;
            //btn_PointData_Next.IsEnabled = true;
            //btn_PointData_Last.IsEnabled = true;
            btn_Add_Default.IsEnabled = true;
            //SimPlayer.PlayingPosition = 0;
            //SimPlayer.State_AllEnabled();
            btn_Calculate.Background = Brushes.Gray;
            btn_Calculate.IsEnabled = true;
            btn_SyncPaths.IsEnabled = true;

            bool bTemp = false;
            foreach (var curve in movePaths)
            {
                if (curve.curveShape is CurveType.BEZIER)
                {
                    btn_ExportMode1.IsEnabled = false;
                    btn_ExportMode2.IsEnabled = true;
                    intctrl_ExportMode2_StreamFrequency.IsEnabled = true;
                    bTemp = true;
                    break;
                }
            }
            if (!bTemp)
            {
                btn_ExportMode1.IsEnabled = true;
                btn_ExportMode2.IsEnabled = true;
                intctrl_ExportMode2_StreamFrequency.IsEnabled = true;
            }

            if (wasSyncEnabled)
            {
                IsSyncEnabled = true;
            }
        }

        private void UIJointSliders_SetLimits()
        {
            // Set Slider Min / Max Limits
            slider_J1.Minimum = RobotJoints[0].joint.angleMin;
            slider_J1.Maximum = RobotJoints[0].joint.angleMax;
            slider_J2.Minimum = RobotJoints[1].joint.angleMin;
            slider_J2.Maximum = RobotJoints[1].joint.angleMax;
            slider_J3.Minimum = RobotJoints[2].joint.angleMin;
            slider_J3.Maximum = RobotJoints[2].joint.angleMax;
            slider_J4.Minimum = RobotJoints[3].joint.angleMin;
            slider_J4.Maximum = RobotJoints[3].joint.angleMax;
            slider_J5.Minimum = RobotJoints[4].joint.angleMin;
            slider_J5.Maximum = RobotJoints[4].joint.angleMax;
            slider_J6.Minimum = RobotJoints[5].joint.angleMin;
            slider_J6.Maximum = RobotJoints[5].joint.angleMax;
        }

        public void UIState_SimSliders_FKMode()
        {
            Btn_AllowFKIK.Content = "F";
            grid_SimJointSliders.IsEnabled = false;
            grid_SimJointSliders.Background = null;
            grid_SimCartesianSliders.IsEnabled = true;
            grid_SimCartesianSliders.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color.FromArgb(30, 0, 200, 0)));
        }

        public void UIState_SimSliders_IKMode()
        {
            Btn_AllowFKIK.Content = "I";
            grid_SimJointSliders.IsEnabled = true;
            grid_SimJointSliders.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color.FromArgb(30, 0, 200, 0)));
            grid_SimCartesianSliders.IsEnabled = false;
            grid_SimCartesianSliders.Background = null;
        }

        private void slider_J1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (activeSlider == SliderName.J1)
            {
                OnPlanningMode_MoveSliderJoints(
                    slider_J1.Value,
                    slider_J2.Value,
                    slider_J3.Value,
                    slider_J4.Value,
                    slider_J5.Value,
                    slider_J6.Value
                );
            }
        }

        private void slider_J2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (activeSlider == SliderName.J2)
            {
                OnPlanningMode_MoveSliderJoints(
                    slider_J1.Value,
                    slider_J2.Value,
                    slider_J3.Value,
                    slider_J4.Value,
                    slider_J5.Value,
                    slider_J6.Value
                );
            }
        }

        private void slider_J3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (activeSlider == SliderName.J3)
            {
                OnPlanningMode_MoveSliderJoints(
                    slider_J1.Value,
                    slider_J2.Value,
                    slider_J3.Value,
                    slider_J4.Value,
                    slider_J5.Value,
                    slider_J6.Value
                );
            }
        }

        private void slider_J4_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (activeSlider == SliderName.J4)
            {
                OnPlanningMode_MoveSliderJoints(
                    slider_J1.Value,
                    slider_J2.Value,
                    slider_J3.Value,
                    slider_J4.Value,
                    slider_J5.Value,
                    slider_J6.Value
                );
            }
        }

        private void slider_J5_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (activeSlider == SliderName.J5)
            {
                OnPlanningMode_MoveSliderJoints(
                    slider_J1.Value,
                    slider_J2.Value,
                    slider_J3.Value,
                    slider_J4.Value,
                    slider_J5.Value,
                    slider_J6.Value
                );
            }
        }

        private void slider_J6_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (activeSlider == SliderName.J6)
            {
                OnPlanningMode_MoveSliderJoints(
                    slider_J1.Value,
                    slider_J2.Value,
                    slider_J3.Value,
                    slider_J4.Value,
                    slider_J5.Value,
                    slider_J6.Value
                );
            }
        }

        private void slider_X_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (activeSlider == SliderName.X)
            {
                OnPlanningMode_MoveSliderCartesian(
                    slider_X.Value,
                    slider_Y.Value,
                    slider_Z.Value,
                    slider_RX.Value,
                    slider_RY.Value,
                    slider_RZ.Value
                );
            }
        }

        private void slider_Y_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (activeSlider == SliderName.Y)
            {
                OnPlanningMode_MoveSliderCartesian(
                    slider_X.Value,
                    slider_Y.Value,
                    slider_Z.Value,
                    slider_RX.Value,
                    slider_RY.Value,
                    slider_RZ.Value
                );
            }
        }

        private void slider_Z_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (activeSlider == SliderName.Z)
            {
                OnPlanningMode_MoveSliderCartesian(
                    slider_X.Value,
                    slider_Y.Value,
                    slider_Z.Value,
                    slider_RX.Value,
                    slider_RY.Value,
                    slider_RZ.Value
                );
            }
        }

        private void slider_RX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (activeSlider == SliderName.RX && !IsTrackingEnabled)
            {
                OnPlanningMode_MoveSliderCartesian(
                    slider_X.Value,
                    slider_Y.Value,
                    slider_Z.Value,
                    slider_RX.Value,
                    slider_RY.Value,
                    slider_RZ.Value
                );
            }
        }

        private void slider_RY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (activeSlider == SliderName.RY && !IsTrackingEnabled)
            {
                OnPlanningMode_MoveSliderCartesian(
                    slider_X.Value,
                    slider_Y.Value,
                    slider_Z.Value,
                    slider_RX.Value,
                    slider_RY.Value,
                    slider_RZ.Value
                );
            }
        }

        private void slider_RZ_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (activeSlider == SliderName.RZ && !IsTrackingEnabled)
            {
                OnPlanningMode_MoveSliderCartesian(
                    slider_X.Value,
                    slider_Y.Value,
                    slider_Z.Value,
                    slider_RX.Value,
                    slider_RY.Value,
                    slider_RZ.Value
                );
            }
        }

        private void slider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ClearSelection();
        }

        private enum SliderName
        {
            NONE, J1, J2, J3, J4, J5, J6, X, Y, Z, RX, RY, RZ
        }

        private SliderName activeSlider = SliderName.NONE;

        private void slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            activeSlider = SliderName.NONE;
            //Log("Active Slider NONE");
        }

        private void slider_J1_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            activeSlider = SliderName.J1;
            //Log("Active Slider J1");
        }

        private void slider_J2_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            activeSlider = SliderName.J2;
            //Log("Active Slider J2");
        }

        private void slider_J3_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            activeSlider = SliderName.J3;
            //Log("Active Slider J3");
        }

        private void slider_J4_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            activeSlider = SliderName.J4;
            //Log("Active Slider J4");
        }

        private void slider_J5_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            activeSlider = SliderName.J5;
            //Log("Active Slider J5");
        }

        private void slider_J6_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            activeSlider = SliderName.J6;
            //Log("Active Slider J6");
        }

        private void slider_X_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            activeSlider = SliderName.X;
            //Log("Active Slider X");
        }

        private void slider_Y_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            activeSlider = SliderName.Y;
            //Log("Active Slider Y");
        }

        private void slider_Z_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            activeSlider = SliderName.Z;
            //Log("Active Slider Z");
        }

        private void slider_RX_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            activeSlider = SliderName.RX;
            //Log("Active Slider RX");
        }

        private void slider_RY_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            activeSlider = SliderName.RY;
            //Log("Active Slider RY");
        }

        private void slider_RZ_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            activeSlider = SliderName.RZ;
            //Log("Active Slider RZ");
        }

        private bool IsTargetPointUpdateLock = false;
        private void slider_target_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsSimInitialized) return;

            if (selectedVisual == SelectedVisualType.TargetPoint && !targetPoint.Manipulator.IsCaptured)
            {
                ClearManipulators();
                selectedVisual = SelectedVisualType.None;
            }
            if (!IsTargetPointUpdateLock)
            {
                SetTargetPoint_Position(new Vector3((float)slider_targetX.Value, (float)slider_targetY.Value, (float)slider_targetZ.Value));
            }
        }

        private void Btn_Pan3DEnable_Click(object sender, RoutedEventArgs e)
        {
            if (!IsPanControlEnabled)
            {
                Enable3DPanControl();
            }
            else
            {
                Disable3DPanControl();
            }
        }

        private void Btn_TracerClear_Click(object sender, RoutedEventArgs e)
        {
            Tracer_Clear();
        }

        //TODO Create PropDPfull
        private void Btn_SyncPaths_Click(object sender, RoutedEventArgs e)
        {
            btn_SyncPaths.IsEnabled = false;
            btn_SyncPaths.Background = Brushes.GreenYellow;
            Calculate_SyncPaths();
            btn_SyncPaths.Background = Brushes.LightGray;
            btn_SyncPaths.IsEnabled = true;
            btn_Calculate.IsEnabled = true;
        }

        private void Btn_Calculate_Click(object sender, RoutedEventArgs e)
        {
            Calculate_Start();
        }

        private void Btn_CalculateStop_Click(object sender, RoutedEventArgs e)
        {
            Calculate_Stop();
        }

        private void Btn_Add_Default_Click(object sender, RoutedEventArgs e)
        {
            //AddBezierDefault();
            if (robotMotionPath.bCurveSelected)
            {
                bCurveSelected = false;
                robotMotionPath.MakeBezier(nCurveSelected, true);
                UpdateTotalLength();
            }
            else
            {
                tb_PointData_Id.Text = Convert.ToString(jointArray.Count);
                tb_PointData_J1.Text = Convert.ToString(Math.Round(slider_J1.Value, 4));
                tb_PointData_J2.Text = Convert.ToString(Math.Round(slider_J2.Value, 4));
                tb_PointData_J3.Text = Convert.ToString(Math.Round(slider_J3.Value, 4));
                tb_PointData_J4.Text = Convert.ToString(Math.Round(slider_J4.Value, 4));
                tb_PointData_J5.Text = Convert.ToString(Math.Round(slider_J5.Value, 4));
                tb_PointData_J6.Text = Convert.ToString(Math.Round(slider_J6.Value, 4));

                //TODO: Quick Mode, add commands table linear moves 
                //UpdateTargetDistance();
                //AddTableRow(new double[] {
                //    slider_J1.Value, slider_J2.Value,
                //    slider_J3.Value, slider_J4.Value,
                //    slider_J5.Value, slider_J6.Value},
                //    CustomSpline.spacing / 10,
                //    0,
                //    targetDistance);

                double[] angles = {
                    slider_J1.Value,
                    slider_J2.Value,
                    slider_J3.Value,
                    slider_J4.Value,
                    slider_J5.Value,
                    slider_J6.Value
                };

                if (robotMotionPath.ptArray.Count > 1)
                    if (angles == robotMotionPath.ptArray[robotMotionPath.ptArray.Count - 1])
                        return;

                robotMotionPath.ptArray.Add(angles);
                velArray.Add(CustomSpline.spacing / 10);
                accelArray.Add(0.01);
                decelArray.Add(0.01);

                double[] temp = { cameraPoint.Position.X, cameraPoint.Position.Y, cameraPoint.Position.Z };

                if (robotMotionPath.ptArray.Count == 1 || robotMotionPath.ptArray.Count == 2)
                    robotMotionPath.AddPathPoint(0, temp, CurveType.BEZIER, PointType.POINT);
                else
                    robotMotionPath.AddPathPoint(robotMotionPath.CurveCount, temp, CurveType.BEZIER, PointType.POINT);

                if (robotMotionPath.ptArray.Count > 1)
                {
                    robotMotionPath.MakeBezier(robotMotionPath.ptArray.Count - 2, false);
                    //btn_Calculate.IsEnabled = true;
                    btn_SyncPaths.IsEnabled = true;
                    btn_Save.IsEnabled = true;
                    btn_PointData_Previous.IsEnabled = true;
                    btn_PointData_First.IsEnabled = true;

                    trajCurPoint = robotMotionPath.ptArray.Count - 1;
                    TotalLength += robotMotionPath.Paths[trajCurPoint - 1].lenCurve;
                }

                btn_Reset.IsEnabled = true;

            }
        }

        private void Btn_GoHome_Click(object sender, RoutedEventArgs e)
        {
            GoHomePosition();
        }

        private void Btn_SetTargetPoint_Click(object sender, RoutedEventArgs e)
        {
            SetTargetPoint_ToPrismRefPosition();
        }

        private void Btn_NewTargetKeyPoint_Click(object sender, RoutedEventArgs e)
        {
            SetTargetPoint_ToPrismRefPosition();
            targetPath.AddLinearDefault();
        }

        private void Btn_SimPointPrevious_Click(object sender, RoutedEventArgs e)
        {
            GoPreviousPathPoint();

            if (!btn_PointData_Next.IsEnabled)
                btn_PointData_Next.IsEnabled = true;
            if (!btn_PointData_Last.IsEnabled)
                btn_PointData_Last.IsEnabled = true;

            tb_PointData_Id.Text = Convert.ToString(trajCurPoint);
            tb_PointData_J1.Text = Convert.ToString(Math.Round(slider_J1.Value, 4));
            tb_PointData_J2.Text = Convert.ToString(Math.Round(slider_J2.Value, 4));
            tb_PointData_J3.Text = Convert.ToString(Math.Round(slider_J3.Value, 4));
            tb_PointData_J4.Text = Convert.ToString(Math.Round(slider_J4.Value, 4));
            tb_PointData_J5.Text = Convert.ToString(Math.Round(slider_J5.Value, 4));
            tb_PointData_J6.Text = Convert.ToString(Math.Round(slider_J6.Value, 4));

            if (IsFirstPoint)
            {
                btn_PointData_Previous.IsEnabled = false;
                btn_PointData_First.IsEnabled = false;
            }
        }

        private void Btn_SimPointNext_Click(object sender, RoutedEventArgs e)
        {
            GoNextPathPoint();

            if (!btn_PointData_Previous.IsEnabled)
                btn_PointData_Previous.IsEnabled = true;
            if (!btn_PointData_First.IsEnabled)
                btn_PointData_First.IsEnabled = true;

            tb_PointData_Id.Text = Convert.ToString(trajCurPoint);
            tb_PointData_J1.Text = Convert.ToString(Math.Round(slider_J1.Value, 4));
            tb_PointData_J2.Text = Convert.ToString(Math.Round(slider_J2.Value, 4));
            tb_PointData_J3.Text = Convert.ToString(Math.Round(slider_J3.Value, 4));
            tb_PointData_J4.Text = Convert.ToString(Math.Round(slider_J4.Value, 4));
            tb_PointData_J5.Text = Convert.ToString(Math.Round(slider_J5.Value, 4));
            tb_PointData_J6.Text = Convert.ToString(Math.Round(slider_J6.Value, 4));

            if (IsLastPoint)
            {
                btn_PointData_Next.IsEnabled = false;
                btn_PointData_Last.IsEnabled = false;
            }
        }

        private void Btn_SimPointFirst_Click(object sender, RoutedEventArgs e)
        {
            GoFirstPathPoint();
            btn_PointData_First.IsEnabled = false;
            btn_PointData_Previous.IsEnabled = false;
            btn_PointData_Next.IsEnabled = true;
            btn_PointData_Last.IsEnabled = true;
            tb_PointData_Id.Text = Convert.ToString(trajCurPoint);
        }

        private void Btn_SimPointLast_Click(object sender, RoutedEventArgs e)
        {
            GoLastPathPoint();
            btn_PointData_First.IsEnabled = true;
            btn_PointData_Previous.IsEnabled = true;
            btn_PointData_Next.IsEnabled = false;
            btn_PointData_Last.IsEnabled = false;
            tb_PointData_Id.Text = Convert.ToString(trajCurPoint);
        }

        private void Btn_Add_Linear_Click(object sender, RoutedEventArgs e)
        {
            if (bCurveSelected)
            {
                bCurveSelected = false;
                //makeLine(nCurveSelected);
                UpdateTotalLength();
            }
        }

        private void Btn_Add_Arc_Click(object sender, RoutedEventArgs e)
        {
            if (bCurveSelected)
            {
                bCurveSelected = false;
                //makeArc(nCurveSelected, false);
                UpdateTotalLength();
            }
        }

        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (bPointSelected)
            {
                if (robotMotionPath.CurveCount == 1)
                    btn_PointData_First.IsEnabled = false;
                else
                    btn_PointData_First.IsEnabled = true;
                btn_PointData_Previous.IsEnabled = false;
                btn_PointData_Next.IsEnabled = false;
                btn_PointData_Last.IsEnabled = false;
                deleteAndUpdate();
            }
        }

        private void Btn_Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void Btn_Reset_Click(object sender, RoutedEventArgs e)
        {
            ResetAll();
        }

        public void ResetAll()
        {
            UIPointData_Clear();
            btn_Reset.IsEnabled = false;
            TargetDistance = 0;
            TotalLength = 0;
            btn_Calculate.IsEnabled = false;
            btn_SyncPaths.IsEnabled = false;
            //SimPlayer.PlayPosition = 0;
            //SimPlayer.State_AllDisabled();
            btn_Save.IsEnabled = false;
            Reset();
        }

        private void Btn_Target_AddDefault_Click(object sender, RoutedEventArgs e)
        {
            //targetPath.AddBezierDefault();
            if (targetPath.bCurveSelected)
            {
                targetPath.bCurveSelected = false;
                targetPath.makeBezier(targetPath.nCurveSelected);
                targetPath.updateTotDistance();
            }
        }

        private void Btn_Target_AddLinear_Click(object sender, RoutedEventArgs e)
        {
            //targetPath.AddLinearDefault();
            if (bCurveSelected)
            {
                bCurveSelected = false;
                //makeLine(nCurveSelected);
                //updateTotDistance();

                /// New implementation
                targetMotionPath.MakeLine(nCurveSelected, true);
                tb_total_length_target.Text = Math.Round(targetMotionPath.GetTotLength(), 2).ToString();
            }
            else
            {
                double[] pt = new double[] { targetPoint.Position.X, targetPoint.Position.Y, targetPoint.Position.Z };
                if (targetMotionPath.ptArray.Count > 1)
                    if (pt == targetMotionPath.ptArray[targetMotionPath.ptArray.Count - 1])
                        return;
                targetMotionPath.ptArray.Add(pt);

                if (targetMotionPath.ptArray.Count == 1 || targetMotionPath.ptArray.Count == 2)
                {
                    targetMotionPath.AddPathPoint(0, pt, CurveType.LINE);
                    //addBezierPoint(0, pt);
                    if (targetMotionPath.ptArray.Count == 2)
                    {
                        targetMotionPath.AddPathPoint(0, new double[] { 0, 0, 0 }, CurveType.LINE, PointType.CONTROL);
                        targetMotionPath.AddPathPoint(0, new double[] { 0, 0, 0 }, CurveType.LINE, PointType.CONTROL);
                        //addBezierPoint(0, new double[] { 0, 0, 0 }, PointType.CONTROL);
                        //addBezierPoint(0, new double[] { 0, 0, 0 }, PointType.CONTROL);
                    }
                }
                else
                {
                    targetMotionPath.AddPathPoint(targetMotionPath.Paths.Count, pt, CurveType.LINE);
                    targetMotionPath.AddPathPoint(targetMotionPath.Paths.Count - 1, new double[] { 0, 0, 0 }, CurveType.LINE, PointType.CONTROL);
                    targetMotionPath.AddPathPoint(targetMotionPath.Paths.Count - 1, new double[] { 0, 0, 0 }, CurveType.LINE, PointType.CONTROL);
                    //addBezierPoint(curveCount, pt);
                    //addBezierPoint(curveCount - 1, new double[] { 0, 0, 0 }, PointType.CONTROL);
                    //addBezierPoint(curveCount - 1, new double[] { 0, 0, 0 }, PointType.CONTROL);
                }
                if (targetMotionPath.ptArray.Count > 1)
                {
                    targetMotionPath.MakeLine(targetMotionPath.ptArray.Count - 2, false);
                    //makeLine(targetMotionPath.ptArray.Count - 2, false);
                    btn_Target_Delete.IsEnabled = true;
                    btn_Target_Save.IsEnabled = true;

                    double dist = Convert.ToDouble(tb_total_length_target.Text);
                    dist += targetMotionPath.Paths[targetMotionPath.ptArray.Count - 2].lenCurve;
                    tb_total_length_target.Text = Math.Round(dist, 2).ToString();
                }
                btn_Target_Reset.IsEnabled = true;
            }
        }

        private void Btn_Target_AddArc_Click(object sender, RoutedEventArgs e)
        {
            if (targetPath.bCurveSelected)
            {
                targetPath.bCurveSelected = false;
                targetPath.makeArc(targetPath.nCurveSelected, false);
                targetPath.updateTotDistance();
            }
        }

        private void Btn_Target_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (targetPath.bPointSelected)
            {
                targetPath.deleteAndUpdate();
            }
        }

        private void Btn_Target_Save_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_Target_Open_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_Target_Reset_Click(object sender, RoutedEventArgs e)
        {
            btn_Target_Reset.IsEnabled = false;
            btn_Target_Delete.IsEnabled = false;
            btn_Target_AddArc.IsEnabled = false;
            btn_Target_AddDefault.IsEnabled = false;
            btn_Target_Save.IsEnabled = false;
            targetPath.reset();
        }

        private void Btn_AllowFKIK_Click(object sender, RoutedEventArgs e)
        {
            IsFKControlEnabled = !IsFKControlEnabled;
        }

        private void Btn_ModelLoad_Click(object sender, RoutedEventArgs e)
        {
            var str = "FilePath";
            ModelLoadFile(str);
        }

        private void Btn_ModelMoveToTarget_Click(object sender, RoutedEventArgs e)
        {
            ModelMove_TargetPoint();
        }

        private void Btn_ModelRotateZ_Click(object sender, RoutedEventArgs e)
        {
            ModelMove_RotateZ();
        }

        private void CameraOffset_Changed(object sender, RoutedEventArgs e)
        {
            if (tb_OffsetLocalX.Text == "" || tb_OffsetLocalY.Text == "" || tb_OffsetLocalZ.Text == "")
                return;
            CameraOffset = new Vector3(
                (float)Convert.ToDouble(tb_OffsetLocalX.Text),
                (float)Convert.ToDouble(tb_OffsetLocalY.Text),
                (float)Convert.ToDouble(tb_OffsetLocalZ.Text)
            );
        }

        private void Point_Data_Changed(object sender, RoutedEventArgs e)
        {
            //if (bPointSelected)
            //{
            //    if (tb_PointData_J1.Text == "" || tb_PointData_J2.Text == ""
            //        || tb_PointData_J3.Text == "" || tb_PointData_J4.Text == ""
            //        || tb_PointData_J5.Text == "" || tb_PointData_J6.Text == ""
            //        || tb_pointdata_vel.Text == "" || tb_pointdata_acc.Text == ""
            //        || tb_pointdata_dec.Text == "" || tb_pointdata_blendIn.Text == ""
            //        || tb_pointdata_blendOut.Text == "")
            //        return;

            //    double[] arrFK = robotSim.ForwardKinematics(
            //        Convert.ToDouble(tb_PointData_J1.Text),
            //        Convert.ToDouble(tb_PointData_J2.Text),
            //        Convert.ToDouble(tb_PointData_J3.Text),
            //        Convert.ToDouble(tb_PointData_J4.Text),
            //        Convert.ToDouble(tb_PointData_J5.Text),
            //        Convert.ToDouble(tb_PointData_J6.Text)
            //    );

            //    Vector3 ptOrigin = new Vector3(arrFK[0], arrFK[1], arrFK[2]);
            //    OnUpdateGraphics(ptOrigin);

            //    int nSelected = Convert.ToInt16(tb_PointData_Id.Text);
            //    velArray[nSelected] = Convert.ToDouble(tb_pointdata_vel.Text);
            //    accelArray[nSelected] = Convert.ToDouble(tb_pointdata_acc.Text);
            //    decelArray[nSelected] = Convert.ToDouble(tb_pointdata_dec.Text);

            //    StreamTable_UpdateRow(nSelected);
            //}

        }

        private void Btn_ExportMode1_Click(object sender, RoutedEventArgs e)
        {
            CommandsTable_Clear();
            if (IsCommandsTableClear)
            {
                for (int i = 0; i < movePaths.Count; i++)
                {
                    // First Point
                    double[] temp = trajectory[i][0].jointAngles;
                    double[] firstPt = RobotKinematics.ForwardKinematics(temp[0], temp[1], temp[2], temp[3], temp[4], temp[5]);

                    // Last Point
                    temp = trajectory[i][trajectory[i].Count - 1].jointAngles;
                    double[] lastPt = RobotKinematics.ForwardKinematics(temp[0], temp[1], temp[2], temp[3], temp[4], temp[5]);

                    // Mid Point
                    //double[] midPt = new double[6]{ movePaths[i].ptAdjust[0].Transform.Value.OffsetX, movePaths[i].ptAdjust[0].Transform.Value.OffsetY, movePaths[i].ptAdjust[0].Transform.Value.OffsetZ, 0, 0, 0};
                    temp = trajectory[i][(trajectory[i].Count - 1) / 2].jointAngles;
                    double[] midPt = RobotKinematics.ForwardKinematics(temp[0], temp[1], temp[2], temp[3], temp[4], temp[5]);

                    var vel = Settings.Default.SIM_EXP1_VEL;
                    var acc = Settings.Default.SIM_EXP1_ACC;
                    var dec = Settings.Default.SIM_EXP1_DEC;
                    var focusPos = trajectory[i][0].distToTarget;
                    var irisPos = 0.0;
                    var zoomPos = 0.0;
                    var auxMotPos = 0.0;

                    CommandsMoveType moveType;
                    if (movePaths[i].curveShape == CurveType.LINE)
                        moveType = CommandsMoveType.LINEAR;
                    else if (movePaths[i].curveShape == CurveType.ARC)
                        moveType = CommandsMoveType.CIRCULAR;
                    else
                    {
                        // Error not a Segment or Arc.
                        ErrorLog($"Simulator Export 1 Error Adding Command Row: Invalid Move Type.");
                        break;
                    }

                    // Start Point
                    if (i == 0)
                    {
                        CommandsTable_AddRow(
                            CommandsMoveType.START,
                            new CartesianPos(firstPt[0], firstPt[1], firstPt[2], firstPt[3], firstPt[4], firstPt[5]),
                            new CartesianPos(lastPt[0], lastPt[1], lastPt[2], lastPt[3], lastPt[4], lastPt[5]),
                            new CartesianPos(midPt[0], midPt[1], midPt[2], midPt[3], midPt[4], midPt[5]),
                            vel,
                            acc,
                            dec,
                            Math.Round(focusPos, 2),
                            Math.Round(irisPos, 2),
                            Math.Round(zoomPos, 2),
                            Math.Round(auxMotPos, 2)
                        );
                    }
                    //Log($"Add Command Row - CurveType: {moveType.ToString()}");
                    CommandsTable_AddRow(
                        moveType,
                        new CartesianPos(firstPt[0], firstPt[1], firstPt[2], firstPt[3], firstPt[4], firstPt[5]),
                        new CartesianPos(lastPt[0], lastPt[1], lastPt[2], lastPt[3], lastPt[4], lastPt[5]),
                        new CartesianPos(midPt[0], midPt[1], midPt[2], midPt[3], midPt[4], midPt[5]),
                        vel,
                        acc,
                        dec,
                        Math.Round(focusPos, 2),
                        Math.Round(irisPos, 2),
                        Math.Round(zoomPos, 2),
                        Math.Round(auxMotPos, 2)
                    );
                }
                IsCommandsTableClear = false;
                UIGoToCommandsTable();
            }
        }

        private void Btn_ExportMode2_Click(object sender, RoutedEventArgs e)
        {
            StreamTable_Clear();
            if (IsStreamTableClear)
            {
                int frequency = Convert.ToInt16(intctrl_ExportMode2_StreamFrequency.Value);

                bool first = false;
                bool last = false;

                int segTotal = trajectory.Count;
                int segNum = 0;
                foreach (var segment in trajectory)
                {
                    if (segNum == 0) first = true; // First 

                    int curSegJointsTotal = segment.Count;
                    int curJoint = 0;

                    foreach (var joints in segment)
                    {
                        if ((segNum == segTotal - 1) && (curJoint == curSegJointsTotal - 1)) last = true; // Last

                        if (curJoint % frequency == 0 || first || last)
                        {
                            StreamTable_AddRow(
                                joints.jointAngles,
                                Convert.ToInt32(tb_pointdata_vel.Text),
                                Convert.ToInt32(tb_pointdata_acc.Text),
                                Convert.ToInt32(tb_pointdata_dec.Text),
                                Convert.ToInt32(tb_pointdata_leave.Text),
                                Convert.ToInt32(tb_pointdata_reach.Text),
                                joints.distToTarget
                            );
                            first = false;
                            last = false;
                        }
                        curJoint++;
                    }

                    segNum++;
                }
                IsStreamTableClear = false;
                UIGoToStreamTable();
            }
        }

        private void chk_ShowControlPoints_Changed(object sender, RoutedEventArgs e)
        {
            if (chk_ShowControlPoints.IsChecked.Value)
            {
                IsControlPointsEnabled = true;
                ShowControlPoints();
            }
            else
            {
                IsControlPointsEnabled = false;
                HideControlPoints();
            }
        }

        private void chk_ShowTrackingLine_Changed(object sender, RoutedEventArgs e)
        {

            if (chk_ShowTrackingLine.IsChecked.Value)
            {
                IsTrackingLineEnabled = true;
                ShowTrackingLine();
            }
            else
            {
                IsTrackingLineEnabled = false;
                HideTrackingLine();
            }
        }

        private void chk_LinkHandle_Changed(object sender, RoutedEventArgs e)
        {
            IsLinkHandle = chk_LinkHandle.IsChecked.Value;
        }

        private void chk_ShowRobotArm_Changed(object sender, RoutedEventArgs e)
        {
            if (chk_ShowRobotArm.IsChecked.Value)
            {
                IsShowRobotArm = true;
                ShowRobotArm();
            }
            else
            {
                IsShowRobotArm = false;
                HideRobotArm();
            }
        }

        private void chk_ShowBoundingBox_Changed(object sender, RoutedEventArgs e)
        {
            if (chk_ShowBoundingBox.IsChecked.Value)
            {
                IsBoundingBoxEnabled = true;
                ShowCamBounds();
            }
            else
            {
                IsBoundingBoxEnabled = false;
                HideCamBounds();
            }
        }

        private void chk_ShowPrism_Changed(object sender, RoutedEventArgs e)
        {
            if (chk_ShowPrism.IsChecked.Value)
            {
                IsPrismEnabled = true;
                ShowPrism();
            }
            else
            {
                IsPrismEnabled = false;
                HidePrism();
            }
        }

        private void chk_ShowBezierLabel_Changed(object sender, RoutedEventArgs e)
        {
            if (chk_ShowBezierLabel.IsChecked.Value)
            {
                IsBezierLabelEnabled = true;
                ShowPointLabels();
            }
            else
            {
                IsBezierLabelEnabled = false;
                HidePointLabels();
            }
        }

        private void chk_ShowLimitSphere_Changed(object sender, RoutedEventArgs e)
        {
            if (chk_ShowLimitSphere.IsChecked.Value)
            {
                IsLimitSphereEnabled = true;
                ShowLimitSphere();
            }
            else
            {
                IsLimitSphereEnabled = false;
                HideLimitSphere();
            }
        }

        private void chk_PlayRealtime_Changed(object sender, RoutedEventArgs e)
        {
            IsPlayRealtime = chk_PlayRealtime.IsChecked.Value;
        }

        #region PLAYER (INTERNAL)

        private void Initialize_SimPlayer()
        {
            //SimPlayer.Duration = Duration;
            //SimPlayer.PlayingPosition = 0;
            //SimPlayer.PlayForwardEvent += Player_PlayForwardHandler;
            //SimPlayer.PlayBackwardEvent += Player_PlayBackwardHandler;
            //SimPlayer.StopEvent += Player_StopHandler;
            //SimPlayer.PauseEvent += Player_PauseHandler;
            //SimPlayer.StepForwardEvent += Player_StepForwardHandler;
            //SimPlayer.StepBackwardEvent += Player_StepBackwardHandler;
            //SimPlayer.GoEndEvent += Player_GoEndHandler;
            //SimPlayer.GoStartEvent += Player_GoStartHandler;
            //SimPlayer.State_AllDisabled();
        }

        private void Player_PlayForwardHandler(object sender, RoutedEventArgs e)
        {
            PlaybackForward();
        }

        private void Player_PlayBackwardHandler(object sender, RoutedEventArgs e)
        {
            PlaybackBackward();
        }

        private void Player_PauseHandler(object sender, RoutedEventArgs e)
        {
            PlaybackPause();
        }

        private void Player_StopHandler(object sender, RoutedEventArgs e)
        {
            PlaybackStop();
        }

        private void Player_StepForwardHandler(object sender, RoutedEventArgs e)
        {
            PlaybackStepForward();
        }

        private void Player_StepBackwardHandler(object sender, RoutedEventArgs e)
        {
            PlaybackStepReverse();
        }

        private void Player_GoEndHandler(object sender, RoutedEventArgs e)
        {
            GoLastPathPoint();
        }

        private void Player_GoStartHandler(object sender, RoutedEventArgs e)
        {
            GoFirstPathPoint();
        }

        #endregion

        private void Sim_StepSize_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (IsSimInitialized)
                StepSize = Sim_StepSize.Value;
        }

        private void CameraHomeButton_Click(object sender, RoutedEventArgs e)
        {
            CameraHome();
        }

        private void CameraFrontButton_Click(object sender, RoutedEventArgs e)
        {
            CameraFront();
        }

        private void CameraBackButton_Click(object sender, RoutedEventArgs e)
        {
            CameraBack();
        }

        private void CameraRightButton_Click(object sender, RoutedEventArgs e)
        {
            CameraRight();
        }

        private void CameraLeftButton_Click(object sender, RoutedEventArgs e)
        {
            CameraLeft();
        }

        private void CameraTopButton_Click(object sender, RoutedEventArgs e)
        {
            CameraTop();
        }

        private void CameraBottomButton_Click(object sender, RoutedEventArgs e)
        {
            CameraBottom();
        }

        private void SceneTestButton_Click(object sender, RoutedEventArgs e)
        {
            SceneTest();
        }

        #endregion

        private void camOffsetSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //OnPlanningMode_MoveSliderJoints(
            //    slider_J1.Value,
            //    slider_J2.Value,
            //    slider_J3.Value,
            //    slider_J4.Value,
            //    slider_J5.Value,
            //    slider_J6.Value,
            //    camSlider_X.Value,
            //    camSlider_Y.Value,
            //    camSlider_Z.Value,
            //    camSlider_RZ.Value
            //);
        }

        private void cb_camRot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_camRotZ == null) return;

            double[] angles = { 0, 90, 180, 270 };
            OnPlanningMode_MoveSliderJoints(
                slider_J1.Value,
                slider_J2.Value,
                slider_J3.Value,
                slider_J4.Value,
                slider_J5.Value,
                slider_J6.Value,
                Convert.ToDouble(tb_camOffset_x.Text),
                Convert.ToDouble(tb_camOffset_y.Text),
                Convert.ToDouble(tb_camOffset_z.Text),
                angles[cb_camRotX.SelectedIndex],
                angles[cb_camRotY.SelectedIndex],
                angles[cb_camRotZ.SelectedIndex]
            );
        }

        private void tb_camSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cb_camRotZ == null || tb_pos_j6 == null) return;

            double[] angles = { 0, 90, 180, 270 };
            OnPlanningMode_MoveSliderJoints(
                Convert.ToDouble(tb_pos_j1.Text),
                Convert.ToDouble(tb_pos_j2.Text),
                Convert.ToDouble(tb_pos_j3.Text),
                Convert.ToDouble(tb_pos_j4.Text),
                Convert.ToDouble(tb_pos_j5.Text),
                Convert.ToDouble(tb_pos_j6.Text),
                Convert.ToDouble(tb_camOffset_x.Text),
                Convert.ToDouble(tb_camOffset_y.Text),
                Convert.ToDouble(tb_camOffset_z.Text),
                angles[cb_camRotX.SelectedIndex],
                angles[cb_camRotY.SelectedIndex],
                angles[cb_camRotZ.SelectedIndex]
            );
        }

        private void Btn_SetSetting_Click(object sender, RoutedEventArgs e)
        {
            switch (CameraMount)
            {
                case ToolPlacement.BOTTOM:
                    RobotKinematics.HomeBottomJointPos = new JointPos(
                        Convert.ToDouble(tb_pos_j1.Text),
                        Convert.ToDouble(tb_pos_j2.Text),
                        Convert.ToDouble(tb_pos_j3.Text),
                        Convert.ToDouble(tb_pos_j4.Text),
                        Convert.ToDouble(tb_pos_j5.Text),
                        Convert.ToDouble(tb_pos_j6.Text));
                    break;
                case ToolPlacement.FRONT:
                    RobotKinematics.HomeFrontJointPos = new JointPos(
                        Convert.ToDouble(tb_pos_j1.Text),
                        Convert.ToDouble(tb_pos_j2.Text),
                        Convert.ToDouble(tb_pos_j3.Text),
                        Convert.ToDouble(tb_pos_j4.Text),
                        Convert.ToDouble(tb_pos_j5.Text),
                        Convert.ToDouble(tb_pos_j6.Text));
                    break;
            }
            
            double[] angles = { 0, 90, 180, 270 };
            RobotKinematics.CameraOffsetConfig = new CartesianPos(
                Convert.ToDouble(tb_camOffset_x.Text),
                Convert.ToDouble(tb_camOffset_y.Text),
                Convert.ToDouble(tb_camOffset_z.Text),
                angles[cb_camRotX.SelectedIndex],
                angles[cb_camRotY.SelectedIndex],
                angles[cb_camRotZ.SelectedIndex]);
        }
    }

}
