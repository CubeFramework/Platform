using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKioskLogger.LoggerClass;
using log4net;

namespace MBMEKioskLogger.Logger
{
    /// <summary>
    /// LOGSOURCE 
    /// ALL - Logs to All
    /// </summary>
    public enum LOGTO
    {
        ALL = 0,
        FILE = 1,
        DATABASE = 2,
        SERVICE = 3
    }

    public class Logger
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Logger()
        {

        }

        public static void LogTransaction(TransactionsLog objLog, LOGTO enumLog = LOGTO.DATABASE)
        {
            if (log.IsInfoEnabled) log.Info("Logger LogTransaction started..");
            int result = DBLogger.AddTransaction(objLog);
            if (log.IsInfoEnabled) log.Info("Logger LogTransaction ended..");
        }

        public static void LogCashCycle(CashCycleLog objLog, LOGTO enumLog = LOGTO.DATABASE)
        {
            int result = DBLogger.AddTxnCashCycle(objLog);
        }

        public static void LogPrintCycle(PrintCycleLog objLog, LOGTO enumLog = LOGTO.DATABASE)
        {
            int result = DBLogger.AddTxnPrintCycle(objLog);
        }

        public static void LogEvents(EventLogger objLog, LOGTO enumLog = LOGTO.DATABASE)
        {
            int result = DBLogger.AddEventLog(objLog);
        }

        public static List<TransactionsLog> GetTransactions()
        {
            return DBLogger.GetTransactions();
        }

        public static List<EventLogger> GetEventLog()
        {
            return DBLogger.GetEventLog();
        }

        public static List<CashCycleLog> GetCashCycle()
        {
            return DBLogger.GetCashCycle();
        }

        public static List<PrintCycleLog> GetPrintCycle()
        {
            return DBLogger.GetPrintCycle();
        }

        public static PackageFileLog GetPackageDetails()
        {
            return DBLogger.GetPackageDetails();
        }

        public static PackageFileLog GetPackageCommandStatus()
        {
            return DBLogger.GetPackageCommandStatus();
        }

        public static int LogPackageStatus(PackageMasterInfo pkgInfo)
        {
            if (log.IsInfoEnabled) log.Info("Logger LogPackageStatus started..");
            int result = DBLogger.LogPackageStatus(pkgInfo);
            if (log.IsInfoEnabled) log.Info("Logger LogPackageStatus ended..");
            return result;
        }

        public static SnapShotLogger getSnapShortDetail(string billerserviceTypeId)
        {
            return DBLogger.GetSnapShortDetails(billerserviceTypeId);
        }

        public static int AddUpdatetblSnapshort(SnapShotLogger objSnapshort, string type)
        {
            return DBLogger.AddUpdatetblSnapshort(objSnapshort, type);
        }

        public static Dictionary<string, string> CleanUpKioskLocalDB()
        {
            return DBLogger.CleanUpKioskLocalDB();
        }
 
        public static string GetRecentScrollers()
        {
            return DBLogger.GetRecentScrollers();
        }


        public static void LogCommand(string p, string p_2)
        {
            DBLogger.LogCommand(p, p_2);
        }
    }
}
