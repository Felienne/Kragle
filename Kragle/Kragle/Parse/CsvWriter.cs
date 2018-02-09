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
        private readonly string _file;
        private readonly string[] _headers;
        private readonly int _linesPerFile;

        private StreamWriter _writer;
        private int _linePosition;
        private int _linesWritten;


        /// <summary>
        ///     Constructs a new <code>CsvWriter</code>.
        /// </summary>
        /// <param name="file">the path to the file to write to, without the file extension</param>
        /// <param name="headers">
        ///     the headers to add to the CSV file(s), or <code>null</code> if there are no headers
        /// </param>
        /// <param name="linesPerFile">
        ///     the number of lines per file (including the header) before a new file should be created; if this value
        ///     is zero only one file will be used, and if this value is non-zero the filename will be appended by a
        ///     number
        /// </param>
        public CsvWriter(string file, string[] headers = null, int linesPerFile = 0)
        {
            _file = file;
            _headers = headers ?? new string[0];
            _linesPerFile = linesPerFile;

            _writer = new StreamWriter(FormatFileName(), false);
            _linePosition = 0;
            _linesWritten = 0;

            WriteHeaders();
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
        /// <returns>this <code>CsvWriter</code></returns>
        private CsvWriter WriteHeaders()
        {
            if (_headers.Length == 0)
            {
                return this;
            }

            Write(string.Join(",", _headers), false);
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
        }

        /// <summary>
        ///     Writes a newline character to the CSV.
        /// </summary>
        /// <returns>this <code>CsvWriter</code></returns>
        public CsvWriter Newline()
        {
            _writer.WriteLine();
            _linePosition = 0;
            _linesWritten++;

            if (_linesPerFile > 0 && _linesWritten >= _linesPerFile)
            {
                _writer.Dispose();
                _writer = new StreamWriter(FormatFileName(), false);
            }

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
        ///     Computes the filename to write to, depending on the number of lines per file and the number of lines
        ///     written.
        /// </summary>
        /// <returns>
        ///     the filename to write to, depending on the number of lines per file and the number of lines
        ///     written.
        /// </returns>
        private string FormatFileName()
        {
            if (_linesPerFile == 0)
            {
                return _file + ".csv";
            }

            if (_linesWritten == 0)
            {
                return _file + "_000.csv";
            }

            return _file + "_" + (_linesPerFile % _linesWritten).ToString("D3") + ".csv";
        }
    }
}
