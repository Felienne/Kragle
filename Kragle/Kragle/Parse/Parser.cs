using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kragle.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Kragle.Parse
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
                writer.WriteHeaders("id", "username", "joinDate", "country");

                FileInfo[] userFiles = FileStore.GetFiles(Resources.UserDirectory);
                int userTotal = userFiles.Length;
                int userCurrent = 0;

                Logger.Log("Parsing " + userTotal + " users to CSV.");

                if (userFiles.Length > 0 && File.ReadAllText(userFiles[0].FullName).Length == 0)
                {
                    Logger.Log("Missing metadata for users.");
                    return;
                }

                foreach (FileInfo userFile in userFiles)
                {
                    string username = userFile.Name.Remove(userFile.Name.Length - 5);

                    userCurrent++;
                    Logger.Log(LoggerHelper.FormatProgress(
                        "Parsing user " + LoggerHelper.ForceLength(username, 10), userCurrent, userTotal));

                    string contents = File.ReadAllText(userFile.FullName);
                    if (contents.Length == 0)
                    {
                        Logger.Log("Missing metadata for user " + userFile.Name);
                        return;
                    }

                    JObject user;
                    try
                    {
                        user = JObject.Parse(File.ReadAllText(userFile.FullName));
                    }
                    catch (JsonReaderException e)
                    {
                        Logger.Log("The metadata for user `" + userFile.Name + "` could not be parsed.", e);
                        return;
                    }

                    writer
                        .Write(int.Parse(user["id"].ToString()))
                        .Write(user["username"].ToString())
                        .Write(user["history"]["joined"].ToString())
                        .Write(user["profile"]["country"].ToString())
                        .Newline();
                }
            }
        }

        /// <summary>
        ///     Writes all projects and their relations to authors to CSV files.
        /// </summary>
        public void WriteProjects()
        {
            using (CsvWriter projectRemixWriter = new CsvWriter(FileStore.GetAbsolutePath(Resources.ProjectRemixCsv)))
            using (CsvWriter projectWriter = new CsvWriter(FileStore.GetAbsolutePath(Resources.ProjectsCsv)))
            {
                projectRemixWriter.WriteHeaders("childId", "parentId");
                projectWriter.WriteHeaders("authorId", "date", "projectId", "title", "modifyDate", "createDate",
                    "shareDate", "viewCount", "loveCount", "favoriteCount", "commentCount");

                DirectoryInfo[] userDirs = FileStore.GetDirectories(Resources.ProjectDirectory);
                int userTotal = userDirs.Length;
                int userCurrent = 0;

                Logger.Log("Parsing metadata for " + userDirs.Length + " users to CSV.");

                ISet<int> projectHistory = new HashSet<int>();

                foreach (DirectoryInfo userDir in userDirs)
                {
                    string username = userDir.Name;

                    userCurrent++;
                    Logger.Log(LoggerHelper.FormatProgress(
                        "Parsing project lists of user " + LoggerHelper.ForceLength(username, 10),
                        userCurrent, userTotal));

                    foreach (FileInfo projectListFile in userDir.GetFiles())
                    {
                        JArray projectList;
                        try
                        {
                            projectList = JArray.Parse(File.ReadAllText(projectListFile.FullName));
                        }
                        catch (JsonReaderException e)
                        {
                            Logger.Log("The project list for user `" + username + "` could not be parsed.", e);
                            return;
                        }

                        foreach (JToken projectFile in projectList)
                        {
                            if (!(projectFile is JObject))
                            {
                                Logger.Log("A project of user `" + username + "` could not be parsed.");
                                return;
                            }

                            JObject project = (JObject) projectFile;
                            int authorId = int.Parse(project["author"]["id"].ToString());
                            int projectId = int.Parse(project["id"].ToString());
                            string remixParentId = project["remix"]["parent"].ToString();
                            string dataDate = projectListFile.Name.Substring(0, projectListFile.Name.Length - 5);

                            projectWriter
                                .Write(authorId)
                                .Write(dataDate)
                                .Write(projectId)
                                .Write(project["title"].ToString())
                                .Write(project["history"]["modified"].ToString())
                                .Write(project["history"]["created"].ToString())
                                .Write(project["history"]["shared"].ToString())
                                .Write(int.Parse(project["stats"]["views"].ToString()))
                                .Write(int.Parse(project["stats"]["loves"].ToString()))
                                .Write(int.Parse(project["stats"]["favorites"].ToString()))
                                .Write(int.Parse(project["stats"]["comments"].ToString()))
                                .Newline();

                            if (remixParentId != "" && !projectHistory.Contains(projectId))
                            {
                                projectRemixWriter
                                    .Write(projectId)
                                    .Write(int.Parse(remixParentId))
                                    .Newline();
                            }

                            projectHistory.Add(projectId);
                        }
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
                projectCodeWriter.WriteHeaders("projectId", "date", "code");
                codeProcedureWriter.WriteHeaders("projectId", "date", "stage", "procedureName", "argumentCount");

                DirectoryInfo[] projects = FileStore.GetDirectories(Resources.CodeDirectory);
                int projectTotal = projects.Length;
                int projectCurrent = 0;

                Logger.Log("Parsing code of " + projectTotal + " projects to CSV.");

                foreach (DirectoryInfo project in projects)
                {
                    int projectId = int.Parse(project.Name);

                    projectCurrent++;
                    Logger.Log(LoggerHelper.FormatProgress("Parsing code of project " + projectId,
                        projectCurrent, projectTotal));

                    foreach (FileInfo codeFile in project.GetFiles())
                    {
                        string code = File.ReadAllText(codeFile.FullName);
                        string codeDate = codeFile.Name.Substring(0, codeFile.Name.Length - 5);

                        projectCodeWriter
                            .Write(int.Parse(project.Name))
                            .Write(codeDate)
                            .Write(code)
                            .Newline();

                        foreach (Procedure procedure in GetProcedures(code))
                        {
                            codeProcedureWriter
                                .Write(int.Parse(project.Name))
                                .Write(codeDate)
                                .Write(procedure.ObjName)
                                .Write(procedure.Name)
                                .Write(procedure.ArgumentCount)
                                .Newline();
                        }
                    }
                }
            }
        }


        /// <summary>
        ///     Compiles the list of procedures in the given code.
        /// </summary>
        /// <param name="rawCode">the complete code of a project</param>
        /// <returns>the list of procedures in the given code</returns>
        private static IEnumerable<Procedure> GetProcedures(string rawCode)
        {
            List<Procedure> procedures = new List<Procedure>();

            // Procedure definitions in root
            JObject code = JObject.Parse(rawCode);
            {
                JArray scripts = code.GetValue("scripts") as JArray;
                if (scripts != null)
                {
                    procedures.AddRange(
                        from script in scripts.OfType<JArray>()
                        where script[2].First[0].ToString() == "procDef"
                        select new Procedure("null", script)
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
                    where script[2].First[0].ToString() == "procDef"
                    select new Procedure(sprite.GetValue("objName").ToString(), script)
                );
            }

            return procedures;
        }


        /// <summary>
        ///     A procedure.
        /// </summary>
        private class Procedure
        {
            internal string ObjName { get; private set; }
            internal string Name { get; private set; }
            internal int ArgumentCount { get; private set; }
            

            /// <summary>
            ///     Constructs a new <code>Procedure</code>.
            /// </summary>
            /// <param name="objName">the name of the sprite the procedure is in</param>
            /// <param name="script">the name of the procedure</param>
            public Procedure(string objName, JArray script)
            {
                ObjName = objName ?? "null";
                Name = script[2].First[1].ToString();
                ArgumentCount = script[2].First[3].Count();
            }
        }
    }
}
