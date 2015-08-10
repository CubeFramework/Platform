using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using MBMEKiosk.Infrastructure.Events;
using MBMEKiosk.Infrastructure.Interfaces;
using MBMEKiosk.Infrastructure.ObjectModel;
using MBMEKiosk.ObjectModel;

namespace MBMEKiosk.Infrastructure.BaseClasses
{
    internal delegate void DUpdateUICPS(string action);

    public class CardPaymentPresenter : PresenterBase
    {
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected string action;
        protected bool boolPay;
        protected double dblerrUnknown;
        protected bool boolInvlaidCard;
        protected bool boolCheck;
        protected bool boolTransCancel;

        public override FrameworkElement LoadXaml(IDeviceAgent devices, KioskState state, TransactionContextBase transactionContext)
        {
            if (log.IsInfoEnabled) log.InfoFormat("{0} :  Load Xaml Started", DateTime.Now);
            dblerrUnknown = 0;
            FrameworkElement viewGrid = base.LoadXaml(devices, state, transactionContext);

            action = null;
            //this.TransactionContext.CardPaymentCycleInProgress = false;
            Devices.GetCardReader().SwipeCardEvent += OnSwipeCardEvent;
            Devices.GetCardReader().PaymentConfirmationEvent += OnPaymentConfirmationEvent;
            Devices.GetCardReader().PaymentFailedEvent += OnPaymentFailedEvent;
            Devices.GetCardReader().PaymentConfirmedEvent += OnPaymentConfirmedEvent;
            Devices.GetCardReader().ReceiptEvent += OnReceiptNotifiedEvent;
            Devices.GetCardReader().CardInValid += OnCardInvalid;
            Devices.GetCardReader().PaymentAsync(Convert.ToDouble(this.TransactionContext.AppliedFeeAmount));
            TransactionContext.Message = string.Empty;
            return viewGrid;
        }

        public override void Deactivate()
        {
            base.Deactivate();

            this.TransactionContext.CashCycleInProgress = false;
            if (Devices.GetCashAcceptor().IsEnabled())
            {
                Devices.GetCashAcceptor().DisableAsync();
            }

            Devices.GetCardReader().SwipeCardEvent -= OnSwipeCardEvent;
            Devices.GetCardReader().PaymentConfirmationEvent -= OnPaymentConfirmationEvent;
            Devices.GetCardReader().PaymentFailedEvent -= OnPaymentFailedEvent;
            Devices.GetCardReader().PaymentConfirmedEvent -= OnPaymentConfirmedEvent;
            Devices.GetCardReader().ReceiptEvent -= OnReceiptNotifiedEvent;
            Devices.GetCardReader().CardInValid -= OnCardInvalid;
        }

        protected override void ExecuteSubmitCommand(string param)
        {
            //this.TransactionContext.CardCycleInProgress = false;
            //Devices.GetCashAcceptor().DisableAsync();
            //base.ExecuteSubmitCommand(param);

            if (action != null)
                OnKioskStateChanged(new KioskStateChangedEventArgs(action));
            else
                OnKioskStateChanged(new KioskStateChangedEventArgs(SUBMITACTION));
        }

        protected override bool CanExecuteSubmitCommand(string param)
        {
            if (action != null)
                return true;

            if (this.State.Name == "CCPayConfirm")
                return boolPay;

            return false;
        }

        private void OnSwipeCardEvent(string text)
        {
            if (log.IsInfoEnabled) log.InfoFormat("{0} : {1} : {2}", DateTime.Now, text, action);

            //this.TransactionContext.Message = text;
            this.TransactionContext.Message = "CC_" + text.Replace(" ", "_");
            if (this.ViewGrid.TryFindResource(this.TransactionContext.Message) != null)
            {
                this.TransactionContext.Message = this.ViewGrid.TryFindResource(this.TransactionContext.Message) as string;
            }
            else
            {
                this.TransactionContext.Message = text;
            }

            if (text != "INSERT CARD ON TERMINAL")
                if (!this.TransactionContext.CardCycleInProgress)
                    this.TransactionContext.CardCycleInProgress = true;

            switch (text)
            {
                case "CUSTOMER PIN":
                    action = "generic";
                    break;
                case "TRANSACTION CANCELLED":
                    action = "txncancelled";
                    boolTransCancel = true;
                    break;
                case "REMOVE CARD FROM TERMINAL":
                    boolCheck = true;
                    action = "removecard";
                    break;
                case "INSERT CARD ON TERMINAL":
                    action = string.Empty;
                    break;
                case "PLEASE WAIT":
                    if (boolCheck)
                    {
                        action = string.Empty;
                        boolCheck = false;
                    }
                    else
                    {
                        action = "generic";
                    }
                    break;
                default:
                    action = "generic";
                    break;
            }

            if (action != string.Empty)
                ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUICPS(DisplayMessage), action);
            if (log.IsInfoEnabled) log.InfoFormat("Card Pay: {0}", text);
        }

        private void OnPaymentConfirmationEvent(double amount)
        {
            //this.TransactionContext.Message = string.Empty;
        }

        private void OnReceiptNotifiedEvent(string MerchantID, string TerminalID, string AuthNum, string AID, string AppName, string TVR, string TSI, string ACInfo, string AC, string CardNo)
        {
            if (!string.IsNullOrEmpty(MerchantID))
                this.TransactionContext.MerchantID = MerchantID.Trim();
            if (!string.IsNullOrEmpty(TerminalID))
                this.TransactionContext.TerminalID = TerminalID.Trim();
            if (!string.IsNullOrEmpty(AuthNum))
                this.TransactionContext.AuthNum = AuthNum.Trim();
            if (!string.IsNullOrEmpty(AID))
                this.TransactionContext.AID = AID.Trim();
            //if (!string.IsNullOrEmpty(AppName))
                this.TransactionContext.AppName = AppName.Trim();
            if (!string.IsNullOrEmpty(TVR))
                this.TransactionContext.TVR = TVR.Trim();
            if (!string.IsNullOrEmpty(TSI))
                this.TransactionContext.TSI = TSI.Trim();
            if (!string.IsNullOrEmpty(ACInfo))
                this.TransactionContext.ACInfo = ACInfo.Trim();
            if (!string.IsNullOrEmpty(AC))
                this.TransactionContext.AC = AC.Trim();
            if (!string.IsNullOrEmpty(CardNo))
                this.TransactionContext.CardNo = CardNo.Trim();
        }

        private void OnPaymentConfirmedEvent(double amount)
        {

            this.TransactionContext.AmountPaid = string.Format("{0:0.00}", amount);
            this.TransactionContext.AmountDue = string.Format("{0:0.00}", double.Parse(this.TransactionContext.BalanceDue) - amount);
            //this.TransactionContext.AuthNum = authNum;
            //this.TransactionContext.TerminalID = terminalID;
            //this.TransactionContext.MerchantID = merchantID;
            //this.TransactionContext.Message = "Payment Authorized/Confirmed";
            this.TransactionContext.Message = this.ViewGrid.TryFindResource("CC_PaymentConfirmed") as string;
            boolPay = true;
            if (log.IsErrorEnabled) log.Error("Card Payment Successful for Txn Ref Num - " + this.TransactionContext.Id);
            if (log.IsErrorEnabled) log.Error("Amount Paid - " + this.TransactionContext.AmountPaid);
            if (log.IsErrorEnabled) log.Error("Message - " + this.TransactionContext.Message);
            action = SUBMITACTION;
            ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUICPS(DisplayMessage), action);

            //OnKioskStateChanged(new KioskStateChangedEventArgs(SUBMITACTION));
        }

        protected virtual void OnPaymentFailedEvent(double amount)
        {
            this.TransactionContext.AmountPaid = "0.00";
            this.TransactionContext.AmountDue = string.Format("{0:0.00}", double.Parse(this.TransactionContext.BalanceDue) - 0.00);



            if ((boolInvlaidCard != false) && (this.State.Name.ToLower() == "cardpayment"))
            {
                if (dblerrUnknown == 33)
                    TransactionContext.Message = "Ensure card is inserted correctly";
                if (dblerrUnknown == 13)
                    TransactionContext.Message = "An Unknown Card";
                                
                action = "removecard";
            }
            else if (boolTransCancel)
            {
                action = string.Empty;
                
            }
            else
            {
                //this.TransactionContext.Message = "Payment Failed";
                this.TransactionContext.Message = this.ViewGrid.TryFindResource("CC_PaymentFailed") as string;
                action = ERRORACTION;
            }
            
            if (log.IsErrorEnabled) log.Error("Card Payment Successful for Txn Ref Num - " + this.TransactionContext.Id);
            if (log.IsErrorEnabled) log.Error("Amount Paid - " + this.TransactionContext.AppliedFeeAmount);
            if (log.IsErrorEnabled) log.Error("Message - " + this.TransactionContext.Message);
            Thread.Sleep(2000);
            ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUICPS(DisplayMessage), action);

            //OnKioskStateChanged(new KioskStateChangedEventArgs("error"));
        }

        protected virtual void OnCardInvalid(short value)
        {
            dblerrUnknown = value;
            
            if (log.IsErrorEnabled) log.Error("Card is Invalid for Txn Ref Num - " + this.TransactionContext.Id);
            this.TransactionContext.AmountPaid = "0.00";
            this.TransactionContext.AmountDue = string.Format("{0:0.00}", double.Parse(this.TransactionContext.BalanceDue) - 0.00);
            //this.TransactionContext.Message = "Card is Invalid";
            

            if (dblerrUnknown == 33)// || dblerrUnknown == 13 || dblerrUnknown == 6) //Error Unknown or Invalid Card
            {
                this.TransactionContext.Message = this.ViewGrid.TryFindResource("CC_CardInvalid") as string;
                boolInvlaidCard = true;
                if (log.IsErrorEnabled) log.Error("Card is Invalid for Txn Ref Num - " + this.TransactionContext.Id + "ErrorCode : " + dblerrUnknown);
                action = string.Empty;
            }

            Thread.Sleep(2000);
            if (action != string.Empty)
                ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUICPS(DisplayMessage), action);
        }
        
        protected void DisplayMessage(string action)
        {
            this.ReevaulateIfCommandsCanExecute();
            this.OnRequestLayoutUpdate();
            if (action != string.Empty || action != SUBMITACTION)
            {
                if (log.IsInfoEnabled) log.Info(this.State.Name);
                Thread.Sleep(2000);
                OnKioskStateChanged(new KioskStateChangedEventArgs(action));
            }
        }

         

        protected override void OnTimeOut(object o, EventArgs args)
        {
            this.TransactionContext.CardCycleInProgress = false;
            base.OnTimeOut(o, args);
        }

        
    }
}
