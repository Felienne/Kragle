using System;
using System.IO;
using Newtonsoft.Json.Linq;


namespace Kragle
{
    /// <summary>
    ///     A scraper that scrapes the list of most recent users.
    /// </summary>
    public class UserScraper
    {
        private const string SubDirectory = "users";
        private const int PageSize = 20;

        private static readonly Logger Logger = Logger.GetLogger("UserScraper");

        private readonly Downloader _downloader;
        private readonly int _targetUserCount;


        /// <summary>
        ///     Constructs a new <code>UserScraper</code>.
        /// </summary>
        /// <param name="downloader">the <code>Downloader</code> to download user data with</param>
        /// <param name="targetUserCount">the target number of scraped users</param>
        public UserScraper(Downloader downloader, int targetUserCount)
        {
            _downloader = downloader;
            _targetUserCount = targetUserCount;
        }


        /// <summary>
        ///     Scrapes users until the target has been reached.
        /// </summary>
        public void ScrapeUsers()
        {
            int pageNumber = 0;
            int userCount = FileStore.GetFiles(SubDirectory).Length;

            Logger.Log("Scraping list of recent projects.");

            // Keep downloading projects until the target has been reached
            while (userCount < _targetUserCount)
            {
                Logger.Log(string.Format("Downloading page {0}. ({1} / {2} users registered)",
                    pageNumber, userCount, _targetUserCount));

                // Loop over projects
                JArray projects = GetRecentProjects(pageNumber, PageSize);
                foreach (JToken project in projects)
                {
                    string fileName = project["author"]["username"].ToString();

                    // Skip project if user is already known
                    if (FileStore.FileExists(SubDirectory, fileName))
                    {
                        continue;
                    }

                    // Add user
                    FileStore.WriteFile(SubDirectory, fileName, "");
                    userCount++;
                    if (userCount >= _targetUserCount)
                    {
                        break;
                    }
                }

                pageNumber++;
            }

            Logger.Log(string.Format("Successfully registered {0} users.\n", userCount));
        }

        /// <summary>
        ///     Downloads meta-data for all users. If the <code>noCache</code> attribute is set, existing meta-data is
        ///     updated.
        /// </summary>
        public void DownloadMetaData()
        {
            FileInfo[] users = FileStore.GetFiles(SubDirectory);
            int userTotal = users.Length;
            int userCurrent = 0;

            Logger.Log(string.Format("Downloading meta-data for {0} users.", userTotal));

            foreach (FileInfo user in users)
            {
                userCurrent++;
                Logger.Log(string.Format("Downloading meta-data for user {0} ({1} / {2}) ({3:P2})",
                    user.Name.Length > 10 ? user.Name.Substring(0, 10) + "..." : user.Name.PadRight(13, ' '),
                    userCurrent, userTotal, userCurrent / (double) userTotal));

                if (user.Length > 0)
                {
                    // Meta-data already downloaded
                    continue;
                }

                string metaData = GetMetaData(user.Name);
                FileStore.WriteFile(SubDirectory, user.Name, metaData);
            }

            Logger.Log(string.Format("Successfully downloaded meta-data for {0} users.\n", userCurrent));
        }


        /// <summary>
        ///     Returns the <code>ICollection</code> of most recently published projects. The results are returned as pages.
        /// </summary>
        /// <param name="pageNumber">the number of the page to return; must be at least 0</param>
        /// <param name="pageSize">the number of projects per page; must be between 1 and 20 (inclusive)</param>
        /// <returns>the <code>ICollection</code> of most recently published projects</returns>
        protected JArray GetRecentProjects(int pageNumber, int pageSize)
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
            JToken projects = _downloader.GetJson(string.Format(url, pageNumber, pageSize));

            return projects as JArray;
        }

        /// <summary>
        ///     Downloads meta-data on a user.
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
