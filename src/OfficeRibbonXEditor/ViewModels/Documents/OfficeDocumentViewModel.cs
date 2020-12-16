using System;
using System.IO;
using System.Linq;
using OfficeRibbonXEditor.Documents;

namespace OfficeRibbonXEditor.ViewModels.Documents
{
    public class OfficeDocumentViewModel : TreeViewItemViewModel, IDisposable
    {
        private OfficeDocument document;

        private bool partsAddedOrRemoved;

        private bool disposed;

        public OfficeDocumentViewModel(OfficeDocument document) 
            : base(null, false, false)
        {
            this.document = document;
            this.LoadParts();
        }

        public OfficeDocument Document => this.document;

        public override string Name => Path.GetFileName(this.document.Name);

        /// <summary>
        /// Gets a value indicating whether any of the parts of this document has unsaved changes.
        /// </summary>
        public bool HasUnsavedChanges
        {
            get
            {
                if (this.partsAddedOrRemoved)
                {
                    return true;
                }

                foreach (var child in this.Children)
                {
                    if (child is OfficePartViewModel part && part.HasUnsavedChanges)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public string? ImageSource  // TODO: Use the actual ImagesResource file somehow
        {
            get
            {
                switch (this.document.FileType)
                {
                    case OfficeApplication.Excel:
                        return "/Resources/Images/excelwkb.png";
                    case OfficeApplication.PowerPoint:
                        return "/Resources/Images/pptpre.png";
                    case OfficeApplication.Word:
                        return "/Resources/Images/worddoc.png";
                    case OfficeApplication.Visio:
                        return "/Resources/Images/visiodoc.png";
                    case OfficeApplication.Xml:
                        return "/Resources/Images/xml.png";
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Reloads the associated Office document, but keeping the OfficeParts currently shown in the GUI. This
        /// ensures that, if the files have been modified externally, the program is still looking at their latest
        /// version. Otherwise, we might accidentally lose those external changes when saving
        /// </summary>
        public void Reload()
        {
            // Store the file name (otherwise, it will have been erased after calling Dispose)
            var fileName = this.document.Name;

            // Dispose current document (not needed as references to its parts are stored in their View models anyway)
            this.document.Dispose();

            // Then, reload it
            this.document = new OfficeDocument(fileName);
            
            // Delete all its original parts
            foreach (var type in Enum.GetValues(typeof(XmlPart)).OfType<XmlPart>())
            {
                this.document.RemoveCustomPart(type);
            }

            // Instead, use the parts currently shown in the editor
            foreach (var part in this.Children.OfType<OfficePartViewModel>())
            {
                if (part.Part == null)
                {
                    continue;
                }

                this.document.SaveCustomPart(part.Part.PartType, part.OriginalContents, true);
                
                // Re-map the Part. This ensures that the PackagePart stored internally in OfficePart points to
                // the right location, in case it is needed
                part.Reload();
            }
        }

        public void Save(bool reloadFirst = false, string? fileName = null, bool preserveAttributes = true)
        {
            if (reloadFirst)
            {
                this.Reload();
            }

            // Save each individual part
            foreach (var part in this.Children.OfType<OfficePartViewModel>())
            {
                part.Save();
            }

            // Now save the actual document
            this.document.Save(fileName, preserveAttributes);

            // The save operation closes the internal package and re-opens it. Hence, parts need to be re-mapped
            foreach (var part in this.Children.OfType<OfficePartViewModel>())
            {
                if (part.Part == null)
                {
                    continue;
                }

                part.Part = this.document.RetrieveCustomPart(part.Part.PartType);
            }

            this.partsAddedOrRemoved = false;
        }

        public void InsertPart(XmlPart type)
        {
            // Check if the part does not exist yet
            var part = this.document.RetrieveCustomPart(type);
            if (part != null)
            {
                return;
            }

            part = this.document.CreateCustomPart(type);
            var partModel = new OfficePartViewModel(part, this);
            this.Children.Add(partModel);
            partModel.IsSelected = true;
            this.partsAddedOrRemoved = true;
        }

        public void RemovePart(XmlPart type)
        {
            this.document.RemoveCustomPart(type);
            this.partsAddedOrRemoved = true;

            for (var i = 0; i < this.Children.Count; ++i)
            {
                if (!(this.Children[i] is OfficePartViewModel part) || part.Part == null)
                {
                    continue;
                }

                if (part.Part.PartType == type)
                {
                    this.Children.RemoveAt(i);
                    return;
                }
            }
        }

        protected override void LoadChildren()
        {
            this.LoadParts();
        }

        private void LoadParts()
        {
            foreach (var part in this.document.Parts ?? Enumerable.Empty<OfficePart>())
            {
                this.Children.Add(new OfficePartViewModel(part, this));
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.document?.Dispose();
            }

            this.disposed = true;
        }
    }
}
