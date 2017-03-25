using System;
using System.IO;
using Newtonsoft.Json.Linq;


namespace Kragle
{
    public class ProjectScraper
    {
        private readonly Downloader _downloader;
        private readonly FileStore _fs;
        private readonly Logger _logger;


        /// <summary>
        ///     Constructs a new <code>ProjectScraper</code>.
        /// </summary>
        /// <param name="fs">the <code>FileStore</code> to use to access the filesystem</param>
        /// <param name="downloader">the <code>Downloader</code> to use for downloading data from the API</param>
        public ProjectScraper(FileStore fs, Downloader downloader)
        {
            _fs = fs;
            _downloader = downloader;
            _logger = Logger.GetLogger("ProjectScraper");
        }


        /// <summary>
        ///     Generates files for all projects of all registered users, but does not download project code yet.
        /// </summary>
        public void UpdateProjectList()
        {
            FileInfo[] users = _fs.GetFiles("users");
            int userTotal = users.Length;
            int userCurrent = 0;

            _logger.Log(string.Format("Downloading project lists for {0} users.", userTotal));

            // Iterate over users
            foreach (FileInfo user in users)
            {
                userCurrent++;
                _logger.Log(string.Format("Downloading project list for user {0} ({1} / {2}) ({3:P2})",
                    user.Name.Length > 13 ? user.Name.Substring(0, 10) + "..." : user.Name.PadRight(13, ' '),
                    userCurrent, userTotal, userCurrent / (double) userTotal));

                // Get list of user projects
                JArray projects = GetUserProjects(user.Name);
                if (projects == null)
                {
                    continue;
                }

                // Save list of projects
                _fs.WriteFile("projects/" + user.Name, "list", projects.ToString());

                // Create empty files for each project
                foreach (JToken project in projects)
                {
                    _fs.WriteFile("projects/" + user.Name, project["id"].ToString(), "");
                }
            }

            _logger.Log(string.Format("Successfully downloaded project lists for {0} users.\n", userCurrent));
        }

        /// <summary>
        ///     Downloads project code for all registered projects.
        /// </summary>
        public void DownloadProjects()
        {
            DirectoryInfo[] users = _fs.GetDirectories("projects");
            int userTotal = users.Length;
            int userCurrent = 0;

            _logger.Log(string.Format("Downloading code for {0} users.", userTotal));

            // Iterate over users
            foreach (DirectoryInfo user in users)
            {
                userCurrent++;
                _logger.Log(string.Format("Downloading code for for user {0} ({1} / {2}) ({3:P2})",
                    user.Name.Length > 13 ? user.Name.Substring(0, 10) + "..." : user.Name.PadRight(13, ' '),
                    userCurrent, userTotal, userCurrent / (double) userTotal));

                string username = user.Name;
                FileInfo[] projects = _fs.GetFiles("projects/" + username);

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

                    if (_fs.FileExists(projectDir, fileName))
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
                    _fs.WriteFile(projectDir, fileName, projectCode);
                }
            }

            _logger.Log(string.Format("Successfully downloaded code for {0} users.\n", userCurrent));
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
