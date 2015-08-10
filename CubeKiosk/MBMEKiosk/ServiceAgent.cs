using System;
using System.Configuration;
using MBMEDevices.CashDevices;
using MBMEKiosk.Infrastructure.Interfaces;
using System.Windows.Forms;

namespace MBMEKiosk
{
    public class DeviceAgent : IDeviceAgent
    {
        private static DeviceAgent theInstance;
        private static object syncroot = new object();

        private ICashAcceptor cashAcceptor;
        private IPrinter printer;
        private ICardReader reader;
        private IUSBKeyboard usbKeyboard;
        private IntPtr source;
         

        private DeviceAgent()
        {

        }

        #region IServiceAgent Members

        public static DeviceAgent GetInstance()
        {
            lock(syncroot)
            {
                if (theInstance == null)
                {
                    theInstance = new DeviceAgent();
                }
            }

            return theInstance;
        }

        public void Init()
        {
            InitializePrinter();
            InitailizeCashAcceptor();
            InitializeReader();
            InitializeUSBKeyboard();
        }

        public ICashAcceptor GetCashAcceptor()
        {
            return cashAcceptor;
        }

        public IPrinter GetPrinter()
        {
            return printer;
        }

        public ICardReader GetCardReader()
        {
            return reader;
        }

        public IUSBKeyboard GetUSBKeyboard()
        {
            return usbKeyboard;
        }

        #endregion

        public void RegisterSource(IntPtr hwnd)
        {
            source = hwnd;
        }
        public void ProcessMessage(Message msg)
        {
            if (usbKeyboard != null)
            {
                usbKeyboard.ProcessMessage(msg);
            }
        }

        private void InitailizeCashAcceptor()
        {
            if (cashAcceptor == null)
            {
                cashAcceptor = new ArdacElite();

           /////Production Change suggested by Sherief    
            #if DEBUG
                if(ConfigurationManager.AppSettings["SimulateCashAcceptor"]==null)
                    cashAcceptor.InitAsync(true);
                else
                    cashAcceptor.InitAsync(Boolean.Parse(ConfigurationManager.AppSettings["SimulateCashAcceptor"]));
            #else
                cashAcceptor.InitAsync(true);
            #endif
            }
        }

        private void InitializePrinter()
        {
            if (printer == null)
            {
                if(ConfigurationManager.AppSettings["Printer"] == "Zebra")
                    printer = new MBMEDevices.Printers.Zebra();
                else
                    printer = new MBMEDevices.Printers.Ithaca();

                 #if DEBUG
                    printer.Init(Boolean.Parse((ConfigurationManager.AppSettings["SimulatePrinter"])));
                 #else
                    printer.Init(false);
                 #endif
            }
        }

        private void InitializeReader()
        {
            if (reader == null)
            {
                reader = new MBMEDevices.Readers.Artema();

                #if DEBUG
                    reader.InitAsync(Boolean.Parse(ConfigurationManager.AppSettings["SimulateReader"]));
                #else
                    reader.InitAsync(false);
                #endif
            }
        }

        private void InitializeUSBKeyboard()
        {
            if (usbKeyboard == null)
            {
                usbKeyboard = new MBMEDevices.USBKeyboard.PP151(source);
                usbKeyboard.InitAsync(Boolean.Parse(string.IsNullOrEmpty(ConfigurationManager.AppSettings["SimulateUSBKeyboard"]) ? "false" : ConfigurationManager.AppSettings["SimulateUSBKeyboard"]));
            }
        }
         
    }
}
