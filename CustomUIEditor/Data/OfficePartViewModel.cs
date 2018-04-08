using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CustomUIEditor.Data
{
    public class OfficePartViewModel : TreeViewItemViewModel
    {
        public OfficePart Part { get; }

        public string Name => Part.Name;

        public string ImageSource => "/Resources/xml.png";  // TODO: That's probably not the only one possible

        public OfficePartViewModel(OfficePart part, OfficeDocumentViewModel parent) 
            : base(parent, false)
        {
            Part = part;
        }
    }
}
