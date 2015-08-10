using System;
using System.Collections.Generic;
using System.Text;
using MBMEKiosk.Infrastructure.Interfaces;
using System.Threading;
using System.Configuration;
using MBMEDevices.CardReader;
using System.IO;
using System.Windows.Forms;

namespace MBMEDevices.USBKeyboard
{
    internal delegate void DInit(bool reconnect);
    public class PP151 : IUSBKeyboard
    {
        // Card Reader Simulation related fields.
        private static bool simulateDevice;
        private static System.Threading.Timer simulationTimer;
        private static System.Threading.Timer initTimer;
        private static bool isInitialized;
        private static bool isEnabled;
        private static double expectedAmount;
        // Not expected in Credit Card cases but can happen in case of Debit/ATM card,
        // and should be allowed for services where partial payment is enabled.

        // Cash Acceptor State/Status/Status Desc...
        private int stateCode;
        private int statusCode;
        private int faultCode;

        // Async method call delegates.
        DInit initMethod;

        IntPtr Source;
        InputDevice id;
        bool DevicesEnumerated = false;
	    int NumberOfKeyboards;
        Message message = new Message();

	// event and Delegate
	//public delegate void DeviceEventHandler(object sender, KeyControlEventArgs e);
        public event Action <byte> KeyPressedEvent;

        #region Methods to interact with the card reader device driver from external applications
        public PP151(IntPtr hwnd)
        {
            Source = hwnd;
        }
        
        /// <summary>
        /// Initializes the card reader
        /// </summary>
        public void InitAsync(bool simulateCardReader)
        {

            if (!isInitialized)
            {
                isInitialized = true;
                simulateDevice = simulateCardReader;

                using (FileStream stream = new FileStream(@"PP151.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- InitAsync invoked with simulateCardReader = {1}.", DateTime.Now, simulateCardReader));
                    writer.Flush();
                    writer.Close();
                }

                // Initialize delegate methods only once in application life cycle.
                initMethod = new DInit(this.Init);

                this.statusCode = (int)USBKeyboardStatus.Initializing;
                // Initialize the cash acceptor.
                initMethod.BeginInvoke(false, null, null);
            }
        }

        public void ProcessMessage(Message msg)
        {
            if ((id != null) && (DevicesEnumerated))
            {
                using (FileStream stream = new FileStream(@"PP151.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- Inside ProcessMessage.", DateTime.Now));
                    writer.Flush();
                    writer.Close();
                }
                id.ProcessMessage(msg);
            }
        }
        #endregion

        #region Methods to manage ArtemaModular
        private void Init(bool reconnect)
        {
            if (!reconnect)
            {
                // Initialize/Reset the Card device.
                isEnabled = false;
            } // End of Reconnect flag check.
            if (simulateDevice)
            {
                using (FileStream stream = new FileStream(@"PP151U.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- CardReader Initialization in Simulated Mode.", DateTime.Now));
                    writer.Flush();
                    writer.Close();
                }
                isEnabled = true;
                this.stateCode = (int)USBKeyboardState.ONLINE;
                this.statusCode = (int)USBKeyboardStatus.NoError;
                if (simulationTimer == null)
                    simulationTimer = new System.Threading.Timer(new TimerCallback(SimulateCardReading), null, Timeout.Infinite, Timeout.Infinite);
            }
            else
            {
                string ReaderName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["EIDAREADERNAME"]) ? "EIDA" : ConfigurationManager.AppSettings["EIDAREADERNAME"];
                using (FileStream stream = new FileStream(@"PP151.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- CardReader Initialization.", DateTime.Now));
                    writer.Flush();
                    writer.Close();
                }
                try
                {
		            id = new InputDevice(Source);
            	    NumberOfKeyboards = id.EnumerateDevices();
                    if (NumberOfKeyboards > 0)
                        DevicesEnumerated = true;
            	    id.KeyPressed += _KeyPressed;
                }
                catch (Exception ex)
                {
                    using (FileStream stream = new FileStream(@"PP151.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- CardReader Initialization Exception - {1}.", DateTime.Now,ex.Message));
                        writer.Flush();
                        writer.Close();
                    }
                    this.stateCode = (int)USBKeyboardState.ONLINE;
                    this.statusCode = (int)USBKeyboardStatus.Error;
                }

            }

        }
        
	    private void _KeyPressed(object sender, InputDevice.KeyControlEventArgs e)
	    {
            using (FileStream stream = new FileStream(@"PP151.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(string.Format(@"Datetime {0} -- Inside KeyPressed -- {1}. Token is {2}", DateTime.Now, e.Keyboard.key.ToString(),
                    e.Keyboard.Name));
                writer.Flush();
                writer.Close();
            }
            //string[] tokens = e.Keyboard.Name.Split(';');
	        //string token = tokens[1];

            //lbHandle.Content = e.Keyboard.deviceHandle.ToString();
            //lbType.Content   = e.Keyboard.deviceType;
            //lbName.Content   = e.Keyboard.deviceName;
            //lbKey.Content    = e.Keyboard.key.ToString();
            //lbVKey.Content   = e.Keyboard.vKey;
            //lbDescription.Content  = token; 
            //lbNumKeyboards.Content = NumberOfKeyboards.ToString();
        
            System.Console.WriteLine("KeyPressed : " + e.Keyboard.key.ToString());
	        Action<byte> handler = KeyPressedEvent;
	        if(handler != null)
                  handler((byte)e.Keyboard.key);
	        System.Console.WriteLine("Inside KeyPressed");
        }

        public bool RegisterRawInputDevices()
        {
            if (id != null)
            {
                if (!id.RegisterTopLevelCollection())
                    return false;
                else
                    return true;
            }
            else
            {
                return false;
            }
        }

        public bool DeRegisterRawInputDevices()
        {
            if (id != null)
            {
                if (!id.DeRegisterTopLevelCollection())
                    return false;
                else
                    return true;
            }
            else
            {
                return false;
            }
        }

        public void ReInitialize(object o)
        {
            using (FileStream stream = new FileStream(@"PP151.log", FileMode.Append, FileAccess.Write))
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
                    statusDesc = "Error";
                    break;
            }

            return statusDesc;

        }

        public bool IsReady()
        {

            if (!simulateDevice)
            {
                //if (readers.Length != 0)
                //{
                //    stateCode = (long)USBKeyboardState.ONLINE;
                //    statusCode = (long)USBKeyboardStatus.NoError;
                //    return true;
                //}
                //else
                //{
                //    stateCode = (long)USBKeyboardState.OFFLINE;
                //    statusCode = (long)USBKeyboardStatus.Error;
                //}
                    
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


        //public bool IsConnected()
        //{
        //    //return reader.IsConnected();
        //}


        private void SimulateCardReading(object o)
        {

        }
        #endregion Methods to manage EIDAPCSCCardReader
    }
}
