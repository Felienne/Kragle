using System;
using System.IO;


namespace Kragle
{
    /// <summary>
    ///     Interface for interacting with the filesystem.
    /// </summary>
    public class FileStore
    {
        private const string KraglePath = "/Kragle";

        private readonly DirectoryInfo _rootDir;


        /// <summary>
        ///     Constructs a new <code>FileStore</code> in the default directory.
        /// </summary>
        public FileStore() : this(null)
        {
        }

        /// <summary>
        ///     Constructs a new <code>FileStore</code>.
        /// </summary>
        /// <param name="rootDir">
        ///     the root directory of the file store, or <code>null</code> for the default directory
        /// </param>
        public FileStore(string rootDir)
        {
            if (rootDir == null)
            {
                rootDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + KraglePath;
            }

            _rootDir = Directory.CreateDirectory(rootDir);
        }


        /// <summary>
        ///     Writes data to a file. If the file and/or directory do not exist, they will be created.
        /// </summary>
        /// <param name="directory">the directory the file is in/will be in</param>
        /// <param name="file">the file to write to</param>
        /// <param name="contents">the new contents for the file</param>
        /// <param name="append"><code>true</code> if the file should be appended rather than overwritten</param>
        public void WriteFile(string directory, string file, string contents, bool append = false)
        {
            DirectoryInfo subDir = _rootDir.CreateSubdirectory(directory);
            string filePath = subDir.FullName + "/" + file;

            if (append)
            {
                using (StreamWriter fileStream = File.AppendText(filePath))
                {
                    fileStream.Write(contents);
                }
            }
            else
            {
                using (StreamWriter fileStream = File.CreateText(filePath))
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
        public void WriteFile(string file, string contents, bool append = false)
        {
            WriteFile("./", file, contents, append);
        }

        /// <summary>
        ///     Reads the data from a file. If the file does not exist, an exception is thrown.
        /// </summary>
        /// <param name="directory">the subdirectory relative to this <code>FileStore</code>'s root</param>
        /// <param name="file">the name of the file within the directory</param>
        /// <returns>the contents of the file</returns>
        public string ReadFile(string directory, string file)
        {
            string filePath = _rootDir.FullName + "/" + directory + "/" + file;
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The file " + filePath + " does not exist");
            }

            return File.ReadAllText(filePath);
        }

        /// <summary>
        ///     Reads the data from a file. If the file does not exist, an exception is thrown.
        /// </summary>
        /// <param name="file">the name of the file within the directory</param>
        /// <returns>the contents of the file</returns>
        public string ReadFile(string file)
        {
            return ReadFile("./", file);
        }

        /// <summary>
        ///     Returns true if the specified file exists.
        /// </summary>
        /// <param name="directory">the directory the file is in</param>
        /// <param name="file">the filename</param>
        /// <returns><code>true</code> if the specified file exists</returns>
        public bool FileExists(string directory, string file)
        {
            return File.Exists(_rootDir.FullName + "/" + directory + "/" + file);
        }

        /// <summary>
        ///     Returns true if the specified file exists.
        /// </summary>
        /// <param name="file">the filename</param>
        /// <returns><code>true</code> if the specified file exists</returns>
        public bool FileExists(string file)
        {
            return FileExists("./", file);
        }

        /// <summary>
        ///     Removes the file.
        /// </summary>
        /// <param name="directory">the directory the file is in</param>
        /// <param name="file">the filename</param>
        public void RemoveFile(string directory, string file)
        {
            try
            {
                File.Delete(_rootDir.FullName + "/" + directory + "/" + file);
            }
            catch (FileNotFoundException)
            {
                // If the file cannot be found, we can consider it deleted
            }
            catch (DirectoryNotFoundException)
            {
                // If the file cannot be found, we can consider it deleted
            }
        }

        /// <summary>
        ///     Removes the file.
        /// </summary>
        /// <param name="file">the filename</param>
        public void RemoveFile(string file)
        {
            RemoveFile("./", file);
        }

        /// <summary>
        ///     Returns true if the specified directory exists.
        /// </summary>
        /// <param name="directory">true if the specified directory exists</param>
        public bool DirectoryExists(string directory)
        {
            return Directory.Exists(_rootDir.FullName + "/" + directory);
        }

        /// <summary>
        ///     Removes a directory and all the contained files.
        /// </summary>
        /// <param name="directory">a directory</param>
        public void RemoveDirectory(string directory)
        {
            try
            {
                Directory.Delete(_rootDir.FullName + "/" + directory, true);
            }
            catch (DirectoryNotFoundException)
            {
                // If the directory cannot be found, we can consider it deleted
            }
        }
    }
}
