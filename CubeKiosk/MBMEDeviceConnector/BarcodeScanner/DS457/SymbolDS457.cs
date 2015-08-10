using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.Interfaces;
using System.IO.Ports;
using System.Configuration;
using System.IO;
using System.Threading;

namespace MBMEDevices.BarcodeScanner
{
    internal delegate void DInit(bool reconnect);
    internal delegate void DStartScanning(short id);
    internal delegate void DStopScanning();
    public class DS457
    {
        public SerialPort _serialPort;
        public short id;
        public byte[] request = new byte[100];
        public byte[] reply = new byte[100];
        public bool readInProgress = false;
        public bool sessionInProgress = false;
        // Barcode Reader State/Status/Status Desc...
        public int stateCode;
        public int statusCode;
        public string statusDesc;
    }

    public class SymbolDS457 : IDecoder
    {
        // Barcode class member variables.
        private static bool simulateDevice;
        private static bool isInitialized;
        private int replyLength = 0;
        private int replyOffset = 0;
        private int multiVend = 0;
        private static List<DS457> scanners;

        System.Threading.Timer tmrScanningSessionTimer = null;

        // Barcode Reader State/Status/Status Desc...
        private int stateCode;
        private int statusCode;
        private string statusDesc;

        // Async method call delegates.
        DInit initMethod;
        DStartScanning startScanningMethod;
        DStopScanning stopScanningMethod;
        /// <summary>
        /// Publish the fact that Scanning has been enabled on the DS457.
        /// </summary>
        ////public event Action ScanEnabledEvent;
        public event Action ScanEnabledEvent;

        /// <summary>
        /// Publish the fact that the decoder has been ordered to start scanning session.
        /// </summary>
        ////public event Action TriggerPulledEvent;
        public event Action TriggerPulledEvent;

        /// <summary>
        /// Publish the fact that the decoder has been ordered to end scanning session.
        /// </summary>
        ////public event Action TriggerReleasedEvent;
        public event Action TriggerReleasedEvent;

        /// <summary>
        /// Event to publish the scanned barcode.
        /// </summary>
        ////public event Action BarcodeScannedEvent;
        public event Action<string> BarcodeScannedEvent;

        public SymbolDS457()
        {
            // Capability to manage maximum 5 m/c's.
            scanners = new List<DS457>();
        }
        #region Methods to interact with the barcode device driver from external applications

        public void InitAsync(bool simulateDecoder)
        {
            if (!isInitialized)
            {
                isInitialized = true;
                simulateDevice = simulateDecoder;
                // Initialize delegate methods only once in application life cycle.
                initMethod = new DInit(this.Init);

                this.statusCode = (int)DecoderStatus.Initializing;
                // Initialize the cash acceptor.
                initMethod.BeginInvoke(false, null, null);
            }
        }

        public void StartScanningAsync(short id)
        {
            if (isInitialized)
            {
                // Initialize delegate methods only once in application life cycle.
                startScanningMethod = new DStartScanning(this.StartScanning);
                // Pulling Trigger.
                startScanningMethod.BeginInvoke(id,null, null);
            }
        }

        public void StopScanningAsync()
        {
            if (isInitialized)
            {
                // Initialize delegate methods only once in application life cycle.
                stopScanningMethod = new DStopScanning(this.StopScanning);
                // Initiating Trigger Release.
                stopScanningMethod.BeginInvoke(null, null);
            }
        }

        #endregion

        #region Methods to manage DS457

        public string GetDetails(out int state, out int status)
        {
            IsReady();
            state = this.stateCode;
            status = this.statusCode;

            return statusDesc;

        }

        public bool IsReady()
        {
            bool happystat = false;
            this.statusDesc = null;
            if (!simulateDevice)
            {
                foreach (DS457 item in scanners)
                {
                    if ((item.stateCode == (int)DecoderState.ONLINE) && (item.statusCode == (int)DecoderStatus.NoError))
                    {
                        this.stateCode = item.stateCode;
                        this.statusCode = item.statusCode;
                        this.statusDesc = item.statusDesc;
                        happystat = true;
                        break;
                    }
                    else
                    {
                        this.statusDesc += item._serialPort.PortName + " - " + item.statusDesc;
                    }
                }
                if (!happystat)
                {
                    this.stateCode = (int)DecoderState.ONLINE;
                    this.statusCode = (int)DecoderStatus.Error;
                }
            }
            else
            {
                this.statusCode = 1;
                this.stateCode = 1;
                return true;
            }

            return happystat;
            //return statusCode;
            // KS TODO : Log status.
            //return statusCode == (short)CashAcceptorStatus.NoError;
        }

        private byte[] CalculateChecksum(DS457 scanner, short reqLen)
        {
            int counter = 0;
            int sum = 0;
            while (counter < reqLen)
            {
                sum += scanner.request[counter];
                counter++;
            }
            // Calculate twos complement of the sum as per SSI protocol requirements.
            sum = ~sum + (int)1;

            byte[] checksum = new byte[2];
            checksum[0] = (byte)(sum >> 8);
            checksum[1] = (byte)(sum & 0xFF);
            return checksum;
        }

        private void Open(DS457 scanner)
        {
            try
            {
                //_serialPort.DataReceived += new SerialDataReceivedEventHandler(Read);
                //_serialPort.ErrorReceived += new SerialErrorReceivedEventHandler(OpError);
                scanner._serialPort.Open();
            }
            catch (TimeoutException tex)
            {
                scanner.stateCode = (int)DecoderState.OFFLINE;
                scanner.statusCode = (int)DecoderStatus.Error;
                scanner.statusDesc = tex.Message + tex.InnerException;
            }
            catch (Exception ex)
            {
                scanner.stateCode = (int)DecoderState.OFFLINE;
                scanner.statusCode = (int)DecoderStatus.Error;
                scanner.statusDesc = ex.Message + ex.InnerException;
            }
            finally
            {
                scanner.stateCode = (int)DecoderState.ONLINE;
            }
        }



        private byte Request_Revision(DS457 scanner)
        {
            scanner.request[0] = 0x04; // Length of message not including Checksum.
            scanner.request[1] = 0xA3; // OPCODE - REQUEST_REVISION
            scanner.request[2] = 0x04; // Message Source - Host
            scanner.request[3] = 0x00; // Status
            byte[] checksum = CalculateChecksum(scanner, 4);
            scanner.request[4] = checksum[0];
            scanner.request[5] = checksum[1];
            return (SendReceive(scanner, (byte)DecoderCommands.REQUEST_REVISION, 6));
        }

        private byte Scan_Enable(DS457 scanner)
        {
            scanner.request[0] = 0x04; //Length of message not including Checksum.
            scanner.request[1] = 0xE9; //OPCODE - SCAN_ENABLE
            scanner.request[2] = 0x04; //Message Source - Host
            scanner.request[3] = 0x00; // Status
            byte[] checksum = CalculateChecksum(scanner, 4);
            scanner.request[4] = checksum[0];
            scanner.request[5] = checksum[1];
            return (SendReceive(scanner, (byte)DecoderCommands.SCAN_ENABLE, 6));
        }

        private byte Start_Session(DS457 scanner)
        {
            scanner.request[0] = 0x04; //Length of message not including Checksum.
            scanner.request[1] = 0xE4; //OPCODE - START_SESSION
            scanner.request[2] = 0x04; //Message Source - Host
            scanner.request[3] = 0x00; // Status
            byte[] checksum = CalculateChecksum(scanner, 4);
            scanner.request[4] = checksum[0];
            scanner.request[5] = checksum[1];
            return (SendReceive(scanner, (byte)DecoderCommands.START_SESSION, 6));
        }

        private byte Cmd_Ack(DS457 scanner)
        {
            scanner.request[0] = 0x04; //Length of message not including Checksum.
            scanner.request[1] = 0xD0; //OPCODE - START_SESSION
            scanner.request[2] = 0x04; //Message Source - Host
            scanner.request[3] = 0x00; //Status
            byte[] checksum = CalculateChecksum(scanner, 4);
            scanner.request[4] = checksum[0];
            scanner.request[5] = checksum[1];
            return (SendReceive(scanner, (byte)DecoderCommands.CMD_ACK, 6));
        }

        private byte Cmd_Nak(DS457 scanner)
        {
            scanner.request[0] = 0x05; //Length of message not including Checksum.
            scanner.request[1] = 0xD1; //OPCODE - START_SESSION
            scanner.request[2] = 0x04; //Message Source - Host
            scanner.request[3] = 0x00; //Status
            scanner.request[4] = 0x01; //Cause
            byte[] checksum = CalculateChecksum(scanner, 5);
            scanner.request[5] = checksum[0];
            scanner.request[6] = checksum[1];
            return (SendReceive(scanner, (byte)DecoderCommands.CMD_NAK, 7));
        }

        private byte Stop_Session(DS457 scanner)
        {
            scanner.request[0] = 0x04; //Length of message not including Checksum.
            scanner.request[1] = 0xE5; //OPCODE - STOP_SESSION
            scanner.request[2] = 0x04; //Message Source - Host
            scanner.request[3] = 0x00; // Status
            byte[] checksum = CalculateChecksum(scanner, 4);
            scanner.request[4] = checksum[0];
            scanner.request[5] = checksum[1];
            return (SendReceive(scanner, (byte)DecoderCommands.STOP_SESSION, 6));
        }

        private byte Param_Send(DS457 scanner, byte paramNum, byte paramValue)
        {
            scanner.request[0] = 0x07; //Length of message not including Checksum.
            scanner.request[1] = 0xC6; //OPCODE - PARAM_SEND
            scanner.request[2] = 0x04; //Message Source - Host
            scanner.request[3] = 0x00; // Status
            scanner.request[4] = 0xFF; // Beep Code - No beep.
            scanner.request[5] = paramNum;
            scanner.request[6] = paramValue;
            byte[] checksum = CalculateChecksum(scanner, 7);
            scanner.request[7] = checksum[0];
            scanner.request[8] = checksum[1];
            return (SendReceive(scanner, (byte)DecoderCommands.PARAM_SEND, 9));
        }

        private void Init(bool reconnect)
        {
            if (!simulateDevice)
            {
                multiVend = Convert.ToInt16(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["MULTIVENDMACHINE"]) ? "1" : ConfigurationManager.AppSettings["MULTIVENDMACHINE"]);
                for (short counter = 0; counter < multiVend; counter++)
                {
                    bool loopFlag = true;
                    DS457 scanner;
                    // Create a new SerialPort object with default settings.
                    scanner = new DS457();
                    scanner._serialPort = new SerialPort();

                    // Allow the user to set the appropriate properties.
                    scanner._serialPort.PortName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["DECODERCOMPORT" + counter.ToString()]) ? "com" + (++counter).ToString() : ConfigurationManager.AppSettings["DECODERCOMPORT" + counter.ToString()];
                    scanner._serialPort.BaudRate = int.Parse(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["DECODERBAUDRATE"]) ? "9600" : ConfigurationManager.AppSettings["DECODERBAUDRATE"]);
                    scanner._serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["DECODERPARITY"]) ? "None" : ConfigurationManager.AppSettings["DECODERPARITY"]);
                    scanner._serialPort.DataBits = int.Parse(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["DECODERDATABITS"]) ? "8" : ConfigurationManager.AppSettings["DECODERDATABITS"]);
                    scanner._serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["DECODERSTOPBITS"]) ? "1" : ConfigurationManager.AppSettings["DECODERSTOPBITS"]);
                    scanner._serialPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["DECODERHANDSHAKE"]) ? "None" : ConfigurationManager.AppSettings["DECODERHANDSHAKE"]);
                    scanner._serialPort.WriteTimeout = int.Parse(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["DECODERWRITETIMEOUT"]) ? "3" : ConfigurationManager.AppSettings["DECODERWRITETIMEOUT"]) * 1000;
                    scanner._serialPort.ReadTimeout = int.Parse(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["DECODERREADTIMEOUT"]) ? "5" : ConfigurationManager.AppSettings["DECODERREADTIMEOUT"]) * 1000;
                    scanner.stateCode = (int)DecoderState.ONLINE;
                    scanner.statusCode = (int)DecoderStatus.Initializing;
                    scanner.statusDesc = "Initialization sequence happening.";

                    //tmrScanningSessionTimer = new System.Threading.Timer(new
                    //TimerCallback(this.tmrScanningSession_TimerCallback),
                    //null, System.Threading.Timeout.Infinite, 10 * 1000);
                    short loopCounter = 0;
                    while (loopFlag)
                    {
                        Open(scanner);
                        // Decode DataPacket Format
                        // Param #EEh
                        // Param Value Send Raw Decode Data 0x00
                        // Param Value Send Packeted Decode Data 0x01
                        // PARAM_SEND
                        if (Param_Send(scanner, 0xEE, 0x01) == 1)
                        {
                            // Trigger Mode
                            // Param #8Ah
                            // Param Value Level 0x00
                            // Param Value Presentation Mode 0x07
                            // Param Value Host 0x08
                            // Param Value Software Trigger Only Mode 0x0F
                            // PARAM_SEND
                            loopCounter = 0;
                            if (Param_Send(scanner, 0x8A, 0x08) == 1)
                            {
                                // Enable Scanning
                                //if (Scan_Enable() == 1)
                                //{
                                using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                                {
                                    StreamWriter writer = new StreamWriter(stream);
                                    writer.WriteLine(string.Format(@"Datetime {0} -- Scanner mapped to {1} is initiallized successfully.", DateTime.Now, scanner._serialPort.PortName));
                                    writer.Flush();
                                    writer.Close();
                                }
                                scanner.id = counter;
                                scanner.statusCode = (int)DecoderStatus.NoError;
                                scanner.statusDesc = "Decoder Initialized Successfully.";
                                scanners.Add(scanner);
                                loopFlag = false;
                                //}
                                //else
                                //    this.statusCode = (int)DecoderStatus.Error;
                            }
                            else
                            {
                                scanner.statusCode = (int)DecoderStatus.Error;
                                scanner.statusDesc = "Invalid Checksum error received.";
                                using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                                {
                                    StreamWriter writer = new StreamWriter(stream);
                                    writer.WriteLine(string.Format(@"Datetime {0} -- Setting TriggerMode for scanner - {1} || state - {2} || status - {3} || Desc - {4}.", DateTime.Now, scanner._serialPort.PortName, scanner.stateCode, scanner.statusCode, scanner.statusDesc));
                                    writer.Flush();
                                    writer.Close();
                                }
                                scanner._serialPort.Close();
                                loopCounter++;
                                Thread.Sleep(2000);
                            }
                        }
                        else
                        {
                            scanner.statusCode = (int)DecoderStatus.Error;
                            scanner.statusDesc = "Invalid Checksum error received.";
                            using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                            {
                                StreamWriter writer = new StreamWriter(stream);
                                writer.WriteLine(string.Format(@"Datetime {0} -- Setting DataPAcket Fromat for scanner - {1} || state - {2} || status - {3} || Desc - {4}.", DateTime.Now, scanner._serialPort.PortName, scanner.stateCode, scanner.statusCode, scanner.statusDesc));
                                writer.Flush();
                                writer.Close();
                            }

                            scanner._serialPort.Close();
                            loopCounter++;
                            Thread.Sleep(2000);
                        }
                        if (loopCounter == 3)
                            loopFlag = false;
                    }
                }

            }
            else
            {
                this.stateCode = (int)DecoderState.ONLINE;
                this.statusCode = (int)DecoderStatus.NoError;
                this.statusDesc = "Decoder Initialized Successfully.";
            }
        }

        private void StartScanning(short id)
        {

            if (!simulateDevice)
            {
                //foreach (DS457 item in scanners)
                //{
                //    using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                //    {
                //        StreamWriter writer = new StreamWriter(stream);
                //        writer.WriteLine(string.Format(@"Datetime {0} -- Before Invoking StartSession on Scanner - {1}.", DateTime.Now, item._serialPort.PortName));
                //        writer.Flush();
                //        writer.Close();
                //    }
                    if (Start_Session(scanners[id]) == 1)
                    {
                        using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- Enabling Session succeed on Scanner - {1}.", DateTime.Now, scanners[id]._serialPort.PortName));
                            writer.Flush();
                            writer.Close();
                        }
                        scanners[id].sessionInProgress = true;
                    }
                //    else
                //    {
                //        using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                //        {
                //            StreamWriter writer = new StreamWriter(stream);
                //            writer.WriteLine(string.Format(@"Datetime {0} -- Enabling Session failed on Scanner - {1}.", DateTime.Now, item._serialPort.PortName));
                //            writer.Flush();
                //            writer.Close();
                //        }
                //    }

                //}
            }
            else
            {
                this.stateCode = (int)DecoderState.ONLINE;
                this.statusCode = (int)DecoderStatus.NoError;
                this.statusDesc = "Decoder Ready.";
            }
        }
        private void StopScanning()
        {
            if (!simulateDevice)
            {
                foreach (DS457 item in scanners)
                {
                    Stop_Session(item);
                }
            }
            else
            {
                this.stateCode = (int)DecoderState.ONLINE;
                this.statusCode = (int)DecoderStatus.NoError;
                this.statusDesc = "Decoder Ready.";
            }
        }

        //private void tmrScanningSession_TimerCallback(object state)
        //{
        //    try
        //    {
        //        //_serialPort.DataReceived -= new SerialDataReceivedEventHandler(Read);
        //        //Manually stop the timer...
        //        tmrScanningSessionTimer.Change(Timeout.Infinite, Timeout.Infinite);

        //        scanner.sessionInProgress = false;

        //    }
        //    catch (Exception ex)
        //    {
        //        //if (log.IsErrorEnabled) log.Info("Error in tmrPrintLog_TimerCallback.. " + ex.Message.ToString());
        //    }
        //    finally
        //    {

        //        //if (log.IsInfoEnabled) log.Info("tmrPrinttLog_TimerCallback execution finished..");
        //    }

        //}

        private void Write(DS457 scanner, short reqLen)
        {
            short ctr;
            try
            {
                scanner._serialPort.RtsEnable = true;
                scanner._serialPort.Write(scanner.request, 0, reqLen);
                for (ctr = 0; ctr < scanner.request.Length; ctr++)
                {
                    scanner.reply[ctr] = 0;
                }
                scanner.statusCode = (int)DecoderStatus.NoError;
            }
            catch (TimeoutException tex)
            {
                //this.statusCode = (int)DecoderStatus.Error;
                using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- Error in Write. Message - {1}|| Decoder - {2} .", DateTime.Now, tex.Message, scanner._serialPort.PortName));
                    writer.Flush();
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                scanner.statusCode = (int)DecoderStatus.Error;
                scanner.statusDesc = "Exception - " + ex.Message;
                using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- Error in Write. Message - {1}|| Decoder - {2} .", DateTime.Now, ex.Message, scanner._serialPort.PortName));
                    writer.Flush();
                    writer.Close();
                }
            }
        }

        public byte SendReceive(DS457 scanner, byte command, short reqLen)
        {
            int ctr, iter = 0;
            bool loopFlag = true;
            try
            {
                while (loopFlag)
                {
                    iter++;
                    if (iter == 4)
                        break;
                    if (iter > 1)
                    {
                        scanner.request[3] = 1;
                        byte[] checksum = CalculateChecksum(scanner, (short)(reqLen - 2));
                        scanner.request[reqLen - 2] = checksum[0];
                        scanner.request[reqLen - 1] = checksum[1];
                    }
                    using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Before Issuing Command {1} for Decoder - {2}.", DateTime.Now, scanner.request[1], scanner._serialPort.PortName));
                        writer.Flush();
                        writer.Close();
                    }

                    Write(scanner, reqLen);

                    loopFlag = false;
                    switch (command)
                    {
                        case (byte)DecoderCommands.REQUEST_REVISION:
                            // REPLY_REVISION expected from the Decoder.
                            // If Write is successful then only issue Read.
                            Read(scanner, null, null);
                            // If Read is successful then proceed further.
                            break;

                        case (byte)DecoderCommands.SCAN_ENABLE:
                            // CMD_ACK/CMD_NAK expected from the decoder.
                            Read(scanner, null, null);
                            //if(reply[1] == 0xD0)
                            // Notify App of the Enable Scanning Event.
                            if ((InterpretResponse(scanner)) && (iter < 4))
                                loopFlag = true;

                            /*************/
                            // Publish the event indicating scanning has been enabled.
                            if (loopFlag)
                            {
                                Action handler = ScanEnabledEvent;
                                if (handler != null)
                                {
                                    using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                                    {
                                        StreamWriter writer = new StreamWriter(stream);
                                        writer.WriteLine(string.Format(@"Datetime {0} -- Publishing ScanEnabledEvent.", DateTime.Now));
                                        writer.Flush();
                                        writer.Close();
                                    }
                                    handler();
                                }
                            }

                            break;

                        case (byte)DecoderCommands.START_SESSION:
                            // CMD_ACK/CMD_NAK expected from the decoder.
                            Read(scanner, null, null);
                            using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                            {
                                StreamWriter writer = new StreamWriter(stream);
                                writer.WriteLine(string.Format(@"Datetime {0} -- StartSession {1}{2}{3}{4}{5}{6}.", DateTime.Now, scanner.reply[0], scanner.reply[1], scanner.reply[2], scanner.reply[3], scanner.reply[4], scanner.reply[5]));
                                writer.Flush();
                                writer.Close();
                            }

                            //if (reply[1] == 0x00)
                            // Notify App of the Start Scanning Event.
                            if ((InterpretResponse(scanner)) && (iter < 4))
                                loopFlag = true;

                            /************
                                if(loopFlag)
                                  Publish the event indicating scanning session has been started.
                                ************/
                            if (!loopFlag)
                            {
                                using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                                {
                                    StreamWriter writer = new StreamWriter(stream);
                                    writer.WriteLine(string.Format(@"Datetime {0} -- Before Issuing Read for Decode Event for Decoder {1}.", DateTime.Now, scanner._serialPort.PortName));
                                    writer.Flush();
                                    writer.Close();
                                }
                                loopFlag = false;
                                Action handler = TriggerPulledEvent;
                                if (handler != null)
                                {
                                    using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                                    {
                                        StreamWriter writer = new StreamWriter(stream);
                                        writer.WriteLine(string.Format(@"Datetime {0} -- Publishing TriggerPulledEvent.", DateTime.Now));
                                        writer.Flush();
                                        writer.Close();
                                    }
                                    handler();
                                }
                                Read(scanner, null, null);
                                //Action handler = ScanEnabledEvent;
                                
                            }
                            //Thread.Sleep(1000);
                            break;

                        case (byte)DecoderCommands.STOP_SESSION:
                            // CMD_ACK/CMD_NAK expected from the decoder.
                            Read(scanner, null, null);
                            //if(reply[1] == 0xD0)
                            // Notify App of the Stop Scanning Event.
                            if ((InterpretResponse(scanner)) && (iter < 4))
                                loopFlag = true;

                            if (loopFlag)
                                Thread.Sleep(1000);
                            /************
                            if(loopFlag)
                                Publish the event indicating scanning session has been stopped.
                            ************/
                            if (!loopFlag)
                            {
                                //Action handler = ScanEnabledEvent;
                                Action handler = TriggerReleasedEvent;
                                if (handler != null)
                                {
                                    using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                                    {
                                        StreamWriter writer = new StreamWriter(stream);
                                        writer.WriteLine(string.Format(@"Datetime {0} -- Publishing TriggerReleasedEvent.", DateTime.Now));
                                        writer.Flush();
                                        writer.Close();
                                    }
                                    handler();
                                }
                            }
                            break;

                        case (byte)DecoderCommands.CMD_ACK:
                            // No Reply expected from the decoder.

                            break;

                        case (byte)DecoderCommands.CMD_NAK:
                            // No Reply expected from the decoder.

                            break;

                        case (byte)DecoderCommands.SCAN_DISABLE:
                            // CMD_ACK/CMD_NAK expected from the decoder.
                            Read(scanner, null, null);
                            //if(reply[1] == 0xD0)
                            // Notify App of the Disable Scanning Event.
                            if ((InterpretResponse(scanner)) && (iter < 3))
                                loopFlag = true;
                            break;

                        case (byte)DecoderCommands.PARAM_SEND:
                            // CMD_ACK/CMD_NAK expected from the decoder.
                            Read(scanner, null, null);
                            //if(reply[1] == 0xD0)
                            // Notify App of the Disable Scanning Event.
                            //Thread.Sleep(2000);
                            if ((InterpretResponse(scanner)) && (iter < 4))
                                loopFlag = true;
                            break;

                        case (byte)DecoderCommands.WAKEUP:
                            // No Reply expected from the decoder.

                            break;

                        default:

                            break;
                    }
                    Thread.Sleep(300);
                }
            }
            catch (TimeoutException tex)
            {
                scanner.statusCode = (int)DecoderStatus.Error;
                this.statusDesc = "Exception - " + tex.Message;
            }
            catch (Exception ex)
            {
                scanner.statusCode = (int)DecoderStatus.Error;
                scanner.statusDesc = "Exception - " + ex.Message;
                Console.WriteLine("Exception - " + ex.Message);
            }
            finally
            {
                // Initialize Request and Response buffers.
                // JK - Need to find a better way to initializa the buffers in c#.
                for (ctr = 0; ctr < scanner.request.Length; ctr++)
                {
                    scanner.request[ctr] = scanner.reply[ctr] = 0;
                }

            }
            if ((loopFlag) && (iter == 4))
            {
                using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- All three attempts to issue command failed.", DateTime.Now));
                    writer.Flush();
                    writer.Close();
                }
                //scanner.statusCode = (int)DecoderStatus.Error;
                //this.statusDesc = "Command can't be processed successfully";
                return 0;
            }
            else
            {
                scanner.statusCode = (int)DecoderStatus.NoError;
                //this.statusDesc = "Command processed successfully";
                return 1;
            }
        }

        public void Read(DS457 scanner, object sender, SerialDataReceivedEventArgs e)
        {
            //byte[] barcode = new byte[100];
            string cardid;
            bool startOfResponse = true;
            int replyLength = -1;
            int replyOffset = 0;
            if (!scanner.readInProgress)
            {
                try
                {
                    scanner.readInProgress = true;
                    using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Before Issuing Read for Command {1} on Decoder - {2}.", DateTime.Now, scanner.request[1], scanner._serialPort.PortName));
                        writer.Flush();
                        writer.Close();
                    }
                    scanner._serialPort.RtsEnable = false;
                    int readLen;
                    while (replyLength != replyOffset)
                    {

                        if (startOfResponse)
                        {
                            readLen = scanner._serialPort.Read(scanner.reply, 0, 1);
                            replyLength = scanner.reply[0] + 2;
                            replyOffset += readLen;
                            startOfResponse = false;
                            using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                            {
                                StreamWriter writer = new StreamWriter(stream);
                                writer.WriteLine(string.Format(@"Datetime {0} -- ReadLength - {1} ---- ReplyLength - {2}.---- ReplyOffset - {3} ---- Request - {4}", DateTime.Now, readLen, replyLength, replyOffset, scanner.request[1]));
                                writer.Flush();
                                writer.Close();
                            }
                        }
                        else
                        {
                            readLen = scanner._serialPort.Read(scanner.reply, replyOffset, replyLength - replyOffset);
                            replyOffset += readLen;
                            using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                            {
                                StreamWriter writer = new StreamWriter(stream);
                                writer.WriteLine(string.Format(@"Datetime {0} -- ReadLength - {1} ---- ReplyLength - {2}.---- ReplyOffset - {3} ---- Request - {4}", DateTime.Now, readLen, replyLength, replyOffset, scanner.request[1]));
                                writer.Flush();
                                writer.Close();
                            }
                        }
                        // end of reply reached
                        if (replyOffset == replyLength)
                        {
                            break;
                            //InterpretResponse(replyLength);
                        }

                    }

                    using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Response for Request - {1}.---- {2}{3}{4}{5}{6}{7}", DateTime.Now, scanner.request[1], scanner.reply[1], scanner.reply[2], scanner.reply[3], scanner.reply[4], scanner.reply[5], scanner.reply[6]));
                        writer.Flush();
                        writer.Close();
                    }

                    // In case of DECODE_DATA the decoder sends unsolicited data.
                    // We need to treat this in a special manner.
                    if (scanner.reply[1] == 0xF3)
                    {
                        cardid = System.Text.Encoding.UTF8.GetString(scanner.reply, 5, scanner.reply[0] - 5);
                        // Publish Decode Event as the card id has been scanned successfully.
                        // Issue CMD_ACK.
                        Cmd_Ack(scanner);
                        Action<string> handler = BarcodeScannedEvent;
                        if (handler != null)
                        {
                            using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                            {
                                StreamWriter writer = new StreamWriter(stream);
                                writer.WriteLine(string.Format(@"Datetime {0} -- Publishing BarcodeScannedEvent - Card ID {1}.", DateTime.Now, cardid));
                                writer.Flush();
                                writer.Close();
                            }
                            handler(cardid);
                        }
                    }
                }
                catch (TimeoutException tex)
                {
                    using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Error in Read - Message {1}.", DateTime.Now, tex.Message));
                        writer.Flush();
                        writer.Close();
                    }
                    //this.statusCode = (int)DecoderStatus.Error;
                    scanner.statusDesc = "Exception - " + tex.Message;
                }
                catch (Exception ex)
                {
                    scanner.statusCode = (int)DecoderStatus.Error;
                    scanner.statusDesc = "Exception - " + ex.Message;
                    using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Error in Read - Message {1}.", DateTime.Now, ex.Message));
                        writer.Flush();
                        writer.Close();
                    }
                }
                finally
                {
                    scanner.readInProgress = false;
                }

            }
        }

        private bool InterpretResponse(DS457 scanner)
        {
            bool flag = false;
            using (FileStream stream = new FileStream(@"SymbolDS457.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(string.Format(@"Datetime {0} -- InterpretResponse for Request - {1}.---- Response - {2}", DateTime.Now, scanner.request[1], scanner.reply[1]));
                writer.Flush();
                writer.Close();
            }
            switch (scanner.reply[1])
            {
                case 0xD1:
                    Console.WriteLine("Command execution Failed.");
                    // Check for the cause if cause is Checksum Failure then Resend.
                    if (scanner.reply[4] == 0x01)
                    {
                        flag = true;
                    }
                    break;
                //case 0x00:
                //    flag = true;
                //    break;
            }
            return flag;
        }

        public void OpError(object sender, SerialErrorReceivedEventArgs e)
        {
            Console.WriteLine("Exception Caught : " + e.ToString());
        }
        #endregion
    }
}






