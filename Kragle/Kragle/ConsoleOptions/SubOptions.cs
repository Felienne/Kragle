using CommandLine;


namespace Kragle.ConsoleOptions
{
    /// <summary>
    ///     Command-line options for all verbs interacting with the file system.
    /// </summary>
    public abstract class SubOptions
    {
        [Option('p', "path", HelpText = "The path files should be read from and written to")]
        public string Path { get; set; }

        [Option('c', "cache", HelpText = "Enable caching; speeds up the process significantly")]
        public bool Cache { get; set; }


        /// <summary>
        ///     Executes the action corresponding to this sub-option.
        /// </summary>
        public abstract void Run();
    }
}
