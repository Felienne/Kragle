using System;
using Kragle.Properties;


namespace Kragle.Explore
{
    /// <summary>
    ///     Command-line options for the 'reset' verb.
    /// </summary>
    public class ResetSubOptions : SubOptions
    {
        /// <summary>
        ///     Removes all downloaded and parsed data.
        /// </summary>
        public override void Run()
        {
            FileStore.Init(Path);

            Console.Write(Resources.ResetRequestConfirmation);
            string confirm = Console.ReadLine();

            if (confirm != null && confirm.ToLower() == "y")
            {
                FileStore.RemoveDirectory(Resources.UserDirectory);
                FileStore.RemoveDirectory(Resources.ProjectDirectory);
                FileStore.RemoveDirectory(Resources.CodeDirectory);
                Console.WriteLine(Resources.ResetConfirmSuccess);
            }
            else
            {
                Console.WriteLine(Resources.ResetConfirmCancel);
            }

            Environment.Exit(0); // Suppress exit message
        }
    }
}
