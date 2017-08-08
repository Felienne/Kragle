using System;
using System.Collections;
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
            using (CsvWriter codeProcedureWriter =
                new CsvWriter(FileStore.GetAbsolutePath(Resources.CodeProceduresCsv)))
            {
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

                        GetProcedures(projectId, DateTime.Parse(codeDate), code);
                    }
                }
            }
        }


        /// <summary>
        ///     Compiles the list of procedures in the given code.
        /// </summary>
        /// <param name="projectId">the id of the code's project</param>
        /// <param name="date">the date the code was updated</param>
        /// <param name="rawCode">the complete code of a project</param>
        /// <returns>the list of procedures in the given code</returns>
        private static void GetProcedures(int projectId, DateTime date, string rawCode)
        {
            // Procedure definitions in root
            JObject code = JObject.Parse(rawCode);
            {
                JArray scripts = code.GetValue("scripts") as JArray;

                if (scripts != null)
                {
                    foreach (JArray script in scripts.OfType<JArray>())
                    {
                        WriteToFile(new Script(projectId, date, script[2] as JArray, ScopeType.Stage, "stage"));
                    }
                }
            }

            // Procedure definitions in sprites
            JArray sprites = (JArray) code.GetValue("children");
            foreach (JObject sprite in sprites.OfType<JObject>())
            {
                if (sprite["objName"] == null)
                {
                    continue;
                }

                string spriteName = sprite["objName"].ToString();
                JArray scripts = sprite.GetValue("scripts") as JArray;

                if (scripts != null)
                {
                    foreach (JArray script in scripts.OfType<JArray>())
                    {
                        WriteToFile(new Script(projectId, date, script[2] as JArray, ScopeType.Sprite, spriteName));
                    }
                }
            }
        }


        public static void WriteToFile(Script script)
        {
            ScopeType scopeType = script.ScopeType;
            string scopeName = script.ScopeName;
            int indent = 0;

            ArrayList allStatements = Flatten(script, script.Code, ref scopeType, ref scopeName, ref indent);


            using (StreamWriter analysisFile = new StreamWriter(FileStore.GetAbsolutePath("code.csv"), true))
            {
                foreach (object statement in allStatements)
                {
                    analysisFile.WriteLine(script.ProgramId + ",\"" + script.Date.ToString("yyyy-MM-dd") + "\"," +
                                           statement);
                }
            }

            using (StreamWriter scriptsFile = new StreamWriter(FileStore.GetAbsolutePath("scripts.csv"), true))
            {
                scriptsFile.WriteLine(script.ProgramId + ",\"" + script.Date.ToString("yyyy-MM-dd") + "\",\"" +
                                      scopeType + "\",\"" + scopeName + "\"," + allStatements.Count);
            }
        }

        private static ArrayList Flatten(Script script, JArray scripts, ref ScopeType scopeType, ref string scopeName,
            ref int indent)
        {
            ArrayList result = new ArrayList();

            string toPrint = "\"" + scopeType + "\",\"" + scopeName + "\"," + indent;
            bool added = false;


            foreach (JToken innerScript in scripts)
            {
                if (innerScript is JValue)
                {
                    toPrint += "," + innerScript;
                    added = true;
                }
                else if (innerScript is JArray)
                {
                    JArray array = (JArray) innerScript;

                    if (AllOneField(array))
                    {
                        if (!array.Any())
                        {
                            toPrint += ",[]";
                        }
                        else
                        {
                            int newIndent = indent + 1;

                            foreach (
                                object item in
                                Flatten(script, array, ref scopeType, ref scopeName, ref newIndent))
                            {
                                result.Add(item);
                            }
                        }
                    }
                    else
                    {
                        if (array.Any() && array[0].ToString() == "procDef")
                        {
                            toPrint += ",\"procdef\"";
                            scopeType = ScopeType.ProcDef;

                            using (StreamWriter scriptsFile =
                                new StreamWriter(FileStore.GetAbsolutePath("procedures.csv"), true))
                            {
                                scriptsFile.WriteLine(script.ProgramId + ",\"" + script.Date.ToString("yyyy-MM-dd") +
                                                      "\",\"" + scopeName + "\"," + array[1].ToString(Formatting.None) +
                                                      "," + array[2].Count());
                            }

                            scopeName = array[1].ToString();
                            added = true;
                        }
                        else
                        {
                            int newIndent = indent + 1;

                            foreach (
                                object item in
                                Flatten(script, array, ref scopeType, ref scopeName, ref newIndent))
                            {
                                result.Add(item);
                            }
                        }
                    }
                }
            }

            if (added)
            {
                result.Add(toPrint);
            }


            return result;
        }

        private static bool AllOneField(JArray script)
        {
            return script.Count <= 1 && script.OfType<JArray>().All(AllOneField);
        }


        public class Script
        {
            public int ProgramId;
            public DateTime Date;
            public JArray Code;
            public ScopeType ScopeType;
            public string ScopeName;

            public Script(int programId, DateTime date, JArray code, ScopeType scopeType, string scopeName)
            {
                ProgramId = programId;
                Date = date;
                Code = code;
                ScopeType = scopeType;
                ScopeName = scopeName;
            }
        }

        public enum ScopeType
        {
            Stage,
            Sprite,
            ProcDef
        }
    }
}
