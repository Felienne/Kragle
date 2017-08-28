using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kragle.Properties;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShellProgressBar;


namespace Kragle.Preparse
{
    public class Preparser
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Preparser));


        public void PreparseCodeDuplicates()
        {
            DirectoryInfo[] userDirs = FileStore.GetDirectories(Resources.ProjectDirectory);
            int userTotal = userDirs.Length;
            int userCurrent = 0;

            Logger.DebugFormat("Preparsing code of {0} users.", userDirs.Length);

            using (ProgressBar progressBar = new ProgressBar(userTotal, "Initializing"))
            {
                foreach (DirectoryInfo userDir in userDirs)
                {
                    userCurrent++;
                    string username = userDir.Name;

                    string logMessage = "Preparsing code of user " + LoggerHelper.ForceLength(username, 10);
                    progressBar.Tick(logMessage);
                    Logger.Debug(LoggerHelper.FormatProgress(logMessage, userCurrent, userTotal));

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
                            Logger.Fatal(
                                "The project metadata list of user `" + userDir.Name + "` could not be parsed.", e);
                            return;
                        }

                        foreach (JToken project in projects)
                        {
                            if (!(project is JObject))
                            {
                                Logger.Fatal("The metadata of a project of user `" + userDir.Name +
                                             "` could not be parsed.");
                                return;
                            }

                            JObject metadata = (JObject) project;
                            int projectId = int.Parse(metadata["id"].ToString());
                            DateTime modifyDate = DateTime.Parse(metadata["history"]["modified"].ToString());

                            if (projectDates.ContainsKey(projectId) && projectDates[projectId].Equals(modifyDate))
                            {
                                Logger.DebugFormat("Deleted duplicate code; {0}/{1}", projectId, projectList.Name);

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

                progressBar.UpdateMessage("Finished preparsing code");
            }

            Logger.DebugFormat("Successfully preparsed code of {0} users.", userDirs.Length);
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

            Logger.DebugFormat("Preparsing projects of {0} users.", userDirs.Length);

            using (ProgressBar progressBar = new ProgressBar(userTotal, "Initializing"))
            {
                foreach (DirectoryInfo userDir in userDirs)
                {
                    string username = userDir.Name;

                    userCurrent++;
                    string logMessage = "Preparsing code of user " + LoggerHelper.ForceLength(username, 10);
                    progressBar.Tick(logMessage);
                    Logger.Debug(LoggerHelper.FormatProgress(logMessage, userCurrent, userTotal));

                    FileInfo[] projectLists = userDir.GetFiles().OrderBy(projectList => projectList.Name).ToArray();

                    if (projectLists.Length <= 1)
                    {
                        userDir.Delete(true);
                    }
                }

                progressBar.UpdateMessage("Finished preparsing user projects");
            }

            Logger.DebugFormat("Successfully preparsed projects of {0} users.", userDirs.Length);
        }

        private static void RemoveUnchangedProjectsFromLists()
        {
            DirectoryInfo[] userDirs = FileStore.GetDirectories(Resources.ProjectDirectory);
            int userTotal = userDirs.Length;
            int userCurrent = 0;

            Logger.DebugFormat("Preparsing code of {0} users.", userDirs.Length);

            using (ProgressBar progressBar = new ProgressBar(userTotal, "Initializing"))
            {
                foreach (DirectoryInfo userDir in userDirs)
                {
                    userCurrent++;
                    string username = userDir.Name;

                    string logMessage = "Preparsing code of user " + LoggerHelper.ForceLength(username, 10);
                    progressBar.Tick(logMessage);
                    Logger.Debug(LoggerHelper.FormatProgress(logMessage, userCurrent, userTotal));

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
                            Logger.Fatal(
                                "The project metadata list of user `" + userDir.Name + "` could not be parsed.", e);
                            return;
                        }

                        JArray filteredProjects = new JArray();
                        foreach (JToken project in projects)
                        {
                            if (!(project is JObject))
                            {
                                Logger.Fatal("The metadata of a project of user `" + userDir.Name +
                                             "` could not be parsed.");
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
                
                progressBar.UpdateMessage("Finished preparsing code");
            }

            Logger.DebugFormat("Successfully preparsed code of {0} users.", userDirs.Length);
        }


        private static void RemoveUsersWithoutProjects()
        {
            FileInfo[] userDirs = FileStore.GetFiles(Resources.UserDirectory);
            int userTotal = userDirs.Length;
            int userCurrent = 0;

            Logger.DebugFormat("Preparsing {0} users.", userDirs.Length);

            using (ProgressBar progressBar = new ProgressBar(userTotal, "Initializing"))
            {
                foreach (FileInfo userDir in userDirs)
                {
                    userCurrent++;
                    string username = Path.GetFileNameWithoutExtension(userDir.Name);

                    string logMessage = "Preparsing code of user " + LoggerHelper.ForceLength(username, 10);
                    progressBar.Tick(logMessage);
                    Logger.Debug(LoggerHelper.FormatProgress(logMessage, userCurrent, userTotal));

                    string userProjects = Resources.ProjectDirectory + "/" + username;
                    if (!FileStore.DirectoryExists(userProjects) || FileStore.GetFiles(userProjects).Length == 0)
                    {
                        FileStore.RemoveFile(Resources.UserDirectory, username + ".json");
                        FileStore.RemoveFile(Resources.ProjectDirectory, username + ".json");
                        FileStore.RemoveDirectory(Resources.ProjectDirectory + "/" + username);
                    }
                }
                
                progressBar.UpdateMessage("Finished preparsing users");
            }

            Logger.DebugFormat("Successfully preparsed {0} users.", userDirs.Length);
        }
    }
}
