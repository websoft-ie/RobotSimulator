using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MonkeyMotionControl.UI
{
    /// <summary>
    /// Interaction logic for NumericBox.xaml
    /// </summary>
    public partial class NumericBox : UserControl
    {
        #region Field

        /// <summary>
        /// The string format used for showing the value into the box.
        /// </summary>
        string numberFormat = "{0}";

        #endregion

        public NumericBox()
        {
            InitializeComponent();
        }

        #region Properties

        #region Value

        /// <summary>
        /// The numeric value assigned for this control.
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Value dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value), typeof(double), typeof(NumericBox),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(ValueChangedCallback),
                                              new CoerceValueCallback(CoerceValue)));

        private static object CoerceValue(DependencyObject element, object value)
        {
            NumericBox control = (NumericBox)element;
            return Math.Max(control.Minimum, Math.Min(control.Maximum, Math.Round((double)value, control.DecimalPlaces)));
        }

        private static void ValueChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as NumericBox).OnValueChanged(new RoutedPropertyChangedEventArgs<double>(
                (double)args.OldValue, (double)args.NewValue, ValueChangedEvent));
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueChanged), RoutingStrategy.Direct,
            typeof(RoutedPropertyChangedEventHandler<double>), typeof(NumericBox));

        /// <summary>
        /// Occurs when the Value property changes.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<double> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }
       
        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            UpdateText();
            RaiseEvent(args);
        }

        #endregion

        #region Minimum

        /// <summary>
        /// The minimum numeric value allowed for this control.
        /// </summary>
        public double Minimum
        {
            get { return (double)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(NumericBox),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnMinValueChanged)));

        private static void OnMinValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NumericBox control = (NumericBox)obj;
            double minValue = (double)args.NewValue;
            control.SetValue(ValueProperty, Math.Max(minValue, control.Value));
        }

        #endregion

        #region Maximum

        /// <summary>
        /// The maximum numeric value allowed for this control.
        /// </summary>
        public double Maximum
        {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(NumericBox),
                new FrameworkPropertyMetadata(double.MaxValue, new PropertyChangedCallback(OnMaxValueChanged)));

        private static void OnMaxValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NumericBox control = (NumericBox)obj;
            double maxValue = (double)args.NewValue;
            control.SetValue(ValueProperty, Math.Min(maxValue, control.Value));
        }

        #endregion

        #region Small Step

        /// <summary>
        /// The increment amount on button click.
        /// </summary>
        public double SmallStep
        {
            get { return (double)GetValue(SmallStepProperty); }
            set { SetValue(SmallStepProperty, value); }
        }

        public static readonly DependencyProperty SmallStepProperty =
            DependencyProperty.Register(nameof(SmallStep), typeof(double), typeof(NumericBox),
                new FrameworkPropertyMetadata(1.0));

        #endregion

        #region Large Step

        /// <summary>
        /// The increment amount on mouse wheel.
        /// </summary>
        public double LargeStep
        {
            get { return (double)GetValue(LargeStepProperty); }
            set { SetValue(LargeStepProperty, value); }
        }

        public static readonly DependencyProperty LargeStepProperty =
            DependencyProperty.Register(nameof(LargeStep), typeof(double), typeof(NumericBox),
                new FrameworkPropertyMetadata(10.0));

        #endregion

        #region Decimal Places

        public int DecimalPlaces
        {
            get { return (int)GetValue(DecimalPlacesProperty); }
            set { SetValue(DecimalPlacesProperty, value); }
        }

        public static readonly DependencyProperty DecimalPlacesProperty =
            DependencyProperty.Register(nameof(DecimalPlaces), typeof(int), typeof(NumericBox),
                new FrameworkPropertyMetadata(2, new PropertyChangedCallback(DecimalPlacesChanged),
                    new CoerceValueCallback(CoerceDecimalPlaces)));

        private static object CoerceDecimalPlaces(DependencyObject element, object value)
        {
            return Math.Max(0, (int)value);
        }

        private static void DecimalPlacesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as NumericBox).OnDecimalPlacesChanged();
        }

        protected virtual void OnDecimalPlacesChanged()
        {
            int value = Math.Max(0, DecimalPlaces);
            numberFormat = value == 0 ? "{0}" : $"{{0:0.{new string('0', value)}}}";
            UpdateText();
        }

        #endregion

        #endregion

        #region UI Event Handlers

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            Value += SmallStep;
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            Value -= SmallStep;
        }

        private void TypeBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!double.TryParse(e.Text, out double value))
                e.Handled = true;
            else if (!(e.Handled = (value < Minimum || value > Maximum)))
                Value = value;
        }

        private void TypeBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Value += LargeStep * Math.Sign(e.Delta);
        }

        private void TypeBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TypeBox.SelectAll();
        }

        #endregion

        /// <summary>
        /// Sets the width size to fit for the specified range with the specified decimal places.
        /// </summary>
        /// <param name="range">The range to show fitting.</param>
        /// <param name="decimalPlaces">The number of places to make room for.</param>
        public void SetWidthForRange(Range range, int decimalPlaces)
        {
            var iv = System.Globalization.CultureInfo.InvariantCulture;
                
            var oldText = TypeBox.Text;
            var min = range.Min.ToString(iv);
            var dotIndex = min.IndexOf('.');
            if (dotIndex != -1)
                min = min.Substring(0, dotIndex);
            min += "." + new string('0', decimalPlaces);
            TypeBox.Text = min;
            var rect = TypeBox.GetRectFromCharacterIndex(min.Length);
            var width = Math.Max(MinWidth, rect.Right);
            TypeBox.Text = min;
            rect = TypeBox.GetRectFromCharacterIndex(min.Length);
            width = Math.Max(width, rect.Right);
            TypeBox.Text = oldText;
            Width = width + Math.Max(13, MainGrid.ColumnDefinitions[1].ActualWidth) + 4;
        }

        /// <summary>
        /// Updates the current visual representation of the value.
        /// </summary>
        private void UpdateText()
        {
            TypeBox.Text = string.Format(numberFormat, Value);
        }

    }
}
