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
        OfficeDocument _document;
        public OfficeDocument Document => _document;

        public string Name => Path.GetFileName(_document.Name);

        public bool HasUnsavedChanges
        {
            get
            {
                foreach (var child in Children)
                {
                    if (child is OfficePartViewModel part)
                    {
                        if (part.HasUnsavedChanges) return true;
                    }
                }
                return false;
            }
        }

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
            : base(null, false, canHaveContents: false)
        {
            _document = document;
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

        /// <summary>
        /// Reloads the associated Office document, but keeping the OfficeParts currently shown in the GUI. This
        /// ensures that, if the files have been modified externally, the program is still looking at their latest
        /// version. Otherwise, we might accidentally lose those external changes when saving
        /// </summary>
        public void Reload()
        {
            // Store the file name (otherwise, it will have been erased after calling Dispose)
            var fileName = _document.Name;

            // Dispose current document (not needed as references to its parts are stored in their view models anyway)
            _document.Dispose();

            // Then, reload it
            _document = new OfficeDocument(fileName);
            
            // Delete all its original parts
            foreach (XMLParts type in Enum.GetValues(typeof(XMLParts)))
                _document.RemoveCustomPart(type);

            // Instead, use the parts currently shown in the editor
            foreach (var child in Children)
            {
                if (!(child is OfficePartViewModel part)) continue;  // TODO: Remember processing pictures here
                _document.SaveCustomPart(part.Part.PartType, part.OriginalContents, true);
            }

            // Technically, there should be no need to re-map anything else in models / underlying documents / parts. Once parts
            // are saved in the document, it will automatically hold a reference to those. The part's view model should not care
        }

        public void Save(bool reloadFirst = false, string fileName = null)
        {
            if (reloadFirst) Reload();

            // Save each individual part
            foreach (var child in Children)
                if (child is OfficePartViewModel part)
                    part.Save();
            // Now save the actual document
            Document.Save(fileName);
        }
    }
}
