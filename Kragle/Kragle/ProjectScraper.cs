using System;
using System.Collections.Generic;
using System.IO;


namespace Kragle
{
    public class ProjectScraper
    {
        private readonly FileStore _fs;
        private readonly Downloader _downloader;


        /// <summary>
        ///     Constructs a new <code>ProjectScraper</code>.
        /// </summary>
        /// <param name="fs">the <code>FileStore</code> to use to access the filesystem</param>
        /// <param name="noCache">true if requests should be made without using the cache in requests</param>
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
                string userName = user.Name;
                ICollection<dynamic> projects = GetUserProjects(userName);

                foreach (dynamic project in projects)
                {
                    _fs.WriteFile("projects", project.id.ToString(), "");
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
            FileInfo[] projects = _fs.GetFiles("projects");
            int projectTotal = projects.Length;
            int projectCurrent = 0;

            Console.WriteLine("Downloading code for " + projects.Length + " projects.");

            foreach (FileInfo project in projects)
            {
                int projectId = Convert.ToInt32(project.Name);

                _fs.WriteFile("code/" + DateTime.Now.ToString("yyyy-MM-dd"), projectId.ToString(), GetProjectCode(projectId));

                projectCurrent++;
                Console.WriteLine("{0:P2}", projectCurrent / (double) projectTotal);
            }
        }


        /// <summary>
        ///     Downloads the list of projects of the given user.
        /// </summary>
        /// <param name="username">the user's username</param>
        /// <returns>the list of projects of the given user</returns>
        protected ICollection<dynamic> GetUserProjects(string username)
        {
            // Fetch JSON
            const string url = "https://api.scratch.mit.edu/users/{0}/projects";
            dynamic projectList = _downloader.GetJson(string.Format(url, username));

            if (projectList == null)
            {
                return null;
            }

            // Parse to collection
            ICollection<dynamic> projects = new List<dynamic>();
            foreach (dynamic project in projectList)
            {
                projects.Add(project);
            }

            return projects;
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
