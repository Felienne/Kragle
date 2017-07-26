using System;
using Kragle.Scrape;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace KragleTests
{
    [TestClass]
    public class UserScraperTests : UserScraper
    {
        public UserScraperTests() : base(new Downloader(), int.MaxValue)
        {
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UserScraperRecentProjectsNegativePageTest()
        {
            GetRecentProjects(-3, 14);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UserScraperRecentProjectsExcessiveSizeTest()
        {
            GetRecentProjects(28, 94);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UserScraperRecentProjectsInsufficientSizeTest()
        {
            GetRecentProjects(12, -38);
        }

        [TestMethod]
        public void UserScraperRecentProjectsSizeTest()
        {
            JArray projects = GetRecentProjects(98, 14);

            Assert.AreEqual(14, projects.Count);
        }


        [TestMethod]
        public void UserScraperMetadataInvalidNameTest()
        {
            // Names must be at least three characters long
            Assert.IsNull(GetMetadata("1"));
        }

        [TestMethod]
        public void UserScraperMetadataTest()
        {
            dynamic metadata = JsonConvert.DeserializeObject(GetMetadata("kragle_user"));

            Assert.AreEqual("kragle_user", metadata.username.ToString());
        }
    }
}
