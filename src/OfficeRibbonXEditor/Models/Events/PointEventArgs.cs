using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfficeRibbonXEditor.Models.Events
{
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
}
