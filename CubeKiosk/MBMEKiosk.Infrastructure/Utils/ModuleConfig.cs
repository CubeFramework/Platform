using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml.Linq;
using MBMEKiosk.Infrastructure.Interfaces;
using MBMEKiosk.Infrastructure.ObjectModel;
using MBMEKiosk.ObjectModel;

namespace MBMEKiosk.Infrastructure.Utils
{
    /// <summary>
    /// This Class Loads the Module from Config File 
    /// </summary>
    public class ModuleConfig
    {
        private string configFilePath;
        public string ShellXamlPath { get; private set; }
        //public string DefaultStyleDictionaryPath { get; private set; }
        //public string DefaultStyleDictionaryKey { get; private set; }
        public string DefaultBackgroundImagePath { get; private set; }
        public int IdleTimeOutInSeconds { get; private set; }
        public int MessageTimeOutInSeconds { get; private set; }

        public List<KioskState> states = new List<KioskState>();

        public string CurrentLanguageKey { get; set; }
        public List<KioskLanguage> kioskLanguages = new List<KioskLanguage>();
        /**** Added By Jags on 01/04/12 ******/
        public string CurrentStyleKey { get; set; }
        public List<KioskStyle> kioskStyles = new List<KioskStyle>();


        public ModuleConfig(string moduleConfigFilePath)
        {
            configFilePath = moduleConfigFilePath;
            LoadModuleConfigFile();
        }

        /// <summary>
        /// load config file
        /// </summary>
        /// <returns></returns>
        private XElement LoadXML(string path)
        {
            try
            {
                XElement root;
                if (File.Exists(path))
                {
                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                    root = XElement.Load(fs, LoadOptions.PreserveWhitespace);
                    fs.Close();
                }
                else
                {
                    throw new Exception(string.Format("{0} file not found", path));
                }

                return root;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void LoadModuleConfigFile()
        {
            IEnumerable<KioskLanguage> languages = null;
            IEnumerable<KioskState> list = null;
            /*** Added By Jags on 01/04/2012 *****/
            IEnumerable<KioskStyle> styles = null;

            try
            {
                XElement root = LoadXML(configFilePath);
                string location = root.Element("location").Attribute("path").Value;
                string shellPath = string.Concat(location, root.Element("defaultshell").Attribute("path").Value);
                //this.DefaultStyleDictionaryPath = string.Concat(location, root.Element("defaultstyledictionary").Attribute("path").Value);
                //this.DefaultStyleDictionaryKey = string.Concat(location, root.Element("defaultstyledictionary").Attribute("key").Value);
                this.DefaultBackgroundImagePath = string.Concat(location, root.Element("defaultbackground").Attribute("path").Value);
                this.IdleTimeOutInSeconds = (root.Element("idletimeout")==null) ? 0 : Convert.ToInt32(root.Element("idletimeout").Attribute("seconds").Value);
                this.MessageTimeOutInSeconds = (root.Element("messagetimeout") == null) ? 0 : Convert.ToInt32(root.Element("messagetimeout").Attribute("seconds").Value);

                if (root.Element("languageoptions") != null)
                {
                    languages = from language in root.Element("languageoptions").Elements("language")
                                select new KioskLanguage
                                {
                                    Key = (language.Attribute("key") == null) ? null : language.Attribute("key").Value,
                                    DictionaryPath = (language.Attribute("path") == null) ? null : string.Concat(location, language.Attribute("path").Value),
                                    IsDefault = Convert.ToBoolean(language.Attribute("default") == null ? "false" : language.Attribute("default").Value)
                                };
                }
                if(root.Element("styleoptions") != null)
                {
                styles = from style in root.Element("styleoptions").Elements("style")
                         select new KioskStyle
                         {
                             Key = (style.Attribute("key") == null) ? null : style.Attribute("key").Value,
                             DictionaryPath = (style.Attribute("path") == null) ? null : string.Concat(location, style.Attribute("path").Value),
                             IsDefault = Convert.ToBoolean(style.Attribute("default") == null ? "false" : style.Attribute("default").Value)
                         };

                }
                
                list = from state in root.Element("states").Elements("state")
                       select new KioskState
                       {
                           Name = (state.Attribute("name") == null) ? null : state.Attribute("name").Value,
                           PresenterName = (state.Element("view") == null) ? null : state.Element("view").Attribute("name").Value.ToString(),
                           IsDefault = Convert.ToBoolean(state.Attribute("default") == null ? "false" : state.Attribute("default").Value),
                           XamlPath = string.Concat(location, (state.Attribute("file") == null) ? null : state.Attribute("file").Value),
                           ShellXamlPath = shellPath,
                           BackgroundImagePath = (state.Element("view").Attribute("background") == null) ? this.DefaultBackgroundImagePath : string.Concat(location, state.Element("view").Attribute("background").Value),
                           //StyleDictionaryPath = (state.Element("view").Attribute("styledictionary") == null) ? this.DefaultStyleDictionaryPath : string.Concat(location, state.Element("view").Attribute("styledictionary").Value),
                           //StyleDictionaryPath = (state.Element("view").Attribute("styledictionarypath") == null) ? null : string.Concat(location, state.Element("view").Attribute("styledictionarypath").Value),
                           StyleDictionaryKey = (state.Element("view").Attribute("styledictionarykey") == null) ? null : state.Element("view").Attribute("styledictionarykey").Value,
                           ViewHeaderKey = (state.Element("view").Attribute("headerkey") == null) ? string.Empty : state.Element("view").Attribute("headerkey").Value,
                           ViewContentKey = (state.Element("view").Attribute("contentkey") == null) ? string.Empty : state.Element("view").Attribute("contentkey").Value,

                           KioskActions = from action in state.Element("actions").Elements("action")
                                          select new KioskAction
                                          {
                                              Name = (action.Attribute("name") == null) ? null : action.Attribute("name").Value,
                                              AcquiredState = (action.Attribute("acquiredstate") == null) ? null : action.Attribute("acquiredstate").Value,
                                              SwitchModule = (action.Attribute("switchmodule") == null) ? false : Convert.ToBoolean(action.Attribute("switchmodule").Value),
                                              HelperClassMethod = (action.Attribute("helperclassmethod") == null) ? null : action.Attribute("helperclassmethod").Value,
                                              HelperClassName = (action.Attribute("helperclassname") == null) ? null : action.Attribute("helperclassname").Value,
                                              ServiceOperationName = (action.Attribute("serviceoperationname") == null) ? null : action.Attribute("serviceoperationname").Value
                                          },

                           IdleTimeOut = (state.Attribute("timeout") == null) ? IdleTimeOutInSeconds : Convert.ToInt32(state.Attribute("timeout").Value),
                           MessageTimeOut = (state.Attribute("timeout") == null) ? MessageTimeOutInSeconds : Convert.ToInt32(state.Attribute("timeout").Value),
                           ServiceName = (state.Attribute("servicename") == null) ? string.Empty : state.Attribute("serviceName").Value,
                           LeadDigits = (state.Attribute("leaddigits") == null) ? string.Empty : state.Attribute("leaddigits").Value,
                           MinAccNoLength = (state.Attribute("minlength") == null) ? 0 : Convert.ToInt32(state.Attribute("minlength").Value),
                           MaxAccNoLength = (state.Attribute("maxlength") == null) ? 0 : Convert.ToInt32(state.Attribute("maxlength").Value),
                           Denomination = (state.Attribute("denomination") == null) ? string.Empty : state.Attribute("denomination").Value,
                           // Added By JK on 19/06/2012
                           RetryCount = (state.Attribute("retrycount") == null) ? (byte)0 : Convert.ToByte(state.Attribute("retrycount").Value),
                           // Added By JK to allow payment b/w min, and max range. 
                           MinAmount = (state.Attribute("min") == null) ? -1 : Convert.ToInt32(state.Attribute("min").Value),
                           MaxAmount = (state.Attribute("max") == null) ? -1 : Convert.ToInt32(state.Attribute("max").Value)                           
                       };
            }
            catch (Exception ex)
            {
                // KS TODO: Handle more specific excpetions and log the same.
                throw ex;
            }

            /*** Added By Jags on 01/04/2012 ******************/
            if (languages != null)
            {
                foreach (KioskLanguage language in languages)
                {
                    kioskLanguages.Add(language);
                }
            }

            CurrentLanguageKey = GetDefaultLanguageDictionaryKey();

            /*** Added By Jags on 01/04/2012 ******************/
            if (styles != null)
            {
                foreach (KioskStyle style in styles)
                {
                    kioskStyles.Add(style);
                }
            }

            CurrentStyleKey = GetDefaultStyleDictionaryKey();

            foreach (KioskState state in list)
            {
                states.Add(state);
            }

            CurrentState = GetStateByName(GetDefaultStateName());
        }

        public string GetNewStateNameByAction(string actionName)
        {
            string newState = string.Empty;
            var action = (from act in CurrentState.KioskActions
                         where act.Name == actionName
                         select act).FirstOrDefault() as KioskAction;

            if ((action as KioskAction) != null)
            {
                newState = (from aState in states
                             where aState.Name.ToString().ToLower() == action.AcquiredState.ToLower()
                             select aState.Name).FirstOrDefault();
            }

            return newState;
        }

        public KioskState GetStateByName(string stateName = null)
        {
            KioskState state = null;

            if (string.IsNullOrEmpty(stateName))
            {
                stateName = GetDefaultStateName();
            }

            if (!string.IsNullOrEmpty(stateName))
            {
                state = (from aState in states
                         where aState.Name.ToString().ToLower() == stateName.ToLower()
                         select aState).FirstOrDefault() as KioskState;
            }

            return state;
        }

        private string GetDefaultStateName()
        {
            return (from aState in states
                            where aState.IsDefault == true
                            select aState.Name).FirstOrDefault();
        }

        public void ResetToDefaultStateAndLanguage()
        {
            CurrentLanguageKey = GetDefaultLanguageDictionaryKey();
            CurrentState = this.GetStateByName(GetDefaultStateName());
        }

        public void ResetToDefaultStateLanguageAndStyle()
        {
            CurrentLanguageKey = GetDefaultLanguageDictionaryKey();
            CurrentStyleKey = GetDefaultStyleDictionaryKey();
            CurrentState = this.GetStateByName(GetDefaultStateName());
        }

        public FrameworkElement LoadState(TransactionContextBase transactionContext, IDeviceAgent devices, string stateName = null)
        {
            Trace.WriteLine(string.Format("{0} - LoadState Started for State {1} ", DateTime.Now, stateName));
            FrameworkElement root = null;
            try
            {
                if (string.IsNullOrEmpty(stateName))
                {
                    stateName = GetDefaultStateName();
                    Trace.WriteLine(string.Format("{0} - LoadState Default State {1} ", DateTime.Now, stateName)); 
                }

                CurrentState = GetStateByName(stateName);
                Trace.WriteLine(string.Format("{0} - LoadState CurrentState {1} ", DateTime.Now, CurrentState));

                if (CurrentState != null)
                {
                    Trace.WriteLine(string.Format("{0} - Loading Class {1} ", DateTime.Now, CurrentState.PresenterName));
                    Type Class = CurrentState.PresenterClass.GetType();
                    root = Class.InvokeMember("LoadXaml", 
                        BindingFlags.InvokeMethod, 
                        Type.DefaultBinder, 
                        CurrentState.PresenterClass,
                        new object[] { devices, this.CurrentState, transactionContext }) as FrameworkElement;
                    Trace.WriteLine(string.Format("{0} - Loaded Class {1} ", DateTime.Now, CurrentState.PresenterName));
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("{0} Error in LoadState StateName: {1} Error Desc: {2}",DateTime.Now.ToString(),CurrentState, ex.Message));
                Trace.WriteLine(string.Format("{0} Error in LoadState StateName: {1} Stacktrace: {2}", DateTime.Now.ToString(), CurrentState, ex.StackTrace));
                Trace.WriteLine(string.Format("{0} - Inner Exception : {1} \n Inner StackTrace: {2} ", (ex.InnerException == null) ? "" : ex.InnerException.Message, (ex.InnerException == null) ? "" : ex.InnerException.StackTrace));
                //throw ex;
            }

            return root;
        }

        public string GetDefaultLanguageDictionaryPath()
        {
            var kioskLanguage = (from language in kioskLanguages
                        where language.IsDefault == true
                        select language).FirstOrDefault();

            /*** Added By Jags on 01/04/12 to handle the common case.*/
            if (kioskLanguage == null)
                return null;

            return kioskLanguage.DictionaryPath;
        }

        public string GetDefaultLanguageDictionaryKey()
        {
            var kioskLanguage = (from language in kioskLanguages
                                 where language.IsDefault == true
                                 select language).FirstOrDefault();

            /*** Added By Jags on 01/04/12 to handle the common case.*/
            if (kioskLanguage == null)
                return null;
            return kioskLanguage.Key;
        }

        public string GetCurrentLanguageDictionaryPath()
        {
            var kioskLanguage = (from language in kioskLanguages
                        where string.CompareOrdinal(language.Key, CurrentLanguageKey) == 0
                        select language).FirstOrDefault();

            if (kioskLanguage != null)
            {
                return kioskLanguage.DictionaryPath; 
            }

            return GetDefaultLanguageDictionaryPath();
        }

        /*****************Added By Jags 01/04/2012*****************************/

        public string GetDefaultStyleDictionaryPath()
        {
            var kioskStyle = (from style in kioskStyles
                              where style.IsDefault == true
                              select style).FirstOrDefault();

            if (kioskStyle == null)
                return null;
            return kioskStyle.DictionaryPath;
        }

        public string GetDefaultStyleDictionaryKey()
        {
            var kioskStyle = (from style in kioskStyles
                              where style.IsDefault == true
                              select style).FirstOrDefault();

            if (kioskStyle == null)
                return null;
            return kioskStyle.Key;
        }

        public string GetCurrentStyleDictionaryPath()
        {
            var kioskStyle = (from style in kioskStyles
                              where string.CompareOrdinal(style.Key, CurrentStyleKey) == 0
                              select style).FirstOrDefault();

            if (kioskStyle == null)
                return GetDefaultStyleDictionaryPath();

            return kioskStyle.DictionaryPath;
        }

        /******************************************************************************************/
        public KioskState CurrentState { get; private set; }
    }
}
