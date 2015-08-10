using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;

namespace MBMEKiosk.Infrastructure.Commands
{
    public interface IKioskCommand : ICommand, INotifyPropertyChanged
    {
        bool IsVisible { get; }

        bool IsActive { get; }

        event EventHandler IsActiveChanged;

        event Action ExecutionCompleted;

        void RaiseCanExecuteChanged();
    }
}
