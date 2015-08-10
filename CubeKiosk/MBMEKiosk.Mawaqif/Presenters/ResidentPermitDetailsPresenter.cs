using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.Events;

namespace MBMEKiosk.Mawaqif.Presenters
{
    public class ResidentPermitDetailsPresenter : MawaqifPresenterBase
    {
        protected override void ExecuteSubmitCommand(string param)
        {
            OnKioskStateChanged(new KioskStateChangedEventArgs(param));
        }

        protected override bool CanExecuteSubmitCommand(string param)
        {
            return State.KioskActions.Where(a => a.Name.ToLower() == param).Count() == 1;
        }

        
    }
}
