using System.Linq;
using System.Windows;
using MBMEKiosk.Infrastructure.Commands;
using MBMEKiosk.Infrastructure.Events;
using MBMEKiosk.Infrastructure.Interfaces;
using MBMEKiosk.Infrastructure.ObjectModel;
using MBMEKiosk.ObjectModel;
using System.Windows.Threading;
using System;
using System.Configuration;
using System.Diagnostics;

namespace MBMEKiosk.Infrastructure.BaseClasses
{
    public class PresenterBase : KioskViewPresenterBase, IPresenter
    {
        /// <summary>
        /// Constant for cancel Action
        /// </summary>
        public const string CANCELACTION = "cancel";

        /// <summary>
        /// Constant for submit Action
        /// </summary>
        public const string SUBMITACTION = "submit";

        /// <summary>
        /// Constant for error Action
        /// </summary>
        public const string ERRORACTION = "error";

        /// <summary>
        /// Constant for TimeOut Action
        /// </summary>
        public const string TIMEOUTACTION = "timeout";

        /// <summary>
        /// Constant for Biller Error Action
        /// </summary>
        public const string BILLERERRORACTION = "billerspecificerror";

        /// <summary>
        /// Constant for cash Action
        /// </summary>
        public const string CASHACTION = "cash";

        /// <summary>
        /// Constant for card Action
        /// </summary>
        public const string CARDACTION = "card";

        private KioskParameterisedCommand<string> submitCommand;
        private KioskCommand cancelCommand;
        public IDeviceAgent Devices { get; private set; }
        
        private DispatcherTimer timer;
        // Added By JK on 16/03/2013 
        private DispatcherTimer stackDelayNotificationTimer;
        private bool enableCashPayment;

        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public virtual void Deactivate()
        {
            if (StateTimer != null)
            {
                StateTimer.Stop();
                StateTimer.Tick -= OnTimeOut;
                StateTimer = null;
            }
             
        }

        public virtual FrameworkElement LoadXaml(IDeviceAgent devices, KioskState state, TransactionContextBase transactionContext)
        {
            Trace.WriteLine(string.Format("{0} - Infrastructure PresenterBase loadxaml started for {0}..", DateTime.Now,state.Name));
            Devices = devices;
            FrameworkElement viewGrid = base.LoadXaml(state, transactionContext);
            RestartStateTimer();
            Trace.WriteLine(string.Format("{0} - Infrastructure PresenterBase loadxaml ended for {0}..", DateTime.Now, state.Name));
            return viewGrid;
             
        }

        /// <summary>
        /// Invoke this method on state loading or on every user interaction (UI/Cash device) 
        /// to initialize or reset the state timer respectively.
        /// Uses the configured time in the following hierarachy:
        /// 1. Idle time out for all states.
        /// 2. State time out specified for the current state.
        /// </summary>
        protected virtual void RestartStateTimer()
        {
            if (ScreenTimeout != 0)
            {
                if (timer != null)
                {
                    timer.Stop();

                }
                else
                {
                    timer = new DispatcherTimer();
                    timer.Tick += OnTimeOut;
                }
            
            
              ////timer.Interval = new TimeSpan(0, 0, this.State.StateTimeOut);
              timer.Interval = new TimeSpan(0, 0, ScreenTimeout);
              timer.Start();
            }
        }

        /// Override this method to change the action to be taken on timeout.
        protected virtual void OnTimeOut(object o, EventArgs args)
        {
            //if (State.KioskActions.Where(a => a.Name.ToLower() == "timeout").Count() == 1)
            //{
                if (timer != null)
                {
                    timer.Stop();
                    timer.Tick -= OnTimeOut;
                    timer = null;
                }

                OnKioskStateChanged(new KioskStateChangedEventArgs("timeout"));
            //}
        }

        protected override void RegisterCommands()
        {
            SubmitCommand = new KioskParameterisedCommand<string>(ExecuteSubmitCommand, CanExecuteSubmitCommand);
            CancelCommand = new KioskCommand(ExecuteCancelCommand, CanExecuteCancelCommand);
        }

        protected override void ReevaulateIfCommandsCanExecute()
        {
            OnPropertyChanged("HeaderText");
            OnPropertyChanged("ContentText");
            SubmitCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }

        public KioskParameterisedCommand<string> SubmitCommand
        {
            get
            {
                return submitCommand;
            }

            private set
            {
                if (submitCommand != value)
                {
                    submitCommand = value;
                    OnPropertyChanged("SubmitCommand");
                }
            }
        }

        protected virtual void ExecuteSubmitCommand(string param)
        {
            //string action = string.IsNullOrEmpty(param) ? "submit" : param;
            OnKioskStateChanged(new KioskStateChangedEventArgs((string.IsNullOrEmpty(param))?SUBMITACTION:param));
        }

        protected virtual bool CanExecuteSubmitCommand(string param)
        {
            bool result;
            string action = string.IsNullOrEmpty(param) ? "submit" : param;
            result = State.KioskActions.Where(a => a.Name.ToLower() == action).Count() == 1;
            return result;
            
        }

        public KioskCommand CancelCommand
        {
            get
            {
                return cancelCommand;
            }

            private set
            {
                if (cancelCommand != value)
                {
                    cancelCommand = value;
                    OnPropertyChanged("CancelCommand");
                }
            }
        }

        protected virtual void ExecuteCancelCommand(EmptyCommandArgument args)
        {
            OnKioskStateChanged(new KioskStateChangedEventArgs("cancel"));
        }

        protected virtual bool CanExecuteCancelCommand(EmptyCommandArgument args)
        {
            return State.KioskActions.Where(a => a.Name.ToLower() == "cancel").Count() == 1;
        }

        public string HeaderText
        {
            get
            {
                return this.ViewGrid.TryFindResource(this.State.ViewHeaderKey) as string;
            }
        }

        public virtual string ContentText
        {
            get
            {
                return this.ViewGrid.TryFindResource(this.State.ViewContentKey) as string;
            }
        }

        /// <summary>
        /// Override this property to change which time out to be considered for the current state.
        /// Uses the configured time in the following hierarachy:
        /// 1. Idle time out specified for all the states.
        /// 2. State time out specified for the current state.
        /// </summary>
        protected virtual int ScreenTimeout
        {
            get
            {
                return this.State.IdleTimeOut;
            }
        }

        protected DispatcherTimer StateTimer
        {
            get
            {
                return timer;
            }

            set
            {
                if (timer != value)
                {
                    timer = value;
                    OnPropertyChanged("StateTimer");
                }
            }
        }

        protected DispatcherTimer StackDelayNotificationTimer
        {
            get
            {
                return stackDelayNotificationTimer;
            }

            set
            {
                if (stackDelayNotificationTimer != value)
                {
                    stackDelayNotificationTimer = value;
                    OnPropertyChanged("StackDelayNotificationTimer");
                }
            }
        }
        /// <summary>
        /// Invoke this method before issuing a stacking command in case of a note of allowed denomination is waiting in escrow. 
        /// to initialize Delay notification timer respectively.
        /// Uses the configured time.
        /// </summary>
        protected virtual void StartStackDelayNotificationTimer()
        {
            try
            {
                if (StackDelayNotificationTimer != null)
                {
                    StackDelayNotificationTimer.Stop();
                }
                else
                {
                    StackDelayNotificationTimer = new DispatcherTimer();
                    StackDelayNotificationTimer.Tick += OnStackDelayNotificationTimeOut;
                }

                StackDelayNotificationTimer.Interval = new TimeSpan(0, 0, Convert.ToInt32(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["CashDeviceStackDelay"]) ? "40" : ConfigurationManager.AppSettings["CashDeviceStackDelay"]));
                StackDelayNotificationTimer.Start();
            }
            catch (Exception ex)
            {
               // if (log.IsInfoEnabled) log.Info(ex.Message);
            }
        }

        /// Override this method to change the action to be taken on timeout.
        protected virtual void OnStackDelayNotificationTimeOut(object o, EventArgs args)
        {
            if (StackDelayNotificationTimer != null)
            {
                StackDelayNotificationTimer.Stop();
                StackDelayNotificationTimer.Tick -= OnTimeOut;
                StackDelayNotificationTimer = null;
            }
        }

        /// <summary>
        /// Added by Amit
        /// Enables Cash Payment ON Values CASH or CASH/CARD read from the key Payment Mode
        /// </summary>
        protected virtual bool EnableCashPayment
        {
            get
            {
                if (ConfigurationManager.AppSettings["PaymentMode"] != null)
                {
                    if (ConfigurationManager.AppSettings["PaymentMode"] == "CASH" ||
                    ConfigurationManager.AppSettings["PaymentMode"] == "CASH/CARD")
                    {
                        enableCashPayment = true;
                    }
                    else
                    {
                        enableCashPayment = false;
                    }
                }
                else
                    enableCashPayment = true;


                return enableCashPayment;
            }
        }

        /// <summary>
        /// Added by Amit
        /// Enables Cash Payment ON Values CASH or CASH/CARD read from the key Payment Mode
        /// </summary>
        protected virtual bool EnableCardPayment
        {
            get
            {
                if (ConfigurationManager.AppSettings["PaymentMode"] != null)
                {
                    if (ConfigurationManager.AppSettings["PaymentMode"] == "CARD" ||
                    ConfigurationManager.AppSettings["PaymentMode"] == "CASH/CARD")
                    {
                        enableCashPayment = true;
                    }
                    else
                    {
                        enableCashPayment = false;
                    }
                }
                else
                    enableCashPayment = true;


                return enableCashPayment;
            }
        }
    }
}
