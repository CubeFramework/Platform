using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKiosk.Infrastructure.Interfaces
{
    public interface IDeviceAgent
    {
        ICashAcceptor GetCashAcceptor();

        IPrinter GetPrinter();

        ICardReader GetCardReader();

        IUSBKeyboard GetUSBKeyboard();
    }
}
