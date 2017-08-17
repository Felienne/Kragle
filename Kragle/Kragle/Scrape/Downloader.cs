using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using log4net;
using Newtonsoft.Json.Linq;


namespace Kragle.Scrape
{
    /// <summary>
    ///     The <code>Downloader</code> class is responsible for downloading contents (mainly JSON) from the Internet
    ///     (mainly the Scratch API).
    /// </summary>
    public class Downloader
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Downloader));

        public int MaxDownloadSize = 0;
        public bool Minify = true;
        public bool UseCache = false;


        /// <summary>
        ///     Validates JSON.
        /// </summary>
        /// <param name="json">a JSON string</param>
        /// <returns>true if the given JSON is valid</returns>
        public static bool IsValidJson(string json)
        {
            try
            {
                JToken.Parse(json);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Fetches the contents from the given URL as a string.
        /// </summary>
        /// <param name="url">the valid url to fetch the contents from</param>
        /// <returns>the contents of the webpage, or <code>null</code> if the url could not be accessed</returns>
        public string GetContents(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new ArgumentException("Invalid URL provided");
            }
            url = UseCache ? url : AppendRandomParameter(url);

            // Download webpage contents
            string contents;
            using (WebClient client = new WebClient())
            {
                try
                {
                    contents = client.DownloadString(AppendRandomParameter(url));
                }
                catch (WebException e)
                {
                    Logger.Fatal("Failed to get JSON: " + e.Message, e);
                    return null;
                }
            }

            if (MaxDownloadSize > 0 && contents.Length > MaxDownloadSize)
            {
                // Contents exceed download size
                return null;
            }

            return Minify ? Regex.Replace(contents, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1") : contents;
        }

        /// <summary>
        ///     Fetches JSON from the given URL.
        /// </summary>
        /// <param name="url">the valid url to fetch the JSON from</param>
        /// <returns>a deserialised JSON object, or <code>null</code> if the JSON could not be deserialised</returns>
        public JToken GetJson(string url)
        {
            string rawJson = GetContents(url);

            try
            {
                return JToken.Parse(rawJson);
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
