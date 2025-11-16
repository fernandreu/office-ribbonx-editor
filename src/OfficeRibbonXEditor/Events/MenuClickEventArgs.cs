namespace OfficeRibbonXEditor.Events;

public class MenuClickEventArgs(string filepath) : EventArgs
{
    public string Filepath { get; } = filepath;
}