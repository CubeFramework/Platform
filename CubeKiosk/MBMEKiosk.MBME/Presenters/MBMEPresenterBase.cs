using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.BaseClasses;

namespace MBMEKiosk.MBME.Presenters
{
    public class MBMEPresenterBase : PresenterBase
    {
        public MBMETransaction Transaction
        {
            get
            {
                return this.TransactionContext as MBMETransaction;
            }
        }

        
    }
}
