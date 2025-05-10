using System.Collections.Generic;
using System.Drawing;
using OfficeRibbonXEditor.Interfaces;
using ScintillaNET;

namespace OfficeRibbonXEditor.Helpers;

public class FindResults : IResultCollection
{
    public FindResults(IEnumerable<CharacterRange> items)
    {
        Items = new List<CharacterRange>(items);
    }

    public string Header { get; } = "Find Results";

    public List<CharacterRange> Items { get; }

    public bool IsEmpty => Items.Count == 0;

    public int Count => Items.Count;

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
        foreach (var item in Items)
        {
            var startLine = editor.LineFromPosition(item.MinPosition);
            var endLine = editor.LineFromPosition(item.MaxPosition);

            if (startLine == endLine)
            {
                var resultsLinePrefix = $"Line {startLine + 1}: ";

                resultsPanel.AppendText($"{resultsLinePrefix}{editor.Lines[startLine].Text}");
            }
        }

        //Highlight
        var resultLineIndex = 0;
        foreach (var item in Items)
        {
            var startLine = editor.LineFromPosition(item.MinPosition);
            var endLine = editor.LineFromPosition(item.MaxPosition);

            if (startLine == endLine)
            {
                var resultsLinePrefix = $"Line {startLine + 1}: ";

                var linePos = editor.Lines[startLine].Position;
                var startPosInLine = item.MinPosition - linePos;

                var lastLineStartPos = resultsPanel.Lines[resultLineIndex].Position;

                resultsPanel.IndicatorFillRange(lastLineStartPos + resultsLinePrefix.Length + startPosInLine, item.MaxPosition - item.MinPosition);

                resultLineIndex++;
            }
        }
    }

    public void GoToPosition(int pos, Scintilla editor, Scintilla resultsPanel)
    {
        var selectedLine = resultsPanel.LineFromPosition(pos);

        var charRange = Items[selectedLine];
        editor.GotoPosition(charRange.MaxPosition);
        editor.GotoPosition(charRange.MinPosition);
        editor.SetSelection(charRange.MinPosition, charRange.MaxPosition);
        editor.ScrollCaret();
        editor.Focus();
    }
}