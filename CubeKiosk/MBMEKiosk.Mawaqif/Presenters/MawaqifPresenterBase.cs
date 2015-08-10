using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.BaseClasses;
using MBMEKioskLogger.Logger;
using MBMEKioskLogger.LoggerClass;
using System.Diagnostics;
using log4net;
using System.Configuration;


namespace MBMEKiosk.Mawaqif.Presenters
{
    public class MawaqifPresenterBase : PresenterBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public MawaqifTransaction Transaction
        {
            get
            {
                return this.TransactionContext as MawaqifTransaction;
            }
        }
        
        public bool ShowGrid
        {
            get
            {
                if (this.Transaction.SelectedLanguageKey == "english")
                    return true;
                else
                    return false;
            }
        }

        public bool LogTransactionToLocalDb()
        {
            int intServiceId = 0;
            string ServiceName = null;

            if (log.IsInfoEnabled) log.Info("mawaqif LogTransactionToLocalDb started..");
    
            try
            {


                switch (this.Transaction.ServiceType)
                {
                    case MawaqifServiceType.None:
                        break;
                    case MawaqifServiceType.AccountTopUp:
                        intServiceId = Convert.ToInt32(ConfigurationManager.AppSettings["topupserviceid"]);
                        ServiceName = "Mawaqif TOP-UP POST PAYMENT";
                        break;
                    case MawaqifServiceType.PermitRenewal:
                        intServiceId = Convert.ToInt32(ConfigurationManager.AppSettings["rpserviceid"]);
                        ServiceName = "Mawaqif Permit Renewal";
                        break;
                    case MawaqifServiceType.ViolationPayment:
                        intServiceId = Convert.ToInt32(ConfigurationManager.AppSettings["vpayserviceid"]);
                        ServiceName = "Mawaqif PVT Post PAYMENT";
                        break;
                }


                Logger.LogTransaction(new TransactionsLog
                {
                    KioskId = Convert.ToInt64(this.Transaction.MachineId),
                    KioskTxnDateTime = this.Transaction.Date,
                    KioskTxnrefNum = this.Transaction.Id,
                    LastBalance = Convert.ToDouble(this.Transaction.BalanceDue),
                    NewBalance = Convert.ToDouble(this.Transaction.AmountDue),
                    TxnAmount = Convert.ToDouble(this.Transaction.AmountPaid),
                    Repost = true,
                    PaymentModeId = (int)(this.Transaction.CardPayment ? PaymentMode.CARD : PaymentMode.CASH),
                    BillerId = intServiceId,
                    ConsumerNumber = this.Transaction.AccountNumber,
                    TxnTypeId = (int) TransactionType.BILLPAYMENT,
                    ProductDetail = ServiceName
                });

                if (log.IsInfoEnabled) log.Info("mawaqif LogTransactionToLocalDb ended..for "+this.Transaction.Id);
                return true;
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Exception thrown in mawaqif LogTransactionToLocalDb" + ex.Message);
                return false;             
            }

        }


        public void LogPrintCycleLogToLocalDb()
        {

            PrintCycleLog obj = null;
            try
            {
                obj = new PrintCycleLog()
                {
                    TxnId = this.Transaction.Id,
                    ReceiptId = this.Transaction.Id,
                    TimePrt = DateTime.Now,
                    ReceiptTaken = 1,                    
                    KioskId = long.Parse(this.Transaction.MachineId)
                };
                if (log.IsErrorEnabled) log.Error("PrintCycle logging for TxnId:" + obj.TxnId.ToString() + "KioskId:" + obj.KioskId.ToString());
                DBLogger.AddTxnPrintCycle(obj);
                if (log.IsInfoEnabled) log.Info("Mawaqif - added entry in tbltxnPrintcycle using LogPrintCycleLogToLocalDb." + "TxnId:" + obj.TxnId.ToString());
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Mawaqif - Caught exception in  LogPrintCycleLogToLocalDb method. " + "TxnId:" + obj.TxnId.ToString() + "." + ex.Message);
            }

        }

        /// <summary>
        /// Service Number Key for Receipt Printing
        /// </summary>
        /// <param name="strKey">Name of the Resource Key</param>
        /// <returns>String in the Specified resource key</returns>
        public string RcptServiceNumberString(string strKey)
        {
            return this.ViewGrid.TryFindResource(strKey) as string; 
        }

        /// <summary>
        /// Footer Key for Receipt Printing
        /// </summary>
        /// <param name="strKey">Name of the Resource Key</param>
        /// <returns>String in the Specified resource key</returns>
        public string RcptFooterString(string strKey)
        {
            return this.ViewGrid.TryFindResource(strKey) as string;
        }
    }
}
