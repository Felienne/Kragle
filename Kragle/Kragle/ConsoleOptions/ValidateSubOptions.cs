using CommandLine;


namespace Kragle.ConsoleOptions
{
    /// <summary>
    ///     Command-line options for the 'validate' verb.
    /// </summary>
    public class ValidateSubOptions : SubOptions
    {
        private static readonly Logger Logger = Logger.GetLogger("ValidateSubOptions");
        
        
        [Option('u', "users", HelpText = "Validate user data")]
        public bool Users { get; set; }

        [Option('p', "projects", HelpText = "Validate project data")]
        public bool Projects { get; set; }

        [Option('c', "code", HelpText = "Validate project code")]
        public bool Code { get; set; }


        /// <summary>
        ///     Validates all downloaded JSON files.
        /// </summary>
        public override void Run()
        {
            FileStore.Init(Path);
            
            Validator validator = new Validator();

            if (Users)
            {
                validator.ValidateUsers();
            }
            if (Projects)
            {
                validator.ValidateProjectLists();
            }
            if (Code)
            {
                validator.ValidateCode();
            }
            
            Logger.Log("");

            if (Users)
            {
                Logger.Log("Found " + validator.InvalidUsers + " invalid users.");
            }
            if (Projects)
            {
                Logger.Log("Found " + validator.InvalidProjectLists + " invalid project lists.");
            }
            if (Code)
            {
                Logger.Log("Found " + validator.InvalidCodeFiles + " invalid code files.");
            }
        }
    }
}
