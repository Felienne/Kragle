using System;


namespace Scraper
{
    internal class MainApp
    {
        private static void Main(string[] args)
        {
            scraper s;


            if (args.Length == 0)
            {
                Console.WriteLine("No arguments given, please provide type with -r for random or -l for list ");
                Console.ReadLine();
            }
            else
            {
                //scraping random currently will just give you the JSON files and not the property files too

                //if you want those, run with -randomprop
                string type = args[0];
                string location = args[1];
                if (type == "-r")
                {
                    s = new randomScraper(location, 10000000, 100000000);
                    s.scrape();
                }


                if (type == "-l")
                {
                    s = new listScraper(location);
                    s.scrape();
                }
            }
        }
    }
}
