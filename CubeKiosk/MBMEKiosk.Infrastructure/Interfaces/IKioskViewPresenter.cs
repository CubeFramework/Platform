using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using MBMEKiosk.ObjectModel;
using System.Windows.Controls;
using MBMEKiosk.Infrastructure.Events;
using MBMEKiosk.Infrastructure.Utils;
using MBMEKiosk.Infrastructure.ObjectModel;

namespace MBMEKiosk.Infrastructure.Interfaces
{
    public interface IKioskViewPresenter
    {
        FrameworkElement LoadXaml(KioskState state, TransactionContextBase transactionContext);

        ////void Deactivate();

        event Action<KioskStateChangedEventArgs> KioskStateChangedEvent;

        event Action<ModuleSelectionChangedEventArgs> ModuleSelectionChangedEvent;
    }
}
