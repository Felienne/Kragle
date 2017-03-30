using Kragle;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace KragleTests
{
    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        public void GetNameTest()
        {
            Logger logger = Logger.GetLogger("logger name");
            Assert.AreEqual("logger name", logger.Name);
        }

        [TestMethod]
        public void EqualsTrueTest()
        {
            Logger logger1 = Logger.GetLogger("logger1");
            Logger logger2 = Logger.GetLogger("logger1");
            Assert.AreEqual(logger1, logger2);
        }

        [TestMethod]
        public void EqualsFalseTest()
        {
            Logger logger1 = Logger.GetLogger("logger1");
            Logger logger2 = Logger.GetLogger("logger2");
            Assert.AreNotEqual(logger1, logger2);
        }
    }
}
