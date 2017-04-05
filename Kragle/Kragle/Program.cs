using System;
using System.Diagnostics;
using CommandLine;
using CommandLine.Text;


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
                Environment.Exit(1);
            }

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
                {
                    ResetSubOptions subOptions = (ResetSubOptions) _invokedVerbInstance;
                    FileStore.Init(subOptions.Path);

                    Console.Write("Remove all users, projects, and code? (y/n)");
                    string confirm = Console.ReadLine();

                    if (confirm != null && confirm.ToLower() == "y")
                    {
                        FileStore.RemoveDirectory("users");
                        FileStore.RemoveDirectory("projects");
                        FileStore.RemoveDirectory("code");
                        Console.WriteLine("Removed all users, projects, and code.");
                    }
                    else
                    {
                        Console.WriteLine("Cancelled.");
                    }

                    Environment.Exit(0); // Suppress exit message
                    break;
                }

                case "open":
                {
                    OpenSubOptions subOptions = (OpenSubOptions) _invokedVerbInstance;
                    FileStore.Init(subOptions.Path);

                    Process.Start(FileStore.GetRootPath());

                    Environment.Exit(0); // Suppress exit message
                    break;
                }

                case "users":
                {
                    UsersSubOptions subOptions = (UsersSubOptions) _invokedVerbInstance;
                    FileStore.Init(subOptions.Path);

                    Downloader downloader = new Downloader(subOptions.NoCache);

                    UserScraper scraper = new UserScraper(downloader, subOptions.Count);
                    scraper.ScrapeUsers();
                    if (subOptions.Meta)
                    {
                        scraper.DownloadMetaData();
                    }

                    break;
                }

                case "projects":
                {
                    ProjectsSubOptions subOptions = (ProjectsSubOptions) _invokedVerbInstance;
                    FileStore.Init(subOptions.Path);

                    Downloader downloader = new Downloader(subOptions.NoCache);

                    ProjectScraper scraper = new ProjectScraper(downloader);
                    if (subOptions.Update)
                    {
                        scraper.UpdateProjectList();
                    }
                    if (subOptions.Download)
                    {
                        scraper.DownloadProjects();
                    }

                    break;
                }

                case "parse":
                {
                    ParseSubOptions subOptions = (ParseSubOptions) _invokedVerbInstance;

                    CodeParser parser = new CodeParser(new FileStore(subOptions.Path));
                    parser.WriteUsers();
                    parser.WriteProjects();
                    parser.WriteCode();

                    break;
                }

                default:
                {
                    Environment.Exit(Parser.DefaultExitCodeFail);
                    break;
                }
            }

            // Exit message
            Console.WriteLine("Done.");
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

        [VerbOption("parse", HelpText = "Generate the list of projects of all registered users")]
        public ParseSubOptions ParseSubOptions { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }
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
        [Option('n', "number", DefaultValue = int.MaxValue, HelpText = "The number of users to scrape")]
        public int Count { get; set; }

        [Option('m', "meta", HelpText = "Download user meta-data")]
        public bool Meta { get; set; }
    }

    /// <summary>
    ///     Command-line options for the 'projects' verb.
    /// </summary>
    internal class ProjectsSubOptions : FileSystemSharedOptions
    {
        [Option('u', "update", HelpText = "Update the list of registered projects")]
        public bool Update { get; set; }

        [Option('d', "download", HelpText = "Download project code")]
        public bool Download { get; set; }
    }

    /// <summary>
    ///     Command-line options for the 'parse' verb.
    /// </summary>
    internal class ParseSubOptions : FileSystemSharedOptions
    {
    }
}
