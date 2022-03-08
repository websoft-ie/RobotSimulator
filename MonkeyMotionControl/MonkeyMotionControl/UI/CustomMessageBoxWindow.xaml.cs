using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MonkeyMotionControl.UI
{
    /// <summary>
    /// Interaction logic for CustomMessageBoxAlert.xaml
    /// </summary>
    public partial class CustomMessageBoxWindow : Window
    {
        public int ReturnValue { get; set; }

        private DoubleAnimation anim;

        private int textLines = 0;

        public CustomMessageBoxWindow(string text, string title, CustomMessageBox.Type type)
        {
            InitializeComponent();

            switch (type)
            {
                case CustomMessageBox.Type.Info:
                    ErrorBox.Visibility = Visibility.Collapsed;
                    WarningBox.Visibility = Visibility.Collapsed;
                    AlertBox.Visibility = Visibility.Collapsed;
                    MsgBox.Visibility = Visibility.Visible;

                    msgBox_Title.Text = title;
                    msgBox_Message.Text = text;
                    textLines = msgBox_Message.LineCount;
                    msgBox_BtnYesNo.Visibility = Visibility.Collapsed;
                    msgBox_BtnOK.Visibility = Visibility.Visible;
                    //msgBox_GraphicQuestion.Visibility = Visibility.Collapsed;
                    //msgBox_GraphicInfo.Visibility = Visibility.Visible;
                    break;

                case CustomMessageBox.Type.Dialog:
                    ErrorBox.Visibility = Visibility.Collapsed;
                    WarningBox.Visibility = Visibility.Collapsed;
                    AlertBox.Visibility = Visibility.Collapsed;
                    MsgBox.Visibility = Visibility.Visible;
                    
                    msgBox_Title.Text = title;
                    msgBox_Message.Text = text;
                    textLines = msgBox_Message.LineCount;
                    msgBox_BtnYesNo.Visibility = Visibility.Visible;
                    msgBox_BtnOK.Visibility = Visibility.Collapsed;
                    //msgBox_GraphicQuestion.Visibility = Visibility.Visible;
                    //msgBox_GraphicInfo.Visibility = Visibility.Collapsed;
                    break;

                case CustomMessageBox.Type.Error:
                    ErrorBox.Visibility = Visibility.Visible;
                    WarningBox.Visibility = Visibility.Collapsed;
                    AlertBox.Visibility = Visibility.Collapsed;
                    MsgBox.Visibility = Visibility.Collapsed;

                    errorBox_Title.Text = title;
                    errorBox_Message.Text = text;
                    textLines = errorBox_Message.LineCount;
                    break;

                case CustomMessageBox.Type.Warning:
                    ErrorBox.Visibility = Visibility.Collapsed;
                    WarningBox.Visibility = Visibility.Visible;
                    AlertBox.Visibility = Visibility.Collapsed;
                    MsgBox.Visibility = Visibility.Collapsed;
                    
                    warningBox_Title.Text = title;
                    warningBox_Message.Text = text;
                    textLines = warningBox_Message.LineCount;

                    //Keyboard.Focus(warningBox_Btn_Yes);
                    //warningBox_Btn_Yes.Focus();
                    
                    //warningBox_GraphicWarning.Visibility = Visibility.Visible;
                    //warningBox_GraphicAlert.Visibility = Visibility.Collapsed;
                    break;

                case CustomMessageBox.Type.Alert:
                    ErrorBox.Visibility = Visibility.Collapsed;
                    WarningBox.Visibility = Visibility.Collapsed;
                    AlertBox.Visibility = Visibility.Visible;
                    MsgBox.Visibility = Visibility.Collapsed;
                    
                    alertBox_Title.Text = title;
                    alertBox_Message.Text = text;
                    textLines = alertBox_Message.LineCount;
                    //warningBox_GraphicWarning.Visibility = Visibility.Collapsed;
                    //warningBox_GraphicAlert.Visibility =  Visibility.Visible;
                    break;

                default:
                    ErrorBox.Visibility = Visibility.Collapsed;
                    WarningBox.Visibility = Visibility.Collapsed;
                    AlertBox.Visibility = Visibility.Collapsed;
                    MsgBox.Visibility = Visibility.Visible;

                    msgBox_Title.Text = title;
                    msgBox_Message.Text = text;
                    textLines = msgBox_Message.LineCount;
                    msgBox_BtnYesNo.Visibility = Visibility.Collapsed;
                    msgBox_BtnOK.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void gBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            ReturnValue = -1;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            anim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.3));
            anim.Completed += (s, _) => this.Close();
            this.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Height = (textLines * 27) + gBar.Height + 60;
            Topmost = true;
        }

        private void btnReturnValue_Click(object sender, RoutedEventArgs e)
        {
            ReturnValue = Convert.ToInt32(((Button)sender).Uid);
            Close();
        }
    }
}
