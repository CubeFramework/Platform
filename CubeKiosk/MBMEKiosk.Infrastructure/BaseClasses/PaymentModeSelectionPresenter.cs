using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.Events;

namespace MBMEKiosk.Infrastructure.BaseClasses
{
    /// <summary>
    /// Presenter Class for PaymentModeSelection Xaml
    /// </summary>
    public class PaymentModeSelectionPresenter : PresenterBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected override void ExecuteSubmitCommand(string param)
        {
            if (log.IsInfoEnabled) log.Info("inside PaymentModeSelectionPresenter.param: " + param);

            if (string.IsNullOrEmpty(param))
                throw new Exception("Requires cash or card as commandparameter in paymentmodeselection");
             

            if (param == CASHACTION)
            {
                TransactionContext.CardPayment = false;
                this.TransactionContext.PaymentMode = 1; //Cash
                OnKioskStateChanged(new KioskStateChangedEventArgs(CASHACTION));
            }
            else if (param == CARDACTION)
            {
                if (string.IsNullOrEmpty(TransactionContext.PaymentAmount)) TransactionContext.PaymentAmount = TransactionContext.BalanceDue;
                this.TransactionContext.CardCycleInProgress = false;
                this.TransactionContext.CardPayment = true;
                this.TransactionContext.PaymentMode = 2; //Card
                OnKioskStateChanged(new KioskStateChangedEventArgs(CARDACTION));
            }

            if (log.IsInfoEnabled) log.Info("inside PaymentModeSelectionPresenter.ExecuteSubmitCommand done.");
        }

        protected override bool CanExecuteSubmitCommand(string param)
        {
            bool result = false;

            switch (param)
            {
                case CASHACTION:
                    if (Devices.GetCashAcceptor().IsReady())
                        result = base.EnableCashPayment;
                    else
                        result = false;
                    break;
                case CARDACTION:
                    if (Devices.GetCardReader().IsReady())
                        result = base.EnableCardPayment;
                    else
                        result = false;
                    break;
            }

            return result;
        }
    }
}
