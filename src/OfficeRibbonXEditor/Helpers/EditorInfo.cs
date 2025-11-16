namespace OfficeRibbonXEditor.Helpers;

public class EditorInfo(string text, Tuple<int, int> selection)
{
    /// <summary>
    /// Gets or sets the current text shown in the editor
    /// </summary>
    public string Text { get; set; } = text;

    /// <summary>
    /// Gets or sets the current selection in the editor, as a (start, end) tuple containing the corresponding indices
    /// </summary>
    public Tuple<int, int> Selection { get; set; } = selection;
}