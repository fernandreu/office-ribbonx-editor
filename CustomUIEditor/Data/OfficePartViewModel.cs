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
        string _originalContents;
        public string OriginalContents => _originalContents;

        public OfficePart Part { get; }

        public bool HasUnsavedChanges => _originalContents != Contents;

        public string Name => Part.Name;

        public string ImageSource => "/Resources/xml.png";  // TODO: That's probably not the only one possible

        public OfficePartViewModel(OfficePart part, OfficeDocumentViewModel parent) 
            : base(parent, false, contents: part.ReadContent())
        {
            Part = part;
            _originalContents = Contents;
        }

        protected override void SelectionLost()
        {
            // I rather prefer not saving them automatically, but only on request. That way it is also easier to check whether there are unsaved changes at any point
            //if (HasUnsavedChanges)
            //    SaveChanges();
        }

        public void Save()
        {
            // Do not simply call Part.Save because that will not flag the part as dirty in the parent document
            var docModel = (OfficeDocumentViewModel) Parent;
            docModel.Document.SaveCustomPart(Part.PartType, Contents);
            _originalContents = Contents;
        }
    }
}
