using System;

namespace OfficeRibbonXEditor.Events;

public class EditorChangeEventArgs : EventArgs
{
    public int Start { get; set; }

    public int End { get; set; } = -1;

    public string NewText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the selection should be set to the exact range that was replaced
    /// </summary>
    public bool UpdateSelection { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the undo history of the editor should be reset. Do this, for example,
    /// when switching to a completely different part
    /// </summary>
    public bool ResetUndoHistory { get; set; }
}