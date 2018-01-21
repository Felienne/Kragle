using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kragle.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Kragle.Preparse
{
    public class Preparser
    {
        private static readonly Logger Logger = Logger.GetLogger("Preparser");


        public void PreparseCodeDuplicates()
        {
            DirectoryInfo[] userDirs = FileStore.GetDirectories(Resources.ProjectDirectory);
            int userTotal = userDirs.Length;
            int userCurrent = 0;

            Logger.Log("Removing code duplicates of " + userDirs.Length + " users.");

            foreach (DirectoryInfo userDir in userDirs)
            {
                string username = userDir.Name;

                userCurrent++;
                Logger.Log(LoggerHelper.FormatProgress(
                    "Removing code duplicates of " + LoggerHelper.ForceLength(username, 10),
                    userCurrent, userTotal));

                Dictionary<int, DateTime> projectDates = new Dictionary<int, DateTime>();
                FileInfo[] projectLists = userDir.GetFiles().OrderBy(projectList => projectList.Name).ToArray();

                foreach (FileInfo projectList in projectLists)
                {
                    JArray projects;
                    try
                    {
                        projects = JArray.Parse(File.ReadAllText(projectList.FullName));
                    }
                    catch (JsonReaderException e)
                    {
                        Logger.Log("The project metadata list of user `" + userDir.Name + "` could not be parsed.", e);
                        return;
                    }

                    foreach (JToken project in projects)
                    {
                        if (!(project is JObject))
                        {
                            Logger.Log("The metadata of a project of user `" + userDir.Name + "` could not be parsed.");
                            return;
                        }

                        JObject metadata = (JObject) project;
                        int projectId = int.Parse(metadata["id"].ToString());
                        DateTime modifyDate = DateTime.Parse(metadata["history"]["modified"].ToString());

                        if (projectDates.ContainsKey(projectId) && projectDates[projectId].Equals(modifyDate))
                        {
                            Logger.Log("Deleted duplicate code; " + projectId + "/" + projectList.Name);

                            string codePath = FileStore.GetAbsolutePath(Resources.CodeDirectory,
                                projectId + "/" + projectList.Name);

                            if (File.Exists(codePath))
                            {
                                File.Delete(codePath);
                            }
                        }

                        projectDates[projectId] = modifyDate;
                    }
                }
            }
        }

        public void RemoveUnchangedProjects()
        {
            RemoveUnchangedProjectCode();
            RemoveUnchangedProjectsFromLists();
            RemoveUsersWithoutProjects();
        }


        private static void RemoveUnchangedProjectCode()
        {
            DirectoryInfo[] userDirs = FileStore.GetDirectories(Resources.CodeDirectory);
            int userTotal = userDirs.Length;
            int userCurrent = 0;

            Logger.Log("Removing unchanged projects of " + userDirs.Length + " users.");

            foreach (DirectoryInfo userDir in userDirs)
            {
                string username = userDir.Name;

                userCurrent++;
                Logger.Log(LoggerHelper.FormatProgress(
                    "Removing unchanged projects of user " + LoggerHelper.ForceLength(username, 10),
                    userCurrent, userTotal));

                FileInfo[] projectLists = userDir.GetFiles().OrderBy(projectList => projectList.Name).ToArray();

                if (projectLists.Length <= 1)
                {
                    userDir.Delete(true);
                }
            }
        }

        private static void RemoveUnchangedProjectsFromLists()
        {
            DirectoryInfo[] userDirs = FileStore.GetDirectories(Resources.ProjectDirectory);
            int userTotal = userDirs.Length;
            int userCurrent = 0;

            Logger.Log("Updating project lists of " + userDirs.Length + " users.");

            foreach (DirectoryInfo userDir in userDirs)
            {
                string username = userDir.Name;

                userCurrent++;
                Logger.Log(LoggerHelper.FormatProgress(
                    "Updating project lists of " + LoggerHelper.ForceLength(username, 10),
                    userCurrent, userTotal));

                FileInfo[] projectLists = userDir.GetFiles().OrderBy(projectList => projectList.Name).ToArray();

                foreach (FileInfo projectList in projectLists)
                {
                    JArray projects;
                    try
                    {
                        projects = JArray.Parse(File.ReadAllText(projectList.FullName));
                    }
                    catch (JsonReaderException e)
                    {
                        Logger.Log("The project metadata list of user `" + userDir.Name + "` could not be parsed.", e);
                        return;
                    }

                    JArray filteredProjects = new JArray();
                    foreach (JToken project in projects)
                    {
                        if (!(project is JObject))
                        {
                            Logger.Log("The metadata of a project of user `" + userDir.Name + "` could not be parsed.");
                            return;
                        }

                        JObject metadata = (JObject) project;
                        int projectId = int.Parse(metadata["id"].ToString());

                        if (FileStore.DirectoryExists(Resources.CodeDirectory + "/" + projectId))
                        {
                            filteredProjects.Add(project);
                        }
                    }


                    if (filteredProjects.Count == 0)
                    {
                        File.Delete(projectList.FullName);
                    }
                    else
                    {
                        File.WriteAllText(projectList.FullName, filteredProjects.ToString(Formatting.None));
                    }
                }
            }
        }

        private static void RemoveUsersWithoutProjects()
        {
            FileInfo[] userDirs = FileStore.GetFiles(Resources.UserDirectory);
            int userTotal = userDirs.Length;
            int userCurrent = 0;

            Logger.Log("Checking  " + userDirs.Length + " users if they have projects.");

            foreach (FileInfo userDir in userDirs)
            {
                string username = Path.GetFileNameWithoutExtension(userDir.Name);

                userCurrent++;
                Logger.Log(LoggerHelper.FormatProgress(
                    "Checking if user " + LoggerHelper.ForceLength(username, 10) + " has projects",
                    userCurrent, userTotal));

                string userProjects = Resources.ProjectDirectory + "/" + username;
                if (!FileStore.DirectoryExists(userProjects) || FileStore.GetFiles(userProjects).Length == 0)
                {
                    FileStore.RemoveFile(Resources.UserDirectory, username + ".json");
                    FileStore.RemoveFile(Resources.ProjectDirectory, username + ".json");
                    FileStore.RemoveDirectory(Resources.ProjectDirectory + "/" + username);
                }
            }
        }
    }
}
