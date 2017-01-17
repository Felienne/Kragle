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

                default:
                    Environment.Exit(Parser.DefaultExitCodeFail);
                    break;
            }
        }
    }


    /// <summary>
    ///     Main command-line options.
    /// </summary>
    internal class Options
    {
        [VerbOption("reset", HelpText = "Reset all files")]
        public ResetSubOptions ResetSubOptions { get; set; }

        [VerbOption("users", HelpText = "Generate/update the list of users who most recently shared a project")]
        public UsersSubOptions UsersSubOptions { get; set; }

        [VerbOption("projects", HelpText = "Generate the list of projects of all registered users")]
        public ProjectsSubOptions ProjectsSubOptions { get; set; }

        [VerbOption("code", HelpText = "Download the latest code of all registered projects")]
        public CodeSubOptions CodeSubOptions { get; set; }
    }

    /// <summary>
    ///     Command-line options for all verbs interacting with the database.
    /// </summary>
    internal abstract class DatabaseSharedOptions
    {
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
        [Option('n', "number", HelpText = "The number of users to scrape")]
        public int Count { get; set; }

        [Option('c', "nocache", HelpText = "Disable caching; slows down the process significantly")]
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
}
