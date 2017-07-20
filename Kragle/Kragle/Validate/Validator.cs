using System;
using System.IO;
using Kragle.Properties;
using Newtonsoft.Json.Linq;


namespace Kragle.Validate
{
    /// <summary>
    ///     Validates downloaded JSON files.
    /// </summary>
    public class Validator
    {
        private static readonly Logger Logger = Logger.GetLogger("Validator");

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

            Logger.Log(string.Format("Validating {0} users.", userTotal));

            foreach (FileInfo user in users)
            {
                string username = user.Name.Remove(user.Name.Length - 5);

                userCurrent++;
                Logger.Log(LoggerHelper.FormatProgress(
                    "Validating user " + LoggerHelper.ForceLength(username, 10), userCurrent, userTotal));

                if (!IsValidJson(File.ReadAllText(user.FullName)))
                {
                    InvalidUsers++;
                    Logger.Log("Metadata of user " + username + " is invalid");
                }
            }
        }

        /// <summary>
        ///     Validates all project lists of each user.
        /// </summary>
        public void ValidateProjectLists()
        {
            DirectoryInfo[] users = FileStore.GetDirectories(Resources.ProjectDirectory);
            int userTotal = users.Length;
            int userCurrent = 0;

            Logger.Log(string.Format("Validating project lists of {0} users.", userTotal));

            foreach (DirectoryInfo user in users)
            {
                string username = user.Name;

                userCurrent++;
                Logger.Log(LoggerHelper.FormatProgress(
                    "Validating project lists of user " + LoggerHelper.ForceLength(username, 10),
                    userCurrent, userTotal));

                FileInfo[] projectLists = user.GetFiles();
                foreach (FileInfo projectList in projectLists)
                {
                    if (!IsValidJson(File.ReadAllText(projectList.FullName)))
                    {
                        InvalidProjectLists++;
                        Logger.Log("Project list of user " + username + " at date " + projectList.Name + " is invalid");
                    }
                }
            }
        }

        /// <summary>
        ///     Validates all code files of each project.
        /// </summary>
        public void ValidateCode()
        {
            DirectoryInfo[] projects = FileStore.GetDirectories(Resources.CodeDirectory);
            int projectTotal = projects.Length;
            int projectCurrent = 0;

            Logger.Log(string.Format("Validating code of {0} projects.", projectTotal));

            foreach (DirectoryInfo project in projects)
            {
                string projectId = project.Name;

                projectCurrent++;
                Logger.Log(LoggerHelper.FormatProgress(
                    "Validating code of project with id " + projectId, projectCurrent, projectTotal));

                FileInfo[] codeFiles = project.GetFiles();
                foreach (FileInfo codeFile in codeFiles)
                {
                    if (!IsValidJson(File.ReadAllText(codeFile.FullName)))
                    {
                        InvalidCodeFiles++;
                        Logger.Log("Code of project " + projectId + " at date " + codeFile.Name + " is invalid");
                    }
                }
            }
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
