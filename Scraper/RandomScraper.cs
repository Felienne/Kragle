using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;


namespace Scraper
{
    public abstract class Scraper
    {
        public string Path;

        public Scraper(string p)
        {
            Path = p;
            Directory.CreateDirectory(Path);
            Directory.CreateDirectory(Path + "/files/");
            Directory.CreateDirectory(Path + "/properties/");
        }


        public abstract void Scrape();
    }


    public class RandomScraper : Scraper
    {
        public int Max;
        public int Min;

        public RandomScraper(string p, int min, int max) : base(p)
        {
            Min = min;
            Max = max;
        }

        [SuppressMessage("ReSharper", "FunctionNeverReturns")]
        public override void Scrape()
        {
            int numFound = 1;
            int numNotFound = 0;
            int numDouble = 0;
            int numException = 0;

            Random rnd = new Random();

            while (true)
            {
                string skippedPath = Path + "\\skipped.txt";
                int id = rnd.Next(Min, Max);
                string idString = id.ToString();

                try
                {
                    string jsonPath = Path + "\\files\\" + id + ".sb";

                    //do we already have this id?
                    if (!File.Exists(jsonPath))
                    {
                        string toWrite = JsonGetter.GetProjectbyId(idString);

                        if (toWrite != null)
                        {
                            JsonGetter.WriteStringToFile(toWrite, jsonPath);
                            numFound++;
                        }
                        else
                        {
                            numNotFound++;
                            JsonGetter.WriteStringToFile("Not Found, " + idString, skippedPath, true);
                        }
                    }
                    else
                    {
                        numDouble++;
                        JsonGetter.WriteStringToFile("Double, " + idString, skippedPath, true);
                    }

                    Console.WriteLine(numFound + "/" + numDouble + "/" + numNotFound + "/" + numException +
                                      "   (found/double/notfound/exception)    " + idString);
                }
                catch (Exception)
                {
                    numException++;
                    JsonGetter.WriteStringToFile("Exception, " + idString, skippedPath, true);
                }
            }
        }
    }
}
