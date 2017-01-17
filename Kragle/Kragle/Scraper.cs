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
    public abstract class Scraper
    {
        private readonly bool _noCache;


        /// <summary>
        ///     Constructs a new Scraper.
        /// </summary>
        /// <param name="noCache">true if requests should be made without using the cache in requests</param>
        public Scraper(bool noCache)
        {
            _noCache = noCache;
        }


        /// <summary>
        ///     Fetches JSON from the given URL and returns the serialised string.
        /// </summary>
        /// <param name="url">the valid url to fetch the JSON from</param>
        /// <returns>a deserialised JSON object, or <code>null</code> if the JSON could not be deserialised</returns>
        protected dynamic GetJson(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new ArgumentException("Invalid URL provided");
            }

            // Download webpage contents
            string rawJson;
            using (WebClient client = new WebClient())
            {
                try
                {
                    rawJson = client.DownloadString(AppendRandomParameter(url));
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
        ///     Appends a query parameter to the given URL, with as its key <code>"random"</code>, and an actually
        ///     random value.
        /// </summary>
        /// <param name="url">the URL to append a query parameter to</param>
        /// <returns>the URL with the appendix</returns>
        protected static string AppendRandomParameter(string url)
        {
            if (!url.Contains("?"))
            {
                // URL has no query component yet (see https://tools.ietf.org/html/rfc3986#section-3.4)
                url += "?";
            }

            return url + "random=" + Path.GetRandomFileName().Substring(0, 8) + "&";
        }
    }
}
