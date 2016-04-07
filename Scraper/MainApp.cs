using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Squirrel;

namespace Scraper
{
   class MainApp
   {
      static void Main(string[] args)
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
            if (type == "-r")
            {
               s = new randomScraper(@"C:\ScratchScrapeData\mainApp\", 10000000, 100000000);
               s.scrape();
            }


            if (type == "-l")
            {
               s = new listScraper(@"C:\ScratchScrapeData\ListScraper\");
               s.scrape();
            }




         }
      }
   }
}
