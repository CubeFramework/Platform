using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEDevices.CashDevices
{
    public enum CashAcceptorEventType
    {
        // KS TODO : Update this or check if this needs to be kept configurable.
        RETURNED = 1,
        REJECTED = 2,
        ESCROW = 23,
        STACKED = 24,
    }
}
