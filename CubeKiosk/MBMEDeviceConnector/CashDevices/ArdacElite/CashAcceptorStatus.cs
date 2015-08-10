using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEDevices.CashDevices
{
    public enum CashAcceptorStatus
    {
        NoError = 1,
        Warning = 2,
        Error = 3,
        Initializing = 4
        
    }

    public enum CashAcceptorState
    {
        ONLINE = 1,
        OFFLINE = 2
    }
}
