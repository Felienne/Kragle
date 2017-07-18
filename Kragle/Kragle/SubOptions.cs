using CommandLine;


namespace Kragle
{
    /// <summary>
    ///     Command-line options for all verbs interacting with the file system.
    /// </summary>
    public abstract class SubOptions
    {
        [Option('o', "output", HelpText = "The path files should be read from and written to")]
        public string Path { get; set; }


        /// <summary>
        ///     Executes the action corresponding to this sub-option.
        /// </summary>
        public abstract void Run();
    }
}
