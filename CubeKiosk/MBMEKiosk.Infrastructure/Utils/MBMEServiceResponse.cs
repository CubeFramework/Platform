using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKiosk.Infrastructure.Utils
{
    public enum MBMEServiceResponse
    {
        Success = 0,
        Failed = -2,
        BillerError = -3,
        ApplicationError = -4
    }
}
