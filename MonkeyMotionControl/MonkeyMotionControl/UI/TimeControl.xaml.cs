using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using static MonkeyMotionControl.UI.IntegerControl;

namespace MonkeyMotionControl.UI
{
    /// <summary>
    /// Interaction logic for TimeControl.xaml
    /// </summary>
    [DefaultProperty(nameof(Value))]
    [DefaultEvent(nameof(ValueChanged))]
    public partial class TimeControl : UserControl
    {
        #region ValueChanged

        public static readonly RoutedEvent ValueChangedRoutedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueChanged), RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(TimeControl));

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedRoutedEvent, value); }
            remove { RemoveHandler(ValueChangedRoutedEvent, value); }
        }

        #endregion

        public TimeControl()
        {
            InitializeComponent();
        }

        #region Dependency Properties

        #region Value

        /// <summary>
        /// Time value of this control, in seconds.
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Value dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
                nameof(Value), typeof(double), typeof(TimeControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnValueChanged), new CoerceValueCallback(CoerceValue)));

        private static object CoerceValue(DependencyObject element, object value)
        {
            var control = (TimeControl)element;
            return Math.Max(control.Minimum, Math.Min(control.Maximum, (double)value));
        }

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as TimeControl).OnValueChanged(
                new RoutedPropertyChangedEventArgs<double>((double)args.OldValue, (double)args.NewValue, ValueChangedRoutedEvent));
        }

        /// <summary>
        /// Called when the time value of the control has changed.
        /// </summary>
        protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<double> e)
        {
            var time = TimeSpan.FromSeconds(Value);

            HourLabel.Value = time.Hours;
            MinuteLabel.Value = time.Minutes;
            SecondLabel.Value = time.Seconds;
            MillisecondLabel.Value = time.Milliseconds;

            RaiseEvent(e);
        }

        #endregion

        #region Minimum

        /// <summary>
        /// The minimum value in seconds allowed for this control.
        /// </summary>
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(TimeControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnMinimumChanged)));

        private static void OnMinimumChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as TimeControl;
            control.SetValue(ValueProperty, Math.Max((double)args.NewValue, control.Value));
        }

        #endregion

        #region Maximum

        /// <summary>
        /// The maximum value in seconds allowed for this control.
        /// </summary>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(TimeControl),
                new FrameworkPropertyMetadata(TimeSpan.MaxValue.TotalSeconds, new PropertyChangedCallback(OnMaximumChanged)));

        private static void OnMaximumChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as TimeControl;
            control.SetValue(ValueProperty, Math.Min((double)args.NewValue, control.Value));
        }

        #endregion

        #endregion

        private void IntegerControl_ValueChanged(object sender, RoutedEventArgs e)
        {
            var newValue = GetCurrentTime();
            var coercedValue = Math.Max(Minimum, Math.Min(Maximum, newValue));
            if (coercedValue != Value)
                Value = coercedValue;
        }

        /// <summary>
        /// Gets the current time shown in the control.
        /// </summary>
        /// <returns></returns>
        protected double GetCurrentTime()
        {
            return SecondLabel.Value +
                (MinuteLabel.Value * 60.0) +
                (HourLabel.Value * 3600.0) +
                MillisecondLabel.Value / 1000.0;
        }

        private void IntegerControl_ValueOverflown(object sender, RoutedEventArgs args)
        {
            if (args is ValueOverflownRoutedEventArgs e)
            {
                var ctl = sender as IntegerControl;
                if (ctl != HourLabel)   // The hour component cannot overflow, so skip it.
                {
                    var time = TimeSpan.FromSeconds(GetCurrentTime());
                    var overflown = e.OverflownAmount;
                    if (ctl == MillisecondLabel)
                    {
                        if (overflown < 0 && Math.Floor(time.TotalSeconds) == 0.0) return;
                        time = time.Add(TimeSpan.FromMilliseconds(e.OverflownAmount));
                    }
                    else if (ctl == SecondLabel)
                    {
                        if (overflown < 0 && Math.Floor(time.TotalMinutes) == 0.0) return;
                        time = time.Add(TimeSpan.FromSeconds(e.OverflownAmount));
                    }
                    else if (ctl == MinuteLabel)
                    {
                        if (overflown < 0 && Math.Floor(time.TotalHours) == 0.0) return;
                        time = time.Add(TimeSpan.FromMinutes(e.OverflownAmount));
                    }
                    Value = time.TotalSeconds;
                }
            }
        }

    }
}
