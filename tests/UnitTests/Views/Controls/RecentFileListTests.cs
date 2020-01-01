using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using OfficeRibbonXEditor.Models;

namespace OfficeRibbonXEditor.UnitTests.Views.Controls
{
    [Apartment(ApartmentState.STA)]
    public sealed class RecentFileListTests
    {
        private string? filePath;

        private RecentFileList? control;

        [SetUp]
        public void SetUp()
        {
            string path;
            do
            {
                path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            } while (File.Exists(path));
            this.filePath = path;

            this.control = new RecentFileList();
            this.control.UseXmlPersister(this.filePath);
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(this.filePath);
        }

        [Test]
        public void CanAddFiles()
        {
            using (var stream = this.TempFile())
            {
                // Arrange
                Assume.That(this.control?.RecentFiles, Is.Empty);

                // Act
                this.control?.InsertFile(stream.Name);

                // Assert
                Assert.Contains(stream.Name, this.control?.RecentFiles);
            }
        }


        [Test]
        public void CanRemoveFiles()
        {
            using (var stream = this.TempFile())
            {
                // Arrange
                Assume.That(this.control?.RecentFiles, Is.Empty);

                // Act
                this.control?.InsertFile(stream.Name);
                this.control?.RemoveFile(stream.Name);

                // Assert
                Assert.IsEmpty(this.control?.RecentFiles);
            }
        }

        [Test]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        public void CannotPassMaxLimit(int maxFiles)
        {
            // Arrange
            this.control!.MaxNumberOfFiles = maxFiles;
            var collection = new List<FileStream>(maxFiles + 5);
            for (var i = 0; i < maxFiles + 5; ++i)
            {
                collection.Add(this.TempFile());
            }

            // Act
            foreach (var stream in collection)
            {
                this.control.InsertFile(stream.Name);
            }

            // Assert
            try
            {
                Assert.AreEqual(maxFiles, this.control.RecentFiles.Count);
            }
            finally
            {
                foreach (var stream in collection)
                {
                    stream.Close();
                }
            }
        }

        private FileStream TempFile()
        {
            return new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
        }
    }
}
