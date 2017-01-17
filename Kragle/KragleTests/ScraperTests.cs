using Kragle;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace KragleTests
{
    /// <summary>
    ///     Unit tests for the <code>Scraper</code> class.
    /// </summary>
    [TestClass]
    public class ScraperTests : Scraper
    {
        public ScraperTests() : base(false)
        {
        }


        [TestMethod]
        public void GetJsonUnparsableTest()
        {
            const string url = "https://www.google.com/";
            Assert.IsNull(GetJson(url));
        }

        [TestMethod]
        public void GetJsonInvalidUrlTest()
        {
            const string url = "http://this-website-does-not-exist.org/";
            Assert.IsNull(GetJson(url));
        }

        [TestMethod]
        public void GetJsonMalformedUrlTest()
        {
            const string url = "GBnbpjHkef0XdQKOspsI";
            Assert.IsNull(GetJson(url));
        }

        [TestMethod]
        public void AppendRandomParameterStartsWithTest()
        {
            const string url = "http://some-random-url.com/?";
            Assert.IsTrue(AppendRandomParameter(url).StartsWith(url));
        }

        [TestMethod]
        public void AppendRandomParameterNameTest()
        {
            const string url = "https://another-website.net/?";
            Assert.IsTrue(AppendRandomParameter(url).StartsWith(url + "&random="));
        }
    }
}
