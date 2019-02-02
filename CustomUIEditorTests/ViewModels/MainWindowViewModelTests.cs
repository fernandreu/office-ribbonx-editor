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

    using CustomUIEditor.Data;
    using CustomUIEditor.Services;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class MainWindowViewModelTests
    {
        private readonly Mock<IMessageBoxService> msgSvc = new Mock<IMessageBoxService>();

        private readonly Mock<IFileDialogService> fileSvc = new Mock<IFileDialogService>();

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

            this.viewModel = new MainWindowViewModel(this.msgSvc.Object, this.fileSvc.Object);
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
    }
}