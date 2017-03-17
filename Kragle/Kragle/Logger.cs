using System;
using System.IO;


namespace Kragle
{
    /// <summary>
    ///     A logger that logs to the console output and to a file. Objects of this class should be treated as if they
    ///     were a stream to the log file, where the <code>Log</code> and <code>LogLine</code> methods incidentally also
    ///     calll <code>Console.Write</code> or <code>Console.WriteLine</code> methods, respectively.
    /// </summary>
    public class Logger : IDisposable
    {
        private readonly StreamWriter _output;
        private bool _autoFlush = true;
        private bool _console = true;


        /// <summary>
        ///     Constructs a new <code>Logger</code>. This logger only writes to the console output, not to a file.
        /// </summary>
        public Logger()
        {
            _output = null;
        }

        /// <summary>
        ///     Constructs a new <code>Logger</code> that writes to both the console output and the given file.
        /// </summary>
        /// <param name="output">the path to the file to write the log to</param>
        public Logger(string output)
        {
            _output = new StreamWriter(output) {AutoFlush = false};
        }

        /// <summary>
        ///     The <code>Logger</code> writes to the console output if and only if this value is set to
        ///     <code>true</code>; calls to <code>Log</code> and <code>LogLine</code> are simply ignored, not
        ///     buffered.
        ///     Defaults to <code>true</code>.
        /// </summary>
        public bool Console
        {
            get { return _console; }
            set { _console = value; }
        }

        /// <summary>
        ///     If set to <code>true</code>, each call to <code>Log</code> or <code>LogLine</code> will also
        ///     initiate a call to <code>Flush</code>. If set to <code>false</code>, it is guaranteed that the file
        ///     stream does not automatically flush; the console output stream may still automatically flush depending
        ///     on its own setting.
        ///     Setting this field to <code>true</code> will also directly call <code>Flush</code>.
        ///     Defaults to <code>true</code>.
        /// </summary>
        public bool AutoFlush
        {
            get { return _autoFlush; }
            set
            {
                _autoFlush = value;
                if (value)
                {
                    Flush();
                }
            }
        }


        public void Dispose()
        {
            if (_output != null)
            {
                _output.Dispose();
            }
        }


        /// <summary>
        ///     Logs a string.
        /// </summary>
        /// <param name="text">a string</param>
        /// <returns>this <code>Logger</code></returns>
        public Logger Log(string text)
        {
            if (Console)
            {
                System.Console.Write(text);
            }
            if (_output != null)
            {
                _output.Write(text);
            }

            if (AutoFlush)
            {
                Flush();
            }

            return this;
        }

        /// <summary>
        ///     Logs a string and appends a newline.
        /// </summary>
        /// <param name="text">a string</param>
        /// <returns>this <code>Logger</code></returns>
        public Logger LogLine(string text)
        {
            return Log(text + Environment.NewLine);
        }

        /// <summary>
        ///     Flushes the console (if the <code>Console</code> field is set to <code>true</code>) and the file stream
        ///     (if there is one).
        /// </summary>
        /// <returns>this <code>Logger</code></returns>
        public Logger Flush()
        {
            if (Console)
            {
                System.Console.Out.Flush();
            }
            if (_output != null)
            {
                _output.Flush();
            }

            return this;
        }
    }
}
