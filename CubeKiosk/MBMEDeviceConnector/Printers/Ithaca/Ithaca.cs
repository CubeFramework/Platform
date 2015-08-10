using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.Interfaces;
using System.Windows;
using System.Drawing;
using MBMEDevices.Printers.Utils;
using Ithaca;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;

namespace MBMEDevices.Printers
{
    public class Ithaca : IPrinter
    {
        private bool simulate;
        private static Epic950 m_PrinterDevice = null;
        private short printerStatus;
        private short printerState;
        private string printerStatusDesc;
        private ReturnStatus prnStatus;
        private delegate void PrintBitmap(Bitmap bmp, string transactionId);
        private static object monitor = new object();

        #region IPrinter Members

        public void Init(bool simulateReceiptPrinting)
        {
            simulate = simulateReceiptPrinting;
            prnStatus += this.Status;
            printerStatus = 3; // Indicates Error status of Printer.
            printerState = 2; // Indicates Printer Offline State.
            printerStatusDesc = "Printer In Offline State.";
            //monitor = this;
        }

        public bool IsReady()
        {
            lock (monitor)
            {
                if (m_PrinterDevice == null)
                    m_PrinterDevice = Epic950.FindPrinter();
            }

            if (simulate)
            {
                printerState = 1;
                printerStatus = 1;
                printerStatusDesc = "Printer Ready.";
                return true;
            }
            if (m_PrinterDevice != null)
            {
                short status = -1;
                
                status = m_PrinterDevice.GetStatus(prnStatus);
                System.Threading.Thread.Sleep(1000);
                if (status > 0)
                    printerState = 1; // Indicates Printer Online State.
                else
                {
                    printerState = 2; // Indicates Printer Offline State.
                    printerStatusDesc = "Printer In Offline State.";
                    m_PrinterDevice = null;
                }
                
                if ((printerState == 1) && ((printerStatus == 1) || (printerStatus == 2)))
                  return true;
            }

            return false;
        }

        public string GetDetails(out short state,out short status)
        {
            IsReady();
            state = printerState;
            status = printerStatus;
            return printerStatusDesc;
        }

        public void Print(FrameworkElement receipt, string transactionId)
        {
            RenderTargetBitmap rtb;
            Bitmap orgBmp;

            Console.WriteLine("{0}: Ithaca Print() called.", DateTime.Now.ToString("HH:mm:ss:ffff"));

            string receiptImagePath = string.Empty;
            Mod_XAMLToBitmap_08 converter = new Mod_XAMLToBitmap_08();
            rtb = converter.XAMLToBitmap(receipt, 203.0);
            Console.WriteLine("{0}: Mod_XAMLToBitmap_08 completed.", DateTime.Now.ToString("HH:mm:ss:ffff"));

            // Write to memory stream.
            using (MemoryStream ms = new MemoryStream())
            {
                BmpBitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(rtb));
                enc.Save(ms);
                orgBmp = new Bitmap(ms);
            }

            Console.WriteLine("{0}: Converting to 1bpp bitmap...", DateTime.Now.ToString("HH:mm:ss:ffff"));
            Bitmap newbmp = BitmapConverter.CopyToBpp(orgBmp, 1);
            Console.WriteLine("{0}: Conversion to 1bpp bitmap completed.", DateTime.Now.ToString("HH:mm:ss:ffff"));
            PrintBitmap printAsync = new PrintBitmap(this.SendToPrinter);
            printAsync.BeginInvoke(newbmp, transactionId, null, null);
        }

        public event Action<bool> ReceiptPrinted;

        #endregion

        private void SendToPrinter(Bitmap image, string transactionId)
        {
            bool printSuccess = false;
            short opCode = -1;
            image.Save(@"Receipt" + transactionId + ".bmp", ImageFormat.Bmp);
            Console.WriteLine("{0}: Before Find printer.", DateTime.Now.ToString("HH:mm:ss:ffff"));
            //m_PrinterDevice = Epic950.FindPrinter();
            Console.WriteLine("{0}: After Find printer.", DateTime.Now.ToString("HH:mm:ss:ffff"));
            if (m_PrinterDevice != null)
            {
                Console.WriteLine("{0}: Printer found.", DateTime.Now.ToString("HH:mm:ss:ffff"));
                using (FileStream fs = new FileStream(@"Receipt" + transactionId + ".bmp", FileMode.Open))
                {
                    using (FileStream stream = new FileStream(@"KIOSKApp.log", FileMode.OpenOrCreate))
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(string.Format(@"SendToPrinter() called. Printing receipt for transaction: {0}", transactionId));
                        writer.Flush();
                        writer.Close();
                    }

                    Console.WriteLine("{0}: PrintBitmap called.", DateTime.Now.ToString("HH:mm:ss:ffff"));
                    opCode = m_PrinterDevice.PrintBitmap(fs, 0);
                    Console.WriteLine("{0}: PrintBitmap completed.", DateTime.Now.ToString("HH:mm:ss:ffff"));
                }
            }

            if (opCode != -1)
              printSuccess = true;

            Action<bool> handler = ReceiptPrinted;
            if (handler != null)
            {
                handler(printSuccess);
            }
        }

        private void Status(PrinterStatus objStatus)
        {
            lock (monitor)
            {
                printerStatus = 1; // Indicates Normal Status.
                printerStatusDesc = "Printer Ready.";
                Epic950Status objStatus1 = (Epic950Status)objStatus;
                if (!objStatus1.IsPrinterReady())
                {
                    printerStatus = 3; // Indicates Error Status.
                    printerStatusDesc = "Printer Not Ready -";
                    if (objStatus1.IsChasisOpen())
                        printerStatusDesc += "Chasis is open.";
                    if (objStatus1.IsOutOfTicket())
                        printerStatusDesc += "Out of Ticket.";
                    if (objStatus1.IsPaperJam())
                        printerStatusDesc += "Paper Jam.";
                    if (objStatus1.IsPrinterHeadCorrectlyPlaced())
                        printerStatusDesc += "Printer head up.";
                    if (!objStatus1.IsTicketLoaded())
                        printerStatusDesc += "No Tickets Loaded.";
                    if (!objStatus1.IsTopOfForm())
                        printerStatusDesc += "Not found top of form. Paper not correctly placed.";
                }
                else
                {
                    if (objStatus1.IsLevelLow())
                    {
                        printerStatus = 2; // Indicates Warning Status.
                        printerStatusDesc = "Printer Paper Level Low.";
                    }
                }
            }
            //System.Console.WriteLine("Is Chasis Open:" + objStatus1.IsChasisOpen());
            //System.Console.WriteLine("Is Level Low:" + objStatus1.IsLevelLow());
            //System.Console.WriteLine("Is Out Of Ticket:" + objStatus1.IsOutOfTicket());
            //System.Console.WriteLine("Is Paper Jam:" + objStatus1.IsPaperJam());
            //System.Console.WriteLine("Is Printer Head Up:" + objStatus1.IsPrinterHeadCorrectlyPlaced());
            //System.Console.WriteLine("Is Printer Ready:" + objStatus1.IsPrinterReady());
            //System.Console.WriteLine("Is Ticket In Path:" + objStatus1.IsTicketInPath());
            //System.Console.WriteLine("Is Ticket Loaded:" + objStatus1.IsTicketLoaded());
            //System.Console.WriteLine("Is Top Of Form:" + objStatus1.IsTopOfForm());
            //System.Console.WriteLine("Is Top Of Form1:" + objStatus1.IsTopOfForm1());
        }
    }
}
