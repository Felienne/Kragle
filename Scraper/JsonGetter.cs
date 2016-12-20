using System;
using System.IO;
using System.Net;
using System.Threading;


namespace Scraper
{
    public class JsonGetter
    {
        public string Url;


        public static string GetJson(string url)
        {
            WebRequest request = WebRequest.Create(url);

            // Obtain a response from the server, if there was an error, return nothing
            HttpWebResponse response;
            try
            {
                response = request.GetResponse() as HttpWebResponse;
            }
            catch (WebException)
            {
                return null;
            }


            Stream data = response?.GetResponseStream();
            if (data == null)
            {
                return null;
            }

            try
            {
                StreamReader sr = new StreamReader(data);
                return sr.ReadToEnd();
            }

            catch (Exception)
            {
                //probably a time out, so wait a bit
                Thread.Sleep(5000);
                return null;
            }
        }

        public static void WriteStringToFile(string toWrite, string path, bool append = false, bool newline = true)
        {
            using (StreamWriter jsonFile = new StreamWriter(path, append))
            {
                if (newline)
                {
                    jsonFile.WriteLine(toWrite);
                }
                else
                {
                    jsonFile.Write(toWrite);
                }
            }
        }

        public static string GetProjectbyId(string id)
        {
            string projecturl = "https://cdn.projects.scratch.mit.edu/internalapi/project/" + id + "/get/";
            return GetJson(projecturl);
        }
    }
}
