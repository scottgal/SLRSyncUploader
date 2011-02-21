using System;
using System.IO;
using System.Text;

namespace NetSync
{
    internal class Log
    {
        /// <summary>
        /// Writes string to log adding newLine character at the end
        /// </summary>
        /// <param name="str"></param>
        public static void WriteLine(string str)
        {
            LogWrite(str + Environment.NewLine);
        }

        /// <summary>
        /// Writes string to log
        /// </summary>
        /// <param name="str"></param>
        public static void Write(string str)
        {
            LogWrite(str);
        }

        /// <summary>
        /// Empty method at this moment
        /// </summary>
        /// <param name="file"></param>
        /// <param name="initialStats"></param>
        public static void LogSend(FileStruct file, Stats initialStats)
        {
        }

        /// <summary>
        /// Writes string to logFile or to console if client
        /// </summary>
        /// <param name="str"></param>
        private static void LogWrite(string str)
        {
            if (Daemon.ServerOptions != null)
            {
                if (Daemon.ServerOptions.logFile == null)
                {
                    try
                    {
                        Daemon.ServerOptions.logFile =
                            new FileStream(Path.Combine(Environment.SystemDirectory, "rsyncd.log"),
                                           FileMode.OpenOrCreate | FileMode.Append, FileAccess.Write);
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
                str = "[ " + DateTime.Now + " ] " + str;
                Daemon.ServerOptions.logFile.Write(Encoding.ASCII.GetBytes(str), 0, str.Length); //@todo cyrillic
                Daemon.ServerOptions.logFile.Flush();
            }
            else
            {
                if (!MainClass.Opt.amDaemon)
                {
                    Console.Write(str);
                }
            }
        }
    }
}