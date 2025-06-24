using System;

namespace OfficeRibbonXEditor.Helpers;

public class EditorInfo
{
    public EditorInfo(string text, Tuple<int, int> selection)
    {
        Text = text;
        Selection = selection;
    }

    /// <summary>
    /// Gets or sets the current text shown in the editor
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the current selection in the editor, as a (start, end) tuple containing the corresponding indices
    /// </summary>
    public Tuple<int, int> Selection { get; set; }
}