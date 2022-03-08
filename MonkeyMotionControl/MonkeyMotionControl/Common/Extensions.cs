using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
//using MonkeyMotionControl.Timeline;

namespace MonkeyMotionControl
{
    using System.Globalization;
    using System.Windows.Data;
    using Win32;

    /// <summary>
    /// Encapsulates the class extensions used in this project.
    /// </summary>
    //public static class Extensions
    //{
    //    #region Math Extensions

    //    public static bool IsEqualTo(this double value, double other, double resolution)
    //    {
    //        return Math.Abs(other - value) < resolution;
    //    }

    //    public static bool IsLessThan(this double value, double other, double resolution)
    //    {
    //        return (other - value) > resolution;
    //    }

    //    public static bool IsGreaterThan(this double value, double other, double resolution)
    //    {
    //        return (other - value) < resolution;
    //    }

    //    #endregion

    //    #region Time Extensions

    //    /// <summary>
    //    /// Gets the floor amount of units for the specified timescale.
    //    /// </summary>
    //    /// <param name="scale">The time scale to get the units.</param>
    //    /// <returns>The floor amount of units for the specified timescale.</returns>
    //    public static int Floor(this TimeSpan timeSpan, TimeScale scale)
    //    {
    //        switch (scale)
    //        {
    //            case TimeScale.Hour:
    //                return timeSpan.Hours;
    //            case TimeScale.Minute:
    //                return (int)Math.Floor(timeSpan.TotalMinutes); // (timeSpan.Hours * 60.0) + timeSpan.Minutes;
    //            case TimeScale.Second:
    //                return (int)Math.Floor(timeSpan.TotalSeconds); // (timeSpan.Hours * 3600.0) + (timeSpan.Minutes * 60.0) + timeSpan.Seconds;
    //            case TimeScale.Millisecond:
    //                return (int)Math.Floor(timeSpan.TotalMilliseconds);
    //            default:
    //                throw new ArgumentOutOfRangeException($"TimeScale not implemented: {scale}");
    //        }
    //    }

    //    /// <summary>
    //    /// Gets the ceiling amount of units for the specified timescale.
    //    /// </summary>
    //    /// <param name="scale">The time scale to get the units.</param>
    //    /// <returns>The ceiling amount of units for the specified timescale.</returns>
    //    public static int Ceiling(this TimeSpan timeSpan, TimeScale scale)
    //    {
    //        switch (scale)
    //        {
    //            case TimeScale.Hour:
    //                return (int)Math.Ceiling(timeSpan.TotalHours);
    //            case TimeScale.Minute:
    //                return (int)Math.Ceiling(timeSpan.TotalMinutes);
    //            case TimeScale.Second:
    //                return (int)Math.Ceiling(timeSpan.TotalSeconds);
    //            case TimeScale.Millisecond:
    //                return (int)Math.Ceiling(timeSpan.TotalMilliseconds);
    //            default:
    //                throw new ArgumentOutOfRangeException($"TimeScale not implemented: {scale}");
    //        }
    //    }

    //    #endregion

    //    #region Point Extensions

    //    /// <summary>
    //    /// Adds this point to the other and return the result as a point.
    //    /// </summary>
    //    /// <param name="A">This point.</param>
    //    /// <param name="B">The point to add to this point.</param>
    //    /// <returns>The addition of the 2 points.</returns>
    //    public static Point Add(this Point A, Point B)
    //    {
    //        return new Point(B.X + A.X, B.Y + A.Y);
    //    }

    //    /// <summary>
    //    /// Computes the distance from this point to the other.
    //    /// </summary>
    //    /// <param name="A">This point.</param>
    //    /// <param name="B">The point to measure the distance from the calling point.</param>
    //    /// <returns>The distance between the 2 points.</returns>
    //    public static Point DistanceTo(this Point A, Point B)
    //    {
    //        return new Point(B.X - A.X, B.Y - A.Y);
    //    }

    //    /// <summary>
    //    /// Performs a linear interpolation between this point and the specified one.
    //    /// </summary>
    //    /// <param name="A">This point.</param>
    //    /// <param name="B">The point to linearly interpolate with.</param>
    //    /// <param name="f">The interpolation factor.</param>
    //    /// <returns>The linear interpolation result.</returns>
    //    public static Point Lerp(this Point A, Point B, double f)
    //    {
    //        return new Point(A.X + (B.X - A.X) * f, A.Y + (B.Y - A.Y) * f);
    //    }

    //    #endregion

    //    #region I/O Extensions

    //    /// <summary>
    //    /// Reads a color from this stream reader.
    //    /// </summary>
    //    /// <returns>The color read.</returns>
    //    public static Color ReadColor(this BinaryReader reader)
    //    {
    //        return Color.FromRgb(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
    //    }

    //    /// <summary>
    //    /// Reads a point from this stream reader.
    //    /// </summary>
    //    /// <returns>The 2D Point read.</returns>
    //    public static Point ReadPoint(this BinaryReader reader)
    //    {
    //        return new Point(reader.ReadDouble(), reader.ReadDouble());
    //    }

    //    /// <summary>
    //    /// Reads a range from this stream reader.
    //    /// </summary>
    //    /// <returns>The numeric Range read.</returns>
    //    public static Range ReadRange(this XmlNode node)
    //    {
    //        return new Range(
    //            double.Parse(node["MinValue"].InnerText, System.Globalization.CultureInfo.InvariantCulture),
    //            double.Parse(node["MaxValue"].InnerText, System.Globalization.CultureInfo.InvariantCulture));
    //    }

    //    /// <summary>
    //    /// Reads a range from this stream reader.
    //    /// </summary>
    //    /// <returns>The numeric Range read.</returns>
    //    public static double ToDouble(this XmlNode node, string childNodeName)
    //    {
    //        return double.Parse(node[childNodeName].InnerText, System.Globalization.CultureInfo.InvariantCulture);
    //    }

    //    /// <summary>
    //    /// Reads a range from this stream reader.
    //    /// </summary>
    //    /// <returns>The numeric Range read.</returns>
    //    public static ushort ToUInt16(this XmlNode node, string childNodeName)
    //    {
    //        return ushort.Parse(node[childNodeName].InnerText);
    //    }

    //    /// <summary>
    //    /// Reads a range from this stream reader.
    //    /// </summary>
    //    /// <returns>The numeric Range read.</returns>
    //    public static Range ReadRange(this BinaryReader reader)
    //    {
    //        return new Range(reader.ReadDouble(), reader.ReadDouble());
    //    }



    //    /// <summary>
    //    /// Writes the RGB components of the specified color in the specified stream writer.
    //    /// 
    //    /// It takes a byte for each of the 3 components.
    //    /// </summary>
    //    /// <param name="writer">The binary writer to write the color to.</param>
    //    /// <param name="color">The color to write into this writer.</param>
    //    public static void Write(this BinaryWriter writer, Color color)
    //    {
    //        writer.Write(color.R);
    //        writer.Write(color.G);
    //        writer.Write(color.B);
    //    }

    //    /// <summary>
    //    /// Writes a 2D point coordinates to the specified stream writer.
    //    /// </summary>
    //    /// <param name="writer"></param>
    //    /// <param name="p"></param>
    //    /// <remarks>
    //    /// WPF forces us to use 16 bytes on saving 2 doubles (8 bytes each).
    //    /// 
    //    /// Single-precision (using 2x4 bytes instead) should be enough and faster 
    //    /// when used with the floating-point instructions in the GPU pipeline in OpenGL.
    //    /// </remarks>
    //    public static void Write(this BinaryWriter writer, Point p)
    //    {
    //        writer.Write(p.X);
    //        writer.Write(p.Y);
    //    }

    //    /// <summary>
    //    /// Writes a numeric range to the specified stream writer.
    //    /// </summary>
    //    /// <param name="writer"></param>
    //    /// <param name="p"></param>
    //    public static void Write(this BinaryWriter writer, Range range)
    //    {
    //        writer.Write(range.Min);
    //        writer.Write(range.Max);
    //    }

    //    #endregion

    //    #region UI Extensions

    //    /// <summary>
    //    /// Hides the annoying overflow button the WPF ToolBar brings by default.
    //    /// </summary>
    //    public static void HideOverflowButton(this ToolBar toolBar)
    //    {
    //        if (toolBar.Template.FindName("OverflowGrid", toolBar) is FrameworkElement overflowGrid)
    //            overflowGrid.Visibility = Visibility.Collapsed;
    //        if (toolBar.Template.FindName("MainPanelBorder", toolBar) is FrameworkElement mainPanelBorder)
    //            mainPanelBorder.Margin = new Thickness();
    //    }

    //    /// <summary>
    //    /// Gets the axis lock state according to the Keyboard state.
    //    /// </summary>
    //    /// <param name="e">The key events args.</param>
    //    /// <returns>The according axis lock state.</returns>
    //    public static AxisLock GetLockKeyState(this KeyEventArgs e)
    //    {
    //        if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl)) return AxisLock.LockedX;
    //        if (e.KeyboardDevice.IsKeyDown(Key.LeftShift)) return AxisLock.LockedY;
    //        return AxisLock.None;
    //    }

    //    public static Action EmptyDelegate = delegate () { };

    //    public static void Refresh(this UIElement ui)
    //    {
    //        ui.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
    //        ui.Arrange(new Rect(ui.DesiredSize));
    //        ui.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
    //    }

    //    public static T FindAncestor<T>(this UIElement ui) where T : FrameworkElement
    //    {
    //        DependencyObject dpParent = ui;
    //        do
    //        {
    //            dpParent = LogicalTreeHelper.GetParent(dpParent);
    //            if (dpParent == null)
    //                return null;
    //        } while (dpParent.GetType() != typeof(T));
    //        return (T)dpParent;
    //    }

    //    #endregion

    //}


    public static class Extensions
    {
        #region Math Extensions

        public static bool IsEqualTo(this double value, double other, double resolution)
        {
            return Math.Abs(other - value) < resolution;
        }

        public static bool IsLessThan(this double value, double other, double resolution)
        {
            return (other - value) > resolution;
        }

        public static bool IsGreaterThan(this double value, double other, double resolution)
        {
            return (other - value) < resolution;
        }

        public static double Clamp(this double value, double min, double max, uint roundDigits = 3, double resolution = 0.001)
        {
            var roundedValue = Math.Round(value, (int)roundDigits);
            if (roundedValue.IsLessThan(min, resolution)) return min;
            if (roundedValue.IsGreaterThan(max, resolution)) return max;
            return roundedValue;
        }

        public static double Map(double value, double fromSource, double toSource, double fromTarget, double toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }


        #endregion

        #region Time Extensions

        ///// <summary>
        ///// Gets the floor amount of units for the specified timescale.
        ///// </summary>
        ///// <param name="scale">The time scale to get the units.</param>
        ///// <returns>The floor amount of units for the specified timescale.</returns>
        //public static int Floor(this TimeSpan timeSpan, TimeScale scale)
        //{
        //    switch (scale)
        //    {
        //        case TimeScale.Hour:
        //            return timeSpan.Hours;
        //        case TimeScale.Minute:
        //            return (int)Math.Floor(timeSpan.TotalMinutes); // (timeSpan.Hours * 60.0) + timeSpan.Minutes;
        //        case TimeScale.Second:
        //            return (int)Math.Floor(timeSpan.TotalSeconds); // (timeSpan.Hours * 3600.0) + (timeSpan.Minutes * 60.0) + timeSpan.Seconds;
        //        case TimeScale.Millisecond:
        //            return (int)Math.Floor(timeSpan.TotalMilliseconds);
        //        default:
        //            throw new ArgumentOutOfRangeException($"TimeScale not implemented: {scale}");
        //    }
        //}

        ///// <summary>
        ///// Gets the ceiling amount of units for the specified timescale.
        ///// </summary>
        ///// <param name="scale">The time scale to get the units.</param>
        ///// <returns>The ceiling amount of units for the specified timescale.</returns>
        //public static int Ceiling(this TimeSpan timeSpan, TimeScale scale)
        //{
        //    switch (scale)
        //    {
        //        case TimeScale.Hour:
        //            return (int)Math.Ceiling(timeSpan.TotalHours);
        //        case TimeScale.Minute:
        //            return (int)Math.Ceiling(timeSpan.TotalMinutes);
        //        case TimeScale.Second:
        //            return (int)Math.Ceiling(timeSpan.TotalSeconds);
        //        case TimeScale.Millisecond:
        //            return (int)Math.Ceiling(timeSpan.TotalMilliseconds);
        //        default:
        //            throw new ArgumentOutOfRangeException($"TimeScale not implemented: {scale}");
        //    }
        //}

        public static string ToTimeStamp(this DateTime time)
        {
            return $"{time:yyyy MM dd HH mm ss fff}";
        }


        #endregion

        #region Point Extensions

        /// <summary>
        /// Adds this point to the other and return the result as a point.
        /// </summary>
        /// <param name="A">This point.</param>
        /// <param name="B">The point to add to this point.</param>
        /// <returns>The addition of the 2 points.</returns>
        public static Point Add(this Point A, Point B)
        {
            return new Point(B.X + A.X, B.Y + A.Y);
        }

        /// <summary>
        /// Computes the distance from this point to the other.
        /// </summary>
        /// <param name="A">This point.</param>
        /// <param name="B">The point to measure the distance from the calling point.</param>
        /// <returns>The distance between the 2 points.</returns>
        public static Point DistanceTo(this Point A, Point B)
        {
            return new Point(B.X - A.X, B.Y - A.Y);
        }

        /// <summary>
        /// Performs a linear interpolation between this point and the specified one.
        /// </summary>
        /// <param name="A">This point.</param>
        /// <param name="B">The point to linearly interpolate with.</param>
        /// <param name="f">The interpolation factor.</param>
        /// <returns>The linear interpolation result.</returns>
        public static Point Lerp(this Point A, Point B, double f)
        {
            return new Point(A.X + (B.X - A.X) * f, A.Y + (B.Y - A.Y) * f);
        }

        public static POINT ToPOINT(this Point point)
        {
            return new POINT((int)point.X, (int)point.Y);
        }

        #endregion

        #region I/O Extensions

        /// <summary>
        /// Reads a color from this stream reader.
        /// </summary>
        /// <returns>The color read.</returns>
        public static Color ReadColor(this BinaryReader reader)
        {
            return Color.FromRgb(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }

        /// <summary>
        /// Reads a point from this stream reader.
        /// </summary>
        /// <returns>The 2D Point read.</returns>
        public static Point ReadPoint(this BinaryReader reader)
        {
            return new Point(reader.ReadDouble(), reader.ReadDouble());
        }

        /// <summary>
        /// Reads a range from this stream reader.
        /// </summary>
        /// <returns>The numeric Range read.</returns>
        public static Range ReadRange(this XmlNode node)
        {
            return new Range(
                double.Parse(node["MinValue"].InnerText, System.Globalization.CultureInfo.InvariantCulture),
                double.Parse(node["MaxValue"].InnerText, System.Globalization.CultureInfo.InvariantCulture));
        }

        public static bool ToBool(this XmlNode node, string childNodeName)
        {
            return bool.Parse(node[childNodeName].InnerText);
        }


        /// <summary>
        /// Reads a range from this stream reader.
        /// </summary>
        /// <returns>The numeric Range read.</returns>
        public static double ToDouble(this XmlNode node, string childNodeName)
        {
            return double.Parse(node[childNodeName].InnerText, System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Reads a range from this stream reader.
        /// </summary>
        /// <returns>The numeric Range read.</returns>
        public static ushort ToUInt16(this XmlNode node, string childNodeName)
        {
            return ushort.Parse(node[childNodeName].InnerText);
        }

        /// <summary>
        /// Reads a range from this stream reader.
        /// </summary>
        /// <returns>The numeric Range read.</returns>
        public static int ToInt32(this XmlNode node, string childNodeName)
        {
            if (node[childNodeName] == null) return -1;
            return int.Parse(node[childNodeName].InnerText);
        }

        /// <summary>
        /// Reads a range from this stream reader.
        /// </summary>
        /// <returns>The numeric Range read.</returns>
        public static Range ReadRange(this BinaryReader reader)
        {
            return new Range(reader.ReadDouble(), reader.ReadDouble());
        }


        /// <summary>
        /// Writes the RGB components of the specified color in the specified stream writer.
        /// 
        /// It takes a byte for each of the 3 components.
        /// </summary>
        /// <param name="writer">The binary writer to write the color to.</param>
        /// <param name="color">The color to write into this writer.</param>
        public static void Write(this BinaryWriter writer, Color color)
        {
            writer.Write(color.R);
            writer.Write(color.G);
            writer.Write(color.B);
        }

        /// <summary>
        /// Writes a 2D point coordinates to the specified stream writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="p"></param>
        /// <remarks>
        /// WPF forces us to use 16 bytes on saving 2 doubles (8 bytes each).
        /// 
        /// Single-precision (using 2x4 bytes instead) should be enough and faster 
        /// when used with the floating-point instructions in the GPU pipeline in OpenGL.
        /// </remarks>
        public static void Write(this BinaryWriter writer, Point p)
        {
            writer.Write(p.X);
            writer.Write(p.Y);
        }

        /// <summary>
        /// Writes a numeric range to the specified stream writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="p"></param>
        public static void Write(this BinaryWriter writer, Range range)
        {
            writer.Write(range.Min);
            writer.Write(range.Max);
        }

        #endregion

        #region UI Extensions

        /// <summary>
        /// Hides the annoying overflow button the WPF ToolBar brings by default.
        /// </summary>
        public static void HideOverflowButton(this ToolBar toolBar)
        {
            if (toolBar.Template.FindName("OverflowGrid", toolBar) is FrameworkElement overflowGrid)
                overflowGrid.Visibility = Visibility.Collapsed;
            if (toolBar.Template.FindName("MainPanelBorder", toolBar) is FrameworkElement mainPanelBorder)
                mainPanelBorder.Margin = new Thickness();
        }


        public static Action EmptyDelegate = delegate () { };

        public static void Refresh(this UIElement ui)
        {
            ui.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            ui.Arrange(new Rect(ui.DesiredSize));
            ui.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        public static T FindAncestor<T>(this UIElement ui) where T : FrameworkElement
        {
            DependencyObject dpParent = ui;
            do
            {
                dpParent = LogicalTreeHelper.GetParent(dpParent);
                if (dpParent == null)
                    return null;
            } while (dpParent.GetType() != typeof(T));
            return (T)dpParent;
        }



        ///// <summary>
        ///// Gets the axis lock state according to the Keyboard state.
        ///// </summary>
        ///// <param name="e">The key events args.</param>
        ///// <returns>The according axis lock state.</returns>
        //public static AxisLock GetLockKeyState(this KeyEventArgs e)
        //{
        //    if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl)) return AxisLock.LockedX;
        //    if (e.KeyboardDevice.IsKeyDown(Key.LeftShift)) return AxisLock.LockedY;
        //    return AxisLock.None;
        //}

        #endregion


    }


}
