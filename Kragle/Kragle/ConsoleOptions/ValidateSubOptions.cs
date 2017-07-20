using CommandLine;


namespace Kragle.ConsoleOptions
{
    /// <summary>
    ///     Command-line options for the 'validate' verb.
    /// </summary>
    public class ValidateSubOptions : SubOptions
    {
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
            Logger logger = Logger.GetLogger("ValidateSubOptions");
            
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
            
            logger.Log("");

            if (Users)
            {
                logger.Log("Found " + validator.InvalidUsers + " invalid users.");
            }
            if (Projects)
            {
                logger.Log("Found " + validator.InvalidProjectLists + " invalid project lists.");
            }
            if (Code)
            {
                logger.Log("Found " + validator.InvalidCodeFiles + " invalid code files.");
            }
        }
    }
}
