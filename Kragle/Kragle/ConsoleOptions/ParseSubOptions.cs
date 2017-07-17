namespace Kragle.ConsoleOptions
{
    /// <summary>
    ///     Command-line options for the 'parse' verb.
    /// </summary>
    public class ParseSubOptions : SubOptions
    {
        /// <summary>
        ///     Parses users, projects, and project code.
        /// </summary>
        public override void Run()
        {
            FileStore.Init(Path);

            CodeParser parser = new CodeParser();
            parser.WriteUsers();
            parser.WriteProjects();
            parser.WriteCode();
        }
    }
}
