using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sapp
{
    public abstract class Logger
    {
        public static readonly bool DebugMode = true;
        private static readonly string logPath = Settings.FILE_LOCATION + @"\log.txt";
        //private static StreamWriter logFile;

        public static void Log(string message, bool includeDataTime = false)
        {
            WriteLog("LOG: " + message, includeDataTime);
        }

        public static void LogError(string message, bool includeDataTime = false)
        {
            WriteLog("ERROR: " + message, includeDataTime);
        }

        public static void LogWarning(string message, bool includeDataTime = false)
        {
            WriteLog("WARN: " + message, includeDataTime);
        }

        private static void WriteLog(string message, bool includeDataTime = false)
        {
            if (DebugMode)
            {
                try
                {
                    StreamWriter logFile = new StreamWriter(logPath, true);
                    logFile.Write(message);

                    if (includeDataTime)
                        logFile.Write(" -- " + DateTime.Now);

                    logFile.WriteLine();
                    logFile.Flush();
                    logFile.Close();
                }
                catch
                {
                    //well shit
                }
            }
        }
    }
}
