using FlaUI.Core.AutomationElements;
using OfficeRibbonXEditor.UITests.Models;

namespace OfficeRibbonXEditor.UITests.Extensions
{
    public static class AutomationElementExtensions
    {
        public static DocumentItem? AsDocumentItem(this AutomationElement self)
        {
            return self != null ? new DocumentItem(self.FrameworkAutomationElement) : null;
        }

        public static Tree? FindTreeView(this AutomationElement self)
        {
            return self?.FindFirstChild(x => x.ByAutomationId("TreeView")).AsTree();
        }

        public static Tab? FindTabView(this AutomationElement self)
        {
            return self?.FindFirstChild(x => x.ByAutomationId("TabView")).AsTab();
        }
    }
}
