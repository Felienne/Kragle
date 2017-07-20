using CommandLine;
using CommandLine.Text;


namespace Kragle.ConsoleOptions
{
    /// <summary>
    ///     Available verb commands for the command line.
    /// </summary>
    public class Options
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

        [VerbOption("archive", HelpText = "Archive all data")]
        public ArchiveSubOptions ArchiveSubOptions { get; set; }

        [VerbOption("extract", HelpText = "Extract existing archives")]
        public ExtractSubOptions ExtractSubOptions { get; set; }

        [VerbOption("validate", HelpText = "Validate downloaded data")]
        public ValidateSubOptions ValidateSubOptions { get; set; }


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
}
