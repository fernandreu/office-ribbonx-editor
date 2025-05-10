using System;
using System.IO;
using System.Reflection;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Tools;
using NUnit.Framework;
using OfficeRibbonXEditor.UITests.Helpers;
using OfficeRibbonXEditor.UITests.Models;

namespace OfficeRibbonXEditor.UITests.Extensions;

public static class AutomationElementExtensions
{
    public static DocumentItem? AsDocumentItem(this AutomationElement self)
    {
        return self != null ? new DocumentItem(self.FrameworkAutomationElement) : null;
    }

    public static Scintilla? AsScintilla(this AutomationElement self)
    {
        return self != null ? new Scintilla(self.FrameworkAutomationElement) : null;
    }

    public static Tree? FindTreeView(this AutomationElement self, TimeSpan? timeout = null)
    {
        return Retry.WhileNull(() => self?.FindFirstChild("TreeView"), timeout).Result.AsTree();
    }

    public static Tab? FindTabView(this AutomationElement self, TimeSpan? timeout = null)
    {
        return Retry.WhileNull(() => self?.FindFirstChild("TabView"), timeout).Result.AsTab();
    }

    /// <summary>
    /// Captures a screenshot of the <see cref="AutomationElement"/> and adds it to the <see cref="TestContext"/> as an attachment. 
    /// </summary>
    /// <param name="self">The <see cref="AutomationElement"/> to be captured</param>
    /// <param name="fileName">The name to be given to the screenshot</param>
    /// <param name="description">The description to be added when attaching the screenshot to the <see cref="TestContext"/></param>
    public static void TestCapture(this AutomationElement self, string fileName, string description)
    {
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, fileName);
        var image = FlaUI.Core.Capturing.Capture.Element(self);
        image.ToFile(path);
        TestContext.AddTestAttachment(path, description);
    }

    public static AutomationElement? FindFirstDescendant(this AutomationElement self,  TimeSpan timeout)
    {
        return Retry.WhileNull(() => self?.FindFirstDescendant(), timeout).Result;
    }

    public static AutomationElement? FindFirstDescendant(this AutomationElement self, ConditionBase condition, TimeSpan timeout)
    {
        return Retry.WhileNull(() => self?.FindFirstDescendant(condition), timeout).Result;
    }

    public static AutomationElement? FindFirstDescendant(this AutomationElement self, Func<ConditionFactory, ConditionBase> conditionFunc, TimeSpan timeout)
    {
        return Retry.WhileNull(() => self?.FindFirstDescendant(conditionFunc), timeout).Result;
    }

    public static AutomationElement? FindFirstDescendant(this AutomationElement self, string automationId, TimeSpan timeout)
    {
        return Retry.WhileNull(() => self?.FindFirstDescendant(automationId), timeout).Result;
    }
}