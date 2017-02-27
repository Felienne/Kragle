using System;
using System.IO;
using Newtonsoft.Json.Linq;


namespace Kragle
{
    /// <summary>
    ///     A <code>CodeParser</code> parses all available project code. It generates several CSV files which can be
    ///     imported into a relational database for further analysis.
    /// </summary>
    public sealed class CodeParser
    {
        private readonly FileStore _fs;


        /// <summary>
        ///     Constructs a new <code>CodeParser</code>.
        /// </summary>
        /// <param name="fs">the <code>FileStore</code> to interact with</param>
        public CodeParser(FileStore fs)
        {
            _fs = fs;
        }


        /// <summary>
        ///     Writes all user data to CSV files.
        /// </summary>
        public void WriteUsers()
        {
            FileInfo[] files = _fs.GetFiles("users");
            Console.WriteLine("Writing " + files.Length + " users to CSV.");

            using (CsvWriter writer = new CsvWriter(_fs.GetRootPath() + "/users.csv"))
            {
                foreach (FileInfo file in files)
                {
                    JObject user = JObject.Parse(File.ReadAllText(file.FullName));

                    writer
                        .Write(int.Parse(user["id"].ToString()))
                        .Write(user["username"].ToString())
                        .Newline();
                }
            }
        }

        /// <summary>
        ///     Writes all projects and their relations to authors to CSV files.
        /// </summary>
        public void WriteProjects()
        {
            using (CsvWriter projectWriter = new CsvWriter(_fs.GetRootPath() + "/projects.csv"))
            using (CsvWriter userProjectWriter = new CsvWriter(_fs.GetRootPath() + "/userprojects.csv"))
            {
                DirectoryInfo[] users = _fs.GetDirectories("projects"); // Project directory contains directory per user
                Console.WriteLine("Writing " + users.Length + " projects to CSV.");

                foreach (DirectoryInfo user in users)
                {
                    JArray projectInfo = JArray.Parse(File.ReadAllText(user.FullName + "/list"));

                    foreach (JToken jToken in projectInfo)
                    {
                        if (!(jToken is JObject))
                        {
                            continue;
                        }

                        JObject project = (JObject) jToken;
                        int authorId = int.Parse(project["author"]["id"].ToString());
                        int projectId = int.Parse(project["id"].ToString());

                        userProjectWriter
                            .Write(authorId)
                            .Write(projectId)
                            .Newline();
                        projectWriter
                            .Write(projectId)
                            .Write(project["title"].ToString())
                            .Write(project["description"].ToString())
                            .Newline();
                    }
                }
            }
        }

        /// <summary>
        ///     Writes all code and their relations to projects to CSV files.
        /// </summary>
        public void WriteCode()
        {
            using (CsvWriter projectCodeWriter = new CsvWriter(_fs.GetRootPath() + "/projectcode.csv"))
            {
                DirectoryInfo[] projects = _fs.GetDirectories("code"); // Code directory contains directory per code
                Console.WriteLine("Writing " + projects.Length + " projects' code to CSV.");

                foreach (DirectoryInfo project in projects)
                {
                    foreach (FileInfo code in project.GetFiles())
                    {
                        projectCodeWriter
                            .Write(int.Parse(project.Name))
                            .Write(code.Name)
                            .Write(File.ReadAllText(code.FullName))
                            .Newline();
                    }
                }
            }
        }
    }
}
