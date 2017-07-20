using System;
using System.Diagnostics;


namespace Kragle.Explore
{
    /// <summary>
    ///     Command-line options for the 'open' verb.
    /// </summary>
    public class OpenSubOptions : SubOptions
    {
        /// <summary>
        ///     Opens the output directory in Windows Explorer.
        /// </summary>
        public override void Run()
        {
            FileStore.Init(Path);
            Process.Start(FileStore.GetRootPath());

            Environment.Exit(0); // Suppress exit message
        }
    }
}
