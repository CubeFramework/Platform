using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.Interfaces;
using MBMEKiosk.Infrastructure.BaseClasses;
using MBMEKiosk.Infrastructure.Commands;
using MBMEKiosk.Infrastructure.Events;
using System.Threading;
using System.IO;
using System.Windows;
using MBMEKiosk.ObjectModel;
using MBMEKiosk.Infrastructure.ObjectModel;

namespace MBMEKiosk.Mawaqif.Presenters
{
    public class CardPaymentAmountEntryPresenter : MawaqifPresenterBase
    {
        private KioskParameterisedCommand<string> addDigit;
        private KioskCommand backspaceCommand;

        public override FrameworkElement LoadXaml(IDeviceAgent devices, KioskState state, TransactionContextBase transactionContext)
        {
            FrameworkElement viewGrid = base.LoadXaml(devices, state, transactionContext);
            this.Transaction.PaymentAmount = string.Empty;
            return viewGrid;
        }
        protected override void RegisterCommands()
        {
            base.RegisterCommands();
            AddDigitCommand = new KioskParameterisedCommand<string>(ExecuteAddDigitCommand, CanExecuteAddDigitCommand, true);
            AddDigitCommand.ExecutionCompleted += Command_ExecutionCompleted;

            BackspaceCommand = new KioskCommand(ExecuteBackspaceCommand, CanExecuteBackspaceCommand, true);
            BackspaceCommand.ExecutionCompleted += Command_ExecutionCompleted;
        }

        void Command_ExecutionCompleted()
        {
            OnRequestLayoutUpdate();
        }

        protected override void ReevaulateIfCommandsCanExecute()
        {
            base.ReevaulateIfCommandsCanExecute();
            AddDigitCommand.RaiseCanExecuteChanged();
            BackspaceCommand.RaiseCanExecuteChanged();
        }

        public KioskParameterisedCommand<string> AddDigitCommand
        {
            get
            {
                return addDigit;
            }

            private set
            {
                if (addDigit != value)
                {
                    addDigit = value;
                    OnPropertyChanged("AddDigitCommand");
                }
            }
        }

        private void ExecuteAddDigitCommand(string param)
        {
            RestartStateTimer();
            PaymentAmount += param;
        }

        private bool CanExecuteAddDigitCommand(string param)
        {
            bool result = false;

            return (PaymentAmount.Length < State.LeadDigits.Length && State.LeadDigits.StartsWith(PaymentAmount + param)) ||
                (PaymentAmount.Length >= State.LeadDigits.Length && PaymentAmount.Length < State.MaxAccNoLength);
             
             
            //switch (Transaction.ServiceType)
            //{
            //    case MawaqifServiceType.None:
            //        break;
            //    case MawaqifServiceType.AccountTopUp:
            //        PaymentAmount = Convert.ToDouble(Transaction.BalanceDue).ToString();
            //        result = Convert.ToDouble((string.IsNullOrEmpty(PaymentAmount))?"0":PaymentAmount) == Convert.ToDouble(Transaction.BalanceDue);
            //        if (result) return false;
            //        break;
            //    case MawaqifServiceType.PermitRenewal:
            //        result = Convert.ToDouble((string.IsNullOrEmpty(PaymentAmount)) ? "0" : PaymentAmount) <= Convert.ToDouble(Transaction.BalanceDue);
            //        break;
            //    case MawaqifServiceType.ViolationPayment:
            //        PaymentAmount = Convert.ToDouble(Transaction.BalanceDue).ToString();
            //        result = Convert.ToDouble((string.IsNullOrEmpty(PaymentAmount)) ? "0" : PaymentAmount) <= Convert.ToDouble(Transaction.BalanceDue);
            //        if (result) return false;
            //        break;
            //}

            return result;
        }

        public KioskCommand BackspaceCommand
        {
            get
            {
                return backspaceCommand;
            }

            private set
            {
                if (backspaceCommand != value)
                {
                    backspaceCommand = value;
                    OnPropertyChanged("BackspaceCommand");
                }
            }
        }

        private void ExecuteBackspaceCommand(EmptyCommandArgument arg)
        {
            RestartStateTimer();
            PaymentAmount = PaymentAmount.Substring(0, PaymentAmount.Length - 1);
        }

        private bool CanExecuteBackspaceCommand(EmptyCommandArgument arg)
        {
            return PaymentAmount.Length > 0;
        }

        protected override void ExecuteSubmitCommand(string param)
        {
            this.Transaction.PaymentAmount = PaymentAmount;
            OnKioskStateChanged(new KioskStateChangedEventArgs("submit"));
        }

        protected override bool CanExecuteSubmitCommand(string param)
        {
            bool result = false;

            if (PaymentAmount.Length >= State.MinAccNoLength && PaymentAmount.Length <= State.MaxAccNoLength)
                return true;

            //switch (Transaction.ServiceType)
            //{
            //    case MawaqifServiceType.None:
            //        break;
            //    case MawaqifServiceType.AccountTopUp:
            //        result = Convert.ToDouble((string.IsNullOrEmpty(PaymentAmount)) ? "0" : PaymentAmount) == Convert.ToDouble(Transaction.BalanceDue);
            //        break;
            //    case MawaqifServiceType.PermitRenewal:
            //        result = Convert.ToDouble((string.IsNullOrEmpty(PaymentAmount)) ? "0" : PaymentAmount) <= Convert.ToDouble(Transaction.BalanceDue);
            //        break;
            //    case MawaqifServiceType.ViolationPayment:
            //        PaymentAmount = Convert.ToDouble(Transaction.BalanceDue).ToString();
            //        result = Convert.ToDouble((string.IsNullOrEmpty(PaymentAmount)) ? "0" : PaymentAmount) <= Convert.ToDouble(Transaction.BalanceDue);
            //        break;
            //}

            return result;
        }

        public string PaymentAmount
        {
            get
            {
                if (this.Transaction == null || string.IsNullOrEmpty(this.Transaction.PaymentAmount))
                {
                    return string.Empty;
                }

                return this.Transaction.PaymentAmount;
            }

            private set
            {
                if (this.Transaction.PaymentAmount != value)
                {
                    this.Transaction.PaymentAmount = value;
                    OnPropertyChanged("CardPaymentAmount");
                    ReevaulateIfCommandsCanExecute();
                }
            }
        }
    }
}
