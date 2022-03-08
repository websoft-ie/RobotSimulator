using System.Linq;
using System.Windows;
using System.Windows.Media.Effects;

namespace MonkeyMotionControl.UI
{
    public class CustomMessageBox
    {
        public enum Buttons { Yes_No, OK }

        public enum Type { Alert, Error, Info, Dialog, Warning, Sequence}

        static BlurEffect MyBlur = new BlurEffect();

        public static bool ShowInfo(string Text)
        {
            return ShowInfo(Text, "");
        }
            
        public static bool ShowInfo(string Text, string Title)
        {
            //ShowBlurEffectAllWindow();
            CustomMessageBoxWindow messageBox = new CustomMessageBoxWindow(Text, Title, Type.Info);
            messageBox.ShowDialog();
            //StopBlurEffectAllWindow();
            return messageBox.ReturnValue > 0 ? true : false;
        }

        public static bool ShowDialog(string Text)
        {
            return ShowDialog(Text, "");
        }

        public static bool ShowDialog(string Text, string Title)
        {
            ShowBlurEffectAllWindow();
            CustomMessageBoxWindow messageBox = new CustomMessageBoxWindow(Text, Title, Type.Dialog);
            messageBox.ShowDialog();
            StopBlurEffectAllWindow();
            return messageBox.ReturnValue > 0 ? true : false;
        }

        public static bool ShowError(string Text)
        {
            return ShowError(Text, "");
        }
        
        public static bool ShowError(string Text, string Title)
        {
            ShowBlurEffectAllWindow();
            CustomMessageBoxWindow messageBox = new CustomMessageBoxWindow(Text, Title, Type.Error);
            messageBox.ShowDialog();
            StopBlurEffectAllWindow();
            return messageBox.ReturnValue > 0 ? true : false;
        }

        public static bool ShowWarning(string Text)
        {
            return ShowWarning(Text, "");
        }

        public static bool ShowWarning(string Text, string Title)
        {
            ShowBlurEffectAllWindow();
            CustomMessageBoxWindow messageBox = new CustomMessageBoxWindow(Text, Title, Type.Warning);
            messageBox.ShowDialog();
            StopBlurEffectAllWindow();
            return messageBox.ReturnValue > 0 ? true : false;
        }

        public static bool ShowAlert(string Text)
        {
            return ShowAlert(Text, "");
        }

        public static bool ShowAlert(string Text, string Title)
        {
            ShowBlurEffectAllWindow();
            CustomMessageBoxWindow messageBox = new CustomMessageBoxWindow(Text, Title, Type.Alert);
            messageBox.ShowDialog();
            StopBlurEffectAllWindow();
            return messageBox.ReturnValue > 0 ? true : false;
        }

        public static bool ShowShortNotification(string text)
        {
            CustomShortNotificationWindow shortNotification = new CustomShortNotificationWindow(text);
            shortNotification.Show();
            return true;
        }

        public static bool ShowShortNotificationDialog(string text)
        {
            ShowBlurEffectAllWindow();
            CustomShortNotificationWindow shortNotification = new CustomShortNotificationWindow(text);
            shortNotification.ShowDialog();
            StopBlurEffectAllWindow();
            return true;
        }

        public static void Close_AllShortNotifications()
        {
            foreach (CustomShortNotificationWindow window in Application.Current.Windows.OfType<CustomShortNotificationWindow>())
                window.FastClose();
            while (Application.Current.Windows.OfType<CustomShortNotificationWindow>().Count() > 0) { }
        }

        public static void Close_AllMessageBoxes()
        {
            foreach (CustomMessageBoxWindow window in Application.Current.Windows.OfType<CustomMessageBoxWindow>())
                window.Close();
            while (Application.Current.Windows.OfType<CustomMessageBoxWindow>().Count() > 0) { }
        }

        public static void ShowBlurEffectAllWindow()
        {
            MyBlur.Radius = 20;
            foreach (Window window in Application.Current.Windows)
                window.Effect = MyBlur;
        }

        public static void StopBlurEffectAllWindow()
        {
            MyBlur.Radius = 0;
        }

    }
}
