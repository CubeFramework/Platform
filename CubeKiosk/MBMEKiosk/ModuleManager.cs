using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using MBMEKiosk.Infrastructure.BaseClasses;
using MBMEKiosk.Infrastructure.Interfaces;
using System.Threading;
using MBMEKiosk.Infrastructure.ObjectModel;

namespace MBMEKiosk
{
    public class ModuleManager
    {
        private static ModuleManager instance = null;
        private static Dictionary<string, IModule> modulesCatalog;
        ////private IModule activeModule;

        private ModuleManager()
        {
            modulesCatalog = new Dictionary<string, IModule>();
        }

        public static ModuleManager GetInstance()
        {
            if (instance == null)
            {
                instance = new ModuleManager();
                InitializeLoadableModules(ConfigurationManager.AppSettings["ModulesPath"]);
            }

            return instance;
        }

        public IModule SwitchToModule(string moduleKey)
        {
            //// KS TODO: Add logic to read/match from config file.
            if (!modulesCatalog.ContainsKey(moduleKey))
            {
                return null;
            }
            
            CurrentModule = modulesCatalog[moduleKey] as IModule;
            return CurrentModule;
        }

        public IModule CurrentModule { get; private set; }

        public IModule GetDefaultModule()
        {
            return SwitchToModule(modulesCatalog.Keys.Where(k => k.Contains("MBMEModule")).FirstOrDefault());
        }

        // KS TODO: Update the logic to support known modules as configured for the KIOSK using config file.
        private static void InitializeLoadableModules(string relativePath)
        {
            modulesCatalog.Clear();
            
            String[] astrLoadableFiles = Directory.GetFiles(relativePath, "*.dll");

            foreach (String strFile in astrLoadableFiles)
            {
                Assembly assembly = null;
                try
                {
#if DEBUG
                    //assembly = Assembly.LoadFrom(strFile);
                    assembly = Assembly.Load(File.ReadAllBytes(strFile));
#else
                    try
                    {
                        // It is important to use this load call in release mode, otherwise you will lose the ability to update modules
                        // without shutting down the application
                        assembly = Assembly.Load(File.ReadAllBytes(strFile));
                    }
                    catch (IOException ioex)
                    {
                        Trace.TraceError("Warning! Error loading file.\r\n\r\n{0}\r\n\r\n{1}\r\n\r\n{2}", ioex.Message, ioex.Source, ioex.StackTrace);
                        
                        // Allow a little time for the system to finalize the file.
                        // FileSystemWatcher gives us notifications of file changes before the file
                        // is actually finalized
                        Thread.Sleep(50);
                        
                        // try again to load it
                        assembly = Assembly.Load(File.ReadAllBytes(strFile));
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Error! Error loading file.\r\n\r\n{0}\r\n\r\n{1}\r\n\r\n{2}", ex.Message, ex.Source, ex.StackTrace);
                        throw ex;
                    }
#endif
                }
                catch (BadImageFormatException)
                {
                    // Assembly was not a valid assembly or it targets a later version of .NET framework than is loaded.
                    continue;
                }

                try
                {
                    // Check if the loaded assembly is a loadable kiosk app module.
                    var moduleTypesPerAssembly = assembly.GetTypes().Where<Type>(m => m.BaseType == typeof(ModuleBase));

                    // A kiosk app must have exactly one class of type IModule.
                    if (moduleTypesPerAssembly != null && moduleTypesPerAssembly.Count() == 1)
                    {
                        Type loadableModuleType = moduleTypesPerAssembly.First();
                        if (!modulesCatalog.Keys.Contains(loadableModuleType.Name))
                        {
                            // Initialize the module and add it to the catalog.
                            var module = Activator.CreateInstance(loadableModuleType, DeviceAgent.GetInstance(), ConfigurationManager.AppSettings[loadableModuleType.Name]) as IModule;

                            if (module != null)
                            {
                                modulesCatalog.Add(loadableModuleType.Name, module);
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("{0} is not a loadable module", strFile);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Error! Error loading file.\r\n\r\n{0}\r\n\r\n{1}\r\n\r\n{2}", ex.Message, ex.Source, ex.StackTrace);
                }
            }
        }

        

        
    }
}
