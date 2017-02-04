﻿using System;
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
        public void GetContentsInvalidUrlTest()
        {
            const string url = "http://invalid-url.net/";

            Assert.IsNull(GetContents(url));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetContentsMalformedUrlTest()
        {
            const string url = "qABkq6C0oi";

            GetContents(url);
        }

        [TestMethod]
        public void GetContentsValidTest()
        {
            const string url = "https://jsonplaceholder.typicode.com/posts";

            Assert.IsNotNull(GetContents(url));
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
        [ExpectedException(typeof(ArgumentException))]
        public void GetJsonMalformedUrlTest()
        {
            const string url = "GBnbpjHkef0XdQKOspsI";

            GetJson(url); // Throws exception
        }

        [TestMethod]
        public void GetJsonValidTest()
        {
            const string url = "https://jsonplaceholder.typicode.com/posts/";

            Assert.IsNotNull(GetJson(url));
        }


        [TestMethod]
        public void AppendRandomParameterStartsWithTest()
        {
            const string url = "http://some-random-url.com/";

            Assert.IsTrue(AppendRandomParameter(url).StartsWith(url));
        }

        [TestMethod]
        public void AppendRandomParameterNameWithoutQueryComponentTest()
        {
            const string url = "https://another-website.net/";

            Assert.IsTrue(AppendRandomParameter(url).StartsWith(url + "?random="));
        }

        [TestMethod]
        public void AppendRandomParameterNameWithQueryComponentTest()
        {
            const string url = "https://another-website.net/?";

            Console.WriteLine(AppendRandomParameter(url));
            Assert.IsTrue(AppendRandomParameter(url).StartsWith(url + "random="));
        }
    }
}
