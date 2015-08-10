using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.Interfaces;
using System.Windows;
using System.IO;
using System.Diagnostics;
using MBMEKiosk.Infrastructure.Utils;
using System.Windows.Markup;
using MBMEKiosk.ObjectModel;
using MBMEKiosk.Infrastructure.Events;
using System.ComponentModel;
using MBMEKiosk.Infrastructure.ObjectModel;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Input;
using log4net;

//[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace MBMEKiosk.Infrastructure.BaseClasses
{
    public abstract class KioskViewPresenterBase : IKioskViewPresenter, INotifyPropertyChanged
    {
        private FrameworkElement viewGrid;
        private KioskState state;
        private TransactionContextBase transactionContext;
        

        public event Action<KioskStateChangedEventArgs> KioskStateChangedEvent;

        public event Action<ModuleSelectionChangedEventArgs> ModuleSelectionChangedEvent;

        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public virtual FrameworkElement LoadXaml(KioskState state, TransactionContextBase transactionContext)
        {
            try
            {
                Trace.WriteLine(string.Format("{0} - BaseMethod loadxaml started for {0}..", DateTime.Now, state.Name));
                //MessageBox.Show(state.Name);
                RegisterCommands();
                Trace.WriteLine(string.Format("{0} - RegisterCommands Successful", DateTime.Now, state.Name));
                this.State = state;
                this.transactionContext = transactionContext;

                FrameworkElement root = LoadXamlFile(state.XamlPath);
                
                if(root==null)
                    Trace.WriteLine(string.Format("{0} - root element is null..", DateTime.Now, state.Name)); 
                else
                    Trace.WriteLine(string.Format("{0} - root element created", DateTime.Now, state.Name)); 

                root.BeginInit();
                try
                {
                    root.DataContext = this;
                    root.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(OnPreviewKeyDown);
                }
                catch (Exception ex)
                {
                    // KS TODO: Add log messages.
                    throw ex;
                }
                finally
                {
                    root.EndInit();
                    ViewGrid = root;
                    root.UpdateLayout();
                }
            }
            catch (Exception ex)
            {
                //if (log.IsErrorEnabled) log.Error("Caught exception in LoadXaml method" + ex.Message); 
                Trace.TraceError("Exception details: \nSource: {0}\nMessage: {1}\nTrace:", ex.Source, ex.Message, ex.StackTrace);
                throw ex;
            }

            return ViewGrid;
        }

        protected FrameworkElement LoadXamlFile(string filePath)
        {
            FrameworkElement root = null;
            try
            {
                FileStream s = new FileStream(filePath, FileMode.Open);
                root = XamlReader.Load(s) as FrameworkElement;
                s.Close();
            }
            catch (Exception ex)
            {
                //if (log.IsErrorEnabled) log.Error("Caught exception in LoadXamlFile method" + ex.Message); 
                Trace.TraceError("Exception details: \nSource: {0}\nMessage: {1}\nTrace:", ex.Source, ex.Message, ex.StackTrace);
                MessageBox.Show(ex.Message);
            }

            return root;
        }

        protected virtual void OnKioskStateChanged(KioskStateChangedEventArgs args)
        {
            try
            {
                var handler = this.KioskStateChangedEvent;
                if (handler != null)
                {
                    handler.Invoke(args);
                }
            }
            catch (Exception ex)
            {
                //if (log.IsErrorEnabled) log.Error("Caught exception in OnKioskStateChanged" + ex.Message); 
            }
        }

        //public event Action<ModuleSelectionChangedEventArgs> ModuleSelectionChangedEvent;
        protected virtual void OnPreviewKeyDown(Object sender, System.Windows.Input.KeyEventArgs e)        
        {
            if (Keyboard.IsKeyDown(Key.RightShift) && Keyboard.IsKeyDown(Key.Enter))
            {
                Trace.WriteLine("SHIFT+ENTER Is pressed");
                if (this.state.Name == "ModuleSelection")
                {
                    this.OnKioskStateChanged(new KioskStateChangedEventArgs("admin"));
                }
            }
            return;
        }

        protected virtual void OnModuleSelectionChanged(ModuleSelectionChangedEventArgs args)
        {
            var handler = ModuleSelectionChangedEvent;
            if (handler != null)
            {
                handler.Invoke(args);
            }
        }

        public event Action RequestLayoutUpdate;

        protected void OnRequestLayoutUpdate()
        {
            Action handler = RequestLayoutUpdate;
            if (handler != null)
            {
                handler();
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        protected abstract void RegisterCommands();

        protected abstract void ReevaulateIfCommandsCanExecute();

        public FrameworkElement ViewGrid
        {
            get
            {
                return viewGrid;
            }

            set
            {
                if (viewGrid != value)
                {
                    viewGrid = value;
                    OnPropertyChanged("ViewGrid");
                }
            }
        }

        public KioskState State
        {
            get
            {
                return state;
            }
            protected set
            {
                if (state != value)
                {
                    state = value;
                    ReevaulateIfCommandsCanExecute();
                    OnPropertyChanged("State");
                }
            }
        }

        public TransactionContextBase TransactionContext
        {
            get
            {
                return transactionContext;
            }
            protected set
            {
                if (transactionContext != value)
                {
                    transactionContext = value;
                    ReevaulateIfCommandsCanExecute();
                    OnPropertyChanged("TransactionContext");
                }
            }
        }
    }
}
