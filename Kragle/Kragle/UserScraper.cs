using System;
using System.Collections.Generic;


namespace Kragle
{
    /// <summary>
    ///     A scraper that scrapes the list of most recent users.
    /// </summary>
    public class UserScraper : Scraper
    {
        public UserScraper(bool noCache) : base(noCache)
        {
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
