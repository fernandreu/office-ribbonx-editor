using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using FlaUI.Core.AutomationElements;
using NUnit.Framework;
using OfficeRibbonXEditor.UITests.Extensions;
using OfficeRibbonXEditor.UITests.Helpers;

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

            var sampleEntry = menu?.Items["Insert"].Items["Sample XML"].Items[0];
            sampleEntry?.Invoke();

            var tabItem = this.manager.Window?.FindTabView()?.SelectedTabItem;
            Assert.NotNull(tabItem);
        }

        [Test]
        public void CanShowAboutDialog()
        {
            // Arrange
            this.manager.Launch();
            var helpMenu = this.manager.Window?.FindFirstDescendant(x => x.ByText("Help")).AsMenu();
            Assume.That(helpMenu, Is.Not.Null, "Missing Help menu");
            helpMenu!.Click();
            var aboutMenu = helpMenu.FindFirstDescendant(x => x.ByText("About"));
            Assume.That(aboutMenu, Is.Not.Null, "Missing About menu");

            // Act
            aboutMenu!.Click();

            // Assert
            var aboutDialog = this.manager.Window?.ModalWindows.FirstOrDefault();
            Assert.NotNull(aboutDialog, "About dialog not launched");
        }
    }
}
