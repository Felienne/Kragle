using CommandLine;


namespace Kragle.ConsoleOptions
{
    /// <summary>
    ///     Command-line options for all verbs interacting with the Internet.
    /// </summary>
    public abstract class DownloadSubOptions : SubOptions
    {
        [Option('c', "cache", HelpText = "Enable caching; speeds up the process significantly")]
        public bool Cache { get; set; }
    }
}
