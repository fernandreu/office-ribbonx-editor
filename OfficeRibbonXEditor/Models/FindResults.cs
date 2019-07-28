using System.Collections.Generic;
using System.Drawing;
using OfficeRibbonXEditor.Interfaces;
using ScintillaNET;

namespace OfficeRibbonXEditor.Models
{
    public class FindResults : IResultCollection
    {
        public FindResults(IEnumerable<CharacterRange> items)
        {
            this.Items = new List<CharacterRange>(items);
        }

        public string Header { get; } = "Find Results";

        public List<CharacterRange> Items { get; }

        public void AddToPanel(Scintilla editor, Scintilla resultsPanel)
        {
            resultsPanel.ClearAll();

            var indicator = resultsPanel.Indicators[16];
            indicator.ForeColor = Color.Red;
            indicator.Alpha = 100;
            indicator.Style = IndicatorStyle.RoundBox;
            indicator.Under = true;
            resultsPanel.IndicatorCurrent = indicator.Index;

            //Write lines
            foreach (var item in this.Items)
            {
                var startLine = editor.LineFromPosition(item.cpMin);
                var endLine = editor.LineFromPosition(item.cpMax);

                if (startLine == endLine)
                {
                    var resultsLinePrefix = $"Line {startLine + 1}: ";

                    resultsPanel.AppendText($"{resultsLinePrefix}{editor.Lines[startLine].Text}");
                }
            }

            //Highlight
            var resultLineIndex = 0;
            foreach (var item in this.Items)
            {
                var startLine = editor.LineFromPosition(item.cpMin);
                var endLine = editor.LineFromPosition(item.cpMax);

                if (startLine == endLine)
                {
                    var resultsLinePrefix = $"Line {startLine + 1}: ";

                    var linePos = editor.Lines[startLine].Position;
                    var startPosInLine = item.cpMin - linePos;

                    var lastLineStartPos = resultsPanel.Lines[resultLineIndex].Position;

                    resultsPanel.IndicatorFillRange(lastLineStartPos + resultsLinePrefix.Length + startPosInLine, item.cpMax - item.cpMin);

                    resultLineIndex++;
                }
            }
        }

        public void GoToPosition(int pos, Scintilla editor, Scintilla resultsPanel)
        {
            var selectedLine = resultsPanel.LineFromPosition(pos);

            var charRange = this.Items[selectedLine];
            editor.GotoPosition(charRange.cpMax);
            editor.GotoPosition(charRange.cpMin);
            editor.SetSelection(charRange.cpMin, charRange.cpMax);
            editor.ScrollCaret();
            editor.Focus();
        }
    }
}
