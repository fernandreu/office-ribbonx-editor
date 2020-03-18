using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.ViewModels.Samples;

namespace OfficeRibbonXEditor.IntegrationTests.Helpers
{
    public class SampleTests
    {
        [Test]
        public void IgnoresInvalidPaths()
        {
            Assert.DoesNotThrow(() => SampleUtils.LoadXmlSamples(new []{ "A", "B", "C" }, false));
        }

        [Test]
        public static void CanScanAssemblySamples()
        {
            // Act
            var result = SampleUtils.LoadXmlSamples(Enumerable.Empty<string>(), true);
            
            // Assert
            ScanResult(result, x => Assert.False(x is FileSampleViewModel));
            Assert.IsNotEmpty(result.Items);
        }

        [Test]
        public static void CanScanSingleFiles()
        {
            // Arrange
            string path, name;
            do
            {
                name = Path.GetRandomFileName();
                path = Path.Combine(Path.GetTempPath(), name + ".xml");
            } while (Directory.Exists(path));

            File.WriteAllText(path, null);

            // Act
            var result = SampleUtils.LoadXmlSamples(new[] {path}, false);

            // Assert
            try
            {
                Assert.AreEqual(1, result.Items.Count);
                Assert.AreEqual(name, result.Items[0].Name);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        [TestCase(3, 2, 2)]
        public static void CanScanFolder(int numFiles, int numFolders, int nestedLevels)
        {
            // Arrange: create a random folder / xml file structure
            string root;
            do
            {
                root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            } while (Directory.Exists(root));

            Directory.CreateDirectory(root);
            var expected = PopulateRandomly(root, numFiles, numFolders, nestedLevels);

            // Act
            var result = SampleUtils.LoadXmlSamples(new[] {root}, false);

            // Assert
            try
            {
                AssertConsistency(expected, (SampleFolderViewModel) result.Items[0]);
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private static SampleFolderViewModel PopulateRandomly(string path, int numFiles, int numFolders, int nestedLevels)
        {
            var result = new SampleFolderViewModel
            {
                Name = Path.GetFileName(path)
            };

            for (var i = 0; i < numFiles; ++i)
            {
                var name = Path.GetRandomFileName();
                var nestedPath = Path.Combine(path, name + ".xml");
                File.WriteAllText(nestedPath, "N/A");
                result.Items.Add(new FileSampleViewModel(nestedPath));
            }

            if (nestedLevels <= 0)
            {
                return result;
            }

            for (var i = 0; i < numFolders; ++i)
            {
                var name = Path.GetRandomFileName();
                var nestedPath = Path.Combine(path, name);
                Directory.CreateDirectory(nestedPath);
                result.Items.Add(PopulateRandomly(nestedPath, numFiles, numFolders, nestedLevels - 1));
            }

            return result;
        }

        private static void ScanResult(SampleFolderViewModel result, Action<ISampleMenuItem> action)
        {
            action(result);

            foreach (var item in result.Items)
            {
                if (item is SampleFolderViewModel folder)
                {
                    ScanResult(folder, action);
                }
                else
                {
                    action(item);
                }
            }
        }

        private static void AssertConsistency(SampleFolderViewModel expected, SampleFolderViewModel actual, string path = null)
        {
            path = $"{path}/{expected.Name}";

            // For simplicity, it is not checked whether actual contains more items than expected
            foreach (var item in expected.Items)
            {
                if (item is SampleFolderViewModel folder)
                {
                    var other = actual.Items.OfType<SampleFolderViewModel>().FirstOrDefault(x => x.Name == item.Name);
                    Assert.NotNull(other, $"Could not find folder {path}/{item.Name}");
                    AssertConsistency(folder, other, path);
                }
                else
                {
                    var other = actual.Items.OfType<FileSampleViewModel>().FirstOrDefault(x => x.Name == item.Name);
                    Assert.NotNull(other, $"Could not find file {path}/{item.Name}");
                }
            }
        }
    }
}
