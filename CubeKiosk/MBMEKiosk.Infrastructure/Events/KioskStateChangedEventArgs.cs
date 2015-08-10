using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKiosk.Infrastructure.Events
{
    public class KioskStateChangedEventArgs : EventArgs
    {
        public readonly string Action;

        public KioskStateChangedEventArgs(string action)
        {
            this.Action = action;
        }
    }
}
