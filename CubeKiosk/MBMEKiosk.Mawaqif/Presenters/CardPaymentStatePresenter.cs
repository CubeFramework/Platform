using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using MBMEKiosk.Infrastructure.Interfaces;
using MBMEKiosk.Infrastructure.ObjectModel;
using MBMEKiosk.ObjectModel;
using System.Threading;
using MBMEKiosk.Infrastructure.Events;
using MBMEKioskLogger.LoggerClass;
using MBMEKioskLogger.Logger;
using log4net;
using System.Diagnostics;


namespace MBMEKiosk.Mawaqif.Presenters
{
    //internal delegate void DUpdateUICPS(string action);

    public class CardPaymentStatePresenter : MawaqifPresenterBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string DisplayedDenominations;
        private string action;
        public override FrameworkElement LoadXaml(IDeviceAgent devices, KioskState state, TransactionContextBase transactionContext)
        {
            FrameworkElement viewGrid = base.LoadXaml(devices, state, transactionContext);
            action = null;
            //this.Transaction.CardPaymentCycleInProgress = false;
            Devices.GetCardReader().SwipeCardEvent += OnSwipeCardEvent;
            Devices.GetCardReader().PaymentConfirmationEvent += OnPaymentConfirmationEvent;
            Devices.GetCardReader().PaymentFailedEvent += OnPaymentFailedEvent;
            Devices.GetCardReader().PaymentConfirmedEvent += OnPaymentConfirmedEvent;
            Devices.GetCardReader().ReceiptEvent += OnReceiptNotifiedEvent;
            //Devices.GetCardReader().CardInValid += OnCardInvalid;

            //Devices.GetCardReader().PaymentAsync(Convert.ToDouble(this.Transaction.PaymentAmount));
            return viewGrid;
        }

        public override void Deactivate()
        {
            base.Deactivate();

            this.Transaction.CashCycleInProgress = false;
            if (Devices.GetCashAcceptor().IsEnabled())
            {
                Devices.GetCashAcceptor().DisableAsync();
            }

            Devices.GetCardReader().SwipeCardEvent -= OnSwipeCardEvent;
            Devices.GetCardReader().PaymentConfirmationEvent -= OnPaymentConfirmationEvent;
            Devices.GetCardReader().PaymentFailedEvent -= OnPaymentFailedEvent;
            Devices.GetCardReader().PaymentConfirmedEvent -= OnPaymentConfirmedEvent;
            Devices.GetCardReader().ReceiptEvent -= OnReceiptNotifiedEvent;
            //Devices.GetCardReader().CardInValid -= OnCardInvalid;

        }

        protected override void ExecuteSubmitCommand(string param)
        {
            //this.Transaction.CardCycleInProgress = false;
            //Devices.GetCashAcceptor().DisableAsync();
            //base.ExecuteSubmitCommand(param);
            OnKioskStateChanged(new KioskStateChangedEventArgs(action));
        }

        protected override bool CanExecuteSubmitCommand(string param)
        {
            if (action != null)
                return true;
            else
                return false;
        }

        private void OnSwipeCardEvent(string text)
        {
            this.Transaction.Message = text;
            //ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUICPS(DisplayMessage), SUBMITACTION);

            switch (text)
            {
                case "CUSTOMER PIN":
                    action = "enterpin";
                    break;

                case "REMOVE CARD FROM TERMINAL":
                    action = "removecard";

                    if (text == "REMOVE CARD FROM TERMINAL" && this.State.Name.ToUpper() == "REMOVECARD")
                        action = string.Empty;

                    break;

                case "INSERT CARD ON TERMINAL":
                    action = string.Empty;
                    break;

                default:
                    if (this.State.Name == "GenericCardState")
                        action = string.Empty;
                    else
                        action = "generic";


                    break;
            }

            Trace.WriteLine(string.Format("Text: {0} action : {1} State: {2} Time: {3}", text, action, this.State.Name, DateTime.Now));

            if (action != string.Empty)
                ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUICPS(DisplayMessage), action);
        }

        private void OnPaymentConfirmationEvent(double amount)
        {
            //this.Transaction.Message
        }

        private void OnReceiptNotifiedEvent(string MerchantID, string TerminalID, string AuthNum, string AID, string AppName, string TVR, string TSI, string ACInfo, string AC, string CardNo)
        {

            if (!string.IsNullOrEmpty(MerchantID))
                this.Transaction.MerchantID = MerchantID.Trim();
            if (!string.IsNullOrEmpty(TerminalID))
                this.Transaction.TerminalID = TerminalID.Trim();
            if (!string.IsNullOrEmpty(AuthNum))
                this.Transaction.AuthNum = AuthNum.Trim();
            if (!string.IsNullOrEmpty(AID))
                this.Transaction.AID = AID.Trim();
            if (!string.IsNullOrEmpty(AppName))
                this.Transaction.AppName = AppName.Trim();
            if (!string.IsNullOrEmpty(TVR))
                this.Transaction.TVR = TVR.Trim();
            if (!string.IsNullOrEmpty(TSI))
                this.Transaction.TSI = TSI.Trim();
            if (!string.IsNullOrEmpty(ACInfo))
                this.Transaction.ACInfo = ACInfo.Trim();
            if (!string.IsNullOrEmpty(AC))
                this.Transaction.AC = AC.Trim();
            if (!string.IsNullOrEmpty(CardNo))
                this.Transaction.CardNo = CardNo.Trim();
        }

        private void OnPaymentConfirmedEvent(double amount)
        {
            this.Transaction.AmountPaid = string.Format("{0:0.00}", amount);
            this.Transaction.AmountDue = string.Format("{0:0.00}", double.Parse(this.Transaction.BalanceDue) - amount);
            //this.Transaction.AuthNum = authNum;
            //this.Transaction.TerminalID = terminalID;
            //this.Transaction.MerchantID = merchantID;
            this.Transaction.Message = "Payment Authorized/Confirmed";
            this.Transaction.TransactionFailed = false;
            if (log.IsErrorEnabled) log.Error("Card Payment Successful for Txn Ref Num - " + this.Transaction.Id);
            if (log.IsErrorEnabled) log.Error("Amount Paid - " + this.Transaction.AmountPaid);
            if (log.IsErrorEnabled) log.Error("Message - " + this.Transaction.Message);
            action = "submit";
            Thread.Sleep(2000);
            ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUICPS(DisplayMessage), action);

            //OnKioskStateChanged(new KioskStateChangedEventArgs("submit"));
        }

        private void OnPaymentFailedEvent(double amount)
        {
            this.Transaction.AmountPaid = "0.00";
            this.Transaction.AmountDue = string.Format("{0:0.00}", double.Parse(this.Transaction.BalanceDue) - 0.00);
            this.Transaction.Message = "Payment Failed";
            this.Transaction.TransactionFailed = true;
            //this.Transaction.AuthNum = authNum;
            //this.Transaction.TerminalID = terminalID;
            //this.Transaction.MerchantID = merchantID;
            if (log.IsErrorEnabled) log.Error("Card Payment Successful for Txn Ref Num - " + this.Transaction.Id);
            if (log.IsErrorEnabled) log.Error("Amount Paid - " + this.Transaction.PaymentAmount);
            if (log.IsErrorEnabled) log.Error("Message - " + this.Transaction.Message);
            action = "submit";
            Thread.Sleep(2000);
            ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUICPS(DisplayMessage), action);

            //OnKioskStateChanged(new KioskStateChangedEventArgs("error"));
        }

        //private void OnCardInvalid(short value)
        //{
        //    if (log.IsErrorEnabled) log.Error("Card is Invalid for Txn Ref Num - " + this.Transaction.Id);
        //    this.Transaction.AmountPaid = "0.00";
        //    this.Transaction.AmountDue = string.Format("{0:0.00}", double.Parse(this.Transaction.BalanceDue) - 0.00);
        //    this.Transaction.Message = "Card is Invalid";
        //    action = "removecard";
        //    Thread.Sleep(2000);
        //    ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUICPS(DisplayMessage), action);
        //}

        //private void OnCashInserted()
        //{
        //    RestartStateTimer();
        //    ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUI(UpdateAmountPaidAndDue), 0, string.Empty, false);
        //}

        private void DisplayMessage(string action)
        {
            this.ReevaulateIfCommandsCanExecute();
            this.OnRequestLayoutUpdate();
            //if (action != string.Empty || action != SUBMITACTION)
            //{
            //Thread.Sleep(2000);
            OnKioskStateChanged(new KioskStateChangedEventArgs(action));
            //}
        }
        //private void UpdateAmountPaidAndDue(int denominationStacked, string allowedDenominations, bool isCashCycleCompleted)
        //{
        //    this.DisplayedDenominations = allowedDenominations;
        //    if (!this.Transaction.CashCycleInProgress)
        //    {
        //        this.Transaction.CashCycleInProgress = true;
        //        this.Transaction.CurrentlyAllowedNotes = allowedDenominations;
        //        this.OnRequestLayoutUpdate();
        //        return;
        //    }

        //    double updatedAmountPaid = double.Parse(this.Transaction.AmountPaid) + denominationStacked;
        //    this.Transaction.AmountPaid = string.Format("{0:0.00}", updatedAmountPaid);
        //    this.Transaction.AmountDue = string.Format("{0:0.00}", double.Parse(this.Transaction.BalanceDue) - updatedAmountPaid);
        //    if (!this.Transaction.IsFinePayment || (double.Parse(this.Transaction.BalanceDue) - updatedAmountPaid > 0))
        //        this.Transaction.CurrentlyAllowedNotes = allowedDenominations;
        //    else
        //        this.Transaction.CurrentlyAllowedNotes = string.Empty;
        //    if (this.Transaction.IsFinePayment && updatedAmountPaid.ToString() == this.MaxAmount)
        //    {
        //        OnKioskStateChanged(new KioskStateChangedEventArgs("submit"));
        //    }

        //    this.ReevaulateIfCommandsCanExecute();
        //    this.OnRequestLayoutUpdate();
        //}

        protected override void OnTimeOut(object o, EventArgs args)
        {
            //this.Transaction.CardCycleInProgress = false;
            //Devices.GetCashAcceptor().DisableAsync();

            //if (double.Parse(this.Transaction.AmountPaid) > 0)
            //{
            //    if (StateTimer != null)
            //    {
            //        StateTimer.Stop();
            //        StateTimer.Tick -= OnTimeOut;
            //        StateTimer = null;
            //    }

            //    if (State.KioskActions.Where(a => a.Name.ToLower() == "autosubmit").Count() == 1)
            //    {
            //        OnKioskStateChanged(new KioskStateChangedEventArgs("autosubmit"));
            //    }
            //    else
            //    {
            //        OnKioskStateChanged(new KioskStateChangedEventArgs("submit"));
            //    }
            //}
            //else
            //{
            base.OnTimeOut(o, args);
            //}
        }


    }
}
