using System;
using System.IO;
using System.Linq;
using System.Windows;
using NUnit.Framework;
using OfficeRibbonXEditor.Documents;
using OfficeRibbonXEditor.Events;
using OfficeRibbonXEditor.Extensions;
using OfficeRibbonXEditor.FunctionalTests.Helpers;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using OfficeRibbonXEditor.ViewModels.Documents;
using OfficeRibbonXEditor.ViewModels.Samples;

namespace OfficeRibbonXEditor.FunctionalTests.Windows
{
    [TestFixture]
    public sealed class MainWindowViewModelTests
    {
        private readonly string _sourceFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/Blank.xlsx");

        private readonly string _undoIcon = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/undo.png");

        private readonly string _redoIcon = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/redo.png");

        [Test]
        public void OpenDocument_Existing_ShouldBeOpened()
        {
            // Arrange / act
            using var wrapper = new MainWindowViewModelWrapper();
            var doc = wrapper.OpenDocument(_sourceFile);

            // Assert
            Assert.AreEqual("Blank.xlsx", doc.Name);
        }

        [Test]
        public void SaveAs_CorrectPath_ShouldBeSaved()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            var doc = wrapper.OpenDocument(_sourceFile);

            using var folder = new TempFolder();
            var destination = Path.Combine(folder.FullName, "Saved.xlsx");

            // Act
            wrapper.SaveAs(doc, destination);
            
            // Assert
            Assert.IsTrue(File.Exists(destination), "File was not saved");
        }

        [Test]
        public void InsertXml12_PartShouldBeInserted()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            var doc = wrapper.OpenDocument(_sourceFile);

            // Act
            wrapper.ViewModel.InsertXml12();

            // Assert
            Assert.AreEqual(1, doc.Children.Count);
            Assert.IsInstanceOf<OfficePartViewModel>(doc.Children[0]);
            Assert.AreEqual(XmlPart.RibbonX12, ((OfficePartViewModel)doc.Children[0]).Part!.PartType);
        }

        [Test]
        public void InsertXml14_PartShouldBeInserted()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            var doc = wrapper.OpenDocument(_sourceFile);

            // Act
            wrapper.ViewModel.InsertXml14();

            // Assert
            Assert.AreEqual(1, doc.Children.Count);
            Assert.IsInstanceOf<OfficePartViewModel>(doc.Children[0]);
            Assert.AreEqual(XmlPart.RibbonX14, ((OfficePartViewModel)doc.Children[0]).Part!.PartType);
        }

        [Test]
        public void InsertXml12_PartAlreadyExists_ShouldNotBeInserted()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            var (doc, _) = wrapper.OpenAndInsertPart(_sourceFile, XmlPart.RibbonX12, false);

            // Act
            wrapper.ViewModel.InsertXml12();

            // Assert
            Assert.AreEqual(1, doc.Children.Count, "Part was inserted twice");
        }
        
        [Test]
        public void InsertXml14_PartAlreadyExists_ShouldNotBeInserted()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            var (doc, _) = wrapper.OpenAndInsertPart(_sourceFile, XmlPart.RibbonX14, false);

            // Act
            wrapper.ViewModel.InsertXml14();

            // Assert
            Assert.AreEqual(1, doc.Children.Count, "Part was inserted twice");
        }

        [Test]
        public void Remove_PartSelected_ShouldBeRemoved()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            var (doc, _) = wrapper.OpenAndInsertPart(_sourceFile);

            // Act
            wrapper.ViewModel.Remove();

            // Assert
            Assert.IsEmpty(doc.Children, "Part was not removed");
        }

        [Test]
        public void InsertIcons_SingleChosen_ShouldBeInserted()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            var (_, part) = wrapper.OpenAndInsertPart(_sourceFile);

            // Act
            wrapper.InsertIcons(part, _undoIcon);

            // Assert
            Assert.AreEqual(1, part.Children.Count);
            Assert.AreEqual("undo", ((IconViewModel)part.Children[0]).Name);
        }

        [Test]
        public void InsertIcons_MultipleChosen_AllIconsShouldBeInserted()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            var (_, part) = wrapper.OpenAndInsertPart(_sourceFile);

            // Act
            wrapper.InsertIcons(part, _undoIcon, _redoIcon);

            // Assert
            Assert.AreEqual(2, part.Children.Count);
            Assert.AreEqual("undo", ((IconViewModel)part.Children[0]).Name);
            Assert.AreEqual("redo", ((IconViewModel)part.Children[1]).Name);
        }

        [Test]
        public void Remove_IconSelected_ShouldBeRemoved()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            var (_, part) = wrapper.OpenAndInsertPart(_sourceFile);
            wrapper.InsertIcons(part, _undoIcon);
            Assume.That(part.Children, Is.Not.Empty, "Icon was not inserted");
            wrapper.ViewModel.SelectedItem = part.Children[0];

            // Act
            wrapper.ViewModel.Remove();

            // Assert
            Assert.IsEmpty(part.Children, "Icon was not removed");
        }

        /// <summary>
        /// Checks if a warning is shown after inserting a part in a document and then trying to close it
        /// </summary>
        [Test]
        public void CloseDocument_PartWasInserted_ShouldGiveWarningMessage()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            var doc = wrapper.OpenDocument(_sourceFile);
            wrapper.ViewModel.InsertXml12();

            // Act / assert
            wrapper.AssertMessage(wrapper.ViewModel.CloseDocument, MessageBoxImage.Warning, MessageBoxResult.Cancel, "Insert XML not detected as change");
        }
        
        /// <summary>
        /// Checks if a warning is shown when a part is removed
        /// </summary>
        [Test]
        public void RemovePart_ShouldGiveWarningMessage()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            var doc = wrapper.OpenDocument(_sourceFile);
            wrapper.ViewModel.InsertXml12();
            var part = doc.Children.FirstOrDefault(p => p is OfficePartViewModel);
            Assume.That(part, Is.Not.Null, "No Office part available");
            wrapper.ViewModel.SelectedItem = part;

            // Act / assert
            wrapper.AssertMessage(wrapper.ViewModel.Remove, MessageBoxImage.Warning, MessageBoxResult.Yes);
        }

        /// <summary>
        /// Checks if a warning is shown when a part is removed and the document is about to be closed
        /// </summary>
        [Test]
        public void CloseDocument_PartWasRemoved_GiveWarningMessage()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            var doc = wrapper.OpenDocument(_sourceFile);
            wrapper.ViewModel.InsertXml12();
            var part = doc.Children.FirstOrDefault(p => p is OfficePartViewModel);
            Assume.That(part, Is.Not.Null, "No Office part available");
            wrapper.ViewModel.SelectedItem = part;

            // Act
            wrapper.ViewModel.Remove();

            // Assert
            Assert.IsTrue(doc.HasUnsavedChanges, "No unsaved changes detected after removing a part");
            wrapper.AssertMessage(wrapper.ViewModel.CloseDocument, MessageBoxImage.Warning, MessageBoxResult.Cancel);
        }

        /// <summary>
        /// Checks if a warning is shown when removing an icon and when closing the document after that
        /// </summary>
        [Test]
        public void CloseDocument_IconsRemoved_ShouldShowWarning()
        {
            using var wrapper = new MainWindowViewModelWrapper();
            var (doc, part) = wrapper.OpenAndInsertPart(_sourceFile);

            // Insert an icon and save the document
            wrapper.InsertIcons(part, _redoIcon);

            using var folder = new TempFolder();
            var destination = Path.Combine(folder.FullName, "Output.xlsx");
            wrapper.SaveAs(doc, destination);
            Assert.IsFalse(doc.HasUnsavedChanges, "The icon insertion was apparently not saved");

            // Remove it and do the appropriate checks
            wrapper.ViewModel.SelectedItem = doc.Children.FirstOrDefault(c => c is OfficePartViewModel)?.Children.FirstOrDefault(c => c is IconViewModel);
            Assert.IsNotNull(wrapper.ViewModel.SelectedItem, "Icon was apparently not created");
            wrapper.AssertMessage(wrapper.ViewModel.Remove, MessageBoxImage.Warning, MessageBoxResult.Yes);
            Assert.IsTrue(doc.HasUnsavedChanges, "No unsaved changes detected after removing a part");
            wrapper.AssertMessage(wrapper.ViewModel.CloseDocument, MessageBoxImage.Warning, MessageBoxResult.Cancel);
        }

        /// <summary>
        /// Checks if the XML validation provides the expected result for a few sample cases
        /// </summary>
        [Test]
        public void ValidateXml_ShouldProduceExpectedResult()
        {
            using var wrapper = new MainWindowViewModelWrapper();
            var doc = wrapper.OpenDocument(_sourceFile);
            Assume.That(wrapper.ViewModel.SelectedItem?.CanHaveContents, Is.False);

            wrapper.ViewModel.InsertXml12();
            wrapper.ViewModel.SelectedItem = doc.Children[0];
            Assert.IsTrue(wrapper.ViewModel.SelectedItem.CanHaveContents);

            var tmp = wrapper.ViewModel.OpenPartTab();
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
                wrapper.ViewModel.Validate();
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
        public void GenerateCallbacks_ShouldProduceExpectedCallback()
        {
            using var wrapper = new MainWindowViewModelWrapper();
            var doc = wrapper.OpenDocument(_sourceFile);
            wrapper.ViewModel.InsertXml12();
            var part = doc.Children[0];
            wrapper.ViewModel.SelectedItem = part;

            var tab = wrapper.ViewModel.OpenPartTab();
            Assert.NotNull(tab);

            // This should show a message saying there are no callbacks to be generated
            part.Contents = @"<customUI xmlns=""http://schemas.microsoft.com/office/2006/01/customui""><ribbon></ribbon></customUI>";
            wrapper.AssertMessage(wrapper.ViewModel.GenerateCallbacks, MessageBoxImage.Information);

            // This should contain a single callback for the onLoad event
            part.Contents = @"<customUI onLoad=""CustomLoad"" xmlns=""http://schemas.microsoft.com/office/2006/01/customui""><ribbon></ribbon></customUI>";
            static void Handler(object? o, LaunchDialogEventArgs e)
            {
                Assert.IsTrue(e.Content is CallbackDialogViewModel, $"Unexpected dialog launched: {e.Content.GetType().Name}");
                Assert.IsTrue(((CallbackDialogViewModel) e.Content).Code?.StartsWith("'Callback for customUI.onLoad", StringComparison.OrdinalIgnoreCase), "Expected callback not generated");
            }

            wrapper.ViewModel.LaunchingDialog += Handler;
            wrapper.ViewModel.GenerateCallbacks();
            wrapper.ViewModel.LaunchingDialog -= Handler;  // Just in case we add other checks later
        }

        [Test]
        public void ShowSettings_DialogShouldBeShown()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            IContentDialogBase? content = null;
            wrapper.ViewModel.LaunchingDialog += (o, e) =>
            {
                content = e.Content;
            };

            // Act
            wrapper.ViewModel.ShowSettings();

            // Assert
            Assert.IsInstanceOf(typeof(SettingsDialogViewModel), content);
            var dialog = (SettingsDialogViewModel)content!;
            Assert.IsEmpty(dialog.Tabs);
        }

        [Test]
        public void ShowSettings_WithOpenTabs_TabsShouldBeAssignedToDialog()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            var (_, part) = wrapper.OpenAndInsertPart(_sourceFile);
            wrapper.ViewModel.OpenTabCommand.Execute(part);
            IContentDialogBase? content = null;
            wrapper.ViewModel.LaunchingDialog += (o, e) =>
            {
                content = e.Content;
            };

            // Act
            wrapper.ViewModel.ShowSettings();

            // Assert
            Assert.IsInstanceOf(typeof(SettingsDialogViewModel), content);
            var dialog = (SettingsDialogViewModel) content!;
            Assert.IsNotEmpty(dialog.Tabs);
        }

        [Test]
        public void MultipleTabs_SameNames_TitleShouldIncludeDocumentName()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            var (_, part) = wrapper.OpenAndInsertPart(_sourceFile);
            wrapper.ViewModel.OpenTabCommand.Execute(part);
            var tab = wrapper.ViewModel.SelectedTab;
            Assume.That(tab, Is.Not.Null, "Tab is null");
            var original = tab!.Title;

            // Act / assert

            // We open the same document again (confirming that action first), and check if the title changed
            wrapper.AssertMessage(() => wrapper.OpenAndInsertPart(_sourceFile), MessageBoxImage.Warning, MessageBoxResult.Yes);
            Assert.AreNotEqual(original, tab.Title);

            // We close the newly opened document, and check if the title is back to normal
            wrapper.ViewModel.CloseDocument();
            Assert.AreEqual(original, tab.Title);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CloseTab_TabShouldBeClosed(bool explicitly)
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            var (_, part) = wrapper.OpenAndInsertPart(_sourceFile);
            wrapper.ViewModel.OpenTabCommand.Execute(part);
            var tab = wrapper.ViewModel.SelectedTab;
            Assume.That(tab, Is.Not.Null, "Tab is null");

            // Act
            wrapper.ViewModel.CloseTabCommand.Execute(explicitly ? tab : null);

            // Assert
            Assert.IsEmpty(wrapper.ViewModel.OpenTabs);
        }

        /// <summary>
        /// Checks whether the sample XML files are inserted as we would expect
        /// </summary>
        [Test]
        public void InsertSample_NonExistingPart_ShouldCreatePart()
        {
            using var wrapper = new MainWindowViewModelWrapper();
            var doc = wrapper.OpenDocument(_sourceFile);
            
            var sample = wrapper.ViewModel.XmlSamples?.Items.OfType<XmlSampleViewModel>().First();  // This shouldn't be null; if it is, the test will be a means to detect that too
            wrapper.ViewModel.InsertXmlSampleCommand.Execute(sample);
            var part = doc.Children.First(); // Again, this should not be null

            // It is expected that the part created will be an Office 2007 one, but the templates are for Office 2010+. This gets automatically replaced
            // at insertion time
            var contents = sample?.ReadContents().Replace("2009/07", "2006/01", StringComparison.OrdinalIgnoreCase) ?? string.Empty;
            Assert.AreEqual(contents, part.Contents, "Sample XML file not created with the expected contents");
        }

        /// <summary>
        /// Checks whether a file being saved while opened in another program is detected correctly and does not cause any crash
        /// </summary>
        [Test]
        public void Save_FileInUse_ShouldShowError()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            using var folder = new TempFolder();
            var destination = Path.Combine(folder.FullName, "Output.xlsx");
            File.Copy(_sourceFile, destination);
            var doc = wrapper.OpenDocument(destination, true);

            // Act / assert: Open the same file in exclusive mode
            using (File.Open(destination, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                wrapper.AssertMessage(() => wrapper.ViewModel.SaveCommand.Execute(null), MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Checks whether a file being saved with the Save As... option while opened in another program is detected correctly and does not cause any crash
        /// </summary>
        [Test]
        public void SaveAs_FileInUse_ShouldShowError()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            var doc = wrapper.OpenDocument(_sourceFile);

            using var folder = new TempFolder();
            var destination = Path.Combine(folder.FullName, "Output.xlsx");
            wrapper.SaveAs(doc, destination);

            // Act / assert: Open the same file in exclusive mode
            using (File.Open(destination, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                wrapper.AssertMessage(() => wrapper.SaveAs(doc, destination), MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Checks whether a file being saved while opened in another program is detected correctly and does not cause any crash
        /// </summary>
        [Test]
        public void SaveAll_FileInUse_ShouldShowError()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            using var folder = new TempFolder();
            var destination = Path.Combine(folder.FullName, "Output.xlsx");
            File.Copy(_sourceFile, destination);
            var doc = wrapper.OpenDocument(destination, true);

            // Act / assert: Open the same file in exclusive mode
            using (File.Open(destination, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                wrapper.AssertMessage(() => wrapper.ViewModel.SaveAll(), MessageBoxImage.Error);
            }
        }

        [Test]
        public void OpenHelpLink_ExternalLinkShgouldBeLaunched()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();

            var triggered = false;
            wrapper.OpenExternalAction = x => triggered = true;

            foreach (var pair in wrapper.ViewModel.HelpLinks)
            {
                triggered = false;

                // Act
                wrapper.ViewModel.OpenHelpLinkCommand.Execute(pair.Value);

                // Assert
                Assert.True(triggered);
            }
        }

        [Test]
        public void OnLoaded_NoRedist_CancelWarning_ShouldCloseApp()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            wrapper.RedistributableDetails.NeedsDownload = true;

            // Act / assert
            wrapper.AssertMessage(() => Assert.IsFalse(wrapper.ViewModel.OnLoaded()), MessageBoxImage.Warning, MessageBoxResult.Cancel);
        }

        [Test]
        public void OnLoaded_NoRedist_YesOnWarning_ShouldOpenDownloadLink()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            wrapper.RedistributableDetails.NeedsDownload = true;
            Uri? openedUri = null;
            wrapper.OpenExternalAction = uri => openedUri = uri;

            // Act
            wrapper.AssertMessage(() => Assert.IsTrue(wrapper.ViewModel.OnLoaded()), MessageBoxImage.Warning, MessageBoxResult.Yes);

            // Assert
            Assert.AreEqual(wrapper.RedistributableDetails.DownloadLink, openedUri, "Unexpected download link");
        }

        [Test]
        public void OnLoaded_NoRedist_NoOnWarning_ShouldStartNormally()
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            wrapper.RedistributableDetails.NeedsDownload = true;
            Uri? openedUri = null;
            wrapper.OpenExternalAction = uri => openedUri = uri;

            // Act
            wrapper.AssertMessage(() => Assert.IsTrue(wrapper.ViewModel.OnLoaded()), MessageBoxImage.Warning, MessageBoxResult.No);

            // Assert
            Assert.IsNull(openedUri, "Download link was still launched");
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
            using var wrapper = new MainWindowViewModelWrapper();
            var e = CreateDragData(dataFormat, existingFile, forceReturnNull);

            // Act
            wrapper.ViewModel.PreviewDragEnterCommand.Execute(e);
            
            // Assert
            return e.Handled;
        }

        [Test]
        [TestCaseSource(nameof(DragData))]
        public bool TestDrop(string dataFormat, bool existingFile, bool forceReturnNull)
        {
            // Arrange
            using var wrapper = new MainWindowViewModelWrapper();
            var e = CreateDragData(dataFormat, existingFile, forceReturnNull);

            // Act
            wrapper.ViewModel.DropCommand.Execute(e);

            // Assert
            return wrapper.ViewModel.DocumentList.Count > 0;
        }

        private DragData CreateDragData(string dataFormat, bool existingFile, bool forceReturnNull)
        {
            object? data;
            if (forceReturnNull)
            {
                data = null;
            }
            else if (existingFile)
            {
                data = new[] { _sourceFile };
            }
            else
            {
                data = new[] { "abcd.efgh" };
            }

            return ContainerWrapper.CreateDragData(dataFormat, data);
        }
    }
}