using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.BaseClasses;
using MBMEKiosk.Infrastructure.Events;
using System.Windows.Threading;
using monitoringProxy;
using MBMEKiosk.LogonProxy;
using System.Diagnostics;
using System.Configuration;
using MBMEKiosk.Infrastructure.Utils;
using System.Windows;
using MBMEKiosk.Infrastructure.Interfaces;
using MBMEKiosk.ObjectModel;
using MBMEKiosk.Infrastructure.ObjectModel;
using System.ServiceModel;
using System.Windows.Controls;
using System.Windows.Input;
using MBMEKiosk.MBME.Command;
using MBMEKioskLogger.Logger;
using log4net;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;
using System.Reflection;
using MBMEKiosk.Infrastructure.Commands;
using System.Security.Cryptography;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace MBMEKiosk.MBME.Presenters
{

    

    public class ModuleSelectionPresenter : MBMEPresenterBase
    {
        private DispatcherTimer connectivityTimer;
        private DispatcherTimer shutdownTimer;
        private DispatcherTimer logonTimer;
        private MBMEMonitoringServiceClient monitorClient;        
        private List<DeviceEntity> deviceList;
        private DeviceEntity objKiosk;
        private DeviceEntity objPrinter;
        private DeviceEntity objCashAcceptor;
        private HealthCheckResponse objServiceResponse;
        private bool IsBackendServiceConnected;
        private bool connectivityPolling = false;
        private bool shutdownCommandActive;
        private string lastPackageDet;
        private short notPolled = 0;
        // Added By JK on 01/09/12
        private bool loggedOn = true;
        //private KioskCommand command = new KioskCommand();
        private monitoringProxy.KioskCommand command = new monitoringProxy.KioskCommand();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ProcessStartInfo procStartInfo;
        private Process proc;
        private DispatcherTimer AppTerminationTimer;
        private short pollcounter;
        private bool isMonitored = false;
        private static readonly string KIOSKEXEPATH = "MBMEKiosk.exe";
        private static readonly string CMEXEPATH = "modules\\MBMEContentManager.exe";
        private static readonly string WATCHDOGEXEPATH = "C:\\USP\\KioskPlatform\\MBMEWatchDog\\MBMEWatchDog.exe";
        private string adminkey;
        private KioskParameterisedCommand<string> adminCommand;

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetSystemTime([In] ref SYSTEMTIME st);

        public ModuleSelectionPresenter()
        {
            deviceList = new List<DeviceEntity>();
            objKiosk = new monitoringProxy.DeviceEntity();
            objPrinter = new monitoringProxy.DeviceEntity();
            objCashAcceptor = new monitoringProxy.DeviceEntity();
            objServiceResponse = new HealthCheckResponse();
            IsBackendServiceConnected = InitConnectivityPolling();

            pollcounter = Convert.ToInt16(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["PUSHMONITORINGUPDATECOUNT"]) ? "10" : ConfigurationManager.AppSettings["PUSHMONITORINGUPDATECOUNT"]);

            //if (bool.Parse(ConfigurationManager.AppSettings["StandAloneMode"]) == false)
            //    IsBackendServiceConnected = InitConnectivityPolling();
            //else
            //    IsBackendServiceConnected = true;

        }

        public override void Deactivate()
        {
            if (log.IsInfoEnabled) log.Info(string.Format("{0} : Customer Service Mode.", DateTime.Now.ToString()));
            adminkey = string.Empty;
            base.Deactivate();
            if (connectivityTimer != null)
            {
                connectivityTimer.Stop();
                connectivityTimer.Tick -= OnConnectivityTimeOut;
                connectivityTimer = null;
            }
            if (shutdownTimer != null)
            {
                shutdownTimer.Stop();
                shutdownTimer.Tick -= OnShutdownTimeOut;
                shutdownTimer = null;
            }
            if (monitorClient != null)
                monitorClient.CheckBackendConnectivityCompleted -= new EventHandler<CheckBackendConnectivityCompletedEventArgs>(monitorClient_CheckBackendConnectivityCompleted);
   
            //this.Transaction.KioskServices = KioskAppConfig.KioskServices;

     }

        public override FrameworkElement LoadXaml(IDeviceAgent devices, KioskState state, TransactionContextBase transactionContext)
        {
            Trace.WriteLine(string.Format("{0} : Load Xaml Started", DateTime.Now));
            FrameworkElement viewGrid = base.LoadXaml(devices, state, transactionContext);
            //WrapPanel t1 = (WrapPanel)this.ViewGrid.FindName("ModuleSelectionPanel");
            //t1.Loaded += new RoutedEventHandler(OnLoad);
            if (log.IsInfoEnabled) log.Info(string.Format("{0} : Kiosk In Idle Mode.", DateTime.Now.ToString()));
            // Added By JK on 01/09/12
            this.Transaction.KioskMode = 1; // Indicating Idle Mode.
            if (!loggedOn)
            {
                Logon();
                LoadUtilities();
            }
            else
            {
                if (connectivityPolling)
                {
                    //if (isMonitored)
                    RestartConnectivityTimer();

                    if (monitorClient != null)
                        monitorClient.CheckBackendConnectivityCompleted += new EventHandler<CheckBackendConnectivityCompletedEventArgs>(monitorClient_CheckBackendConnectivityCompleted);
                }
                if (shutdownCommandActive)
                    RestartShutdownTimer();

            }
           adminkey = string.Empty;
            //this.Transaction.KioskServices = KioskAppConfig.KioskServices;

            return viewGrid;


        }

        #region admincommand
        protected override void RegisterCommands()
        {
            base.RegisterCommands();
            AdminCommand = new KioskParameterisedCommand<string>(ExecuteAdminCommand, CanExecuteAdminCommand, true);
            AdminCommand.ExecutionCompleted += Command_ExecutionCompleted;

        }

        public KioskParameterisedCommand<string> AdminCommand
        {
            get
            {
                return adminCommand;
            }

            private set
            {
                if (adminCommand != value)
                {
                    adminCommand = value;
                    OnPropertyChanged("AdminCommand");
                }
            }
        }

        private void ExecuteAdminCommand(string param)
        {
            if (log.IsInfoEnabled) log.InfoFormat("admin command executed param passed : {0}", param);

            adminkey += param;
            if (log.IsInfoEnabled) log.InfoFormat("adminkey val : {0}", adminkey);
            if (adminkey == "1234")
            {
                adminkey = string.Empty;
                OnKioskStateChanged(new KioskStateChangedEventArgs("admin"));
            }
        }

        private bool CanExecuteAdminCommand(string param)
        {
            return true;
        }

        void Command_ExecutionCompleted()
        {
            OnRequestLayoutUpdate();
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            //StackPanel t1 = (StackPanel)this.ViewGrid.FindName("ModuleSelectionPanel");
            WrapPanel t1 = (WrapPanel)this.ViewGrid.FindName("ModuleSelectionPanel");
            DependencyObject focusScope = FocusManager.GetFocusScope(t1);
            if (FocusManager.GetFocusedElement(focusScope) == null)
            {
                FocusManager.SetFocusedElement(focusScope, t1);
            }

            bool foc = t1.Focus();

        }

        public bool InitConnectivityPolling()
        {

            //open monitoring proxy
            try
            {

                if (monitorClient != null)
                    monitorClient.CheckBackendConnectivityCompleted -= new EventHandler<CheckBackendConnectivityCompletedEventArgs>(monitorClient_CheckBackendConnectivityCompleted);

                if (ConfigurationManager.AppSettings["ConnectivityStatusTimer"] != null)
                {
                    ValidateCertificate.RegisterCallback();
                    monitorClient = new MBMEMonitoringServiceClient();
                    monitorClient.Open();
                    connectivityPolling = true;
                    monitorClient.CheckBackendConnectivityCompleted += new EventHandler<CheckBackendConnectivityCompletedEventArgs>(monitorClient_CheckBackendConnectivityCompleted);
                }
            }
            catch (Exception ex)
            {
                monitorClient.Abort();
                InitConnectivityPolling();
                ValidateCertificate.DeregisterCallback();
                if (log.IsErrorEnabled) log.Error(ex.Message);
                if (connectivityTimer != null)
                {
                    connectivityTimer.Stop();
                }
                return false;
            }
            finally
            {

            }
            return true;

        }        

        void monitorClient_CheckBackendConnectivityCompleted(object sender, CheckBackendConnectivityCompletedEventArgs e)
        {
            try
            {
                if (log.IsInfoEnabled) log.Info("HealtCheck Service Call Completed" + DateTime.Now.ToString());
                IsBackendServiceConnected = e.Result.IsConnected;

                command = e.Result.Command;
                if (log.IsInfoEnabled) log.Info("Command Received :" + command.CommandParameter + ",command:" + command.ToString() + ",lastPackageDet:" + lastPackageDet);

                if (command.Command == "SHUTDOWN")
                {
                    if (lastPackageDet != command.CommandParameter)
                    {
                        lastPackageDet = command.CommandParameter;
                        shutdownCommandActive = true;
                        if (log.IsInfoEnabled) log.Info("Shutdown timer activated for:" + command.CommandParameter);
                    }

                    RestartShutdownTimer();
                }
                
                if (command.Command == "ONDEMAND-SHUTDOWN")
                {
                    shutdownCommandActive = true;
                    RestartShutdownTimer();
                    if (log.IsInfoEnabled) log.Info("Shutdown timer activated for:" + command.Command);
                }

                //if (command.Command == "MESSAGE-REPOSITORY-DOWNLOAD")
                //{
                //    //The Scrollers will change only if the message in commandparameter changes
                //    if (KioskAppConfig.Scrollers != command.CommandParameter)
                //    {
                //        KioskAppConfig.Scrollers = command.CommandParameter;
                //        Logger.LogCommand(command.Command, command.CommandParameter);
                //        OnKioskStateChanged(new KioskStateChangedEventArgs("mbme")); //Load the State again to enable scroller
                //    }
                //}

                if (command.Command == "START-MONITORING")
                {
                    if (log.IsInfoEnabled) log.Info("before Executing Start Monitoring ");
                    isMonitored = true;
                    InitConnectivityPolling();
                    //RestartLogonTimer();
                    if (log.IsInfoEnabled) log.Info("after Executing Start Monitoring ");


                }

                if (command.Command == "STOP-MONITORING")
                {
                    if (log.IsInfoEnabled) log.Info("before Executing Stop Monitoring ");
                    isMonitored = false;
                    //RestartLogonTimer();
                    if (log.IsInfoEnabled) log.Info("after Executing Stop Shutdown");
                    

                }

                try
                {
                    //Update the Command Status to the Portal db
                    if (command != null)
                    {
                        if (command.Command == "START-MONITORING" ||
                            command.Command == "STOP-MONITORING" ||
                            command.Command == "MESSAGE-REPOSITORY-DOWNLOAD")
                        {
                            //ValidateCertificate.RegisterCallback();
                            int result = monitorClient.UpdatePackageCommandStatus(new CommandRequest
                            {
                                KioskId = Convert.ToInt32(KioskAppConfig.KioskId),
                                QueueId = command.QueueId,
                                CommandExecuted = true,
                                IssuedDate = DateTime.Now,
                                ExecutedDate = DateTime.Now
                            });
                        }
                    }
                }
                catch (Exception ex)
                {

                    if (log.IsErrorEnabled) log.ErrorFormat("OnConnectivity Timeout {0} Error {1}", command.Command, ex.Message);
                }
                finally
                {
                    //ValidateCertificate.DeregisterCallback();
                }
            }
            catch (Exception ex)
            {
                monitorClient.Abort();
                if (log.IsInfoEnabled) log.Info("HealthCheck Service Call Completion Exception :" + DateTime.Now.ToString() + ex.Message);
                //+ "- InnerException : " + ex.InnerException.ToString());  
                if (ex.InnerException != null)
                {
                    if (log.IsErrorEnabled) log.ErrorFormat("HealthCheck Service Call Completion Inner Exception : {0}", ex.InnerException.Message);
                    if (log.IsErrorEnabled) log.ErrorFormat("HealthCheck Service Call Completion Inner Stacktrace : {0}", ex.InnerException.StackTrace);
                }

                IsBackendServiceConnected = false;
                InitConnectivityPolling();
            }
            finally
            {
                //if (isMonitored)
                RestartConnectivityTimer();
            }
        }

        protected override void ExecuteSubmitCommand(string param)
        {
            bool canLoadSelectedModule = true;
            if (param == "1" || param == "2" || param == "3" || param == "4")
                adminkey += param;

            try
            {

                Transaction.SelectedLanguageKey = "english";

                if (this.Devices.GetCashAcceptor().IsReady())
                {
                    Transaction.CashDeviceStatus = "Cash Device: OK";
                }
                else
                {
                    Transaction.CashDeviceStatus = "Cash Device: BUSY/ERROR";
                    canLoadSelectedModule = false;
                }

                //if (this.Devices.GetRecycler().IsReady())
                //{
                //    Transaction.CashDeviceStatus = "Cash Device: OK";
                //}
                //else
                //{
                //    Transaction.CashDeviceStatus = "Cash Device: BUSY/ERROR";
                //    canLoadSelectedModule = false;
                //}

                if (this.Devices.GetPrinter().IsReady())
                {
                    Transaction.PrinterStatus = "Printer: OK";
                }
                else
                {
                    Transaction.PrinterStatus = "Printer: BUSY/UNAVAILABLE";
                    canLoadSelectedModule = false;
                }

                if (log.IsInfoEnabled) log.Info("Transaction.CashDeviceStatus:" + Transaction.CashDeviceStatus + " ,Transaction.PrinterStatus" + Transaction.PrinterStatus);

                //if (this.Devices.GetCardReader().IsReady())
                //{
                //    Transaction.CardReaderStatus = "Card Reader Initialized";
                //}
                //else
                //{
                //    Transaction.CardReaderStatus = "Card Reader: BUSY/UNAVAILABLE";
                //    canLoadSelectedModule = false;
                //}

                //if (this.Devices.GetPackDispenser().IsReady())
                //{
                //    if (log.IsInfoEnabled) log.Info("PackDispenser Initialized.");
                //    Transaction.PackDispenserStatus = "Pack Dispenser Initialized";
                //}
                //else
                //{
                //    if (log.IsInfoEnabled) log.Info("PackDispenser Error.");
                //    Transaction.PackDispenserStatus = "Pack Dispenser BUSY/UNAVAILABLE";
                //    canLoadSelectedModule = false;
                //}

                if ((bool.Parse(ConfigurationManager.AppSettings["StandAloneMode"])) || IsBackendServiceConnected)
                {
                    Transaction.ServiceStatus = "Service: ONLINE";

                }
                else
                {
                    Transaction.ServiceStatus = "Service: OFFLINE";
                    //Allowed to do offline transaction for Ewallet
                    canLoadSelectedModule = false;
                }
                if (log.IsInfoEnabled) log.Info("loggedOn:" + loggedOn + " ,Transaction.ServiceStatus" + Transaction.ServiceStatus);
                if (!loggedOn)
                {
                    Transaction.LogonStatus = "Kiosk not logged on.";
                    //Disabling logon for offline transactions for Ewallet
                    canLoadSelectedModule = false;
                }

                if (log.IsInfoEnabled) log.Info("canLoadSelectedModule:" + canLoadSelectedModule);
                if (canLoadSelectedModule)
                {
                    this.Transaction.KioskMode = 2; // Indicating Customer Service Mode
                    if (param == "dbpolice")
                    {
                        //procStartInfo = new ProcessStartInfo("sample");
                        procStartInfo = new ProcessStartInfo("Myshell.vbs");
                        proc = new Process();
                        proc.StartInfo = procStartInfo;
                        proc.Start();
                        RestartAppTerminationTimer();
                    }
                    else
                    {
                        if (param == "otherapp")
                            ShowOtherApps = true;
                        else
                            ShowOtherApps = false;

                        if (adminkey == "1234")
                        {
                            adminkey = string.Empty;
                            OnKioskStateChanged(new KioskStateChangedEventArgs("admin"));
                        }
                        else
                            if (string.IsNullOrEmpty(adminkey))
                                OnKioskStateChanged(new KioskStateChangedEventArgs(param));
                    }

                    // OnKioskStateChanged(new KioskStateChangedEventArgs(param));
                }
                else
                {
                    //if (adminkey == "1234")
                    //{
                    //    adminkey = string.Empty;
                    //    OnKioskStateChanged(new KioskStateChangedEventArgs("admin"));
                    //}
                    //else
                    //    if (string.IsNullOrEmpty(adminkey))
                    if (log.IsInfoEnabled) log.Info("state changed to error");
                            OnKioskStateChanged(new KioskStateChangedEventArgs("error"));

                }
            }
            catch (Exception ex)
            {
                if (log.IsInfoEnabled) log.Info("Exception occured in ExecuteSubmitCommand." + ex.Message);
                if (ex.InnerException != null)
                {
                    if (!string.IsNullOrEmpty(ex.InnerException.Message))
                        if (log.IsErrorEnabled) log.Error("InnerException in ExecuteSubmitCommand of moduleselectionPresenter. " + ex.InnerException.Message);
                }
            }
            
        }

        protected override bool CanExecuteSubmitCommand(string param)
        {
            if (param == "1" || param == "2"
                || param == "3" || param == "4")
                return true;
            else
                return State.KioskActions.Where(a => a.Name.ToLower() == param).Count() == 1;
        }
        #endregion


        #region private methods
        // Added By JK on 01/09/12
        // Moved the code from Shell.xaml.cs to ModuleSelectionPresenter as the client wants
        // the Dispatcher to start even if the connection to backend is not established.
        private void Logon()
        {
            if (log.IsInfoEnabled) log.Info("Initiating Log on");
            int maxTime = (ConfigurationManager.AppSettings["MaxTurnTime"] == null) ? 20 : int.Parse(ConfigurationManager.AppSettings["MaxTurnTime"]);
            LogOnKioskResponse logonResponse = new LogOnKioskResponse();
            LogonServiceClient logonproxy = null;
            DateTime dtBeforeLogon, dtafterLogon, newDateTime;
            TimeSpan timeDiff;
            try
            {
                ValidateCertificate.RegisterCallback();
                logonproxy = new LogonServiceClient();

                if (log.IsInfoEnabled) log.Info("Requesting for Log on");
                dtBeforeLogon = DateTime.Now;
                logonResponse = logonproxy.LogOnKiosk(new LogOnKioskRequest
                {
                    KioskId = Int32.Parse(KioskAppConfig.KioskId),
                    StatusUpdated = DateTime.UtcNow,
                    ReleaseVersion = KioskAppConfig.CurrentVersion
                });
                
                dtafterLogon = DateTime.Now;
                timeDiff = dtafterLogon.Subtract(dtBeforeLogon);
                if (log.IsInfoEnabled) log.InfoFormat("Hours: {0}, Mins: {1} Sec: {2} Days:{3}", timeDiff.Hours, timeDiff.Minutes, timeDiff.Seconds, timeDiff.Days);
                if (log.IsInfoEnabled) log.Info("Requesting for Log on completed");

                if (!string.IsNullOrEmpty(logonResponse.XXX12))
                {
                    try
                    {
                        RegistryKey pRegKey = Registry.LocalMachine;
                        pRegKey = pRegKey.OpenSubKey("SOFTWARE\\USPInc");

                        SHA256 hasher = SHA256Managed.Create();
                        byte[] hashedData = hasher.ComputeHash(
                            Encoding.Unicode.GetBytes(logonResponse.XXX12));
                        StringBuilder sb = new StringBuilder();
                        foreach (byte b in hashedData)
                        {
                            sb.AppendFormat("{0:x2}", b);
                        }
                        
                        pRegKey.SetValue("xxx23", sb.ToString(), RegistryValueKind.String);
                    }
                    catch (Exception ex)
                    {
                        if (log.IsErrorEnabled) log.ErrorFormat("Set xxx12 error : {0}", ex.Message);
                    }
                }

                logonResponse.isLoggedOn = true;
                if (logonResponse.isLoggedOn)
                {
                    if (log.IsInfoEnabled) log.Info("Kiosk Logged on");
                    //Populate Service List

                    KioskAppConfig.KioskServices = new List<Infrastructure.ObjectModel.KioskService>();
                    KioskAppConfig.KioskDevices = new List<Infrastructure.ObjectModel.KioskDevice>();                    
                    KioskAppConfig.KioskLocation = logonResponse.KioskLocation;
                    isMonitored = logonResponse.isMonitored;
                    
                    

                    if (timeDiff.Seconds >= 0 &&
                    timeDiff.Seconds <= maxTime)
                    {
                        if (log.IsInfoEnabled) log.InfoFormat("Kiosk Logged on between the turnaround time with time : {0}",logonResponse.CurrentTimeStamp);
                        timeDiff = TimeSpan.FromTicks(timeDiff.Ticks / 2);
                        newDateTime = Convert.ToDateTime(logonResponse.CurrentTimeStamp);//.ToLocalTime();
                        newDateTime.Add(timeDiff);
                        
                        //admin Change the system time
                        if (log.IsInfoEnabled) log.Info("Synchronizing System Time");
                        SYSTEMTIME SysTime = new SYSTEMTIME();
                        SysTime.wYear = (short)newDateTime.Year;
                        SysTime.wMonth = (short)newDateTime.Month;
                        SysTime.wDay = (short)newDateTime.Day;
                        SysTime.wHour = (short)newDateTime.Hour;
                        SysTime.wMinute = (short)newDateTime.Minute;
                        SysTime.wSecond = (short)newDateTime.Second;
                        bool res = SetSystemTime(ref SysTime);

                        if (!res)
                        {
                            if (log.IsErrorEnabled) log.Error("System Time not changed Successfully");
                        }
                        else
                        {
                            if (log.IsInfoEnabled) log.Info("Changed System Time Successfully");
                        }
                    }

                       //this.Transaction.KioskServices = null;


                    foreach (MBMEKiosk.LogonProxy.KioskService service in logonResponse.BillerServiceList)
                    {
                        MBMEKiosk.Infrastructure.ObjectModel.KioskService item = new MBMEKiosk.Infrastructure.ObjectModel.KioskService()
                        {
                            BillerServiceId = service.BillerServiceId,
                            ServiceKey = service.ServiceKey,
                            ActivationDate = service.ActivationDate,
                            Available = service.Available,
                            BillerServiceName = service.BillerServiceName,
                            KioskId = service.KioskId,
                            KioskRefNum = service.KioskRefNum
                        };
                        KioskAppConfig.KioskServices.Add(item);
                    }

                    //Populate Device List
                    foreach (MBMEKiosk.LogonProxy.KioskDevice device in logonResponse.DeviceList)
                    {
                        MBMEKiosk.Infrastructure.ObjectModel.KioskDevice item = new MBMEKiosk.Infrastructure.ObjectModel.KioskDevice()
                        {
                            DeviceId = device.DeviceId,
                            //KioskDeviceId = device.KioskDeviceId,
                            DeviceKey = device.DeviceKey,
                            DeviceEnabled = device.DeviceEnabled,
                            DeviceName = device.DeviceName,
                            KioskId = device.KioskId,
                            KioskRefNum = device.KioskRefNum
                        };

                        KioskAppConfig.KioskDevices.Add(item);
                        if (log.IsInfoEnabled) log.Info("Machine Location retrieved is:" + KioskAppConfig.KioskLocation + ",MachineId:" + KioskAppConfig.KioskId);
                    }
                                       

                    // Modified By JK on 01/09/12
                    // Previously the developer had put AND condition but has been replaced with OR.
                    if ((logonResponse.BillerServiceList.Length == 0) &&
                        (logonResponse.DeviceList.Length == 0))
                    {
                        if (log.IsInfoEnabled) log.Info("Kiosk logged on not successful");
                        loggedOn = false;
                        if (log.IsInfoEnabled) log.Info("No Biller Service or Device configured for this Kiosk.");
                    }
                    else
                    {
                        loggedOn = true;
                        if (log.IsInfoEnabled) log.Info("Kiosk logged on successfull");
                        //initialize Devices
                        objKiosk.Name = ConfigurationManager.AppSettings["KDeviceId"].ToString(); ;

                        objPrinter.Name = ConfigurationManager.AppSettings["PDeviceId"].ToString();

                        objCashAcceptor.Name = ConfigurationManager.AppSettings["CADeviceId"].ToString();

                        var kioskDevice = (from A in KioskAppConfig.KioskDevices
                                           where A.DeviceName.Equals(objKiosk.Name)
                                           select A.DeviceId).SingleOrDefault();

                        objKiosk.DeviceId = kioskDevice.ToString();

                        var printerDevice = (from A in KioskAppConfig.KioskDevices
                                             where A.DeviceName.Equals(objPrinter.Name)
                                             select A.DeviceId).SingleOrDefault();

                        objPrinter.DeviceId = printerDevice.ToString();

                        var cashAcceptorDevice = (from A in KioskAppConfig.KioskDevices
                                                  where A.DeviceName.Equals(objCashAcceptor.Name)
                                                  select A.DeviceId).SingleOrDefault();

                        objCashAcceptor.DeviceId = cashAcceptorDevice.ToString();

                        if (connectivityPolling)
                        {
                            //if (isMonitored)
                            RestartConnectivityTimer();
                            //if (monitorClient == null)
                            //  monitorClient.CheckBackendConnectivityCompleted += new EventHandler<CheckBackendConnectivityCompletedEventArgs>(monitorClient_CheckBackendConnectivityCompleted);
                        }
                        if (shutdownCommandActive)
                            RestartShutdownTimer();
                    }
                }
            }
            catch (Exception ex)
            {
                loggedOn = false;
                if (logonproxy != null)
                    logonproxy.Abort();
                if (log.IsErrorEnabled) log.ErrorFormat("Kiosk logon unsuccessfull: {0} {1}", DateTime.Now.ToString(), ex.Message);
            }
            finally
            {
                if (!loggedOn)
                    RestartLogonTimer();
                if (logonproxy != null)
                    logonproxy.Close();
                ValidateCertificate.DeregisterCallback();
            }


        }

        private void LoadUtilities()
        {                
                MBMEKiosk.LogonProxy.UpdateUtilityRequest utility = null;
                List<MBMEKiosk.LogonProxy.UpdateUtilityRequest> utilities = new List<LogonProxy.UpdateUtilityRequest>();
                LogonServiceClient logonproxy = null;
                try
                {
                    string relativePath = ConfigurationManager.AppSettings["ModulesPath"] != null ? ConfigurationManager.AppSettings["ModulesPath"] : "modules";
                    String[] astrLoadableFiles = Directory.GetFiles(relativePath, "*.dll");
                    ValidateCertificate.RegisterCallback();
                    logonproxy = new LogonServiceClient();

                    Assembly assembly = null;
                    foreach (String strFile in astrLoadableFiles)
                    {                         
                        try
                        {
#if DEBUG
                    assembly = Assembly.LoadFrom(strFile);
#else
                            try
                            {
                                utility = new LogonProxy.UpdateUtilityRequest();                              
                                assembly = Assembly.LoadFrom(strFile);                              

                                Version dllVersion = assembly.GetName().Version;
                                utility.Version = dllVersion.ToString();
                                utility.UtilityName = assembly.ManifestModule.Name;
                                utility.DateModified = new System.IO.FileInfo(strFile).LastWriteTime;
                                utility.Size = new System.IO.FileInfo(strFile).Length;
                                utilities.Add(utility);
                            }
                            catch (IOException ioex)
                            {
                                Trace.TraceError("Warning! Error loading file LoadUtilities \r\n\r\n{0}\r\n\r\n{1}\r\n\r\n{2}", ioex.Message, ioex.Source, ioex.StackTrace);
                                if (log.IsErrorEnabled) log.ErrorFormat(" Warning! Error loading file in LoadUtilities  {0} {1}", DateTime.Now.ToString(), ioex.Message);                                
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError("Error! Error loading file.\r\n\r\n{0}\r\n\r\n{1}\r\n\r\n{2}", ex.Message, ex.Source, ex.StackTrace);
                                if (log.IsErrorEnabled) log.ErrorFormat("Error! Error loading file in LoadUtilities  {0} {1}", DateTime.Now.ToString(), ex.Message);                               
                            }
#endif
                        }
                        catch (BadImageFormatException ex)
                        {
                            // Assembly was not a valid assembly or it targets a later version of .NET framework than is loaded.
                            if (log.IsErrorEnabled) log.ErrorFormat("Caught BadImageFormatException in LoadUtilities  {0} {1}", DateTime.Now.ToString(), ex.Message);                               
                            continue;
                        }
                    }                     

                    if(File.Exists(CMEXEPATH))
                    {
                        assembly = Assembly.LoadFrom(CMEXEPATH);

                        utility = new LogonProxy.UpdateUtilityRequest();
                        Version dllVersion = assembly.GetName().Version;
                        utility.Version = dllVersion.ToString();
                        utility.UtilityName = assembly.ManifestModule.Name;
                        utility.DateModified = new System.IO.FileInfo(CMEXEPATH).LastWriteTime;
                        utility.Size = new System.IO.FileInfo(CMEXEPATH).Length;
                        utilities.Add(utility);
                    }
                    else
                        if (log.IsInfoEnabled) log.InfoFormat("MBMEContentManager.exe not found in LoadUtilities  {0}", DateTime.Now.ToString());             
                  
                    if(File.Exists(KIOSKEXEPATH))
                    {
                        assembly = Assembly.LoadFrom(KIOSKEXEPATH);

                        utility = new LogonProxy.UpdateUtilityRequest();
                        Version dllVersion = assembly.GetName().Version;
                        utility.Version = dllVersion.ToString();
                        utility.UtilityName = assembly.ManifestModule.Name;
                        utility.DateModified = new System.IO.FileInfo(KIOSKEXEPATH).LastWriteTime;
                        utility.Size = new System.IO.FileInfo(KIOSKEXEPATH).Length;
                        utilities.Add(utility);
                    }
                    else
                        if (log.IsInfoEnabled) log.InfoFormat("MBMEKiosk.exe not found in LoadUtilities  {0}", DateTime.Now.ToString());  

                    if(File.Exists(WATCHDOGEXEPATH))
                    {
                        assembly = Assembly.LoadFrom(WATCHDOGEXEPATH);

                        utility = new LogonProxy.UpdateUtilityRequest();
                        Version dllVersion = assembly.GetName().Version;
                        utility.Version = dllVersion.ToString();
                        utility.UtilityName = assembly.ManifestModule.Name;
                        utility.DateModified = new System.IO.FileInfo(WATCHDOGEXEPATH).LastWriteTime;
                        utility.Size = new System.IO.FileInfo(WATCHDOGEXEPATH).Length;
                        utilities.Add(utility);
                    }
                    else
                        if (log.IsInfoEnabled) log.InfoFormat("MBMEWatchDog.exe not found in LoadUtilities  {0}", DateTime.Now.ToString());  
                   

                    if (logonproxy != null && utilities.Count() > 0)
                        logonproxy.LogOnUpdateUtilityAsync(Int32.Parse(KioskAppConfig.KioskId), utilities.ToArray());
                
                }
                catch (Exception ex)
                {                    
                    if (logonproxy != null)
                        logonproxy.Abort();
                    if (log.IsErrorEnabled) log.ErrorFormat("LoadUtilities  unsuccessfull: {0} {1}", DateTime.Now.ToString(), ex.Message);
                }
                finally
                {                     
                    if (logonproxy != null)
                        logonproxy.Close();
                    ValidateCertificate.DeregisterCallback();
                }     
        }

        /// <summary>
        /// Invoke this method on initialization (UI/Cash device) 
        /// to initialize or reset the connectivity timer respectively.
        /// Uses the configured time:
        /// 1. Connectivity time for checking backend connectivity.
        /// </summary>
        private void RestartConnectivityTimer()
        {
            if (connectivityTimer != null)
            {
                connectivityTimer.Stop();
            }
            else
            {
                connectivityTimer = new DispatcherTimer();
                connectivityTimer.Tick += OnConnectivityTimeOut;
            }
            connectivityTimer.Interval = new TimeSpan(0, 0, Int32.Parse(ConfigurationManager.AppSettings["ConnectivityStatusTimer"]));
            connectivityTimer.Start();
        }

        /// <summary>
        /// Invoke this method on initialization (UI/Cash device) 
        /// to initialize or reset the logon timer respectively.
        /// Uses the configured time:
        /// 1. Timer for checking whether Kiosk is logged on to the Platform.
        /// </summary>
        private void RestartLogonTimer()
        {
            if (logonTimer != null)
            {
                logonTimer.Stop();
            }
            else
            {
                logonTimer = new DispatcherTimer();
                logonTimer.Tick += OnLogonTimeOut;
            }
            logonTimer.Interval = new TimeSpan(0, 0, Int32.Parse(ConfigurationManager.AppSettings["LogonTimer"]));
            logonTimer.Start();
        }

        /// <summary>
        /// Invoke this method on initialization (UI/Cash device) 
        /// to initialize or reset the shutdown timer respectively.
        /// Uses the configured time:
        /// 1. Connectivity time for checking customer activity.
        /// </summary>
        private void RestartShutdownTimer()
        {
            if (shutdownTimer != null)
            {
                shutdownTimer.Stop();
            }
            else
            {
                shutdownTimer = new DispatcherTimer();
                shutdownTimer.Tick += OnShutdownTimeOut;
            }
            shutdownTimer.Interval = new TimeSpan(0, 0, Int32.Parse(ConfigurationManager.AppSettings["ShutdownTimer"]));
            shutdownTimer.Start();
        }

        public void OnLogonTimeOut(object o, EventArgs args)
        {
            if (log.IsInfoEnabled) log.Info("Inside OnLogonTimeOut..");
            if (logonTimer != null)
            {
                logonTimer.Stop();
                logonTimer.Tick -= OnLogonTimeOut;
                logonTimer = null;
                if (log.IsInfoEnabled) log.Info("Logon timeout callback deregistered.");
            }
            Logon();

        }

        public void OnShutdownTimeOut(object o, EventArgs args)
        {
            // Shutdown the App as we have monitored no customer activity 
            // for the defined wait period as indicated by ShutdownTimer period.
            if (log.IsInfoEnabled) log.Info("Inside OnShutdownTimeOut..");
            if (shutdownTimer != null)
            {
                shutdownTimer.Stop();
                shutdownTimer.Tick -= OnShutdownTimeOut;
                shutdownTimer = null;
                if (log.IsInfoEnabled) log.Info("Shutdown timeout callback deregistered.");
            }

            if (shutdownCommandActive)
            {
                if (command.Command == "SHUTDOWN")
                {
                    if (log.IsInfoEnabled) log.Info("Shutdown Command Active.");
                    //DBLogger.AddQueueStatus(command.QueueId, true);
                    int result = LogCommandStatusToLocalDb();
                    if (log.IsInfoEnabled) log.Info("result:" + result.ToString());
                    if (result > 0)
                    {
                        if (log.IsInfoEnabled) log.Info("before Executing Shutdown for commandParameter:" + command.CommandParameter);
                        Application.Current.Shutdown();
                        shutdownCommandActive = false;
                        if (log.IsInfoEnabled) log.Info("after Executing Shutdown");
                    }

                    lastPackageDet = null;
                }
                if (command.Command == "ONDEMAND-SHUTDOWN")
                {
                    if (log.IsInfoEnabled) log.Info("before Executing OnDemand Shutdown ");
                    shutdownCommandActive = false;
                    Application.Current.Shutdown();
                    
                    if (log.IsInfoEnabled) log.Info("after Executing OnDemand Shutdown");
                    try
                    {
                        
                        if (command.CommandParameter == "sysreboot")
                        {
                            if (log.IsInfoEnabled) log.Info("Executing System Restart ");
                            ProcessStartInfo startinfo = new ProcessStartInfo("shutdown.exe", "-r -f");
                            Process.Start(startinfo);
                            startinfo.CreateNoWindow = true;
                            startinfo.UseShellExecute = false;
                            startinfo.WindowStyle = ProcessWindowStyle.Hidden;
                            if (log.IsInfoEnabled) log.Info("Executed System Restart "); 
                        }
                        

                        int result = monitorClient.UpdatePackageCommandStatus(new CommandRequest
                        {
                            KioskId = Convert.ToInt32(KioskAppConfig.KioskId),
                            QueueId = command.QueueId,
                            CommandExecuted = true,
                            ExecutedDate = DateTime.Now,
                            IssuedDate = DateTime.Now
                        });

                        if (result > 0)
                            if (log.IsInfoEnabled) log.InfoFormat("Update to Portal db Successfull for {0}",command.Command);
                    }
                    catch (Exception ex)
                    {
                        if (log.IsErrorEnabled) log.ErrorFormat("OnDemand Shutdown : {0}", ex.Message);
                    }

                    RestartShutdownTimer();

                }
            }
        }

        protected void OnConnectivityTimeOut(object o, EventArgs args)
        {
            // Do something here.
            // Execute the loop for all modules present in the modulescatalog of the module manager.
            if(adminkey == "1234")
                adminkey = string.Empty;
            try
            {
                if (connectivityTimer != null)
                {
                    connectivityTimer.Stop();
                    connectivityTimer.Tick -= OnConnectivityTimeOut;
                    connectivityTimer = null;
                }

                short printerState = -1;
                short printerStatus = -1;
                int CAState = -1;
                int CAStatus = -1;

                objPrinter.Description = this.Devices.GetPrinter().GetDetails(out printerState, out printerStatus);
                objCashAcceptor.Description = this.Devices.GetCashAcceptor().GetDetails(out CAState, out CAStatus);

                if (((objPrinter.State != (DeviceState)printerState) ||
                    (objPrinter.Status != (DeviceStatus)printerStatus) ||
                    (objCashAcceptor.State != (DeviceState)CAState) ||
                    (objCashAcceptor.Status != (DeviceStatus)CAStatus)) || (notPolled == pollcounter))
                {
                    notPolled = 0;
                    if (isMonitored)
                    {
                        objPrinter.State = (DeviceState)printerState;
                        objPrinter.Status = (DeviceStatus)printerStatus;


                        objCashAcceptor.State = (DeviceState)CAState;
                        objCashAcceptor.Status = (DeviceStatus)CAStatus;

                        objKiosk.State = DeviceState.OnLine;
                        if ((objPrinter.Status == DeviceStatus.Error) || (objCashAcceptor.Status == DeviceStatus.Error))
                        {
                            objKiosk.Status = DeviceStatus.Error;
                            objKiosk.Description = "Error.";
                        }
                        else
                        {
                            if ((objPrinter.Status == DeviceStatus.Warning) || (objCashAcceptor.Status == DeviceStatus.Warning))
                            {
                                objKiosk.Status = DeviceStatus.Warning;
                                objKiosk.Description = "Warning.";
                            }
                            else
                            {
                                objKiosk.Status = DeviceStatus.NoError;
                                objKiosk.Description = "No Error.";
                            }
                        }

                        deviceList.Add(objPrinter);
                        deviceList.Add(objCashAcceptor);
                        deviceList.Add(objKiosk);
                    }
                    try
                    {
                        ValidateCertificate.RegisterCallback();
                        //using (MBMEMonitoringServiceClient monitorClient = new MBMEMonitoringServiceClient())
                        //{
                        if (log.IsInfoEnabled) log.Info(string.Format("{0} : HealthCheck Service Call Initiated", DateTime.Now.ToString()));
                        if (log.IsInfoEnabled) log.Info("Inside OnConnectivityTimeout. HealthCheck Service Call Initiated..");
                        monitorClient.CheckBackendConnectivityAsync(new HealthCheckRequest
                        {
                            //KioskId = int.Parse(ConfigurationManager.AppSettings["MachineId"].ToString()),
                            KioskId = int.Parse(KioskAppConfig.KioskId),
                            StatusChanged = DateTime.Now,
                            DeviceList = deviceList.ToArray()
                        });
                        if (log.IsInfoEnabled) log.Info("HealthCheck Service Call Executed..");
                        //}

                    }
                    catch (Exception ex)
                    {

                        IsBackendServiceConnected = false;
                        if (log.IsErrorEnabled) log.Error(string.Format("{0} : HealthCheck Service Call InitiationException :", DateTime.Now.ToString()) + ex.Message + "- InnerException :" + ex.InnerException.ToString());// + " InnerException : " + ex.InnerException.ToString());
                    }
                    finally
                    {
                        if (log.IsInfoEnabled) log.Info("Printer State : " + objPrinter.State + " -- Printer Status : " + objPrinter.Status + " -- Printer Status Desc : " + objPrinter.Description);
                        if (log.IsInfoEnabled) log.Info("Cash Acceptor State : " + objCashAcceptor.State + " -- Cash Acceptor Status : " + objCashAcceptor.Status + " -- Cash Acceptor Status Desc : " + objCashAcceptor.Description);
                        if (log.IsInfoEnabled) log.Info("Kiosk State : " + objKiosk.State + " -- Kiosk Status : " + objKiosk.Status + " -- Kiosk Status Desc : " + objKiosk.Description);
                        ValidateCertificate.DeregisterCallback();
                        deviceList.Clear();
                    }
                }
                else
                {
                    notPolled++;
                    //if (isMonitored)
                    RestartConnectivityTimer();
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.ErrorFormat("OnConnectivity Timeout Error : {0} ", ex.Message);

            }
        }


        public int LogCommandStatusToLocalDb()
        {

            if (log.IsInfoEnabled) log.Info("LogCommandStatusToLocalDbstarted..");

            string[] commandParameter; int packageId = 0; int result = 0;
            commandParameter = command.CommandParameter.Split('=');
            packageId = Convert.ToInt32(commandParameter[1].Replace(";", ""));


            try
            {
                result = Logger.LogPackageStatus(new PackageMasterInfo
                {
                    QueueId = command.QueueId,
                    PackageId = packageId,
                    ExecutionDateTime = DateTime.Now,
                    CommandExecuted = true,


                });

                if (log.IsInfoEnabled) log.Info("LogCommandStatusToLocalDb ended..and result is:" + result.ToString());

                return result;
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Exception thrown in LogCommandStatusToLocalDb" + ex.Message);
                return 0;
            }

        }

        /// <summary>
        /// Invoke this method on Browser initialization 
        /// to initialize or reset the AppTermination timer respectively.
        /// Uses the configured time:
        /// 1. Timer for terminating the browser launched previously by the Platform.
        /// </summary>
        private void RestartAppTerminationTimer()
        {
            if (logonTimer != null)
            {
                AppTerminationTimer.Stop();
            }
            else
            {
                AppTerminationTimer = new DispatcherTimer();
                AppTerminationTimer.Tick += OnAppTerminationTimeOut;
            }
            AppTerminationTimer.Interval = new TimeSpan(0, 0, Int32.Parse(ConfigurationManager.AppSettings["AppTerminationTimer"]));
            AppTerminationTimer.Start();
        }


        protected void OnAppTerminationTimeOut(object o, EventArgs args)
        {
            if (AppTerminationTimer != null)
            {
                AppTerminationTimer.Stop();
                AppTerminationTimer.Tick -= OnAppTerminationTimeOut;
                AppTerminationTimer = null;
            }
            Process[] processes = System.Diagnostics.Process.GetProcessesByName("iexplore");
            // if the process is still alive Kill the process.
            if (processes.Length != 0)
            {
                short counter = 0;
                for (counter = 0; counter < processes.Length; counter++)
                {
                    if (!processes[counter].HasExited)
                        processes[counter].Kill();
                }
            }
        }




        #endregion

        public bool ShowOtherApps { get; set; }
    }
}

