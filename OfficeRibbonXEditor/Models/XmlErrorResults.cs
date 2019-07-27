using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using OfficeRibbonXEditor.Interfaces;
using ScintillaNET;

namespace OfficeRibbonXEditor.Models
{
    public class XmlErrorResults : IResultCollection
    {
        public XmlErrorResults(IEnumerable<XmlError> items)
        {
            this.Items = new List<XmlError>(items);
        }

        public List<XmlError> Items { get; }

        public void AddToPanel(Scintilla editor, Scintilla resultsPanel)
        {
            resultsPanel.ClearAll();
            foreach (var item in this.Items)
            {
                resultsPanel.AppendText($"Ln {item.LineNumber}, Col {item.LinePosition}: {item.Message}\n");
            }

            if (this.Items.Count == 1)
            {
                // Go to that single line immediately

                // There is a chance that the following happens:
                // - The panel is initially hidden
                // - The line to be selected is below the visible region of the editor
                // - The line gets selected before the panel is shown
                // - When shown, the panel hides the line
                // To prevent this from happening, we introduce a small delay to give the editor time to show the panel
                // TODO: Find a more robust way to achieve the same effect

                Task.Delay(100).ContinueWith(t => Application.Current.Dispatcher.Invoke(() => this.GoToPosition(1, editor, resultsPanel)));
            }
        }

        public void GoToPosition(int pos, Scintilla editor, Scintilla resultsPanel)
        {
            var selectedLine = resultsPanel.LineFromPosition(pos);
            var item = this.Items[selectedLine];
            if (item.LineNumber > editor.Lines.Count)
            {
                return;
            }

            var targetLine = editor.Lines[item.LineNumber - 1];
            editor.SetSelection(targetLine.Position, targetLine.Position + targetLine.Length);
            editor.ScrollCaret();
            editor.Focus();
        }
    }
}
