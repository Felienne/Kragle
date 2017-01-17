using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;


namespace Kragle
{
    /// <summary>
    ///     A scraper class "scrapes" the Scratch API by mass-downloading certain pages and storing them. A scraper
    ///     does not analyse or read these pages, it is only responsible for storing them.
    /// </summary>
    internal abstract class Scraper
    {
        private readonly bool _noCache;


        /// <summary>
        ///     Constructs a new Scraper.
        /// </summary>
        /// <param name="noCache">true if requests should be made without using the cache in requests</param>
        protected Scraper(bool noCache)
        {
            _noCache = noCache;
        }


        /// <summary>
        ///     Fetches JSON from the given URL and returns the serialised string.
        /// </summary>
        /// <param name="url">the url to fetch the JSON from</param>
        /// <returns>a deserialised JSON object, or <code>null</code> if the JSON could not be deserialised</returns>
        protected dynamic GetJson(string url)
        {
            string rawJson;

            // Download webpage contents
            using (WebClient client = new WebClient())
            {
                try
                {
                    url += _noCache ? GetRandomSuffix() : "";
                    rawJson = client.DownloadString(url);
                }
                catch (WebException e)
                {
                    Console.WriteLine("Failed to get JSON: " + e.Message);
                    return null;
                }
            }

            // Verify parsability
            try
            {
                return JsonConvert.DeserializeObject(rawJson);
            }
            catch (Exception)
            {
                return null;
            }
        }


        /// <summary>
        ///     Returns a key-value pair as a string with key "random" and a random value.
        /// </summary>
        /// <returns>a key-value pair with a random value</returns>
        private static string GetRandomSuffix()
        {
            return "&random=" + Path.GetRandomFileName().Substring(0, 8);
        }
    }
}
