using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKioskLogger.LoggerClass
{

    public enum PaymentMode
    {
        NONE = 0,
        CASH = 1,
        CARD = 2

    }

    public enum TransactionStatus
    {
        InComplete = 1,
        Complete = 2
    }

    public enum TransactionType
    {
        BILLPAYMENT = 1,
        MONEYTRANSFER = 2,
        INTMOBILERECHARGE = 3,
        MOBILERECHARGE = 4,
        GETBALANCE = 5
    }

    public class TransactionsLog
    {
         
        public TransactionsLog()
        {
            TxnDtTm = DateTime.Now;
            KioskTxnDateTime = DateTime.Now;
            Status = (int)TransactionStatus.InComplete;
            Repost = false;
            PaymentModeId = (int)PaymentMode.CASH;
        }

        public Int64 TxnId { get; set; }
        public Int64 KioskId { get; set; }
        public int TxnTypeId { get; set; }
        public DateTime TxnDtTm { get; set; }
        //public double TxnAmount { get; set; }
        public Double TxnAmount { get; set; }
        public string KioskTxnrefNum { get; set; }
        public Int64 KioskTxnId { get; set; }
        public DateTime KioskTxnDateTime { get; set; }
        public Int32 BillerId { get; set; }
        public string BillerTxnrefNum { get; set; }
        public string ConsumerNumber { get; set; }
        public Int32 PaymentModeId { get; set; }
        public string ProductDetail { get; set; }
        public double LastBalance { get; set; }
        public double NewBalance { get; set; }
        public int Status { get; set; }
        public string ResponseCode { get; set; }
        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Field3 { get; set; }
        public int Field4 { get; set; }
        public int Field5 { get; set; }
        public int Field6 { get; set; }

        public string Field7 { get; set; }
        public string Field8 { get; set; }
        public string Field9 { get; set; }
        public string Field10 { get; set; }
        public string Field11 { get; set; }
        public string Field12 { get; set; }
        public string Field13 { get; set; }
        public string Field14 { get; set; }
        public string Field15 { get; set; }
        public string Field16 { get; set; }
        public string Field17 { get; set; }
        public string Field18 { get; set; }
        public string Field19 { get; set; }
        public string Field20 { get; set; }



        public string SystemRespMesg { get; set; }
        public string BillerRespMesg { get; set; }
        public bool Repost { get; set; }

        
    }
}