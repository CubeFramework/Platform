using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.ObjectModel;
using System.Windows;

namespace MBMEKiosk.MBME.Command
{
    public class ShutDownCommand :BaseKioskCommand
    {
         
        protected override void ExecuteCommand()
        {
            Application.Current.Shutdown();
        }
    }
}
