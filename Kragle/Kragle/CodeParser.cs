using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kragle.Properties;
using Newtonsoft.Json.Linq;


namespace Kragle
{
    /// <summary>
    ///     A <code>CodeParser</code> parses all available project code. It generates several CSV files which can be
    ///     imported into a relational database for further analysis.
    /// </summary>
    public sealed class CodeParser
    {
        private static readonly Logger Logger = Logger.GetLogger("CodeParser");


        /// <summary>
        ///     Writes all user data to CSV files.
        /// </summary>
        public void WriteUsers()
        {
            using (CsvWriter writer = new CsvWriter(FileStore.GetAbsolutePath(Resources.UsersCsv)))
            {
                FileInfo[] users = FileStore.GetFiles(Resources.UserDirectory);
                Logger.Log("Writing " + users.Length + " users to CSV.");

                foreach (FileInfo userFile in users)
                {
                    JObject user = JObject.Parse(File.ReadAllText(userFile.FullName));

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
            using (CsvWriter projectWriter = new CsvWriter(FileStore.GetAbsolutePath(Resources.ProjectsCsv)))
            using (CsvWriter userProjectWriter =
                new CsvWriter(FileStore.GetAbsolutePath(Resources.UserProjectsCsv)))
            {
                FileInfo[] users = FileStore.GetFiles(Resources.ProjectDirectory);
                Logger.Log("Writing " + users.Length + " projects to CSV.");

                foreach (FileInfo user in users)
                {
                    JArray projectInfo = JArray.Parse(File.ReadAllText(user.FullName));

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
            using (CsvWriter projectCodeWriter = new CsvWriter(FileStore.GetAbsolutePath(Resources.ProjectCodeCsv)))
            using (CsvWriter codeProcedureWriter =
                new CsvWriter(FileStore.GetAbsolutePath(Resources.CodeProceduresCsv)))
            {
                DirectoryInfo[] projects = FileStore.GetDirectories(Resources.CodeDirectory);
                Logger.Log("Writing " + projects.Length + " projects' code to CSV.");

                foreach (DirectoryInfo project in projects)
                {
                    foreach (FileInfo codeFile in project.GetFiles())
                    {
                        string code = File.ReadAllText(codeFile.FullName);

                        projectCodeWriter
                            .Write(int.Parse(project.Name))
                            .Write(codeFile.Name)
                            .Write(code)
                            .Newline();

                        foreach (Tuple<string, string> procedure in GetProcedures(code))
                        {
                            codeProcedureWriter
                                .Write(int.Parse(project.Name))
                                .Write(codeFile.Name)
                                .Write(procedure.Item1 ?? "null")
                                .Write(procedure.Item2)
                                .Newline();
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Compiles the list of procedure definitions in the given code.
        /// </summary>
        /// <param name="rawCode">the complete code of a project</param>
        /// <returns>the list of procedure definitions in the given code</returns>
        public List<Tuple<string, string>> GetProcedures(string rawCode)
        {
            List<Tuple<string, string>> procedures = new List<Tuple<string, string>>();

            // Procedure definitions in root
            JObject code = JObject.Parse(rawCode);
            {
                JArray scripts = code.GetValue("scripts") as JArray;
                if (scripts != null)
                {
                    procedures.AddRange(
                        from script in scripts.OfType<JArray>()
                        where script[2].First.First.ToString() == "procDef"
                        select new Tuple<string, string>("null", script[2].ToString())
                    );
                }
            }

            // Procedure definitions in sprites
            JArray sprites = (JArray) code.GetValue("children");
            foreach (JObject sprite in sprites.OfType<JObject>())
            {
                JArray scripts = sprite.GetValue("scripts") as JArray;
                if (scripts == null)
                {
                    continue;
                }

                procedures.AddRange(
                    from script in scripts.OfType<JArray>()
                    where script[2].First.First.ToString() == "procDef"
                    select new Tuple<string, string>(sprite.GetValue("objName").ToString(), script[2].ToString())
                );
            }

            return procedures;
        }
    }
}
