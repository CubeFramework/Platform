using System;
using System.Collections.Generic;
using System.Text;
using MBMEKiosk.Infrastructure.Interfaces;
using System.Threading;
using System.Configuration;
using MBMEDevices.CardReader;
using System.IO;
using System.Runtime.InteropServices;


namespace MBMEDevices.Readers
{
    internal delegate void DPaymentComplete(bool confirmPayment, double amount);

    public class Xenteo : ICardReader
    {
        EftTerminalThales terminal;
        Display display;
        Printer printer;
        Confirmation confirmation;

        // Card Reader Simulation related fields.
        private static bool simulateDevice;
        private static Timer simulationTimer;
        private static Timer initTimer;
        private static Timer settlementTimer;
        private static bool isInitialized;
        private static bool isEnabled;
        private static double expectedAmount;
        // Not expected in Credit Card cases but can happen in case of Debit/ATM card,
        // and should be allowed for services where partial payment is enabled.

        // Cash Acceptor State/Status/Status Desc...
        private int stateCode;
        private int statusCode;
        private int faultCode;

        // Payment Stage Identifier
        byte PaymentStageIdentifier;
        // Xenteo - Automate.DLL related fields
        string com;
        ushort uEtatPaiement = 0x70;
        ushort uEtatApplication = 0x00;
        byte timeout = 99;
        IntPtr ptrEtatPaiement;
        IntPtr ptr32Montant;
        IntPtr ptrEtatApplication;
        IntPtr ptrOptions;
        IntPtr ptrCartePresent;
        IntPtr ptrTicket;
        IntPtr ptrDossier;
        IntPtr ptrTLC;
        IntPtr ptrTLP;
        IntPtr ptrDLW;

        // Async method call delegates.
        DInit initMethod;
        DPayment paymentMethod;
        DPaymentComplete paymentCompleteMethod;

        /// <summary>
        /// Event to prompt the User to Swipe(INSERT/REMOVE)) Card/Enter PIN /ReEnter PIN.
        /// </summary>
        ////public event Action SwipeCardEvent;
        public event Action<string> SwipeCardEvent;

        /// <summary>
        /// Event to publish the action of Payment being authorized by the host.
        /// </summary>
        ////public event Action<double> PaymentAuthorizedEvent;
        public event Action<double> PaymentAuthorizedEvent;

        /// <summary>
        /// Event to publish the action of Payment confirmation Notification.
        /// </summary>
        ////public event Action<double> PaymentConfirmedEvent;
        public event Action<double> PaymentConfirmationEvent;
        /// <summary>
        /// Event to publish the action of Payment being confirmed by the Bank Host.
        /// </summary>
        ////public event Action<double> PaymentConfirmedEvent;
        public event Action<double> PaymentConfirmedEvent;

        /// <summary>
        /// Event to publish the action of failed Payment.
        /// </summary>
        ////public event Action<double> PaymentFailedEvent;
        public event Action<double> PaymentFailedEvent;

        /// <summary>
        /// Event to publish the action of printing receipt.
        /// </summary>
        ////public event Action<string,string,string,string,string,string,string,string,string> ReceiptEvent;
        public event Action<string, string, string, string, string, string, string, string, string, string> ReceiptEvent;

        #region Wrappers for the cash acceptor APIs.

        //, EntryPoint = "PollingThreadCommand", SetLastError = false, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall
        //[System.Runtime.InteropServices.DllImport(@"CashDevices\ArdacElite\drivers\Ardac-Elite_V1_0.dll")]
        [System.Runtime.InteropServices.DllImport(@"Automate.dll")]
        internal static extern UInt16 TPADoPaymentStep([MarshalAs(UnmanagedType.LPStr)] string strIODeviceName, IntPtr cEtatPaiement, IntPtr ptrTicket, [MarshalAs(UnmanagedType.I2)]short i16Devise, IntPtr ptr32Montant, IntPtr ptrOptions, [MarshalAs(UnmanagedType.I1)] byte To, IntPtr ptrDossier);

        [System.Runtime.InteropServices.DllImport(@"Automate.dll", EntryPoint = "BVUOpen", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        internal static extern UInt16 TPAEndSession([MarshalAs(UnmanagedType.LPStr)] string strIODeviceName, IntPtr cEtatPaiement);

        [System.Runtime.InteropServices.DllImport(@"Automate.dll")]
        internal static extern UInt16 TPAStatus([MarshalAs(UnmanagedType.LPStr)] string strIODeviceName, [MarshalAs(UnmanagedType.LPStr)] string strNumAppli, IntPtr cEtatPaiement, IntPtr ptrCartePresent, IntPtr cEtatApplication, IntPtr ptrTLC, IntPtr ptrTLP, IntPtr ptrDLW);

        [System.Runtime.InteropServices.DllImport(@"Automate.dll")]
        internal static extern UInt16 TPASendReceipt([MarshalAs(UnmanagedType.LPStr)] string strIODeviceName, IntPtr cEtatPaiement, [MarshalAs(UnmanagedType.I1)]byte bReceiptType, [MarshalAs(UnmanagedType.LPStr)] string strNumAppli, [MarshalAs(UnmanagedType.LPStr)] string strTicket, [MarshalAs(UnmanagedType.LPStr)] string strMoreBlock);
        #endregion

        #region Methods to interact with the card reader device driver from external applications
        /// <summary>
        /// Initializes the card reader
        /// </summary>
        public void InitAsync(bool simulateCardReader)
        {

            if (!isInitialized)
            {

                simulateDevice = simulateCardReader;

                using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- InitAsync invoked with simulateCardReader = {1}.", DateTime.Now, simulateCardReader));
                    writer.Flush();
                    writer.Close();
                }

                // Initialize delegate methods only once in application life cycle.
                initMethod = new DInit(this.Init);

                this.statusCode = (int)ReaderStatus.Initializing;
                // Initialize the cash acceptor.
                initMethod.BeginInvoke(false, null, null);
            }
        }

        /// <summary>
        /// Initiates the Payment Authorization Sequence (Step - 1 / Step - 2).
        /// </summary>
        public bool PaymentAsync(double amount)
        {
            expectedAmount = amount;
            PaymentStageIdentifier = 1;
            if (isInitialized)
            {
                using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- PaymentAsync invoked with Amount = {1}.", DateTime.Now, amount));
                    writer.Flush();
                    writer.Close();
                }

                // Initialize delegate methods only once in application life cycle.
                paymentMethod = new DPayment(this.Payment);
                // Initiating Card Payment.
                paymentMethod.BeginInvoke(amount, null, null);
            }

            return true;
        }

        /// <summary>
        /// Initiates the Payment Authorization Sequence.
        /// </summary>
        public bool PaymentCompleteAsync(bool confirmPayment, double amount)
        {
            PaymentStageIdentifier = 2;
            if (isInitialized)
            {
                using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- PaymentAsync invoked with Amount = {1}.", DateTime.Now, amount));
                    writer.Flush();
                    writer.Close();
                }

                // Initialize delegate methods only once in application life cycle.
                paymentCompleteMethod = new DPaymentComplete(this.PaymentComplete);
                // Initiating Card Payment.
                paymentCompleteMethod.BeginInvoke(confirmPayment, amount, null, null);
            }

            return true;
        }

        #endregion

        #region Methods to manage Xenteo
        private void Init(bool reconnect)
        {

            if (simulateDevice)
            {
                using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- CardReader Initialization in Simulated Mode.", DateTime.Now));
                    writer.Flush();
                    writer.Close();
                }
                isEnabled = true;
                isInitialized = true;
                this.stateCode = (int)CardReaderState.ONLINE;
                this.statusCode = (int)ReaderStatus.NoError;
                if (simulationTimer == null)
                    simulationTimer = new Timer(new TimerCallback(SimulateCardReading), null, Timeout.Infinite, Timeout.Infinite);
            }
            else
            {
                // call TPAStatus to check the status of CB Application..

                ptrEtatPaiement = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ushort)));
                ptr32Montant = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ushort)));
                ptrEtatApplication = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ushort)));
                ptrOptions = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)));
                ptrTicket = Marshal.AllocHGlobal(2048);
                ptrDossier = Marshal.AllocHGlobal(12);
                ptrCartePresent = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)));
                ptrTLC = Marshal.AllocHGlobal(2048);
                ptrTLP = Marshal.AllocHGlobal(2048);
                ptrDLW = Marshal.AllocHGlobal(2048);
                Marshal.WriteInt16(ptrEtatPaiement, (short)uEtatPaiement);
                Marshal.WriteInt16(ptrEtatApplication, (short)uEtatApplication);

                using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- CardReader Initialization.", DateTime.Now));
                    writer.Flush();
                    writer.Close();
                }
                com = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["COMPORT"]) ? "COM1" : ConfigurationManager.AppSettings["COMPORT"];
                string strTLC = new string('\0', 2048);
                string strTLP = new string('\0', 2048);
                string strDLW = new string('\0', 2048);
                byte bCartePresent = 0x00;
                Marshal.WriteByte(ptrCartePresent, bCartePresent);
                ptrTLC = Marshal.StringToHGlobalAnsi(strTLC);
                ptrTLP = Marshal.StringToHGlobalAnsi(strTLP);
                ptrDLW = Marshal.StringToHGlobalAnsi(strDLW);

                faultCode = TPAStatus(com, "02001", ptrEtatPaiement, ptrCartePresent, ptrEtatApplication, ptrTLC, ptrTLP, ptrDLW);
                uEtatPaiement = (ushort)Marshal.ReadInt16(ptrEtatPaiement);
                uEtatApplication = (ushort)Marshal.ReadInt16(ptrEtatApplication);
                bCartePresent = Marshal.ReadByte(ptrCartePresent);
                strTLC = Marshal.PtrToStringAuto(ptrTLC);
                strTLP = Marshal.PtrToStringAuto(ptrTLP);
                strDLW = Marshal.PtrToStringAuto(ptrDLW);
                using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- Initialization Successful - Payment - {1} - Fault.", DateTime.Now, uEtatPaiement));
                    writer.Flush();
                    writer.Close();
                }

                if ((faultCode == 0) && (uEtatPaiement == 0) && (uEtatApplication == 1))
                {
                    using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Initialization Successful.", DateTime.Now));
                        writer.Flush();
                        writer.Close();
                    }
                    //terminal.disconnect();
                    isEnabled = true;
                    isInitialized = true;
                    this.stateCode = (int)CardReaderState.ONLINE;
                    this.statusCode = (int)ReaderStatus.NoError;
                }
                else
                {
                    using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Initialization Failed. RetCode {1}. EtatPaiement {2}. EtatApplication {3}.", DateTime.Now, faultCode, uEtatPaiement, uEtatApplication));
                        writer.Flush();
                        writer.Close();
                    }
                    this.stateCode = (int)CardReaderState.OFFLINE;
                    this.statusCode = (int)ReaderStatus.Error;
                    isEnabled = false;
                    isInitialized = false;
                    if (0 == this.faultCode)
                        this.faultCode = uEtatPaiement;
                    // start a timer to make sure the device get's initialized properly.
                    if (initTimer == null)
                        initTimer = new Timer(new TimerCallback(ReInitialize), null, 20000, 20000);
                    else
                        initTimer.Change(20000, 20000);
                }
            }


        }

        // Settlement function Implementation as specified by the EMV Kernel and the Bank Host at midnight. 
        public void Settlement(object o)
        {
            settlementTimer.Change(Timeout.Infinite, Timeout.Infinite);


            settlementTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void ReInitialize(object o)
        {
            using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(string.Format(@"Datetime {0} -- ReInitializing.", DateTime.Now));
                writer.Flush();
                writer.Close();
            }
            initTimer.Change(Timeout.Infinite, Timeout.Infinite);
            Init(true);
        }

        public int GetFaultCode()
        {
            return faultCode;
        }

        public string GetDetails(out int state, out int status)
        {
            string statusDesc = string.Empty;
            IsReady();
            state = this.stateCode;
            status = this.statusCode;
            switch (status)
            {
                case (int)ReaderStatus.Initializing:
                    statusDesc = "Card Reader Initialization Sequence happening.";
                    break;
                case (int)ReaderStatus.NoError:
                    statusDesc = "No Error.";
                    break;
                case (int)ReaderStatus.Warning:
                    statusDesc = "Warning.";
                    break;
                case (int)ReaderStatus.Error:
                    statusDesc = ArtemaModularFaultCodes.GetFaultDesc(GetFaultCode());
                    break;
            }

            return statusDesc;

        }

        public bool IsReady()
        {

            if (!simulateDevice)
            {
                if ((stateCode == (long)CardReaderState.ONLINE) && (statusCode == (long)ReaderStatus.NoError))
                    return true;
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

        public void ParseTicket(string ticket)
        {
            this.OnReceiptPrintNotification(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        public void OnReceiptPrintNotification(string MerchantID, string TerminalID, string Authorization, string AID, string AppName, string TVR, string TSI, string ACInfo, string AC, string responseCode)
        {
            Action<string, string, string, string, string, string, string, string, string, string> handler = ReceiptEvent;
            if (handler != null)
            {
                using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- OnReceiptPrintNotification Called - TVR-{1}|||TSI-{2}|||ACInfo-{3}|||MerchantID-{4}|||TerminalID-{5}|||AID-{6}|||AppName-{7}|||AC-{8}|||RC-{9}.", DateTime.Now, TVR, TSI, ACInfo, MerchantID, TerminalID, AID, AppName, AC, responseCode));
                    writer.Flush();
                    writer.Close();
                }
                handler(MerchantID, TerminalID, Authorization, AID, AppName, TVR, TSI, ACInfo, AC, responseCode);
            }
        }

        public void PaymentComplete(bool confirmPayment, double amount)
        {
            Action<double> handler;
            if (!simulateDevice)
            {
                if (confirmPayment)
                {
                    // Based on the value of the confirmPayment Flag the driver would perform or
                    // by pass the Payment Step - II.
                    short i16Devise = 0x03D2;
                    int i32Montant = Convert.ToInt32(amount * 100);
                    uEtatPaiement = 0x70;
                    Marshal.WriteInt16(ptrEtatPaiement, (short)uEtatPaiement);
                    Marshal.WriteInt32(ptr32Montant, i32Montant);
                    string tcTicket = new string('\0', 2048);
                    string Dossier = new string('0', 12);
                    byte bOption = 0x34;
                    Marshal.WriteByte(ptrOptions, bOption);
                    using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Before Calling TPADoPaymentStep(1).", DateTime.Now));
                        writer.Flush();
                        writer.Close();
                    }
                    ptrTicket = Marshal.StringToHGlobalAnsi(tcTicket);
                    ptrDossier = Marshal.StringToHGlobalAnsi(Dossier);
                    faultCode = TPADoPaymentStep(com, ptrEtatPaiement, ptrTicket, i16Devise, ptr32Montant, ptrOptions, timeout, ptrDossier);
                    uEtatPaiement = (ushort)Marshal.ReadInt16(ptrEtatPaiement);
                    bOption = Marshal.ReadByte(ptrOptions);
                    tcTicket = Marshal.PtrToStringAuto(ptrTicket);
                    Dossier = Marshal.PtrToStringAuto(ptrDossier);
                    if ((faultCode == 0) && (uEtatPaiement == 0))
                    {
                        using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- Payment successful.", DateTime.Now));
                            writer.Flush();
                            writer.Close();
                        }
                        //Console.WriteLine("payment successful, PAN=" + PAN);
                        handler = PaymentConfirmedEvent;
                        if (handler != null)
                        {
                            using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                            {
                                StreamWriter writer = new StreamWriter(stream);
                                writer.WriteLine(string.Format(@"Datetime {0} -- Invoking PaymentConfirmedEvent.", DateTime.Now));
                                writer.Flush();
                                writer.Close();
                            }
                            handler(expectedAmount);
                        }
                        this.ParseTicket(tcTicket);
                    }
                    else
                    {
                        if (0 == this.faultCode)
                            this.faultCode = uEtatPaiement;
                        using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- PaymentFailed with error # {1}.", DateTime.Now, faultCode));
                            writer.Flush();
                            writer.Close();
                        }
                        handler = PaymentFailedEvent;
                        //this.statusCode = (int)ReaderStatus.Error;                    
                        if (handler != null)
                        {
                            using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                            {
                                StreamWriter writer = new StreamWriter(stream);
                                writer.WriteLine(string.Format(@"Datetime {0} -- Invoking PaymentFailedEvent.", DateTime.Now));
                                writer.Flush();
                                writer.Close();
                            }
                            handler(expectedAmount);
                        }
                    }

                    //Free up the global memory.
                }
                // The driver needs to perform the TPAEndSession everytime irrespective of the fact
                // whether payment needs to be confirmed/completed or cancelled.
                uEtatPaiement = 0x70;
                Marshal.WriteInt16(ptrEtatPaiement, (short)uEtatPaiement);
                TPAEndSession(com, ptrEtatPaiement);
                uEtatPaiement = (ushort)Marshal.ReadInt16(ptrEtatPaiement);

            }
            else
            {
                simulationTimer.Change(5000, 5000);
            }

        }

        public void Payment(double amount)
        {
            if (!simulateDevice)
            {
                Action<double> handler;
                faultCode = 0;
                // Initializing currency ID = 978 (EURO)
                short i16Devise = 0x03D2;
                int i32Montant = Convert.ToInt32(amount * 100);
                uEtatPaiement = 0x70;
                Marshal.WriteInt16(ptrEtatPaiement, (short)uEtatPaiement);
                Marshal.WriteInt32(ptr32Montant, i32Montant);
                string tcTicket = new string('\0', 2048);
                string Dossier = new string('0', 12);
                byte bOption = 0x31;
                Marshal.WriteByte(ptrOptions, bOption);
                using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- Before Calling TPADoPaymentStep(1).", DateTime.Now));
                    writer.Flush();
                    writer.Close();
                }
                ptrTicket = Marshal.StringToHGlobalAnsi(tcTicket);
                ptrDossier = Marshal.StringToHGlobalAnsi(Dossier);
                faultCode = TPADoPaymentStep(com, ptrEtatPaiement, ptrTicket, i16Devise, ptr32Montant, ptrOptions, timeout, ptrDossier);
                uEtatPaiement = (ushort)Marshal.ReadInt16(ptrEtatPaiement);
                bOption = Marshal.ReadByte(ptrOptions);
                tcTicket = Marshal.PtrToStringAuto(ptrTicket);
                Dossier = Marshal.PtrToStringAuto(ptrDossier);
                if ((faultCode == 0) && (uEtatPaiement == 0))
                {
                    using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Payment successful - cpOptions - {1}.", DateTime.Now, bOption.ToString()));
                        writer.Flush();
                        writer.Close();
                    }
                    //Console.WriteLine("payment successful, PAN=" + PAN);
                    handler = PaymentAuthorizedEvent;
                    if (handler != null)
                    {
                        using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- Invoking PaymentAuthorizedEvent.", DateTime.Now));
                            writer.Flush();
                            writer.Close();
                        }
                        handler(expectedAmount);
                    }
                }
                else
                {
                    if (0 == this.faultCode)
                        this.faultCode = uEtatPaiement;
                    using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- PaymentFailed with error # {1}.", DateTime.Now, faultCode));
                        writer.Flush();
                        writer.Close();
                    }
                    handler = PaymentFailedEvent;
                    //this.statusCode = (int)ReaderStatus.Error;                    
                    if (handler != null)
                    {
                        using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- Invoking PaymentFailedEvent.", DateTime.Now));
                            writer.Flush();
                            writer.Close();
                        }
                        handler(expectedAmount);
                    }
                    //if (faultCode == (int)ErrorCode.ERR_LOST_CONNECTION)
                    //{
                    //    // Lost Connection with the CardReader/Pinpad
                    //    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                    //    {
                    //        StreamWriter writer = new StreamWriter(stream);
                    //        writer.WriteLine(string.Format(@"Datetime {0} -- Connection Lost. ReConnecting.", DateTime.Now));
                    //        writer.Flush();
                    //        writer.Close();
                    //    }
                    //    this.stateCode = (int)CardReaderState.OFFLINE;
                    //    this.statusCode = (int)ReaderStatus.Error;
                    //    // Try ReConnecting.
                    //    if (initTimer == null)
                    //        initTimer = new Timer(new TimerCallback(ReInitialize), null, 50, 50);
                    //    else
                    //        initTimer.Change(50, 50);
                    //}
                }
            }
            else
            {
                simulationTimer.Change(5000, 5000);
            }
            //return faultCode;
        }

        private void SimulateCardReading(object o)
        {
            ushort confirm;

            simulationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            // Simulate Card based Payment sequence.

            if (PaymentStageIdentifier == 1)
            {
                // Authorization Received Notification.
                Thread.Sleep(3000);
                Action<double> phandler = PaymentAuthorizedEvent;
                if (phandler != null)
                {
                    using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Invoking PaymentAuthorizedEvent.", DateTime.Now));
                        writer.Flush();
                        writer.Close();
                    }
                    phandler(expectedAmount);
                }
            }

            if (PaymentStageIdentifier == 2)
            {
                Action<double> handler = PaymentConfirmedEvent;
                if (handler != null)
                {
                    using (FileStream stream = new FileStream(@"Xenteo.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Invoking PaymentConfirmedEvent.", DateTime.Now));
                        writer.Flush();
                        writer.Close();
                    }
                    handler(expectedAmount);
                }
            }
        }
        #endregion Methods to manage Xenteo
    }
}