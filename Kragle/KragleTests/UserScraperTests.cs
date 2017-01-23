using System;
using System.Collections.Generic;
using Kragle;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace KragleTests
{
    [TestClass]
    public class UserScraperTests : UserScraper
    {
        public UserScraperTests() : base(false)
        {
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UserScraperNegativePageTest()
        {
            GetRecentProjects(-3, 14);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UserScraperExcessiveSizeTest()
        {
            GetRecentProjects(28, 94);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UserScraperInsufficientSizeTest()
        {
            GetRecentProjects(12, -38);
        }

        [TestMethod]
        public void UserScraperSizeTest()
        {
            ICollection<dynamic> projects = GetRecentProjects(98, 14);

            Assert.AreEqual(14, projects.Count);
        }
    }
}
