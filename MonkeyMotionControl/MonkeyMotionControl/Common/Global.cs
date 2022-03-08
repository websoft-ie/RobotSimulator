using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;

namespace MonkeyMotionControl
{
    //public class Global
    //{
    //    #region Constants

    //    /// <summary>
    //    /// Maximum duration of 99 hours, in seconds unit.
    //    /// </summary>
    //    public const double MaxDuration = 99 * 60 * 60;

    //    /// <summary>
    //    /// The time resolution in seconds.
    //    /// 
    //    /// Default value is one millisecond.
    //    /// </summary>
    //    public static readonly double TimeResolution = 0.001;
    //    public static readonly double TimeResolutionTriggerTracks = 0.05;

    //    // Maximum performance values.
    //    public const int MaxAxisCount = 30;

    //    public const int MaxKeyFramesPerAxisCount = 500;

    //    #endregion

    //    public static readonly string AppPath;

    //    public static bool CreatingKeyFrame = false;

    //    #region UX Properties

    //    /// <summary>
    //    /// The minimum distance the user must drag a key-frame to start its dragging.
    //    /// </summary>
    //    public static double DragDistance = 1.0;

    //    public static double ScrollBarWidth = 24.0;

    //    #endregion

    //    static Global()
    //    {
    //        AppPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
    //    }


    //    #region Style UI Properties


    //    /// <summary>
    //    /// The radius of the points in the visual graph.
    //    /// </summary>
    //    public static double KeyframeRadius = 5.0;

    //    public static double KeyframeSelectedBorderThickness = 2.0;

    //    /// <summary>
    //    /// The radius of the points in the visual graph.
    //    /// </summary>
    //    public static double HandleRadius = 5.0;

    //    /// <summary>
    //    /// The thickness of the curve in the visual graph.
    //    /// </summary>
    //    public static double TrackThickness = 2.0;
    //    /// <summary>
    //    /// The thickness of the line handles in the visual graph.
    //    /// </summary>
    //    public static double HandleLineThickness = 1.0;

    //    /// <summary>
    //    /// The brush to represent an unselected keyframe.
    //    /// </summary>
    //    public static Brush KeyframeBrush = Brushes.Red;
    //    /// <summary>
    //    /// The brush to render the border of the selected keyframe.
    //    /// </summary>
    //    public static Brush KeyframeSelectedBorderBrush = Brushes.Orange;

    //    /// <summary>
    //    /// The brush to represent a handle line.
    //    /// </summary>
    //    public static Color HandleLineBrush = Colors.White;
    //    /// <summary>
    //    /// The brush to represent a handle point.
    //    /// </summary>
    //    public static Color HandlePointBrush = Colors.White;

    //    /// <summary>
    //    /// The brush to represent a handle line.
    //    /// </summary>
    //    public static Color LinkedHandleLineBrush = Colors.Yellow;
    //    /// <summary>
    //    /// The brush to represent a handle point.
    //    /// </summary>
    //    public static Color LinkedHandlePointBrush = Colors.Yellow;

    //    public static Color PlayHeadLineColor = Colors.Yellow;

    //    public static double TimeLabelFontSize = 10.0;
    //    public static string TimeTypefaceName = "Roboto";
    //    public static Color TimeLabelColor = Colors.DarkGray;

    //    public static Color LightColor = Color.FromArgb(255, 50, 50, 50);
    //    public static Color MiddleColor = Color.FromArgb(255, 30, 30, 30);
    //    public static Color DarkColor = Color.FromArgb(255,20,20,20);
    //    public static Color DimColor = Color.FromArgb(255, 10, 10, 10);

    //    public static double TimeRulerHeight = 24.0;

    //    #endregion

    //    /// <summary>
    //    /// Loads the configuration settings from the project library Settings.
    //    /// </summary>
    //    /// <param name="settings">The project settings.</param>
    //    /// <see cref="MonkeyMotionControl -> Properties -> Settings"/>
    //    internal static void LoadConfigurationSettings(Properties.Settings settings)
    //    {
    //        TrackThickness = settings.TrackThickness;
    //        HandleLineThickness = settings.HandleLineThickness;
    //        KeyframeRadius = settings.KeyframeRadius;
    //        HandleRadius = settings.HandleRadius;
    //        KeyframeSelectedBorderThickness = settings.KeyframeSelectedBorderThickness;

    //        HandleLineBrush = settings.HandleLineColor;
    //        HandlePointBrush = settings.HandlePointColor;

    //        LinkedHandleLineBrush = settings.LinkedHandleLineColor;
    //        LinkedHandlePointBrush = settings.LinkedHandlePointColor;

    //        PlayHeadLineColor = settings.PlayHeadLineColor;
    //        KeyframeBrush = new SolidColorBrush(settings.KeyframeColor);
    //        KeyframeSelectedBorderBrush = new SolidColorBrush(settings.KeyframeSelectedBorderColor);
    //        TimeTypefaceName = settings.TimeTypefaceName;
    //        TimeLabelFontSize = settings.TimeLabelFontSize;
    //        TimeLabelColor = settings.TimeLabelColor;

    //        LightColor = settings.LightColor;
    //        MiddleColor = settings.MiddleColor;
    //        DarkColor = settings.DarkColor;
    //        DimColor = settings.DimColor;

    //        TimeRulerHeight = settings.TimeRulerHeight;
    //    }


    //    #region Content Files Management

    //    public static IEnumerable<string> EnumerateFiles(string basePath, string ext, bool recursive = false)
    //    {
    //        var dir = new DirectoryInfo(Path.Combine(AppPath, basePath));
    //        foreach (var file in dir.GetFiles($"*{ext}", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
    //            yield return file.FullName;
    //    }

    //    public static FileStream OpenFileStream(string relativePath)
    //    {
    //        return File.OpenRead(Path.Combine(AppPath, relativePath));
    //    }

    //    #endregion


    //}

    public class Global
    {
        public static double DefaultDuration = 20.0;

        public static int ZoomSteps = 25;

        public static string CustomFormatName = "MonkeyMotion";
        public static string CustomFormatExtension = ".monkeymotion";

        public static readonly string AppPath;

        static Global()
        {
            AppPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        }



        #region Constants

        /// <summary>
        /// Maximum duration of 99 hours, in seconds unit.
        /// </summary>
        public const double MaxDuration = 99 * 60 * 60;

        /// <summary>
        /// The time resolution in seconds.
        /// 
        /// Default value is one millisecond.
        /// </summary>
        public static readonly double TimeResolution = 0.001;

        public static readonly double TimeResolutionTriggerTracks = 0.05;

        // Maximum performance values.
        public const int MaxAxisCount = 30;

        public const int MaxKeyFramesPerAxisCount = 500;

        #endregion

        public static bool CreatingKeyFrame = false;

        #region UX Properties

        /// <summary>
        /// The minimum distance the user must drag a key-frame to start its dragging.
        /// </summary>
        public static double DragDistance = 1.0;

        public static double ScrollBarWidth = 24.0;

        #endregion

        #region Style UI Properties

        /// <summary>
        /// The radius of the points in the visual graph.
        /// </summary>
        public static double KeyframeRadius = 5.0;

        public static double KeyframeSelectedBorderThickness = 2.0;

        /// <summary>
        /// The radius of the points in the visual graph.
        /// </summary>
        public static double HandleRadius = 5.0;

        /// <summary>
        /// The thickness of the curve in the visual graph.
        /// </summary>
        public static double TrackThickness = 2.0;
        /// <summary>
        /// The thickness of the line handles in the visual graph.
        /// </summary>
        public static double HandleLineThickness = 1.0;

        /// <summary>
        /// The brush to represent an unselected keyframe.
        /// </summary>
        public static Brush KeyframeBrush = Brushes.Red;
        /// <summary>
        /// The brush to render the border of the selected keyframe.
        /// </summary>
        public static Brush KeyframeSelectedBorderBrush = Brushes.Orange;

        /// <summary>
        /// The brush to represent a handle line.
        /// </summary>
        public static Color HandleLineBrush = Colors.White;
        /// <summary>
        /// The brush to represent a handle point.
        /// </summary>
        public static Color HandlePointBrush = Colors.White;

        /// <summary>
        /// The brush to represent a handle line.
        /// </summary>
        public static Color LinkedHandleLineBrush = Colors.Yellow;
        /// <summary>
        /// The brush to represent a handle point.
        /// </summary>
        public static Color LinkedHandlePointBrush = Colors.Yellow;

        public static Color PlayHeadLineColor = Colors.Yellow;

        public static double TimeLabelFontSize = 10.0;
        public static string TimeTypefaceName = "Roboto";
        public static Color TimeLabelColor = Colors.DarkGray;

        public static Color LightColor = Color.FromArgb(255, 50, 50, 50);
        public static Color MiddleColor = Color.FromArgb(255, 30, 30, 30);
        public static Color DarkColor = Color.FromArgb(255, 20, 20, 20);
        public static Color DimColor = Color.FromArgb(255, 10, 10, 10);

        public static double TimeRulerHeight = 28.0;

        #endregion

        /// <summary>
        /// Loads the configuration settings from the project library Settings.
        /// </summary>
        /// <param name="settings">The project settings.</param>
        /// <see cref="MonkeyMotionControl -> Properties -> Settings"/>
        internal static void LoadConfigurationSettings(Properties.Settings settings)
        {
            //TrackThickness = settings.TrackThickness;
            //HandleLineThickness = settings.HandleLineThickness;
            //KeyframeRadius = settings.KeyframeRadius;
            //HandleRadius = settings.HandleRadius;
            //KeyframeSelectedBorderThickness = settings.KeyframeSelectedBorderThickness;

            //HandleLineBrush = settings.HandleLineColor;
            //HandlePointBrush = settings.HandlePointColor;

            //LinkedHandleLineBrush = settings.LinkedHandleLineColor;
            //LinkedHandlePointBrush = settings.LinkedHandlePointColor;

            //PlayHeadLineColor = settings.PlayHeadLineColor;
            //KeyframeBrush = new SolidColorBrush(settings.KeyframeColor);
            //KeyframeSelectedBorderBrush = new SolidColorBrush(settings.KeyframeSelectedBorderColor);
            //TimeTypefaceName = settings.TimeTypefaceName;
            //TimeLabelFontSize = settings.TimeLabelFontSize;
            //TimeLabelColor = settings.TimeLabelColor;

            //LightColor = settings.LightColor;
            //MiddleColor = settings.MiddleColor;
            //DarkColor = settings.DarkColor;
            //DimColor = settings.DimColor;

            //TimeRulerHeight = settings.TimeRulerHeight;
        }

        #region Content Files Management

        public static IEnumerable<string> EnumerateFiles(string basePath, string ext, bool recursive = false)
        {
            var dir = new DirectoryInfo(Path.Combine(AppPath, basePath));
            foreach (var file in dir.GetFiles($"*{ext}", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                yield return file.FullName;
        }

        public static FileStream OpenFileStream(string relativePath)
        {
            return File.OpenRead(Path.Combine(AppPath, relativePath));
        }

        #endregion

    }

}
