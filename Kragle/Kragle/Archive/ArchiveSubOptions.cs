using CommandLine;


namespace Kragle.Archive
{
    /// <summary>
    ///     Command-line options for the 'archive' verb.
    /// </summary>
    public class ArchiveSubOptions : SubOptions
    {
        [Option('n', "new", HelpText = "Overwrite existing archives with new archives")]
        public bool Overwrite { get; set; }

        
        
        /// <summary>
        ///     Archives all scraped data.
        /// </summary>
        public override void Run()
        {
            FileStore.Init(Path);

            new Archiver().Archive(Overwrite);
        }
    }
}
