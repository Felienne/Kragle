using System;
using System.Collections.Generic;
using System.IO;


namespace Kragle
{
    public class ProjectScraper : Scraper
    {
        private readonly FileStore _fs;


        /// <summary>
        ///     Constructs a new <code>ProjectScraper</code>.
        /// </summary>
        /// <param name="fs">the <code>FileStore</code> to use to access the filesystem</param>
        /// <param name="noCache">true if requests should be made without using the cache in requests</param>
        public ProjectScraper(FileStore fs, bool noCache) : base(noCache)
        {
            _fs = fs;
        }


        /// <summary>
        ///     Generates files for all projects of all registered users, but does not download project code yet.
        /// </summary>
        public void DownloadProjects()
        {
            FileInfo[] users = _fs.GetFiles("users");

            Console.WriteLine("Downloading project lists for " + users.Length + " users\n\n");

            foreach (FileInfo user in users)
            {
                string userName = user.Name;
                ICollection<dynamic> projects = GetUserProjects(userName);

                foreach (dynamic project in projects)
                {
                    _fs.WriteFile("projects", project.id.ToString(), "");
                }
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
            dynamic projectList = GetJson(string.Format(url, username));

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
    }
}
