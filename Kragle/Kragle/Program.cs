using System;
using System.Diagnostics;
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
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Invalid parameters given: Missing command verb");
                Console.ReadLine();
                Environment.Exit(1);
            }

            Options options = new Options();
            if (!Parser.Default.ParseArguments(args, options, (verb, subOptions) =>
            {
                _invokedVerb = verb;
                _invokedVerbInstance = subOptions;
            }))
            {
                Console.WriteLine("Invalid parameters given: ");
                Console.ReadLine();
                Environment.Exit(Parser.DefaultExitCodeFail);
            }

            // Select action
            switch (_invokedVerb)
            {
                case "reset":
                {
                    ResetSubOptions subOptions = (ResetSubOptions) _invokedVerbInstance;

                    FileStore fs = new FileStore(subOptions.Path);
                    fs.RemoveDirectory("./");
                    Console.WriteLine("Removed all files.");

                    break;
                }

                case "open":
                {
                    OpenSubOptions subOptions = (OpenSubOptions) _invokedVerbInstance;

                    Process.Start(new FileStore(subOptions.Path).GetRootPath());

                    Environment.Exit(0); // Suppress exit message
                    break;
                }

                case "users":
                {
                    UsersSubOptions subOptions = (UsersSubOptions) _invokedVerbInstance;
                    break;
                }

                case "projects":
                {
                    ProjectsSubOptions subOptions = (ProjectsSubOptions) _invokedVerbInstance;

                    FileStore fs = new FileStore(subOptions.Path);
                    ProjectScraper scraper = new ProjectScraper(fs, subOptions.NoCache);

                    scraper.UpdateProjectList();
                    Console.WriteLine();
                    scraper.DownloadProjects();

                    break;
                }

                case "code":
                {
                    CodeSubOptions subOptions = (CodeSubOptions) _invokedVerbInstance;
                    break;
                }

                default:
                {
                    Environment.Exit(Parser.DefaultExitCodeFail);
                    break;
                }
            }

            // Exit message
            Console.WriteLine("\nDone. Press enter to close this window.");
            Console.ReadLine();
        }
    }


    /// <summary>
    ///     Main command-line options.
    /// </summary>
    internal class Options
    {
        [VerbOption("reset", HelpText = "Reset all files")]
        public ResetSubOptions ResetSubOptions { get; set; }

        [VerbOption("open", HelpText = "Opens the data folder in Windows Explorer")]
        public OpenSubOptions OpenSubOptions { get; set; }

        [VerbOption("users", HelpText = "Generate/update the list of users who most recently shared a project")]
        public UsersSubOptions UsersSubOptions { get; set; }

        [VerbOption("projects", HelpText = "Generate the list of projects of all registered users")]
        public ProjectsSubOptions ProjectsSubOptions { get; set; }

        [VerbOption("code", HelpText = "Download the latest code of all registered projects")]
        public CodeSubOptions CodeSubOptions { get; set; }
    }

    /// <summary>
    ///     Command-line options for all verbs interacting with the file system.
    /// </summary>
    internal abstract class FileSystemSharedOptions
    {
        [Option('p', "path", HelpText = "The path files should be read from and written to")]
        public string Path { get; set; }

        [Option('c', "nocache", HelpText = "Disable caching; slows down the process significantly")]
        public bool NoCache { get; set; }
    }

    /// <summary>
    ///     Command-line options for the 'reset' verb.
    /// </summary>
    internal class ResetSubOptions : FileSystemSharedOptions
    {
    }

    /// <summary>
    ///     Command-line options for the 'open' verb.
    /// </summary>
    internal class OpenSubOptions : FileSystemSharedOptions
    {
    }

    /// <summary>
    ///     Command-line options for the 'users' verb.
    /// </summary>
    internal class UsersSubOptions : FileSystemSharedOptions
    {
        [Option('n', "number", HelpText = "The number of users to scrape")]
        public int Count { get; set; }
    }

    /// <summary>
    ///     Command-line options for the 'projects' verb.
    /// </summary>
    internal class ProjectsSubOptions : FileSystemSharedOptions
    {
    }

    /// <summary>
    ///     Command-line options for the 'code' verb.
    /// </summary>
    internal class CodeSubOptions : FileSystemSharedOptions
    {
    }
}
