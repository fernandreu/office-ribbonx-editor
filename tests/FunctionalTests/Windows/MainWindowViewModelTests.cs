using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Autofac;
using Moq;
using NUnit.Framework;
using OfficeRibbonXEditor.Documents;
using OfficeRibbonXEditor.Events;
using OfficeRibbonXEditor.Extensions;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using OfficeRibbonXEditor.ViewModels.Documents;
using OfficeRibbonXEditor.ViewModels.Samples;
using OfficeRibbonXEditor.ViewModels.Windows;

namespace OfficeRibbonXEditor.FunctionalTests.Windows
{
    [TestFixture]
    public sealed class MainWindowViewModelTests : IDisposable
    {
        private readonly Mock<IMessageBoxService> _msgSvc = new Mock<IMessageBoxService>();

        private readonly Mock<IFileDialogService> _fileSvc = new Mock<IFileDialogService>();

        private readonly Mock<IVersionChecker> _versionChecker = new Mock<IVersionChecker>();

        private readonly Mock<IUrlHelper> _urlHelper = new Mock<IUrlHelper>();

        private readonly IContainer _container = App.CreateContainer();

        private readonly string _sourceFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/Blank.xlsx");

        private readonly string _destFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Output/BlankSaved.xlsx");

        private readonly string _undoIcon = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/undo.png");

        private readonly string _redoIcon = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/redo.png");

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. This is defined in SetUp anyway
        private MainWindowViewModel _viewModel;
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        [SetUp]
        public void SetUp()
        {
            MockOpenFile(_sourceFile);
            MockSaveFile(_destFile);
            
            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(Path.GetDirectoryName(_destFile));

            if (File.Exists(_destFile))
            {
                File.Delete(_destFile);
            }

            _viewModel = new MainWindowViewModel(
                _msgSvc.Object, 
                _fileSvc.Object, 
                _versionChecker.Object, 
                _container.Resolve<IDialogProvider>(),
                _urlHelper.Object);
            _viewModel.OnLoaded();
        }

        [Test]
        public void DocumentShouldBeOpened()
        {
            // Arrange / act
            var doc = OpenSource();

            // Assert
            Assert.AreEqual("Blank.xlsx", doc.Name);
        }

        [Test]
        public void DocumentShouldBeSaved()
        {
            // Arrange
            OpenSource();
            Assume.That(File.Exists(_destFile), Is.False, "Output file was not deleted before unit test");
            
            // Act
            _viewModel.SaveAsCommand.Execute();
            
            // Assert
            Assert.IsTrue(File.Exists(_destFile), "File was not saved");
        }

        [Test]
        public void Xml12PartShouldBeInserted()
        {
            // Arrange
            var doc = OpenSource();

            // Act
            _viewModel.InsertXml12Command.Execute();

            // Assert
            Assert.AreEqual(1, doc.Children.Count);
            Assert.IsInstanceOf<OfficePartViewModel>(doc.Children[0]);
            Assert.AreEqual(XmlPart.RibbonX12, ((OfficePartViewModel)doc.Children[0]).Part!.PartType);
        }

        [Test]
        public void Xml14PartShouldBeInserted()
        {
            // Arrange
            var doc = OpenSource();

            // Act
            _viewModel.InsertXml14Command.Execute();

            // Assert
            Assert.AreEqual(1, doc.Children.Count);
            Assert.IsInstanceOf<OfficePartViewModel>(doc.Children[0]);
            Assert.AreEqual(XmlPart.RibbonX14, ((OfficePartViewModel)doc.Children[0]).Part!.PartType);
        }

        [Test]
        public void Xml12PartShouldNotBeInsertedIfAlreadyExists()
        {
            // Arrange
            var (doc, _) = OpenAndInsertPart(XmlPart.RibbonX12, false);

            // Act
            _viewModel.InsertXml12Command.Execute();

            // Assert
            Assert.AreEqual(1, doc.Children.Count, "Part was inserted twice");
        }
        
        [Test]
        public void Xml14PartShouldNotBeInsertedIfAlreadyExists()
        {
            // Arrange
            var (doc, _) = OpenAndInsertPart(XmlPart.RibbonX14, false);

            // Act
            _viewModel.InsertXml14Command.Execute();

            // Assert
            Assert.AreEqual(1, doc.Children.Count, "Part was inserted twice");
        }

        [Test]
        public void PartShouldBeRemoved()
        {
            // Arrange
            var (doc, _) = OpenAndInsertPart();

            // Act
            _viewModel.RemoveCommand.Execute();

            // Assert
            Assert.IsEmpty(doc.Children, "Part was not removed");
        }

        [Test]
        public void IconShouldBeInserted()
        {
            // Arrange
            var (_, part) = OpenAndInsertPart();
            MockOpenFiles(_undoIcon);

            // Act
            _viewModel.InsertIconsCommand.Execute();

            // Assert
            Assert.AreEqual(1, part.Children.Count);
            Assert.AreEqual("undo", ((IconViewModel)part.Children[0]).Name);
        }

        [Test]
        public void MultipleIconsShouldBeInserted()
        {
            // Arrange
            var (_, part) = OpenAndInsertPart();

            // Act
            MockOpenFiles(_undoIcon);
            _viewModel.InsertIconsCommand.Execute();
            MockOpenFiles(_redoIcon);
            _viewModel.InsertIconsCommand.Execute();

            // Assert
            Assert.AreEqual(2, part.Children.Count);
            Assert.AreEqual("undo", ((IconViewModel)part.Children[0]).Name);
            Assert.AreEqual("redo", ((IconViewModel)part.Children[1]).Name);
        }

        [Test]
        public void IconShouldBeRemoved()
        {
            // Arrange
            var (_, part) = OpenAndInsertPart();
            MockOpenFiles(_undoIcon);
            _viewModel.InsertIconsCommand.Execute();
            Assume.That(part.Children, Is.Not.Empty, "Icon was not inserted");
            _viewModel.SelectedItem = part.Children[0];

            // Act
            _viewModel.RemoveCommand.Execute();

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
            OpenSource();
            _viewModel.InsertXml12Command.Execute();

            // Act / assert
            AssertMessage(_viewModel.CloseDocumentCommand.Execute, MessageBoxImage.Warning, MessageBoxResult.Cancel, "Insert XML not detected as change");
        }
        
        /// <summary>
        /// Checks if a warning is shown when a part is removed
        /// </summary>
        [Test]
        public void RemovingPartShouldGiveWarningMessage()
        {
            // Arrange
            var doc = OpenSource();
            _viewModel.InsertXml12Command.Execute();
            var part = doc.Children.FirstOrDefault(p => p is OfficePartViewModel);
            Assume.That(part, Is.Not.Null, "No Office part available");
            _viewModel.SelectedItem = part;

            // Act / assert
            AssertMessage(_viewModel.RemoveCommand.Execute, MessageBoxImage.Warning, MessageBoxResult.Yes);
        }

        /// <summary>
        /// Checks if a warning is shown when a part is removed and the document is about to be closed
        /// </summary>
        [Test]
        public void ClosingDocumentAfterRemovingPartShouldGiveWarningMessage()
        {
            // Arrange
            var doc = OpenSource();
            _viewModel.InsertXml12Command.Execute();
            var part = doc.Children.FirstOrDefault(p => p is OfficePartViewModel);
            Assume.That(part, Is.Not.Null, "No Office part available");
            _viewModel.SelectedItem = part;

            // Act
            _viewModel.RemoveCommand.Execute();

            // Assert
            Assert.IsTrue(doc.HasUnsavedChanges, "No unsaved changes detected after removing a part");
            AssertMessage(_viewModel.CloseDocumentCommand.Execute, MessageBoxImage.Warning, MessageBoxResult.Cancel);
        }

        /// <summary>
        /// Checks if a warning is shown when removing an icon and when closing the document after that
        /// </summary>
        [Test]
        public void RemoveIconWarningTest()
        {
            // Open a document, insert a part and select it
            var doc = OpenSource();
            _viewModel.InsertXml12Command.Execute();
            _viewModel.SelectedItem = doc.Children[0];

            // Insert an icon and save the document
            MockOpenFiles(_redoIcon);
            _viewModel.InsertIconsCommand.Execute();
            _viewModel.SaveAsCommand.Execute();
            Assert.IsFalse(doc.HasUnsavedChanges, "The icon insertion was apparently not saved");

            // Remove it and do the appropriate checks
            _viewModel.SelectedItem = doc.Children.FirstOrDefault(c => c is OfficePartViewModel)?.Children.FirstOrDefault(c => c is IconViewModel);
            Assert.IsNotNull(_viewModel.SelectedItem, "Icon was apparently not created");
            AssertMessage(_viewModel.RemoveCommand.Execute, MessageBoxImage.Warning, MessageBoxResult.Yes);
            Assert.IsTrue(doc.HasUnsavedChanges, "No unsaved changes detected after removing a part");
            AssertMessage(_viewModel.CloseDocumentCommand.Execute, MessageBoxImage.Warning, MessageBoxResult.Cancel);
        }

        /// <summary>
        /// Checks if the XML validation provides the expected result for a few sample cases
        /// </summary>
        [Test]
        public void XmlValidationTest()
        {
            var doc = OpenSource();
            Assume.That(_viewModel.SelectedItem?.CanHaveContents, Is.False);
            
            _viewModel.InsertXml12Command.Execute();
            _viewModel.SelectedItem = doc.Children[0];
            Assert.IsTrue(_viewModel.SelectedItem.CanHaveContents);

            var tmp = _viewModel.OpenPartTab();
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
                _viewModel.ValidateCommand.Execute();
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
            var doc = OpenSource();
            _viewModel.InsertXml12Command.Execute();
            var part = doc.Children[0];
            _viewModel.SelectedItem = part;

            var tab = _viewModel.OpenPartTab();
            Assert.NotNull(tab);

            // This should show a message saying there are no callbacks to be generated
            part.Contents = @"<customUI xmlns=""http://schemas.microsoft.com/office/2006/01/customui""><ribbon></ribbon></customUI>";
            AssertMessage(_viewModel.GenerateCallbacksCommand.Execute, MessageBoxImage.Information);

            // This should contain a single callback for the onLoad event
            part.Contents = @"<customUI onLoad=""CustomLoad"" xmlns=""http://schemas.microsoft.com/office/2006/01/customui""><ribbon></ribbon></customUI>";
            static void Handler(object? o, LaunchDialogEventArgs e)
            {
                Assert.IsTrue(e.Content is CallbackDialogViewModel, $"Unexpected dialog launched: {e.Content.GetType().Name}");
                Assert.IsTrue(((CallbackDialogViewModel) e.Content).Code?.StartsWith("'Callback for customUI.onLoad", StringComparison.OrdinalIgnoreCase), "Expected callback not generated");
            }

            _viewModel.LaunchingDialog += Handler;
            _viewModel.GenerateCallbacksCommand.Execute();
            _viewModel.LaunchingDialog -= Handler;  // Just in case we add other checks later
        }

        [Test]
        public void CanLaunchSettingsDialog()
        {
            // Arrange
            IContentDialogBase? content = null;
            _viewModel.LaunchingDialog += (o, e) =>
            {
                content = e.Content;
            };

            // Act
            _viewModel.ShowSettingsCommand.Execute();

            // Assert
            Assert.IsInstanceOf(typeof(SettingsDialogViewModel), content);
            var dialog = (SettingsDialogViewModel)content!;
            Assert.IsEmpty(dialog.Tabs);
        }

        [Test]
        public void CanLaunchSettingsDialog_WithEditorTab()
        {
            // Arrange
            var (_, part) = OpenAndInsertPart();
            _viewModel.OpenTabCommand.Execute(part);
            IContentDialogBase? content = null;
            _viewModel.LaunchingDialog += (o, e) =>
            {
                content = e.Content;
            };

            // Act
            _viewModel.ShowSettingsCommand.Execute();

            // Assert
            Assert.IsInstanceOf(typeof(SettingsDialogViewModel), content);
            var dialog = (SettingsDialogViewModel) content!;
            Assert.IsNotEmpty(dialog.Tabs);
        }

        [Test]
        public void TabTitlesAreAdjusted()
        {
            // Arrange
            var (_, part) = OpenAndInsertPart();
            _viewModel.OpenTabCommand.Execute(part);
            var tab = _viewModel.SelectedTab;
            Assume.That(tab, Is.Not.Null, "Tab is null");
            var original = tab!.Title;

            // Act / assert

            // We open the same document again (confirming that action first), and check if the title changed
            AssertMessage(() => OpenAndInsertPart(), MessageBoxImage.Warning, MessageBoxResult.Yes);
            Assert.AreNotEqual(original, tab.Title);

            // We close the newly opened document, and check if the title is back to normal
            _viewModel.CloseDocumentCommand.Execute();
            Assert.AreEqual(original, tab.Title);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CanCloseTabs(bool explicitly)
        {
            // Arrange
            var (_, part) = OpenAndInsertPart();
            _viewModel.OpenTabCommand.Execute(part);
            var tab = _viewModel.SelectedTab;
            Assume.That(tab, Is.Not.Null, "Tab is null");

            // Act
            _viewModel.CloseTabCommand.Execute(explicitly ? tab : null);

            // Assert
            Assert.IsEmpty(_viewModel.OpenTabs);
        }

        /// <summary>
        /// Checks whether the sample XML files are inserted as we would expect
        /// </summary>
        [Test]
        public void InsertSampleInNonExistingPart()
        {
            var doc = OpenSource();
            
            var sample = _viewModel.XmlSamples?.Items.OfType<XmlSampleViewModel>().First();  // This shouldn't be null; if it is, the test will be a means to detect that too
            _viewModel.InsertXmlSampleCommand.Execute(sample);
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
            File.Copy(_sourceFile, _destFile);
            MockOpenFile(_destFile);
            _viewModel.OpenDocumentCommand.Execute();
            _viewModel.SelectedItem = _viewModel.DocumentList[0];

            // Act / assert: Open the same file in exclusive mode
            using (File.Open(_destFile, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                AssertMessage(() => _viewModel.SaveCommand.Execute(), MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Checks whether a file being saved with the Save As... option while opened in another program is detected correctly and does not cause any crash
        /// </summary>
        [Test]
        public void SaveAsInUseTest()
        {
            // Arrange
            OpenSource();
            _viewModel.SaveAsCommand.Execute();

            // Act / assert: Open the same file in exclusive mode
            // Act / assert: Open the same file in exclusive mode
            using (File.Open(_destFile, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                AssertMessage(() => _viewModel.SaveAsCommand.Execute(), MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Checks whether a file being saved while opened in another program is detected correctly and does not cause any crash
        /// </summary>
        [Test]
        public void SaveAllInUseTest()
        {
            // Arrange
            File.Copy(_sourceFile, _destFile);
            MockOpenFile(_destFile);
            _viewModel.OpenDocumentCommand.Execute();
            _viewModel.SelectedItem = _viewModel.DocumentList[0];

            // Act / assert: Open the same file in exclusive mode
            using (File.Open(_destFile, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                AssertMessage(() => _viewModel.SaveAllCommand.Execute(), MessageBoxImage.Error);
            }
        }

        [Test]
        public void CanOpenHelpPages()
        {
            // Arrange
            var triggered = false;
            _urlHelper.Setup(x => x.OpenExternal(It.IsAny<Uri>())).Callback(() => triggered = true);

            try
            {
                foreach (var pair in _viewModel.HelpLinks)
                {
                    triggered = false;

                    // Act
                    _viewModel.OpenHelpLinkCommand.Execute(pair.Value);

                    // Assert
                    Assert.True(triggered);
                }
            }
            finally
            {
                _urlHelper.Reset();
            }
        }

        public static readonly TestCaseData[] DragData =
        {
            new TestCaseData(DataFormats.Text, true, false).Returns(false),
            new TestCaseData(DataFormats.FileDrop, true, false).Returns(true),
            new TestCaseData(DataFormats.FileDrop, true, true).Returns(false),
            new TestCaseData(DataFormats.FileDrop, false, false).Returns(false),
        };

        [Test]
        [TestCaseSource(nameof(DragData))]
        public bool TestPreviewDragEnter(string dataFormat, bool existingFile, bool forceReturnNull)
        {
            // Arrange
            var e = MockDragData(dataFormat, forceReturnNull, existingFile);

            // Act
            _viewModel.PreviewDragEnterCommand.Execute(e);
            
            // Assert
            return e.Handled;
        }

        [Test]
        [TestCaseSource(nameof(DragData))]
        public bool TestDrop(string dataFormat, bool existingFile, bool forceReturnNull)
        {
            // Arrange
            var e = MockDragData(dataFormat, forceReturnNull, existingFile);

            // Act
            _viewModel.DropCommand.Execute(e);

            // Assert
            return _viewModel.DocumentList.Count > 0;
        }

        private DragData MockDragData(string dataFormat, bool forceReturnNull, bool existingFile)
        {
            var dataMock = new Mock<IDataObject>();
            dataMock.Setup(x => x.GetDataPresent(It.IsAny<string>()))
                .Returns<string>(x => x == dataFormat);
            dataMock.Setup(x => x.GetData(It.IsAny<string>()))
#pragma warning disable CS8603 // Possible null reference return.
                .Returns<string>(x => x != dataFormat || forceReturnNull ? null : new[] { existingFile ? _sourceFile : "abcd.efgh" });
#pragma warning restore CS8603 // Possible null reference return.
            return new DragData(dataMock.Object);
        }

        /// <summary>
        /// Opens the document given by sourceFile
        /// </summary>
        /// <param name="select">Whether the document should be selected once it has been opened</param>
        /// <returns>The opened document</returns>
        private OfficeDocumentViewModel OpenSource(bool select = true)
        {
            _viewModel.OpenDocumentCommand.Execute();
            Assume.That(_viewModel.DocumentList, Is.Not.Empty);
            var doc = _viewModel.DocumentList[_viewModel.DocumentList.Count - 1];
            if (select)
            {
                _viewModel.SelectedItem = doc;
            }

            return doc;
        }

        private Tuple<OfficeDocumentViewModel, OfficePartViewModel> OpenAndInsertPart(XmlPart partType = XmlPart.RibbonX12, bool select = true)
        {
            var doc = OpenSource();
            if (partType == XmlPart.RibbonX12)
            {
                _viewModel.InsertXml12Command.Execute();
            }
            else
            {
                _viewModel.InsertXml14Command.Execute();
            }

            Assume.That(doc.Children, Is.Not.Empty, "XML part was not inserted");
            Assume.That(doc.Children[0], Is.InstanceOf<OfficePartViewModel>(), "Wrong class was inserted");

            if (select)
            {
                _viewModel.SelectedItem = doc.Children[0];
            }

            return Tuple.Create(doc, (OfficePartViewModel)doc.Children[0]);
        }

        private void MockOpenFile(string path)
        {
            _fileSvc.Setup(x => x.OpenFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true)
                .Callback<string, string, Action<string>, string, int>((title, filter, action, fileName, filterIndex) => action(path));
        }

        private void MockOpenFiles(string path)
        {
            MockOpenFiles(new[] { path });
        }

        private void MockOpenFiles(IEnumerable<string> paths)
        {
            _fileSvc.Setup(x => x.OpenFilesDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<IEnumerable<string>>>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true)
                .Callback<string, string, Action<IEnumerable<string>>, string, int>((title, filter, action, fileName, filterIndex) => action(paths));
        }

        private void MockSaveFile(string path)
        {
            _fileSvc.Setup(x => x.SaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true)
                .Callback<string, string, Action<string>, string, int>((title, filter, action, fileName, filterIndex) => action(path));
        }

        private void AssertMessage(Action action, MessageBoxImage image, MessageBoxResult result = MessageBoxResult.OK, string message = "Message not shown")
        {
            var count = 0;
            try
            {
                _msgSvc.Setup(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), image)).Returns(result).Callback(() => ++count);
                action();
                Assert.AreEqual(1, count, message);
            }
            finally
            {
                _msgSvc.Reset();
            }
        }

        public void Dispose()
        {
            _viewModel?.Dispose();
        }
    }
}