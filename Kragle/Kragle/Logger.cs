using System;
using System.Collections.Generic;
using System.IO;


namespace Kragle
{
    /// <summary>
    ///     A logger that logs to the console output and to a file. Objects of this class should be treated as if they
    ///     were a stream to the log file, where the <code>Log</code> and <code>LogLine</code> methods incidentally also
    ///     calll <code>EnableConsole.Write</code> or <code>EnableConsole.WriteLine</code> methods, respectively.
    /// </summary>
    public class Logger
    {
        private static readonly Dictionary<string, Logger> Loggers = new Dictionary<string, Logger>();
        private static readonly object SyncLock = new object();
        private static readonly StreamWriter FileStream;

        public readonly string Name;

        static Logger()
        {
            string logFile = string.Format("log_{0:yyyyMMMMddhhmmss}.log", DateTime.Now);
            FileStream = new StreamWriter(logFile);
        }

        /// <summary>
        ///     Constructs a new <code>Logger</code>. This logger only writes to the console output, not to a file.
        /// </summary>
        private Logger(string name)
        {
            Name = name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Name == ((Logger) obj).Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <summary>
        ///     Returns the <code>Logger</code> instance with the given name.
        /// </summary>
        /// <param name="name">the name of the <code>Logger</code> to return</param>
        /// <returns>the <code>Logger</code> instance with the given name</returns>
        public static Logger GetLogger(string name = "")
        {
            lock (SyncLock)
            {
                if (!Loggers.ContainsKey(name))
                {
                    Loggers.Add(name, new Logger(name));
                }

                return Loggers[name];
            }
        }


        /// <summary>
        ///     Logs a string.
        /// </summary>
        /// <param name="name">the name of the logging <code>Logger</code></param>
        /// <param name="text">a string</param>
        private static void Log(string name, string text)
        {
            string logText = name == ""
                ? string.Format("[{0}] [{1}] {2}\n", DateTime.Now, name, text)
                : string.Format("[{0}] {1}\n", DateTime.Now, text);

            Console.Write(logText);
            if (FileStream != null)
            {
                FileStream.Write(logText);
            }

            Flush();
        }

        /// <summary>
        ///     Flushes the console (if the <code>EnableConsole</code> field is set to <code>true</code>) and the file stream
        ///     (if there is one).
        /// </summary>
        private static void Flush()
        {
            Console.Out.Flush();
            if (FileStream != null)
            {
                FileStream.Flush();
            }
        }


        /// <summary>
        ///     Logs a string.
        /// </summary>
        /// <param name="text">a string</param>
        public void Log(string text)
        {
            Log(Name, text);
        }
    }
}
