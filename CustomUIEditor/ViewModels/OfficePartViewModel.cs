// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OfficePartViewModel.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the OfficePartViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.ViewModels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Media.Imaging;
    using System.Xml;

    using Data;

    public class OfficePartViewModel : TreeViewItemViewModel
    {
        private string originalContents;

        private OfficePart part;

        private bool iconsAddedOrRemoved;

        public OfficePartViewModel(OfficePart part, OfficeDocumentViewModel parent) 
            : base(parent, false, contents: part.ReadContent())
        {
            this.part = part;
            this.originalContents = this.Contents;
            this.LoadIcons();
        }

        public string OriginalContents => this.originalContents;

        public OfficePart Part => this.part;

        public bool HasUnsavedChanges => this.originalContents != this.Contents || this.iconsAddedOrRemoved;

        public string Name => this.Part.Name;

        public string ImageSource => "/Resources/xml.png";  // TODO: That's probably not the only one possible

        public void InsertIcon(string filePath, string id = null)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            if (id == null)
            {
                id = fileName == null ? null : XmlConvert.EncodeName(fileName);
            }

            id = this.Part.AddImage(filePath, id);
            Debug.Assert(id != null, "Cannot create image part.");

            this.Children.Add(new IconViewModel(id, filePath, this));
            this.iconsAddedOrRemoved = true;
        }

        public void RemoveIcon(string id)
        {
            this.part.RemoveImage(id);
            for (var i = 0; i < this.Children.Count; ++i)
            {
                if (!(this.Children[i] is IconViewModel icon))
                {
                    continue;
                }

                if (icon.Id == id)
                {
                    this.Children.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Reloads the OfficePart associated with this View model, but preserving the current edits if not saved
        /// </summary>
        /// <returns>Whether it was reloaded successfully (it might not if the corresponding part type does not 
        /// exist in the document anymore)</returns>
        public bool Reload()
        {
            var reloaded = ((OfficeDocumentViewModel)this.Parent).Document.RetrieveCustomPart(this.Part.PartType);
            if (reloaded == null)
            {
                // Don't do anything if there is no equivalent to reload
                return false;
            }

            this.originalContents = reloaded.ReadContent();
            this.part = reloaded;

            var children = new List<TreeViewItemViewModel>(this.Children);
            this.Children.Clear();

            // Add the icons that were already loaded
            foreach (var tmp in children)
            {
                if (!(tmp is IconViewModel icon))
                {
                    continue;
                }

                // Save it to a temporary path file
                // TODO: This might not coincide with the original file format
                var encoder = new PngBitmapEncoder();

                var photoId = Guid.NewGuid();
                var location = Path.Combine(Path.GetTempPath(), photoId + ".png");
                encoder.Frames.Add(BitmapFrame.Create(icon.Image));

                using (var stream = new FileStream(location, FileMode.Create))
                {
                    encoder.Save(stream);
                }

                // Load the image
                this.InsertIcon(location, icon.Id);
                
                File.Delete(location);
            }

            return true;
        }

        public void Save()
        {
            // Do not simply call Part.Save because that will not flag the part as dirty in the parent document
            var docModel = (OfficeDocumentViewModel)this.Parent;
            docModel.Document.SaveCustomPart(this.Part.PartType, this.Contents);
            this.originalContents = this.Contents;
            this.iconsAddedOrRemoved = false;
        }

        protected override void LoadChildren()
        {
            this.LoadIcons();
        }

        private void LoadIcons()
        {
            foreach (var image in this.Part.GetImages())
            {
                this.Children.Add(new IconViewModel(image.Key, image.Value, this));
            }
        }
    }
}
