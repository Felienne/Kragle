using System;
using System.IO;
using Scraper;


namespace Kragle
{
    internal class MainApp
    {
        private static void Main(string[] args)
        {
            const string param1 = "-p"; //args[0];
            const string path = @"C:\Users\Felienne\Dropbox\Code\KragleData\testData\"; //args[1];

            switch (param1)
            {
                case "-p":
                    JsonReader.ProcessJson(path);
                    break;
                case "-s":
                    //split the shared and non-shared files
                    Split(path);
                    break;
                default:
                    JsonPropertiesReader.WriteProperties(path);
                    break;
            }
        }

        private static void Split(string path)
        {
            //merge all local list scraped files:
            const string from = @"C:\ScratchScrapeData\Random_RunLocal\files";

            DirectoryInfo d = new DirectoryInfo(from);

            FileInfo[] files = d.GetFiles();
            int i = 0;

            foreach (FileInfo file in files)
            {
                //get the id:
                string id = Path.GetFileNameWithoutExtension(file.Name);

                string filenameTo = @"C:\ScratchScrapeData\ScrapedFromList\files\" + id + ".sb";

                try
                {
                    File.Move(file.FullName, filenameTo);
                    Console.WriteLine(i*100/files.Length);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                i++;
            }
        }
    }
}
