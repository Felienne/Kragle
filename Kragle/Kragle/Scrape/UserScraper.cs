﻿using System;
using System.IO;
using Kragle.Properties;
using Newtonsoft.Json.Linq;


namespace Kragle.Scrape
{
    /// <summary>
    ///     A scraper that scrapes the list of most recent users.
    /// </summary>
    public class UserScraper
    {
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
        /// <param name="pageNumber">the number of the page to start scraping at</param>
        public void ScrapeUsers(int pageNumber)
        {
            if (pageNumber < 0)
            {
                throw new ArgumentOutOfRangeException("Page number must be at least 0.");
            }

            int userCount = FileStore.GetFiles(Resources.UserDirectory).Length;

            Logger.Log("Scraping list of recent projects.");

            // Keep downloading projects until the target has been reached
            while (userCount < _targetUserCount && pageNumber * PageSize < 10000)
            {
                Logger.Log(string.Format("Downloading page {0}. ({1} / {2} users registered)",
                    pageNumber, userCount, _targetUserCount));

                // Loop over projects
                JArray projects = GetRecentProjects(pageNumber, PageSize);
                foreach (JToken project in projects)
                {
                    string fileName = project["author"]["username"].ToString();

                    // Skip project if user is already known
                    if (FileStore.FileExists(Resources.UserDirectory, fileName + ".json"))
                    {
                        continue;
                    }

                    // Add user
                    FileStore.WriteFile(Resources.UserDirectory, fileName + ".json", "");

                    userCount++;
                    if (userCount >= _targetUserCount)
                    {
                        break;
                    }
                }

                pageNumber++;
            }

            if (userCount < _targetUserCount)
            {
                Logger.Log("Kragle was unable to scrape more users because the Scratch API records only the 10,000"
                           + " latest activities.");
            }

            Logger.Log(string.Format("Successfully registered {0} users.\n", userCount));
        }

        /// <summary>
        ///     Downloads metadata for all users. If the <code>cache</code> attribute is not set, existing metadata is
        ///     updated.
        /// </summary>
        public void DownloadMetadata()
        {
            FileInfo[] users = FileStore.GetFiles(Resources.UserDirectory);
            int userTotal = users.Length;
            int userCurrent = 0;

            Logger.Log(string.Format("Downloading metadata for {0} users.", userTotal));

            foreach (FileInfo user in users)
            {
                string username = user.Name.Remove(user.Name.Length - 5);

                userCurrent++;
                Logger.Log(LoggerHelper.FormatProgress(
                    "Downloading metadata for user " + LoggerHelper.ForceLength(username, 10), userCurrent,
                    userTotal));

                if (user.Length > 0)
                {
                    // Metadata already downloaded
                    continue;
                }

                string metadata = GetMetadata(username);
                FileStore.WriteFile(Resources.UserDirectory, username + ".json", metadata);
            }

            Logger.Log(string.Format("Successfully downloaded metadata for {0} users.\n", userCurrent));
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
            JToken projects = _downloader.GetJson(string.Format(url, pageNumber * pageSize, pageSize));

            return projects as JArray;
        }

        /// <summary>
        ///     Downloads metadata on a user.
        /// </summary>
        /// <param name="username">the user's username</param>
        /// <returns>the metadata on the user</returns>
        protected string GetMetadata(string username)
        {
            const string url = "https://api.scratch.mit.edu/users/{0}";
            return _downloader.GetContents(string.Format(url, username));
        }
    }
}
