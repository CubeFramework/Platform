using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;

namespace MBMEKiosk.Infrastructure.Commands
{
    public class KioskParameterisedCommand<T> : IKioskCommand
    {
        private readonly Func<T, bool> canExecuteMethod;
        private readonly Action<T> executeMethod;
        private List<WeakReference> canExecuteChangedHandlers;
        private bool isActive;
        private bool isVisible;
        private bool notifyOnExecutionCompletion;

        private bool lockExecute;

        public event EventHandler IsActiveChanged;

        public KioskParameterisedCommand(Action<T> executeMethod)
            : this (executeMethod, null, false)
        {
        }

        public KioskParameterisedCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
            : this(executeMethod, canExecuteMethod, false)
        {

        }

        public KioskParameterisedCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod, bool notifyOnExecutionCompletion)
        {
            try
            {
                this.notifyOnExecutionCompletion = notifyOnExecutionCompletion;
                this.executeMethod = executeMethod;
                this.canExecuteMethod = canExecuteMethod;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("{0} Error in KioskParameterised Command : {1} StackTrace {2} \n InnerException : {3} : Inner StackTrace {4}", DateTime.Now,
                    ex.Message, ex.StackTrace, (ex.InnerException == null) ? "" : ex.InnerException.Message, (ex.InnerException == null) ? "" : ex.InnerException.StackTrace));
            }
        }

        #region IKioskCommand Members

        public bool IsVisible
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsActive
        {
            get { throw new NotImplementedException(); }
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        protected virtual void OnCanExecuteChanged()
        {
            KioskWeakEventHandlerManager.InvokeHandlers(this, canExecuteChangedHandlers);
        }

        #endregion

        #region ICommand Members

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute((T)parameter);
        }

        public virtual bool CanExecute(T parameter)
        {

            if (canExecuteMethod == null)
            {
                return true;
            }

            return canExecuteMethod(parameter) && !lockExecute;
        }

        public event EventHandler CanExecuteChanged;

        void ICommand.Execute(object parameter)
        {
            Execute((T)parameter);
        }

        public void Execute(T parameter)
        {
            if (executeMethod == null)
            {
                return;
            }

            if (CanExecute(parameter))
            {
                //// KS TODO: Log the command execution.
                lockExecute = true;
                try
                {
                    executeMethod(parameter);
                }
                finally
                {
                    lockExecute = false;
                    RaiseCanExecuteChanged();
                    if (notifyOnExecutionCompletion)
                    {
                        OnExecutionCompleted();
                    }
                }
            }
        }

        public event Action ExecutionCompleted;

        private void OnExecutionCompleted()
        {
            Action handler = ExecutionCompleted;
            if (handler != null)
            {
                handler();
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
