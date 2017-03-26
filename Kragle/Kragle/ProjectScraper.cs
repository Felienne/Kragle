﻿using System;
using System.IO;
using Newtonsoft.Json.Linq;


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

            // Iterate over users
            foreach (FileInfo user in users)
            {
                // Get list of user projects
                JArray projects = GetUserProjects(user.Name);
                if (projects == null)
                {
                    continue;
                }

                // Save list of projects
                _fs.WriteFile("projects", user.Name, projects.ToString());

                userCurrent++;
                Console.WriteLine("{0:P2}", userCurrent / (double) userTotal);
            }
        }

        /// <summary>
        ///     Downloads project code for all registered projects.
        /// </summary>
        public void DownloadProjects()
        {
            FileInfo[] users = _fs.GetFiles("projects");
            int userTotal = users.Length;
            int userCurrent = 0;

            Console.WriteLine("Downloading code for " + users.Length + " users.");

            // Iterate over users
            foreach (FileInfo user in users)
            {
                string username = user.Name;
                JArray projects = JArray.Parse(_fs.ReadFile("projects", username));

                // Iterate over user projects
                foreach (JToken project in projects)
                {
                    DateTime currentDate = DateTime.Now.Date;
                    DateTime modifyDate = DateTime.Parse(project["history"]["modified"].ToString()).Date;

                    int projectId = Convert.ToInt32(project["id"].ToString());
                    string codeDir = "code/" + projectId;
                    string yesterdayFileName = currentDate.AddDays(-1).ToString("yyyy-MM-dd");
                    string todayFileName = currentDate.ToString("yyyy-MM-dd");

                    if (_fs.FileExists(codeDir, todayFileName))
                    {
                        // Code already downloaded today
                        continue;
                    }

                    if (currentDate.Subtract(modifyDate).Days > 0 && _fs.FileExists(codeDir, yesterdayFileName))
                    {
                        // No code modifications in last day, copy old file
                        _fs.CopyFile(codeDir, yesterdayFileName, codeDir, todayFileName);
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
                    _fs.WriteFile(codeDir, todayFileName, projectCode);
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
