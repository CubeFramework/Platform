using System;
using System.Collections.Generic;
using System.Text;
using MBMEKiosk.Infrastructure.Interfaces;
using System.Threading;
using System.Configuration;
using MBMEDevices.CardReader;
using System.IO;

namespace MBMEDevices.Readers
{
    internal delegate void DInit(bool reconnect);
    internal delegate void DPayment(double amount);
    // define own display class according to interface ECRDisplay
    public class Display : ECRDisplay
    {
        /// <summary>
        /// Event to prompt the User to Swipe(INSERT/REMOVE)) Card/Enter PIN /ReEnter PIN.
        /// </summary>
        ////public event Action SwipeCardEvent;
        public event Action<string> SwipeCardEvent;

        public Display(int w, int h)
            : base(w, h)
        {
        }

        // override printToDisplay to show display texts
        public override int printToDisplay(int x, int y, int mode, string text, EftTerminal terminal)
        {
            //Console.WriteLine("ECR display:");
            //Console.WriteLine(text);
            using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(string.Format(@"Datetime {0} -- printToDisplay : Text = {1}.", DateTime.Now, text));
                writer.Flush();
                writer.Close();
            }
            Action<string> handler = SwipeCardEvent;
            if (handler != null)
            {
                text = text.Replace('\n', ' ');
                handler(text);
            }
            return 0;
        }
    }

    // define own printer class according to interface ECRPrinter
    class Printer : ECRPrinter
    {
        /// <summary>
        /// Event to publish the terminal Receipt.
        /// </summary>
        ////public event Action ReceiptEvent;
        public event Action<string, string, string, string, string, string, string, string, string, string> ReceiptEvent;

        public Printer()
        {
        }

        //// override print to show printer texts
        //public override int print(string text, EftTerminal terminal)
        //{

        //    return 0;
        //}

        // override print to show printer texts
        public override int print(string receipt, EftTerminal terminal)
        {
            //Console.WriteLine("ECR printer:");
            //Console.WriteLine(text);
            string MerchantID = string.Empty;
            string TerminalID = string.Empty;
            string Authorization = string.Empty;
            string AID = string.Empty;
            string APPName = string.Empty;
            string TVR = string.Empty;
            string TSI = string.Empty;
            string ACInfo = string.Empty;
            string AC = string.Empty;
            string CARDNO = string.Empty;
            int first, last;

            if (string.IsNullOrEmpty(receipt))
                receipt = @"H$ Maxbox Middle East FZh$ Du Head office  Al Salam Tower, Dubai  DATE:18/12/13 TIME:10:46 MID: 985602000 TID: 00028867 Batch ID: 000001 <CARDHOLDER COPY> PURCHASE H$VISAh$466704******1299 EXPIRES 06/20";
 
            using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(string.Format(@"Datetime {0} -- print : Text = {1}.", DateTime.Now, receipt));
                writer.Flush();
                writer.Close();
            }
            try
            {
                // Extracting value of MID
                first = receipt.IndexOf("MID:");
                if (first > -1)
                {
                    last = receipt.IndexOf("TID: ", first);
                    if (last > -1)
                    {
                        MerchantID = receipt.Substring(first + 4, last - first - 4);
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- print : MID-{1}.", DateTime.Now, MerchantID));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                    else
                    {
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- print : MID Linefeed not detected.", DateTime.Now));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                }

                // Extracting value of TID
                first = receipt.IndexOf("TID: ");
                if (first > -1)
                {
                    last = receipt.IndexOf("Batch ID:", first);
                    if (last > -1)
                    {
                        TerminalID = receipt.Substring(first + 5, last - first - 5);
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- print : TID-{1}.", DateTime.Now, TerminalID));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                    else
                    {
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- print : TID Linefeed not detected.", DateTime.Now));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                }

                // Extracting value of Application
                first = receipt.IndexOf("PURCHASE");
                if (first > -1)
                {
                    last = receipt.IndexOf("h$", first + 11);
                    if (last > -1)
                    {
                        APPName = receipt.Substring(first + 12, last - first - 13);
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- print : AppName-{1}.", DateTime.Now, APPName));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                    else
                    {
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- print : PURCHASE Linefeed not detected.", DateTime.Now));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                }

                // Extracting value of AID
                first = receipt.IndexOf("AID: ");
                if (first > -1)
                {
                    last = receipt.IndexOf("*****", first);
                    if (last > -1)
                    {
                        AID = receipt.Substring(first + 5, last - first - 5);
                        int expiryStart = receipt.IndexOf("EXPIRES");

                        if (expiryStart > -1)
                        {
                            using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                            {
                                StreamWriter writer = new StreamWriter(stream);
                                writer.WriteLine(string.Format(@"Datetime {0} -- print : CARDNO-{1}.", DateTime.Now, CARDNO));
                                writer.Flush();
                                writer.Close();
                            }
                            CARDNO = receipt.Substring(last, expiryStart - last);
                        }
                        else
                        {
                            using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                            {
                                StreamWriter writer = new StreamWriter(stream);
                                writer.WriteLine(string.Format(@"Datetime {0} -- print : EXPIRES not detected.", DateTime.Now, CARDNO));
                                writer.Flush();
                                writer.Close();
                            }
                        }
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- print : AID-{1}.", DateTime.Now, AID));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                    else
                    {
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- print : AID Linefeed not detected.", DateTime.Now));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                }
                // Extracting value of AUTH in Declined case
                //first = receipt.IndexOf("AAC:");
                //if (first > -1)
                //{
                //    //last = receipt.IndexOf('\r', first);
                //    //if (last > -1)
                //    //{
                //    Authorization = receipt.Substring(first + 4, 16);
                //    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                //    {
                //        StreamWriter writer = new StreamWriter(stream);
                //        writer.WriteLine(string.Format(@"Datetime {0} -- print : Auth-{1}.", DateTime.Now, Authorization));
                //        writer.Flush();
                //        writer.Close();
                //    }
                //    //}
                //    //else
                //    //{
                //    //    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                //    //    {
                //    //        StreamWriter writer = new StreamWriter(stream);
                //    //        writer.WriteLine(string.Format(@"Datetime {0} -- print : AAC Linefeed not detected.", DateTime.Now));
                //    //        writer.Flush();
                //    //        writer.Close();
                //    //    }
                //    //}
                //}

                // Extracting value of AUTH in Approved case.
                first = receipt.IndexOf("APPROVAL CODE: ");
                if (first > -1)
                {
                    last = receipt.IndexOf("RESPONSE CODE:", first);
                    if (last > -1)
                    {
                        //Authorization = receipt.Substring(first + 16, last - first - 16);
                        Authorization = receipt.Substring(first + 16, last - first - 17);
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- print : Auth-{1}.", DateTime.Now, Authorization));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                    else
                    {
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- print : APPROVAL CODE Linefeed not detected.", DateTime.Now));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                }

                first = receipt.IndexOf("RESPONSE CODE:");
                if (first > -1)
                {
                    Authorization += " RC:" + receipt.Substring(first + 14, 10).Trim();
                }
                else
                {
                    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- print : RESPONSE CODE Linefeed not detected.", DateTime.Now));
                        writer.Flush();
                        writer.Close();
                    }
                }

                // Extracting value of TVR
                first = receipt.IndexOf("TVR:");
                if (first > -1)
                {
                    last = receipt.IndexOf("TSI:", first);
                    if (last > -1)
                    {
                        TVR = receipt.Substring(first + 4, last - first - 4);
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- print : TVR-{1}.", DateTime.Now, TVR));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                    else
                    {
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- print : TVR Linefeed not detected.", DateTime.Now));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                }

                // Extracting value of TSI
                first = receipt.IndexOf("TSI:");
                if (first > -1)
                {
                    last = receipt.IndexOf("AC INFO :", first);
                    if (last > -1)
                    {
                        TSI = receipt.Substring(first + 4, last - first - 4);
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- print : TSI-{1}.", DateTime.Now, TSI));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                    else
                    {
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- print : TSI Linefeed not detected.", DateTime.Now));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                }

                // Extracting value of AC INFO
                first = receipt.IndexOf("AC INFO :");
                if (first > -1)
                {
                    last = receipt.IndexOf("*********", first);
                    if (last > -1)
                    {
                        ACInfo = receipt.Substring(first + 9, last - first - 9);
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- print : ACInfo-{1}.", DateTime.Now, ACInfo));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                    else
                    {
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- print : ACInfo Linefeed not detected.", DateTime.Now));
                            writer.Flush();
                            writer.Close();
                        }
                    }
                }

                // Extracting value of AC
                first = receipt.IndexOf("AC :");
                if (first > -1)
                {
                    //last = receipt.IndexOf('\r', first);
                    //if (last > -1)
                    //{
                    AC = receipt.Substring(first + 4, 16);
                    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- print : AC-{1}.", DateTime.Now, AC));
                        writer.Flush();
                        writer.Close();
                    }
                    //}
                    //else
                    //{
                    //    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                    //    {
                    //        StreamWriter writer = new StreamWriter(stream);
                    //        writer.WriteLine(string.Format(@"Datetime {0} -- print : AC Linefeed not detected.", DateTime.Now));
                    //        writer.Flush();
                    //        writer.Close();
                    //    }
                    //}
                }

                //first = receipt.IndexOf("************");
                //if (first > -1)
                //{
                //    //last = receipt.IndexOf('\r', first);
                //    //if (last > -1)
                //    //{
                //    CARDNO = receipt.Substring(first, 16);
                //    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                //    {
                //        StreamWriter writer = new StreamWriter(stream);
                //        writer.WriteLine(string.Format(@"Datetime {0} -- print : CardNo-{1}.", DateTime.Now, CARDNO));
                //        writer.Flush();
                //        writer.Close();
                //    }
                //}
                //else
                //{
                //    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                //    {
                //        StreamWriter writer = new StreamWriter(stream);
                //        writer.WriteLine(string.Format(@"Datetime {0} -- print : CardNo Linefeed not detected.", DateTime.Now));
                //        writer.Flush();
                //        writer.Close();
                //    }
                //}


                Action<string, string, string, string, string, string, string, string, string, string> handler = ReceiptEvent;
                if (handler != null)
                {
                    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- print : Invoking OnReceiptPrintNotification.", DateTime.Now));
                        writer.Flush();
                        writer.Close();
                    }
                    handler(MerchantID, TerminalID, Authorization, AID, APPName, TVR, TSI, ACInfo, AC, CARDNO);
                }
                else
                {
                    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- print : OnReceiptPrintNotification not registered.", DateTime.Now));
                        writer.Flush();
                        writer.Close();
                    }
                }

            }

            catch (Exception ex)
            {
                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- print : Exception Caught Message {1}.", DateTime.Now, ex.Message));
                    writer.Flush();
                    writer.Close();
                }
            }
            return 0;
        }


        //public override int print(string receipt, EftTerminal terminal)
        //{
        //    //Console.WriteLine("ECR printer:");
        //    //Console.WriteLine(text);
        //    string MerchantID = string.Empty;
        //    string TerminalID = string.Empty;
        //    string Authorization = string.Empty;
        //    string AID = string.Empty;
        //    string APPName = string.Empty;
        //    string TVR = string.Empty;
        //    string TSI = string.Empty;
        //    string ACInfo = string.Empty;
        //    string AC = string.Empty;
        //    string CARDNO = string.Empty;
        //    int first, last;


        //    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //    {
        //        StreamWriter writer = new StreamWriter(stream);
        //        writer.WriteLine(string.Format(@"Datetime {0} -- print : Text = {1}.", DateTime.Now, receipt));
        //        writer.Flush();
        //        writer.Close();
        //    }
        //    try
        //    {
        //        // Extracting value of MID
        //        first = receipt.IndexOf("MID:");
        //        if (first > -1)
        //        {
        //            last = receipt.IndexOf("TID: ", first);
        //            if (last > -1)
        //            {
        //                MerchantID = receipt.Substring(first + 4, last - first - 4);
        //                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //                {
        //                    StreamWriter writer = new StreamWriter(stream);
        //                    writer.WriteLine(string.Format(@"Datetime {0} -- print : MID-{1}.", DateTime.Now, MerchantID));
        //                    writer.Flush();
        //                    writer.Close();
        //                }
        //            }
        //            else
        //            {
        //                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //                {
        //                    StreamWriter writer = new StreamWriter(stream);
        //                    writer.WriteLine(string.Format(@"Datetime {0} -- print : MID Linefeed not detected.", DateTime.Now));
        //                    writer.Flush();
        //                    writer.Close();
        //                }
        //            }
        //        }

        //        // Extracting value of TID
        //        first = receipt.IndexOf("TID: ");
        //        if (first > -1)
        //        {
        //            last = receipt.IndexOf("Batch ID:", first);
        //            if (last > -1)
        //            {
        //                TerminalID = receipt.Substring(first + 5, last - first - 5);
        //                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //                {
        //                    StreamWriter writer = new StreamWriter(stream);
        //                    writer.WriteLine(string.Format(@"Datetime {0} -- print : TID-{1}.", DateTime.Now, TerminalID));
        //                    writer.Flush();
        //                    writer.Close();
        //                }
        //            }
        //            else
        //            {
        //                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //                {
        //                    StreamWriter writer = new StreamWriter(stream);
        //                    writer.WriteLine(string.Format(@"Datetime {0} -- print : TID Linefeed not detected.", DateTime.Now));
        //                    writer.Flush();
        //                    writer.Close();
        //                }
        //            }
        //        }

        //        // Extracting value of Application
        //        first = receipt.IndexOf("PURCHASE");
        //        if (first > -1)
        //        {
        //            last = receipt.IndexOf("h$", first + 11);
        //            if (last > -1)
        //            {
        //                APPName = receipt.Substring(first + 12, last - first - 13);
        //                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //                {
        //                    StreamWriter writer = new StreamWriter(stream);
        //                    writer.WriteLine(string.Format(@"Datetime {0} -- print : AppName-{1}.", DateTime.Now, APPName));
        //                    writer.Flush();
        //                    writer.Close();
        //                }
        //            }
        //            else
        //            {
        //                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //                {
        //                    StreamWriter writer = new StreamWriter(stream);
        //                    writer.WriteLine(string.Format(@"Datetime {0} -- print : PURCHASE Linefeed not detected.", DateTime.Now));
        //                    writer.Flush();
        //                    writer.Close();
        //                }
        //            }
        //        }

        //        // Extracting value of AID
        //        first = receipt.IndexOf("AID: ");
        //        if (first > -1)
        //        {
        //            last = receipt.IndexOf("*****", first);
        //            if (last > -1)
        //            {
        //                AID = receipt.Substring(first + 5, last - first - 5);
        //                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //                {
        //                    StreamWriter writer = new StreamWriter(stream);
        //                    writer.WriteLine(string.Format(@"Datetime {0} -- print : AID-{1}.", DateTime.Now, AID));
        //                    writer.Flush();
        //                    writer.Close();
        //                }
        //            }
        //            else
        //            {
        //                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //                {
        //                    StreamWriter writer = new StreamWriter(stream);
        //                    writer.WriteLine(string.Format(@"Datetime {0} -- print : AID Linefeed not detected.", DateTime.Now));
        //                    writer.Flush();
        //                    writer.Close();
        //                }
        //            }
        //        }
        //        // Extracting value of AUTH in Declined case
        //        //first = receipt.IndexOf("AAC:");
        //        //if (first > -1)
        //        //{
        //        //    //last = receipt.IndexOf('\r', first);
        //        //    //if (last > -1)
        //        //    //{
        //        //    Authorization = receipt.Substring(first + 4, 16);
        //        //    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //        //    {
        //        //        StreamWriter writer = new StreamWriter(stream);
        //        //        writer.WriteLine(string.Format(@"Datetime {0} -- print : Auth-{1}.", DateTime.Now, Authorization));
        //        //        writer.Flush();
        //        //        writer.Close();
        //        //    }
        //        //    //}
        //        //    //else
        //        //    //{
        //        //    //    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //        //    //    {
        //        //    //        StreamWriter writer = new StreamWriter(stream);
        //        //    //        writer.WriteLine(string.Format(@"Datetime {0} -- print : AAC Linefeed not detected.", DateTime.Now));
        //        //    //        writer.Flush();
        //        //    //        writer.Close();
        //        //    //    }
        //        //    //}
        //        //}

        //        // Extracting value of AUTH in Approved case.
        //        first = receipt.IndexOf("APPROVAL CODE: ");
        //        if (first > -1)
        //        {
        //            last = receipt.IndexOf("********", first);
        //            if (last > -1)
        //            {
        //                Authorization = receipt.Substring(first + 14, 10).Trim();
        //                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //                {
        //                    StreamWriter writer = new StreamWriter(stream);
        //                    writer.WriteLine(string.Format(@"Datetime {0} -- print : Auth-{1}.", DateTime.Now, Authorization));
        //                    writer.Flush();
        //                    writer.Close();
        //                }
        //            }
        //            else
        //            {
        //                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //                {
        //                    StreamWriter writer = new StreamWriter(stream);
        //                    writer.WriteLine(string.Format(@"Datetime {0} -- print : APPROVAL CODE Linefeed not detected.", DateTime.Now));
        //                    writer.Flush();
        //                    writer.Close();
        //                }
        //            }
        //        }

        //        // Extracting value of TVR
        //        first = receipt.IndexOf("TVR:");
        //        if (first > -1)
        //        {
        //            last = receipt.IndexOf("TSI:", first);
        //            if (last > -1)
        //            {
        //                TVR = receipt.Substring(first + 4, last - first - 4);
        //                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //                {
        //                    StreamWriter writer = new StreamWriter(stream);
        //                    writer.WriteLine(string.Format(@"Datetime {0} -- print : TVR-{1}.", DateTime.Now, TVR));
        //                    writer.Flush();
        //                    writer.Close();
        //                }
        //            }
        //            else
        //            {
        //                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //                {
        //                    StreamWriter writer = new StreamWriter(stream);
        //                    writer.WriteLine(string.Format(@"Datetime {0} -- print : TVR Linefeed not detected.", DateTime.Now));
        //                    writer.Flush();
        //                    writer.Close();
        //                }
        //            }
        //        }

        //        // Extracting value of TSI
        //        first = receipt.IndexOf("TSI:");
        //        if (first > -1)
        //        {
        //            last = receipt.IndexOf("AC INFO :", first);
        //            if (last > -1)
        //            {
        //                TSI = receipt.Substring(first + 4, last - first - 4);
        //                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //                {
        //                    StreamWriter writer = new StreamWriter(stream);
        //                    writer.WriteLine(string.Format(@"Datetime {0} -- print : TSI-{1}.", DateTime.Now, TSI));
        //                    writer.Flush();
        //                    writer.Close();
        //                }
        //            }
        //            else
        //            {
        //                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //                {
        //                    StreamWriter writer = new StreamWriter(stream);
        //                    writer.WriteLine(string.Format(@"Datetime {0} -- print : TSI Linefeed not detected.", DateTime.Now));
        //                    writer.Flush();
        //                    writer.Close();
        //                }
        //            }
        //        }

        //        // Extracting value of AC INFO
        //        first = receipt.IndexOf("AC INFO :");
        //        if (first > -1)
        //        {
        //            last = receipt.IndexOf("*********", first);
        //            if (last > -1)
        //            {
        //                ACInfo = receipt.Substring(first + 9, last - first - 9);
        //                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //                {
        //                    StreamWriter writer = new StreamWriter(stream);
        //                    writer.WriteLine(string.Format(@"Datetime {0} -- print : ACInfo-{1}.", DateTime.Now, ACInfo));
        //                    writer.Flush();
        //                    writer.Close();
        //                }
        //            }
        //            else
        //            {
        //                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //                {
        //                    StreamWriter writer = new StreamWriter(stream);
        //                    writer.WriteLine(string.Format(@"Datetime {0} -- print : ACInfo Linefeed not detected.", DateTime.Now));
        //                    writer.Flush();
        //                    writer.Close();
        //                }
        //            }
        //        }

        //        // Extracting value of AC
        //        first = receipt.IndexOf("AC :");
        //        if (first > -1)
        //        {
        //            //last = receipt.IndexOf('\r', first);
        //            //if (last > -1)
        //            //{
        //            AC = receipt.Substring(first + 4, 16);
        //            using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //            {
        //                StreamWriter writer = new StreamWriter(stream);
        //                writer.WriteLine(string.Format(@"Datetime {0} -- print : AC-{1}.", DateTime.Now, AC));
        //                writer.Flush();
        //                writer.Close();
        //            }
        //            //}
        //            //else
        //            //{
        //            //    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //            //    {
        //            //        StreamWriter writer = new StreamWriter(stream);
        //            //        writer.WriteLine(string.Format(@"Datetime {0} -- print : AC Linefeed not detected.", DateTime.Now));
        //            //        writer.Flush();
        //            //        writer.Close();
        //            //    }
        //            //}
        //        }

        //        first = receipt.IndexOf("PURCHASE");
        //        if (first > -1)
        //        {
        //            first = receipt.IndexOf("************");
        //            if (first > -1)
        //            {
        //                //last = receipt.IndexOf('\r', first);
        //                //if (last > -1)
        //                //{
        //                CARDNO = receipt.Substring(first, 16);
        //                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //                {
        //                    StreamWriter writer = new StreamWriter(stream);
        //                    writer.WriteLine(string.Format(@"Datetime {0} -- print : CardNo-{1}.", DateTime.Now, CARDNO));
        //                    writer.Flush();
        //                    writer.Close();
        //                }
        //            }
        //        }
        //        else
        //        {
        //            using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //            {
        //                StreamWriter writer = new StreamWriter(stream);
        //                writer.WriteLine(string.Format(@"Datetime {0} -- print : CardNo Linefeed not detected.", DateTime.Now));
        //                writer.Flush();
        //                writer.Close();
        //            }
        //        }

        //        //Appending Response Code to Authorization
        //        first = receipt.IndexOf("RESPONSE CODE:");
        //        if (first > -1)
        //        {
        //            Authorization += " RC: " + receipt.Substring(first + 14, 10).Trim();
        //            using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //            {
        //                StreamWriter writer = new StreamWriter(stream);
        //                writer.WriteLine(string.Format(@"Datetime {0} -- print : Appending RC -{1}.", DateTime.Now, Authorization));
        //                writer.Flush();
        //                writer.Close();
        //            }
        //        }
        //        else
        //        {
        //            using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //            {
        //                StreamWriter writer = new StreamWriter(stream);
        //                writer.WriteLine(string.Format(@"Datetime {0} -- print : CardNo Linefeed not detected.", DateTime.Now));
        //                writer.Flush();
        //                writer.Close();
        //            }
        //        }



        //        Action<string, string, string, string, string, string, string, string, string, string> handler = ReceiptEvent;
        //        if (handler != null)
        //        {
        //            using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //            {
        //                StreamWriter writer = new StreamWriter(stream);
        //                writer.WriteLine(string.Format(@"Datetime {0} -- print : Invoking OnReceiptPrintNotification.", DateTime.Now));
        //                writer.Flush();
        //                writer.Close();
        //            }
        //            handler(MerchantID, TerminalID, Authorization, AID, APPName, TVR, TSI, ACInfo, AC, CARDNO);
        //        }
        //        else
        //        {
        //            using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //            {
        //                StreamWriter writer = new StreamWriter(stream);
        //                writer.WriteLine(string.Format(@"Datetime {0} -- print : OnReceiptPrintNotification not registered.", DateTime.Now));
        //                writer.Flush();
        //                writer.Close();
        //            }
        //        }

        //    }

        //    catch (Exception ex)
        //    {
        //        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
        //        {
        //            StreamWriter writer = new StreamWriter(stream);
        //            writer.WriteLine(string.Format(@"Datetime {0} -- print : Exception Caught Message {1}.", DateTime.Now, ex.Message));
        //            writer.Flush();
        //            writer.Close();
        //        }
        //    }
        //    return 0;
        //}

        public override int status(EftTerminal terminal)
        {
            return READY;
        }

        public override int width(EftTerminal terminal)
        {
            return 24;
        }
    }

    // define own confirmation class
    public class Confirmation : ECRConfirmation
    {
        /// <summary>
        /// Event to publish the action of Payment confirmation Notification.
        /// </summary>
        ////public event Action<double> PaymentConfirmedEvent;
        public event Action PaymentConfirmationEvent;

        // override confirmation e.g. to confirm issuing of goods
        public override int confirmation(out ushort confirm, EftTerminal terminal)
        {
            //Console.WriteLine("confirmation");
            using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(string.Format(@"Datetime {0} -- Payment Confirmation.", DateTime.Now));
                writer.Flush();
                writer.Close();
            }
            Action handler = PaymentConfirmationEvent;
            if (handler != null)
            {
                handler();
            }
            confirm = (int)ConfirmType.CONFTYPE_GOODS_ISSUE; // CONFIRM_OK
            return 0;    // OK
        }
    }

    public class Artema : ICardReader
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



        // Async method call delegates.
        DInit initMethod;
        DPayment paymentMethod;

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
        /// Event to publish the action of Unknown/not Correctly inserted Card.
        /// </summary>
        ////public event Action<bool> CardInValid;
        public event Action<short> CardInValid;

        /// <summary>
        /// Event to publish the action of printing receipt.
        /// </summary>
        ////public event Action<string,string,string,string,string,string,string,string,string> ReceiptEvent;
        public event Action<string, string, string, string, string, string, string, string, string, string> ReceiptEvent;

        #region Methods to interact with the card reader device driver from external applications
        /// <summary>
        /// Initializes the card reader
        /// </summary>
        public void InitAsync(bool simulateCardReader)
        {

            if (!isInitialized)
            {
                isInitialized = true;
                simulateDevice = simulateCardReader;

                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
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
        /// Initiates the Payment Authorization Sequence.
        /// </summary>
        public bool PaymentAsync(double amount)
        {


            expectedAmount = amount;
            if (isInitialized)
            {
                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
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

            return false;
        }

        #endregion

        #region Methods to manage ArtemaModular
        private void Init(bool reconnect)
        {
            if (!reconnect)
            {
                // Initialize/Reset the Card device.
                isEnabled = false;
                terminal = new EftTerminalThales();
                display = new Display(25, 4);
                printer = new Printer();
                confirmation = new Confirmation();
                // Register for SwipeCardEvent
                display.SwipeCardEvent += OnSwipeCardNotification;
                // Register for PaymentConfirmationEvent
                confirmation.PaymentConfirmationEvent += OnPaymentConfirmationNotification;
                //Register for ReceiptEvent
                printer.ReceiptEvent += OnReceiptPrintNotification;

                terminal.setLocalDisplay(display);
                terminal.setLocalPrinter(printer);
                terminal.setConfirmation(confirmation);
                terminal.setOptions((uint)EftTerminalThales.Options.Confirmation |
                    (uint)EftTerminalThales.Options.TranspDisplText);
            } // End of Reconnect flag check.
            if (simulateDevice)
            {
                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- CardReader Initialization in Simulated Mode.", DateTime.Now));
                    writer.Flush();
                    writer.Close();
                }
                isEnabled = true;
                this.stateCode = (int)CardReaderState.ONLINE;
                this.statusCode = (int)ReaderStatus.NoError;
                if (simulationTimer == null)
                    simulationTimer = new Timer(new TimerCallback(SimulateCardReading), null, Timeout.Infinite, Timeout.Infinite);
            }
            else
            {

                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- CardReader Initialization.", DateTime.Now));
                    writer.Flush();
                    writer.Close();
                }
                string com = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["COMPORT"]) ? "COM1" : ConfigurationManager.AppSettings["COMPORT"];
                string tmsUser = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TMSUSER"]) ? "" : ConfigurationManager.AppSettings["TMSUSER"];
                string tmsPassword = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TMSPASS"]) ? "000000" : ConfigurationManager.AppSettings["TMSPASS"];

                faultCode = terminal.connect(com, tmsUser, tmsPassword, 9600, 8, 'N', 1);
                if (faultCode == 0)
                {
                    Console.WriteLine("connect successful");
                    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Connect Successful.", DateTime.Now));
                        writer.Flush();
                        writer.Close();
                    }
                    //terminal.disconnect();
                    isEnabled = true;
                    this.stateCode = (int)CardReaderState.ONLINE;
                    this.statusCode = (int)ReaderStatus.NoError;
                    //DateTime expectedTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 0);
                    //DateTime currentTime = DateTime.Now;

                    //if (settlementTimer != null)
                    //{
                    //    settlementTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    //    settlementTimer = null;
                    //}
                    //else
                    //    settlementTimer = new Timer(new TimerCallback(Settlement), null, (expectedTime - currentTime).Milliseconds, Timeout.Infinite);
                }
                else
                {
                    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Connection Failed.", DateTime.Now));
                        writer.Flush();
                        writer.Close();
                    }
                    this.stateCode = (int)CardReaderState.OFFLINE;
                    this.statusCode = (int)ReaderStatus.Error;
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
            using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
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

        public void OnSwipeCardNotification(string text)
        {
            Action<string> handler = SwipeCardEvent;
            if (handler != null)
            {
                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- OnSwipeCardNotification Called - {1}.", DateTime.Now, text));
                    writer.Flush();
                    writer.Close();
                }
                handler(text);
            }
        }

        public void OnPaymentConfirmationNotification()
        {
            Action<double> handler = PaymentConfirmationEvent;
            if (handler != null)
            {
                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- OnPaymentConfirmationNotification Called.", DateTime.Now));
                    writer.Flush();
                    writer.Close();
                }
                handler(expectedAmount);
            }
        }

        public void OnReceiptPrintNotification(string MerchantID, string TerminalID, string Authorization, string AID, string AppName, string TVR, string TSI, string ACInfo, string AC, string CardNo)
        {
            Action<string, string, string, string, string, string, string, string, string, string> handler = ReceiptEvent;
            if (handler != null)
            {
                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- OnReceiptPrintNotification Called - TVR-{1}|||TSI-{2}|||ACInfo-{3}|||MerchantID-{4}|||TerminalID-{5}|||AID-{6}|||AppName-{7}|||AC-{8}.", DateTime.Now, TVR, TSI, ACInfo, MerchantID, TerminalID, AID, AppName, AC));
                    writer.Flush();
                    writer.Close();
                }
                handler(MerchantID, TerminalID, Authorization, AID, AppName, TVR, TSI, ACInfo, AC, CardNo);
            }
        }

        public void Payment(double amount)
        {
            if (!simulateDevice)
            {
                Action<double> handler;
                Action<short> thandler;
                faultCode = (short)ErrorCode.OK;
                // Initializing currency ID = 784 (AED)
                ushort currID = (ushort)7;
                currID = (ushort)(currID << (ushort)8);
                currID = (ushort)(currID | (ushort)132);
                TagBuffer tb = new TagBuffer();
                double cents = amount * 100;
                tb.addTag((ushort)EftTag.TAG_ADDITIONAL_DATA, "00000");
                tb.addTag((ushort)EftTag.TAG_CURRENCY_ID, currID);
                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format(@"Datetime {0} -- Before Calling Terminal.Payment.", DateTime.Now));
                    writer.Flush();
                    writer.Close();
                }
                faultCode = terminal.payment(cents.ToString(), (int)PaymentType.PAY_PIN, tb);
                if (faultCode == 0)
                {
                    tb = terminal.getReply();
                    string PAN = tb.getTagAscii((ushort)EftTag.TAG_PAN);
                    //string authNumber = tb.getTag((ushort)EftTag.TAG_AUTHORIZATION_NUMBER);
                    //string terminalID = tb.getTag((ushort)EftTag.TAG_TID);
                    //string merchantID = tb.getTag((ushort)EftTag.TAG_RETAILER_REF_NUMBER);
                    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- Payment successful - {1}.", DateTime.Now, PAN));
                        writer.Flush();
                        writer.Close();
                    }
                    //Console.WriteLine("payment successful, PAN=" + PAN);
                    handler = PaymentConfirmedEvent;
                    if (handler != null)
                    {
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- Invoking PaymentConfirmedEvent.", DateTime.Now));
                            writer.Flush();
                            writer.Close();
                        }
                        handler(expectedAmount);
                    }
                }
                else
                {
                    Console.WriteLine("payment error #" + faultCode);
                    using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"Datetime {0} -- PaymentFailed with error # {1}.", DateTime.Now, faultCode));
                        writer.Flush();
                        writer.Close();
                    }


                    if (faultCode == (int) ErrorCode.ERR_UNKNOWN || //For Transaction 1 in case card is inserted wrongly
                        faultCode == (int) ErrorCode.ERR_CARD_INVALID) // For 2 Transaction in case a wrong card is inserted
                    {
                        thandler = CardInValid;    
                            if (thandler != null)
                            {
                                using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                                {
                                    StreamWriter writer = new StreamWriter(stream);
                                    writer.WriteLine(string.Format(@"Datetime {0} -- Invoking CardInValid.", DateTime.Now));
                                    writer.Flush();
                                    writer.Close();
                                }
                                thandler((short)faultCode); // Error UnkNown
                            }
                    }

                    handler = PaymentFailedEvent;
                    //this.statusCode = (int)ReaderStatus.Error;
                    if (handler != null)
                    {
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- Invoking PaymentFailedEvent.", DateTime.Now));
                            writer.Flush();
                            writer.Close();
                        }
                        handler(expectedAmount);
                    }                   

                    if (faultCode == (int)ErrorCode.ERR_LOST_CONNECTION)
                    {
                        // Lost Connection with the CardReader/Pinpad
                        using (FileStream stream = new FileStream(@"Artema.log", FileMode.Append, FileAccess.Write))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.WriteLine(string.Format(@"Datetime {0} -- Connection Lost. ReConnecting.", DateTime.Now));
                            writer.Flush();
                            writer.Close();
                        }
                        this.stateCode = (int)CardReaderState.OFFLINE;
                        this.statusCode = (int)ReaderStatus.Error;
                        // Try ReConnecting.
                        if (initTimer == null)
                            initTimer = new Timer(new TimerCallback(ReInitialize), null, 50, 50);
                        else
                            initTimer.Change(50, 50);
                    }
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
            simulationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            string MerchantID = "TestMerchantId";
            string TerminalID = "TestTerminalId";
            string Authorization = "TestAuth";
            string AID = "TestAId";
            string APPName = "TestAppName";
            string TVR = "TestTVR";
            string TSI = "TestTSI";
            string ACInfo = "TestAcInfo";
            string AC = "TestAC";
            string CardNo = "************0010";
            // Simulate Card based Payment sequence.


            #region Mag Stripe Fallback

            //// Insert Card Notification.
            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "INSERT CARD ON TERMINAL", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(3000);
            //display.printToDisplay(25, 4, 0, "REMOVE CARD FROM TERMINAL", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "REMOVE CARD FROM TERMINAL", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "PLEASE WAIT", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "CUSTOMER PIN", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "ONLINE PIN ENTERED", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "IN PROGRESS", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "CONNECTION MADE", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "RE-ENTER PIN", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "ONLINE PIN ENTERED", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "SORRY FOR DELAY", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "CONNECTION MADE", terminal);

            ////// Payment Confirmed Notification.
            ////Thread.Sleep(2000);

            //Action<string, string, string, string, string, string, string, string, string, string> handler = ReceiptEvent;
            //if (handler != null)
            //{
            //    handler(MerchantID, TerminalID, Authorization, AID, APPName, TVR, TSI, ACInfo, AC, CardNo);
            //}

            ////// Prompt the User to SWIPE the Card.
            ////Thread.Sleep(2000);
            ////display.printToDisplay(25, 4, 0, "PLEASE WAIT", terminal);


            //// Prompt for Payment Confirmed Event.
            //Thread.Sleep(2000);
            //Action<double> phandler1 = PaymentConfirmedEvent;
            //if (phandler1 != null)
            //    phandler1(expectedAmount);

            #endregion

            #region Payment Approved

            // Insert Card Notification.
            // Prompt the User to SWIPE the Card.
            Thread.Sleep(2000);
            display.printToDisplay(25, 4, 0, "INSERT CARD ON TERMINAL", terminal);

            // Prompt the User to SWIPE the Card.
            Thread.Sleep(2000);
            display.printToDisplay(25, 4, 0, "PLEASE WAIT", terminal);

            // Prompt the User to SWIPE the Card.
            Thread.Sleep(2000);
            display.printToDisplay(25, 4, 0, "PLEASE WAIT", terminal);

            // Prompt the User to SWIPE the Card.
            Thread.Sleep(2000);
            display.printToDisplay(25, 4, 0, "CUSTOMER PIN", terminal);

            // Prompt the User to SWIPE the Card.
            Thread.Sleep(2000);
            display.printToDisplay(25, 4, 0, "PIN ACCEPTED", terminal);

            // Prompt the User to SWIPE the Card.
            Thread.Sleep(2000);
            display.printToDisplay(25, 4, 0, "IN PROGRESS", terminal);

            // Prompt the User to SWIPE the Card.
            Thread.Sleep(2000);
            display.printToDisplay(25, 4, 0, "IN PROGRESS", terminal);

            // Prompt the User to SWIPE the Card.
            Thread.Sleep(2000);
            display.printToDisplay(25, 4, 0, "CONNECTION MADE", terminal);


            // Prompt the User to SWIPE the Card.
            Thread.Sleep(2000);
            display.printToDisplay(25, 4, 0, "REMOVE CARD FROM TERMINAL", terminal);

            // Prompt the User to SWIPE the Card.
            Thread.Sleep(2000);
            display.printToDisplay(25, 4, 0, "PLEASE WAIT", terminal);

            // Prompt the User to SWIPE the Card.
            Thread.Sleep(2000);
            display.printToDisplay(25, 4, 0, "PLEASE WAIT", terminal);

            // Payment Confirmed Notification.
            Thread.Sleep(2000);

            Action<string, string, string, string, string, string, string, string, string, string> handler = ReceiptEvent;
            if (handler != null)
            {
                handler(MerchantID, TerminalID, Authorization, AID, APPName, TVR, TSI, ACInfo, AC, CardNo);
            }

            // Prompt the User to SWIPE the Card.
            Thread.Sleep(2000);
            display.printToDisplay(25, 4, 0, "PLEASE WAIT", terminal);


            // Prompt for Payment Confirmed Event.
            Thread.Sleep(2000);
            Action<double> phandler1 = PaymentConfirmedEvent;
            if (phandler1 != null)
                phandler1(expectedAmount);

            #endregion

            #region Payment failed

            //// Insert Card Notification.
            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "INSERT CARD ON TERMINAL", terminal);


            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "PLEASE WAIT", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "PLEASE WAIT", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "CUSTOMER PIN", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "PIN ACCEPTED", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "IN PROGRESS", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "IN PROGRESS", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "CONNECTION MADE", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "REMOVE CARD FROM TERMINAL", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "PLEASE WAIT", terminal);

            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "PLEASE WAIT", terminal);


            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "PLEASE WAIT", terminal);

            ////// Prompt for Payment Confirmed Event.
            ////Thread.Sleep(2000);
            ////Action<short> phandler1 = CardInValid;
            ////if (phandler1 != null)
            ////    phandler1(13);

            //// Prompt for Payment Confirmed Event.
            //Thread.Sleep(2000);
            //Action<double> phandler2 = PaymentFailedEvent;
            //if (phandler2 != null)
            //    phandler2(expectedAmount);

            #endregion

            #region valid card inserted wrongly

            //// Insert Card Notification.
            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "INSERT CARD ON TERMINAL", terminal);

            //// Insert Card Notification.
            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "REMOVE CARD FROM TERMINAL", terminal);

            //// Prompt for Payment Confirmed Event.
            //Thread.Sleep(3000);
            //Action<short> phandler1 = CardInValid;
            //if (phandler1 != null)
            //    phandler1(6);

            //// Prompt for Payment Confirmed Event.
            //Thread.Sleep(2000);
            //Action<double> phandler2 = PaymentFailedEvent;
            //if (phandler1 != null)
            //    phandler2(expectedAmount);

            #endregion

            #region Unknown card

            //// Insert Card Notification.
            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "INSERT CARD ON TERMINAL", terminal);

            //// Insert Card Notification.
            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "REMOVE CARD FROM TERMINAL", terminal);

            //// Insert Card Notification.
            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "REMOVE CARD FROM TERMINAL", terminal);

            //// Insert Card Notification.
            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "PLEASE WAIT", terminal);

            ////// Prompt for Payment Confirmed Event.
            ////Thread.Sleep(2000);
            ////Action<short> phandler1 = CardInValid;
            ////if (phandler1 != null)
            ////    phandler1(33);

            //// Prompt for Payment Confirmed Event.
            //Thread.Sleep(2000);
            //Action<double> phandler2 = PaymentFailedEvent;
            //if (phandler2 != null)
            //    phandler2(33);

            #endregion

            #region Transaction Cancelled

            //// Insert Card Notification.
            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "INSERT CARD ON TERMINAL", terminal);

            //// Insert Card Notification.
            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "REMOVE CARD FROM TERMINAL", terminal);

            //// Insert Card Notification.
            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "REMOVE CARD FROM TERMINAL", terminal);

            //// Insert Card Notification.
            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "REMOVE CARD FROM TERMINAL", terminal);

            //// Insert Card Notification.
            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "REMOVE CARD FROM TERMINAL", terminal);

            //// Insert Card Notification.
            //// Prompt the User to SWIPE the Card.
            //Thread.Sleep(2000);
            //display.printToDisplay(25, 4, 0, "TRANSACTION CANCELLED", terminal);
            
            #endregion
            

        }
        #endregion Methods to manage ArtemaModular
    }
}