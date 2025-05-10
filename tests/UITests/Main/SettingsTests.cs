using System;
using System.Diagnostics.CodeAnalysis;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using NUnit.Framework;
using OfficeRibbonXEditor.UITests.Extensions;
using OfficeRibbonXEditor.UITests.Helpers;

namespace OfficeRibbonXEditor.UITests.Main;

[TestFixture]
[SingleThreaded]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown")]
public class SettingsTests
{
    private AppManager? _manager;

    private Window? _dialog;

    [SetUp]
    public void SetUp()
    {
        _manager = new AppManager();
        _manager.Launch();

        _manager.Window!
            .FindFirstChild(x => x.ByClassName("ToolBar"))!
            .FindFirstChild("SettingsButton")!
            .Click();
        _dialog = Retry.WhileNull(
                () => _manager.Window.FindFirstDescendant(x => x.ByClassName("Window")),
                TimeSpan.FromSeconds(1))
            .Result.AsWindow();
    }

    [TearDown]
    public void TearDown()
    {
        _manager!.Dispose();
    }

    [Test]
    public void CanChangeLanguage()
    {
        // Arrange
        var combo = _dialog!.FindFirstDescendant("LanguageCombo").AsComboBox();
        var applyButton = _dialog.FindFirstDescendant("ApplyButton");
        var original = combo!.SelectedItem!.Text;

        // Act
        combo.Select(original.StartsWith("English", StringComparison.OrdinalIgnoreCase) ? 1 : 0);
        applyButton?.Click();

        // Assert
        var exceptionDialog = _manager!.Window!.FindFirstModalWindow(TimeSpan.FromSeconds(1));
        Assert.That(exceptionDialog, Is.Null);
        Assert.That(combo.SelectedItem.Text, Is.Not.EqualTo(original));
    }
}