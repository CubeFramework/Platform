using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using MBMEKiosk.Infrastructure.BaseClasses;
using MBMEKiosk.Infrastructure.Events;
using MBMEKiosk.Infrastructure.Interfaces;
using MBMEKiosk.Infrastructure.ObjectModel;
using MBMEKiosk.ObjectModel;
using MBMEKiosk.Infrastructure.Proxies.ProxyCardService;
using System.Configuration;


namespace MBMEKiosk.Infrastructure.BaseClasses
{
    //internal delegate void DUpdateUICPS(string action);

    public class CardPaymentStatePresenter : PresenterBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string DisplayedDenominations;
        protected string action;
        protected bool boolPay;
        protected int dblerrUnknown;
        protected bool boolInvlaidCard;
        public override FrameworkElement LoadXaml(IDeviceAgent devices, KioskState state, TransactionContextBase transactionContext)
        {
             
            FrameworkElement viewGrid = base.LoadXaml(devices, state, transactionContext);

            action = null;
            //this.TransactionContext.CardPaymentCycleInProgress = false;
            Devices.GetCardReader().SwipeCardEvent += OnSwipeCardEvent;
            Devices.GetCardReader().PaymentConfirmationEvent += OnPaymentConfirmationEvent;
            Devices.GetCardReader().PaymentFailedEvent += OnPaymentFailedEvent;
            Devices.GetCardReader().PaymentConfirmedEvent += OnPaymentConfirmedEvent;
            Devices.GetCardReader().ReceiptEvent += OnReceiptNotifiedEvent;
            Devices.GetCardReader().CardInValid += OnCardInvalid;

            //This Line is commented to disable multiple payments in the transaction 
            //Devices.GetCardReader().PaymentAsync(Convert.ToDouble(this.TransactionContext.PaymentAmount));

            using (FileStream stream = new FileStream(@"app001.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(string.Format("{0} : State : {1} : Load Xaml Ended", DateTime.Now, this.State.XamlPath));
                writer.Flush();
                writer.Close();
            }

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

            switch (text)
            {
                case "CUSTOMER PIN":
                    action = "enterpin";

                    break;
                case "TRANSACTION CANCELLED":
                    action = "txncancelled";
                    break;
                case "REMOVE CARD FROM TERMINAL":
                    action = "removecard";

                    if (text == "REMOVE CARD FROM TERMINAL" &&
                        (this.State.Name.ToUpper() == "REMOVECARD") || (this.State.Name.ToUpper() == "FINEREMOVECARD"))
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

            if (log.IsInfoEnabled) log.InfoFormat("Card Pay: {0}", text);

            //OnKioskStateChanged(new KioskStateChangedEventArgs(SUBMITACTION));
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
            if (!string.IsNullOrEmpty(AppName))
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

        protected virtual void OnPaymentConfirmedEvent(double amount)
        {
            this.TransactionContext.CardCycleInProgress = false;
            this.TransactionContext.AmountPaid = string.Format("{0:0.00}", (amount-this.TransactionContext.AppliedFeeValue));
            this.TransactionContext.AmountDue = string.Format("{0:0.00}", double.Parse(this.TransactionContext.BalanceDue) - amount);
            //this.TransactionContext.Message = "Payment Authorized/Confirmed";
            this.TransactionContext.Message = "CC_PaymentConfirmed";
            if (this.ViewGrid.TryFindResource(this.TransactionContext.Message) != null)
            {
                this.TransactionContext.Message = this.ViewGrid.TryFindResource(this.TransactionContext.Message) as string;
            }
            else
                this.TransactionContext.Message = "Payment Authorized/Confirmed";

            boolPay = true;
            PostCardDetails();
            if (log.IsErrorEnabled) log.Error("Card Payment Successful for Txn Ref Num - " + this.TransactionContext.Id);
            if (log.IsErrorEnabled) log.Error("Amount Paid - " + this.TransactionContext.AmountPaid);
            if (log.IsErrorEnabled) log.Error("Message - " + this.TransactionContext.Message);
            action = SUBMITACTION;
             
            //Thread.Sleep(2000);
            ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUICPS(DisplayMessage), action);

            //OnKioskStateChanged(new KioskStateChangedEventArgs(SUBMITACTION));
        }

        private void PostCardDetails()
        {
            if (log.IsInfoEnabled) log.Info("Postcarddetails started");
            
            try
            {
                CardRequest request = new CardRequest
                {
                    Ac = this.TransactionContext.AC,
                    AcInfo = this.TransactionContext.ACInfo,
                    AID = this.TransactionContext.AID,
                    AppName = this.TransactionContext.AppName,
                    AuthNum = this.TransactionContext.AuthNum,
                    KioskTxnRefNum = this.TransactionContext.Id,
                    MerchantId = this.TransactionContext.MerchantID,
                    TerminalId = this.TransactionContext.TerminalID,
                    TSI = this.TransactionContext.TSI,
                    TVR = this.TransactionContext.TVR,
                    CardNumber = this.TransactionContext.CardNo
                };

                if (Boolean.Parse(ConfigurationManager.AppSettings["StandAloneMode"]))
                {
                    Thread.Sleep(1000);
                    this.TransactionContext.AppliedFee = 2.35;
                    if (log.IsInfoEnabled) log.InfoFormat("Post Card Details in Stimulation Mode : {0}", TransactionContext.Id);
                }
                else
                {
                    using (CardServiceClient client = new CardServiceClient())
                    {
                        int result = client.PostCardDetails(request);

                        if (result > 1)
                        {
                            if (log.IsInfoEnabled) log.InfoFormat("Card Details posted successfully for TxnRefnum : {0}", TransactionContext.Id);

                        }
                        else
                        {
                            if (log.IsInfoEnabled) log.InfoFormat("Card Details Posting failed for TxnRefnum : {0}", TransactionContext.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.ErrorFormat("Error while Posting card details : {0}", ex.Message);
            }

            if (log.IsInfoEnabled) log.Info("Postcarddetails completed");
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

        private void OnPaymentFailedEvent(double amount)
        {
            this.TransactionContext.AmountPaid = "0.00";
            this.TransactionContext.CardCycleInProgress = false;
            this.TransactionContext.AmountDue = string.Format("{0:0.00}", double.Parse(this.TransactionContext.BalanceDue) - 0.00);
            if (boolInvlaidCard == false)
            {
                this.TransactionContext.Message = "Payment Failed";
                action = SUBMITACTION;
                PostCardDetails();
            }
            else
            {
                if (dblerrUnknown == 33 && this.State.Name.ToLower() == "cardpayment")
                    TransactionContext.Message = "Ensure card is inserted correctly";
                if (dblerrUnknown == 13 && this.State.Name.ToLower() == "cardpayment")
                    TransactionContext.Message = "Unknown Card";

                 
            }
            
            //this.TransactionContext.AuthNum = authNum;
            //this.TransactionContext.TerminalID = terminalID;
            //this.TransactionContext.MerchantID = merchantID;
            if (log.IsErrorEnabled) log.Error("Card Payment Successful for Txn Ref Num - " + this.TransactionContext.Id);
            if (log.IsErrorEnabled) log.Error("Amount Paid - " + this.TransactionContext.PaymentAmount);
            if (log.IsErrorEnabled) log.Error("Message - " + this.TransactionContext.Message);
            //action = SUBMITACTION;
            Thread.Sleep(2000);
            ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUICPS(DisplayMessage), action);

            //OnKioskStateChanged(new KioskStateChangedEventArgs("error"));
        }

        protected void DisplayMessage(string action)
        {
            this.ReevaulateIfCommandsCanExecute();
            this.OnRequestLayoutUpdate();
            OnKioskStateChanged(new KioskStateChangedEventArgs(action));
        }
        
        protected override void OnTimeOut(object o, EventArgs args)
        {
            base.OnTimeOut(o, args);
        }

    }
}
