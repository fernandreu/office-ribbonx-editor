using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Moq;
using NUnit.Framework;
using OfficeRibbonXEditor.Extensions;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Models.Documents;
using OfficeRibbonXEditor.Models.Events;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using OfficeRibbonXEditor.ViewModels.Documents;
using OfficeRibbonXEditor.ViewModels.Samples;
using OfficeRibbonXEditor.ViewModels.Windows;

namespace OfficeRibbonXEditor.FunctionalTests.Windows
{
    [TestFixture]
    public sealed class MainWindowViewModelTests : IDisposable
    {
        private readonly Mock<IMessageBoxService> msgSvc = new Mock<IMessageBoxService>();

        private readonly Mock<IFileDialogService> fileSvc = new Mock<IFileDialogService>();

        private readonly Mock<IVersionChecker> versionChecker = new Mock<IVersionChecker>();

        private readonly Mock<IDialogProvider> dialogProvider = new Mock<IDialogProvider>();

        private readonly string sourceFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/Blank.xlsx");

        private readonly string destFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Output/BlankSaved.xlsx");

        private readonly string undoIcon = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/undo.png");

        private readonly string redoIcon = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/redo.png");

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. This is defined in SetUp anyway
        private MainWindowViewModel viewModel;
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        [SetUp]
        public void SetUp()
        {
            this.MockOpenFile(this.sourceFile);
            this.MockSaveFile(this.destFile);
            
            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(Path.GetDirectoryName(this.destFile));

            if (File.Exists(this.destFile))
            {
                File.Delete(this.destFile);
            }

            this.viewModel = new MainWindowViewModel(
                this.msgSvc.Object, 
                this.fileSvc.Object, 
                this.versionChecker.Object, 
                this.dialogProvider.Object);
        }

        [Test]
        public void DocumentShouldBeOpened()
        {
            // Arrange / act
            var doc = this.OpenSource();

            // Assert
            Assert.AreEqual("Blank.xlsx", doc.Name);
        }

        [Test]
        public void DocumentShouldBeSaved()
        {
            // Arrange
            this.OpenSource();
            Assume.That(File.Exists(this.destFile), Is.False, "Output file was not deleted before unit test");
            
            // Act
            this.viewModel.SaveAsCommand.Execute();
            
            // Assert
            Assert.IsTrue(File.Exists(this.destFile), "File was not saved");
        }

        [Test]
        public void Xml12PartShouldBeInserted()
        {
            // Arrange
            var doc = this.OpenSource();

            // Act
            this.viewModel.InsertXml12Command.Execute();

            // Assert
            Assert.AreEqual(1, doc.Children.Count);
            Assert.IsInstanceOf<OfficePartViewModel>(doc.Children[0]);
            Assert.AreEqual(XmlPart.RibbonX12, ((OfficePartViewModel)doc.Children[0]).Part!.PartType);
        }

        [Test]
        public void Xml14PartShouldBeInserted()
        {
            // Arrange
            var doc = this.OpenSource();

            // Act
            this.viewModel.InsertXml14Command.Execute();

            // Assert
            Assert.AreEqual(1, doc.Children.Count);
            Assert.IsInstanceOf<OfficePartViewModel>(doc.Children[0]);
            Assert.AreEqual(XmlPart.RibbonX14, ((OfficePartViewModel)doc.Children[0]).Part!.PartType);
        }

        [Test]
        public void Xml12PartShouldNotBeInsertedIfAlreadyExists()
        {
            // Arrange
            var (doc, _) = this.OpenAndInsertPart(XmlPart.RibbonX12, false);

            // Act
            this.viewModel.InsertXml12Command.Execute();

            // Assert
            Assert.AreEqual(1, doc.Children.Count, "Part was inserted twice");
        }
        
        [Test]
        public void Xml14PartShouldNotBeInsertedIfAlreadyExists()
        {
            // Arrange
            var (doc, _) = this.OpenAndInsertPart(XmlPart.RibbonX14, false);

            // Act
            this.viewModel.InsertXml14Command.Execute();

            // Assert
            Assert.AreEqual(1, doc.Children.Count, "Part was inserted twice");
        }

        [Test]
        public void PartShouldBeRemoved()
        {
            // Arrange
            var (doc, _) = this.OpenAndInsertPart();

            // Act
            this.viewModel.RemoveCommand.Execute();

            // Assert
            Assert.IsEmpty(doc.Children, "Part was not removed");

        }

        [Test]
        public void IconShouldBeInserted()
        {
            // Arrange
            var (_, part) = this.OpenAndInsertPart();
            this.MockOpenFiles(this.undoIcon);

            // Act
            this.viewModel.InsertIconsCommand.Execute();

            // Assert
            Assert.AreEqual(1, part.Children.Count);
            Assert.AreEqual("undo", ((IconViewModel)part.Children[0]).Name);
        }

        [Test]
        public void MultipleIconsShouldBeInserted()
        {
            // Arrange
            var (_, part) = this.OpenAndInsertPart();

            // Act
            this.MockOpenFiles(this.undoIcon);
            this.viewModel.InsertIconsCommand.Execute();
            this.MockOpenFiles(this.redoIcon);
            this.viewModel.InsertIconsCommand.Execute();

            // Assert
            Assert.AreEqual(2, part.Children.Count);
            Assert.AreEqual("undo", ((IconViewModel)part.Children[0]).Name);
            Assert.AreEqual("redo", ((IconViewModel)part.Children[1]).Name);
        }

        [Test]
        public void IconShouldBeRemoved()
        {
            // Arrange
            var (_, part) = this.OpenAndInsertPart();
            this.MockOpenFiles(this.undoIcon);
            this.viewModel.InsertIconsCommand.Execute();
            Assume.That(part.Children, Is.Not.Empty, "Icon was not inserted");
            this.viewModel.SelectedItem = part.Children[0];

            // Act
            this.viewModel.RemoveCommand.Execute();

            // Assert
            Assert.IsEmpty(part.Children, "Icon was not removed");
        }

        /// <summary>
        /// Checks if a warning is shown after inserting a part in a document and then trying to close it
        /// </summary>
        [Test]
        public void ClosingDocumentAfterInsertingPartShouldGiveWarningMessage()
        {
            // Arrange
            this.OpenSource();
            this.viewModel.InsertXml12Command.Execute();

            // Act / assert
            this.AssertMessage(this.viewModel.CloseDocumentCommand.Execute, MessageBoxImage.Warning, MessageBoxResult.Cancel, "Insert XML not detected as change");
        }
        
        /// <summary>
        /// Checks if a warning is shown when a part is removed
        /// </summary>
        [Test]
        public void RemovingPartShouldGiveWarningMessage()
        {
            // Arrange
            var doc = this.OpenSource();
            this.viewModel.InsertXml12Command.Execute();
            var part = doc.Children.FirstOrDefault(p => p is OfficePartViewModel);
            Assume.That(part, Is.Not.Null, "No Office part available");
            this.viewModel.SelectedItem = part;

            // Act / assert
            this.AssertMessage(this.viewModel.RemoveCommand.Execute, MessageBoxImage.Warning, MessageBoxResult.Yes);
        }

        /// <summary>
        /// Checks if a warning is shown when a part is removed and the document is about to be closed
        /// </summary>
        [Test]
        public void ClosingDocumentAfterRemovingPartShouldGiveWarningMessage()
        {
            // Arrange
            var doc = this.OpenSource();
            this.viewModel.InsertXml12Command.Execute();
            var part = doc.Children.FirstOrDefault(p => p is OfficePartViewModel);
            Assume.That(part, Is.Not.Null, "No Office part available");
            this.viewModel.SelectedItem = part;

            // Act
            this.viewModel.RemoveCommand.Execute();

            // Assert
            Assert.IsTrue(doc.HasUnsavedChanges, "No unsaved changes detected after removing a part");
            this.AssertMessage(this.viewModel.CloseDocumentCommand.Execute, MessageBoxImage.Warning, MessageBoxResult.Cancel);
        }

        /// <summary>
        /// Checks if a warning is shown when removing an icon and when closing the document after that
        /// </summary>
        [Test]
        public void RemoveIconWarningTest()
        {
            // Open a document, insert a part and select it
            var doc = this.OpenSource();
            this.viewModel.InsertXml12Command.Execute();
            this.viewModel.SelectedItem = doc.Children[0];

            // Insert an icon and save the document
            this.MockOpenFiles(this.redoIcon);
            this.viewModel.InsertIconsCommand.Execute();
            this.viewModel.SaveAsCommand.Execute();
            Assert.IsFalse(doc.HasUnsavedChanges, "The icon insertion was apparently not saved");

            // Remove it and do the appropriate checks
            this.viewModel.SelectedItem = doc.Children.FirstOrDefault(c => c is OfficePartViewModel)?.Children.FirstOrDefault(c => c is IconViewModel);
            Assert.IsNotNull(this.viewModel.SelectedItem, "Icon was apparently not created");
            this.AssertMessage(this.viewModel.RemoveCommand.Execute, MessageBoxImage.Warning, MessageBoxResult.Yes);
            Assert.IsTrue(doc.HasUnsavedChanges, "No unsaved changes detected after removing a part");
            this.AssertMessage(this.viewModel.CloseDocumentCommand.Execute, MessageBoxImage.Warning, MessageBoxResult.Cancel);
        }

        /// <summary>
        /// Checks if the XML validation provides the expected result for a few sample cases
        /// </summary>
        [Test]
        public void XmlValidationTest()
        {
            var doc = this.OpenSource();
            Assume.That(this.viewModel.SelectedItem?.CanHaveContents, Is.False);
            
            this.viewModel.InsertXml12Command.Execute();
            this.viewModel.SelectedItem = doc.Children[0];
            Assert.IsTrue(this.viewModel.SelectedItem.CanHaveContents);

            var tmp = this.viewModel.OpenPartTab();
            Assert.NotNull(tmp);
            var tab = tmp!;

            void AssertIsValid(bool expected, string? message = null)
            {
                var actual = true;
                void Handler(object? o, DataEventArgs<IResultCollection> e)
                {
                    actual = e.Data?.IsEmpty ?? false;
                }

                tab.ShowResults += Handler;
                this.viewModel.ValidateCommand.Execute();
                tab.ShowResults -= Handler;

                Assert.AreEqual(expected, actual, message);
            }

            AssertIsValid(false);
            tab.Part.Contents = "asd";
            AssertIsValid(false);
            
            tab.Part.Contents = @"<customUI xmlns=""http://schemas.microsoft.com/office/2006/01/customui""><ribbon></ribbon></customUI>";
            AssertIsValid(true);
            
            tab.Part.Contents = @"<customUI xmlns=""http://schemas.microsoft.com/office/2006/01/customui""><ribbon><tabs></tabs></ribbon></customUI>";
            AssertIsValid(false);
        }

        /// <summary>
        /// Checks whether the callbacks window would show the callbacks we would expect
        /// </summary>
        [Test]
        public void GenerateCallbacksTest()
        {
            var doc = this.OpenSource();
            this.viewModel.InsertXml12Command.Execute();
            var part = doc.Children[0];
            this.viewModel.SelectedItem = part;

            var tab = this.viewModel.OpenPartTab();
            Assert.NotNull(tab);

            // This should show a message saying there are no callbacks to be generated
            part.Contents = @"<customUI xmlns=""http://schemas.microsoft.com/office/2006/01/customui""><ribbon></ribbon></customUI>";
            this.AssertMessage(this.viewModel.GenerateCallbacksCommand.Execute, MessageBoxImage.Information);

            // This should contain a single callback for the onLoad event
            part.Contents = @"<customUI onLoad=""CustomLoad"" xmlns=""http://schemas.microsoft.com/office/2006/01/customui""><ribbon></ribbon></customUI>";
            static void Handler(object? o, LaunchDialogEventArgs e)
            {
                Assert.IsTrue(e.Content is CallbackDialogViewModel, $"Unexpected dialog launched: {e.Content.GetType().Name}");
                Assert.IsTrue(((CallbackDialogViewModel) e.Content).Code?.StartsWith("'Callback for customUI.onLoad", StringComparison.OrdinalIgnoreCase), "Expected callback not generated");
            }

            this.viewModel.LaunchingDialog += Handler;
            this.viewModel.GenerateCallbacksCommand.Execute();
            this.viewModel.LaunchingDialog -= Handler;  // Just in case we add other checks later
        }

        /// <summary>
        /// Checks whether the sample XML files are inserted as we would expect
        /// </summary>
        [Test]
        public void InsertSampleInNonExistingPart()
        {
            var doc = this.OpenSource();
            
            var sample = this.viewModel.XmlSamples?.Items.OfType<XmlSampleViewModel>().First();  // This shouldn't be null; if it is, the test will be a means to detect that too
            this.viewModel.InsertXmlSampleCommand.Execute(sample);
            var part = doc.Children.First(); // Again, this should not be null

            // It is expected that the part created will be an Office 2007 one, but the templates are for Office 2010+. This gets automatically replaced
            // at insertion time
#pragma warning disable CA1307 // Missing StringComparison because it is not available in .NET Framework 4.6.1
            var contents = sample?.ReadContents().Replace("2009/07", "2006/01") ?? string.Empty;
#pragma warning restore CA1307
            Assert.AreEqual(contents, part.Contents, "Sample XML file not created with the expected contents");
        }

        /// <summary>
        /// Checks whether a file being saved while opened in another program is detected correctly and does not cause any crash
        /// </summary>
        [Test]
        public void SaveInUseTest()
        {
            // Arrange
            File.Copy(this.sourceFile, this.destFile);
            this.MockOpenFile(this.destFile);
            this.viewModel.OpenDocumentCommand.Execute();
            this.viewModel.SelectedItem = this.viewModel.DocumentList[0];
            
            // Act / assert: Open the same file in exclusive mode
            Assert.DoesNotThrow(() =>
            {
                using (File.Open(this.destFile, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    this.viewModel.SaveCommand.Execute();
                }
            });
        }

        /// <summary>
        /// Checks whether a file being saved with the Save As... option while opened in another program is detected correctly and does not cause any crash
        /// </summary>
        [Test]
        public void SaveAsInUseTest()
        {
            // Arrange
            this.OpenSource();
            this.viewModel.SaveAsCommand.Execute();
            
            // Act / assert: Open the same file in exclusive mode
            Assert.DoesNotThrow(() =>
                {
                    using (File.Open(this.destFile, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        this.viewModel.SaveAsCommand.Execute();
                    }
                });
        }
        
        /// <summary>
        /// Opens the document given by sourceFile
        /// </summary>
        /// <param name="select">Whether the document should be selected once it has been opened</param>
        /// <returns>The opened document</returns>
        private OfficeDocumentViewModel OpenSource(bool select = true)
        {
            this.viewModel.OpenDocumentCommand.Execute();
            Assume.That(this.viewModel.DocumentList, Is.Not.Empty);
            var doc = this.viewModel.DocumentList[this.viewModel.DocumentList.Count - 1];
            if (select)
            {
                this.viewModel.SelectedItem = doc;
            }

            return doc;
        }

        private Tuple<OfficeDocumentViewModel, OfficePartViewModel> OpenAndInsertPart(XmlPart partType = XmlPart.RibbonX12, bool select = true)
        {
            var doc = this.OpenSource();
            if (partType == XmlPart.RibbonX12)
            {
                this.viewModel.InsertXml12Command.Execute();
            }
            else
            {
                this.viewModel.InsertXml14Command.Execute();
            }

            Assume.That(doc.Children, Is.Not.Empty, "XML part was not inserted");
            Assume.That(doc.Children[0], Is.InstanceOf<OfficePartViewModel>(), "Wrong class was inserted");

            if (select)
            {
                this.viewModel.SelectedItem = doc.Children[0];
            }

            return Tuple.Create(doc, (OfficePartViewModel)doc.Children[0]);
        }

        private void MockOpenFile(string path)
        {
            this.fileSvc.Setup(x => x.OpenFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true)
                .Callback<string, string, Action<string>, string, int>((title, filter, action, fileName, filterIndex) => action(path));
        }

        private void MockOpenFiles(string path)
        {
            this.MockOpenFiles(new[] { path });
        }

        private void MockOpenFiles(IEnumerable<string> paths)
        {
            this.fileSvc.Setup(x => x.OpenFilesDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<IEnumerable<string>>>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true)
                .Callback<string, string, Action<IEnumerable<string>>, string, int>((title, filter, action, fileName, filterIndex) => action(paths));
        }

        private void MockSaveFile(string path)
        {
            this.fileSvc.Setup(x => x.SaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true)
                .Callback<string, string, Action<string>, string, int>((title, filter, action, fileName, filterIndex) => action(path));
        }

        private void AssertMessage(Action action, MessageBoxImage image, MessageBoxResult result = MessageBoxResult.OK, string message = "Message not shown")
        {
            var count = 0;
            this.msgSvc.Setup(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), image)).Returns(result).Callback(() => ++count);
            action();
            Assert.AreEqual(1, count, message);
        }

        public void Dispose()
        {
            viewModel?.Dispose();
        }
    }
}