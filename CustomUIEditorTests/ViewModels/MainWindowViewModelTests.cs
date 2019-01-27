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
    using System.IO;

    using CustomUIEditor.Services;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class MainWindowViewModelTests
    {
        private readonly Mock<IMessageBoxService> msgSvc = new Mock<IMessageBoxService>();

        private readonly Mock<IFileDialogService> fileSvc = new Mock<IFileDialogService>();

        [Test]
        public void OpenFileTest()
        {
            var testFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/Blank.xlsx");
            
            this.MockOpenFile(testFile);

            var viewModel = new MainWindowViewModel(this.msgSvc.Object, this.fileSvc.Object);
            viewModel.OpenCommand.Execute();

            Assert.IsNotEmpty(viewModel.DocumentList);

            var doc = viewModel.DocumentList[0];
            Assert.AreEqual("Blank.xlsx", doc.Name);
        }

        [Test]
        public void SaveAsTest()
        {
            var testFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/Blank.xlsx");
            
            this.MockOpenFile(testFile);

            var viewModel = new MainWindowViewModel(this.msgSvc.Object, this.fileSvc.Object);
            viewModel.OpenCommand.Execute();
            
            var savePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Output/BlankSaved.xlsx");
            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));

            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            this.MockSaveFile(savePath);
            
            var doc = viewModel.DocumentList[0];
            viewModel.SelectedItem = doc;

            viewModel.SaveAsCommand.Execute();
            Assert.IsTrue(File.Exists(savePath), "File was not saved");
        }

        private void MockOpenFile(string path)
        {
            this.fileSvc.Setup(x => x.OpenFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true)
                .Callback<string, string, Action<string>, string, int>((title, filter, action, fileName, filterIndex) => action(path));
        }

        private void MockSaveFile(string path)
        {
            this.fileSvc.Setup(x => x.SaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true)
                .Callback<string, string, Action<string>, string, int>((title, filter, action, fileName, filterIndex) => action(path));
        }
    }
}