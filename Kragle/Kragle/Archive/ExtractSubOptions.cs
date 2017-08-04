using CommandLine;


namespace Kragle.Archive
{
    /// <summary>
    ///     Command-line options for the 'extract' verb.
    /// </summary>
    public class ExtractSubOptions : SubOptions
    {
        [Option('f', "force", HelpText = "Overwrite files if they already exist")]
        public bool Overwrite { get; set; }

        [Option('a', "append", HelpText = "Extract only new files if a user is already registered")]
        public bool Append { get; set; }


        /// <summary>
        ///     Extracts backup archives.
        /// </summary>
        public override void Run()
        {
            FileStore.Init(Path);

            new Archiver().Extract(Overwrite, Append);
        }
    }
}
