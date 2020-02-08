using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using NUnit.Framework;
using OfficeRibbonXEditor.UITests.Extensions;
using OfficeRibbonXEditor.UITests.Helpers;

namespace OfficeRibbonXEditor.UITests.Main
{
    [TestFixture]
    [SingleThreaded]
    [Apartment(ApartmentState.STA)]
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown")]
    public class EditorTabTests
    {
        private readonly string sourceFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/SingleXml.xlsx");

        private readonly AppManager manager = new AppManager();

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        private TabItem tab;

        private Scintilla editor;

        private string originalCode;
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            this.manager.Launch(sourceFile);
            var tabView = this.manager.Window?.FindTabView();
            Assume.That(tabView, Is.Not.Null);

            this.tab = tabView!.TabItems.FirstOrDefault();
            Assume.That(this.tab, Is.Not.Null, "Editor tab not automatically shown");

            var scintilla = this.tab.FindFirstDescendant("Editor").AsScintilla();
            Assume.That(scintilla, Is.Not.Null);
            this.editor = scintilla!;

            this.originalCode = this.editor.Text;
            Assume.That(this.originalCode, Is.Not.Empty);
        }

        [SetUp] 
        public void SetUp()
        {
            this.editor.Text = this.originalCode;
            this.editor.Click();
            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.HOME);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            this.manager.Dispose();
        }

        [Test]
        public void TestCut()
        {
            // Arrange
            Clipboard.Clear();
            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_A);

            // Act
            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_X);
            WaitForClipboard();

            // Assert
            Assert.AreEqual(originalCode, Clipboard.GetText());
            Assert.IsEmpty(this.editor.Text);
        }

        [Test]
        public void TestCopy()
        {
            // Arrange
            Clipboard.Clear();
            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_A);

            // Act
            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_C);
            WaitForClipboard();

            // Assert
            Assert.AreEqual(this.editor.Text, Clipboard.GetText());
        }

        [Test]
        public void TestPaste()
        {
            // Arrange
            const string pastedText = "$@*@#&$#()"; // Just some random text with no chances to appear in a template
            Clipboard.SetText(pastedText);
            
            // Act
            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_V);
            this.manager.App!.WaitWhileBusy();

            // Assert
            Assert.AreNotEqual(originalCode, this.editor.Text);
            Assert.That(this.editor.Text, Contains.Substring(pastedText));
        }

        [Test]
        public void TestFoldingLevels()
        {
            // Using a for loop instead of method arguments because this avoids calling SetUp for
            // such short actions
            for (var level = 0; level <= 8; ++level)
            {
                // Arrange
                var key = (VirtualKeyShort)((int)VirtualKeyShort.KEY_0 + level);

                // Act
                Keyboard.TypeSimultaneously(VirtualKeyShort.ALT, key); // Folding
                Task.Delay(50).Wait();

                Keyboard.TypeSimultaneously(VirtualKeyShort.ALT, VirtualKeyShort.SHIFT, key); // Unfolding
                Task.Delay(50).Wait();

                // Assert
                // TODO: Find a proper way of checking whether the folding / unfolding actions really did anything
                Assert.AreEqual(originalCode, this.editor.Text);
            }
        }

        [Test]
        public void TestFoldingCurrent()
        {
            // Act
            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.ALT, VirtualKeyShort.KEY_F); // Folding
            Task.Delay(50).Wait();

            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.ALT, VirtualKeyShort.KEY_G); // Unfolding
            Task.Delay(50).Wait();

            // Assert
            // TODO: Find a proper way of checking whether the folding / unfolding actions really did anything
            Assert.AreEqual(originalCode, this.editor.Text);
        }

        private static void WaitForClipboard()
        {
            for (var i = 0; !Clipboard.ContainsText(); ++i)
            {
                if (i > 10)
                {
                    Assert.Fail("Clipboard not set");
                }

                // Do NOT:
                // - await the task, as this causes issues with the STA mode
                // - switch to Thread.Sleep(50), as that does not apparently have the same effect
                Task.Delay(50).Wait();
            }
        }
    }
}
