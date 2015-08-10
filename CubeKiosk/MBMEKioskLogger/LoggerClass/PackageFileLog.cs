using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKioskLogger.LoggerClass
{
    public class PackageFileLog
    {
        public string ReleaseVersion { get; set; }
        public int PackageId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public string FileSize { get; set; }
        public bool Complete { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int DownloadDetailId { get; set; }
        public int PkgDetId { get; set; }
        //// Added By JK and AJ on 18/07/2012
        public int PackageDetailId { get; set; }
        public int KioskPackageId { get; set; }
        //// Added By JK and AJ on 18/07/2012
        public bool CommandExecuted { get; set; }
        public DateTime ExecutionDatetime { get; set; }
        public DateTime IssuedDatetime { get; set; }

        public int QueueId { get; set; }
    }
}
