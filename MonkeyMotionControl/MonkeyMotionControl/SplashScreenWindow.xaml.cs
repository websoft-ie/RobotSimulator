using System.Windows;

namespace MonkeyMotionControl
{
    /// <summary>
    /// Interaction logic for SplashScreenWindow.xaml
    /// </summary>
    public partial class SplashScreenWindow : Window
    {
        private double _progress = 0;
        public double Progress
        {
            get { return ProgressBar.Width; }
            set 
            {
                _progress += value * (SplashScreenWindowInternal.Width / 1000);
                if(_progress <= SplashScreenWindowInternal.Width)
                {
                    ProgressBar.Width = _progress;
                }
            }
        }

        public SplashScreenWindow()
        {
            InitializeComponent();
        }
    }
}
