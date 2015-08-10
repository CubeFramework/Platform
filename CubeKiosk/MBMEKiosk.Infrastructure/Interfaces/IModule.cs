using System;
using System.ComponentModel;
using System.Windows;
using MBMEKiosk.Infrastructure.BaseClasses;
using MBMEKiosk.Infrastructure.Events;

namespace MBMEKiosk.Infrastructure.Interfaces
{
    public interface IModule : INotifyPropertyChanged
    {
        void Activate(string dispatcherAction = null);

        void Deactivate();

        event Action<ModuleSelectionChangedEventArgs> ModuleSelectionChangedEvent;

        event Action ModuleLayoutUpdatedEvent;
        
        string ConfigPath { get; }

        FrameworkElement ShellGrid { get; }

        ShellPresenterBase ShellPresenter { get; }
    }
}
