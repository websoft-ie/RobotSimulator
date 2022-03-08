using System;

namespace MonkeyMotionControl.UI
{
    public class LogEventArgs : EventArgs
    {
        public string LogMsg { get; set; }

        public LogEventArgs(string msg)
        {
            this.LogMsg = msg;
        }
    }

}
