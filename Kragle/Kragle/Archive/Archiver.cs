using System;
using System.IO;
using System.IO.Compression;
using Kragle.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Kragle.Archive
{
    /// <summary>
    ///     Manages archives.
    /// </summary>
    public class Archiver
    {
        private static readonly Logger Logger = Logger.GetLogger("Archiver");


        /// <summary>
        ///    Creates a separate archive for each user containing all that user's data.
        /// </summary>
        /// <param name="overwrite">true if existing archives should be overwritten</param>
        public void Archive(bool overwrite = false)
        {
            FileInfo[] users = FileStore.GetFiles(Resources.UserDirectory);
            int userTotal = users.Length;
            int userCurrent = 0;

            foreach (FileInfo user in users)
            {
                userCurrent++;
                Logger.Log(LoggerHelper.FormatProgress(
                    "Archiving user " + LoggerHelper.ForceLength(user.Name, 10), userCurrent, userTotal));

                string username = user.Name.Remove(user.Name.Length - 5);
                Archive(username);
            }
        }

        /// <summary>
        ///     Extracts all archives into the data folder.
        /// </summary>
        /// <param name="overwrite">true if existing files should be overwritten</param>
        public void Extract(bool overwrite = false)
        {
            FileInfo[] archives = FileStore.GetFiles(Resources.ArchiveDirectory);
            int archiveTotal = archives.Length;
            int archiveCurrent = 0;

            foreach (FileInfo archive in archives)
            {
                archiveCurrent++;
                Logger.Log(LoggerHelper.FormatProgress(
                    "Extracting archive " + LoggerHelper.ForceLength(archive.Name, 10), archiveCurrent, archiveTotal));

                Extract(archive, overwrite);
            }
        }


        /// <summary>
        ///     Creates an archive for the specified user containing all that user's data.
        /// </summary>
        /// <param name="username">a username</param>
        private static void Archive(string username)
        {
            FileStore.CreateDirectory(Resources.ArchiveDirectory);

            if (FileStore.FileExists(Resources.ArchiveDirectory, username + ".zip"))
            {
                return;
            }
            string archivePath = FileStore.GetAbsolutePath(Resources.ArchiveDirectory, username + ".zip");

            using (var fileStream = new FileStream(archivePath, FileMode.CreateNew))
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
            {
                ArchiveUserData(archive, username);
                ArchiveCurrentProjectList(archive, username);
                ArchiveOldProjectLists(archive, username);
                ArchiveProjectCode(archive, username);
            }
        }

        /// <summary>
        ///     Archives the metadata of the specified user.
        /// </summary>
        /// <param name="archive">the archive to write to</param>
        /// <param name="username">the username</param>
        private static void ArchiveUserData(ZipArchive archive, string username)
        {
            string userDataPath = Resources.UserDirectory + "/" + username + ".json";
            archive.CreateEntryFromFile(FileStore.GetAbsolutePath(userDataPath), userDataPath);
        }

        /// <summary>
        ///     Archives the most recent project list of the specified user.
        /// </summary>
        /// <param name="archive">the archive to write to</param>
        /// <param name="username">the username</param>
        private static void ArchiveCurrentProjectList(ZipArchive archive, string username)
        {
            string projectListPath = Resources.ProjectDirectory + "/" + username + ".json";
            archive.CreateEntryFromFile(FileStore.GetAbsolutePath(projectListPath), projectListPath);
        }

        /// <summary>
        ///     Archives previous project lists of the specified user.
        /// </summary>
        /// <param name="archive">the archive to write to</param>
        /// <param name="username">the username</param>
        private static void ArchiveOldProjectLists(ZipArchive archive, string username)
        {
            string oldProjectListsPath = Resources.ProjectDirectory + "/" + username;
            FileInfo[] oldProjectLists = FileStore.GetFiles(oldProjectListsPath);
            foreach (FileInfo oldProjectList in oldProjectLists)
            {
                string oldProjectListPath = oldProjectListsPath + "/" + oldProjectList.Name;
                archive.CreateEntryFromFile(FileStore.GetAbsolutePath(oldProjectListPath), oldProjectListPath);
            }
        }

        /// <summary>
        ///     Archives all code files of the specified user.
        /// </summary>
        /// <param name="archive">the archive to write to</param>
        /// <param name="username">the username</param>
        private static void ArchiveProjectCode(ZipArchive archive, string username)
        {
            JArray projects;
            try
            {
                projects = JArray.Parse(FileStore.ReadFile(Resources.ProjectDirectory, username + ".json"));
            }
            catch (JsonReaderException e)
            {
                Logger.Log("Could not parse list of projects of user `" + username + "`", e);
                return;
            }

            foreach (JToken project in projects)
            {
                int projectId = Convert.ToInt32(project["id"]);

                FileInfo[] projectCodeFiles = FileStore.GetFiles(Resources.CodeDirectory + "/" + projectId);
                foreach (FileInfo projectCodeFile in projectCodeFiles)
                {
                    archive.CreateEntryFromFile(projectCodeFile.FullName,
                        Resources.CodeDirectory + "/" + projectId + "/" + projectCodeFile.Name);
                }
            }
        }


        /// <summary>
        ///     Extracts the given archive.
        /// </summary>
        /// <param name="archive">the archive to extract</param>
        /// <param name="overwrite">true if existing files should be overwritten</param>
        private static void Extract(FileSystemInfo archive, bool overwrite)
        {
            string username = Path.GetFileNameWithoutExtension(archive.Name);
            bool userExists = FileStore.FileExists(Resources.UserDirectory, username + ".json");

            if (!userExists)
            {
                ZipFile.ExtractToDirectory(archive.FullName, FileStore.GetRootPath());
            }
            else if (overwrite)
            {
                using (ZipArchive zipArchive = ZipFile.OpenRead(archive.FullName))
                {
                    foreach (ZipArchiveEntry entry in zipArchive.Entries)
                    {
                        string destination = Path.Combine(archive.FullName, entry.FullName);

                        if (entry.Name == "")
                        {
                            string directoryName = Path.GetDirectoryName(destination);
                            if (directoryName == null)
                            {
                                continue;
                            }

                            Directory.CreateDirectory(directoryName);
                        }

                        entry.ExtractToFile(destination, true);
                    }
                }
            }
        }
    }
}
