using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKiosk.Infrastructure.Events
{
    public class ModuleSelectionChangedEventArgs : EventArgs
    {
        public readonly string NewModule;
        public readonly string DispatcherAction;

        public ModuleSelectionChangedEventArgs(string newModule, string dispatcherAction = null)
        {
            this.NewModule = newModule;
            this.DispatcherAction = dispatcherAction;
        }
    }
}
