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
    class Return
    {
        public string title;
        public string page;
    }
    class Program
    {
        public static string GetHTML_simple(string url)
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
        


        public static Return GetWebPageandTitle(string url)
        {
            var r = new Return();

            // Create a request to the url
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;

            // If the request wasn't an HTTP request (like a file), ignore it
            if (request == null) return null;

            // Use the user's credentials
            request.UseDefaultCredentials = true;

            // Obtain a response from the server, if there was an error, return nothing
            HttpWebResponse response = null;
            try { response = request.GetResponse() as HttpWebResponse; }
            catch (WebException) { return null; }

            // Regular expression for an HTML title
            string regex = @"(?<=<title.*>)([\s\S]*)(?=</title>)";

            // If the correct HTML header exists for HTML text, continue
            if (new List<string>(response.Headers.AllKeys).Contains("Content-Type"))
                if (response.Headers["Content-Type"].StartsWith("text/html"))
                {
                    // Download the page
                    WebClient web = new WebClient();
                    web.UseDefaultCredentials = true;
                    r.page = web.DownloadString(url);

                    // Extract the title
                    Regex ex = new Regex(regex, RegexOptions.IgnoreCase);
                    r.title= ex.Match(r.page).Value.Trim();

                    return r;
                }

            // Not a valid HTML page
            return null;
        }


        static void Main(string[] args)
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
