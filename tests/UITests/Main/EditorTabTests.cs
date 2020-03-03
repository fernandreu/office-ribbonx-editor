using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Dragablz;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using NUnit.Framework;
using OfficeRibbonXEditor.UITests.Extensions;
using OfficeRibbonXEditor.UITests.Helpers;
using Window = FlaUI.Core.AutomationElements.Window;

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
            this.editor.Selection.Position = 0;
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

        [Test]
        [TestCase(2, 1, 1)]
        [TestCase(4, 5, 5)]
        [TestCase(3, 10, 3)] // Values outside range should be ignored
        [TestCase(3, 0, 3)] // Values outside range should be ignored
        public void TestGoTo(int originalLine, int newLine, int expected)
        {
            // Arrange
            var textParts = new [] {"This", "text", "has", "five", "lines"};
            this.editor.Text = string.Join(Environment.NewLine, textParts);
            this.editor.Selection.Line = originalLine - 1;

            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_G); // Go To
            Window? dialog = null;
            Retry.WhileNull(() =>
            {
                dialog = this.manager.Window!.FindFirstDescendant(x => x.ByControlType(ControlType.Window)).AsWindow();
                return dialog;
            }, TimeSpan.FromSeconds(1));
            Assert.NotNull(dialog, "No dialog launched");
            Assert.That(dialog!.Title, Does.Match(".*Go To.*"), "Go To dialog not shown");

            var currentLineBox = dialog.FindFirstDescendant("CurrentLineBox").AsTextBox();
            Assert.AreEqual($"{originalLine}", currentLineBox.Text);

            var maximumLineBox = dialog.FindFirstDescendant("MaximumLineBox").AsTextBox();
            Assert.AreEqual($"{textParts.Length}", maximumLineBox.Text);

            // Due to this being a SpinBox, the original AutomationId (TargetBox) is ignored
            var targetBox = dialog.FindFirstDescendant("PART_TextBox").AsTextBox();

            // Act
            targetBox.Text = $"{newLine}";
            dialog.FindFirstDescendant("AcceptButton").Click();

            // Assert
            Assert.AreEqual(expected, this.editor.Selection.Line + 1);
        }

        [Test]
        [TestCase(2)]
        public void TestToggleComments(int line)
        {
            // Arrange
            var textParts = new[] { "This", "text", "has", "five", "lines" };
            this.editor.Text = string.Join(Environment.NewLine, textParts);
            this.editor.Selection.Line = line - 1;

            // Act / Assert

            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.DIVIDE); // Toggle comment
            Assert.That(this.editor.Selection.Text, Does.Match($"<\\!--{textParts[line - 1]}-->.*"));

            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.DIVIDE); // Toggle comment
            Assert.That(this.editor.Selection.Text, Does.Match($"{textParts[line - 1]}.*"));
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
