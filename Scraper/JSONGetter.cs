using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scraper
{
    public class JSONGetter
    {
        public string url;



        public static string GetJSON(string url)
        {

            WebRequest request = WebRequest.Create(url);

            // Obtain a response from the server, if there was an error, return nothing
            HttpWebResponse response = null;
            try { response = request.GetResponse() as HttpWebResponse; }
            catch (WebException) { return null; }


            Stream data = response.GetResponseStream();

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

        public static void writeStringToFile(string toWrite, string path, bool append=false, bool newline=true)
        {
            using (System.IO.StreamWriter JSONfile =
            new System.IO.StreamWriter(path, append))
            {
               if (newline)
               {
                  JSONfile.WriteLine(toWrite);
               }
               else 
               {
                  JSONfile.Write(toWrite);
               }

            }

        }

        public static string getProjectbyID(string id)
        {
            string projecturl = "https://cdn.projects.scratch.mit.edu/internalapi/project/" + id + "/get/";
            return GetJSON(projecturl);

        }

    }
}
