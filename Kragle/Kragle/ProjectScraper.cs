using System;
using System.IO;
using Newtonsoft.Json.Linq;


namespace Kragle
{
    public class ProjectScraper
    {
        private static readonly Logger Logger = Logger.GetLogger("ProjectScraper");

        private readonly Downloader _downloader;


        /// <summary>
        ///     Constructs a new <code>ProjectScraper</code>.
        /// </summary>
        /// <param name="downloader">the <code>Downloader</code> to use for downloading data from the API</param>
        public ProjectScraper(Downloader downloader)
        {
            _downloader = downloader;
        }


        /// <summary>
        ///     Generates files for all projects of all registered users, but does not download project code yet.
        /// </summary>
        public void UpdateProjectList()
        {
            FileInfo[] users = FileStore.GetFiles("users");
            int userTotal = users.Length;
            int userCurrent = 0;

            Logger.Log(string.Format("Downloading project lists for {0} users.", userTotal));

            // Iterate over users
            foreach (FileInfo user in users)
            {
                userCurrent++;
                Logger.Log(string.Format("Downloading project list for user {0} ({1} / {2}) ({3:P2})",
                    user.Name.Length > 13 ? user.Name.Substring(0, 10) + "..." : user.Name.PadRight(13, ' '),
                    userCurrent, userTotal, userCurrent / (double) userTotal));

                // Get list of user projects
                JArray projects = GetUserProjects(user.Name);
                if (projects == null)
                {
                    continue;
                }

                // Save list of projects
                FileStore.WriteFile("projects/" + user.Name, "list", projects.ToString());

                // Create empty files for each project
                foreach (JToken project in projects)
                {
                    FileStore.WriteFile("projects/" + user.Name, project["id"].ToString(), "");
                }
            }

            Logger.Log(string.Format("Successfully downloaded project lists for {0} users.\n", userCurrent));
        }

        /// <summary>
        ///     Downloads project code for all registered projects.
        /// </summary>
        public void DownloadProjects()
        {
            DirectoryInfo[] users = FileStore.GetDirectories("projects");
            int userTotal = users.Length;
            int userCurrent = 0;

            Logger.Log(string.Format("Downloading code for {0} users.", userTotal));

            // Iterate over users
            foreach (DirectoryInfo user in users)
            {
                userCurrent++;
                Logger.Log(string.Format("Downloading code for for user {0} ({1} / {2}) ({3:P2})",
                    user.Name.Length > 13 ? user.Name.Substring(0, 10) + "..." : user.Name.PadRight(13, ' '),
                    userCurrent, userTotal, userCurrent / (double) userTotal));

                string username = user.Name;
                FileInfo[] projects = FileStore.GetFiles("projects/" + username);

                // Iterate over user projects
                foreach (FileInfo project in projects)
                {
                    if (project.Name == "list")
                    {
                        continue;
                    }

                    int projectId = Convert.ToInt32(project.Name);
                    string projectDir = "code/" + projectId;
                    string fileName = DateTime.Now.ToString("yyyy-MM-dd");

                    if (FileStore.FileExists(projectDir, fileName))
                    {
                        // Code already downloaded today
                        continue;
                    }

                    string projectCode = GetProjectCode(projectId);
                    if (projectCode == null)
                    {
                        // Code not downloaded for whatever reason
                        continue;
                    }
                    if (!Downloader.IsValidJson(projectCode))
                    {
                        // Invalid JSON, no need to save it
                        continue;
                    }
                    FileStore.WriteFile(projectDir, fileName, projectCode);
                }
            }

            Logger.Log(string.Format("Successfully downloaded code for {0} users.\n", userCurrent));
        }


        /// <summary>
        ///     Downloads the list of projects of the given user.
        /// </summary>
        /// <param name="username">the user's username</param>
        /// <returns>the list of projects of the given user</returns>
        protected JArray GetUserProjects(string username)
        {
            const string url = "https://api.scratch.mit.edu/users/{0}/projects";
            JToken projectList = _downloader.GetJson(string.Format(url, username));

            return projectList as JArray;
        }

        /// <summary>
        ///     Fetches the current state of a project's code.
        /// </summary>
        /// <param name="projectId">the project's id</param>
        /// <returns>the current state of the code of the project</returns>
        protected string GetProjectCode(int projectId)
        {
            const string url = "http://projects.scratch.mit.edu/internalapi/project/{0}/get/";
            return _downloader.GetContents(string.Format(url, projectId));
        }
    }
}
