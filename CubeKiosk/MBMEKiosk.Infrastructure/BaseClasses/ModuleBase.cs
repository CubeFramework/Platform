using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using MBMEKiosk.Infrastructure.Events;
using MBMEKiosk.Infrastructure.Interfaces;
using MBMEKiosk.Infrastructure.ObjectModel;
using MBMEKiosk.Infrastructure.Utils;
using MBMEKiosk.ObjectModel;
using log4net;

namespace MBMEKiosk.Infrastructure.BaseClasses
{
    public abstract class ModuleBase : IModule
    {
        private bool isActive;
        private string configPath;
        private ModuleConfig stateManager;
        private ShellPresenterBase shellPresenter;
        private FrameworkElement shellGrid;
        private TransactionContextBase currentTransactionContext;
        //private bool isCashCycleInProgress;
        private IDeviceAgent devices;
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected ModuleBase(IDeviceAgent deviceAgent)
        {
            devices = deviceAgent;
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

        #region IModule Members

        public virtual void Deactivate()
        {
            isActive = false;
            // Add logic to deactivate to reset to inactive state of the app module.
        }

        protected virtual void IntializeTransaction()
        {
            CurrentTransactionContext = new TransactionContextBase();
        }

        protected virtual void IntializeShellPresenter()
        {
            // Create the parent page + initial/default state/view of the app module.
            ShellPresenter = new ShellPresenterBase();
        }

        public virtual void Activate(string dispatcherAction=null)
        {
            try
            {
                isActive = true;

                IntializeTransaction();

                CurrentTransactionContext.BranchId = "1234509876";
                CurrentTransactionContext.DispatcherAction = dispatcherAction;
                //CurrentTransactionContext.MachineId = "0";
                //CurrentTransactionContext.MachineLocation = "ENINTHANE";
                ////CurrentTransactionContext.Id = DateTime.Now.ToString("yyMMddHHmmssff") + this.CurrentTransactionContext.MachineId;

                IntializeShellPresenter();

                ShellPresenter.KioskStateChangedEvent += OnKioskStateChangedEvent;

                // Load the default shell for the state.
                //Commented by Jags on 01/04/2012 ****/
                //this.StateManager.ResetToDefaultStateAndLanguage();
                this.StateManager.ResetToDefaultStateLanguageAndStyle();
                this.CurrentTransactionContext.CurrentLanguageDictionaryPath = this.StateManager.GetCurrentLanguageDictionaryPath();
                ShellGrid = ShellPresenter.LoadXaml(StateManager.CurrentState, CurrentTransactionContext);

                // Load the default resource dictionary.
                var shell = ShellPresenter.ViewGrid as FrameworkElement;
                if (shell != null)
                {
                    //using (FileStream fs = new FileStream(StateManager.CurrentState.StyleDictionaryPath, FileMode.Open))
                    this.StateManager.CurrentStyleKey = CurrentTransactionContext.SelectedStyleKey = this.StateManager.GetDefaultStyleDictionaryKey();
                    using (FileStream fs = new FileStream(StateManager.GetDefaultStyleDictionaryPath(), FileMode.Open))
                    {
                        ResourceDictionary dic = (ResourceDictionary)XamlReader.Load(fs);
                        //if (log.IsInfoEnabled) log.Info(" Loading Style Dictionary : " + this.StateManager.CurrentStyleKey + " - State :" + StateManager.CurrentState.Name);
                        shell.Resources.MergedDictionaries.Clear();
                        shell.Resources.MergedDictionaries.Add(dic);
                    }

                    // Load the default language dictionary.
                    /**** Added By Jags on 01/04/2012 ******/
                    if (this.StateManager.GetDefaultLanguageDictionaryKey() != null)
                    {
                        this.StateManager.CurrentLanguageKey = this.StateManager.GetDefaultLanguageDictionaryKey();
                        using (FileStream fs = new FileStream(StateManager.GetCurrentLanguageDictionaryPath(), FileMode.Open))
                        {
                            //if (log.IsInfoEnabled) log.Info(" Loading Language Dictionary : " + this.StateManager.CurrentLanguageKey + " - State :" + StateManager.CurrentState.Name);
                            ResourceDictionary dic = (ResourceDictionary)XamlReader.Load(fs);
                            shell.Resources.MergedDictionaries.Add(dic);
                        }
                    }
                }

                // Load the default current state view.
                ShellPresenter.Child = StateManager.LoadState(CurrentTransactionContext, devices);
                ShellPresenter.RefreshShellForNewState(StateManager.CurrentState);
                RaiseModuleLayoutUpdatedEvent();
                PresenterBase currentstatePresenter = StateManager.CurrentState.PresenterClass as PresenterBase;

                if (currentstatePresenter != null)
                {
                    currentstatePresenter.RequestLayoutUpdate += ChildPresenter_LayoutUpdate;
                    currentstatePresenter.KioskStateChangedEvent += OnKioskStateChangedEvent;
                }
            }
            catch (Exception ex)
            {
                //if (log.IsInfoEnabled) log.Info("Caught exception in ModuleBase Activate.." + ex.Message + " State :" + StateManager.CurrentState.Name); 
                Trace.WriteLine("Caught exception in ModuleBase Activate.." + ex.Message + " State :" + StateManager.CurrentState.Name + "stacktrace: " + ex.StackTrace);
            }
        }

        private void ChildPresenter_LayoutUpdate()
        {
            ShellPresenter.Child.BeginInit();
            ShellPresenter.Child.DataContext = null;
            ShellPresenter.Child.DataContext = StateManager.CurrentState.PresenterClass as PresenterBase;
            ShellPresenter.Child.EndInit();
            ShellPresenter.Child.UpdateLayout();

            // Refresh the shell view as well.
            ShellPresenter.RefreshShellForNewState(StateManager.CurrentState);
        }

        private void OnKioskStateChangedEvent(KioskStateChangedEventArgs args)
        {
            Trace.WriteLine(string.Format("{0} OnKioskStateChangedEvent Started",DateTime.Now));
            bool isLanguageChanged = false;
            bool isStyleChanged = false;
            try
            {
                KioskAction action = StateManager.CurrentState.KioskActions.Where(a => string.CompareOrdinal(a.Name, args.Action) == 0).FirstOrDefault();
                Trace.WriteLine(string.Format(" Current state: {0} - action : {1}", StateManager.CurrentState.Name, args.Action));
                // Deregister old child view events.
                PresenterBase currentstatePresenter = StateManager.CurrentState.PresenterClass as PresenterBase;
                if (currentstatePresenter != null)
                {
                    Trace.WriteLine(string.Format("{0} CurrentStatePresenter is not null", DateTime.Now));
                    currentstatePresenter.Deactivate();
                    currentstatePresenter.KioskStateChangedEvent -= OnKioskStateChangedEvent;
                    currentstatePresenter.RequestLayoutUpdate -= ChildPresenter_LayoutUpdate;
                }

                if (action != null && (action.SwitchModule))
                {
                    Trace.WriteLine(string.Format("{0} Deactivate the current Module.", DateTime.Now));
                     
                    // Deactivate the current module.
                    this.Deactivate();

                    // Switch to a different module or home.
                    Trace.WriteLine(string.Format("{0} switching to new state {1} ", DateTime.Now,action.AcquiredState));
                    RaiseModuleSelectionChangedEvent(action.AcquiredState, args.Action);
                    return;
                }

                // Switch to the requested state in the same module.
                string newStateNameRequested = StateManager.GetNewStateNameByAction(args.Action);
                Trace.WriteLine(string.Format("{0} New state {1} Requested with action {2}", DateTime.Now,newStateNameRequested,args.Action));
                KioskState newStateRequested = StateManager.GetStateByName(newStateNameRequested);

                if (newStateRequested.IsDefault && string.CompareOrdinal(StateManager.CurrentLanguageKey, StateManager.GetDefaultLanguageDictionaryKey()) != 0)
                {
                    isLanguageChanged = true;
                    this.StateManager.CurrentLanguageKey = this.StateManager.GetDefaultLanguageDictionaryKey();
                    //Trace.WriteLine(string.Format("{0} Current Language Key {2} Loaded", DateTime.Now, this.StateManager.CurrentLanguageKey));
                    Trace.WriteLine(string.Format("{0} Current Language Key {1} Loaded", DateTime.Now, this.StateManager.CurrentLanguageKey));
                    //this.CurrentTransactionContext.SelectedLanguageKey = this.StateManager.CurrentLanguageKey;
                    this.CurrentTransactionContext.CurrentLanguageDictionaryPath = this.StateManager.GetCurrentLanguageDictionaryPath();
                    //Trace.WriteLine(string.Format("{0} Current Language Dictionary Path {2} Loaded", DateTime.Now, this.CurrentTransactionContext.CurrentLanguageDictionaryPath));
                    Trace.WriteLine(string.Format("{0} Current Language Dictionary Path {1} Loaded", DateTime.Now, this.CurrentTransactionContext.CurrentLanguageDictionaryPath));
                }
                else if (string.CompareOrdinal(StateManager.CurrentLanguageKey, CurrentTransactionContext.SelectedLanguageKey) != 0)
                {
                    isLanguageChanged = true;
                    if (CurrentTransactionContext.SelectedLanguageKey != null)
                        StateManager.CurrentLanguageKey = CurrentTransactionContext.SelectedLanguageKey;
                    this.CurrentTransactionContext.CurrentLanguageDictionaryPath = this.StateManager.GetCurrentLanguageDictionaryPath();
                }

                /*** Added By Jags on 02/04/2012 to handle multiple Style Dictionaries********************************************/

                if (newStateRequested.IsDefault && string.CompareOrdinal(StateManager.CurrentStyleKey, StateManager.GetDefaultStyleDictionaryKey()) != 0)
                {

                    isStyleChanged = true;
                    this.StateManager.CurrentStyleKey = this.StateManager.GetDefaultStyleDictionaryKey();
                    //this.CurrentTransactionContext.SelectedStyleKey = this.StateManager.CurrentStyleKey;
                    this.CurrentTransactionContext.CurrentStyleDictionaryPath = this.StateManager.GetDefaultStyleDictionaryPath();
                    Trace.WriteLine(string.Format(" {0} - Style Loaded {1} Language loaded {2}", DateTime.Now, stateManager.CurrentStyleKey, stateManager.CurrentLanguageKey));
                }
                else if ((newStateRequested.StyleDictionaryKey == null) && (string.CompareOrdinal(StateManager.CurrentStyleKey, CurrentTransactionContext.SelectedStyleKey) != 0))
                {

                    isStyleChanged = true;
                    if (CurrentTransactionContext.SelectedStyleKey != null)
                        StateManager.CurrentStyleKey = CurrentTransactionContext.SelectedStyleKey;
                    this.CurrentTransactionContext.CurrentStyleDictionaryPath = this.StateManager.GetCurrentStyleDictionaryPath();
                    Trace.WriteLine(string.Format(" {0} - Style Loaded {1} Language loaded {2}", DateTime.Now, stateManager.CurrentStyleKey, stateManager.CurrentLanguageKey));
                }
                else if ((newStateRequested.StyleDictionaryKey != null) && (newStateRequested.StyleDictionaryKey != StateManager.CurrentStyleKey))
                {

                    isStyleChanged = true;
                    //if (newStateRequested.StyleDictionaryKey != null)
                    //{
                    StateManager.CurrentStyleKey = newStateRequested.StyleDictionaryKey;
                    this.CurrentTransactionContext.CurrentStyleDictionaryPath = this.StateManager.GetCurrentStyleDictionaryPath();
                    //}
                    //else
                    //{
                    //    StateManager.CurrentStyleKey = CurrentTransactionContext.SelectedStyleKey;
                    //    this.CurrentTransactionContext.CurrentStyleDictionaryPath = this.StateManager.GetCurrentStyleDictionaryPath();
                    //}
                    Trace.WriteLine(string.Format(" {0} - Style Loaded {1} Language loaded {2}", DateTime.Now, stateManager.CurrentStyleKey, stateManager.CurrentLanguageKey));
                }

                //if (log.IsInfoEnabled) log.Info(" Current state: " + StateManager.CurrentState.Name + ", action : " + args.Action + ", new state: " + newStateNameRequested);
                Trace.WriteLine(string.Format(" Current state: {0} - action : {1} - new state: {2}", StateManager.CurrentState.Name, args.Action, newStateNameRequested));
                var shell = ShellPresenter.ViewGrid as FrameworkElement;
                if (isLanguageChanged || (isStyleChanged))
                {
                    shell.Resources.MergedDictionaries.Clear();
                    //if ( (newStateRequested.StyleDictionaryPath != null) && (string.CompareOrdinal(this.StateManager.GetCurrentStyleDictionaryPath(), newStateRequested.StyleDictionaryPath) != 0))
                    //using (FileStream fs = new FileStream(newStateRequested.StyleDictionaryPath, FileMode.Open))
                    //{
                    //    ResourceDictionary dic = (ResourceDictionary)XamlReader.Load(fs);
                    //    shell.Resources.MergedDictionaries.Add(dic);
                    //}
                    //else if(this.StateManager.CurrentStyleKey == null)
                    //using (FileStream fs = new FileStream(StateManager.DefaultStyleDictionaryPath, FileMode.Open))
                    //{
                    //    ResourceDictionary dic = (ResourceDictionary)XamlReader.Load(fs);
                    //    shell.Resources.MergedDictionaries.Add(dic);
                    //}
                    //else
                    //using (FileStream fs = new FileStream(StateManager.GetCurrentStyleDictionaryPath(), FileMode.Open))
                    //{
                    //    ResourceDictionary dic = (ResourceDictionary)XamlReader.Load(fs);
                    //    shell.Resources.MergedDictionaries.Add(dic);
                    //}

                    if (StateManager.CurrentLanguageKey != null)
                        using (FileStream fs = new FileStream(StateManager.GetCurrentLanguageDictionaryPath(), FileMode.Open))
                        {
                            Trace.WriteLine(string.Format(" {0} - Current Language Dic Path {1} ", DateTime.Now, stateManager.GetCurrentLanguageDictionaryPath()));
                            ResourceDictionary dic = (ResourceDictionary)XamlReader.Load(fs);
                            //if (log.IsInfoEnabled) log.Info(" Loading Language Dictionary : " + this.StateManager.CurrentLanguageKey + " - State :" + StateManager.CurrentState.Name);
                            shell.Resources.MergedDictionaries.Add(dic);
                        }
                    if (StateManager.CurrentStyleKey != null)
                        using (FileStream fs = new FileStream(StateManager.GetCurrentStyleDictionaryPath(), FileMode.Open))
                        {
                            Trace.WriteLine(string.Format(" {0} - Current Style Dic Path {1} ", DateTime.Now, stateManager.GetCurrentStyleDictionaryPath()));
                            ResourceDictionary dic = (ResourceDictionary)XamlReader.Load(fs);
                            //if (log.IsInfoEnabled) log.Info(" Loading Style Dictionary : " + this.StateManager.CurrentStyleKey + " - State :" + StateManager.CurrentState.Name);
                            shell.Resources.MergedDictionaries.Add(dic);
                        }

                }

                // Load the new state and register for corresponding events.
                ShellPresenter.Child = StateManager.LoadState(CurrentTransactionContext, devices, newStateNameRequested);
                ChildPresenter_LayoutUpdate();
                //ShellPresenter.RefreshShellForNewState(StateManager.CurrentState);
                currentstatePresenter = StateManager.CurrentState.PresenterClass as PresenterBase;
                if (currentstatePresenter != null)
                {
                    currentstatePresenter.RequestLayoutUpdate += ChildPresenter_LayoutUpdate;
                    currentstatePresenter.KioskStateChangedEvent += OnKioskStateChangedEvent;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("{0} - Error in KioskStateChangedEventArgs - {1} Stack Trace {2}", DateTime.Now, ex.Message, ex.StackTrace));
                Trace.WriteLine(string.Format("{0} - Inner Error in KioskStateChangedEventArgs - {1} \n Inner Stack Trace {2}", DateTime.Now, (ex.InnerException == null) ? "" : ex.InnerException.Message, (ex.InnerException == null) ? "" : ex.InnerException.StackTrace));
                //if (log.IsInfoEnabled) log.Info("Caught exception in OnKioskStateChangedEvent." + ex.Message + " - State :" + StateManager.CurrentState.Name); 
            }
        }

        public event Action<ModuleSelectionChangedEventArgs> ModuleSelectionChangedEvent;

        protected void RaiseModuleSelectionChangedEvent(string newModule, string dispatcherAction=null)
        {
            Action<ModuleSelectionChangedEventArgs> handler = ModuleSelectionChangedEvent;
            if (handler != null)
            {
                handler(new ModuleSelectionChangedEventArgs(newModule, dispatcherAction));
            }
        }

        public event Action ModuleLayoutUpdatedEvent;

        protected void RaiseModuleLayoutUpdatedEvent()
        {
            Action handler = ModuleLayoutUpdatedEvent;
            if (handler != null)
            {
                handler();
            }
        }

        #region Properties

        public string ConfigPath
        {
            get
            {
                return configPath;
            }

            protected set
            {
                if (string.Compare(configPath, value, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    configPath = value;
                    OnPropertyChanged("ConfigPath");
                }
            }
        }

        public FrameworkElement ShellGrid
        {
            get
            {
                return shellGrid;
            }

            protected set
            {
                if (shellGrid != value)
                {
                    shellGrid = value;
                    OnPropertyChanged("ShellGrid");
                }
            }
        }

        public virtual ModuleConfig StateManager
        {
            get
            {
                if (stateManager == null)
                {
                    stateManager = new ModuleConfig(ConfigPath);
                }

                return stateManager;
            }
        }

        public ShellPresenterBase ShellPresenter
        {
            get
            {
                return shellPresenter;
            }

            protected set
            {
                if (shellPresenter != value)
                {
                    shellPresenter = value;
                    OnPropertyChanged("ShellPresenter");
                }
            }
        }

        public virtual TransactionContextBase CurrentTransactionContext
        {
            get
            {
                return currentTransactionContext;
            }

            protected set
            {
                if (currentTransactionContext != value)
                {
                    currentTransactionContext = value;
                    OnPropertyChanged("CurrentTransactionContext");
                }
            }
        }

        #endregion

        #endregion
    }
}
