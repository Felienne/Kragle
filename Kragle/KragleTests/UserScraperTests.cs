using System;
using System.Collections.Generic;
using Kragle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;


namespace KragleTests
{
    [TestClass]
    public class UserScraperTests : UserScraper
    {
        public UserScraperTests() : base(new FileStore(), new Downloader(true))
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
            ICollection<dynamic> projects = GetRecentProjects(98, 14);

            Assert.AreEqual(14, projects.Count);
        }


        [TestMethod]
        public void UserScraperMetaDataInvalidNameTest()
        {
            // Names must be at least three characters long
            Assert.IsNull(GetMetaData("1"));
        }

        [TestMethod]
        public void UserScraperMetaDataTest()
        {
            dynamic metaData = JsonConvert.DeserializeObject(GetMetaData("kragle_user"));

            Assert.AreEqual("kragle_user", metaData.username.ToString());
        }
    }
}
