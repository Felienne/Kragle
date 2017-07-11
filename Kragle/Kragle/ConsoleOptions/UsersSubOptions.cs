using CommandLine;


namespace Kragle.ConsoleOptions
{
    /// <summary>
    ///     Command-line options for the 'users' verb.
    /// </summary>
    public class UsersSubOptions : SubOptions
    {
        [Option('n', "number", HelpText = "The number of users to scrape", DefaultValue = int.MaxValue)]
        public int Count { get; set; }

        [Option('m', "meta", HelpText = "Download user meta-data")]
        public bool Meta { get; set; }
        
        [Option('p', "page", HelpText = "The page to start scraping at", DefaultValue = 0)]
        public int Page { get; set; }


        /// <summary>
        ///     Scrapes users.
        /// </summary>
        public override void Run()
        {
            FileStore.Init(Path);
            Downloader downloader = new Downloader {UseCache = Cache};
            UserScraper scraper = new UserScraper(downloader, Count);

            scraper.ScrapeUsers(Page);
            if (Meta)
            {
                scraper.DownloadMetaData();
            }
        }
    }
}
