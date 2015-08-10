using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.ObjectModel;
using System.Configuration;
using MBMEKioskLogger.LoggerClass;
using MBMEKioskLogger.Logger;
using System.Globalization;
using System.Threading;

namespace MBMEKiosk.Mawaqif.Presenters
{
    public class MawaqifTransaction : TransactionContextBase
    {

        
        public string CashDeviceStatus { get; set; }

        public string PrinterStatus { get; set; }

        public string ServiceStatus { get; set; }

        public MawaqifServiceType ServiceType { get; set; }

        public string PlateNumber { get; set; }

        public string IssueDate { get; set; }

        public string ExpiryDate { get; set; }

        private string currentlyAllowedNotes;
        

        public string CurrentlyAllowedNotes
        {
            get
            {
                return currentlyAllowedNotes;
            }

            set
            {
                if (string.Compare(currentlyAllowedNotes, value, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    currentlyAllowedNotes = value;
                    OnPropertyChanged("CurrentlyAllowedNotes");
                }
            }
        }

        

        

       


        public string Country { get; set; }
        public string Category { get; set; }
        public string PVTType { get; set; } 

        public string LocaleId { get; set; }

        public string StageMessage { get; set; }

        public  bool StimulateBackend
        {
            get
            {
                bool boolStimulateBackend = bool.Parse(ConfigurationManager.AppSettings["StimulateBackend"].ToString());
                return boolStimulateBackend;
            }
        }

        public bool TransactionFailed { get; set; }

        public string TransFailedMessage
        {
            get
            {
                string strRes = null;
                //if (this.TransactionFailed)
                    strRes = this.Message;

                return strRes;
            }
        }

        public bool ShowTextBlock
        {
            get
            {
                if (this.SelectedLanguageKey == "english")
                    return true;
                else
                    return false;
            }
        }

        public bool ShowCurrentBalance
        {
            get
            {
                bool currbal = false;
                switch (ServiceType)
                {
                    case MawaqifServiceType.AccountTopUp:
                        currbal = true;
                        break;
                    case MawaqifServiceType.ViolationPayment:
                        if(Convert.ToDouble(this.AmountDue) > 0) currbal = true;
                        break;
                    default:
                        currbal = false;
                        break;
                }
                return currbal;
            }
        }

        /// <summary>
        /// Used in the Mawaqif Receipt
        /// </summary>
        public string AccountNumberLabelText { get; set; }
         

        public string MawaqifDate
        {
            get
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
                if (SelectedLanguageKey == "arabic")
                    return Date.ToString("HH:mm:ss yyyy/MM/dd");
                else
                    return Date.ToString("dd/MM/yyyy HH:mm:ss");
            }
        }
        
        /// <summary>
        /// Used in Mawaqif Receipt 
        /// </summary>
        public string ServiceAmountLabelText { get; set; }

        public string CurrBalanceLabelText { get; set; }

        public string CurrBalanceLabelTextAR { get; set; }

        public string ReceiptServiceNameKeyAR { get; set; }

        public string AccountNumberLabelTextAR { get; set; }

        public string ServiceAmountLabelTextAR { get; set; }

        //public bool CardCycleInProgress { get; set; }

        //public PaymentMode PaymentMode { get; set; }
    }
}
