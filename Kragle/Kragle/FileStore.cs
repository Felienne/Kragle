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

        private static DirectoryInfo _rootDir;

        static FileStore()
        {
            Init();
        }


        /// <summary>
        ///     Sets the root directory for the <code>FileStore</code> to the default directory.
        /// </summary>
        public static void Init()
        {
            Init(null);
        }

        /// <summary>
        ///     Sets the root directory for the <code>FileStore</code>.
        /// </summary>
        /// <param name="rootDir">
        ///     the root directory of the file store, or <code>null</code> for the default directory
        /// </param>
        public static void Init(string rootDir)
        {
            if (rootDir == null)
            {
                rootDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + KraglePath;
            }

            _rootDir = Directory.CreateDirectory(rootDir);
        }


        /// <summary>
        ///     Returns the full path to this <code>FileStore</code>'s root directory.
        /// </summary>
        /// <returns>the full path to this <code>FileStore</code>'s root directory</returns>
        public static string GetRootPath()
        {
            return _rootDir.FullName;
        }

        /// <summary>
        ///     Returns the absolute path to the directory and file.
        /// </summary>
        /// <param name="directory">the subdirectory</param>
        /// <param name="file">the file</param>
        /// <returns>the absolute path to the directory and file</returns>
        public static string GetAbsolutePath(string directory, string file = "")
        {
            return Path.GetFullPath(_rootDir.FullName + "/" + directory + "/" + file);
        }

        /// <summary>
        ///     Writes data to a file. If the file and/or directory do not exist, they will be created.
        /// </summary>
        /// <param name="directory">the directory the file is in/will be in</param>
        /// <param name="file">the file to write to</param>
        /// <param name="contents">the new contents for the file</param>
        /// <param name="append"><code>true</code> if the file should be appended rather than overwritten</param>
        public static void WriteFile(string directory, string file, string contents, bool append = false)
        {
            CreateDirectory(directory);
            string filePath = GetAbsolutePath(directory, file);

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
        public static void WriteFile(string file, string contents, bool append = false)
        {
            WriteFile("./", file, contents, append);
        }

        /// <summary>
        ///     Reads the data from a file. If the file does not exist, an exception is thrown.
        /// </summary>
        /// <param name="directory">the subdirectory relative to this <code>FileStore</code>'s root</param>
        /// <param name="file">the name of the file within the directory</param>
        /// <returns>the contents of the file</returns>
        public static string ReadFile(string directory, string file)
        {
            string filePath = GetAbsolutePath(directory, file);
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
        public static string ReadFile(string file)
        {
            return ReadFile("./", file);
        }

        /// <summary>
        ///     Returns true if the specified file exists.
        /// </summary>
        /// <param name="directory">the directory the file is in</param>
        /// <param name="file">the filename</param>
        /// <returns><code>true</code> if the specified file exists</returns>
        public static bool FileExists(string directory, string file)
        {
            return File.Exists(GetAbsolutePath(directory, file));
        }

        /// <summary>
        ///     Returns true if the specified file exists.
        /// </summary>
        /// <param name="file">the filename</param>
        /// <returns><code>true</code> if the specified file exists</returns>
        public static bool FileExists(string file)
        {
            return FileExists("./", file);
        }

        /// <summary>
        ///     Removes the file.
        /// </summary>
        /// <param name="directory">the directory the file is in</param>
        /// <param name="file">the filename</param>
        public static void RemoveFile(string directory, string file)
        {
            try
            {
                File.Delete(GetAbsolutePath(directory, file));
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
        public static void RemoveFile(string file)
        {
            RemoveFile("./", file);
        }

        /// <summary>
        ///     Copies a file.
        /// </summary>
        /// <param name="fromDirectory">the directory of the source file</param>
        /// <param name="fromFile">the name of the source file</param>
        /// <param name="toDirectory">the directory of the destination file</param>
        /// <param name="toFile">the name of the destination file</param>
        public static void CopyFile(string fromDirectory, string fromFile, string toDirectory, string toFile)
        {
            File.Copy(GetAbsolutePath(fromDirectory, fromFile), GetAbsolutePath(toDirectory, toFile));
        }

        /// <summary>
        ///     Copies a file.
        /// </summary>
        /// <param name="fromFile">the name of the source file</param>
        /// <param name="toFile">the name of the destination file</param>
        public static void CopyFile(string fromFile, string toFile)
        {
            CopyFile("./", fromFile, "./", toFile);
        }

        /// Creates a new subdirectory.
        /// </summary>
        /// <param name="directory">he name of the subdirectory</param>
        /// <returns>the <code>DirectoryInfo</code> on the new subdirectory</returns>
        public static DirectoryInfo CreateDirectory(string directory = "./")
        {
            return GetDirectory(directory);
        }

        /// <summary>
        ///     Returns true if the specified directory exists.
        /// </summary>
        /// <param name="directory">true if the specified directory exists</param>
        public static bool DirectoryExists(string directory)
        {
            return Directory.Exists(GetAbsolutePath(directory));
        }

        /// <summary>
        ///     Removes a directory and all the contained files.
        /// </summary>
        /// <param name="directory">a directory</param>
        public static void RemoveDirectory(string directory = "./")
        {
            try
            {
                GetDirectory(directory).Delete(true);
            }
            catch (DirectoryNotFoundException)
            {
                // If the directory cannot be found, we can consider it deleted
            }
        }

        /// <summary>
        ///     Returns an array of <code>DirectoryInfo</code>s on all subdirectories in the specified (sub)directory.
        /// </summary>
        /// <param name="directory">the name of a subdirectory</param>
        /// <returns>an array of <code>DirectoryInfo</code>s on all subdirectories in the specified (sub)directory</returns>
        public static DirectoryInfo[] GetDirectories(string directory = "./")
        {
            return GetDirectory(directory).GetDirectories();
        }

        /// <summary>
        ///     Returns an array of <code>FileInfo</code> on all files in the specified (sub)directory.
        /// </summary>
        /// <param name="directory">the name of a subdirectory</param>
        /// <returns>an array of <code>FileInfo</code> on all files in the specified (sub)directory</returns>
        public static FileInfo[] GetFiles(string directory = "./")
        {
            return GetDirectory(directory).GetFiles();
        }


        /// <summary>
        ///     Returns the <code>DirectoryInfo</code> on the specified subdirectory.
        /// </summary>
        /// <param name="directory">the name of a subdirectory</param>
        /// <returns>the <code>DirectoryInfo</code> on the specified subdirectory</returns>
        private static DirectoryInfo GetDirectory(string directory = "./")
        {
            return Directory.CreateDirectory(GetAbsolutePath(directory));
        }
    }
}
