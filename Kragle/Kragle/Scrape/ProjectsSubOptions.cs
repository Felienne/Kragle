using CommandLine;


namespace Kragle.Scrape
{
    /// <summary>
    ///     Command-line options for the 'projects' verb.
    /// </summary>
    public class ProjectsSubOptions : DownloadSubOptions
    {
        [Option('u', "update", HelpText = "Update the list of registered projects")]
        public bool Update { get; set; }

        [Option('d', "download", HelpText = "Download project code")]
        public bool Download { get; set; }


        /// <summary>
        ///     Updates and/or downloads project code.
        /// </summary>
        public override void Run()
        {
            FileStore.Init(Path);

            Downloader downloader = new Downloader {UseCache = Cache};
            ProjectScraper scraper = new ProjectScraper(downloader);

            if (Update)
            {
                scraper.UpdateProjectList();
            }
            if (Download)
            {
                scraper.DownloadProjects();
            }
        }
    }
}
