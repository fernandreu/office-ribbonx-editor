using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using OfficeRibbonXEditor.Models.Documents;

namespace OfficeRibbonXEditor.ViewModels.Documents
{
    public class OfficePartViewModel : TreeViewItemViewModel
    {
        private string originalContents;

        private OfficePart? part;

        public OfficePartViewModel(OfficePart part, OfficeDocumentViewModel parent) 
            : base(parent, false, contents: part.ReadContent())
        {
            this.part = part;
            this.originalContents = this.Contents;
            this.LoadIcons();
        }

        public string OriginalContents => this.originalContents;

        public OfficePart? Part
        {
            get => this.part;
            set => this.Set(ref this.part, value);
        }

        public bool IconsChanged { get; set; }

        public bool HasUnsavedChanges => this.originalContents != this.Contents || this.IconsChanged;

        public override string Name => this.Part?.Name ?? string.Empty;

        public string ImageSource => "/Resources/Images/xml.png";  // TODO: That's probably not the only one possible

        public void InsertIcon(string filePath, string? id = null, Func<string?, string?, bool>? alreadyExistingAction = null)
        {
            if (this.Part == null)
            {
                return;
            }

            id = this.Part.AddImage(filePath, id, alreadyExistingAction);
            if (id == null)
            {
                // This probably means it was cancelled due to alreadyExistingAction
                return;
            }

            this.Children.Add(new IconViewModel(id, filePath, this));
            this.IconsChanged = true;
        }

        public void RemoveIcon(string id)
        {
            if (this.Part == null)
            {
                return;
            }

            this.Part.RemoveImage(id);
            for (var i = 0; i < this.Children.Count; ++i)
            {
                if (!(this.Children[i] is IconViewModel icon))
                {
                    continue;
                }

                if (icon.Name == id)
                {
                    this.Children.RemoveAt(i);
                    this.IconsChanged = true;
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
            if (this.Part == null)
            {
                return false;
            }

            var reloaded = (this.Parent as OfficeDocumentViewModel)?.Document.RetrieveCustomPart(this.Part.PartType);
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
            foreach (var icon in children.OfType<IconViewModel>())
            {
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
                this.InsertIcon(location, icon.Name);
                
                File.Delete(location);
            }

            return true;
        }

        public void Save()
        {
            if (this.Part == null)
            {
                return;
            }

            // Do not simply call Part.Save because that will not flag the part as dirty in the parent document
            var docModel = this.Parent as OfficeDocumentViewModel;
            if (docModel?.Document == null)
            {
                // TODO: Should this throw?
                return;
            }

            docModel.Document.SaveCustomPart(this.Part.PartType, this.Contents);

            this.Part = docModel.Document.RetrieveCustomPart(this.Part.PartType);
            this.LoadIcons();

            this.originalContents = this.Contents;
            this.IconsChanged = false;
        }

        protected override void LoadChildren()
        {
            this.LoadIcons();
        }

        private void LoadIcons()
        {
            if (this.Part == null)
            {
                return;
            }

            this.Children.Clear();
            foreach (var image in this.Part.GetImages())
            {
                this.Children.Add(new IconViewModel(image.Key, image.Value, this));
            }
        }
    }
}
