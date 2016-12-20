using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Json;


namespace Scraper
{
    internal class ListScraper : Scraper
    {
        public ListScraper(string p) : base(p)
        {
        }

        [SuppressMessage("ReSharper", "FunctionNeverReturns")]
        public override void Scrape()
        {
            int all = 0;
            int i = 1;

            while (true)
            {
                try
                {
                    //get the JSON over the overview page
                    string url = "https://scratch.mit.edu/site-api/explore/more/projects/all/" + i +
                                 "/?date=this_month";

                    //parse JSON all projectds i
                    string json = JsonGetter.GetJson(url);

                    dynamic result = JsonValue.Parse(json);

                    foreach (dynamic item in result)
                    {
                        //get the id
                        int id = item["pk"].Value;
                        string idString = id.ToString();

                        string jsonPath = Path + "files//" + id + ".sb";
                        string propertiesPath = Path + "properties.sb";

                        //do we already have this id?
                        if (File.Exists(jsonPath))
                        {
                            continue;
                        }

                        string toWrite = JsonGetter.GetProjectbyId(idString);

                        string properties = idString;
                        dynamic fields = item["fields"];

                        properties = MakeStringFromJsonArray(properties, fields);

                        if (toWrite != null)
                        {
                            JsonGetter.WriteStringToFile(toWrite, jsonPath);
                            JsonGetter.WriteStringToFile(properties, propertiesPath, true);
                        }

                        all++;
                        Console.WriteLine(all + ": " + idString);
                    }

                    Console.WriteLine("Page " + i + " done");

                    i++;
                }
                catch (Exception)
                {
                    string skippedPath = Path + "skipped.txt";
                    Console.WriteLine("exception!");
                    using (StreamWriter skippedFile = new StreamWriter(skippedPath, true))
                    {
                        skippedFile.WriteLine("Excption in project on page " + i);
                    }
                }
            }
        }

        private static string MakeStringFromJsonArray(string properties, dynamic fields)
        {
            foreach (dynamic v in fields.Values)
            {
                if (v is JsonPrimitive)
                {
                    properties += "," + v;
                }
                else
                {
                    //JSONarray
                    dynamic rec = MakeStringFromJsonArray(properties, v);
                    properties = rec;
                }
            }

            return properties;
        }
    }
}
