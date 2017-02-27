using System;
using System.IO;


namespace Kragle
{
    public class ProjectScraper
    {
        private readonly Downloader _downloader;
        private readonly FileStore _fs;


        /// <summary>
        ///     Constructs a new <code>ProjectScraper</code>.
        /// </summary>
        /// <param name="fs">the <code>FileStore</code> to use to access the filesystem</param>
        /// <param name="downloader">the <code>Downloader</code> to use for downloading data from the API</param>
        public ProjectScraper(FileStore fs, Downloader downloader)
        {
            _fs = fs;
            _downloader = downloader;
        }


        /// <summary>
        ///     Generates files for all projects of all registered users, but does not download project code yet.
        /// </summary>
        public void UpdateProjectList()
        {
            FileInfo[] users = _fs.GetFiles("users");
            int userTotal = users.Length;
            int userCurrent = 0;

            Console.WriteLine("Downloading project lists for " + users.Length + " users.");

            foreach (FileInfo user in users)
            {
                dynamic projects = GetUserProjects(user.Name);
                if (projects == null)
                {
                    continue;
                }

                _fs.WriteFile($"projects/{user.Name}", "list", projects.ToString());
                foreach (dynamic project in projects)
                {
                    _fs.WriteFile($"projects/{user.Name}", project.id.ToString(), "");
                }

                userCurrent++;
                Console.WriteLine("{0:P2}", userCurrent / (double) userTotal);
            }
        }

        /// <summary>
        ///     Downloads project code for all registered projects.
        /// </summary>
        public void DownloadProjects()
        {
            DirectoryInfo[] users = _fs.GetDirectories("projects");
            int userTotal = users.Length;
            int userCurrent = 0;

            Console.WriteLine("Downloading code for " + users.Length + " users.");

            foreach (DirectoryInfo user in users)
            {
                string username = user.Name;
                FileInfo[] projects = _fs.GetFiles($"projects/{username}");

                foreach (FileInfo project in projects)
                {
                    if (project.Name == "list")
                    {
                        continue;
                    }

                    int projectId = Convert.ToInt32(project.Name);
                    string projectDir = $"code/{projectId}";
                    string fileName = DateTime.Now.ToString("yyyy-MM-dd");

                    if (!_fs.FileExists(projectDir, fileName))
                    {
                        _fs.WriteFile(projectDir, fileName, GetProjectCode(projectId));
                    }
                }

                userCurrent++;
                Console.WriteLine("{0:P2}", userCurrent / (double) userTotal);
            }
        }


        /// <summary>
        ///     Downloads the list of projects of the given user.
        /// </summary>
        /// <param name="username">the user's username</param>
        /// <returns>the list of projects of the given user</returns>
        protected dynamic GetUserProjects(string username)
        {
            const string url = "https://api.scratch.mit.edu/users/{0}/projects";
            dynamic projectList = _downloader.GetJson(string.Format(url, username));

            return projectList;
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
