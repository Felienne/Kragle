using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Scraper
{  
    class listScraper:scraper
    {
       public listScraper(string p): base(p)
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

                    foreach (var item in result)
                    {
                        
                        //get the id
                        int id = item["pk"].Value;
                        string idString = id.ToString();

                        string projecturl = "https://cdn.projects.scratch.mit.edu/internalapi/project/" + idString + "/get/";

                       string JSONpath = _path +"files//" + id + ".sb";
                       string propertiesPath = _path + "properties.sb";

                        //do we already have this id?
                        if (!File.Exists(JSONpath))
                        {
                            string toWrite = JSONGetter.getProjectbyID(idString);

                            string properties = idString;
                            var fields = item["fields"];

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
                    using (System.IO.StreamWriter skippedFile = new System.IO.StreamWriter(skippedPath, true))
                    {
                        skippedFile.WriteLine("Excption in project on page " + i.ToString());
                    }
                }
            }
        }

       private static string makeStringFromJSONArray(string properties, dynamic fields)
       {
          foreach (var v in fields.Values)
          {
             if (v is JsonPrimitive)
             {
                properties += "," + v;  
             }
             else
             {
                //JSONarray
                var rec = makeStringFromJSONArray(properties, v);
                properties = rec;
             }

          }

          return properties;
       }
              
    }
}
