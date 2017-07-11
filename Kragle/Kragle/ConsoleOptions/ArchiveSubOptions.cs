namespace Kragle.ConsoleOptions
{
    /// <summary>
    ///     Command-line options for the 'archive' verb.
    /// </summary>
    public class ArchiveSubOptions : SubOptions
    {
        /// <summary>
        ///     Archives all scraped data.
        /// </summary>
        public override void Run()
        {
            FileStore.Init(Path);

            new Archiver().Archive();
        }
    }
}
