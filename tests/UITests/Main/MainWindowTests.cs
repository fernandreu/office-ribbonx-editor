using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using NUnit.Framework;
using OfficeRibbonXEditor.UITests.Extensions;
using OfficeRibbonXEditor.UITests.Helpers;
using OfficeRibbonXEditor.Views.Controls;

namespace OfficeRibbonXEditor.UITests.Main
{
    [TestFixture]
    [SingleThreaded]
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown")]
    public class MainWindowTests
    {
        private readonly string _sourceFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/Blank.xlsx");

        // TODO: Test save as command, then check if file was created as destFile
        //private readonly string destFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Output/BlankSaved.xlsx");

        // The initialization is actually not needed because it is again done in SetUp, but it is a clean way of
        // removing a warning due to uninitialized non-null fields. The alternative is via pragma
        private AppManager _manager = new AppManager();

        [SetUp]
        public void SetUp()
        {
            _manager = new AppManager();
        }

        [TearDown]
        public void TearDown()
        {
            _manager.Dispose();
        }

        [Test]
        public void CanOpenWindow()
        {
            // Arrange / act
            _manager.Launch();

            // Assert
            Assert.AreEqual("Office RibbonX Editor", _manager.Window?.Title);
        }

        [Test]
        public void CanOpenDocumentFromExecutable()
        {
            // Arrange / act
            _manager.Launch(_sourceFile);
            var treeView = _manager.Window!.FindTreeView();
            Assume.That(treeView, Is.Not.Null);

            // Assert
            Assert.AreEqual(1, treeView!.Items.Length);
            var item = treeView!.Items.First().AsDocumentItem();

            Assert.AreEqual(Path.GetFileName(_sourceFile), item!.Title);
        }

        [Test]
        public void CanInsertSample()
        {
            // Arrange / act
            _manager.Launch(_sourceFile);
            _manager.Window?.FindTreeView()?.Items.First().Select();
            var menu = _manager.Window?.FindFirstChild(cf => cf.Menu()).AsMenu();
            Assert.NotNull(menu);
            var tabView = _manager.Window?.FindTabView();
            var tabItem = tabView?.FindFirstChild(x => x.ByClassName(nameof(EditorTab)));
            Assert.Null(tabItem);

            var insertMenu = menu!.Items.First(x => x.AutomationId == "Insert");
            var sampleMenu = insertMenu.Items.First(x => x.AutomationId == "SampleXml");
            var sampleEntry = sampleMenu.Items[0];
            sampleEntry?.Invoke();

            tabItem = tabView?.FindFirstChild(x => x.ByClassName(nameof(EditorTab)));
            Assert.NotNull(tabItem);
        }

        [Test]
        public void CanShowAboutDialog()
        {
            // Arrange
            _manager.Launch();
            var helpMenu = _manager.HelpMenu;
            Assume.That(helpMenu, Is.Not.Null, "Missing Help menu");
            helpMenu!.Click();
            var aboutMenu = helpMenu.FindFirstDescendant(x => x.ByText("About"), TimeSpan.FromSeconds(1));
            Assume.That(aboutMenu, Is.Not.Null, "Missing About menu");

            // Act
            aboutMenu!.Click();

            // Assert
            var aboutDialog = _manager.Window?.FindFirstModalWindow();
            Assert.NotNull(aboutDialog, "About dialog not launched");
        }

        [Test]
        public void FileAppearsInRecentList()
        {
            // Arrange : open a file and close it so that it appears in the recent file list
            _manager.Launch(_sourceFile);
            var treeView = _manager.Window!.FindTreeView();
            Assume.That(treeView, Is.Not.Null);
            treeView!.Items.First().Click();

            var fileMenu = _manager.FileMenu;
            Assume.That(fileMenu, Is.Not.Null, "Missing File menu");
            fileMenu!.Click();

            var closeDocumentEntry = fileMenu.FindFirstDescendant(x => x.ByText("Close Current Document"), TimeSpan.FromSeconds(1));
            Assume.That(closeDocumentEntry, Is.Not.Null, "Missing Close Current Document menu");
            closeDocumentEntry!.Click();
            _manager.App!.WaitWhileBusy();
            Assume.That(treeView!.Items, Is.Empty);

            var fileName = Path.GetFileName(_sourceFile);
            Assume.That(fileName, Is.Not.Null, "Cannot get filename");

            // Act / assert: check if the file has been added to the recent list, and open it
            fileMenu!.Click();
            var entry = Retry.WhileNull(() => fileMenu.FindAllDescendants().FirstOrDefault(
                    x => x.Name.StartsWith("1: ", StringComparison.OrdinalIgnoreCase) && x.Name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)))
                .Result;
            Assert.NotNull(entry, "Recent file entry for opened file not found");
            entry.Click();
            _manager.App.WaitWhileBusy();
            Assert.IsNotEmpty(treeView.Items, "Recent file not opened");
        }
    }
}
