using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKioskLogger.LoggerClass
{
    public class CashCycleLog
    {
        public CashCycleLog()
        {
            TimeIns = DateTime.Now;
            Uploaded = 0;
        }

        public int CashCycleId { get; set; }
        public string TxnId { get; set; }
        public DateTime TimeIns { get; set; }
        public string KioskId { get; set; }
        public int NoteVal { get; set; }
        public int NoteId { get; set; }
        public int Accepted { get; set; }
        public int Uploaded { get; set; }
        public string CurrencyCode { get; set; }
    }


}
