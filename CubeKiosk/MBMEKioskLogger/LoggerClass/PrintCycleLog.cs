using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKioskLogger.LoggerClass
{
    public class PrintCycleLog
    {
        public PrintCycleLog()
        {
            TimePrt = DateTime.Now;
            Uploaded = 0;
        }

        public string PrnCycleId { get; set; }
        public string TxnId { get; set; }
        public DateTime TimePrt { get; set; }
        public string ReceiptId { get; set; }
        public short ReceiptTaken { get; set; }
        public short Uploaded { get; set; }
        public long KioskId { get; set; }
    }


}
