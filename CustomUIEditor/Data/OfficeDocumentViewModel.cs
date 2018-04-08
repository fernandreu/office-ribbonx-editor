using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomUIEditor.Data
{
    public class OfficeDocumentViewModel : TreeViewItemViewModel
    {
        public OfficeDocument Document { get; }

        public string Name => Path.GetFileName(Document.Name);

        public string ImageSource  // TODO: Use the actual ImagesResource file somehow
        {
            get
            {
                switch (Document.FileType)
                {
                    case OfficeApplications.Excel:
                        return "/Resources/excelwkb.png";
                    case OfficeApplications.PowerPoint:
                        return "/Resources/pptpre.png";
                    case OfficeApplications.Word:
                        return "/Resources/worddoc.png";
                    case OfficeApplications.XML:
                        return "/Resources/xml.png";
                    default:
                        return null;
                }
            }
        }

        public OfficeDocumentViewModel(OfficeDocument document) 
            : base(null, false)
        {
            Document = document;
            LoadParts();
        }

        protected override void LoadChildren()
        {
            LoadParts();
        }

        private void LoadParts()
        {
            foreach (var part in Document.Parts)
                Children.Add(new OfficePartViewModel(part, this));
        }
    }
}
