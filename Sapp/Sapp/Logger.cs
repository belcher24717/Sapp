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
        private static readonly string logPath = @".\log.txt";
        //private static StreamWriter logFile;

        public static void Log(string message)
        {
            Log(message, false);
        }


        public static void Log(string message, bool includeDataTime)
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

                    logFile.Close();
                }
                catch
                {

                }
            }

        }
    }
}
