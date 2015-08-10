using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.BaseClasses;
using MBMEKiosk.Infrastructure.Events;

namespace MBMEKiosk.Mawaqif.Presenters
{
    public class LanguageSelectionPresenter: MawaqifPresenterBase
    {
        protected override void ExecuteSubmitCommand(string param)
        {
             
             this.Transaction.SelectedLanguageKey = param;

             

             if (param == "english")
             {
                 this.Transaction.SelectedStyleKey = "styleEN";
                 this.Transaction.LocaleId = "en";
             }
             else
             {
                 this.TransactionContext.SelectedStyleKey = "styleAR";
                 this.Transaction.LocaleId = "ar";
             }

             this.Transaction.AccountNumber = string.Empty;
             this.Transaction.BalanceDue = "0.00";
             this.Transaction.AmountDue = "0.00";
             this.Transaction.AmountPaid = "0.00";

            OnKioskStateChanged(new KioskStateChangedEventArgs("submit"));
        }

        protected override bool CanExecuteSubmitCommand(string param)
        {
            return true;
        } 
    }
}
