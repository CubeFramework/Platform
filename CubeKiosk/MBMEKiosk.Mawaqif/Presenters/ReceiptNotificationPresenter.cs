using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using MBMEKiosk.Infrastructure.ObjectModel;
using MBMEKiosk.Infrastructure.Events;
using MBMEKiosk.ObjectModel;
using MBMEKiosk.Infrastructure.Interfaces;
using System.Windows.Threading;
using MBMEKiosk.Infrastructure.BaseClasses;
using System.IO;
 

namespace MBMEKiosk.Mawaqif.Presenters
{
    internal delegate void PrintCompleted(string stateChangeTrigger);

    public class ReceiptNotificationPresenter : MawaqifPresenterBase
    {
        private bool isPrintRequestSent;
        private bool stateTimeoutExceeded;
        private bool receiptPrintingCompleted;
        private FrameworkElement receiptElement;
        DispatcherTimer timer;
         

        public override FrameworkElement LoadXaml(IDeviceAgent devices, KioskState state, TransactionContextBase transactionContext)
        {

            using (FileStream stream = new FileStream(@"app001.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(string.Format("{0} - Mawaqif Receipt LoadXaml Started", DateTime.Now));
                writer.Flush();
                writer.Close();
            }
             
            timer = new DispatcherTimer();

            FrameworkElement viewGrid = base.LoadXaml(devices, state, transactionContext);
            ReceiptPresenterBase<MawaqifTransaction> presenter = new ReceiptPresenterBase<MawaqifTransaction>(this.Transaction);
            isPrintRequestSent = false;

            

           

            if (this.Transaction.PostingFailed)
                this.State.ViewContentKey = "transactioncomplete_postingfailedtext";
            else
                if (Transaction.TransactionFailed)
                    this.State.ViewContentKey = "mawaqif_txt_pct_transaction_failed";
                else
                    this.State.ViewContentKey = "transactioncomplete_completionetext";


            if (string.Compare(this.Transaction.SelectedLanguageKey, "arabic", StringComparison.OrdinalIgnoreCase) == 0)
            {
                //receiptElement = presenter.LoadReceiptXaml(@"modules/mawaqif/pages/receipts/mawaqifreceipt_ar.xaml");
                if (!Transaction.CardPayment)
                    receiptElement = presenter.LoadReceiptXaml(@"modules/mawaqif/pages/receipts/mawaqifreceipt.xaml");
                else
                    receiptElement = presenter.LoadReceiptXaml(@"modules/mawaqif/pages/receipts/mawaqifreceipt_card.xaml");
            }
            else
            {
                //receiptElement = presenter.LoadReceiptXaml(@"modules/mawaqif/pages/receipts/mawaqifreceipt.xaml");
                if (!Transaction.CardPayment)
                    receiptElement = presenter.LoadReceiptXaml(@"modules/mawaqif/pages/receipts/mawaqifreceipt.xaml");
                else
                    receiptElement = presenter.LoadReceiptXaml(@"modules/mawaqif/pages/receipts/mawaqifreceipt_card.xaml");
            }

            

            receiptPrintingCompleted = false;
            stateTimeoutExceeded = false;

            timer.Tick += PrintReceiptOnRenderComplete;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timer.Start();
            Console.WriteLine("{0}: receipt notification presenter loaded.", DateTime.Now);
            using (FileStream stream = new FileStream(@"app001.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(string.Format("{0}: receipt notification presenter loaded.", DateTime.Now));
                writer.Flush();
                writer.Close();
            }
            return viewGrid;

            using (FileStream stream = new FileStream(@"app001.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(string.Format("{0}: Mawaqif Receipt Load Xaml Completed.", DateTime.Now));
                writer.Flush();
                writer.Close();
            }
             
        }

        public override void Deactivate()
        {
            base.Deactivate();

            timer.Stop();
            timer.Tick -= PrintReceiptOnRenderComplete;
            timer = null;
        }

        private void PrintReceiptOnRenderComplete(object o, EventArgs args)
        {
            Console.WriteLine("{0}: PrintReceiptOnRenderComplete.", DateTime.Now);
            using (FileStream stream = new FileStream(@"app001.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(string.Format("{0}: PrintReceiptOnRenderComplete.", DateTime.Now));
                writer.Flush();
                writer.Close();
            }
            string outputTrigger = string.Empty;
            timer.Stop();

            if (!isPrintRequestSent)
            {
                isPrintRequestSent = true;
                IPrinter printer = Devices.GetPrinter();
                printer.ReceiptPrinted += OnReceiptPrinted;
                Console.WriteLine("{0}: Sending print request from Fewa.", DateTime.Now);
                using (FileStream stream = new FileStream(@"app001.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format("{0}: Sending print request from Mawaqif.", DateTime.Now));
                    writer.Flush();
                    writer.Close();
                }
                printer.Print(receiptElement, this.Transaction.Id);
                timer.Interval = new TimeSpan(0, 0, this.State.MessageTimeOut - 1);
                timer.Start();
            }
            else if (receiptPrintingCompleted)
            {
                Console.WriteLine("{0}: Receipt printing completed.", DateTime.Now);
                using (FileStream stream = new FileStream(@"app001.log", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(string.Format("{0}: Receipt printing completed.", DateTime.Now));
                    writer.Flush();
                    writer.Close();
                }
                outputTrigger = "submit";
            }
            else
            {
                stateTimeoutExceeded = true;
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Start();
            }

            if (!string.IsNullOrEmpty(outputTrigger))
            {
                IPrinter printer = Devices.GetPrinter();
                printer.ReceiptPrinted -= OnReceiptPrinted;
                ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new PrintCompleted(ChangeState), outputTrigger);
            }
        }

        private void ChangeState(string trigger)
        {
            OnKioskStateChanged(new KioskStateChangedEventArgs(trigger));
        }

        private void OnReceiptPrinted(bool printStatus)
        {
            this.receiptPrintingCompleted = true;
            //this.LogPrintCycleLogToLocalDb();
            if (stateTimeoutExceeded)
            {
                timer.Stop();
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Start();
            }
        }

        public FrameworkElement ReceiptView
        {
            get
            {
                return receiptElement;
            }

            set
            {
                if (receiptElement != value)
                {
                    receiptElement = value;
                    OnPropertyChanged("ReceiptView");
                }
            }
        }

        

        
           
    }
}
