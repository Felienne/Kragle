using System;
using System.IO;
using Kragle.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Kragle.Scrape
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
            DateTime currentDate = DateTime.Now.Date;
            FileInfo[] users = FileStore.GetFiles(Resources.UserDirectory);
            int userTotal = users.Length;
            int userCurrent = 0;

            Logger.Log(string.Format("Downloading project lists for {0} users.", userTotal));

            // Iterate over users
            foreach (FileInfo user in users)
            {
                string username = user.Name.Remove(user.Name.Length - 5);

                userCurrent++;
                Logger.Log(LoggerHelper.FormatProgress(
                    "Downloading project list for user " + LoggerHelper.ForceLength(username, 10), userCurrent,
                    userTotal));

                // Get list of user projects
                string projects = GetUserProjects(username);
                if (projects == null)
                {
                    continue;
                }

                // Save list of projects
                FileStore.WriteFile(Resources.ProjectDirectory, username + ".json", projects);
                FileStore.WriteFile(Resources.ProjectDirectory + "/" + username,
                    currentDate.ToString("yyyy-MM-dd") + ".json", projects);
            }

            Logger.Log(string.Format("Successfully downloaded project lists for {0} users.\n", userCurrent));
        }

        /// <summary>
        ///     Downloads project code for all registered projects.
        /// </summary>
        public void DownloadProjects()
        {
            DateTime currentDate = DateTime.Now.Date;
            FileInfo[] users = FileStore.GetFiles(Resources.ProjectDirectory);

            int userTotal = users.Length;
            int userCurrent = 0;

            Logger.Log(string.Format("Downloading code for {0} users.", userTotal));

            // Iterate over users
            foreach (FileInfo user in users)
            {
                string username = user.Name.Remove(user.Name.Length - 5);

                userCurrent++;
                Logger.Log(LoggerHelper.FormatProgress(
                    "Downloading code for user " + LoggerHelper.ForceLength(username, 10), userCurrent, userTotal));

                JArray projects;
                try
                {
                    projects = JArray.Parse(FileStore.ReadFile(Resources.ProjectDirectory, username + ".json"));
                }
                catch (JsonReaderException e)
                {
                    Logger.Log("Could not parse list of projects of user `" + username + "`", e);
                    return;
                }

                // Iterate over user projects
                foreach (JToken project in projects)
                {
                    DateTime modifyDate = DateTime.Parse(project["history"]["modified"].ToString()).Date;

                    int projectId = Convert.ToInt32(project["id"].ToString());
                    string codeDir = Resources.CodeDirectory + "/" + projectId;
                    string yesterdayFileName = currentDate.AddDays(-1).ToString("yyyy-MM-dd") + ".json";
                    string todayFileName = currentDate.ToString("yyyy-MM-dd") + ".json";

                    if (FileStore.FileExists(codeDir, todayFileName))
                    {
                        // Code already downloaded today
                        continue;
                    }

                    if (currentDate.Subtract(modifyDate).Days > 0 && FileStore.FileExists(codeDir, yesterdayFileName))
                    {
                        // No code modifications in last day, copy old file
                        FileStore.CopyFile(codeDir, yesterdayFileName, codeDir, todayFileName);
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

                    FileStore.WriteFile(codeDir, todayFileName, projectCode);
                }
            }

            Logger.Log(string.Format("Successfully downloaded code for {0} users.\n", userCurrent));
        }


        /// <summary>
        ///     Downloads the list of projects of the given user.
        /// </summary>
        /// <param name="username">the user's username</param>
        /// <returns>the list of projects of the given user</returns>
        protected string GetUserProjects(string username)
        {
            JArray allProjects = new JArray();

            int pageNumber = 0;
            while (true)
            {
                JArray projectsOnPage = GetUserProjects(username, pageNumber);
                if (projectsOnPage == null)
                {
                    return null;
                }
                if (projectsOnPage.Count == 0)
                {
                    break;
                }

                allProjects.Merge(projectsOnPage);
                pageNumber++;
            }

            return allProjects.ToString(Formatting.None);
        }

        /// <summary>
        ///     Downloads the list of 20 projects on the given page of the given user.
        /// </summary>
        /// <param name="username">the user's username</param>
        /// <param name="page">the page to download</param>
        /// <returns>the list of 20 projects on the given page of the given user</returns>
        private JArray GetUserProjects(string username, int page)
        {
            const string url = "https://api.scratch.mit.edu/users/{0}/projects?offset={1}";
            string contents = _downloader.GetContents(string.Format(url, username, page * 20));

            try
            {
                return JArray.Parse(contents);
            }
            catch (Exception)
            {
                return null;
            }
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
