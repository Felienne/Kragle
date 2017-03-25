﻿using Kragle;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace KragleTests
{
    [TestClass]
    public class ProjectScraperTests : ProjectScraper
    {
        public ProjectScraperTests() : base(new FileStore(), new Downloader(false))
        {
        }


        [TestMethod]
        public void GetUserProjectsInvalidUsernameTest()
        {
            // Names must be at least three characters long
            Assert.IsNull(GetUserProjects("1"));
        }

        [TestMethod]
        public void GetUserProjectsValidTest()
        {
            Assert.AreEqual(0, GetUserProjects("kragle_user").Count);
        }

        [TestMethod]
        public void GetProjectCodeNegativeIdTest()
        {
            Assert.IsNull(GetProjectCode(-1));
        }

        [TestMethod]
        public void GetProjectCodeUnusedIdTest()
        {
            Assert.IsNull(GetProjectCode(14));
        }
    }
}