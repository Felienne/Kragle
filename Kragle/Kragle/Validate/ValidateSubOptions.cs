using CommandLine;
using log4net;


namespace Kragle.Validate
{
    /// <summary>
    ///     Command-line options for the 'validate' verb.
    /// </summary>
    public class ValidateSubOptions : SubOptions
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ValidateSubOptions));


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

            if (Users)
            {
                Logger.InfoFormat("Found {0} invalid users.", validator.InvalidUsers);
            }
            if (Projects)
            {
                Logger.InfoFormat("Found {0} invalid project lists.", validator.InvalidProjectLists);
            }
            if (Code)
            {
                Logger.InfoFormat("Found {0} invalid code files.", validator.InvalidCodeFiles);
            }
        }
    }
}
