﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kragle.Properties;
using Newtonsoft.Json;
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
            using (CsvWriter projectWriter = new CsvWriter(FileStore.GetAbsolutePath(Resources.ProjectsCsv)))
            using (CsvWriter projectRemixWriter = new CsvWriter(FileStore.GetAbsolutePath(Resources.ProjectRemixCsv)))
            {
                FileInfo[] userFiles = FileStore.GetFiles(Resources.ProjectDirectory);
                int userTotal = userFiles.Length;
                int userCurrent = 0;

                Logger.Log("Parsing " + userTotal + " projects to CSV.");

                foreach (FileInfo userFile in userFiles)
                {
                    string username = userFile.Name.Remove(userFile.Name.Length - 5);

                    userCurrent++;
                    Logger.Log(LoggerHelper.FormatProgress(
                        "Parsing projects of user " + LoggerHelper.ForceLength(username, 10), userCurrent, userTotal));

                    JArray projectFiles;
                    try
                    {
                        projectFiles = JArray.Parse(File.ReadAllText(userFile.FullName));
                    }
                    catch (JsonReaderException e)
                    {
                        Logger.Log("The project list for user `" + userFile.Name + "` could not be parsed.", e);
                        return;
                    }

                    foreach (JToken projectFile in projectFiles)
                    {
                        if (!(projectFile is JObject))
                        {
                            Logger.Log("A project of user `" + userFile.Name + "` could not be parsed.");
                            return;
                        }

                        JObject project = (JObject) projectFile;
                        int authorId = int.Parse(project["author"]["id"].ToString());
                        int projectId = int.Parse(project["id"].ToString());
                        string remixParentId = project["remix"]["parent"].ToString();

                        projectWriter
                            .Write(authorId)
                            .Write(projectId)
                            .Write(project["history"]["created"].ToString())
                            .Write(project["history"]["shared"].ToString())
                            .Newline();

                        if (remixParentId != "")
                        {
                            projectRemixWriter
                                .Write(projectId)
                                .Write(int.Parse(remixParentId))
                                .Newline();
                        }
                    }
                }
            }

            using (CsvWriter projectMetaWriter = new CsvWriter(FileStore.GetAbsolutePath(Resources.ProjectMetaCsv)))
            {
                DirectoryInfo[] userDirs = FileStore.GetDirectories(Resources.ProjectDirectory);
                int userTotal = userDirs.Length;
                int userCurrent = 0;

                Logger.Log("Parsing metadata for " + userDirs.Length + " projects to CSV.");

                foreach (DirectoryInfo userDir in userDirs)
                {
                    string username = userDir.Name;

                    userCurrent++;
                    Logger.Log(LoggerHelper.FormatProgress(
                        "Parsing project metadata of user " + LoggerHelper.ForceLength(username, 10),
                        userCurrent, userTotal));

                    foreach (FileInfo projectFile in userDir.GetFiles())
                    {
                        JArray projects;
                        try
                        {
                            projects = JArray.Parse(File.ReadAllText(projectFile.FullName));
                        }
                        catch (JsonReaderException e)
                        {
                            Logger.Log("The project metadata list of user `" + userDir.Name + "` could not be parsed.",
                                e);
                            return;
                        }

                        foreach (JToken project in projects)
                        {
                            if (!(project is JObject))
                            {
                                Logger.Log("The metadata of a project of user `" + userDir.Name +
                                           "` could not be parsed.");
                                return;
                            }

                            JObject metadata = (JObject) project;
                            int projectId = int.Parse(metadata["id"].ToString());
                            string dataDate = projectFile.Name.Substring(0, projectFile.Name.Length - 5);

                            projectMetaWriter
                                .Write(projectId)
                                .Write(dataDate)
                                .Write(metadata["title"].ToString())
                                .Write(metadata["history"]["modified"].ToString())
                                .Write(int.Parse(metadata["stats"]["views"].ToString()))
                                .Write(int.Parse(metadata["stats"]["loves"].ToString()))
                                .Write(int.Parse(metadata["stats"]["favorites"].ToString()))
                                .Write(int.Parse(metadata["stats"]["comments"].ToString()))
                                .Newline();
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
                DirectoryInfo[] projects = FileStore.GetDirectories(Resources.CodeDirectory);
                int projectTotal = projects.Length;
                int projectCurrent = 0;

                Logger.Log("Parsing code of " + projectTotal + " projects to CSV.");

                foreach (DirectoryInfo project in projects)
                {
                    string projectName = project.Name;

                    projectCurrent++;
                    Logger.Log(LoggerHelper.FormatProgress(
                        "Parsing code of project " + LoggerHelper.ForceLength(projectName, 10),
                        projectCurrent, projectTotal));

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
