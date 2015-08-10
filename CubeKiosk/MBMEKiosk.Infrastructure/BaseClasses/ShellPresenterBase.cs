using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using MBMEKiosk.Infrastructure.Commands;
using MBMEKiosk.Infrastructure.Events;
using MBMEKiosk.Infrastructure.ObjectModel;
using MBMEKiosk.ObjectModel;

namespace MBMEKiosk.Infrastructure.BaseClasses
{
    public class ShellPresenterBase : KioskViewPresenterBase
    {
        protected const string RESOURCEPATHTEMPLATE = @"pack://siteoforigin:,,,/[KEY]";
        private FrameworkElement child;

        private KioskCommand homeCommand;
        private KioskCommand helpCommand;
        private KioskCommand backCommand;

        public override FrameworkElement LoadXaml(KioskState state, TransactionContextBase transactionContext)
        {
            RegisterCommands();
            this.TransactionContext = transactionContext;
            this.State = state;
            try
            {
                OnPropertyChanged("BackgroundImagePath");
                FrameworkElement root = LoadXamlFile(state.ShellXamlPath);
                root.BeginInit();
                try
                {
                    root.DataContext = this;
                    ReevaulateIfCommandsCanExecute();
                }
                catch (Exception ex)
                {
                    // KS TODO: Add log messages.
                    throw ex;
                }
                finally
                {
                    root.EndInit();
                    ViewGrid = root;
                    root.UpdateLayout();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception details: \nSource: {0}\nMessage: {1}\nTrace:", ex.Source, ex.Message, ex.StackTrace);
                throw ex;
            }

            return ViewGrid;
        }

        public void RefreshShellForNewState(KioskState state)
        {
            this.State = state;
            ViewGrid.BeginInit();
            ViewGrid.DataContext = null;
            ViewGrid.DataContext = this;
            ReevaulateIfCommandsCanExecute();
            ViewGrid.EndInit();
            ViewGrid.UpdateLayout();
        }

        protected override void RegisterCommands()
        {
            BackCommand = new KioskCommand(ExecuteBackCommand, CanExecuteBackCommand, true);
            BackCommand.ExecutionCompleted += OnCommandExecutionCompleted;

            HelpCommand = new KioskCommand(ExecuteHelpCommand, CanExecuteHelpCommand, true);
            HelpCommand.ExecutionCompleted += OnCommandExecutionCompleted;

            HomeCommand = new KioskCommand(ExecuteHomeCommand, CanExecuteHomeCommand, true);
            HomeCommand.ExecutionCompleted += OnCommandExecutionCompleted;
        }

        private void OnCommandExecutionCompleted()
        {
            ViewGrid.BeginInit();
            ViewGrid.DataContext = null;
            ViewGrid.DataContext = this;
            ReevaulateIfCommandsCanExecute();
            ViewGrid.EndInit();
            ViewGrid.UpdateLayout();
        }

        protected override void ReevaulateIfCommandsCanExecute()
        {
            BackCommand.RaiseCanExecuteChanged();
            HelpCommand.RaiseCanExecuteChanged();
            HomeCommand.RaiseCanExecuteChanged();
        }

        #region Home Command

        public KioskCommand HomeCommand
        {
            get
            {
                return homeCommand;
            }

            private set
            {
                if (homeCommand != value)
                {
                    homeCommand = value;
                    OnPropertyChanged("HomeCommand");
                }
            }
        }

        protected virtual void ExecuteHomeCommand(EmptyCommandArgument arg)
        {
            OnKioskStateChanged(new KioskStateChangedEventArgs("home"));
        }

        protected virtual bool CanExecuteHomeCommand(EmptyCommandArgument arg)
        {
            //bool temp = (!TransactionContext.CashCycleInProgress || !TransactionContext.CardCycleInProgress) && State.KioskActions.Where(a => a.Name.ToLower() == "home").Count() == 1;

            bool result = (!TransactionContext.CashCycleInProgress) && State.KioskActions.Where(a => a.Name.ToLower() == "home").Count() == 1;

            if (TransactionContext.CardCycleInProgress)
                result = (!TransactionContext.CardCycleInProgress) && State.KioskActions.Where(a => a.Name.ToLower() == "home").Count() == 1;

            Trace.WriteLine(string.Format("{0} Enable MainMenu Button : {1}", DateTime.Now, result));

            return result;
        }

        #endregion

        #region Help command
        public KioskCommand HelpCommand
        {
            get
            {
                return helpCommand;
            }

            private set
            {
                if (helpCommand != value)
                {
                    helpCommand = value;
                    OnPropertyChanged("HelpCommand");
                }
            }
        }

        protected virtual void ExecuteHelpCommand(EmptyCommandArgument arg)
        {
            OnKioskStateChanged(new KioskStateChangedEventArgs("help"));
        }

        protected virtual bool CanExecuteHelpCommand(EmptyCommandArgument arg)
        {
            ////return double.Parse(TransactionContext.AmountPaid) <= 0 && State.KioskActions.Where(a => a.Name.ToLower() == "help").Count() == 1;
            return !TransactionContext.CashCycleInProgress && State.KioskActions.Where(a => a.Name.ToLower() == "help").Count() == 1;
        }
        #endregion

        #region Back command
        public KioskCommand BackCommand
        {
            get
            {
                return backCommand;
            }

            private set
            {
                if (backCommand != value)
                {
                    backCommand = value;
                    OnPropertyChanged("BackCommand");
                }
            }
        }

        protected virtual void ExecuteBackCommand(EmptyCommandArgument arg)
        {
            TransactionContext.isForward = false;
            OnKioskStateChanged(new KioskStateChangedEventArgs("back"));
        }

        protected virtual bool CanExecuteBackCommand(EmptyCommandArgument arg)
        {
            ////return double.Parse(TransactionContext.AmountPaid) <= 0 && State.KioskActions.Where(a => a.Name.ToLower() == "back").Count() == 1;
            bool result = (!TransactionContext.CashCycleInProgress) && State.KioskActions.Where(a => a.Name.ToLower() == "back").Count() == 1;

            if (TransactionContext.CardCycleInProgress)
                result = (!TransactionContext.CardCycleInProgress) && State.KioskActions.Where(a => a.Name.ToLower() == "back").Count() == 1;

            Trace.WriteLine(string.Format("{0} Enable Back Button : {1}", DateTime.Now, result));

            return result;
        }
        #endregion

        public FrameworkElement Child
        {
            get
            {
                return child;
            }
            set
            {
                if (child != value)
                {
                    child = value;
                    OnPropertyChanged("Child");
                }
            }
        }

        public virtual string BackgroundImagePath
        {
            get
            {
                string fullPath = RESOURCEPATHTEMPLATE.Replace("[KEY]", State.BackgroundImagePath);
                return fullPath;
            }
        }

        public string ShellHeight
        {
            get
            {
                string resolution = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["app_ht"]) ? "1024" : ConfigurationManager.AppSettings["app_ht"];
                return resolution;
            }
        }
    }
}
