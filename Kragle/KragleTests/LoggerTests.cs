using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Kragle;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace KragleTests
{
    [TestClass]
    public class LoggerTests
    {
        private string _filename;


        [TestInitialize]
        public void SetUp()
        {
            _filename = Path.GetRandomFileName();
            File.WriteAllText(_filename, "");
        }

        [TestCleanup]
        public void TearDown()
        {
            File.Delete(_filename);
        }


        // Constructor
        [TestMethod]
        [SuppressMessage("ReSharper", "UnusedVariable")]
        public void ConstructorEmptyFileTest()
        {
            // Test that calling constructor creates empty file
            File.Delete(_filename);

            using (Logger logger = new Logger(_filename))
            {
            }

            Assert.AreEqual("", File.ReadAllText(_filename));
        }

        [TestMethod]
        public void ConstructorConsoleDisabledTest()
        {
            using (StringWriter writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (Logger logger = new Logger(_filename) {Console = false})
                {
                    logger.Log("vjeGzEZvNs");
                }

                Assert.AreEqual("", writer.ToString());
            }
        }

        [TestMethod]
        public void ConstructorConsoleEnabledTest()
        {
            using (StringWriter writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (Logger logger = new Logger(_filename))
                {
                    logger.Log("bvGS3rh7Wz");
                }

                Assert.AreEqual("bvGS3rh7Wz", writer.ToString());
            }
        }


        // Set/Get Console
        [TestMethod]
        public void SetGetConsoleTest()
        {
            using (Logger logger = new Logger())
            {
                logger.Console = true;
                Assert.AreEqual(true, logger.Console);

                logger.Console = false;
                Assert.AreEqual(false, logger.Console);
            }
        }

        [TestMethod]
        public void SetConsoleFalseTest()
        {
            using (StringWriter writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (Logger logger = new Logger(_filename))
                {
                    logger.Console = true;
                    logger.Log("rD1Va0QDrD");

                    logger.Console = false;
                    logger.Log("l1QMidiEh1");
                }

                Assert.AreEqual("rD1Va0QDrD", writer.ToString());
            }
        }

        [TestMethod]
        public void SetConsoleTrueTest()
        {
            using (StringWriter writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (Logger logger = new Logger(_filename))
                {
                    logger.Console = false;
                    logger.Log("BZuzNRpslq");

                    logger.Console = true;
                    logger.Log("3QJPTPSOLn");
                }

                Assert.AreEqual("3QJPTPSOLn", writer.ToString());
            }
        }


        // Set/Get AutoFlush
        [TestMethod]
        public void SetGetAutoFlushTest()
        {
            using (Logger logger = new Logger())
            {
                logger.AutoFlush = true;
                Assert.AreEqual(true, logger.AutoFlush);

                logger.AutoFlush = false;
                Assert.AreEqual(false, logger.AutoFlush);
            }
        }

        [TestMethod]
        public void AutoFlushFileTrueTest()
        {
            using (FileStream fileIn = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fileIn))
            using (Logger logger = new Logger(_filename) {AutoFlush = true})
            {
                logger.Log("vH2X3NOV8K");
                Assert.AreEqual("vH2X3NOV8K", reader.ReadToEnd());
            }
        }

        [TestMethod]
        public void AutoFlushFileFalseTest()
        {
            using (FileStream fileIn = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fileIn))
            using (Logger logger = new Logger(_filename) {AutoFlush = false})
            {
                logger.Log("67RkBj2AQE");
                Assert.AreEqual("", reader.ReadToEnd());

                logger.AutoFlush = true;
                Assert.AreEqual("67RkBj2AQE", reader.ReadToEnd());
            }
        }


        // Log
        [TestMethod]
        public void LogTest()
        {
            using (Logger logger = new Logger(_filename))
            {
                logger.Log("4HSrOMkg4X");
            }

            Assert.AreEqual("4HSrOMkg4X", File.ReadAllText(_filename));
        }

        [TestMethod]
        public void LogMultipleTest()
        {
            using (Logger logger = new Logger(_filename))
            {
                logger.Log("6cLJHbijSO");
                logger.Log("4HSrOMkg4X");
            }

            Assert.AreEqual("6cLJHbijSO4HSrOMkg4X", File.ReadAllText(_filename));
        }

        [TestMethod]
        public void LogChainTest()
        {
            using (Logger logger = new Logger(_filename))
            {
                logger.Log("NhD7A7VpJF").Log("bVhfBW15Ku");
            }

            Assert.AreEqual("NhD7A7VpJFbVhfBW15Ku", File.ReadAllText(_filename));
        }


        // LogLine
        [TestMethod]
        public void LogLineTest()
        {
            using (Logger logger = new Logger(_filename))
            {
                logger.LogLine("9453YjrEvB");
            }

            Assert.AreEqual("9453YjrEvB" + Environment.NewLine, File.ReadAllText(_filename));
        }

        [TestMethod]
        public void LogLineMultipleTest()
        {
            using (Logger logger = new Logger(_filename))
            {
                logger.LogLine("UaWNYl8Bnl");
                logger.LogLine("eBHy0RksHS");
            }

            Assert.AreEqual(
                "UaWNYl8Bnl" + Environment.NewLine + "eBHy0RksHS" + Environment.NewLine,
                File.ReadAllText(_filename)
            );
        }

        [TestMethod]
        public void LogLineChainTest()
        {
            using (Logger logger = new Logger(_filename))
            {
                logger.LogLine("Zser78psU5").LogLine("Xlv0ZkP8d1");
            }

            Assert.AreEqual(
                "Zser78psU5" + Environment.NewLine + "Xlv0ZkP8d1" + Environment.NewLine,
                File.ReadAllText(_filename)
            );
        }


        // Flush
        [TestMethod]
        public void FlushConsoleTest()
        {
            using (FileStream fileIn = new FileStream(_filename, FileMode.Append, FileAccess.Write, FileShare.Read))
            using (FileStream fileOut = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamWriter writer = new StreamWriter(fileIn) {AutoFlush = false}) // "Console" does not flush
            using (StreamReader reader = new StreamReader(fileOut))
            using (Logger logger = new Logger {AutoFlush = false}) // Logger does not flush either
            {
                Console.SetOut(writer);

                logger.Log("eb7nplzYBG");
                Assert.AreEqual("", reader.ReadToEnd());

                logger.Log("EICUpWeTDt").Flush();
                Assert.AreEqual("eb7nplzYBGEICUpWeTDt", reader.ReadToEnd());
            }
        }

        [TestMethod]
        public void FlushFileTest()
        {
            using (FileStream fileIn = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fileIn))
            using (Logger logger = new Logger(_filename) {AutoFlush = false})
            {
                logger.Log("vH2X3NOV8K");
                Assert.AreEqual("", reader.ReadToEnd());

                logger.Log("N1BpV8sTHJ").Flush();
                Assert.AreEqual("vH2X3NOV8KN1BpV8sTHJ", reader.ReadToEnd());
            }
        }
    }
}
