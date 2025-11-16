namespace OfficeRibbonXEditor.Interfaces;

public interface IFileDialogService
{
    bool? OpenFileDialog(string title, string filter, Action<string> completedAction, string? fileName = null, int filterIndex = 0);

    bool? OpenFilesDialog(string title, string filter, Action<IEnumerable<string>> completedAction, string? fileName = null, int filterIndex = 0);

    bool? SaveFileDialog(string title, string filter, Action<string> completedAction, string? fileName = null, int filterIndex = 0);
}