using FlaUI.Core;
using FlaUI.Core.AutomationElements;

namespace OfficeRibbonXEditor.UITests.Helpers
{
    public class Scintilla : AutomationElement
    {
        public Scintilla(FrameworkAutomationElementBase frameworkAutomationElement) : base(frameworkAutomationElement)
        {
        }

        public string Text
        {
            get => this.Patterns.Value.Pattern.Value.Value;
            set => this.Patterns.Value.Pattern.SetValue(value);
        }
    }
}
