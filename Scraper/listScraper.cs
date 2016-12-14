using System;
using System.IO;
using System.Json;
using System.Net;


namespace Scraper
{
    internal class listScraper : scraper
    {
        public listScraper(string p) : base(p)
        {
        }

        public override void scrape()
        {
            int all = 0;
            int i = 1;
            while (true)
            {
                try
                {
                    //get the JSON over the overview page
                    string url = "https://scratch.mit.edu/site-api/explore/more/projects/all/" + i + "/?date=this_month";

                    //parse JSON all project ids
                    string JSON = JSONGetter.GetJSON(url);

                    WebClient webClient = new WebClient();
                    dynamic result = JsonValue.Parse(JSON);
                    int numSkips = 0;

                    foreach (dynamic item in result)
                    {
                        //get the id
                        int id = item["pk"].Value;
                        string idString = id.ToString();

                        string projecturl = "https://cdn.projects.scratch.mit.edu/internalapi/project/" + idString +
                                            "/get/";

                        string JSONpath = _path + "files//" + id + ".sb";
                        string propertiesPath = _path + "properties.sb";

                        //do we already have this id?
                        if (!File.Exists(JSONpath))
                        {
                            string toWrite = JSONGetter.getProjectbyID(idString);

                            string properties = idString;
                            dynamic fields = item["fields"];

                            properties = makeStringFromJSONArray(properties, fields);

                            if (toWrite != null)
                            {
                                JSONGetter.writeStringToFile(toWrite, JSONpath);
                                JSONGetter.writeStringToFile(properties, propertiesPath, true);
                            }

                            all++;
                            Console.WriteLine(all + ": " + idString);
                        }
                        else
                        {
                            numSkips++;
                        }
                    }

                    Console.WriteLine("Page " + i + " done");

                    i++;
                }
                catch (Exception E)
                {
                    string skippedPath = _path + "skipped.txt";
                    Console.WriteLine("exception!");
                    using (StreamWriter skippedFile = new StreamWriter(skippedPath, true))
                    {
                        skippedFile.WriteLine("Excption in project on page " + i);
                    }
                }
            }
        }

        private static string makeStringFromJSONArray(string properties, dynamic fields)
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
                    dynamic rec = makeStringFromJSONArray(properties, v);
                    properties = rec;
                }
            }

            return properties;
        }
    }
}
