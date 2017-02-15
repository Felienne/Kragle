using System;
using System.Collections.Generic;
using System.IO;


namespace Kragle
{
    /// <summary>
    ///     A scraper that scrapes the list of most recent users.
    /// </summary>
    public class UserScraper
    {
        private const string SubDirectory = "users";
        private const int PageSize = 20;

        private readonly FileStore _fs;
        private readonly Downloader _downloader;
        private readonly int _targetUserCount;


        /// <summary>
        ///     Constructs a new <code>UserScraper</code>.
        /// </summary>
        /// <param name="fs">the <code>FileStore</code> to use to access the filesystem</param>
        /// <param name="downloader">the <code>Downloader</code> to download user data with</param>
        /// <param name="targetUserCount">the target number of scraped users</param>
        public UserScraper(FileStore fs, Downloader downloader, int targetUserCount = int.MaxValue)
        {
            _fs = fs;
            _downloader = downloader;
            _targetUserCount = targetUserCount;
        }


        /// <summary>
        ///     Scrapes users until the target has been reached.
        /// </summary>
        public void ScrapeUsers()
        {
            int pageNumber = 0;
            int userCount = _fs.GetFiles(SubDirectory).Length;

            Console.WriteLine("Starting user scraping...\n" +
                              userCount + " users already registered.\n\n");

            // Keep downloading projects until the target has been reached
            while (userCount < _targetUserCount)
            {
                Console.WriteLine("Downloading page " + pageNumber);
                ICollection<dynamic> projects = GetRecentProjects(pageNumber, PageSize);

                // Loop over projects
                foreach (dynamic project in projects)
                {
                    string fileName = project.author.username;

                    // Skip project if user is already known
                    if (_fs.FileExists(SubDirectory, fileName))
                    {
                        continue;
                    }

                    // Add user
                    _fs.WriteFile(SubDirectory, fileName, "");
                    userCount++;
                }

                Console.WriteLine(userCount + " / " + _targetUserCount + " users\n");
                pageNumber++;
            }
        }

        /// <summary>
        ///     Downloads meta-data for all users. If the <code>noCache</code> attribute is set, existing meta-data is updated.
        /// </summary>
        public void DownloadMetaData()
        {
            FileInfo[] users = _fs.GetFiles(SubDirectory);

            Console.WriteLine("Downloading meta-data for " + users.Length + " users.\n");

            foreach (FileInfo user in users)
            {
                if (user.Length > 0)
                {
                    continue;
                }

                string metaData = GetMetaData(user.Name);
                _fs.WriteFile(SubDirectory, user.Name, metaData);
            }
        }


        /// <summary>
        ///     Returns the <code>ICollection</code> of most recently published projects. The results are returned as pages.
        /// </summary>
        /// <param name="pageNumber">the number of the page to return; must be at least 0</param>
        /// <param name="pageSize">the number of projects per page; must be between 1 and 20 (inclusive)</param>
        /// <returns>the <code>ICollection</code> of most recently published projects</returns>
        protected ICollection<dynamic> GetRecentProjects(int pageNumber, int pageSize)
        {
            if (pageNumber < 0)
            {
                throw new ArgumentException("The page number must be at least 0.");
            }
            if (pageSize < 1 || pageSize > 20)
            {
                throw new ArgumentException("The page size must be between 1 and 20 (inclusive).");
            }

            // Fetch JSON
            const string url = "https://api.scratch.mit.edu/search/projects?mode=recent&offset={0}&limit={1}";
            dynamic projectList = _downloader.GetJson(string.Format(url, pageNumber, pageSize));

            // Parse to collection
            ICollection<dynamic> projects = new List<dynamic>(pageSize);
            foreach (dynamic project in projectList)
            {
                projects.Add(project);
            }

            return projects;
        }

        /// <summary>
        /// Downloads meta-data on a user.
        /// </summary>
        /// <param name="username">the user's username</param>
        /// <returns>the meta-data on the user</returns>
        protected string GetMetaData(string username)
        {
            const string url = "https://api.scratch.mit.edu/users/{0}";
            return _downloader.GetContents(string.Format(url, username));
        }
    }
}
