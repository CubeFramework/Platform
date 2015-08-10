using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using MBMEKiosk.Infrastructure.Interfaces;
using System.Windows.Threading;
using MBMEKiosk.Infrastructure.BaseClasses;
using MBMEKiosk.Infrastructure.ObjectModel;
using MBMEKioskLogger.Logger;
using MBMEKioskLogger.LoggerClass;
using System.Configuration;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace MBMEDevices.CashDevices
{
    /// <summary>
    /// Callback delegate for cash acceptor driver.
    /// </summary>
    internal delegate void BVUEventCallback(IntPtr testPtr, int iEvent);

    internal delegate void DInit();
    internal delegate void DEnable(List<int> denominations, double amount, double minAmount, double maxAmount, bool isAmountFixed);
    internal delegate void DDisable();

    public class ArdacElite : ICashAcceptor
    {
        // Cash Acceptor operational status flags and constructs.
        private static bool isInitialized;
        private static bool isEnabled;
        private static BVUControl lastBVUControl;

        // Cash cycle related fields.
        private static bool dynamicallyUpdateAllowedDenominations;
        private static double expectedCashAmount;
        private static double amountStackedinCurrentCashCycle;
        private static double minAmount;
        private static double maxAmount;
        private static List<int> expectedDenominations;
        private static List<int> allowedDenominations;

        // Cash Acceptor Simulation related fields.
        private static bool simulateDevice;
        private static Timer simulationTimer;

        // Cash Acceptor State/Status/Status Desc...
        private int stateCode;
        private int statusCode;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Async method call delegates.
        DInit initMethod;
        DEnable enableMethod;
        DDisable disableMethod;

        /// <summary>
        /// Callback method declaration for cash acceptor driver.
        /// </summary>
        internal BVUEventCallback cashAcceptorCallback;

        /// <summary>
        /// Event to publish the action of cash being inserted by the user.
        /// </summary>
        ////public event Action<bool> CashInsertedEvent;
        public event Action<string> CashCycleInitiatedEvent;

        /// <summary>
        /// Event to publish the action of cash being inserted by the user.
        /// </summary>
        ////public event Action<bool> CashInsertedEvent;
        public event Action<int, bool> CashInsertedEvent;

        /// <summary>
        /// Event to publish the note stacked and the allowed denominations details.
        /// </summary>
        public event Action<int, string> NoteStackedEvent;

        /// <summary>
        /// Event to publish the note returned.
        /// </summary>

        public event Action<int> NoteReturnedEvent;

        /// <summary>
        /// Event to publish the completion of cash cycle and disabling of cash acceptor.
        /// </summary>
        ////public event Action CashCycleCompletedEvent;

        /// <summary>
        /// Cash acceptor data structure model.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class BVUControl
        {
            [MarshalAs(UnmanagedType.I4)]
            public int iDebugMode;
            ////[MarshalAs(UnmanagedType.I4)]
            ////public int iSerialPort;
            [MarshalAs(UnmanagedType.I4)]
            public int iEventType;
            [MarshalAs(UnmanagedType.I4)]
            public int iDenominations;
            [MarshalAs(UnmanagedType.I4)]
            public int iDirection;
            [MarshalAs(UnmanagedType.I4)]
            public int iBarCodeLen;
            [MarshalAs(UnmanagedType.I4)]
            public int iBillValue;
            [MarshalAs(UnmanagedType.I4)]
            public int iMaxNoResponseInterval;
            //[MarshalAs(UnmanagedType.SafeArray)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public byte[] cInfo;
        }

        #region Wrappers for the cash acceptor APIs.

        //, EntryPoint = "PollingThreadCommand", SetLastError = false, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall
        //[System.Runtime.InteropServices.DllImport(@"CashDevices\ArdacElite\drivers\Ardac-Elite_V1_0.dll")]
        [System.Runtime.InteropServices.DllImport(@"Ardac-Elite_V1_0.dll")]
        internal static extern int PollingThreadCommand([MarshalAs(UnmanagedType.I2)]short command);

        [System.Runtime.InteropServices.DllImport(@"Ardac-Elite_V1_0.dll", EntryPoint = "BVUOpen", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        internal static extern int BVUOpen(IntPtr ptrBVUControl, BVUEventCallback eventHandler);

        [System.Runtime.InteropServices.DllImport(@"Ardac-Elite_V1_0.dll")]
        internal static extern int GetStatus();

        [System.Runtime.InteropServices.DllImport(@"Ardac-Elite_V1_0.dll")]
        internal static extern short GetFaultCode();

        #endregion

        #region Methods to interact with the cash acceptor device driver from external applications
        /// <summary>
        /// Initializes the cash acceptor
        /// </summary>
        public void InitAsync(bool simulateCashAcceptor)
        {
            if (!isInitialized)
            {
                isInitialized = true;
                simulateDevice = simulateCashAcceptor;

                // Initialize delegate methods only once in application life cycle.
                initMethod = new DInit(this.Init);
                enableMethod = new DEnable(this.Enable);
                disableMethod = new DDisable(this.Disable);

                // Initialize the cash acceptor.
                initMethod.BeginInvoke(null, null);
            }
        }

        private void Init()
        {
            // Initialize/Reset the cash device status related data.
            isEnabled = false;
            if (allowedDenominations == null)
            {
                allowedDenominations = new List<int>();
            }

            if (expectedDenominations == null)
            {
                expectedDenominations = new List<int>();
            }

            if (simulateDevice)
            {
                simulationTimer = new Timer(new TimerCallback(SimulateNoteStacked), null, Timeout.Infinite, Timeout.Infinite);
                using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- Ardac Elite cash device initialized in simulation mode.", DateTime.Now));
                    writer.Flush();
                    writer.Close();
                }
            }
            else
            {
                try
                {
                    BVUControl objControl = new BVUControl();
                    objControl.iDenominations = 0;
                    objControl.iDebugMode = 2;
                    objControl.cInfo = new byte[100];

                    // Obtain the size of an unmanaged serialized
                    // representation of a TestStruct struct.
                    int iSize = Marshal.SizeOf(typeof(BVUControl));
                    IntPtr pBVUControl = Marshal.AllocHGlobal(iSize);

                    // Copy the full contepBVUControlnts of the members of test
                    // to the unmanaged representation of BVU_Control.
                    // Data from the members of test will be
                    // serialized into a flat memory format and stored
                    // into the unmanaged serialized representation.
                    Marshal.StructureToPtr(objControl, pBVUControl, false);
                    cashAcceptorCallback = BVUEventHandler;
                    int success = BVUOpen(pBVUControl, cashAcceptorCallback);

                    using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        if (success == 1)
                        {
                            writer.WriteLine(string.Format(@"Datetime {0} --  Ardac Elite cash device initialized succeessfully.", DateTime.Now));
                        }
                        else
                        {
                            writer.WriteLine(string.Format(@"Datetime {0} --  Ardac Elite cash device initialization failed. Error Code: {1}.", DateTime.Now, success));
                        }

                        writer.Flush();
                        writer.Close();
                    }

                    DisableAsync();
                }
                catch (Exception e)
                {
                    using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Init: Exception occured.\nMSG:{1} \ Source={2} \nStack Trace:{3}.", DateTime.Now, e.Message, e.Source, e.StackTrace));
                        writer.Flush();
                        writer.Close();
                    }
                }
            }
        }

        // Added By JK on 19/03/13 to take care of the last note stack delay issue and single note escrow feature.
        // Currently Kept commented as it is not required for now.
        //public bool IssueHold()
        //{
        //    Hold();
        //    return true;
        //}

        // Added By JK on 19/03/13 to take care of the last note stack delay issue and single note escrow feature.
        public bool IssueStack()
        {
            Stack();
            return true;
        }

        // Added By JK on 19/03/13 to take care of the last note stack delay issue and single note escrow feature.
        public bool IssueReturn()
        {
            Return();
            return true;
        }

        // Added By JK on 19/03/13 to take care of the last note stack delay issue and single note escrow feature.
        // Currently Kept commented as it is not required for now.
        //private bool Hold()
        //{
        //    int success = 0;
        //    if (isEnabled)
        //    {
        //        if (simulateDevice)
        //        {
        //            success = 1;
        //        }
        //        else
        //        {
        //            try
        //            {
        //                success = SendCommand(CashAcceptorCommand.Hold);
        //            }
        //            catch (Exception e)
        //            {
        //                if (log.IsInfoEnabled) log.Info("Hold: Exception occured.\nMSG: " + e.Message + " Source= " + e.Source + " \nStack Trace: " + e.StackTrace);
        //            }
        //        }

        //        if (success == 1)
        //        {
        //            if (log.IsInfoEnabled) log.Info("Hold request successful");
        //        }
        //        else
        //        {
        //            if (log.IsInfoEnabled) log.Info("Hold request failed. Error Code: " + success);
        //        }
        //    }

        //    return success == 1;
        //}

        private bool Stack()
        {
            int success = 0;
            if (isEnabled)
            {
                if (simulateDevice)
                {
                    success = 1;
                }
                else
                {
                    try
                    {
                        success = SendCommand(CashAcceptorCommand.Stack);
                    }
                    catch (Exception e)
                    {
                        using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- Stack: Exception occured.\nMSG:{1} \ Source={2} \nStack Trace:{3}.", DateTime.Now, e.Message, e.Source, e.StackTrace));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                }

                using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    if (success == 1)
                    {
                        writer.WriteLine(string.Format(@"Datetime {0} --  Stack request successful", DateTime.Now));
                    }
                    else
                    {
                        writer.WriteLine(string.Format(@"Datetime {0} --  Stack request failed. Error Code: {1}.", DateTime.Now, success));
                    }

                    writer.Flush();
                    writer.Close();
                }
            }

            return success == 1;
        }

        private void Return()
        {
            int success = 0;
            if (simulateDevice)
            {
                success = 1;
            }
            else
            {
                try
                {
                    success = SendCommand(CashAcceptorCommand.Return);
                }
                catch (Exception e)
                {
                    using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Return: Exception occured.\nMSG:{1} \ Source={2} \nStack Trace:{3}.", DateTime.Now, e.Message, e.Source, e.StackTrace));
                        writer.Flush();
                        writer.Close();
                    }
                }

            }

            using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                if (success == 1)
                {
                    writer.WriteLine(string.Format(@"Datetime {0} -- Return request successful", DateTime.Now));
                }
                else
                {
                    writer.WriteLine(string.Format(@"Datetime {0} -- Return request failed. Error Code: {1}.", DateTime.Now, success));
                }

                writer.Flush();
                writer.Close();
            }
        }

        public void DisableAsync()
        {
            isEnabled = false;

            // Added By JK on 05/02/2014 because the counters need to be reset at the time of Disabling rather than Enabling
            // as in some cases like Card Payment or FEWA Reconnection where Cash is not involved, the library tends to provide
            // the previous transactions Cash stacked counter.
            dynamicallyUpdateAllowedDenominations = false;
            expectedCashAmount = 0;
            //amountStackedinCurrentCashCycle = 0;
            expectedDenominations.Clear();
            allowedDenominations.Clear();
            ArdacElite.minAmount = 0;
            ArdacElite.maxAmount = 0;

            if (disableMethod != null)
            {
                disableMethod.BeginInvoke(null, null);
            }
        }

        private void Disable()
        {
            using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(string.Format(@"Datetime {0} -- Disabling cash device...", DateTime.Now));

                writer.Flush();
                writer.Close();
            }

            int success = 0;
            if (simulateDevice)
            {
                success = 1;
            }
            else
            {
                try
                {
                    success = SendCommand(CashAcceptorCommand.Disable);
                }
                catch (Exception e)
                {
                    using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Disable: Exception occured.\nMSG:{1} \ Source={2} \nStack Trace:{3}.", DateTime.Now, e.Message, e.Source, e.StackTrace));
                        writer.Flush();
                        writer.Close();
                    }
                }
            }

            using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                if (success == 1)
                {
                    writer.WriteLine(string.Format(@"Datetime {0} -- Cash device disabled successfully", DateTime.Now));
                }
                else
                {
                    writer.WriteLine(@"Datetime {0} -- Cash device disable request failed. Error code: {1}", DateTime.Now, success);
                }

                writer.Flush();
                writer.Close();
            }
        }

        public bool IsEnabled()
        {
            // KS TODO : Check if additional flags are needed to keep track of the requests currently in progress 
            // with the cash device driver.
            return isEnabled;
        }

        public string GetDetails(out int state, out int status)
        {
            string statusDesc = string.Empty;
            IsReady();
            state = this.stateCode;
            if (state == (int)CashAcceptorState.OFFLINE)
                statusDesc = "Offline - ";
            else if (state == (int)CashAcceptorState.ONLINE)
                statusDesc = "Online - ";
            else
                statusDesc = "Undefined State - ";
            status = this.statusCode;

            switch (status)
            {
                case (int)CashAcceptorStatus.Initializing:
                    statusDesc += "Cash Acceptor Initialization Sequence happening.";
                    break;
                case (int)CashAcceptorStatus.NoError:
                    statusDesc += "No Error.";
                    break;
                case (int)CashAcceptorStatus.Warning:
                    statusDesc += "Warning.";
                    break;
                case (int)CashAcceptorStatus.Error:
                    if (StatusMessage == null)
                        statusDesc += ArdacEliteFaultCodes.GetFaultDesc(GetFaultCode());
                    else
                        statusDesc += StatusMessage;
                    break;
                default:
                    statusDesc += "Unknown Status.";
                    break;
            }

            return statusDesc;

        }

        public string StatusMessage { get; set; }

        public bool IsReady()
        {
            int devStatus = -1;

            stateCode = (int)CashAcceptorState.OFFLINE;
            statusCode = (int)CashAcceptorStatus.Error;
            StatusMessage = null;

            if (!simulateDevice)
            {
                try
                {
                    devStatus = GetStatus();
                    if (devStatus != -1)
                    {
                        stateCode = (devStatus >> 16) & 0xFFFF;
                        statusCode = devStatus & 0xFFFF;
                        // check for state == Online and status = NOERROR
                        if ((stateCode == (long)CashAcceptorState.ONLINE) && (statusCode == (long)CashAcceptorStatus.NoError))
                            return true;

                    }

                }
                catch (Exception ex)
                {
                    this.StatusMessage = ex.Message;
                    using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"{0} - Cash Device Error while doing Get Status: {1} - .", DateTime.Now, ex.Message));
                        writer.Flush();
                        writer.Close();
                    }
                }
            }
            else
            {
                statusCode = 1;
                stateCode = 1;
                return true;
            }

            return false;
            //return statusCode;
            // KS TODO : Log status.
            //return statusCode == (short)CashAcceptorStatus.NoError;
        }

        public double GetAmountStackedInLastCycle()
        {
            return amountStackedinCurrentCashCycle;
        }

        public void EnableAsync(List<int> denominations, double amount, double minAmount, double maxAmount, bool isAmountFixed)
        {
            if (isInitialized && !isEnabled)
            {
                isEnabled = true;

                // Reset all the cash cycle related values.
                dynamicallyUpdateAllowedDenominations = false;
                expectedCashAmount = 0;
                amountStackedinCurrentCashCycle = 0;
                expectedDenominations.Clear();
                allowedDenominations.Clear();
                ArdacElite.minAmount = 0;
                ArdacElite.maxAmount = 0;
                if (enableMethod != null)
                {
                    enableMethod.BeginInvoke(denominations, amount, minAmount, maxAmount, isAmountFixed, null, null);
                }
            }
        }

        private void Enable(List<int> denominations, double amount, double minAmount, double maxAmount, bool isAmountFixed)
        {
            using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                if (denominations != null)
                {
                    writer.WriteLine(string.Format(@"Datetime {0} -- Enabling cash device... Denominations Count = {1}; Allowed denominations : {2}; Expected Amount = {3}; Is amount fixed = {4}.", DateTime.Now, denominations.Count, string.Join<int>(", ", denominations), amount, isAmountFixed));
                }
                else
                {
                    writer.WriteLine(string.Format(@"Datetime {0} -- Enabling cash device... Denominations Count = None/Default; Expected Amount = {1}; Is amount fixed = {2}.", DateTime.Now, amount, isAmountFixed));
                }

                writer.Flush();
                writer.Close();
            }

            int success = 0;
            if (simulateDevice)
            {
                success = 1;
            }
            else
            {
                try
                {
                    success = SendCommand(CashAcceptorCommand.Enable);
                }
                catch (Exception e)
                {
                    using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Enable: Exception occured.\nMSG:{1} \ Source={2} \nStack Trace:{3}.", DateTime.Now, e.Message, e.Source, e.StackTrace));
                        writer.Flush();
                        writer.Close();
                    }
                }
            }

            using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                if (success == 1)
                {
                    writer.WriteLine(string.Format(@"Datetime {0} -- Cash Device enabled successfully", DateTime.Now));
                }
                else
                {
                    writer.WriteLine(string.Format(@"Datetime {0} -- Cash Device enable failed. Error code: {1}.", DateTime.Now, success));
                    isEnabled = false;
                }


                writer.Flush();
                writer.Close();
            }

            // If the cash acceptor is enabled successfully, initialize the expected and allowed denominations
            // as per the need to dynamically update the allowed denominations.
            if (isEnabled)
            {
                expectedCashAmount = amount;
                dynamicallyUpdateAllowedDenominations = isAmountFixed;


                expectedDenominations.Clear();
                if (denominations != null && denominations.Count > 0)
                {
                    foreach (int item in denominations)
                    {
                        expectedDenominations.Add(item);
                    }
                }
                else
                {
                    // KS TODO: Check if this range needs to be read from config.
                    // Add default 
                    expectedDenominations.AddRange(new List<int> { 10, 20, 50, 100, 200, 500, 1000 });
                }

                ArdacElite.minAmount = minAmount;
                ArdacElite.maxAmount = maxAmount;
                UpdateAllowedDenominations(minAmount, maxAmount, true);

                // Notify the application of the allowed denominations at the start of the Cash Cycle.
                Action<string> handler = CashCycleInitiatedEvent;
                if (handler != null)
                {
                    handler(GetConcatenatedAllowedDenominations());
                }

                if (simulateDevice)
                {
                    simulationTimer.Change(5000, 5000);
                }
            }
        }

        #endregion

        #region Helper methods and device event handlers.

        private int SendCommand(CashAcceptorCommand cmd)
        {
            int response = 0;
            if (IsReady())
            {
                response = PollingThreadCommand((short)cmd);
            }
            else
            {
                // KS TODO: Device not ready.
            }

            return response;
        }

        private void BVUEventHandler(IntPtr testPtr, int iEvent)
        {
            if (log.IsInfoEnabled) log.Info("BVUEventHandler started.iEvent:" + iEvent.ToString() + ",testPtr:" + testPtr.ToString());

            lastBVUControl = (BVUControl)(Marshal.PtrToStructure(testPtr, typeof(BVUControl)));

            using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(string.Format(@"Datetime {0} -- Device message : Event id = {1}, bill value = {2}.", DateTime.Now, iEvent, lastBVUControl.iBillValue));
                writer.Flush();
                writer.Close();
            }

            switch (iEvent)
            {

                case (int)CashAcceptorEventType.RETURNED:

                    OnReturned(lastBVUControl.iBillValue, Encoding.UTF8.GetString(lastBVUControl.cInfo).TrimEnd('\0'));

                    break;

                case (int)CashAcceptorEventType.ESCROW:
                    OnEscrow(lastBVUControl.iBillValue);
                    break;
                case (int)CashAcceptorEventType.STACKED:

                    OnStacked(lastBVUControl.iBillValue, Encoding.UTF8.GetString(lastBVUControl.cInfo).TrimEnd('\0'));
                    // Abhijeet to include the update for tblKioskSummary and tblKioskCashDetails
                    // Increment NumCashTxns.
                    // Increment TotalCash.
                    // Increment Denomination total.

                    break;

                default:
                    break;
            }
            if (log.IsInfoEnabled) log.Info("Inside BVUEventHandler and before LogEventToLocalDb() call");
            this.LogEventToLocalDb(iEvent.ToString());
        }

        private void OnEscrow(int denomination)
        {
            if (!isEnabled)
            {
                Return();
                using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Unexpected device status. Device expected to be disabled. Returning denomination available on escrow: {0}.", denomination));
                    writer.Flush();
                    writer.Close();
                }

                return;
            }

            // Notify the application of a note being available on escrow.
            // The notify app logic has been moved down below the IsExpectedDenomination check in order
            // to make sure the fact that the driver informs the App only
            Action<int, bool> handler = CashInsertedEvent;

            if (!IsExpectedDenomination(denomination))
            {
                Return();
                // Modified By JK to make sure the App is informed about a note in Escrow even though
                // it is not of an allowed denomination.
                if (handler != null)
                {
                    handler(denomination, false);
                }
                using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Returning unexpected denomination available on escrow: {0}.", denomination));
                    writer.Flush();
                    writer.Close();
                }

                return;
            }


            using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(string.Format(@"Expected denomination available on escrow: {0}.", denomination));
                writer.Flush();
                writer.Close();
            }

            // Notify the application of a allowed denomination note being available on escrow.
            bool isAppInformed = false;
            if (handler != null)
            {
                handler(denomination, true);
                isAppInformed = true;
            }

            // Stack if the application in correct state else return cash.
            if (!isAppInformed)
            {
                Return();
                using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Application not in a state to accept cash. Returning denomination available on escrow: {0}.", denomination));
                    writer.Flush();
                    writer.Close();
                }

                ////DisableAsync();
                return;
            }

            //Stack();
        }

        private void OnStacked(int denomination, string currency)
        {
            try
            {
                using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Stacked: denomination : {0}.", denomination));
                    writer.Flush();
                    writer.Close();
                }
                if (log.IsInfoEnabled) log.Info("Stacked: denomination : " + denomination.ToString());

                amountStackedinCurrentCashCycle += denomination;

                // Update the list of allowed denominations if configured.
                if (dynamicallyUpdateAllowedDenominations)
                {
                    UpdateAllowedDenominations(ArdacElite.minAmount, ArdacElite.maxAmount, false);
                }

                // Notify the application of the amount stacked and the currently allowed denominations.
                Action<int, string> handler = NoteStackedEvent;
                if (handler != null)
                {
                    handler(denomination, GetConcatenatedAllowedDenominations());
                }
                else
                {
                    // The application not in state to accept more cash after the denomination is returned, so disable the cash device.
                    if (log.IsInfoEnabled) log.Info("Application not in a state to accept more cash. Stacked Denomination: " + denomination.ToString());
                    using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Application not in a state to accept more cash. Stacked Denomination: {0}", denomination));
                        writer.Flush();
                        writer.Close();
                    }
                }

                // If in simulation mode, stop the simulation timer once the expected cash amount is stacked.
                if (simulateDevice && amountStackedinCurrentCashCycle >= expectedCashAmount)
                {
                    simulationTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    ////DisableAsync();
                    return;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void OnReturned(int denomination, string currency)
        {
            ////using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
            ////{
            ////    StreamWriter writer = new StreamWriter(stream);
            ////    writer.WriteLine(string.Format(@"Returned: denomination : {0}.", denomination));
            ////    writer.Flush();
            ////    writer.Close();
            ////}

            Action<int> handler = NoteReturnedEvent;
            if (handler != null)
            {
                // Update the list of allowed denominations if configured.
                if (dynamicallyUpdateAllowedDenominations)
                {
                    UpdateAllowedDenominations(ArdacElite.minAmount, ArdacElite.maxAmount, false);
                }
                handler(denomination);
                if (log.IsInfoEnabled) log.Info("Returned: denomination : " + denomination.ToString());
                // Log the confirmation of the denomination returned.
                using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Returned: denomination : {0}.", denomination));
                    writer.Flush();
                    writer.Close();
                }
            }
            else
            {
                // The application not in state to accept more cash after the denomination is returned, so disable the cash device.
                if (log.IsInfoEnabled) log.Info("Application not in a state to accept more cash. Returning denomination available on escrow: " + denomination.ToString() + " and disabling device.");
                using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Application not in a state to accept more cash. Returning denomination available on escrow: {0} and disabling device.", denomination));
                    writer.Flush();
                    writer.Close();
                }

                DisableAsync();
            }
        }

        //private void UpdateAllowedDenominations()
        //{
        //    if (dynamicallyUpdateAllowedDenominations)
        //    {
        //        allowedDenominations.Clear();

        //        double balance = expectedCashAmount - amountStackedinCurrentCashCycle;
        //        foreach (int item in expectedDenominations)
        //        {
        //            if (balance >= item)
        //            {
        //                allowedDenominations.Add(item);
        //            }
        //        }
        //    }
        //    else if (allowedDenominations.Count == 0)
        //    {
        //        foreach (int item in expectedDenominations)
        //        {
        //            allowedDenominations.Add(item);
        //        }
        //    }

        //    if (allowedDenominations != null)
        //    {
        //        using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
        //        {
        //            StreamWriter writer = new StreamWriter(stream);
        //            writer.WriteLine(string.Format(@"Allowed denominations updated to : {0}.", string.Join<int>(", ", allowedDenominations)));
        //            writer.Flush();
        //            writer.Close();
        //        }
        //    }
        //}

        private void UpdateAllowedDenominations(double minAmount, double maxAmount, bool startOfCashCycle)
        {
            int minAcceptableDenom = 0;
            int maxAcceptableDenom = 0;
            bool maxDenomFound = false;

            /*** Especially for Ezetop By Jags on 11/04/12 ****/
            if ((dynamicallyUpdateAllowedDenominations) && minAmount == -2 && maxAmount == -2)
            {
                allowedDenominations.Clear();

                double eZbalance = expectedCashAmount - amountStackedinCurrentCashCycle;
                foreach (int item in expectedDenominations)
                {
                    using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Inside for statement: Balance - {0} MinDenom - {1} Item - {2}.", eZbalance, expectedDenominations[0], item));
                        writer.Flush();
                        writer.Close();
                    }
                    if (eZbalance >= item)
                    {
                        allowedDenominations.Add(item);
                    }
                    //else
                    //{
                    //    using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                    //    {
                    //        StreamWriter writer = new StreamWriter(stream);
                    //        writer.WriteLine(string.Format(@"In Else case: Balance - {0} MinDenom - {1} Item - {2}.", eZbalance, expectedDenominations[0], item));
                    //        writer.Flush();
                    //        writer.Close();
                    //    }
                    //    if (((item - eZbalance) < expectedDenominations[0]) || ((item - eZbalance) < expectedDenominations[1]))
                    //        allowedDenominations.Add(item);
                    //    break;
                    //}
                }
                return;
            }

            if (allowedDenominations == null)
            {
                allowedDenominations = new List<int>();
            }
            allowedDenominations.Clear();

            double allowedAmount = maxAmount - ArdacElite.amountStackedinCurrentCashCycle;

            if ((maxAmount != -1) && (amountStackedinCurrentCashCycle == maxAmount))
                return;

            if (dynamicallyUpdateAllowedDenominations)
            {
                if ((startOfCashCycle) || (0 == amountStackedinCurrentCashCycle)) // Modified By JK on 27/09/12 to enable the logic to work even in case of Bill Return.
                {
                    for (int i = expectedDenominations.Count - 1; i >= 0; i--)
                    {
                        if (minAmount <= expectedDenominations[i])
                        {
                            minAcceptableDenom = (int)expectedDenominations[i];
                        }
                        if ((maxAmount >= expectedDenominations[i]) && !maxDenomFound)
                        {
                            if (i != expectedDenominations.Count - 1)
                                maxAcceptableDenom = expectedDenominations[i + 1];
                            else
                                maxAcceptableDenom = expectedDenominations[i];
                            maxDenomFound = true;
                        }

                    }
                    if (allowedAmount <= minAcceptableDenom)
                    {
                        allowedDenominations.Add(minAcceptableDenom);
                    }
                    else
                    {
                        bool found = false;
                        foreach (int item in expectedDenominations)
                        {
                            if (minAcceptableDenom == item)
                            {
                                found = true;
                                using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                                {
                                    StreamWriter writer = new StreamWriter(stream);
                                    writer.WriteLine(string.Format(@"In StartOfCashcycle case: MaxAcceptableDenom - {0} MinAcceptableDenom - {1} Item - {2}.", maxAcceptableDenom, minAcceptableDenom, item));
                                    writer.Flush();
                                    writer.Close();
                                }
                                allowedDenominations.Add(minAcceptableDenom);
                                continue;
                            }
                            if (found && (maxAcceptableDenom >= item))
                            {
                                using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                                {
                                    StreamWriter writer = new StreamWriter(stream);
                                    writer.WriteLine(string.Format(@"In StartOfCashcycle case: MaxAcceptableDenom - {0} MinAcceptableDenom - {1} Item - {2}.", maxAcceptableDenom, minAcceptableDenom, item));
                                    writer.Flush();
                                    writer.Close();
                                }

                                if (maxAcceptableDenom == item)
                                {
                                    if ((maxAmount >= maxAcceptableDenom) || ((maxAcceptableDenom - maxAmount) <= expectedDenominations[0]) || ((maxAcceptableDenom - maxAmount) <= expectedDenominations[1]))
                                        allowedDenominations.Add(item);
                                    break;
                                }
                                else
                                    allowedDenominations.Add(item);
                            }
                        }
                    }
                } //end of if case for startCashCycle.
                else
                {
                    using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format("Max Amount : {0} ", maxAmount));
                        writer.WriteLine(string.Format("Allowed Amount : {0} ", allowedAmount));
                        writer.Flush();
                        writer.Close();
                    }

                    if (allowedAmount <= expectedDenominations[0])
                    {
                        //if(maxAmount == -1)  
                        allowedDenominations.Add(expectedDenominations[0]);
                        allowedDenominations.Add(expectedDenominations[1]);
                    }
                    else
                    {
                        foreach (int item in expectedDenominations)
                        {
                            if (item <= allowedAmount)
                                allowedDenominations.Add(item);
                            //else //Added By Jags on 27/06/12
                            else // Added By Jags on 14/01/15 for Vinod Kumar
                            {
                                if (((item - allowedAmount) <= expectedDenominations[0]) || ((item - allowedAmount) <= expectedDenominations[1]))
                                    allowedDenominations.Add(item);
                                break;
                            }
                        }

                    }
                    using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        foreach (int item in allowedDenominations)
                        {
                            writer.WriteLine(string.Format("Allowed Denominations : {0} ", item.ToString()));
                        }
                        writer.Flush();
                        writer.Close();
                    }

                } //  end of else for startCashCycle
            }  // end of dynamicdenominations update case..
            else if (allowedDenominations.Count == 0)
            {
                foreach (int item in expectedDenominations)
                {
                    allowedDenominations.Add(item);
                }
            }
        }

        private bool IsExpectedDenomination(int denomination)
        {
            bool isValid = false;
            for (int i = 0; i < allowedDenominations.Count; i++)
            {
                if (denomination == allowedDenominations[i])
                {
                    isValid = true;
                    break;
                }
            }

            return isValid;
        }

        private int GetDenomination(int code)
        {
            int value = 0;
            switch (code)
            {
                case 98:
                    value = 5;
                    break;
                case 99:
                    value = 10;
                    break;
                case 100:
                    value = 20;
                    break;
                case 101:
                    value = 50;
                    break;
                case 102:
                    value = 100;
                    break;
                case 103:
                    value = 200;
                    break;
                case 104:
                    value = 500;
                    break;
                case 105:
                    value = 1000;
                    break;
            }

            return value;
        }

        private string GetConcatenatedAllowedDenominations()
        {
            try
            {
                string denominations = string.Empty;
                StringBuilder denominationStringBuilder = new StringBuilder();
                for (int i = 0; i < allowedDenominations.Count; i++)
                {
                    denominationStringBuilder = denominationStringBuilder.Append(allowedDenominations[i]);
                    denominationStringBuilder = denominationStringBuilder.Append(", ");
                }

                if (denominationStringBuilder.Length > 0)
                {
                    denominations = denominationStringBuilder.ToString().Substring(0, denominationStringBuilder.Length - 2);
                }
                return denominations;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void SimulateNoteStacked(object o)
        {
            // Populate the highest expected denomination to complete the simulated transaction at the earliest.
            int simulatedDenominationValue = 0;
            if (simulateDevice && isEnabled && allowedDenominations.Count > 0)
            {
                // Initialize to the minimum denomination value.
                simulatedDenominationValue = allowedDenominations[0];

                for (int i = allowedDenominations.Count - 1; i >= 0; i--)
                {
                    if (allowedDenominations[i] <= (expectedCashAmount - amountStackedinCurrentCashCycle))
                    {
                        simulatedDenominationValue = allowedDenominations[i];
                        break;
                    }
                }
            }

            if (simulatedDenominationValue > 0)
            {
                // Simulate note insertion.
                OnEscrow(simulatedDenominationValue);

                // Simulate note stacked.
                OnStacked(simulatedDenominationValue, "02");
            }
        }

        private void LogEventToLocalDb(string eventId)
        {

            if (log.IsInfoEnabled) log.Info("LogEventToLocalDb started..");
            try
            {
                Logger.LogEvents(new EventLogger
                {
                    //KioskId =  ConfigurationManager.AppSettings["MachineId"] != null? Convert.ToInt32(ConfigurationManager.AppSettings["MachineId"]):0,
                    KioskId = Convert.ToInt32(KioskAppConfig.KioskId),
                    EventDtTm = DateTime.Now,
                    EventId = eventId,
                    Desc = string.Empty

                }, LOGTO.DATABASE);

                if (log.IsInfoEnabled) log.Info("LogEventToLocalDb ended.eventId added is:" + eventId + ",KioskId:" + KioskAppConfig.KioskId + ",Description:" + ArdacEliteEventCodes.GetEventDesc(Convert.ToInt16(eventId)).ToString());
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Exception thrown in LogEventToLocalDb" + ex.Message);

            }
            if (log.IsInfoEnabled) log.Info("LogEventToLocalDb ended..");

        }
        #endregion
    }
}
