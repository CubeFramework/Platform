using System;
using System.IO;
using System.Drawing;

namespace Ithaca
{
    public class Ithaca
    {
        // USB Printer Device
        private static Epic950 m_PrinterDevice = null;
        private static void Status(PrinterStatus objStatus)
        {
            Epic950Status objStatus1 = (Epic950Status)objStatus;
            System.Console.WriteLine("Is Chasis Open:" + objStatus1.IsChasisOpen());
            System.Console.WriteLine("Is Level Low:" + objStatus1.IsLevelLow());
            System.Console.WriteLine("Is Out Of Ticket:" + objStatus1.IsOutOfTicket());
            System.Console.WriteLine("Is Paper Jam:" + objStatus1.IsPaperJam());
            System.Console.WriteLine("Is Printer Head Up:" + objStatus1.IsPrinterHeadCorrectlyPlaced());
            System.Console.WriteLine("Is Printer Ready:" + objStatus1.IsPrinterReady());
            System.Console.WriteLine("Is Ticket In Path:" + objStatus1.IsTicketInPath());
            System.Console.WriteLine("Is Ticket Loaded:" + objStatus1.IsTicketLoaded());
            System.Console.WriteLine("Is Top Of Form:" + objStatus1.IsTopOfForm());
            System.Console.WriteLine("Is Top Of Form1:" + objStatus1.IsTopOfForm1());
        }
        [STAThread]
        static void Main(string[] args)
        {
            Mod_XAMLToBitmap_08 temp = new Mod_XAMLToBitmap_08();
            temp.XAMLToBitmap("D:\\MBME\\FewaReceipt_small_English_ForJK_new.XAML", "D:\\MBME\\Sample.bmp", "Image", 203.0);
            System.Drawing.Bitmap bmp = new Bitmap("D:\\MBME\\Sample.bmp");
            System.Drawing.Bitmap newbmp = BitmapConverter.CopyToBpp(bmp, 1);
            //BitmapConverter.SplashImage(newbmp, 100, 100);
            newbmp.Save("D:\\MBME\\Sample1.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            m_PrinterDevice = Epic950.FindPrinter();
            ReturnStatus del;
            del = Status;
            m_PrinterDevice.GetStatus(del);
            FileStream fsBMP = new FileStream("D:\\MBME\\Sample1.bmp", FileMode.Open);
            ////m_PrinterDevice.PrintBitmap(fsBMP,500);
            m_PrinterDevice.PrintBitmap(fsBMP, 0);
            //for (int i=0;i < 7;i++)
            //  m_PrinterDevice.PrintBitmap();
            System.Threading.Thread.Sleep(2000);
            m_PrinterDevice.GetStatus(del);
            
            m_PrinterDevice.Dispose();
            //m_PrinterDevice.Reset();
            //m_PrinterDevice.GetStatus();
            //m_PrinterDevice.WriteText();
         }

    }

}
