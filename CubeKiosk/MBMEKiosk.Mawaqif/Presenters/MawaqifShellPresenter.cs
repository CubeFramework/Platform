using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.BaseClasses;
using MBMEKiosk.Infrastructure.Commands;

namespace MBMEKiosk.Mawaqif.Presenters
{
    public class MawaqifShellPresenter : ShellPresenterBase
    {
        protected override bool CanExecuteHomeCommand(EmptyCommandArgument arg)
        {
            return !TransactionContext.CashCycleInProgress && State.KioskActions.Where(a => a.Name.ToLower() == "home").Count() == 1;
        }
    }
}
