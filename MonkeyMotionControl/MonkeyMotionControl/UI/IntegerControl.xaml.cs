using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MonkeyMotionControl.UI
{
    /// <summary>
    /// Interaction logic for IntegerControl.xaml
    /// </summary>
    public partial class IntegerControl : UserControl
    {
        #region Events

        #region ValueChanged

        public class ValueOverflownRoutedEventArgs : RoutedEventArgs
        {
            public ValueOverflownRoutedEventArgs(int overflowValue, RoutedEvent routedEvent)
                : base(routedEvent)
            {
                OverflownAmount = overflowValue;
            }

            public int OverflownAmount { get; }

        }

        public static readonly RoutedEvent ValueChangedRoutedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueChanged), RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(IntegerControl));

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedRoutedEvent, value); }
            remove { RemoveHandler(ValueChangedRoutedEvent, value); }
        }

        #endregion

        #region ValueOverflown

        public static readonly RoutedEvent ValueOverflownRoutedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueOverflown), RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(IntegerControl));

        public event RoutedEventHandler ValueOverflown
        {
            add { AddHandler(ValueOverflownRoutedEvent, value); }
            remove { RemoveHandler(ValueOverflownRoutedEvent, value); }
        }

        #endregion

        #endregion

        /// <summary>
        /// Protected field member to store digit count string format.
        /// </summary>
        protected string numberFormat = "0";

        public IntegerControl()
        {
            InitializeComponent();
        }

        #region Dependency Properties

        #region Value

        /// <summary>
        /// Gets or sets the value assigned to the control.
        /// </summary>
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Value dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
                nameof(Value), typeof(int), typeof(IntegerControl),
                new FrameworkPropertyMetadata(0,
                    new PropertyChangedCallback(OnValueChanged),
                    new CoerceValueCallback(CoerceValue)));

        private static object CoerceValue(DependencyObject element, object value)
        {
            var control = element as IntegerControl;
            return Math.Max(control.Minimum, Math.Min(control.Maximum, (int)value));
        }

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            //var control = obj as IntegerControl;
            //var v = (int)args.NewValue;
            //int max = control.Maximum;
            //var min = control.Minimum;
            //if (v < min)
            //    control.OnValueOverflown(v - min);
            //else if (v > max)
            //    control.OnValueOverflown(v - max);
            //else

            (obj as IntegerControl).OnValueChanged(new RoutedPropertyChangedEventArgs<int>(
                    (int)args.OldValue, (int)args.NewValue, ValueChangedRoutedEvent));
        }

        protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<int> e)
        {
            UpdateLabel();
            RaiseEvent(e);
        }

        protected virtual void OnValueOverflown(int overflowValue)
        {
            RaiseEvent(new ValueOverflownRoutedEventArgs(overflowValue, ValueOverflownRoutedEvent));
        }

        #endregion

        #region Minimum

        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(int), typeof(IntegerControl),
                new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnMinimumChanged)));

        private static void OnMinimumChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            IntegerControl control = (IntegerControl)obj;
            int minValue = (int)args.NewValue;
            if (control.Value < minValue)
                control.Value = minValue;
        }

        #endregion

        #region Maximum

        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(int), typeof(IntegerControl),
                new FrameworkPropertyMetadata(99, new PropertyChangedCallback(OnMaximumChanged)));

        private static void OnMaximumChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as IntegerControl;
            int maxValue = (int)args.NewValue;
            if (control.Value > maxValue) control.Value = maxValue;
        }

        #endregion

        #region DigitsShown

        /// <summary>
        /// The number of '0' digits shown for right alignment.
        /// </summary>
        public int DigitsShown
        {
            get { return (int)GetValue(DigitsShownProperty); }
            set { SetValue(DigitsShownProperty, (int)value); }
        }

        public static readonly DependencyProperty DigitsShownProperty =
            DependencyProperty.Register(nameof(DigitsShown), typeof(int), typeof(IntegerControl),
                new FrameworkPropertyMetadata(1,
                    new PropertyChangedCallback(OnDigitsShownChanged),
                    new CoerceValueCallback(CoerceDigitsShown)));

        private static object CoerceDigitsShown(DependencyObject element, object value)
        {
            return Math.Max(1, (int)value);
        }

        private static void OnDigitsShownChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            IntegerControl control = (IntegerControl)obj;
            control.numberFormat = $"{{0:{new string('0', (int)args.NewValue)}}}";
            control.UpdateLabel();
        }

        #endregion

        #endregion

        protected void UpdateLabel()
        {
            ValueBox.Text = string.Format(numberFormat, Value);
        }

        #region UI Event Handlers

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var oldValue = Value;
            var newValue = oldValue + Math.Sign(e.Delta);
            if (newValue < Minimum)
                OnValueOverflown(newValue - Minimum);
            else if (newValue > Maximum)
                OnValueOverflown(newValue - Maximum);
            else
                Value = newValue;
        }

        private void ValueBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var c = (char)e.Key;

            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                //e.Handled = true;

            }
            else if (!(e.Handled = !char.IsControl(c) || !char.IsDigit(c)))
            {

            }
        }

        private void ValueBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(ValueBox.Text, out int res))
            {
                if (res != Value)
                    Value = res;
            } else
                UpdateLabel();
        }

        #endregion

    }
}
