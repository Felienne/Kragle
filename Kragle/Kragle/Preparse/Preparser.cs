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


        public void PreparseCode()
        {
            DirectoryInfo[] userDirs = FileStore.GetDirectories(Resources.ProjectDirectory);
            int userTotal = userDirs.Length;
            int userCurrent = 0;

            Logger.Log("Preparsing projects of " + userDirs.Length + " users.");

            foreach (DirectoryInfo userDir in userDirs)
            {
                string username = userDir.Name;

                userCurrent++;
                Logger.Log(LoggerHelper.FormatProgress(
                    "Preparsing projects of user " + LoggerHelper.ForceLength(username, 10),
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
    }
}
