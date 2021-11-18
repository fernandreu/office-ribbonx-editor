using System.IO;
using NUnit.Framework;
using OfficeRibbonXEditor.Documents;
using OfficeRibbonXEditor.ViewModels.Documents;

namespace OfficeRibbonXEditor.FunctionalTests.Documents
{
    [TestFixture]
    public class OfficeDocumentViewModelTests
    {
        private readonly string _sourceFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/Blank.xlsx");

        private readonly string _destFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Output/BlankSaved.xlsx");

        private readonly string _undoIcon = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/undo.png");

        private readonly string _redoIcon = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/redo.png");

        [SetUp]
        public void SetUp()
        {
            var directory = Path.GetDirectoryName(_sourceFile);
            if (directory == null)
            {
                Assert.Fail("Wrong _sourceFile path");
                return; // Not needed, but suppreses nullable warnings below
            }

            Directory.CreateDirectory(directory);

            if (File.Exists(_destFile))
            {
                File.Delete(_destFile);
            }
        }

        [Test]
        public void PartShouldBeInserted()
        {
            // Arrange
            using (var doc = new OfficeDocument(_sourceFile))
            using (var viewModel = new OfficeDocumentViewModel(doc))
            {
                // Act
                viewModel.InsertPart(XmlPart.RibbonX12);

                // Assert
                Assert.AreEqual(1, viewModel.Children.Count);
            }
        }

        [Test]
        public void DocumentShouldBeSaved()
        {
            // Arrange
            using (var doc = new OfficeDocument(_sourceFile))
            using (var viewModel = new OfficeDocumentViewModel(doc))
            {
                viewModel.InsertPart(XmlPart.RibbonX12);
                Assume.That(File.Exists(_destFile), Is.False, "File was not deleted before test");

                // Act
                doc.Save(_destFile);

                // Assert
                Assert.IsTrue(File.Exists(_destFile), "File was not saved");
            }
        }
        
        /// <summary>
        /// The reload before saving can be tricky, especially if icons are involved
        /// </summary>
        [Test]
        public void ReloadOnSaveTest()
        {
            // Arrange
            void CheckIntegrity(OfficeDocumentViewModel innerModel)
            {
                Assert.AreEqual(2, innerModel.Children.Count);

                for (var i = 0; i < 2; ++i)
                {
                    var innerPart = (OfficePartViewModel)innerModel.Children[i];

                    if (innerPart.Part?.PartType == XmlPart.RibbonX12)
                    {
                        Assert.AreEqual(1, innerPart.Children.Count);
                        Assert.AreEqual("redo", ((IconViewModel)innerPart.Children[0]).Name);
                    }
                    else
                    {
                        Assert.AreEqual(2, innerPart.Children.Count);
                        Assert.AreEqual("changedId", ((IconViewModel)innerPart.Children[0]).Name);
                        Assert.AreEqual("redo", ((IconViewModel)innerPart.Children[1]).Name);
                    }
                }
            }

            using (var doc = new OfficeDocument(_sourceFile))
            using (var viewModel = new OfficeDocumentViewModel(doc))
            {
                viewModel.InsertPart(XmlPart.RibbonX12);
                viewModel.InsertPart(XmlPart.RibbonX14);

                var part = (OfficePartViewModel) viewModel.Children[0];
                part.InsertIcon(_undoIcon);
                part.InsertIcon(_redoIcon);
                part.RemoveIcon("undo");
                part = (OfficePartViewModel) viewModel.Children[1];
                part.InsertIcon(_redoIcon);
                var icon = (IconViewModel) part.Children[0];
                icon.Name = "changedId";
                part.InsertIcon(_redoIcon);

                // Act / assert
                CheckIntegrity(viewModel);

                viewModel.Save(false, _destFile);
            }

            using (var doc = new OfficeDocument(_destFile))
            using (var viewModel = new OfficeDocumentViewModel(doc))
            {
                CheckIntegrity(viewModel);
                viewModel.Save(true, _destFile);
            }

            using (var doc = new OfficeDocument(_destFile))
            using (var viewModel = new OfficeDocumentViewModel(doc))
            {
                CheckIntegrity(viewModel);
            }
        }
    }
}
