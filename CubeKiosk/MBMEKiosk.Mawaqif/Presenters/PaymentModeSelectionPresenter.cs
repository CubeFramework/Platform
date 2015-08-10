using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.BaseClasses;
using MBMEKiosk.Infrastructure.Events;
using System.Windows;
using MBMEKiosk.ObjectModel;
using MBMEKiosk.Infrastructure.ObjectModel;
using MBMEKioskLogger.LoggerClass;
using System.Diagnostics;

namespace MBMEKiosk.Mawaqif.Presenters
{
    public class PaymentModeSelectionPresenter : MawaqifPresenterBase
    {
        protected override void ExecuteSubmitCommand(string param)
        {

            Debug.Print(Transaction.ServiceType.ToString());
            if (param == "cash")
            {
                Transaction.CardPayment = false;
                Transaction.PaymentMode = (int)PaymentMode.CASH;
                OnKioskStateChanged(new KioskStateChangedEventArgs("cash"));
            }
            else if (param == "card")
            {

                Transaction.PaymentMode = (int)PaymentMode.CARD;
                if (string.IsNullOrEmpty(Transaction.PaymentAmount)) Transaction.PaymentAmount = Transaction.BalanceDue;
                this.Transaction.CardCycleInProgress = false;
                this.Transaction.CardPayment = true;
                OnKioskStateChanged(new KioskStateChangedEventArgs("card"));
            }
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
