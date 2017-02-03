using System;
using System.Collections.Generic;


namespace Kragle
{
    /// <summary>
    ///     A scraper that scrapes the list of most recent users.
    /// </summary>
    public class UserScraper : Scraper
    {
        private const string SubDirectory = "users";
        private const int PageSize = 20;

        private readonly FileStore _fs;
        private readonly int _targetUserCount;


        /// <summary>
        ///     Constructs a new <code>UserScraper</code>.
        /// </summary>
        /// <param name="fs">the <code>FileStore</code> to use to access the filesystem</param>
        /// <param name="targetUserCount">the target number of scraped users</param>
        /// <param name="noCache">true if requests should be made without using the cache in requests</param>
        public UserScraper(FileStore fs, int targetUserCount = int.MaxValue, bool noCache = false) : base(noCache)
        {
            _fs = fs;
            _targetUserCount = targetUserCount;
        }


        /// <summary>
        ///     Scrapes users until the target has been reached.
        /// </summary>
        public void Scrape()
        {
            int pageNumber = 0;
            int userCount = _fs.GetFiles(SubDirectory).Length;

            // Keep downloading projects until the target has been reached
            while (userCount < _targetUserCount)
            {
                ICollection<dynamic> projects = GetRecentProjects(pageNumber, PageSize);

                // Loop over projects
                foreach (dynamic project in projects)
                {
                    string fileName = project.author.id + ".json";

                    // Skip project if user is already known
                    if (_fs.FileExists(SubDirectory, fileName))
                    {
                        continue;
                    }

                    // Add user
                    _fs.WriteFile(SubDirectory, fileName, "");
                    userCount++;
                }

                pageNumber++;
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
            dynamic projectList = GetJson(string.Format(url, pageNumber, pageSize));

            // Parse to collection
            ICollection<dynamic> projects = new List<dynamic>(pageSize);
            foreach (dynamic project in projectList)
            {
                projects.Add(project);
            }

            return projects;
        }
    }
}
