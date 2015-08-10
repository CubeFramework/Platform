using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MBMEKiosk.Infrastructure.Interfaces
{
    public interface IPrinter
    {
        void Init(bool simulate);

        void Print(FrameworkElement receipt, string transactionId);


        //void Print(byte[] barcode,  byte barcodelen);

        bool IsReady();

        string GetDetails(out short state, out short status);

        event Action<bool> ReceiptPrinted;
    }
}
