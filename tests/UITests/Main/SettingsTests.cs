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
    public class SettingsTests
    {
        private AppManager? manager;

        private Window? dialog;

        [SetUp]
        public void SetUp()
        {
            this.manager = new AppManager();
            this.manager.Launch();

            this.manager.Window!
                .FindFirstChild(x => x.ByClassName("ToolBar"))
                .FindFirstChild("SettingsButton")
                .Click();
            this.dialog = Retry.WhileNull(
                    () => this.manager.Window.FindFirstDescendant(x => x.ByClassName("Window")),
                TimeSpan.FromSeconds(1))
                .Result.AsWindow();
        }

        [TearDown]
        public void TearDown()
        {
            this.manager!.Dispose();
        }

        [Test]
        public void CanChangeLanguage()
        {
            // Arrange
            var combo = this.dialog!.FindFirstDescendant("LanguageCombo").AsComboBox();
            var applyButton = this.dialog.FindFirstDescendant("ApplyButton");
            var original = combo.SelectedItem.Text;

            // Act
            combo.Select(original == "English" ? 1 : 0);
            applyButton.Click();

            // Assert
            var exceptionDialog = this.manager!.Window!.FindFirstModalWindow(TimeSpan.FromSeconds(1));
            Assert.Null(exceptionDialog);
            Assert.AreNotEqual(original, combo.SelectedItem.Text);
        }
    }
}
