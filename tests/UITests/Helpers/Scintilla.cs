using System.Linq;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;

namespace OfficeRibbonXEditor.UITests.Helpers;

public class Scintilla : AutomationElement
{
    public Scintilla(FrameworkAutomationElementBase frameworkAutomationElement) : base(frameworkAutomationElement)
    {
    }

    public string Text
    {
        get => Patterns.Value.Pattern.Value.Value;
        set => Patterns.Value.Pattern.SetValue(value);
    }

    public SelectedTextRange Selection => new SelectedTextRange(Patterns.Text.Pattern, Patterns.Value.Pattern, Patterns.Text.Pattern.GetSelection().First());
}