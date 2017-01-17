using System.IO;
using Kragle;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace KragleTests
{
    [TestClass]
    public class FileStoreTests : FileStore
    {
        private string _directory;
        private string _fileContents;
        private string _fileName;


        public FileStoreTests() : base("./")
        {
        }


        [TestInitialize]
        public void SetUp()
        {
            _directory = Path.GetRandomFileName().Substring(0, 8);
            _fileName = Path.GetRandomFileName();
            _fileContents = Path.GetRandomFileName();

            RemoveFile(_fileName);
        }


        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ReadFileNotExistsTest()
        {
            ReadFile("does-not-exist.txt");
        }

        [TestMethod]
        public void WriteFileTest()
        {
            WriteFile(_fileName, _fileContents);

            Assert.IsTrue(FileExists(_fileName));
        }

        [TestMethod]
        public void ReadFileTest()
        {
            WriteFile(_fileName, _fileContents);

            Assert.AreEqual(_fileContents, ReadFile(_fileName));
        }

        [TestMethod]
        public void WriteFileAppendTest()
        {
            WriteFile(_fileName, _fileContents, true);
            WriteFile(_fileName, _fileContents, true);

            Assert.AreEqual(_fileContents + _fileContents, ReadFile(_fileName));
        }

        [TestMethod]
        public void ReadWriteFileSubDirectoryTest()
        {
            WriteFile(_directory, _fileName, _fileContents);

            Assert.AreEqual(_fileContents, ReadFile(_directory, _fileName));
        }

        [TestMethod]
        public void WriteFileSubDirectoryExistsTest()
        {
            WriteFile(_directory, _fileName, _fileContents);

            Assert.IsTrue(FileExists(_directory, _fileName));
        }

        [TestMethod]
        public void FileExistsFalseTest()
        {
            Assert.IsFalse(FileExists(_fileName));
        }

        [TestMethod]
        public void FileExistsInDirectoryFalseTest()
        {
            Assert.IsFalse(FileExists(_directory, _fileName));
        }

        [TestMethod]
        public void FileExistsTrueTest()
        {
            WriteFile(_fileName, _fileContents);

            Assert.IsTrue(FileExists(_fileName));
        }

        [TestMethod]
        public void FileExistsInDirectoryTrue()
        {
            WriteFile(_directory, _fileName, _fileContents);

            Assert.IsTrue(FileExists(_directory, _fileName));
        }

        [TestMethod]
        public void RemoveFileTest()
        {
            WriteFile(_fileName, _fileContents);

            RemoveFile(_fileName);

            Assert.IsFalse(FileExists(_fileName));
        }

        [TestMethod]
        public void RemoveFileNotFoundTest()
        {
            RemoveFile(_fileName);

            Assert.IsFalse(FileExists(_fileName));
        }
    }
}
