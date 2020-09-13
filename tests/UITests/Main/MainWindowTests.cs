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
        private readonly string sourceFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/Blank.xlsx");

        // TODO: Test save as command, then check if file was created as destFile
        //private readonly string destFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Output/BlankSaved.xlsx");

        // The initialization is actually not needed because it is again done in SetUp, but it is a clean way of
        // removing a warning due to uninitialized non-null fields. The alternative is via pragma
        private AppManager manager = new AppManager();

        [SetUp]
        public void SetUp()
        {
            this.manager = new AppManager();
        }

        [TearDown]
        public void TearDown()
        {
            this.manager.Dispose();
        }

        [Test]
        public void CanOpenWindow()
        {
            // Arrange / act
            this.manager.Launch();

            // Assert
            Assert.AreEqual("Office RibbonX Editor", this.manager.Window?.Title);
        }

        [Test]
        public void CanOpenDocumentFromExecutable()
        {
            // Arrange / act
            this.manager.Launch(sourceFile);
            var treeView = this.manager.Window!.FindTreeView();
            Assume.That(treeView, Is.Not.Null);

            // Assert
            Assert.AreEqual(1, treeView!.Items.Length);
            var item = treeView!.Items.First().AsDocumentItem();

            Assert.AreEqual(Path.GetFileName(sourceFile), item!.Title);
        }

        [Test]
        public void CanInsertSample()
        {
            // Arrange / act
            this.manager.Launch(sourceFile);
            this.manager.Window?.FindTreeView()?.Items.First().Select();
            var menu = this.manager.Window?.FindFirstChild(cf => cf.Menu()).AsMenu();
            Assert.NotNull(menu);
            var tabView = this.manager.Window?.FindTabView();
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
            this.manager.Launch();
            var helpMenu = this.manager.HelpMenu;
            Assume.That(helpMenu, Is.Not.Null, "Missing Help menu");
            helpMenu!.Click();
            var aboutMenu = helpMenu.FindFirstDescendant(x => x.ByText("About"), TimeSpan.FromSeconds(1));
            Assume.That(aboutMenu, Is.Not.Null, "Missing About menu");

            // Act
            aboutMenu!.Click();

            // Assert
            var aboutDialog = this.manager.Window?.FindFirstModalWindow();
            Assert.NotNull(aboutDialog, "About dialog not launched");
        }

        [Test]
        public void FileAppearsInRecentList()
        {
            // Arrange : open a file and close it so that it appears in the recent file list
            this.manager.Launch(sourceFile);
            var treeView = this.manager.Window!.FindTreeView();
            Assume.That(treeView, Is.Not.Null);
            treeView!.Items.First().Click();

            var fileMenu = this.manager.FileMenu;
            Assume.That(fileMenu, Is.Not.Null, "Missing File menu");
            fileMenu!.Click();

            var closeDocumentEntry = fileMenu.FindFirstDescendant(x => x.ByText("Close Current Document"), TimeSpan.FromSeconds(1));
            Assume.That(closeDocumentEntry, Is.Not.Null, "Missing Close Current Document menu");
            closeDocumentEntry!.Click();
            this.manager.App!.WaitWhileBusy();
            Assume.That(treeView!.Items, Is.Empty);

            var fileName = Path.GetFileName(sourceFile);
            Assume.That(fileName, Is.Not.Null, "Cannot get filename");

            // Act / assert: check if the file has been added to the recent list, and open it
            fileMenu!.Click();
            var entry = Retry.WhileNull(() => fileMenu.FindAllDescendants().FirstOrDefault(
                    x => x.Name.StartsWith("1: ", StringComparison.OrdinalIgnoreCase) && x.Name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)))
                .Result;
            Assert.NotNull(entry, "Recent file entry for opened file not found");
            entry.Click();
            this.manager.App.WaitWhileBusy();
            Assert.IsNotEmpty(treeView.Items, "Recent file not opened");
        }
    }
}
