﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.Events;
using MBMEKiosk.Infrastructure.Commands;
using System.Windows;

namespace MBMEKiosk.Mawaqif.Presenters
{
    public class ConsumerNumberEntryPresenter : MawaqifPresenterBase
    {
        private KioskParameterisedCommand<string> addDigit;
        private KioskCommand backspaceCommand;

        public override System.Windows.FrameworkElement LoadXaml(Infrastructure.Interfaces.IDeviceAgent devices, ObjectModel.KioskState state, Infrastructure.ObjectModel.TransactionContextBase transactionContext)
        {
            transactionContext.AccountNumber = string.Empty;
            FrameworkElement viewGrid = base.LoadXaml(devices, state, transactionContext);

            if (string.IsNullOrEmpty(this.Transaction.AccountNumber))
            {
                CustomerNumber = this.State.LeadDigits;
            }

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

        public override void Deactivate()
        {
            base.Deactivate();

            // KS TODO: Implement a pub-sub mechanism, where subscription for a specific handler/token can be checked and unregistered.
            AddDigitCommand.ExecutionCompleted -= Command_ExecutionCompleted;
            BackspaceCommand.ExecutionCompleted -= Command_ExecutionCompleted;
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
            CustomerNumber += param;
        }

        private bool CanExecuteAddDigitCommand(string param)
        {
            return (CustomerNumber.Length < State.LeadDigits.Length && State.LeadDigits.StartsWith(CustomerNumber + param)) ||
                (CustomerNumber.Length >= State.LeadDigits.Length && CustomerNumber.Length < State.MaxAccNoLength);
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
            CustomerNumber = CustomerNumber.Substring(0, CustomerNumber.Length - 1);
        }

        private bool CanExecuteBackspaceCommand(EmptyCommandArgument arg)
        {
            return CustomerNumber.Length > State.LeadDigits.Length;
        }

        protected override void ExecuteSubmitCommand(string param)
        {
            // KS TODO: Add the entered value to tx context and call service agent to retrieve balance.
            ////this.TransactionContext.BalanceDue = "500.45";
            ////this.TransactionContext.AmountPaid = "0.00";
            ////this.TransactionContext.AmountDue = this.TransactionContext.BalanceDue;
            OnKioskStateChanged(new KioskStateChangedEventArgs("submit"));
        }

        protected override bool CanExecuteSubmitCommand(string param)
        {
            return CustomerNumber.Length >= State.MinAccNoLength && CustomerNumber.Length <= State.MaxAccNoLength;
        }

        public string CustomerNumber
        {
            get
            {
                if (this.TransactionContext == null || string.IsNullOrEmpty(this.TransactionContext.AccountNumber))
                {
                    return string.Empty;
                }

                return this.Transaction.AccountNumber;
            }

            private set
            {
                if (this.Transaction.AccountNumber != value)
                {
                    this.Transaction.AccountNumber = value;
                    OnPropertyChanged("CustomerNumber");
                    ReevaulateIfCommandsCanExecute();
                }
            }
        }

        public bool ShowRP
        {
            get
            {
                bool result = false;

                if (this.Transaction.ServiceType == MawaqifServiceType.PermitRenewal)
                    result = true;

                return result;
            }
        }
    }



}

