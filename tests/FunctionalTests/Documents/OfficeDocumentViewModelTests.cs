using System.IO;
using NUnit.Framework;
using OfficeRibbonXEditor.Documents;
using OfficeRibbonXEditor.ViewModels.Documents;

namespace OfficeRibbonXEditor.FunctionalTests.Documents
{
    [TestFixture]
    public class OfficeDocumentViewModelTests
    {
        private readonly string sourceFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/Blank.xlsx");

        private readonly string destFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Output/BlankSaved.xlsx");

        private readonly string undoIcon = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/undo.png");

        private readonly string redoIcon = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/redo.png");

        [SetUp]
        public void SetUp()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(Path.GetDirectoryName(this.destFile));

            if (File.Exists(this.destFile))
            {
                File.Delete(this.destFile);
            }
        }

        [Test]
        public void PartShouldBeInserted()
        {
            // Arrange
            using (var doc = new OfficeDocument(this.sourceFile))
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
            using (var doc = new OfficeDocument(this.sourceFile))
            using (var viewModel = new OfficeDocumentViewModel(doc))
            {
                viewModel.InsertPart(XmlPart.RibbonX12);
                Assume.That(File.Exists(this.destFile), Is.False, "File was not deleted before test");

                // Act
                doc.Save(this.destFile);

                // Assert
                Assert.IsTrue(File.Exists(this.destFile), "File was not saved");
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

            using (var doc = new OfficeDocument(this.sourceFile))
            using (var viewModel = new OfficeDocumentViewModel(doc))
            {
                viewModel.InsertPart(XmlPart.RibbonX12);
                viewModel.InsertPart(XmlPart.RibbonX14);

                var part = (OfficePartViewModel) viewModel.Children[0];
                part.InsertIcon(this.undoIcon);
                part.InsertIcon(this.redoIcon);
                part.RemoveIcon("undo");
                part = (OfficePartViewModel) viewModel.Children[1];
                part.InsertIcon(this.redoIcon);
                var icon = (IconViewModel) part.Children[0];
                icon.Name = "changedId";
                part.InsertIcon(this.redoIcon);

                // Act / assert
                CheckIntegrity(viewModel);

                viewModel.Save(false, this.destFile);
            }

            using (var doc = new OfficeDocument(this.destFile))
            using (var viewModel = new OfficeDocumentViewModel(doc))
            {
                CheckIntegrity(viewModel);
                viewModel.Save(true, this.destFile);
            }

            using (var doc = new OfficeDocument(this.destFile))
            using (var viewModel = new OfficeDocumentViewModel(doc))
            {
                CheckIntegrity(viewModel);
            }
        }
    }
}
