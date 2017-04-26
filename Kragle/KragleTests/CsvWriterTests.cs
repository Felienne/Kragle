using System;
using System.IO;
using Kragle;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace KragleTests
{
    [TestClass]
    public class CsvWriterTests
    {
        private readonly string _n = Environment.NewLine;
        private string _filename;


        [TestInitialize]
        public void SetUp()
        {
            _filename = Path.GetRandomFileName();
        }

        [TestCleanup]
        public void TearDown()
        {
            File.Delete(_filename);
        }


        [TestMethod]
        public void WriteStringSimpleTest()
        {
            const string contents = "EBeHBuZOuJ";
            const string expected = "\"EBeHBuZOuJ\"";

            using (CsvWriter writer = new CsvWriter(_filename))
            {
                writer.Write(contents);
            }

            Assert.AreEqual(expected, File.ReadAllText(_filename));
        }

        [TestMethod]
        public void WriteStringTwoArgumentsTest()
        {
            string[] contents = {"u3xG0k1fkc", "HYyCaFclz7"};
            const string expected = "\"u3xG0k1fkc\",\"HYyCaFclz7\"";

            using (CsvWriter writer = new CsvWriter(_filename))
            {
                writer.Write(contents[0]).Write(contents[1]);
            }

            Assert.AreEqual(expected, File.ReadAllText(_filename));
        }

        [TestMethod]
        public void WriteStringAutoNewlineTest()
        {
            string[] contents = {"I7VgDhi8n5", "62lIH91BgD", "pxVAXoWH7n"};
            string expected = "\"I7VgDhi8n5\",\"62lIH91BgD\"" + _n
                              + "\"pxVAXoWH7n\"";

            CsvWriter writer = new CsvWriter(_filename, 2);
            writer.Write(contents[0]).Write(contents[1]).Write(contents[2]);
            writer.Dispose();

            Assert.AreEqual(expected, File.ReadAllText(_filename));
        }

        [TestMethod]
        public void WriteIntegerSimpleTest()
        {
            const int contents = 927;
            const string expected = "927";

            using (CsvWriter writer = new CsvWriter(_filename))
            {
                writer.Write(contents);
            }

            Assert.AreEqual(expected, File.ReadAllText(_filename));
        }

        [TestMethod]
        public void WriteIntegerTwoArgumentsTest()
        {
            int[] contents = {-3917, 1709};
            const string expected = "-3917,1709";

            using (CsvWriter writer = new CsvWriter(_filename))
            {
                writer.Write(contents[0]).Write(contents[1]);
            }

            Assert.AreEqual(expected, File.ReadAllText(_filename));
        }

        [TestMethod]
        public void WriteIntegerAutoNewlineTest()
        {
            int[] contents = {8093, -6965, 7944};
            string expected = "8093,-6965" + _n
                              + "7944";

            using (CsvWriter writer = new CsvWriter(_filename, 2))
            {
                writer.Write(contents[0]).Write(contents[1]).Write(contents[2]);
            }

            Assert.AreEqual(expected, File.ReadAllText(_filename));
        }

        [TestMethod]
        public void WriteMixedAutoNewlineTest()
        {
            string[] contentsS = {"Bw0DZWR20N", "DQW0kwtwOZ"};
            int[] contentsI = {8570, 7060, -3597};
            string expected = "\"Bw0DZWR20N\",8570" + _n
                              + "7060,\"DQW0kwtwOZ\"" + _n
                              + "-3597";

            using (CsvWriter writer = new CsvWriter(_filename, 2))
            {
                writer
                    .Write(contentsS[0])
                    .Write(contentsI[0])
                    .Write(contentsI[1])
                    .Write(contentsS[1])
                    .Write(contentsI[2]);
            }

            Assert.AreEqual(expected, File.ReadAllText(_filename));
        }

        [TestMethod]
        public void NewlineTest()
        {
            using (CsvWriter writer = new CsvWriter(_filename))
            {
                writer.Newline();
            }

            Assert.AreEqual(Environment.NewLine, File.ReadAllText(_filename));
        }

        [TestMethod]
        public void NewlineResetsAutoNewlineTest()
        {
            string[] contents = {"1KQNcXjJXV", "2CgkFpxsBX", "lCe6slMt5H", "ewbJqVYdsW", "dow4NkCYC8", "6HELnIkMIg"};
            string expected = "\"1KQNcXjJXV\",\"2CgkFpxsBX\"" + _n
                              + "\"lCe6slMt5H\"" + _n
                              + "\"ewbJqVYdsW\",\"dow4NkCYC8\"" + _n
                              + "\"6HELnIkMIg\"";

            using (CsvWriter writer = new CsvWriter(_filename, 2))
            {
                writer
                    .Write(contents[0])
                    .Write(contents[1]) // Auto-newline
                    .Write(contents[2])
                    .Newline()
                    .Write(contents[3])
                    .Write(contents[4]) // Auto-newline
                    .Write(contents[5]);
            }

            Assert.AreEqual(expected, File.ReadAllText(_filename));
        }
    }
}
