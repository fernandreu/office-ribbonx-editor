// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindowViewModelTests.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the MainWindowViewModelTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using CustomUIEditor.Extensions;
    using CustomUIEditor.Models;
    using CustomUIEditor.Services;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    [Category("Integration")]
    public class MainWindowViewModelTests
    {
        private readonly Mock<IMessageBoxService> msgSvc = new Mock<IMessageBoxService>();

        private readonly Mock<IFileDialogService> fileSvc = new Mock<IFileDialogService>();

        private readonly Mock<IVersionChecker> versionChecker = new Mock<IVersionChecker>();

        private readonly string sourceFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/Blank.xlsx");

        private readonly string destFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Output/BlankSaved.xlsx");

        private readonly string undoIcon = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/undo.png");

        private readonly string redoIcon = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/redo.png");

        private MainWindowViewModel viewModel;

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

            this.viewModel = new MainWindowViewModel(this.msgSvc.Object, this.fileSvc.Object, this.versionChecker.Object);
        }

        [Test]
        public void OpenFileTest()
        {
            this.viewModel.OpenCommand.Execute();
            Assert.IsNotEmpty(this.viewModel.DocumentList);
            var doc = this.viewModel.DocumentList[0];
            Assert.AreEqual("Blank.xlsx", doc.Name);
        }

        [Test]
        public void SaveAsTest()
        {
            this.viewModel.OpenCommand.Execute();
            Assert.IsFalse(File.Exists(this.destFile), "Output file was not deleted before unit test");
            this.viewModel.SelectedItem = this.viewModel.DocumentList[0];
            this.viewModel.SaveAsCommand.Execute();
            Assert.IsTrue(File.Exists(this.destFile), "File was not saved");
        }

        /// <summary>
        /// Checks that both parts and icons can be inserted and removed correctly
        /// </summary>
        [Test]
        public void InsertAndRemoveTest()
        {
            this.viewModel.OpenCommand.Execute();

            Assert.IsNotEmpty(this.viewModel.DocumentList);

            var doc = this.viewModel.DocumentList[0];
            this.viewModel.SelectedItem = doc;

            this.viewModel.InsertXml12Command.Execute();
            Assert.AreEqual(1, doc.Children.Count);

            this.viewModel.InsertXml14Command.Execute();
            Assert.AreEqual(2, doc.Children.Count);

            this.viewModel.InsertXml14Command.Execute();  // This should do nothing because it is already added
            Assert.AreEqual(2, doc.Children.Count);

            var part = doc.Children[0] as OfficePartViewModel;
            Assert.IsNotNull(part, "Part is null");
            this.viewModel.SelectedItem = part;
            
            this.MockOpenFiles(this.undoIcon);
            this.viewModel.InsertIconsCommand.Execute();
            Assert.AreEqual(1, part.Children.Count);
            Assert.AreEqual("undo", ((IconViewModel)part.Children[0]).Id);
            
            this.MockOpenFiles(this.redoIcon);
            this.viewModel.InsertIconsCommand.Execute();
            Assert.AreEqual(2, part.Children.Count);
            Assert.AreEqual("redo", ((IconViewModel)part.Children[1]).Id);

            this.viewModel.SelectedItem = part.Children[0];
            this.viewModel.RemoveCommand.Execute();
            Assert.AreEqual(1, part.Children.Count);

            this.viewModel.SelectedItem = part;
            this.viewModel.RemoveCommand.Execute();
            Assert.AreEqual(1, doc.Children.Count);
        }

        /// <summary>
        /// Checks if a warning is shown after inserting a part in a document and then trying to close it
        /// </summary>
        [Test]
        public void InsertPartCloseDocumentWarningTest()
        {
            this.viewModel.OpenCommand.Execute();
            var doc = this.viewModel.DocumentList[0];
            this.viewModel.SelectedItem = doc;
            this.viewModel.InsertXml12Command.Execute();
            this.AssertMessage(this.viewModel.CloseCommand.Execute, MessageBoxImage.Warning, MessageBoxResult.Cancel, "Insert XML not detected as change");
        }

        /// <summary>
        /// Checks if a warning is shown when removing a part and when closing the document after that
        /// </summary>
        [Test]
        public void RemovePartWarningTest()
        {
            this.viewModel.OpenCommand.Execute();

            // First check if a warning is shown when a part is removed and you then attempt to close the document
            var doc = this.viewModel.DocumentList[0];
            this.viewModel.SelectedItem = doc;
            this.viewModel.InsertXml12Command.Execute();
            var part = doc.Children.FirstOrDefault(p => p is OfficePartViewModel);
            Assert.NotNull(part, "No Office part available");
            this.viewModel.SelectedItem = part;
            this.AssertMessage(this.viewModel.RemoveCommand.Execute, MessageBoxImage.Warning, MessageBoxResult.Yes);
            Assert.IsTrue(doc.HasUnsavedChanges, "No unsaved changes detected after removing a part");
            this.AssertMessage(this.viewModel.CloseCommand.Execute, MessageBoxImage.Warning, MessageBoxResult.Cancel);
        }

        /// <summary>
        /// Checks if a warning is shown when removing an icon and when closing the document after that
        /// </summary>
        [Test]
        public void RemoveIconWarningTest()
        {
            // Open a document, insert a part and select it
            this.viewModel.OpenCommand.Execute();
            var doc = this.viewModel.DocumentList[0];
            this.viewModel.SelectedItem = doc;
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
            this.AssertMessage(this.viewModel.CloseCommand.Execute, MessageBoxImage.Warning, MessageBoxResult.Cancel);
        }

        [Test]
        public void XmlValidationTest()
        {
            this.viewModel.OpenCommand.Execute();

            var doc = this.viewModel.DocumentList[0];
            this.viewModel.SelectedItem = doc;
            Assert.IsFalse(this.viewModel.SelectedItem.CanHaveContents);
            
            this.viewModel.InsertXml12Command.Execute();
            this.viewModel.SelectedItem = doc.Children[0];
            Assert.IsTrue(this.viewModel.SelectedItem.CanHaveContents);
            
            this.AssertMessage(this.viewModel.ValidateCommand.Execute, MessageBoxImage.Error);
            this.viewModel.SelectedItem.Contents = "asd";
            this.AssertMessage(this.viewModel.ValidateCommand.Execute, MessageBoxImage.Error);
            
            this.viewModel.SelectedItem.Contents = @"<customUI xmlns=""http://schemas.microsoft.com/office/2006/01/customui""><ribbon></ribbon></customUI>";
            this.AssertMessage(this.viewModel.ValidateCommand.Execute, MessageBoxImage.Information);
            
            this.viewModel.SelectedItem.Contents = @"<customUI xmlns=""http://schemas.microsoft.com/office/2006/01/customui""><ribbon><tabs></tabs></ribbon></customUI>";
            this.AssertMessage(this.viewModel.ValidateCommand.Execute, MessageBoxImage.Error);
        }

        /// <summary>
        /// Checks whether the callbacks window would show the callbacks we would expect
        /// </summary>
        [Test]
        public void GenerateCallbacksTest()
        {
            this.viewModel.OpenCommand.Execute();
            var doc = this.viewModel.DocumentList[0];
            this.viewModel.SelectedItem = doc;
            this.viewModel.InsertXml12Command.Execute();
            var part = doc.Children[0];
            this.viewModel.SelectedItem = part;

            // This should show a message saying there are no callbacks to be generated
            part.Contents = @"<customUI xmlns=""http://schemas.microsoft.com/office/2006/01/customui""><ribbon></ribbon></customUI>";
            this.AssertMessage(this.viewModel.GenerateCallbacksCommand.Execute, MessageBoxImage.Information);

            // This should contain a single callback for the onLoad event
            part.Contents = @"<customUI onLoad=""CustomLoad"" xmlns=""http://schemas.microsoft.com/office/2006/01/customui""><ribbon></ribbon></customUI>";
            void Handler(object o, DataEventArgs<string> e) => Assert.IsTrue(e.Data.StartsWith("'Callback for customUI.onLoad"), "Expected callback not generated");
            this.viewModel.ShowCallbacks += Handler;
            this.viewModel.GenerateCallbacksCommand.Execute();
            this.viewModel.ShowCallbacks -= Handler;  // Just in case we add other checks later
        }

        /// <summary>
        /// Checks whether the sample XML files are inserted as we would expect
        /// </summary>
        [Test]
        public void InsertSampleTest()
        {
            this.viewModel.OpenCommand.Execute();
            var doc = this.viewModel.DocumentList[0];
            this.viewModel.SelectedItem = doc;

            var sample = this.viewModel.XmlSamples.First();  // This shouldn't be null; if it is, the test will be a means to detect that too
            this.viewModel.InsertXmlSampleCommand.Execute(sample.ResourceName);
            var part = doc.Children.First(); // Again, this should not be null

            var contents = sample.ReadContents();
            Assert.AreEqual(contents, part.Contents, "Sample XML file not created with the expected contents");
        }

        /// <summary>
        /// Checks whether a file being saved while opened in another program is detected correctly and does not cause any crash
        /// </summary>
        [Test]
        public void SaveInUseTest()
        {
            File.Copy(this.sourceFile, this.destFile);
            this.MockOpenFile(this.destFile);
            this.viewModel.OpenCommand.Execute();
            this.viewModel.SelectedItem = this.viewModel.DocumentList[0];
            
            // Open the same file in exclusive mode
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
            this.viewModel.OpenCommand.Execute();
            this.viewModel.SelectedItem = this.viewModel.DocumentList[0];
            this.viewModel.SaveAsCommand.Execute();
            
            // Open the same file in exclusive mode
            Assert.DoesNotThrow(() =>
                {
                    using (File.Open(this.destFile, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        this.viewModel.SaveAsCommand.Execute();
                    }
                });
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
    }
}