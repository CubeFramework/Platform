using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKioskLogger.Logger
{
    public class PackageMasterInfo
    {
        // Added By JK and AJ on 18/07/2012
        public int KioskPackageId { get; set; }
        // Added By JK and AJ on 18/07/2012
        public int PackageId { get; set; }
        public string ReleaseVersion { get; set; }
        public bool Deployed { get; set; }

        public bool CommandExecuted { get; set; }
        public DateTime ExecutionDateTime { get; set; }

        public int QueueId { get; set; }

    }
}
