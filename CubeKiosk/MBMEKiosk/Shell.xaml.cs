using System.Configuration;
using System.Windows;
using System.Windows.Threading;
using System;
using MBMEKiosk.Infrastructure.Events;
using MBMEKiosk.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32;
//using MBMEKiosk.LogonProxy;
using MBMEKiosk.Infrastructure.Utils;
using MBMEKiosk.Infrastructure.ObjectModel;
using log4net;
using System.Windows.Input;
using System.ComponentModel;
using MBMEKioskLogger.Logger;
using System.Windows.Interop;
using System.Windows.Forms;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace MBMEKiosk
{
    public delegate void DInitCashDevice(bool simulate);

    /// <summary>
    /// The main shell container of the Kiosk application.
    /// </summary>
    public partial class Shell : Window, INotifyPropertyChanged
    {
        private const int WM_INPUT = 0x00FF;
        Message message = new Message();
        ModuleManager moduleManager;
        IModule currentModule;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
         

        public Shell()
        {         
            // Commented below 2 lines as Logon function has been moved to ModuleSelectionPresenter.cs
            // Modified By JK on 01/09/12
            //LogOnKioskResponse logonResponse = new LogOnKioskResponse();
            //LogonServiceClient logonproxy = null;
            bool LoadApp = true;
            //this.ScrollerText = string.Empty;
           // System.Configuration.Configuration config;

            bool showCursor = (ConfigurationManager.AppSettings["ShowCursor"] == null) ? false : Convert.ToBoolean(ConfigurationManager.AppSettings["ShowCursor"]);

            if (showCursor)
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            else
                this.Cursor = System.Windows.Input.Cursors.None;
            
            InitializeComponent();
                        
            //if (LoadApp)
            //{
            //    // Init cash acceptor device and printer devices.
                 
                //DeviceAgent.GetInstance().Init();
                //string result = Logger.GetRecentScrollers();
                //if ((!string.IsNullOrEmpty(result)) &&
                //    (KioskAppConfig.Scrollers != result))
                //{
                //    KioskAppConfig.Scrollers = result;
                     
                //}
                moduleManager = ModuleManager.GetInstance();
                currentModule = moduleManager.GetDefaultModule();
                currentModule.ModuleLayoutUpdatedEvent += OnModuleLayoutUpdated;
                currentModule.ModuleSelectionChangedEvent += OnModuleSelectionChanged;
                currentModule.Activate();
                this.BeginInit();
                try
                {
                    this.ccModule.Content = null;
                    this.ccModule.Content = currentModule.ShellGrid;

                }
                finally
                {
                    this.EndInit();
                    this.UpdateLayout();
                }
            //}
            //else
            //{
            //    if (log.IsErrorEnabled) log.ErrorFormat("Logon not Successfull");
            //    Application.Current.Shutdown(1);
            //}
        }
        
        #region Event handling for repainting the module view updates and module switching.
        private void OnModuleLayoutUpdated()
        {
            this.BeginInit();
            try
            {
                this.ccModule.Content = currentModule.ShellGrid;
            }
            finally
            {
                this.EndInit();
                this.UpdateLayout();
            }
        }

        private void OnModuleSelectionChanged(ModuleSelectionChangedEventArgs obj)
        {

            IModule newModule;
            newModule = ModuleManager.GetInstance().SwitchToModule(obj.NewModule);
            
            
            if (newModule != null)
            {
                textBlock.Text = KioskAppConfig.GetCurrentScrollerText(obj.NewModule.ToLower().Replace("module", string.Empty));

                if (KioskAppConfig.ShowScroller)
                    grScroller.Visibility = System.Windows.Visibility.Visible;
                else
                    grScroller.Visibility = System.Windows.Visibility.Collapsed;

                currentModule.ModuleSelectionChangedEvent -= OnModuleSelectionChanged;
                currentModule.ModuleLayoutUpdatedEvent -= OnModuleLayoutUpdated;
                currentModule.Deactivate();

                currentModule = newModule;
                currentModule.ModuleLayoutUpdatedEvent += OnModuleLayoutUpdated;
                currentModule.ModuleSelectionChangedEvent += OnModuleSelectionChanged;
                currentModule.Activate(obj.DispatcherAction);
                this.BeginInit();
                try
                {
                    this.ccModule.Content = currentModule.ShellGrid;
                }
                finally
                {
                    this.EndInit();
                    this.UpdateLayout();
                }
            }
        }

        #endregion

        public string ShellHeight
        {
            get
            {
                string resolution = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["app_ht"]) ? "1024" : ConfigurationManager.AppSettings["app_ht"];
                return resolution;
            }
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            message.HWnd = hwnd;
            message.Msg = msg;
            message.LParam = lParam;
            message.WParam = wParam;

            if (message.Msg == WM_INPUT)
            {
                DeviceAgent.GetInstance().ProcessMessage(message);
            }
            return IntPtr.Zero;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            // I am new to WPF and I don't know where else to call this function.
            // It has to be called after the window is created or the handle won't
            // exist yet and the function will throw an exception.
            StartWndProcHandler();
            // Init cash acceptor device and printer devices.
            DeviceAgent.GetInstance().Init();
            base.OnSourceInitialized(e);
        }

        void StartWndProcHandler()
        {
            IntPtr hwnd = IntPtr.Zero;
            Window myWin = System.Windows.Application.Current.MainWindow;

            try
            {
                hwnd = new WindowInteropHelper(myWin).Handle;
                Keyboard.ClearFocus();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            //Get the Hwnd source   
            HwndSource source = HwndSource.FromHwnd(hwnd);
            //Win32 queue sink
            source.AddHook(new HwndSourceHook(WndProc));
            DeviceAgent.GetInstance().RegisterSource(source.Handle);
        }
        
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                  new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }
}
