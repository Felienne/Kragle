using System;
using CommandLine;
using Kragle.ConsoleOptions;


namespace Kragle
{
    /// <summary>
    ///     Application entry point.
    /// </summary>
    internal class Program
    {
        /// <summary>
        ///     Application entry point.
        /// </summary>
        /// <param name="args">the command-line arguments</param>
        private static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Environment.Exit(1);
            }

            
            Options options = new Options();
            if (!Parser.Default.ParseArguments(args, options, (verb, subOptions) =>
            {
                if (!(subOptions is SubOptions))
                {
                    Environment.Exit(Parser.DefaultExitCodeFail);
                }

                ((SubOptions) subOptions).Run();
            }))
            {
                Environment.Exit(Parser.DefaultExitCodeFail);
            }

            
            Console.WriteLine("Done.");
        }
    }
}
