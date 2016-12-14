using System;
using System.IO;


namespace Scraper
{
    public abstract class scraper
    {
        public string _path;

        public scraper(string p)
        {
            _path = p;
            //check to see if the path exists and whether the subfolders /files and /properties do

            makeIfNotExists(_path);

            makeIfNotExists(_path + "\\files\\");
            makeIfNotExists(_path + "\\properties\\");
        }


        public abstract void scrape();

        private void makeIfNotExists(string p)
        {
            bool existsWhole = Directory.Exists(p);

            if (!existsWhole)
            {
                Directory.CreateDirectory(p);
            }
        }
    }


    public class randomScraper : scraper
    {
        public int _max;
        public int _min;

        public randomScraper(string p, int min, int max) : base(p)
        {
            _min = min;
            _max = max;
        }

        public override void scrape()
        {
            int numFound = 1;
            int numNotFound = 0;
            int numDouble = 0;
            int numException = 0;

            Random rnd = new Random();

            while (true)
            {
                string skippedPath = _path + "\\skipped.txt";
                int id = rnd.Next(_min, _max);
                string idString = id.ToString();

                try
                {
                    string JSONpath = _path + "\\files\\" + id + ".sb";

                    //do we already have this id?
                    if (!File.Exists(JSONpath))
                    {
                        string toWrite = JSONGetter.getProjectbyID(idString);

                        if (toWrite != null)
                        {
                            JSONGetter.writeStringToFile(toWrite, JSONpath);
                            numFound++;
                        }
                        else
                        {
                            numNotFound++;
                            JSONGetter.writeStringToFile("Not Found, " + idString, skippedPath, true);
                        }
                    }
                    else
                    {
                        numDouble++;
                        JSONGetter.writeStringToFile("Double, " + idString, skippedPath, true);
                    }

                    Console.WriteLine(numFound + "/" + numDouble + "/" + numNotFound + "/" + numException +
                                      "   (found/double/notfound/exception)    " + idString);
                }
                catch (Exception E)
                {
                    numException++;
                    JSONGetter.writeStringToFile("Exception, " + idString, skippedPath, true);
                }
            }
        }
    }
}
