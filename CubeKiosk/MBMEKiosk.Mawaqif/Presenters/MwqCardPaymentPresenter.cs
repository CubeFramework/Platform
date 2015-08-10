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
using MBMEKiosk.Infrastructure.BaseClasses;


namespace MBMEKiosk.Mawaqif.Presenters
{
    internal delegate void DUpdateUICPS(string action);

    public class MwqCardPaymentPresenter : CardPaymentPresenter
    {
        
        public MawaqifTransaction Transaction
        {
            get
            {
                return this.TransactionContext as MawaqifTransaction;
            }
        }
        
        protected override void OnPaymentFailedEvent(double amount)
        {
            this.Transaction.AmountPaid = "0.00";
            bool isStateCardPayment = false;
            this.Transaction.AmountDue = string.Format("{0:0.00}", double.Parse(this.Transaction.BalanceDue) - 0.00);

            switch (this.Transaction.ServiceType)
            {
                case MawaqifServiceType.None:
                    break;
                case MawaqifServiceType.AccountTopUp:
                    isStateCardPayment = this.State.Name.ToLower() == "cardpayment";
                    break;
                case MawaqifServiceType.PermitRenewal:
                    isStateCardPayment = this.State.Name.ToLower() == "rpcardpayment";
                    break;
                case MawaqifServiceType.ViolationPayment:
                    isStateCardPayment = this.State.Name.ToLower() == "pvtcardpayment";
                    break;
                default:
                    break;
            }

            if ((boolInvlaidCard != false) && (isStateCardPayment))
            {
                if (dblerrUnknown == 33)
                    Transaction.Message = "Ensure card is inserted correctly";
                if (dblerrUnknown == 13)
                    Transaction.Message = "An Unknown Card";

                action = "removecard";
            }
            else if (boolTransCancel)
                action = string.Empty;
            else
            {
                this.Transaction.Message = "Payment Failed";
                action = ERRORACTION;
            }
            
            if (log.IsErrorEnabled) log.Error("Card Payment Successful for Txn Ref Num - " + this.Transaction.Id);
            if (log.IsErrorEnabled) log.Error("Amount Paid - " + this.Transaction.PaymentAmount);
            if (log.IsErrorEnabled) log.Error("Message - " + this.Transaction.Message);
            ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUICPS(DisplayMessage), action);
        
        }

         

    }
}
