using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Configuration;

namespace MBMEKioskLogger.Logger
{

    public static class FileLogger
    {
        
        public static void Error(string message, string module)
        {
            WriteEntry(message, "error", module);
        }

        public static void Error(Exception ex, string module)
        {
             //StringBuilder   str  = new StringBuilder();
             //str.Append(ex.InnerException.Source.ToString() + Environment.NewLine + ex.InnerException.TargetSite);
             //str.Append(ex.InnerException.StackTrace.ToString() + Environment.NewLine + ex.InnerException.Message);

            WriteEntry(ex.Message.ToString(), "error", module);
        }

        public static void Warning(string message, string module)
        {
            WriteEntry(message, "warning", module);
        }

        public static void Info(string message, string module)
        {
            WriteEntry(message, "info", module);
        }

        private static void WriteEntry(string message, string type, string module)
        {

            try
            {
                bool enableTrace = bool.Parse(ConfigurationManager.AppSettings["EnableTrace"].ToString());
                if (enableTrace)
                {
                    Trace.WriteLine(
                        string.Format("{0},{1},{2},{3}",
                                      DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                      type,
                                      module,
                                      message));
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(
                    string.Format("{0},{1},{2},{3}",
                                  DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                  "null",
                                  "FileLogger",
                                  ex.Message));
            }
            

            
        }

    }
}
