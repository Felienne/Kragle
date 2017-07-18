namespace Kragle.Archive
{
    /// <summary>
    ///     Command-line options for the 'extract' verb.
    /// </summary>
    public class ExtractSubOptions : SubOptions
    {
        /// <summary>
        ///     Extracts backup archives.
        /// </summary>
        public override void Run()
        {
            FileStore.Init(Path);

            new Archiver().Extract();
        }
    }
}
