using System;
using CommandLine;


namespace Kragle
{
    /// <summary>
    ///     Application entry point.
    /// </summary>
    internal class Program
    {
        private static string _invokedVerb;
        private static object _invokedVerbInstance;


        /// <summary>
        ///     Application entry point.
        /// </summary>
        /// <param name="args">the command-line arguments</param>
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            // Parse options
            Options options = new Options();
            if (!Parser.Default.ParseArguments(args, options, (verb, subOptions) =>
            {
                _invokedVerb = verb;
                _invokedVerbInstance = subOptions;
            }))
            {
                Environment.Exit(Parser.DefaultExitCodeFail);
            }

            // Select action
            switch (_invokedVerb)
            {
                case "reset":
                    ResetSubOptions resetSubOptions = (ResetSubOptions) _invokedVerbInstance;
                    Database.Reset(resetSubOptions.DatabaseHost, resetSubOptions.DatabasePort,
                        resetSubOptions.DatabaseUser, resetSubOptions.DatabasePass, resetSubOptions.DatabaseName);
                    break;

                case "users":
                    UsersSubOptions userSubOptions = (UsersSubOptions) _invokedVerbInstance;
                    break;

                case "projects":
                    ProjectsSubOptions projectsSubOptions = (ProjectsSubOptions) _invokedVerbInstance;
                    break;

                case "code":
                    CodeSubOptions codeSubOptions = (CodeSubOptions) _invokedVerbInstance;
                    break;

                case "parse":
                    ParseSubOptions parseSubOptions = (ParseSubOptions) _invokedVerbInstance;
                    break;

                case "analyze":
                    AnalyzeSubOptions analyzeSubOptions = (AnalyzeSubOptions) _invokedVerbInstance;
                    break;

                default:
                    Environment.Exit(Parser.DefaultExitCodeFail);
                    break;
            }

            // Finish
            Console.WriteLine("\nProgram finished execution.\nPress enter to close this window.");
            Console.ReadLine();
        }
    }


    /// <summary>
    ///     Main command-line options.
    /// </summary>
    internal class Options
    {
        [VerbOption("reset", HelpText = "Reset the database file, deleting all contents")]
        public ResetSubOptions ResetSubOptions { get; set; }

        [VerbOption("users", HelpText = "Generate the list of users who most recently shared a project")]
        public UsersSubOptions UsersSubOptions { get; set; }

        [VerbOption("projects", HelpText = "Generate the list of projects of all registered users")]
        public ProjectsSubOptions ProjectsSubOptions { get; set; }

        [VerbOption("code", HelpText = "Download the latest code of all registered projects")]
        public CodeSubOptions CodeSubOptions { get; set; }

        [VerbOption("parse", HelpText = "Parse all registered code")]
        public ParseSubOptions ParseSubOptions { get; set; }

        [VerbOption("analyse", HelpText = "Analyse all parsed code")]
        public AnalyzeSubOptions AnalyzeSubOptions { get; set; }
    }

    /// <summary>
    ///     Command-line options for all verbs interacting with the database.
    /// </summary>
    internal abstract class DatabaseSharedOptions
    {
        [Option('h', "host", DefaultValue = "127.0.0.1", HelpText = "The database host address")]
        public string DatabaseHost { get; set; }

        [Option('u', "user", DefaultValue = "postgres", HelpText = "The database user account name")]
        public string DatabaseUser { get; set; }

        [Option('p', "pass", Required = true, HelpText = "The database user account password")]
        public string DatabasePass { get; set; }

        [Option("port", DefaultValue = 5432, HelpText = "The database port")]
        public int DatabasePort { get; set; }

        [Option("db", DefaultValue = "kragle", HelpText = "The database name")]
        public string DatabaseName { get; set; }
    }

    /// <summary>
    ///     Command-line options for the `reset` verb.
    /// </summary>
    internal class ResetSubOptions : DatabaseSharedOptions
    {
    }

    /// <summary>
    ///     Command-line options for the `users` verb.
    /// </summary>
    internal class UsersSubOptions : DatabaseSharedOptions
    {
        [Option('n', "number", DefaultValue = int.MaxValue, HelpText = "The number of users to scrape")]
        public int Count { get; set; }

        [Option('r', "reset database", HelpText = "Reset the database")]
        public bool Reset { get; set; }

        [Option('c', "disable caching", HelpText = "Disable caching with requests; slows down the process significantly"
        )]
        public bool NoCache { get; set; }
    }

    /// <summary>
    ///     Command-line options for the `projects` verb.
    /// </summary>
    internal class ProjectsSubOptions : DatabaseSharedOptions
    {
    }

    /// <summary>
    ///     Command-line options for the `code` verb.
    /// </summary>
    internal class CodeSubOptions : DatabaseSharedOptions
    {
    }

    /// <summary>
    ///     Command-line options for the `parse` verb.
    /// </summary>
    internal class ParseSubOptions : DatabaseSharedOptions
    {
    }

    /// <summary>
    ///     Command-line options for the `analyze` verb.
    /// </summary>
    internal class AnalyzeSubOptions : DatabaseSharedOptions
    {
    }
}
