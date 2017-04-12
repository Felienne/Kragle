using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;


namespace Kragle
{
    /// <summary>
    ///     The <code>Downloader</code> class is responsible for downloading contents (mainly JSON) from the Internet
    ///     (mainly the Scratch API).
    /// </summary>
    public class Downloader
    {
        private static readonly Logger Logger = Logger.GetLogger("Downloader");

        private readonly int _maxDownloadSize;
        private readonly bool _noCache;


        /// <summary>
        ///     Constructs a new <code>Downloader</code>.
        /// </summary>
        /// <param name="noCache">true if requests should be made without using the cache in requests</param>
        /// <param name="maxDownloadSize">
        ///     the maximum size of any data to be downloaded. If this value is exceeded, download methods will return
        ///     <code>null</code>
        /// </param>
        public Downloader(bool noCache, int maxDownloadSize = 0)
        {
            _noCache = noCache;
            _maxDownloadSize = maxDownloadSize;
        }


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
        /// <param name="minify">true if the returned string should be stripped of unnecessary spaces</param>
        /// <returns>the contents of the webpage, or <code>null</code> if the url could not be accessed</returns>
        public string GetContents(string url, bool minify = false)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new ArgumentException("Invalid URL provided");
            }
            url = _noCache ? AppendRandomParameter(url) : url;

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
                    Logger.Log("Failed to get JSON: " + e.Message);
                    return null;
                }
            }

            if (_maxDownloadSize > 0 && contents.Length > _maxDownloadSize)
            {
                // Contents exceed download size
                return null;
            }
            if (minify)
            {
                // Strip unnecessary whitespace
                Regex.Replace(contents, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");
            }
            return contents;
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
