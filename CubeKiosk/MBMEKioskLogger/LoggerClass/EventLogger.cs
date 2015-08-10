using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKioskLogger.LoggerClass
{
    public class EventLogger
    {
        public EventLogger()
        {
            Uploaded = 0;
        }

        public int EventLogId { get; set; }
        public int KioskId { get; set; }
        public DateTime EventDtTm { get; set; }
        public string EventId { get; set; }
        public string Desc { get; set; }
        public short Uploaded { get; set; }
    }
}
