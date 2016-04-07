using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Scraper
{
    class ScrapeFromList
    {



        public static string GetHTML(string url)
        {
            WebRequest request = WebRequest.Create(url);

            // Obtain a response from the server, if there was an error, return nothing
            HttpWebResponse response = null;
            try { response = request.GetResponse() as HttpWebResponse; }
            catch (WebException) { return null; }


            Stream data = response.GetResponseStream(); 
            
            StreamReader sr = new StreamReader(data); 
            return sr.ReadToEnd(); 


        }

        public static string getTitlefromHTML(string html)
        {
            string regex = @"(?<=<title.*>)([\s\S]*)(?=</title>)"; 
            System.Text.RegularExpressions.Regex ex = new System.Text.RegularExpressions.Regex(regex, System.Text.RegularExpressions.RegexOptions.IgnoreCase); 
            return ex.Match(html).Value.Trim(); 
        }

        public static bool isShared(string html)
        {
            return !html.Contains("Sorry this project is not shared");
        }
        


        static void Main2(string[] args)
        {
            int found = 0;
            while (true)
            {
                Random rnd = new Random();
                int id = rnd.Next(10000000, 100000000);

                Console.WriteLine("Requesting " + id.ToString());

                string url = "https://scratch.mit.edu/projects/" +id + "/#editor/";

                var html = GetHTML(url);

                if (html != null)
                {
                    var title = getTitlefromHTML(html);
                    if (title != null)
                    {
                        var shared = isShared(html);
                        if (shared)
                        {
                            found++;
                            Console.WriteLine(found.ToString() + " found");
                            using (System.IO.StreamWriter file =
                            new System.IO.StreamWriter(@"C:\Users\Felienne\Dropbox\Code\ScaScra\Scraper\Scraper\urls-active.txt", true))
                            {
                                file.WriteLine(id);
                            }
                        }

                    }
                    else
                    {
                        using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(@"C:\Users\Felienne\Dropbox\Code\ScaScra\Scraper\Scraper\urls-not-active.txt", true))
                        {
                            file.WriteLine(id);
                        }

                    }
                }

                else
                {
                    using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(@"C:\Users\Felienne\Dropbox\Code\ScaScra\Scraper\Scraper\urls-not-active.txt", true))
                    {
                        file.WriteLine(id);
                    }

                }


            }


            


        }
    }
}
