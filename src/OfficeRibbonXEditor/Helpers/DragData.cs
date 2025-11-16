using System.Windows;

namespace OfficeRibbonXEditor.Helpers;

public class DragData(IDataObject data)
{
    public IDataObject Data { get; } = data;

    public bool Handled { get; set; }
}