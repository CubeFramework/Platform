using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.ObjectModel;

namespace MBMEKiosk.MBME.Presenters
{
    public class MBMETransaction : TransactionContextBase
    {
        private List<KioskService> _kioskservices;
        public string CashDeviceStatus { get; set; }

        public string PrinterStatus { get; set; }

        public string ServiceStatus { get; set; }

        public string LogonStatus { get; set; }

        public short KioskMode { get; set; }

        public string CardReaderStatus { get; set; }

        public string PackDispenserStatus { get; set; }

        public List<KioskService> KioskServices
        {
            get
            {
                return _kioskservices;
            }
            set
            {
                if (value != null)
                {
                    _kioskservices = value;
                    OnPropertyChanged("KioskServices");
                }
            }
        }
    }
}
