using System;
using System.IO;
using System.Text.RegularExpressions;


namespace Kragle.Parse
{
    /// <summary>
    ///     The <code>CsvWriter</code> can be used to write data in CSV format. It can automatically append a newline
    ///     character after a given number of arguments have been written, and the write methods return the instance of
    ///     the used <code>CsvWriter</code> to allow the chaining of write calls.
    /// </summary>
    public class CsvWriter : IDisposable
    {
        private readonly int _lineArguments;
        private readonly StreamWriter _writer;

        private int _linePosition;


        /// <summary>
        ///     Constructs a new <code>CsvWriter</code>.
        /// </summary>
        /// <param name="file">the path to the file to write to</param>
        /// <param name="append">true if the given file should be appended</param>
        /// <param name="lineArguments">
        ///     the number of arguments per line; if this value is non-zero a newline will be appended after this many
        ///     arguments have been written
        /// </param>
        public CsvWriter(string file, bool append = false, int lineArguments = 0)
        {
            _lineArguments = lineArguments;
            _writer = new StreamWriter(file, append);

            _linePosition = 0;
        }

        /// <summary>
        ///     Disposes of this <code>CsvWriter</code> and its resources.
        /// </summary>
        public void Dispose()
        {
            _writer.Dispose();
        }


        /// <summary>
        ///     Writes the given strings as parameters without escaping, separated by commas.
        /// </summary>
        /// <param name="data">the names of the columns</param>
        /// <returns>this <code>CsvWriter</code></returns>
        public CsvWriter WriteHeaders(params string[] data)
        {
            Write(string.Join(",", data), false);
            Newline();
            return this;
        }

        /// <summary>
        ///     Writes the given data to the CSV. All strings are escaped by surrounding them with ".
        /// </summary>
        /// <param name="data">the string data to write</param>
        /// <returns>this <code>CsvWriter</code></returns>
        public CsvWriter Write(object data)
        {
            return Write(data.ToString());
        }

        /// <summary>
        ///     Writes the given string data to the CSV. All strings are escaped by surrounding them with ".
        /// </summary>
        /// <param name="data">the string data to write</param>
        /// <returns>this <code>CsvWriter</code></returns>
        public CsvWriter Write(string data)
        {
            Write(data, true);
            return this;
        }

        /// <summary>
        ///     Writes the given integer data to the CSV.
        /// </summary>
        /// <param name="data">the integer data to write</param>
        /// <returns>this <code>CsvWriter</code></returns>
        public CsvWriter Write(int data)
        {
            Write(data.ToString(), false);
            return this;
        }

        /// <summary>
        ///     Writes a newline character to the CSV.
        /// </summary>
        /// <returns>this <code>CsvWriter</code></returns>
        public CsvWriter Newline()
        {
            _writer.WriteLine();
            _linePosition = 0;
            return this;
        }


        /// <summary>
        ///     Escapes data by surrounding it with " symbols.
        /// </summary>
        /// <param name="data">the data to escape</param>
        /// <returns>the escaped data</returns>
        private static string EscapeData(string data)
        {
            return "\"" + Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(data,
                                   @"\r\n?|\n", "\\n"), // Replace newlines
                               "\"", "\"\""), // Replace quotes with double quotes
                           "\\\\", "\\\\"), // Replace backslashes with double backslashes
                       "\0", "") // Remove NUL bytes
                   + "\"";
        }

        /// <summary>
        ///     Writes the given data to the CSV.
        /// </summary>
        /// <param name="data">the data to write</param>
        /// <param name="escape">true if the data should be escaped</param>
        private void Write(dynamic data, bool escape = false)
        {
            if (_linePosition != 0)
            {
                _writer.Write(",");
            }

            _writer.Write(escape ? EscapeData(data) : data);
            _linePosition++;

            if (_lineArguments > 0 && _lineArguments <= _linePosition)
            {
                Newline();
            }
        }
    }
}
