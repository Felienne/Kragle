using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kragle.Properties;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShellProgressBar;


namespace Kragle.Parse
{
    /// <summary>
    ///     A <code>CodeParser</code> parses all available project code. It generates several CSV files which can be
    ///     imported into a relational database for further analysis.
    /// </summary>
    public sealed class CodeParser
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CodeParser));
        private const int ParamCount = 20;


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

                Logger.DebugFormat("Parsing {0} users to CSV.", userTotal);

                if (userFiles.Length > 0 && File.ReadAllText(userFiles[0].FullName).Length == 0)
                {
                    Logger.Fatal("Missing metadata for users.");
                    return;
                }

                using (ProgressBar progressBar = new ProgressBar(userTotal, "Initializing"))
                {
                    foreach (FileInfo userFile in userFiles)
                    {
                        userCurrent++;
                        string username = userFile.Name.Remove(userFile.Name.Length - 5);

                        string logMessage = "Parsing user " + LoggerHelper.ForceLength(username, 10);
                        progressBar.Tick(logMessage);
                        Logger.Debug(LoggerHelper.FormatProgress(logMessage, userCurrent, userTotal));

                        string contents = File.ReadAllText(userFile.FullName);
                        if (contents.Length == 0)
                        {
                            Logger.Fatal("Missing metadata for user " + userFile.Name);
                            return;
                        }

                        JObject user;
                        try
                        {
                            user = JObject.Parse(File.ReadAllText(userFile.FullName));
                        }
                        catch (JsonReaderException e)
                        {
                            Logger.Fatal("The metadata for user `" + userFile.Name + "` could not be parsed.", e);
                            return;
                        }

                        writer
                            .Write(int.Parse(user["id"].ToString()))
                            .Write(user["username"].ToString())
                            .Write(user["history"]["joined"].ToString())
                            .Write(user["profile"]["country"].ToString())
                            .Newline();
                    }

                    progressBar.UpdateMessage("Finished parsing users");
                }

                Logger.DebugFormat("Successfully parsed {0} users to CSV.", userTotal);
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

                Logger.DebugFormat("Parsing metadata for {0} users to CSV.", userDirs.Length);

                ISet<int> projectHistory = new HashSet<int>();

                using (ProgressBar progressBar = new ProgressBar(userTotal, "Initializing"))
                {
                    foreach (DirectoryInfo userDir in userDirs)
                    {
                        userCurrent++;
                        string username = userDir.Name;

                        string logMessage = "Parsing project lists of user " + LoggerHelper.ForceLength(username, 10);
                        progressBar.Tick(logMessage);
                        Logger.Debug(LoggerHelper.FormatProgress(logMessage, userCurrent, userTotal));

                        foreach (FileInfo projectListFile in userDir.GetFiles())
                        {
                            JArray projectList;
                            try
                            {
                                projectList = JArray.Parse(File.ReadAllText(projectListFile.FullName));
                            }
                            catch (JsonReaderException e)
                            {
                                Logger.Fatal("The project list for user `" + username + "` could not be parsed.", e);
                                return;
                            }

                            foreach (JToken projectFile in projectList)
                            {
                                if (!(projectFile is JObject))
                                {
                                    Logger.Fatal("A project of user `" + username + "` could not be parsed.");
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

                    progressBar.UpdateMessage("Finished parsing metadata");
                }

                Logger.DebugFormat("Successfully parsed metadata for {0} users to CSV.", userDirs.Length);
            }
        }

        /// <summary>
        ///     Writes all code and their relations to projects to CSV files.
        /// </summary>
        public void WriteCode()
        {
            DirectoryInfo[] projects = FileStore.GetDirectories(Resources.CodeDirectory);
            int projectTotal = projects.Length;
            int projectCurrent = 0;

            Logger.DebugFormat("Parsing code of {0} projects to CSV.", projectTotal);

            using (CsvWriter commandWriter = new CsvWriter(FileStore.GetAbsolutePath("commands.csv"), true))
            using (CsvWriter scriptWriter = new CsvWriter(FileStore.GetAbsolutePath("scripts.csv"), true))
            using (CsvWriter procedureWriter = new CsvWriter(FileStore.GetAbsolutePath("procedures.csv"), true))
            {
                commandWriter.WriteHeaders("scriptId", "projectId", "date", "depth", "scopeType", "scopeName",
                    "command", "param1", "param2", "param3", "param4", "param5", "param6", "param7", "param8", "param9",
                    "param10", "param11", "param12", "param13", "param14", "param15", "param16", "param17", "param18",
                    "param19", "param20");
                scriptWriter.WriteHeaders("scriptId", "projectId", "date", "scopeType", "scopeName", "lineCount");
                procedureWriter.WriteHeaders("projectId", "date", "scopeType", "scopeName", "name", "argumentCount");


                using (ProgressBar progressBar = new ProgressBar(projectTotal, "Initializing"))
                {
                    foreach (DirectoryInfo project in projects)
                    {
                        projectCurrent++;
                        int projectId = int.Parse(project.Name);

                        string logMessage = "Parsing code of project " + projectId;
                        progressBar.Tick(logMessage);
                        Logger.Debug(LoggerHelper.FormatProgress(logMessage, projectCurrent, projectTotal));

                        foreach (FileInfo codeFile in project.GetFiles())
                        {
                            string code = File.ReadAllText(codeFile.FullName);
                            string codeDate = codeFile.Name.Substring(0, codeFile.Name.Length - 5);

                            ParsedCode parsedCode = ParseCode(projectId, DateTime.Parse(codeDate), code);
                            WriteAllToCsv(commandWriter, parsedCode.Commands);
                            WriteAllToCsv(scriptWriter, parsedCode.Scripts);
                            WriteAllToCsv(procedureWriter, parsedCode.Procedures);
                        }
                    }
                }
            }

            Logger.DebugFormat("Successfully parsed code of {0} projects to CSV.", projectTotal);
        }


        /// <summary>
        ///     Compiles the lists of commands, scripts, and procedures in the given code.
        /// </summary>
        /// <param name="projectId">the id of the code's project</param>
        /// <param name="date">the date the code was updated</param>
        /// <param name="rawCode">the complete code of a project</param>
        /// <returns>the list of commands, scripts, and procedures in the given code</returns>
        private static ParsedCode ParseCode(int projectId, DateTime date, string rawCode)
        {
            ParsedCode parsedCode = new ParsedCode();

            // Procedure definitions in root
            JObject code = JObject.Parse(rawCode);
            {
                JArray scripts = code.GetValue("scripts") as JArray;

                if (scripts != null)
                {
                    foreach (JArray scriptArray in scripts.OfType<JArray>())
                    {
                        Script script = new Script(projectId, date, scriptArray[2] as JArray, ScopeType.Stage, "stage");

                        parsedCode.Join(ParseScript(script));
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
                    foreach (JArray scriptArray in scripts.OfType<JArray>())
                    {
                        Script script = new Script(projectId, date, scriptArray[2] as JArray, ScopeType.Sprite,
                            spriteName);

                        parsedCode.Join(ParseScript(script));
                    }
                }
            }

            return parsedCode;
        }


        /// <summary>
        ///     Compiles the given script.
        /// </summary>
        /// <param name="script">a script</param>
        /// <returns>the list of commands and procedures in the given script, and the script itself</returns>
        private static ParsedCode ParseScript(Script script)
        {
            ParsedCode scriptCode = new ParsedCode();

            ScopeType scopeType = script.ScopeType;
            string scopeName = script.ScopeName;
            int indent = 0;

            scriptCode.Join(ParseScripts(script, script.Code, ref scopeType, ref scopeName, ref indent));
            scriptCode.Scripts.Add(new List<object>
            {
                script.ScriptId,
                script.ProjectId,
                script.Date.ToString("yyyy-MM-dd"),
                scopeType,
                scopeName,
                scriptCode.Commands.Count
            });

            return scriptCode;
        }

        /// <summary>
        ///     Recursively compiles the lists of commands and procedures in the given script.
        /// </summary>
        /// <param name="script">a script</param>
        /// <param name="scripts">the script's code</param>
        /// <param name="scopeType">the type of the current scope</param>
        /// <param name="scopeName">the name of the current scope</param>
        /// <param name="depth">the current recursive depth</param>
        /// <returns>the lists of commands and procedures in the given script</returns>
        private static ParsedCode ParseScripts(Script script, JArray scripts, ref ScopeType scopeType,
            ref string scopeName, ref int depth)
        {
            ParsedCode parsedCode = new ParsedCode();
            List<object> command = new List<object>
            {
                0,
                script.ScriptId,
                script.ProjectId,
                script.Date.ToString("yyyy-MM-dd"),
                depth,
                scopeType,
                scopeName
            };
            bool added = false;

            int i = 0;
            foreach (JToken innerScript in scripts)
            {
                if (innerScript is JValue)
                {
                    i++;
                    added = true;
                    command.Add(innerScript);
                }
                else if (innerScript is JArray)
                {
                    JArray array = (JArray) innerScript;

                    if (AllOneField(array))
                    {
                        if (!array.Any())
                        {
                            command.Add("[]");
                        }
                        else
                        {
                            int newDepth = depth + 1;

                            parsedCode.Join(ParseScripts(script, array, ref scopeType, ref scopeName, ref newDepth));
                        }
                    }
                    else
                    {
                        if (array.Any() && array[0].ToString() == "procDef")
                        {
                            i++;
                            added = true;
                            command.Add("procdef");

                            parsedCode.Procedures.Add(new List<object>
                            {
                                0,
                                script.ProjectId,
                                script.Date.ToString("yyyy-MM-dd"),
                                scopeType,
                                scopeName,
                                array[1].ToString(),
                                array[2].Count()
                            });

                            scopeType = ScopeType.ProcDef;
                            scopeName = array[1].ToString();
                        }
                        else
                        {
                            int newDepth = depth + 1;

                            parsedCode.Join(
                                ParseScripts(script, array, ref scopeType, ref scopeName, ref newDepth));
                        }
                    }
                }
            }

            for (; i < ParamCount + 1; i++)
            {
                command.Add("");
            }

            if (added)
            {
                parsedCode.Commands.Add(command);
            }

            return parsedCode;
        }

        /// <summary>
        ///     Writes all given data using the given writer.
        /// </summary>
        /// <param name="writer">a <code>CsvWriter</code></param>
        /// <param name="data">the data to write</param>
        private static void WriteAllToCsv(CsvWriter writer, IEnumerable<List<object>> data)
        {
            foreach (List<object> datum in data)
            {
                foreach (object column in datum)
                {
                    writer.Write(column);
                }

                writer.Newline();
            }
        }


        /// <summary>
        /// Returns true iff. the given script consists of one field, or of one field 
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        private static bool AllOneField(JArray script)
        {
            return script.Count <= 1 && script.OfType<JArray>().All(AllOneField);
        }

        private class Script
        {
            private static int _scriptCount;

            public readonly int ScriptId;
            public readonly int ProjectId;
            public readonly DateTime Date;
            public readonly JArray Code;
            public readonly ScopeType ScopeType;
            public readonly string ScopeName;

            public Script(int projectId, DateTime date, JArray code, ScopeType scopeType, string scopeName)
            {
                ScriptId = _scriptCount++;
                ProjectId = projectId;
                Date = date;
                Code = code;
                ScopeType = scopeType;
                ScopeName = scopeName;
            }
        }

        private enum ScopeType
        {
            Stage,
            Sprite,
            ProcDef
        }

        private class ParsedCode
        {
            public readonly List<List<object>> Commands;
            public readonly List<List<object>> Scripts;
            public readonly List<List<object>> Procedures;


            public ParsedCode()
            {
                Commands = new List<List<object>>();
                Scripts = new List<List<object>>();
                Procedures = new List<List<object>>();
            }


            public void Join(ParsedCode parsedCode)
            {
                Commands.AddRange(parsedCode.Commands);
                Scripts.AddRange(parsedCode.Scripts);
                Procedures.AddRange(parsedCode.Procedures);
            }
        }
    }
}
