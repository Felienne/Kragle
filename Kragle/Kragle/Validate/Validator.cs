using System;
using System.IO;
using Kragle.Properties;
using log4net;
using Newtonsoft.Json.Linq;
using ShellProgressBar;


namespace Kragle.Validate
{
    /// <summary>
    ///     Validates downloaded JSON files.
    /// </summary>
    public class Validator
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Validator));

        public int InvalidUsers { get; private set; }
        public int InvalidProjectLists { get; private set; }
        public int InvalidCodeFiles { get; private set; }


        /// <summary>
        ///     Constructs a new <code>Validator</code>.
        /// </summary>
        public Validator()
        {
            InvalidCodeFiles = 0;
            InvalidProjectLists = 0;
            InvalidUsers = 0;
        }


        /// <summary>
        ///     Validates metadata for all users.
        /// </summary>
        public void ValidateUsers()
        {
            FileInfo[] users = FileStore.GetFiles(Resources.UserDirectory);
            int userTotal = users.Length;
            int userCurrent = 0;

            Logger.DebugFormat("Validating {0} users.", userTotal);

            using (ProgressBar progressBar = new ProgressBar(userTotal, "Initializing"))
            {
                foreach (FileInfo user in users)
                {
                    userCurrent++;
                    string username = user.Name.Remove(user.Name.Length - 5);

                    string logMessage = "Validating user " + LoggerHelper.ForceLength(username, 10);
                    progressBar.Tick(logMessage);
                    Logger.Debug(LoggerHelper.FormatProgress(logMessage, userCurrent, userTotal));

                    if (!IsValidJson(File.ReadAllText(user.FullName)))
                    {
                        InvalidUsers++;
                        Logger.WarnFormat("Metadata of user {0} is invalid", username);
                    }
                }

                progressBar.UpdateMessage("Finished validating user metadata");
            }

            Logger.DebugFormat("Successfully validated {0} users.", userTotal);
        }

        /// <summary>
        ///     Validates all project lists of each user.
        /// </summary>
        public void ValidateProjectLists()
        {
            DirectoryInfo[] users = FileStore.GetDirectories(Resources.ProjectDirectory);
            int userTotal = users.Length;
            int userCurrent = 0;

            Logger.DebugFormat("Validating project lists of {0} users.", userTotal);

            using (ProgressBar progressBar = new ProgressBar(userTotal, "Initializing"))
            {
                foreach (DirectoryInfo user in users)
                {
                    userCurrent++;
                    string username = user.Name;

                    string logMessage = "Validating project lists of user " + LoggerHelper.ForceLength(username, 10);
                    progressBar.Tick(logMessage);
                    Logger.Debug(LoggerHelper.FormatProgress(logMessage, userCurrent, userTotal));

                    FileInfo[] projectLists = user.GetFiles();
                    foreach (FileInfo projectList in projectLists)
                    {
                        if (!IsValidJson(File.ReadAllText(projectList.FullName)))
                        {
                            InvalidProjectLists++;
                            Logger.WarnFormat("Project list of user {0} at date {1} is invalid",
                                username, projectList.Name);
                        }
                    }
                }
                
                progressBar.UpdateMessage("Finished validating project lists");
            }

            Logger.DebugFormat("Successfully validated project lists of {0} users.", userTotal);
        }

        /// <summary>
        ///     Validates all code files of each project.
        /// </summary>
        public void ValidateCode()
        {
            DirectoryInfo[] projects = FileStore.GetDirectories(Resources.CodeDirectory);
            int projectTotal = projects.Length;
            int projectCurrent = 0;

            Logger.DebugFormat("Validating code of {0} projects.", projectTotal);

            using (ProgressBar progressBar = new ProgressBar(projectTotal, "Initializing"))
            {
                foreach (DirectoryInfo project in projects)
                {
                    projectCurrent++;
                    string projectId = project.Name;

                    string logMessage = "Validating code of project with id " + projectId;
                    progressBar.Tick(logMessage);
                    Logger.Debug(LoggerHelper.FormatProgress(logMessage, projectCurrent, projectTotal));

                    FileInfo[] codeFiles = project.GetFiles();
                    foreach (FileInfo codeFile in codeFiles)
                    {
                        if (!IsValidJson(File.ReadAllText(codeFile.FullName)))
                        {
                            InvalidCodeFiles++;
                            Logger.WarnFormat("Code of project {0} at date {1} is invalid", projectId, codeFile.Name);
                        }
                    }
                }
                
                progressBar.UpdateMessage("Finished validating code");
            }

            Logger.DebugFormat("Successfully validated code of {0} projects.", projectTotal);
        }


        /// <summary>
        ///     Validates JSON.
        /// </summary>
        /// <param name="json">a JSON string</param>
        /// <returns>true if the given JSON is valid</returns>
        private static bool IsValidJson(string json)
        {
            try
            {
                JToken.Parse(json);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
