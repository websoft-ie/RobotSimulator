﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MonkeyMotionControl.UI
{
    /// <summary>
    /// Interaction logic for CustomShortNotificationWindow.xaml
    /// </summary>
    public partial class CustomShortNotificationWindow : Window
    {
        public CustomShortNotificationWindow(string Text)
        {
            InitializeComponent();
            txbText.Text = Text;
        }

        public int Time = 5;
        DoubleAnimation anim;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            anim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(Time));
            anim.Completed += (s, _) => this.Close();
            this.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Height = (txbText.LineCount * 52) + 50;
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FastClose();
        }

        public void FastClose()
        {
            anim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.1));
            anim.Completed += (s, _) => this.Close();
            this.BeginAnimation(UIElement.OpacityProperty, anim);
        }

    }
}
