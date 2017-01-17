using System;
using System.IO;


namespace Kragle
{
    /// <summary>
    ///     Interface for interacting with the filesystem.
    /// </summary>
    internal class FileStore
    {
        private readonly DirectoryInfo _rootDir;


        /// <summary>
        ///     Constructs a new <code>FileStore</code>.
        /// </summary>
        /// <param name="rootDir">the root directory of the file store</param>
        public FileStore(string rootDir)
        {
            _rootDir = Directory.CreateDirectory(rootDir);
        }


        /// <summary>
        ///     Writes data to a file. If the file and/or directory do not exist, they will be created.
        /// </summary>
        /// <param name="directory">the directory the file is in/will be in</param>
        /// <param name="file">the file to write to</param>
        /// <param name="contents">the new contents for the file</param>
        /// <param name="append"><code>true</code> if the file should be appended rather than overwritten</param>
        protected void WriteFile(string directory, string file, string contents, bool append)
        {
            DirectoryInfo subDir = _rootDir.CreateSubdirectory(directory);
            string filePath = subDir.FullName + "/" + file;

            if (append)
            {
                using (StreamWriter fileStream = File.CreateText(filePath))
                {
                    fileStream.Write(contents);
                }
            }
            else
            {
                using (StreamWriter fileStream = File.AppendText(filePath))
                {
                    fileStream.Write(contents);
                }
            }
        }

        /// <summary>
        ///     Writes data to a file. If the file does not exist, it will be created.
        /// </summary>
        /// <param name="file">the file to write to</param>
        /// <param name="contents">the new contents for the file</param>
        /// <param name="append">
        ///     should be <code>true</code> if the file should be appended rather than overwritten
        /// </param>
        protected void WriteFile(string file, string contents, bool append = false)
        {
            WriteFile(_rootDir.FullName, file, contents, append);
        }

        /// <summary>
        ///     Reads the data from a file. If the file does not exist, an exception is thrown.
        /// </summary>
        /// <param name="directory">the subdirectory relative to this <code>FileStore</code>'s root</param>
        /// <param name="file">the name of the file within the directory</param>
        /// <returns>the contents of the file</returns>
        protected string ReadFile(string directory, string file)
        {
            string filePath = _rootDir + "/" + directory + "/" + file;
            if (!File.Exists(filePath))
            {
                throw new Exception(""); // TODO decide on what exception to throw
            }

            return File.ReadAllText(filePath);
        }

        /// <summary>
        ///     Reads the data from a file. If the file does not exist, an exception is thrown.
        /// </summary>
        /// <param name="file">the name of the file within the directory</param>
        /// <returns>the contents of the file</returns>
        protected string ReadFile(string file)
        {
            return ReadFile(_rootDir.FullName, file);
        }
    }
}
