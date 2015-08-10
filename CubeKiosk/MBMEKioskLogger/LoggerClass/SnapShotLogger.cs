using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKioskLogger.LoggerClass
{
    public class SnapShotLogger
    {
        public int Id { get; set; }

        public int BillerServiceId { get; set; }

        public DateTime ChangedDate { get; set; }

        public int SnapSortId { get; set; }

        public string Type { get; set; }

        public string Path { get; set; }
    }
}
