using System.Windows;

namespace OfficeRibbonXEditor.Helpers;

public class DragData
{
    public DragData(IDataObject data)
    {
        Data = data;
    }

    public IDataObject Data { get; }

    public bool Handled { get; set; }
}