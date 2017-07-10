using CommandLine;


namespace Kragle.ConsoleOptions
{
    /// <summary>
    ///     Command-line options for the 'users' verb.
    /// </summary>
    public class UsersSubOptions : SubOptions
    {
        [Option('n', "number", DefaultValue = int.MaxValue, HelpText = "The number of users to scrape")]
        public int Count { get; set; }

        [Option('m', "meta", HelpText = "Download user meta-data")]
        public bool Meta { get; set; }


        /// <summary>
        ///     Scrapes users.
        /// </summary>
        public override void Run()
        {
            FileStore.Init(Path);
            Downloader downloader = new Downloader {UseCache = Cache};
            UserScraper scraper = new UserScraper(downloader, Count);

            scraper.ScrapeUsers();
            if (Meta)
            {
                scraper.DownloadMetaData();
            }
        }
    }
}
