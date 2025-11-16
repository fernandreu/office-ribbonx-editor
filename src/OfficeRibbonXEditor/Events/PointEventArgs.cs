using System.Drawing;

namespace OfficeRibbonXEditor.Events;

public class PointEventArgs : EventArgs
{
    public PointEventArgs()
    {
    }

    public PointEventArgs(Point data)
    {
        Data = data;
    }

    public Point Data { get; set; }
}