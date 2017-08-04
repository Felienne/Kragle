using CommandLine;


namespace Kragle.Archive
{
    /// <summary>
    ///     Command-line options for the 'extract' verb.
    /// </summary>
    public class ExtractSubOptions : SubOptions
    {
        [Option('n', "new", HelpText = "Overwrite files if they already exist")]
        public bool Overwrite { get; set; }
        
        
        /// <summary>
        ///     Extracts backup archives.
        /// </summary>
        public override void Run()
        {
            FileStore.Init(Path);

            new Archiver().Extract(Overwrite);
        }
    }
}
