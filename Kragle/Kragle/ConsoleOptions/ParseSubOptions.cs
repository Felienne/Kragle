using CommandLine;


namespace Kragle.ConsoleOptions
{
    /// <summary>
    ///     Command-line options for the 'parse' verb.
    /// </summary>
    public class ParseSubOptions : SubOptions
    {
        [Option('u', "users", HelpText = "Parse user data")]
        public bool Users { get; set; }

        [Option('p', "projects", HelpText = "Parse project data")]
        public bool Projects { get; set; }

        [Option('c', "code", HelpText = "Parse project code")]
        public bool Code { get; set; }


        /// <summary>
        ///     Parses users, projects, and project code.
        /// </summary>
        public override void Run()
        {
            FileStore.Init(Path);

            CodeParser parser = new CodeParser();
            if (Users)
            {
                parser.WriteUsers();
            }
            if (Projects)
            {
                parser.WriteProjects();
            }
            if (Code)
            {
                parser.WriteCode();
            }
        }
    }
}
