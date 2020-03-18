using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Win32;
using NUnit.Framework;
using OfficeRibbonXEditor.Views.Controls;

namespace OfficeRibbonXEditor.UnitTests.Views.Controls
{
    [Apartment(ApartmentState.STA)]
    public abstract class RecentFileListTests
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. Always defined in SetUp
        protected RecentFileList Control { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        [Test]
        public void CanAddFiles()
        {
            using (var stream = TempFile())
            {
                // Arrange
                Assume.That(this.Control.RecentFiles, Is.Empty);

                // Act
                this.Control.InsertFile(stream.Name);

                // Assert
                Assert.Contains(stream.Name, this.Control?.RecentFiles);
            }
        }

        [Test]
        public void CanRemoveFiles()
        {
            using (var stream = TempFile())
            {
                // Arrange
                Assume.That(this.Control.RecentFiles, Is.Empty);

                // Act
                this.Control.InsertFile(stream.Name);
                this.Control.RemoveFile(stream.Name);

                // Assert
                Assert.IsEmpty(this.Control.RecentFiles);
            }
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        public void CannotPassMaxLimit(int maxFiles)
        {
            // Arrange
            this.Control.MaxNumberOfFiles = maxFiles;
            var collection = new List<FileStream>(maxFiles + 5);
            for (var i = 0; i < maxFiles + 5; ++i)
            {
                collection.Add(TempFile());
            }

            // Act
            foreach (var stream in collection)
            {
                this.Control.InsertFile(stream.Name);
            }

            // Assert
            try
            {
                Assert.AreEqual(maxFiles, this.Control.RecentFiles.Count);
            }
            finally
            {
                foreach (var stream in collection)
                {
                    stream.Close();
                }
            }
        }

        private static FileStream TempFile()
        {
            return new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
        }
    }

    public class RecentFileTestsWithFilePersister : RecentFileListTests
    {
        private string? filePath;

        [SetUp]
        public void SetUp()
        {
            do
            {
                this.filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            } while (File.Exists(this.filePath));

            this.Control = new RecentFileList();
            this.Control.UseXmlPersister(this.filePath);
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(this.filePath);
        }
    }

    public class RecentFileTestsWithRegistryPersister : RecentFileListTests
    {
        private string? registryKey;

        [SetUp]
        public void SetUp()
        {
            do
            {
                this.registryKey = "Software\\" + Path.GetRandomFileName();
            } while (Registry.CurrentUser.OpenSubKey(this.registryKey) != null);

            this.Control = new RecentFileList();
            this.Control.UseRegistryPersister(this.registryKey);
        }

        [TearDown]
        public void TearDown()
        {
            if (Registry.CurrentUser.OpenSubKey(this.registryKey) != null)
            {
                Registry.CurrentUser.DeleteSubKey(this.registryKey);
            }
        }
    }
}
