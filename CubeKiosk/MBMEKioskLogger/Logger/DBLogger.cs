using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKioskLogger.LoggerClass;
using System.Data.SqlServerCe;
using System.Diagnostics;
using log4net;

namespace MBMEKioskLogger.Logger
{

    public class DBLogger
    {
        public const string DateTimeFormat = "MM/d/yyyy HH:mm:ss";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static SqlCeConnection _sqlCeConnection = null;
        private static object monitor = new object();
        private static string strConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["LocalDbConnectionString"].ToString();
       
        private static Int32 BatchSize=  System.Configuration.ConfigurationManager.AppSettings["BatchSize"]!=null ? Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["BatchSize"].ToString()):10;

        static void Open(string connectionString)
        {
            lock (monitor)
            {
                if (_sqlCeConnection == null)
                {
                    if (String.IsNullOrEmpty(connectionString))
                    {
                        if (log.IsErrorEnabled) log.Error("The Local database connection string not found \n Please add the connectionstring key named (LocalDbConnectionString) in app.config file");
                        throw new Exception("The Local database connection string not found \n Please add the connectionstring key named (LocalDbConnectionString) in app.config file");
                    }
                    _sqlCeConnection = new SqlCeConnection(connectionString);
                    try
                    {
                        _sqlCeConnection.Open();
                    }
                    catch (SqlCeException ex)
                    {
                        if (log.IsErrorEnabled) log.Error("SqlCeException caught in Dblogger.Open.." + ex.Message);
                        System.Environment.Exit(1);  //  exit  
                    }
                    catch (Exception ex)
                    {
                        if (log.IsErrorEnabled) log.Error("Generic exception caught in Dblogger.Open.." + ex.Message);
                        System.Environment.Exit(1);  //  exit  
                    }
                }
            }
        }

        static void Close()
        {
            lock (monitor)
            {
                if (_sqlCeConnection != null)
                {
                    _sqlCeConnection.Dispose();
                    _sqlCeConnection = null;
                }
            }
        }

        static int ExecuteNonQuery(SqlCeCommand sqlCommand)
        {
            lock (monitor)
            {
                sqlCommand.Connection = _sqlCeConnection;
                return sqlCommand.ExecuteNonQuery();
            }
        }

        static SqlCeDataReader ExecuteReader(SqlCeCommand sqlCommand)
        {
            lock (monitor)
            {
                sqlCommand.Connection = _sqlCeConnection;
                return sqlCommand.ExecuteReader();
            }
        }

        public static string FormatDateTime(DateTime dtDate)
        {
            return dtDate.ToString(DateTimeFormat);
        }

        #region public properties

        #region LoggingTypeType

        public static int AddUpdatetblSnapshort(SnapShotLogger objSnapshort, string type)
        {
            int result = 0;
            StringBuilder strSql = new StringBuilder();
            if (type == "Add")
            {
                strSql.AppendFormat("INSERT INTO tblSnapShort (BillerServiceId, ChangedDate, SnapSortId,Type, Path ) VALUES('{0}','{1}', '{2}','{3}','{4}')",
               objSnapshort.BillerServiceId, objSnapshort.ChangedDate, objSnapshort.SnapSortId, objSnapshort.Type, objSnapshort.Path);               


            }
            else
            {
                strSql.AppendFormat("Update tblSnapShort SET ChangedDate = '{0}', SnapSortId = '{1}', Type= '{2}'", objSnapshort.ChangedDate, objSnapshort.SnapSortId, objSnapshort.Type);
            }
            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql.ToString());

                result = ExecuteNonQuery(command);
                if (result > 0)
                    if (log.IsInfoEnabled) log.Info(" AddUpdatetblSnapshort successfull.. for tblSnapShort.." + objSnapshort.BillerServiceId);
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.AddUpdatetblSnapshort.." + ex.Message);
                Close();


            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic Exception caught in  DBLogger.AddUpdatetblSnapshort.." + ex.Message);

            }
            finally
            {
                Close();
            }

            return result;
        }

        /// <summary>
        /// Adds the Transaction
        /// </summary>
        /// <param name="objTrans"></param>
        /// <returns>returns 1 on insert</returns>
        public static int AddTransaction(TransactionsLog objTrans)
        {
            ////Set the Connection String
            //SqlCeConnection Conn = ConnectDb();

            int result = 0;
            StringBuilder strSql = new StringBuilder();


            strSql.Append("INSERT INTO [tblTransactions]([KIOSKID],[TxnDttm],[TxnAmount],[KIOSKTxnDttm],[BillerID],[ConsumerNumber],[ProductDetail] ,[LastBalance] ,[NewBalance] ,[TxnTypeId] ,[KioskTxnId] ,[KioskTxnRefNum] ,[PaymentModeId] ,[Status] ,[ResponseCode] ,[Field1] ,[Field2] ,[Field3] ,[Field4] ,[Field5] ,[Field6] ,[BillerTxnRefNum] ,[SystemRespCode] ,[BillerRespCode] ,[Repost],[Field7],[Field8],[Field9],[Field10],[Field11],[Field12],[Field13],[Field14],[Field15],[Field16],[Field17],[Field18],[Field19],[Field20]) Values (");

            strSql.AppendFormat("'{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}','{20}','{21}','{22}','{23}','{24}','{25}','{26}','{27}','{28}','{29}','{30}','{31}','{32}','{33}','{34}','{35}','{36}','{37}','{38}')", objTrans.KioskId, FormatDateTime(objTrans.TxnDtTm), objTrans.TxnAmount, FormatDateTime(objTrans.KioskTxnDateTime), objTrans.BillerId, objTrans.ConsumerNumber, objTrans.ProductDetail, objTrans.LastBalance, objTrans.NewBalance, objTrans.TxnTypeId, objTrans.KioskTxnId, objTrans.KioskTxnrefNum, objTrans.PaymentModeId, objTrans.Status, objTrans.ResponseCode, objTrans.Field1, objTrans.Field2, objTrans.Field3, objTrans.Field4, objTrans.Field5, objTrans.Field6, objTrans.BillerTxnrefNum, objTrans.ResponseCode, objTrans.ResponseCode, objTrans.Repost, objTrans.Field7, objTrans.Field8, objTrans.Field9, objTrans.Field10, objTrans.Field11, objTrans.Field12, objTrans.Field13, objTrans.Field14, objTrans.Field15, objTrans.Field16, objTrans.Field17, objTrans.Field18, objTrans.Field19, objTrans.Field20);
            if (log.IsInfoEnabled) log.Info(" sql query is:" + strSql);
            
            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                //Trace.WriteLine(strSql.ToString());
                //Trace.WriteLine("Transaction Inserted");
                result = ExecuteNonQuery(command);                
                if (result > 0)
                    if (log.IsInfoEnabled) log.Info(" AddTransaction successfull.. for KioskTxnRefNum.." + objTrans.KioskTxnrefNum);
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.AddTransaction.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic Exception caught in  DBLogger.AddTransaction.." + ex.Message);

            }

            return result;


        }

        #region ContentManagement


        public static int AddPackageMaster(int intPkgId,
     string strRV, DateTime plannedDate, bool booldeployed = false)
        {
            int result = 0;
            StringBuilder strSql = new StringBuilder();

            strSql.AppendFormat("INSERT INTO [tblPackageMaster](PackageId,ReleaseVersion,Deployed,PlannedActivationDttm,Downloaded) Values ('{0}','{1}','{2}','{3}','{4}')",
                intPkgId.ToString(), strRV, booldeployed, FormatDateTime(plannedDate).ToString(), false);

            if (log.IsInfoEnabled) log.Info(" sql query is:" + strSql);

            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                //Trace.WriteLine(strSql.ToString());
                result = ExecuteNonQuery(command);

                if (result > 0)
                    if (log.IsInfoEnabled) log.Info(" AddPackage successfull.. for PackageId.." + intPkgId.ToString());
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.AddPackage.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }

            return result;
        }

        public static int UpdatePackageMaster(int KioskPackageId, int intPkgId,
             byte status, bool booldownload = false)
        {
            int result = 0;
            StringBuilder strSql = new StringBuilder();

            strSql.AppendFormat("Update [tblPackageMaster] Set Downloaded = '{0}', PackageStatus = '{1}' Where PackageId = {2} and KioskPackageId = {3}",
                booldownload, status, intPkgId.ToString(), KioskPackageId);

            if (log.IsInfoEnabled) log.Info(" sql query is:" + strSql);

            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                //Trace.WriteLine(strSql.ToString());
                result = ExecuteNonQuery(command);

                if (result > 0)
                    if (log.IsInfoEnabled) log.Info(" AddPackage successfull.. for PackageId.." + intPkgId.ToString());
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.AddPackage.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }

            return result;
        }

        public static int AddPackageDetails(int intPkgId, int intPkgDetId, int intDlDetailId, string strFileName, string strFilePath, string strFileType,
            string strFileSize, bool complete = false, string dtStart = null)
        {
            int result = 0;
            StringBuilder strSql = new StringBuilder();

            if (string.IsNullOrEmpty(dtStart))
                dtStart = FormatDateTime(DateTime.Now).ToString();

            strSql.AppendFormat("INSERT INTO [tblPackageDetails](PackageId, PkgDetId, DownloadDetailId, FileName, FilePath, FileType, FileSize, StartDateTime, Complete) Values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')",
                intPkgId.ToString(), intPkgDetId, intDlDetailId, strFileName, strFilePath, strFileType, strFileSize, dtStart, complete);

            if (log.IsInfoEnabled) log.Info(" sql query is:" + strSql);

            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                //Trace.WriteLine(strSql.ToString());
                if (log.IsInfoEnabled) log.Info("Adding Package File " + strSql);
                // Commented By JK and AJ as this is not unique.
                //if (GetPackageDetails(intDlDetailId) == 0)
                result = ExecuteNonQuery(command);

                if (result > 0)
                    if (log.IsInfoEnabled) log.Info(string.Format("File Details {0} successfull.. for PackageId.. {1}", intPkgId.ToString(), strFileName));
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.AddFileDetails.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic Exception caught in  DBLogger.AddPackage.." + ex.Message);

            }

            return result;
        }

        public static int UpdatePackageDetails(int intDlDetailId, int PackageDetailId, bool complete = false, string dtEnd = null)
        {
            int result = 0;
            StringBuilder strSql = new StringBuilder();

            if (string.IsNullOrEmpty(dtEnd))
                dtEnd = FormatDateTime(DateTime.Now).ToString();

            strSql.AppendFormat("Update [tblPackageDetails] Set Enddatetime = '{0}', Complete = '{1}' Where DownloadDetailId = '{2}' and PackageDetailId = '{3}'",
                dtEnd.ToString(), complete, intDlDetailId, PackageDetailId);

            if (log.IsInfoEnabled) log.Info(" sql query is:" + strSql);

            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                if (log.IsInfoEnabled) log.Info("Update Package Details " + strSql);
                result = ExecuteNonQuery(command);

                if (result > 0)
                    if (log.IsInfoEnabled) log.Info(string.Format("Downlaod File Details {0} successfull.. for Download Detail Id.. ", intDlDetailId.ToString()));
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.AddFileDetails.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic Exception caught in  DBLogger.AddPackage.." + ex.Message);

            }

            return result;
        }

        public static int GetPackageDetails(int intDetailId)
        {
            int result = 0;
            SqlCeDataReader dr;
            StringBuilder strSql = new StringBuilder();

            strSql.AppendFormat("Select * from tblPackageDetails Where DownloadDetailId = {0}", intDetailId);

            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                dr = ExecuteReader(command);

                while (dr.Read())
                {
                    result = Convert.ToInt32(dr[0]);
                }

                if (result > 0)
                    if (log.IsInfoEnabled) log.Info(" Get Package Details successfull.. for PackageId.." + intDetailId.ToString());
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.Get Package Details.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }
            return result;
        }

        public static int StartFileDownload(PackageFileLog file)
        {
            int result = 0;
            StringBuilder strSql = new StringBuilder();

            //if (string.IsNullOrEmpty(file.StartDateTime.tos))
            //    file.StartDateTime = DateTime.Now;
            // Changed By JK and AJ. Added PackageDetailId to make sure unique record gets updated.
            strSql.AppendFormat("Update [tblPackageDetails] Set StartDateTime = '{0}', Complete = '{1}' Where DownloadDetailId = '{2}' and PackageDetailId = '{3}'",
                FormatDateTime(DateTime.Now), false, file.DownloadDetailId, file.PackageDetailId);

            if (log.IsInfoEnabled) log.Info(" sql query is:" + strSql);

            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                if (log.IsInfoEnabled) log.Info("Start File Download " + strSql);
                result = ExecuteNonQuery(command);

                if (result > 0)
                    if (log.IsInfoEnabled) log.Info(string.Format("Downlaod File Details {0} successfull.. for Download Detail Id.. ", file.DownloadDetailId));
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.AddFileDetails.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic Exception caught in  DBLogger.AddPackage.." + ex.Message);

            }

            return result;
        }

        public static List<PackageFileLog> GetNotDownloadedContent()
        {

            int result = 0;
            SqlCeDataReader dr;
            List<PackageFileLog> packglist = new List<PackageFileLog>();
            StringBuilder strSql = new StringBuilder();

            //strSql.Append("Select * from tblPackageDetails Where Complete = 'False'");
            strSql.Append("SELECT tblPackageMaster.KioskPackageId, tblPackageMaster.PackageId, tblPackageMaster.ReleaseVersion, tblPackageMaster.Deployed, tblPackageMaster.PlannedActivationDttm, tblPackageMaster.ActualActivationDttm, tblPackageMaster.Downloaded, tblPackageMaster.CommandIssued,  tblPackageDetails.PackageDetailId, tblPackageDetails.FileName, tblPackageDetails.FilePath, tblPackageDetails.FileType, tblPackageDetails.FileSize, tblPackageDetails.Complete,  tblPackageDetails.StartDateTime, tblPackageDetails.EndDateTime, tblPackageDetails.DownloadDetailId, tblPackageDetails.PkgDetId FROM tblPackageDetails INNER JOIN tblPackageMaster ON tblPackageDetails.PackageId = tblPackageMaster.PackageId Where Downloaded = 'False' order by KioskPackageId");

            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                dr = ExecuteReader(command);

                while (dr.Read())
                {
                    PackageFileLog pkg = new PackageFileLog();
                    pkg.KioskPackageId = Convert.ToInt32(dr["KioskPackageId"]);
                    pkg.PackageId = Convert.ToInt32(dr["PackageId"]);
                    pkg.FileName = Convert.ToString(dr["FileName"]);
                    pkg.FilePath = Convert.ToString(dr["FilePath"]);
                    pkg.FileType = Convert.ToString(dr["FileType"]);
                    pkg.FileSize = Convert.ToString(dr["FileSize"]);
                    pkg.Complete = Convert.ToBoolean(dr["Complete"]);
                    pkg.StartDateTime = Convert.ToDateTime(dr["StartDateTime"]);
                    //pkg.EndDateTime = Convert.ToDateTime(dr["EndDateTime"]);
                    pkg.DownloadDetailId = Convert.ToInt32(dr["DownloadDetailId"]);
                    pkg.PkgDetId = Convert.ToInt32(dr["PkgDetId"]);
                    pkg.ReleaseVersion = Convert.ToString(dr["ReleaseVersion"]);
                    pkg.PackageDetailId = Convert.ToInt32(dr["PackageDetailId"]);
                    packglist.Add(pkg);
                }
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.Get Package Details.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }
            return packglist;
        }

        public static PackageMasterInfo GetPackageInfo(int intPkgId = 0)
        {
            int result = 0;
            SqlCeDataReader dr;
            PackageMasterInfo pkg = null;
            List<PackageFileLog> packglist = new List<PackageFileLog>();
            StringBuilder strSql = new StringBuilder();

            //strSql.Append("Select * from tblPackageDetails Where Complete = 'False'");
            // Added By JK and AJ 18/07/2012
            // Added Order By clause to make sure the earliest package is picked.
            strSql.AppendFormat("SELECT * from tblPackageMaster Where Downloaded = 'False' order by KioskPackageId");

            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                dr = ExecuteReader(command);

                while (dr.Read())
                {
                    pkg = new PackageMasterInfo();
                    pkg.KioskPackageId = Convert.ToInt32(dr["KioskPackageId"]);
                    pkg.PackageId = Convert.ToInt32(dr["PackageId"]);
                    pkg.ReleaseVersion = Convert.ToString(dr["ReleaseVersion"]);
                    pkg.Deployed = Convert.ToBoolean(dr["Deployed"]);
                    break;
                }
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.Get Package Details.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }
            return pkg;
        }

        #endregion

        public static int AddQueueStatus(int intQueueId, bool Complete = false)
        {
            int result = 0;
            StringBuilder strSql = new StringBuilder();

            //strSql.AppendFormat("Update [tblPackageMaster](PackageId,ReleaseVersion,Deployed) Values ('{0}','{1}','{2}')",
            //    intPkgId.ToString(), strRV, booldeployed);

            strSql.AppendFormat("INSERT INTO tblKioskCommandQueue (QueueId, Complete) VALUES('{0}','{1}')",
                intQueueId, Complete);

            if (log.IsInfoEnabled) log.Info(" sql query is:" + strSql);

            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                //Trace.WriteLine(strSql.ToString());
                result = ExecuteNonQuery(command);

                if (result > 0)
                    if (log.IsInfoEnabled) log.Info(" AddQueueStatus successfull.. for QueueId.." + intQueueId.ToString());
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.AddQueueStatus.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }

            return result;
        }

        public static int AddEventLog(EventLogger objTrans)
        {
            //Set the Connection String


            int result = 0;
            StringBuilder strSql = new StringBuilder();

            strSql.Append("INSERT INTO tblEventLog");
            strSql.Append("(KIOSKID, EventDttm, EventID, Description, Uploaded) VALUES (");
            strSql.AppendFormat("'{0}','{1}',{2},'{3}','{4}')", objTrans.KioskId, FormatDateTime(objTrans.EventDtTm), objTrans.EventId, objTrans.Desc, objTrans.Uploaded);

            if (log.IsInfoEnabled) log.Info("QUERY FORMED IN AddEventLog IS : " + strSql.ToString());

            //Trace.WriteLine(strSql.ToString());
            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                result = ExecuteNonQuery(command);
                if (log.IsErrorEnabled) log.Error("KioskId:"+objTrans.KioskId.ToString() +" and EventId:"+ objTrans.EventId.ToString() +" is added in tblEventLog and result is:" + result.ToString());
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.AddEventLog.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic exception caught in DBLogger.AddEventLog.." + ex.Message);
            }

            return result;
        }
        public static int AddTxnCashCycle(CashCycleLog obj)
        {            
            int result = 0;
            StringBuilder strSql = new StringBuilder();
            //string strTxnIdDataType=string.Empty; 
            //string strKioskIDColumn=string.Empty;           
            //StringBuilder strSelectSql = new StringBuilder();
            //StringBuilder strAlterSql = new StringBuilder(); 
            //StringBuilder strFindKioskIDSql = new StringBuilder();
            //StringBuilder strAddKioskIDSql = new StringBuilder();            
            //SqlCeDataReader dr = null;
            //SqlCeDataReader drKioskID = null;

            

                //logic to add KIOSKID column in tblTxnCashCycle            
            //    strFindKioskIDSql.Append("SELECT COLUMN_NAME FROM  information_schema.columns WHERE table_name = 'tblTxnCashCycle' AND column_name = 'KIOSKID'");
            //    strAddKioskIDSql.Append("ALTER table tblTxnCashCycle add KIOSKID bigint");
            //    try
            //    {
            //        Open(strConnectionString);

            //        SqlCeCommand commandKiosk = new SqlCeCommand(strFindKioskIDSql.ToString());
            //        drKioskID = ExecuteReader(commandKiosk);

            //        while (drKioskID.Read())
            //        {
            //            strKioskIDColumn = "KIOSKID";
            //            break;
            //        }

            //        if (drKioskID != null || drKioskID.IsClosed == false)
            //            drKioskID.Close();

            //        if (strKioskIDColumn != "KIOSKID")
            //        {
            //            SqlCeCommand commandAdd = new SqlCeCommand(strAddKioskIDSql.ToString());
            //            result = ExecuteNonQuery(commandAdd);
            //        }

            //    }
            //    catch (SqlCeException ex)
            //    {
            //        if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.AddTxnCashCycle while adding KIOSKID column .." + ex.Message);
            //        Close();
            //        System.Environment.Exit(1);  //  exit  

            //    }
            //    catch (Exception ex)
            //    {
            //        if (log.IsErrorEnabled) log.Error("Generic exception caught in DBLogger.AddTxnCashCycle while adding KIOSKID column.." + ex.Message);
            //    }

            //    if (log.IsInfoEnabled) log.Info("Altering tblTxnCashCycle to add KIOSKID column is done.");

            //    // end of logic to add KIOSKID column in tblTxnCashCycle


            ////logic to find  datatype of Txnid
            //strSelectSql.Append("SELECT data_type FROM  information_schema.columns WHERE table_name = 'tblTxnCashCycle' AND column_name = 'TxnID'");            
            //try 
            //{
            //    Open(strConnectionString);
            //    SqlCeCommand command = new SqlCeCommand(strSelectSql.ToString());
            //    dr = ExecuteReader(command);

            //    while (dr.Read())
            //    {
                   
            //       strTxnIdDataType = Convert.ToString(dr["data_type"]);                  
            //        break;
            //    }
               
            //}
            //catch (SqlCeException ex)
            //{
            //    if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.AddTxnCashCycle while reading TxnID datatype table.." + ex.Message);
            //    Close();
            //    System.Environment.Exit(1);  //  exit  

            //}
            //catch (Exception ex)
            //{
            //    if (log.IsErrorEnabled) log.Error("Generic exception caught in DBLogger.AddTxnCashCycle while TxnID datatype table.." + ex.Message);
            //}
            //finally
            //{
            //    if(dr!=null || dr.IsClosed==false)
            //        dr.Close();
            //}    //  end of logic to  find txnid column datatype in  tbltxncashcycle           


            //// logic to alter table to change datatype of Txnid in tblTxnCashCycle
            //if(strTxnIdDataType== "bigint")
            //{            
            //    strAlterSql.Append("ALTER table tblTxnCashCycle alter column TXNID nvarchar(50)");
            //    try
            //    {
            //        Open(strConnectionString);
            //        SqlCeCommand commandAlter = new SqlCeCommand(strAlterSql.ToString());
            //        result = ExecuteNonQuery(commandAlter);
               
            //    }
            //    catch (SqlCeException ex)
            //    {
            //        if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.AddTxnCashCycle while altering table.." + ex.Message);
            //        Close();
            //        System.Environment.Exit(1);  //  exit  

            //    }
            //    catch (Exception ex)
            //    {
            //        if (log.IsErrorEnabled) log.Error("Generic exception caught in DBLogger.AddTxnCashCycle while altering table.." + ex.Message);
            //    }

            //    if (log.IsInfoEnabled) log.Info("Altering tblTxnCashCycle TxnId column datatype is done.");

            //}//  end of logic to alter table to change datatype of Txnid in tblTxnCashCycle


            strSql.Append("INSERT INTO tblTxnCashCycle");
            strSql.Append("(TXNID, TIMEINS, NOTEVAL, NOTEID, ACCEPTED, UPLOADED,KIOSKID) VALUES (");
            strSql.AppendFormat("'{0}','{1}','{2}','{3}','{4}','{5}','{6}')", obj.TxnId, FormatDateTime(obj.TimeIns), obj.NoteVal, obj.NoteId, obj.Accepted, obj.Uploaded, obj.KioskId);
            //Trace.WriteLine(strSql.ToString());
            if (log.IsInfoEnabled) log.Info(strSql.ToString());
            try
            {
                Open(strConnectionString);
                SqlCeCommand commandInsert = new SqlCeCommand(strSql.ToString());
                result = ExecuteNonQuery(commandInsert);
                if (log.IsErrorEnabled) log.Error(obj.TxnId.ToString()+ " is added in tblTxnCashCycle.And Execution result is:"+result.ToString());
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.AddTxnCashCycle.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic exception caught in DBLogger.AddTxnCashCycle.." + ex.Message);
            }

            return result;
        }
        public static int AddTxnPrintCycle(PrintCycleLog obj)
        {

            int result = 0;           
            StringBuilder strSql = new StringBuilder();
            StringBuilder strSelect = new StringBuilder();
            SqlCeDataReader dr;

            strSelect.Append("select TXNID from tblTxnPrintCycle where TXNID='" + obj.TxnId + "'");            

            strSql.Append("INSERT INTO tblTxnPrintCycle");
            strSql.Append("(TXNID, TIMEPRT, RECID, RECTAKEN,KioskID) VALUES (");
            strSql.AppendFormat("'{0}','{1}','{2}',{3},{4})", obj.TxnId,
               FormatDateTime(obj.TimePrt), obj.ReceiptId, obj.ReceiptTaken,obj.KioskId);
            //Trace.WriteLine(strSql.ToString());
            if (log.IsInfoEnabled) log.Info(strSql.ToString());
            try
            {
                Open(strConnectionString);

                string readerResult = string.Empty;
                SqlCeCommand commandDr = new SqlCeCommand(strSelect.ToString());
                dr = ExecuteReader(commandDr);

                while (dr.Read())
                {
                    readerResult = Convert.ToString(dr["TxnId"]);
                }
                dr.Close();

                if (string.IsNullOrEmpty(readerResult))
                {
                    SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                    result = ExecuteNonQuery(command);
                }

                if (log.IsErrorEnabled) log.Error("TxnID:"+obj.TxnId.ToString()+"KioskID:"+obj.KioskId  + " is added in tblTxnPrintCycle.And Execution result is:" + result.ToString());
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.AddTxnPrintCycle.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic exception caught in DBLogger.AddTxnPrintCycle.." + ex.Message);
            }

            return result;
        }


        public static int LogPackageStatus(PackageMasterInfo obj)
        {

            int result = 0;
            StringBuilder strSql = new StringBuilder();

            strSql.AppendFormat("Update tblPackageMaster Set Deployed='true',ExecutionDatetime='" + FormatDateTime(obj.ExecutionDateTime) + "' , IssuedDatetime='" + FormatDateTime(DateTime.Now) + "' ,QueueId='" + obj.QueueId.ToString() + "' Where PackageId in ({0})", obj.PackageId.ToString());
            if (log.IsInfoEnabled) log.Info("strSql:" + strSql);
            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                result = ExecuteNonQuery(command);
                if (log.IsInfoEnabled) log.Info("Inside DBLogger.LogPackageStatus .ExecuteNonQuery result is:" + result.ToString());
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.LogPackageStatus.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic exception caught in DBLogger.LogPackageStatus.." + ex.Message);
            }

            return result;
        }

        internal static int LogCommand(string cmd, string param)
        {
            int result = 0;
            StringBuilder strSql = new StringBuilder();

            //strSql.AppendFormat("Update [tblPackageMaster](PackageId,ReleaseVersion,Deployed) Values ('{0}','{1}','{2}')",
            //    intPkgId.ToString(), strRV, booldeployed);

            strSql.AppendFormat("INSERT INTO tblKioskCommand (Command, CommandParameter) VALUES('{0}','{1}')",
                cmd, param.Replace("'","''"));

            if (log.IsInfoEnabled) log.Info(" sql query is:" + strSql);

            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                if (log.IsInfoEnabled) log.Info(strSql.ToString());
                result = ExecuteNonQuery(command);

                if (result > 0)
                    if (log.IsInfoEnabled) log.Info(" AddCommand successfull.. for Command.." + cmd);
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.LogCommand" + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }

            return result;
        }
        
        #endregion



        #region Select ALL

        #region GetSnapShort Details
        public static SnapShotLogger GetSnapShortDetails(string billerServiceId)
        {
            string strSql = "Select  * from tblSnapShort Where BillerServiceId = " + billerServiceId;

            return getSnapShortDetails(strSql);
        }
        #endregion

        /// <summary>
        /// Gets the Latest Scroller Logged in
        /// </summary>
        /// <returns>Scrollers Text</returns>
        internal static string GetRecentScrollers()
        {
            if (log.IsInfoEnabled) log.Info("GetScrollerText started..");
            string result=null;

            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand("SELECT QueueId, Command, CommandParameter FROM tblKioskCommand AS tblKioskCommand_1 WHERE (QueueId IN (SELECT MAX(QueueId) AS QueueId FROM tblKioskCommand AS tblKioskCommand WHERE (Command = 'MESSAGE-REPOSITORY-DOWNLOAD'))) order by QueueId desc");
                SqlCeDataReader reader = ExecuteReader(command);

                while (reader.Read())
                {
                    result = Convert.ToString(reader["CommandParameter"]);
                    if (log.IsErrorEnabled) log.Error("TxnId:" + Convert.ToString(reader["TXNID"]) + ",KioskId:" + Convert.ToInt64(reader["KioskID"]).ToString() + "is read.");
                }
                
                reader.Close();
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.GetScrollerText.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic exception caught in GetScrollerText.." + ex.Message);
            }

            if (log.IsInfoEnabled) log.Info("GetScrollerText ended..");
            return result;
        }  

        /// <summary>
        /// gets all transactions
        /// </summary>
        /// <returns>List Collection of transaction table</returns>
        public static List<TransactionsLog> GetTransactions()
        {
            //string strSql = "SELECT ISNULL(TXNID, 0) AS TXNID, ISNULL(KIOSKID, 0) AS KIOSKID, ISNULL(TXNTYPEID, 0) AS TXNTYPEID, ISNULL(TXNDTTM, '') AS TXNDTTM, ISNULL(TXNAMOUNT, 0)  AS TXNAMOUNT, ISNULL(KIOSKTXNREFNUM, '') AS KIOSKTXNREFNUM, ISNULL(KIOSKTXNID, 0) AS KIOSKTXNID, ISNULL(KIOSKTXNDTTM, '') AS KIOSKTXNDTTM,  ISNULL(BILLERID, 0) AS BILLERID, ISNULL(BILLERTXNREFNUM, '') AS BILLERTXNREFNUM, ISNULL(CONSUMERNUMBER, '') AS CONSUMERNUMBER,  ISNULL(PAYMENTMODEID, 0) AS PAYMENTMODEID, ISNULL(PRODUCTDETAIL, '') AS PRODUCTDETAIL, ISNULL(LASTBALANCE, 0) AS LASTBALANCE,  ISNULL(NEWBALANCE, 0) AS NEWBALANCE, ISNULL(UPLOADED, 0) AS UPLOADED FROM TBLTRANSACTIONS";
            string strSql = "Select  * from tblTransactions Where Uploaded = 'false' order by TxnId ";

            return SelectTransaction(strSql);
        }

         
        /// <summary>
        /// gets all transactions
        /// </summary>
        /// <returns>List Collection of transaction table</returns>
        public static PackageFileLog GetPackageDetails()
        {
            //string strSql = "SELECT ISNULL(TXNID, 0) AS TXNID, ISNULL(KIOSKID, 0) AS KIOSKID, ISNULL(TXNTYPEID, 0) AS TXNTYPEID, ISNULL(TXNDTTM, '') AS TXNDTTM, ISNULL(TXNAMOUNT, 0)  AS TXNAMOUNT, ISNULL(KIOSKTXNREFNUM, '') AS KIOSKTXNREFNUM, ISNULL(KIOSKTXNID, 0) AS KIOSKTXNID, ISNULL(KIOSKTXNDTTM, '') AS KIOSKTXNDTTM,  ISNULL(BILLERID, 0) AS BILLERID, ISNULL(BILLERTXNREFNUM, '') AS BILLERTXNREFNUM, ISNULL(CONSUMERNUMBER, '') AS CONSUMERNUMBER,  ISNULL(PAYMENTMODEID, 0) AS PAYMENTMODEID, ISNULL(PRODUCTDETAIL, '') AS PRODUCTDETAIL, ISNULL(LASTBALANCE, 0) AS LASTBALANCE,  ISNULL(NEWBALANCE, 0) AS NEWBALANCE, ISNULL(UPLOADED, 0) AS UPLOADED FROM TBLTRANSACTIONS";
            //string strSql = "Select  * from tblPackageMaster where PlannedActivationDttm <=getdate() and Deployed='false' and PackageStatus='2' and CommandIssued='false' order  by PlannedActivationDttm";
            //string strSql = "Select  * from tblPackageMaster where Deployed='false' and PackageStatus='2' and CommandIssued='false' order  by PlannedActivationDttm";
            string strSql = string.Format("Select  * from tblPackageMaster where PlannedActivationDttm <= GETDATE() and Deployed='false' and PackageStatus='2' and CommandIssued='false' order  by PlannedActivationDttm", FormatDateTime(DateTime.Now));

            if (log.IsInfoEnabled) log.Info(strSql);
            return SelectPackage(strSql);
        }


        /// <summary>
        /// gets all transactions
        /// </summary>
        /// <returns>List Collection of transaction table</returns>
        public static PackageFileLog GetPackageCommandStatus()
        {
            //string strSql = "SELECT ISNULL(TXNID, 0) AS TXNID, ISNULL(KIOSKID, 0) AS KIOSKID, ISNULL(TXNTYPEID, 0) AS TXNTYPEID, ISNULL(TXNDTTM, '') AS TXNDTTM, ISNULL(TXNAMOUNT, 0)  AS TXNAMOUNT, ISNULL(KIOSKTXNREFNUM, '') AS KIOSKTXNREFNUM, ISNULL(KIOSKTXNID, 0) AS KIOSKTXNID, ISNULL(KIOSKTXNDTTM, '') AS KIOSKTXNDTTM,  ISNULL(BILLERID, 0) AS BILLERID, ISNULL(BILLERTXNREFNUM, '') AS BILLERTXNREFNUM, ISNULL(CONSUMERNUMBER, '') AS CONSUMERNUMBER,  ISNULL(PAYMENTMODEID, 0) AS PAYMENTMODEID, ISNULL(PRODUCTDETAIL, '') AS PRODUCTDETAIL, ISNULL(LASTBALANCE, 0) AS LASTBALANCE,  ISNULL(NEWBALANCE, 0) AS NEWBALANCE, ISNULL(UPLOADED, 0) AS UPLOADED FROM TBLTRANSACTIONS";
            string strSql = "Select  * from tblPackageMaster where Deployed ='true' and CommandExecuted = 'false'";

            return SelectPackageCommandInfo(strSql);
        }


        /// <summary>
        /// queries not uploaded events to the server
        /// </summary>
        /// <returns>collection of records</returns>
        public static List<EventLogger> GetEventLog()
        {
            //query not uploaded events
            string strSql = "Select  *  from tblEventLog Where Uploaded = 'false' order by EventLogId ";
            return SelectEventLog(strSql);
        }

        /// <summary>
        /// queries not uploaded cash cycle to the server
        /// </summary>s
        /// <returns>Collection of records</returns>
        public static List<CashCycleLog> GetCashCycle()
        {
            string strSql = "Select  *  from tbltxnCashCycle Where Uploaded = 'false' order by CashCycleId";
            return SelectTxnCashCycle(strSql);
        }

        /// <summary>
        /// queries not uploaded print cycle to the server
        /// </summary>
        /// <returns></returns>
        public static List<PrintCycleLog> GetPrintCycle()
        {
            return SelectTxnPrintCycle("Select  *  from tblTxnPrintCycle Where Uploaded = 'false' order by PrnCycleId");
        }

        #endregion

        #region update uploaded methods

        public static void UpdateEventLog(Dictionary<string, string> result)
        {
            StringBuilder strSql = new StringBuilder();
            string val = string.Empty;
            try
            {

                foreach (var item in result)
                {
                    if (item.Value == "Successfully Posted" || item.Value == "Already Posted")
                    {
                        if (string.IsNullOrEmpty(val))
                            val += "'" + item.Key.ToString() + "'";
                        else
                            val += string.Format(",{0}", "'" + item.Key.ToString() + "'");
                    }
                }

                if (!String.IsNullOrEmpty(val))
                {
                    strSql.AppendFormat("Update tblEventLog Set Uploaded = '1' Where EventLogId in ({0})", val.ToString());

                    Open(strConnectionString);
                    SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                    ExecuteNonQuery(command);
                    // Added By Jags for troubleshooting on 11/05/12
                    System.Console.WriteLine(" Update in local database successful.");
                    // Added By Jags for troubleshooting on 11/05/12
                    if (log.IsInfoEnabled) log.Info(val.ToString() + "added in db from UpdateEventLog()");
                }

            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.UpdateEventLog.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                if (log.IsErrorEnabled) log.Error("Generic exception caught in UpdateEventLog()  " + ex.Message);
            }
            if (log.IsInfoEnabled) log.Info("UpdateEventLog ended..");

        }
        public static void UpdateTransactionLog(Dictionary<string, string> result)
        {
            StringBuilder strSql = new StringBuilder();
            // Added By Jags for troubleshooting on 11/05/12
            //System.Console.WriteLine(" Inside UpdateTransactionLog.");
            // Added By Jags for troubleshooting on 11/05/12
            if (log.IsInfoEnabled) log.Info("UpdateTransactionLog started..");
            string val = string.Empty;
            try
            {
                // Added By Jags for troubleshooting on 11/05/12
                System.Console.WriteLine(" Records to be Updated : " + result.Count().ToString());
                // Added By Jags for troubleshooting on 11/05/12
                foreach (var item in result)
                {
                    // Added By Jags for troubleshooting on 11/05/12
                    System.Console.WriteLine(" Record to be Updated KioskTxnRefNum : " + item.Key + " Status : " + item.Value);
                    // Added By Jags for troubleshooting on 11/05/12

                    if (item.Value == "Successfully Posted" || item.Value == "Already Posted")
                    {
                        if (string.IsNullOrEmpty(val))
                            val += "'" + item.Key.ToString() + "'";
                        else
                            val += string.Format(",{0}", "'" + item.Key.ToString() + "'");
                    }

                }

                if (log.IsInfoEnabled) log.Info("UpdateTransactionLog" + val.ToString());
                if (!String.IsNullOrEmpty(val))
                {
                    strSql.AppendFormat("Update tbltransactions Set Uploaded = '1' Where KIOSKTxnRefNum in ({0})", val.ToString());

                    Open(strConnectionString);
                    SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                    ExecuteNonQuery(command);
                    // Added By Jags for troubleshooting on 11/05/12
                    System.Console.WriteLine(" Update in local database successful.");
                    // Added By Jags for troubleshooting on 11/05/12
                    if (log.IsInfoEnabled) log.Info(val.ToString() + "added in db from UpdateTransactionLog()");
                }
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.UpdateTransactionLog.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }
            catch (Exception ex)
            {
                // Added By Jags for troubleshooting on 11/05/12
                //System.Console.WriteLine(" Update in local database failed : " + ex.Message);
                // Added By Jags for troubleshooting on 11/05/12
                if (log.IsErrorEnabled) log.Error("Generic exception caught in UpdateTransactionLog" + ex.Message.ToString());

            }
            if (log.IsInfoEnabled) log.Info("UpdateTransactionLog ended..");
        }


        public static void UpdateCashCycle(Dictionary<string, string> result)
        {
            StringBuilder strSql = new StringBuilder();

            System.Console.WriteLine(" Inside UpdateCashCycle.");
            // Added By Jags for troubleshooting on 11/05/12
            if (log.IsInfoEnabled) log.Info("UpdateCashCycle started..");

            string valCashCycleID = string.Empty;
            string valTxnId = string.Empty;
            try
            {
                // Added By Jags for troubleshooting on 11/05/12
                System.Console.WriteLine(" Records to be Updated : " + result.Count().ToString());
                // Added By Jags for troubleshooting on 11/05/12
                foreach (var item in result)
                {
                    // Added By Jags for troubleshooting on 11/05/12
                    System.Console.WriteLine(" Record to be Updated TxnId : " + item.Key + " Status : " + item.Value);
                    // Added By Jags for troubleshooting on 11/05/12
                    
                    if (item.Value == "Successfully Posted" || item.Value == "Already Posted")
                    {
                        if (item.Key.Contains("#"))
                        {
                            string[] keys = item.Key.Split('#');                    

                            if (string.IsNullOrEmpty(valTxnId))
                                valTxnId += "'" + keys[0].ToString() + "'";
                            else
                                valTxnId += string.Format(",{0}", "'" + keys[0].ToString() + "'");


                            if (string.IsNullOrEmpty(valCashCycleID))
                                valCashCycleID += keys[1].ToString();
                            else
                                valCashCycleID += string.Format(",{0}", keys[1].ToString());
                        }
                    }
                }
                if (log.IsInfoEnabled) log.Info("UpdateCashCycle" + valCashCycleID.ToString());

                if (!string.IsNullOrEmpty(valCashCycleID))
                {
                    //strSql.AppendFormat("Update tbltxnCashCycle Set Uploaded = '1' Where TxnId in ({0})", val.ToString());
                    strSql.AppendFormat("Update tbltxnCashCycle Set Uploaded = '1'  Where CashCycleId in ({0}) and TxnId in ({1})", valCashCycleID,valTxnId);
                    if (log.IsInfoEnabled) log.Info("strSql:" + strSql.ToString());
                    Open(strConnectionString);
                    SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                    ExecuteNonQuery(command);
                    if (log.IsInfoEnabled) log.Info(valCashCycleID.ToString() + "added in db from UpdateCashCycle()");
                }
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.UpdateCashCycle.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  

            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic exception caught in UpdateCashCycle" + ex.Message.ToString());
            }
            if (log.IsInfoEnabled) log.Info("UpdateCashCycle ended..");
        }

        public static short UpdatePrintCycle(List<PrintCycleLog> req)
        {
            if (log.IsInfoEnabled) log.Info("UpdatePrintCycle started..");
            StringBuilder strSql = new StringBuilder();
            short result = 0;
            string val = null;
            try
            {
                foreach (PrintCycleLog item in req)
                {
                    if (string.IsNullOrEmpty(val))
                        val += "'" + item.PrnCycleId.ToString() + "'";
                    else
                        val += string.Format(",{0}", "'" + item.PrnCycleId.ToString() + "'");
                }

                if (log.IsInfoEnabled) log.Info("UpdatePrintCycle" + val.ToString());

                if (!string.IsNullOrEmpty(val))
                {
                    strSql.AppendFormat("Update tbltxnPrintCycle Set Uploaded = '1' Where PrnCycleId in ({0})", val.ToString());

                    Open(strConnectionString);
                    SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                    result = (short)ExecuteNonQuery(command);
                    if (log.IsInfoEnabled) log.Info(val.ToString() + " updated in db from UpdatePrintCycle()");
                }
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.UpdatePrintCycle.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (log.IsErrorEnabled) log.Error("Generic exception caught in UpdatePrintCycle" + ex.Message.ToString());
            }
            if (log.IsInfoEnabled) log.Info("UpdatePrintCycle ended..");
            return result;
        }

        public static void UpdateCommandIssuedStatus(string packageId)
        {

            StringBuilder strSql = new StringBuilder();
            int result = 0;
            try
            {
                if (!string.IsNullOrEmpty(packageId))
                {
                    strSql.AppendFormat("Update tblPackageMaster Set CommandIssued = '1',IssuedDateTime='" + FormatDateTime(DateTime.Now) + "' Where PackageId in ({0})", packageId.ToString());
                    if (log.IsInfoEnabled) log.Info("strSql in UpdateCommandIssuedStatus is:" + strSql.ToString());

                    Open(strConnectionString);
                    SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                    result = (short)ExecuteNonQuery(command);
                    if (log.IsInfoEnabled) log.Info(packageId.ToString() + "updated in  tblPackageMaster through UpdateCommandIssuedStatus");
                }

            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic exception caught in UpdateCommandIssuedStatus" + ex.Message.ToString());

            }

        }


        public static void UpdateCommandExecutedStatus(string packageId)
        {

            StringBuilder strSql = new StringBuilder();
            int result = 0;
            try
            {
                if (!string.IsNullOrEmpty(packageId))
                {
                    strSql.AppendFormat("Update tblPackageMaster Set CommandExecuted = '1' Where PackageId in ({0})", packageId.ToString());
                    if (log.IsInfoEnabled) log.Info("strSql in UpdateCommandExecutedStatus is:" + strSql.ToString());

                    Open(strConnectionString);
                    SqlCeCommand command = new SqlCeCommand(strSql.ToString());
                    result = (short)ExecuteNonQuery(command);
                    if (log.IsInfoEnabled) log.Info(packageId.ToString() + "updated in  tblPackageMaster  through UpdateCommandExecutedStatus");
                }

            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic exception caught in UpdateCommandExecutedStatus" + ex.Message.ToString());

            }

        }




        #endregion

        #endregion

         

        #region private properties

        //private static SqlCeConnection ConnectDb()
        //{


        //    if (System.Configuration.ConfigurationManager.ConnectionStrings["LocalDbConnectionString"]==null)
        //    {
        //        if (log.IsErrorEnabled) log.Error("LocalDbConnectionString Key not configured");
        //        throw new Exception("LocalDbConnectionString Key not configured");
        //        Console.ReadLine();

        //    }

        //    strConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["LocalDbConnectionString"].ToString();

        //    if (string.IsNullOrEmpty(strConnectionString))
        //     {
        //         if (log.IsErrorEnabled) log.Error("The Local database connection string not found \n Please add the connectionstring key named (LocalDbConnectionString) in app.config file");
        //         throw new Exception("The Local database connection string not found \n Please add the connectionstring key named (LocalDbConnectionString) in app.config file");
        //         Console.ReadLine();
        //    }

        //    if(_instance == null)
        //    {
        //        _instance = new SqlCeConnection(strConnectionString);
        //    }


        //    return _instance; 
        //}

        private static SnapShotLogger getSnapShortDetails(string strSql)
        {
            if (log.IsInfoEnabled) log.Info("SelectTransaction started..");

            SnapShotLogger objsnapshort = new SnapShotLogger();
            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql);
                SqlCeDataReader reader = ExecuteReader(command);

                while (reader.Read())
                {
                    

                    objsnapshort.BillerServiceId = Convert.ToInt32(reader["BillerServiceId"]);
                    objsnapshort.ChangedDate = Convert.ToDateTime(reader["ChangedDate"]);
                    objsnapshort.SnapSortId = Convert.ToInt32(reader["SnapSortId"]);
                    objsnapshort.Type = Convert.ToString(reader["Type"]);
                    objsnapshort.Path = Convert.ToString(reader["Path"]); 

                    
                }
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.SelectTransaction.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic exception caught in SelectTransaction .." + ex.Message);
            }
            if (log.IsInfoEnabled) log.Info("SelectTransaction ended..");

            return objsnapshort;
        }

        private static List<TransactionsLog> SelectTransaction(string strSql)
        {
            if (log.IsInfoEnabled) log.Info("SelectTransaction started..");           

            List<TransactionsLog> lstTrans = new List<TransactionsLog>();

            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql);
                SqlCeDataReader reader = ExecuteReader(command);

                while (reader.Read())
                {
                    TransactionsLog objTrLog = new TransactionsLog();
                    objTrLog.TxnId = Convert.ToInt64(reader["TxnId"]);
                    objTrLog.KioskId = Convert.ToInt64(reader["KioskId"]);
                    objTrLog.TxnTypeId = Convert.ToInt32(reader["TxnTypeId"]);
                    objTrLog.TxnDtTm = Convert.ToDateTime(reader["TxnDtTm"]);
                    objTrLog.KioskTxnDateTime = Convert.ToDateTime(reader["KioskTxnDttm"]);
                    objTrLog.TxnAmount = Convert.ToInt32(reader["TxnAmount"]);
                    objTrLog.KioskTxnrefNum = reader["KioskTxnrefNum"].ToString();
                    objTrLog.KioskTxnId = Convert.ToInt64(reader["KioskTxnId"]);
                    objTrLog.BillerId = Convert.ToInt32(reader["BillerId"]);
                    objTrLog.BillerTxnrefNum = Convert.ToString(reader["BillerTxnrefNum"]);
                    objTrLog.ConsumerNumber = reader["ConsumerNumber"].ToString();
                    objTrLog.PaymentModeId = Convert.ToInt32(reader["PaymentModeId"]);
                    objTrLog.ProductDetail = Convert.ToString(reader["ProductDetail"]);
                    objTrLog.LastBalance = Convert.ToDouble(reader["LastBalance"].ToString());
                    objTrLog.NewBalance = Convert.ToDouble(reader["NewBalance"].ToString());
                    objTrLog.Repost = Convert.ToBoolean(reader["Repost"].ToString());
                    objTrLog.Field1 = Convert.ToString(reader["Field1"]);
                    objTrLog.Field2 = Convert.ToString(reader["Field2"]);
                    objTrLog.Field3 = Convert.ToString(reader["Field3"]);
                    objTrLog.Field7 = Convert.ToString(reader["Field7"]);
                    objTrLog.Field8 = Convert.ToString(reader["Field8"]);
                    objTrLog.Field9 = Convert.ToString(reader["Field9"]);
                    objTrLog.Field10 = Convert.ToString(reader["Field10"]);
                    objTrLog.Field11 = Convert.ToString(reader["Field11"]);
                    objTrLog.Field12 = Convert.ToString(reader["Field12"]);
                    objTrLog.Field13 = Convert.ToString(reader["Field13"]);
                    objTrLog.Field14 = Convert.ToString(reader["Field14"]);
                    objTrLog.Field15 = Convert.ToString(reader["Field15"]);
                    objTrLog.Field16 = Convert.ToString(reader["Field16"]);
                    objTrLog.Field17 = Convert.ToString(reader["Field17"]);
                    objTrLog.Field18 = Convert.ToString(reader["Field18"]);
                    objTrLog.Field19 = Convert.ToString(reader["Field19"]);
                    objTrLog.Field20 = Convert.ToString(reader["Field20"]);

                    lstTrans.Add(objTrLog);
                    if (lstTrans.Count() == BatchSize)
                        break;
                }

                reader.Close();
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.SelectTransaction.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic exception caught in SelectTransaction .." + ex.Message);
            }
            if (log.IsInfoEnabled) log.Info("SelectTransaction ended..");
            return lstTrans;
        }

        private static List<EventLogger> SelectEventLog(string strSql)
        {
            if (log.IsInfoEnabled) log.Info("SelectEventLog started..");

            List<EventLogger> lstTrans = new List<EventLogger>();

            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql);
                SqlCeDataReader reader = ExecuteReader(command);

                while (reader.Read())
                {
                    lstTrans.Add(new EventLogger
                    {
                        KioskId = Convert.ToInt32(reader["KioskId"]),
                        EventDtTm = Convert.ToDateTime(reader["EventDtTm"]),
                        EventId = Convert.ToString(reader["EventId"]),
                        Desc = reader["Description"].ToString(),
                        EventLogId = Convert.ToInt32(reader["EventLogId"])
                    });
                    if (lstTrans.Count() == BatchSize)
                        break;
                }
                reader.Close();

            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.SelectEventLog.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic exception caught in SelectEventLog .." + ex.Message);

            }
            if (log.IsInfoEnabled) log.Info("SelectEventLog ended..");

            return lstTrans;
        }

        private static List<CashCycleLog> SelectTxnCashCycle(string strSql)
        {
            if (log.IsInfoEnabled) log.Info("SelectTxnCashCycle started..");
            List<CashCycleLog> lstTrans = new List<CashCycleLog>();
            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql);
                SqlCeDataReader reader = ExecuteReader(command);

                while (reader.Read())
                {
                    lstTrans.Add(new CashCycleLog
                    {
                        CashCycleId = Convert.ToInt32(reader["CASHCYCLEID"]),
                        TxnId = Convert.ToString(reader["TXNID"]),                        
                        TimeIns = Convert.ToDateTime(reader["TIMEINS"]),
                        NoteVal = Convert.ToInt32(reader["NoteVal"]),
                        NoteId = Convert.ToInt32(reader["NOTEID"]),
                        Accepted = Convert.ToInt32(reader["ACCEPTED"]),
                        Uploaded = Convert.ToInt32(reader["UPLOADED"]),
                        KioskId = Convert.ToString(reader["KIOSKID"]),
                        //CurrencyCode = Convert.ToString(reader["CURRENCYCODE"])
                    });
                    if (log.IsErrorEnabled) log.Error("CashCycleId:" + Convert.ToInt32(reader["CASHCYCLEID"]).ToString() + ",TxnId:" + Convert.ToString(reader["TXNID"]) + ",KioskId:" + Convert.ToString(reader["KIOSKID"]) + ",TimeIns:" + Convert.ToDateTime(reader["TIMEINS"]).ToString() + ",NoteVal:" + Convert.ToInt32(reader["NoteVal"]).ToString() + "is read.");

                    if (lstTrans.Count() == BatchSize)
                        break;

                }

                reader.Close();

            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.SelectTxnCashCycle.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic exception caught in SelectTxnCashCycle .." + ex.Message);
            }

            if (log.IsInfoEnabled) log.Info("SelectTxnCashCycle ended..");
            return lstTrans;
        }

        private static List<PrintCycleLog> SelectTxnPrintCycle(string strSql)
        {
            if (log.IsInfoEnabled) log.Info("SelectTxnPrintCycle started..");           

            List<PrintCycleLog> lstTrans = new List<PrintCycleLog>();

            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql);
                SqlCeDataReader reader = ExecuteReader(command);

                while (reader.Read())
                {
                    lstTrans.Add(new PrintCycleLog
                    {
                        PrnCycleId = Convert.ToString(reader["PRNCYCLEID"]),
                        TxnId = Convert.ToString(reader["TXNID"]),
                        TimePrt = Convert.ToDateTime(reader["TIMEPRT"]),
                        ReceiptId = Convert.ToString(reader["RECID"]),
                        ReceiptTaken = Convert.ToInt16(reader["RECTAKEN"]),
                        Uploaded = Convert.ToInt16(reader["Uploaded"]),
                        KioskId = Convert.ToInt64(reader["KioskID"])
                    });

                    if (log.IsErrorEnabled) log.Error("PrnCycleId:" + Convert.ToString(reader["PRNCYCLEID"]) + "TxnId:" + Convert.ToString(reader["TXNID"]) + ",KioskId:" + Convert.ToInt64(reader["KioskID"]).ToString() + "is read.");

                    if (lstTrans.Count() == BatchSize)
                        break;
                  
                }

                reader.Close();

            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.SelectTxnPrintCycle.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic exception caught in SelectTxnPrintCycle .." + ex.Message);
            }

            if (log.IsInfoEnabled) log.Info("SelectTxnPrintCycle ended..");
            return lstTrans;
        }



        private static PackageFileLog SelectPackage(string strSql)
        {
            if (log.IsInfoEnabled) log.Info("SelectPackage started..");
            PackageFileLog pkginfo = null;
            try
            {

                Open(strConnectionString);

                //new 

                SqlCeCommand getdate = new SqlCeCommand("select getdate()");
                SqlCeDataReader readerdate = ExecuteReader(getdate);


                while (readerdate.Read())
                {
                    string result = readerdate.GetValue(0).ToString();
                    if (log.IsInfoEnabled) log.Info("result getdate():" + result);

                }
                readerdate.Close();


                SqlCeCommand a1 = new SqlCeCommand("Select  PlannedActivationDttm from tblPackageMaster where  Deployed='false' and PackageStatus='2' and CommandIssued='false' order  by PlannedActivationDttm");
                SqlCeDataReader dr = ExecuteReader(a1);


                while (dr.Read())
                {
                    string result1 = dr.GetValue(0).ToString();
                    if (log.IsInfoEnabled) log.Info("result PlannedActivationDttm:" + result1);
                    break;

                }
                dr.Close();



                //new end











                pkginfo = new PackageFileLog();
                SqlCeCommand command = new SqlCeCommand(strSql);
                SqlCeDataReader reader = ExecuteReader(command);

                while (reader.Read())
                {
                    if (log.IsInfoEnabled) log.Info("Reading Started...");
                    pkginfo.PackageId = Convert.ToInt32(reader["PackageId"].ToString());
                    pkginfo.ReleaseVersion = reader["ReleaseVersion"].ToString();
                    if (log.IsInfoEnabled) log.Info("Reading Ended...");
                    break;
                }
                reader.Close();
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.SelectPackage.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic exception caught in SelectPackage .." + ex.Message);
            }

            if (log.IsInfoEnabled) log.Info("SelectPackage ended..");

            return pkginfo;
        }



        private static PackageFileLog SelectPackageCommandInfo(string strSql)
        {
            if (log.IsInfoEnabled) log.Info("SelectPackage started..");
            PackageFileLog pkginfo = null;
            try
            {
                Open(strConnectionString);
                pkginfo = new PackageFileLog();
                SqlCeCommand command = new SqlCeCommand(strSql);
                SqlCeDataReader reader = ExecuteReader(command);

                while (reader.Read())
                {

                    pkginfo.PackageId = Convert.ToInt32(reader["PackageId"].ToString());
                    pkginfo.CommandExecuted = reader["CommandExecuted"] != null ? Convert.ToBoolean(reader["CommandExecuted"].ToString()) : false;
                    pkginfo.ExecutionDatetime = reader["ExecutionDatetime"] != null ? Convert.ToDateTime(reader["ExecutionDatetime"]) : DateTime.Now;
                    pkginfo.IssuedDatetime = reader["IssuedDatetime"] != null ? Convert.ToDateTime(reader["IssuedDatetime"]) : DateTime.Now;
                    pkginfo.QueueId = reader["QueueId"] != null ? Convert.ToInt32(reader["QueueId"].ToString()) : 0;


                    break;
                }
                reader.Close();
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.SelectPackage.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic exception caught in SelectPackage .." + ex.Message);
            }

            if (log.IsInfoEnabled) log.Info("SelectPackage ended..");

            return pkginfo;
        }


        /// <summary>
        /// queries not uploaded cash cycle to the server
        /// </summary>s
        /// <returns>Collection of records</returns>
        public static Dictionary<string, string> CleanUpKioskLocalDB()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            DateTime lastTwoDaysOld = DateTime.Now.AddDays(-8);
            string delCashCycleSql = "delete   from tbltxnCashCycle Where Uploaded = 'true' and TIMEINS <='" + FormatDateTime(lastTwoDaysOld) + "' ";
            string delPrintCycleSql = "delete    from tbltxnPrintCycle Where Uploaded = 'true' and TIMEPRT <='" + FormatDateTime(lastTwoDaysOld) + "' ";
            string delTransactionsSql = "delete    from tblTransactions Where Uploaded = 'true' and KIOSKTXNDTTM <='" + FormatDateTime(lastTwoDaysOld) + "' ";
            string delEventLogSql = "delete    from tblEventLog Where Uploaded = 'true' and EVENTDTTM <='" + FormatDateTime(lastTwoDaysOld) + "' ";

            string delPkgMasterSql = "delete  from  tblPackageMaster   Where Deployed = 'true' and Downloaded = 'true' and PackageStatus = '2' ";
            string delPkgDetSql  = "delete    from tblPackageDetails  Where COMPLETE = 'True'";


            if (log.IsErrorEnabled) log.Error("CleanUpKioskLocalDB started..");
            try
            {
                Open(strConnectionString);
                SqlCeCommand command1 = new SqlCeCommand(delCashCycleSql);
                int result1= ExecuteNonQuery(command1);
                if (result1 > 0)
                    result.Add("tbltxnCashCycle Cleanup", result1.ToString() + " rows deleted");              
                 
                SqlCeCommand command2 = new SqlCeCommand(delPrintCycleSql);
                int result2 = ExecuteNonQuery(command2);
                if (result2 > 0)
                    result.Add("tbltxnPrintCycle Cleanup", result2.ToString() + " rows deleted");               


                SqlCeCommand command3 = new SqlCeCommand(delTransactionsSql);
                int result3 = ExecuteNonQuery(command3);
                if (result3 > 0)
                    result.Add("tblTransactions Cleanup", result3.ToString() + " rows deleted");               


                SqlCeCommand command4 = new SqlCeCommand(delEventLogSql);
                int result4 = ExecuteNonQuery(command4);
                if (result4 > 0)
                    result.Add("tblEventLog Cleanup", result4.ToString()+ " rows deleted");                


                SqlCeCommand command5 = new SqlCeCommand(delPkgMasterSql);
                int result5 = ExecuteNonQuery(command5);
                if (result5 > 0)
                    result.Add("tblPackageMaster Cleanup", result5.ToString() + " rows deleted");  
                

                SqlCeCommand command6 = new SqlCeCommand(delPkgDetSql);
                int result6 = ExecuteNonQuery(command6);
                if (result6 > 0)
                    result.Add("tblPackageDetails Cleanup", result6.ToString() + " rows deleted");       


                foreach(var val in result)
                {
                    if (log.IsErrorEnabled) log.Error(val.Key + " "+ val.Value);                   
                }

            }
            catch (SqlCeException ex)
            { 
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in CleanUpKioskLocalDB.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic exception caught in CleanUpKioskLocalDB .." + ex.Message);
            }
            
            return result;
        }

        #endregion

        //intPkgId, file.PackageDetailId, file.PkgFileDetailId, file.FileName, file.FilePath, file.FileType, file.FileSize.ToString()

        /// <summary>
        /// gets all transactions
        /// </summary>
        /// <returns>List Collection of transaction table</returns>
        public static TransactionsLog GetTransactionDetailsbyTransactionId(string kioskTxnRefNum)
        {
            //string strSql = "SELECT ISNULL(TXNID, 0) AS TXNID, ISNULL(KIOSKID, 0) AS KIOSKID, ISNULL(TXNTYPEID, 0) AS TXNTYPEID, ISNULL(TXNDTTM, '') AS TXNDTTM, ISNULL(TXNAMOUNT, 0)  AS TXNAMOUNT, ISNULL(KIOSKTXNREFNUM, '') AS KIOSKTXNREFNUM, ISNULL(KIOSKTXNID, 0) AS KIOSKTXNID, ISNULL(KIOSKTXNDTTM, '') AS KIOSKTXNDTTM,  ISNULL(BILLERID, 0) AS BILLERID, ISNULL(BILLERTXNREFNUM, '') AS BILLERTXNREFNUM, ISNULL(CONSUMERNUMBER, '') AS CONSUMERNUMBER,  ISNULL(PAYMENTMODEID, 0) AS PAYMENTMODEID, ISNULL(PRODUCTDETAIL, '') AS PRODUCTDETAIL, ISNULL(LASTBALANCE, 0) AS LASTBALANCE,  ISNULL(NEWBALANCE, 0) AS NEWBALANCE, ISNULL(UPLOADED, 0) AS UPLOADED FROM TBLTRANSACTIONS";
            string strSql = "Select  * from tblTransactions Where Field1='false' and kioskTxnRefNum='" + kioskTxnRefNum + "' ";

            return GetTransaction(strSql);
        }


        private static TransactionsLog GetTransaction(string strSql)
        {
            if (log.IsInfoEnabled) log.Info("GetTransaction started..");

            TransactionsLog objTrLog = null;

            try
            {
                Open(strConnectionString);
                SqlCeCommand command = new SqlCeCommand(strSql);
                SqlCeDataReader reader = ExecuteReader(command);

                while (reader.Read())
                {
                    objTrLog = new TransactionsLog();
                    objTrLog.TxnId = Convert.ToInt64(reader["TxnId"]);
                    objTrLog.KioskId = Convert.ToInt64(reader["KioskId"]);
                    objTrLog.TxnTypeId = Convert.ToInt32(reader["TxnTypeId"]);
                    objTrLog.TxnDtTm = Convert.ToDateTime(reader["TxnDtTm"]);
                    objTrLog.KioskTxnDateTime = Convert.ToDateTime(reader["KioskTxnDttm"]);
                    objTrLog.TxnAmount = Convert.ToInt32(reader["TxnAmount"]);
                    objTrLog.KioskTxnrefNum = reader["KioskTxnrefNum"].ToString();
                    objTrLog.KioskTxnId = Convert.ToInt64(reader["KioskTxnId"]);
                    objTrLog.BillerId = Convert.ToInt32(reader["BillerId"]);
                    objTrLog.BillerTxnrefNum = Convert.ToString(reader["BillerTxnrefNum"]);
                    objTrLog.ConsumerNumber = reader["ConsumerNumber"].ToString();
                    objTrLog.PaymentModeId = Convert.ToInt32(reader["PaymentModeId"]);
                    objTrLog.ProductDetail = Convert.ToString(reader["ProductDetail"]);
                    objTrLog.LastBalance = Convert.ToDouble(reader["LastBalance"].ToString());
                    objTrLog.NewBalance = Convert.ToDouble(reader["NewBalance"].ToString());
                    objTrLog.Repost = Convert.ToBoolean(reader["Repost"].ToString());
                    objTrLog.Field1 = Convert.ToString(reader["Field1"]);
                    objTrLog.Field2 = Convert.ToString(reader["Field2"]);
                    objTrLog.Field3 = Convert.ToString(reader["Field3"]);
                    objTrLog.Field7 = Convert.ToString(reader["Field7"]);
                    objTrLog.Field8 = Convert.ToString(reader["Field8"]);
                    objTrLog.Field9 = Convert.ToString(reader["Field9"]);
                    objTrLog.Field10 = Convert.ToString(reader["Field10"]);
                    objTrLog.Field11 = Convert.ToString(reader["Field11"]);
                    objTrLog.Field12 = Convert.ToString(reader["Field12"]);
                    objTrLog.Field13 = Convert.ToString(reader["Field13"]);
                    objTrLog.Field14 = Convert.ToString(reader["Field14"]);
                    objTrLog.Field15 = Convert.ToString(reader["Field15"]);
                    objTrLog.Field16 = Convert.ToString(reader["Field16"]);
                    objTrLog.Field17 = Convert.ToString(reader["Field17"]);
                    objTrLog.Field18 = Convert.ToString(reader["Field18"]);
                    objTrLog.Field19 = Convert.ToString(reader["Field19"]);
                    objTrLog.Field20 = Convert.ToString(reader["Field20"]);

                    break;
                }

                reader.Close();
            }
            catch (SqlCeException ex)
            {
                if (log.IsErrorEnabled) log.Error("SqlCeException caught in DBLogger.GetTransaction.." + ex.Message);
                Close();
                //System.Environment.Exit(1);  //  exit  
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Generic exception caught in GetTransaction .." + ex.Message);
            }
            if (log.IsInfoEnabled) log.Info("GetTransaction ended..");
            return objTrLog;
        }

        public static int UpdateTransaction(string kioskTxnRefNum)
        {
            int result1=0;
            //string strSql = "SELECT ISNULL(TXNID, 0) AS TXNID, ISNULL(KIOSKID, 0) AS KIOSKID, ISNULL(TXNTYPEID, 0) AS TXNTYPEID, ISNULL(TXNDTTM, '') AS TXNDTTM, ISNULL(TXNAMOUNT, 0)  AS TXNAMOUNT, ISNULL(KIOSKTXNREFNUM, '') AS KIOSKTXNREFNUM, ISNULL(KIOSKTXNID, 0) AS KIOSKTXNID, ISNULL(KIOSKTXNDTTM, '') AS KIOSKTXNDTTM,  ISNULL(BILLERID, 0) AS BILLERID, ISNULL(BILLERTXNREFNUM, '') AS BILLERTXNREFNUM, ISNULL(CONSUMERNUMBER, '') AS CONSUMERNUMBER,  ISNULL(PAYMENTMODEID, 0) AS PAYMENTMODEID, ISNULL(PRODUCTDETAIL, '') AS PRODUCTDETAIL, ISNULL(LASTBALANCE, 0) AS LASTBALANCE,  ISNULL(NEWBALANCE, 0) AS NEWBALANCE, ISNULL(UPLOADED, 0) AS UPLOADED FROM TBLTRANSACTIONS";
            string strSql = "Update tbltransactions set Field1='true'  where kioskTxnRefNum='" + kioskTxnRefNum + "' ";
            try
            {
                Open(strConnectionString);
                SqlCeCommand command1 = new SqlCeCommand(strSql);
                result1 = ExecuteNonQuery(command1);
            }
            catch(Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Caught exception in UpdateTransaction."+ex.Message);
                Close();
            }
            return result1;
        }

         
 
    }
}
