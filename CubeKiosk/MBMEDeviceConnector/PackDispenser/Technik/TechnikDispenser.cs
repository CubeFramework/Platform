using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.Interfaces;
using Technik;
using System.Configuration;
using System.Threading;
using System.IO;

namespace MBMEDevices.PackDispenser
{
    internal delegate void DInit(bool reconnect);
    internal delegate void DVend(short number);
    public class PackVendor
    {
        public Dispenser packDispenser = null;
        public short id;
        public string com;
        // Pack Dispenser State/Status/Status Desc...
        public int stateCode;
        public int statusCode;
        public string statusDesc;

        public PackVendor()
        {
            packDispenser = new Dispenser();
        }
    }

    public class TechnikDispenser : IPackDispenser
    {
        // Async method call delegates.
        DInit initMethod;
        DVend vendMethod;

        // Card Reader Simulation related fields.
        private static bool simulateDevice;
        private static Timer initTimer;
        private static Timer simulationTimer;
        private static bool isInitialized;
        private static bool isEnabled;
        private short multiVend = 0;
        private string[] com;
        private short waitTime;
        private short numCards;
        private short cardIssued;
        private static List<PackVendor> dispensers;

        // Multi Vend Device State/Status/Status Desc...
        public int stateCode;
        public int statusCode;
        public string statusDesc;

        /// <summary>
        /// Event to prompt the User to Swipe(INSERT/REMOVE)) Card/Enter PIN /ReEnter PIN.
        /// </summary>
        ////public event Action CardVendStatusEvent;
        public event Action<bool> CardVendStatusEvent;

        ////public event Action VendCompletionStatusEvent;
        public event Action VendCompletionStatusEvent;

        ////public event Action InitiateVendEvent;
        public event Action<short> InitiateVendEvent;
        public TechnikDispenser()
        {
            // Capability to manage maximum 5 m/c's.
            dispensers = new List<PackVendor>();
        }

        /// <summary>
        /// Initializes the card reader
        /// </summary>
        public void InitAsync(bool simulatePackDispenser)
        {

            if (!isInitialized)
            {
                isInitialized = true;
                simulateDevice = simulatePackDispenser;

                using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- InitAsync called with simulatedmode flag set to {1}.", DateTime.Now, simulateDevice));
                    writer.Flush();
                    writer.Close();
                }
                if (!simulateDevice)
                {
                    // Initialize delegate methods only once in application life cycle.
                    initMethod = new DInit(this.Init);

                    this.statusCode = (int)DispenserStatus.Initializing;
                    // Initialize the cash acceptor.
                    initMethod.BeginInvoke(false, null, null);
                }
            }
        }

        public bool VendAsync(short number)
        {

            if (isInitialized)
            {
                using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- VendAsync called with {1} card/s to vend.", DateTime.Now, number));
                    writer.Flush();
                    writer.Close();
                }

                numCards = number;
                cardIssued = 0;
                if (!simulateDevice)
                {
                    // Initialize delegate methods only once in application life cycle.
                    vendMethod = new DVend(this.MultiVend);
                    // Initiating Card Pack Vending.
                    vendMethod.BeginInvoke(number, null, null);
                }
                else
                {
                    if (simulationTimer == null)
                    {
                        simulationTimer = new Timer(new TimerCallback(SimulatePackVending), null, Timeout.Infinite, Timeout.Infinite);
                    }
                    //int counter=0;
                    //Action<bool> handler = CardVendStatusEvent;
                    //while (counter < number)
                    //{
                    //    Thread.Sleep(7000);
                    //    using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                    //    {
                    //        StreamWriter writer = new StreamWriter(stream);
                    //        writer.WriteLine(string.Format(@"Datetime {0} -- VendAsync - Else Called.Counter - {1}|||| Number {2}", DateTime.Now, counter,number));
                    //        writer.Flush();
                    //        writer.Close();
                    //    }

                    //    try
                    //    {

                    //        if (handler != null)
                    //        {
                    //            handler(true);
                    //            using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                    //            {
                    //                StreamWriter writer = new StreamWriter(stream);
                    //                writer.WriteLine(string.Format(@"Datetime {0} -- Vend - CardVend Status Handler Called. Vend Status : {1}", DateTime.Now, packDispenser.Success));
                    //                writer.Flush();
                    //                writer.Close();
                    //            }
                    //        }
                    //        else
                    //        {
                    //            using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                    //            {
                    //                StreamWriter writer = new StreamWriter(stream);
                    //                writer.WriteLine(string.Format(@"Datetime {0} -- VendAsync - Else Called.Handler is null", DateTime.Now));
                    //                writer.Flush();
                    //                writer.Close();
                    //            }


                    //        }
                    //    }
                    //    catch(Exception ex)
                    //    {
                    //        using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                    //        {
                    //            StreamWriter writer = new StreamWriter(stream);
                    //            writer.WriteLine(string.Format(@"Datetime {0} -- VendAsync - Else Called.Exception {1}", DateTime.Now,ex.Message));
                    //            writer.Flush();
                    //            writer.Close();
                    //        }
                    //    }
                    //    counter++;
                    //}
                    //Action completionHandler = VendCompletionStatusEvent;
                    //if (completionHandler != null)
                    //{
                    //    completionHandler();
                    //    using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                    //    {
                    //        StreamWriter writer = new StreamWriter(stream);
                    //        writer.WriteLine(string.Format(@"Datetime {0} -- Vend - Completion Handler Called.", DateTime.Now));
                    //        writer.Flush();
                    //        writer.Close();
                    //    }
                    //}
                    using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Vending simulation starting.", DateTime.Now));
                        writer.Flush();
                        writer.Close();
                    }
                    try
                    {
                        simulationTimer.Change(2000, 2000);
                    }
                    catch (Exception ex)
                    {
                        using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- Vending simulation ended in exception. Message - {1}", DateTime.Now, ex.Message));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                }
            }

            return true;
        }


        // Changes done By JK on 9th Apr, 2013 to enable this class to control multiple Vend Machines.
        private void Init(bool reconnect)
        {
            short counter = 0;
            if (simulateDevice)
            {
                isEnabled = true;
                this.stateCode = (int)DispenserCommState.ONLINE;
                this.statusCode = (int)DispenserStatus.NoError;
                if (simulationTimer == null)
                    simulationTimer = new Timer(new TimerCallback(SimulatePackVending), null, Timeout.Infinite, Timeout.Infinite);
            }
            else
            {
                multiVend = Convert.ToInt16(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["MULTIVENDMACHINE"]) ? "1" : ConfigurationManager.AppSettings["MULTIVENDMACHINE"]);
                waitTime = Convert.ToInt16(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["OPERATIONTIMEOUT"]) ? "1500" : ConfigurationManager.AppSettings["OPERATIONTIMEOUT"]);
                using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- Number of PackDispensers to be Initialized - {1}.", DateTime.Now, multiVend));
                    writer.Flush();
                    writer.Close();
                }
                for (counter = 0; counter < multiVend; counter++)
                {
                    // Instaniate new Pack Vendor object
                    PackVendor pv = new PackVendor();

                    pv.com = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["PACKDISPENSERCOMPORT" + counter.ToString()]) ? counter.ToString() : ConfigurationManager.AppSettings["PACKDISPENSERCOMPORT" + counter.ToString()];
                    using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- PackDispenser Init on COM{1}.", DateTime.Now, pv.com));
                        writer.Flush();
                        writer.Close();
                    }
                    // Set comm port 
                    pv.packDispenser.CommPort = Convert.ToInt16(pv.com);
                    // Attempt to open port
                    pv.packDispenser.PortOpen = true;

                    try
                    {
                        // Check if the comm port is open to continue
                        if (pv.packDispenser.PortOpen)
                        {
                            // Wait for dispenser to complete processing now we have connected
                            System.Threading.Thread.Sleep(1000);
                            // Refresh all dispenser properties
                            pv.packDispenser.UpdateStatus(waitTime);
                            // Check that we did refresh successfully
                            if (pv.packDispenser.Success)
                            {
                                // Reset the unit
                                pv.packDispenser.Reset();
                                // Update status for the user
                                CheckStatus(pv);
                                using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                                {
                                    StreamWriter writer = new StreamWriter(stream);
                                    writer.WriteLine(string.Format(@"Datetime {0} -- Pack Dispenser Successfully Initialized on COM{1}.", DateTime.Now, pv.com));
                                    writer.Flush();
                                    writer.Close();
                                }
                            }
                            else
                            {
                                // Error last command was not a success
                                pv.stateCode = (int)DispenserCommState.OFFLINE;
                                pv.statusCode = (int)DispenserStatus.Error;
                                pv.statusDesc = "Pack Dispenser Fatal Error.";
                                using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                                {
                                    StreamWriter writer = new StreamWriter(stream);
                                    writer.WriteLine(string.Format(@"Datetime {0} -- Init- UpdateStatus operation failed.", DateTime.Now));
                                    writer.Flush();
                                    writer.Close();
                                }
                            }

                        }
                        else
                        {
                            // Error last command was not a success
                            pv.stateCode = (int)DispenserCommState.OFFLINE;
                            pv.statusCode = (int)DispenserStatus.Error;
                            pv.statusDesc = "Port Not Open.";
                            using (FileStream stream = new FileStream(@"Technik.log", FileMode.Open, FileAccess.Read))
                            {
                                StreamWriter writer = new StreamWriter(stream);
                                writer.WriteLine(string.Format(@"Datetime {0} -- COM{1} port not found/open.", DateTime.Now, pv.com));
                                writer.Flush();
                                writer.Close();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Error last command was not a success
                        pv.stateCode = (int)DispenserCommState.OFFLINE;
                        pv.statusCode = (int)DispenserStatus.Error;
                        pv.statusDesc = "Pack Dispenser Fatal Error.";
                        using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- Init - Exception Caught - Message {1}.", DateTime.Now, ex.Message));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                    pv.id = counter;
                    dispensers.Add(pv);
                }



            }
        }


        private void MultiVend(short number)
        {

            foreach (PackVendor item in dispensers)
            {
                using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- Attempting to Vend Cards#{1} from com{2}.", DateTime.Now, number, item.com));
                    writer.Flush();
                    writer.Close();
                }
                if (cardIssued < numCards)
                    Vend(item, (short)(number - cardIssued));
            }
            Action completionHandler = VendCompletionStatusEvent;
            if (completionHandler != null)
            {
                completionHandler();
                using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- Vend - Completion Handler Called.", DateTime.Now));
                    writer.Flush();
                    writer.Close();
                }
            }
        }
        private void Vend(PackVendor pv, short number)
        {
            short counter = 0;
            try
            {
                pv.packDispenser.UpdateStatus();
                CheckStatus(pv);

                if ((number > 0) && (!simulateDevice))
                {
                    for (counter = 0; counter < number; counter++)
                    {
                        using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- Attempting to Vend Card#{1}.", DateTime.Now, counter));
                            writer.Flush();
                            writer.Close();
                        }
                        if ((pv.packDispenser.PortOpen) && (pv.packDispenser.Success))
                        {
                            if ((statusCode == (int)DispenserStatus.NoError) && (stateCode == (int)DispenserCommState.ONLINE))
                            {
                                using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                                {
                                    StreamWriter writer = new StreamWriter(stream);
                                    writer.WriteLine(string.Format(@"Datetime {0} -- Issuing Vend call for Card#{1}.", DateTime.Now, counter));
                                    writer.Flush();
                                    writer.Close();
                                }

                                Action<short> handler = InitiateVendEvent;
                                if (handler != null)
                                {
                                    handler(pv.id);
                                    using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                                    {
                                        StreamWriter writer = new StreamWriter(stream);
                                        writer.WriteLine(string.Format(@"Datetime {0} -- Publishing Initiate Vend Event- com{1}", DateTime.Now, pv.com));
                                        writer.Flush();
                                        writer.Close();
                                    }
                                }
                                pv.packDispenser.Vend();

                                //if(packDispenser.Success)
                                // raise an event to notify the application that a card has been sucessfully dispensed.
                                Action<bool> handler1 = CardVendStatusEvent;
                                if (handler != null)
                                {
                                    handler1(pv.packDispenser.Success);
                                    using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                                    {
                                        StreamWriter writer = new StreamWriter(stream);
                                        writer.WriteLine(string.Format(@"Datetime {0} -- Vend - CardVend Status Handler Called. Vend Status : {1}", DateTime.Now, pv.packDispenser.Success));
                                        writer.Flush();
                                        writer.Close();
                                    }
                                    if (pv.packDispenser.Success)
                                      cardIssued++;
                                }
                                Thread.Sleep(3000);

                            }
                            else
                            {
                                break;
                            }

                            // Recheck to ensure that was not the last product or a jam has occured
                            pv.packDispenser.UpdateStatus();
                            CheckStatus(pv);
                            if (0 != pv.packDispenser.Status)
                              break;
                        }
                        else
                        {
                            break;
                        }

                    }

                }
                else
                {
                    simulationTimer.Change(5000, 5000);
                }
            }
            catch (Exception ex)
            {
                using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- Vend - Exception Caught Message {1}.", DateTime.Now, ex.Message));
                    writer.Flush();
                    writer.Close();
                }
                pv.stateCode = (int)DispenserCommState.ONLINE;
                pv.statusCode = (int)DispenserStatus.Error;
                pv.statusDesc = "Pack Dispenser Fatal Error.";
            }
        }

        public string GetDetails(out int state, out int status)
        {
            IsReady();
            state = this.stateCode;
            status = this.statusCode;

            return statusDesc;

        }

        private void CheckStatus(PackVendor packVendor)
        {

            try
            {
                // Check the dispenser status and update user interface
                if (packVendor.packDispenser.Success && packVendor.packDispenser.Error == 0)
                {
                    switch (packVendor.packDispenser.Status)
                    {
                        case 0:
                            packVendor.statusCode = (int)DispenserStatus.NoError;
                            packVendor.stateCode = (int)DispenserCommState.ONLINE;
                            packVendor.statusDesc = " OK.  Dispenser is ready.";
                            break;
                        case 1:
                            packVendor.statusCode = (int)DispenserStatus.Error;
                            packVendor.stateCode = (int)DispenserCommState.ONLINE;
                            packVendor.statusDesc = "Pack Dispenser Stock Sold Out. No Product is available in any column.";
                            break;
                        case 2:
                            packVendor.statusCode = (int)DispenserStatus.Error;
                            stateCode = (int)DispenserCommState.ONLINE;
                            statusDesc = "Pack Dispenser Down. No Column is able to Dispense.";
                            break;
                        case 13:
                            packVendor.statusCode = (int)DispenserStatus.Error;
                            packVendor.stateCode = (int)DispenserCommState.ONLINE;
                            packVendor.statusDesc = "Product Jam.";
                            break;

                        case 14:
                            packVendor.statusCode = (int)DispenserStatus.Error;
                            packVendor.stateCode = (int)DispenserCommState.ONLINE;
                            packVendor.statusDesc = "Product sensed in wrong column (for multi-column dispensers).  Can also be caused by inserting an object through the front of the dispenser in an attempt to steal product.";
                            break;

                        default:
                            packVendor.statusCode = (int)DispenserStatus.Error;
                            packVendor.stateCode = (int)DispenserCommState.ONLINE;
                            packVendor.statusDesc = "Pack Dispenser Fatal Error.";
                            break;
                    }
                }
                else
                {
                    packVendor.statusCode = (int)DispenserStatus.Error;
                    packVendor.stateCode = (int)DispenserCommState.ONLINE;
                    packVendor.statusDesc = "Pack Dispenser Fatal Error.";
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "The port is closed.")
                {
                    packVendor.stateCode = (int)DispenserCommState.OFFLINE;
                    packVendor.statusCode = (int)DispenserStatus.Error;
                    packVendor.statusDesc = ex.Message;
                }
                else
                {
                    packVendor.statusDesc = "There is some problem with PackDispenser.";
                    packVendor.statusCode = (int)DispenserStatus.Error;
                }
            }
            using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(string.Format(@"Datetime {0} -- CheckStatus on COM{1} || state-{2} || status - {3} || statusDesc - {4} .", DateTime.Now, packVendor.com, packVendor.stateCode, packVendor.statusCode, packVendor.statusDesc));
                writer.Flush();
                writer.Close();
            }

        }

        public bool IsReady()
        {
            bool happystat = false;
            this.statusDesc = null;
            if (!simulateDevice)
            {
                foreach (PackVendor item in dispensers)
                {
                    try
                    {
                        item.packDispenser.UpdateStatus(waitTime);
                        CheckStatus(item);

                        if ((item.statusCode == (int)DispenserStatus.NoError) && (stateCode == (int)DispenserCommState.ONLINE))
                        {
                            this.stateCode = (int)DispenserCommState.ONLINE;
                            this.statusCode = (int)DispenserStatus.NoError;
                            this.statusDesc = "Ok. MultiVend Dispenser is Ready";
                            happystat = true;
                            using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                            {
                                StreamWriter writer = new StreamWriter(stream);
                                writer.WriteLine(string.Format(@"Datetime {0} -- PackDispenser IsReady on COM{1} || state-{2} || status - {3} .", DateTime.Now, item.com, item.stateCode, item.statusCode));
                                writer.Flush();
                                writer.Close();
                            }
                            break;
                        }
                        else
                        {
                            this.statusDesc += item.com + " - " + item.statusDesc;
                        }
                    }
                    catch (Exception ex)
                    {
                        using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- PackDispenser IsReady Exception on COM{1} || Exception - {2}.", DateTime.Now, item.com, ex.Message));
                            writer.Flush();
                            writer.Close();
                        }
                        if (ex.Message == "The port is closed.")
                        {
                            item.stateCode = (int)DispenserCommState.OFFLINE;
                            item.statusCode = (int)DispenserStatus.Error;
                            item.statusDesc = ex.Message;
                            this.statusDesc += item.com + " - " + item.statusDesc;
                        }
                        else
                        {
                            item.statusDesc = "There is some problem with PackDispenser.";
                            item.statusCode = (int)DispenserStatus.Error;
                            this.statusDesc += item.com + " - " + item.statusDesc;
                        }
                    }
                }
                if (!happystat)
                {
                    this.stateCode = (int)DispenserCommState.ONLINE;
                    this.statusCode = (int)DispenserStatus.Error;
                }
            }
            else
            {
                statusCode = 1;
                stateCode = 1;
                return true;
            }
            using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(string.Format(@"Datetime {0} -- PackDispenser || state-{1} || status - {2} .", DateTime.Now, this.stateCode, this.statusCode));
                writer.Flush();
                writer.Close();
            }
            return (happystat);

        }

        private void SimulatePackVending(object o)
        {
            using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(string.Format(@"Datetime {0} -- VendSimulation .", DateTime.Now));
                writer.Flush();
                writer.Close();
            }
            simulationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            // Simulate Card based Payment sequence.


            Action<bool> handler = CardVendStatusEvent;
            short counter;

            for (counter = 0; counter < numCards; counter++)
            {
                Thread.Sleep(3000);
                try
                {
                    if (handler != null)
                    {
                        handler(true);
                        using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- VendCardStatusEventCalled .", DateTime.Now));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- VendSimulation Exception - Message - {1}.", DateTime.Now, ex.Message));
                        writer.Flush();
                        writer.Close();
                    }
                }
            }

            Action completionHandler = VendCompletionStatusEvent;
            if (completionHandler != null)
            {
                using (FileStream stream = new FileStream(@"Technik.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- VendSimulationCompletion Called .", DateTime.Now));
                    writer.Flush();
                    writer.Close();
                }
                completionHandler();
            }

        }

    }

}






