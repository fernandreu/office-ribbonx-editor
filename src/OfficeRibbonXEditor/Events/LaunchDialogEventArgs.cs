using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.Events;

public class LaunchDialogEventArgs(IContentDialogBase content, bool showDialog = true) : EventArgs
{
    public IContentDialogBase Content { get; } = content;

    public bool ShowDialog { get; } = showDialog;
}