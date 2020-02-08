using System.IO;
using System.Reflection;
using FlaUI.Core.AutomationElements;
using NUnit.Framework;
using OfficeRibbonXEditor.UITests.Helpers;
using OfficeRibbonXEditor.UITests.Models;

namespace OfficeRibbonXEditor.UITests.Extensions
{
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

        public static Tree? FindTreeView(this AutomationElement self)
        {
            return self?.FindFirstChild(x => x.ByAutomationId("TreeView")).AsTree();
        }

        public static Tab? FindTabView(this AutomationElement self)
        {
            return self?.FindFirstChild(x => x.ByAutomationId("TabView")).AsTab();
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
    }
}
