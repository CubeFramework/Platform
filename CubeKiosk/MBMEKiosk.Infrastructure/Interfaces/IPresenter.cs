using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using MBMEKiosk.ObjectModel;
using MBMEKiosk.Infrastructure.ObjectModel;

namespace MBMEKiosk.Infrastructure.Interfaces
{
    public interface IPresenter
    {
        FrameworkElement LoadXaml(IDeviceAgent devices, KioskState state, TransactionContextBase transactionContext);

        void Deactivate();

        IDeviceAgent Devices { get; }
    }
}
