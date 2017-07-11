using System;
using System.IO;
using System.IO.Compression;
using Kragle.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Kragle
{
    public class Archiver
    {
        private static readonly Logger Logger = Logger.GetLogger("Archiver");


        public void Archive()
        {
            FileInfo[] users = FileStore.GetFiles(Resources.UserDirectory);

            foreach (FileInfo user in users)
            {
                Archive(user);
            }
        }

        public void Extract()
        {
            FileInfo[] archives = FileStore.GetFiles(Resources.ArchiveDirectory);

            foreach (FileInfo archive in archives)
            {
                ZipFile.ExtractToDirectory(archive.FullName, FileStore.GetRootPath());
            }
        }


        private void Archive(FileInfo user)
        {
            string username = user.Name.Remove(user.Name.Length - 5);

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

            FileStore.CreateDirectory(Resources.ArchiveDirectory);

            string archivePath = FileStore.GetAbsolutePath(Resources.ArchiveDirectory + "/" + username + ".zip");
            using (var fileStream = new FileStream(archivePath, FileMode.CreateNew))
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
            {
                string userData = Resources.UserDirectory + "/" + username + ".json";
                archive.CreateEntryFromFile(FileStore.GetAbsolutePath(userData), userData);

                string projectList = Resources.ProjectDirectory + "/" + username + ".json";
                archive.CreateEntryFromFile(FileStore.GetAbsolutePath(projectList), projectList);

                FileInfo[] oldProjectLists = FileStore.GetFiles(Resources.ProjectDirectory + "/" + username);
                foreach (FileInfo oldProjectList in oldProjectLists)
                {
                    archive.CreateEntryFromFile(oldProjectList.FullName,
                        Resources.ProjectDirectory + "/" + username + "/" + oldProjectList.Name);
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
        }
    }
}
