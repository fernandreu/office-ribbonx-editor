using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using OfficeRibbonXEditor.Documents;

namespace OfficeRibbonXEditor.ViewModels.Documents
{
    public partial class OfficePartViewModel : TreeViewItemViewModel
    {
        public OfficePartViewModel(OfficePart part, OfficeDocumentViewModel parent) 
            : base(parent, false, contents: part.ReadContent())
        {
            _part = part;
            OriginalContents = Contents ?? string.Empty;
            LoadIcons();
        }

        public string OriginalContents { get; private set; }

        [ObservableProperty]
        private OfficePart? _part;

        public bool IconsChanged { get; set; }

        public bool HasUnsavedChanges => OriginalContents != Contents || IconsChanged;

        public override string Name => Part?.Name ?? string.Empty;

#pragma warning disable CA1822 // ImageSource does not access instance data and can be made static (but is needed for WPF, in case there are different images one day)
        public string ImageSource => "/Resources/Images/xml.png";  // TODO: That's probably not the only one possible
#pragma warning restore CA1822

        public void InsertIcon(string filePath, string? id = null, Func<string?, string?, bool>? alreadyExistingAction = null)
        {
            if (Part == null)
            {
                return;
            }

            id = Part.AddImage(filePath, id, alreadyExistingAction);
            if (id == null)
            {
                // This probably means it was cancelled due to alreadyExistingAction
                return;
            }

            Children.Add(new IconViewModel(id, filePath, this));
            IconsChanged = true;
        }

        public void RemoveIcon(string id)
        {
            if (Part == null)
            {
                return;
            }

            Part.RemoveImage(id);
            for (var i = 0; i < Children.Count; ++i)
            {
                if (Children[i] is not IconViewModel icon)
                {
                    continue;
                }

                if (icon.Name == id)
                {
                    Children.RemoveAt(i);
                    IconsChanged = true;
                    return;
                }
            }
        }

        public void ChangeIconId(string oldId, string newId)
        {
            Part?.ChangeImageId(oldId, newId);
            IconsChanged = true;
            SortedChildren.Refresh();
        }

        /// <summary>
        /// Reloads the OfficePart associated with this View model, but preserving the current edits if not saved
        /// </summary>
        /// <returns>Whether it was reloaded successfully (it might not if the corresponding part type does not 
        /// exist in the document anymore)</returns>
        public bool Reload()
        {
            if (Part == null)
            {
                return false;
            }

            var reloaded = (Parent as OfficeDocumentViewModel)?.Document.RetrieveCustomPart(Part.PartType);
            if (reloaded == null)
            {
                // Don't do anything if there is no equivalent to reload
                return false;
            }

            OriginalContents = reloaded.ReadContent();
            _part = reloaded;

            var children = new List<TreeViewItemViewModel>(Children);
            Children.Clear();

            // Add the icons that were already loaded
            foreach (var icon in children.OfType<IconViewModel>())
            {
                // Save it to a temporary path file
                // TODO: This might not coincide with the original file format
                // The other, most common alternative is using .ico files; however, there is no
                // standard IconBitmapEncoder available
                var encoder = new PngBitmapEncoder();

                var photoId = Guid.NewGuid();
                var location = Path.Combine(Path.GetTempPath(), photoId + ".png");
                encoder.Frames.Add(BitmapFrame.Create(icon.Image));

                try
                {
                    using (var stream = new FileStream(location, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }

                    // Load the image
                    InsertIcon(location, icon.Name);
                }
                finally
                {
                    File.Delete(location);
                }
            }

            return true;
        }

        public void Save()
        {
            if (Part == null)
            {
                return;
            }

            // Do not simply call Part.Save because that will not flag the part as dirty in the parent document
            var docModel = Parent as OfficeDocumentViewModel;
            if (docModel?.Document == null)
            {
                // TODO: Should this throw?
                return;
            }

            Contents ??= string.Empty;
            docModel.Document.SaveCustomPart(Part.PartType, Contents);

            Part = docModel.Document.RetrieveCustomPart(Part.PartType);
            LoadIcons();

            OriginalContents = Contents;
            IconsChanged = false;
        }

        protected override void LoadChildren()
        {
            LoadIcons();
        }

        private void LoadIcons()
        {
            if (Part == null)
            {
                return;
            }

            Children.Clear();
            foreach (var image in Part.GetImages())
            {
                Children.Add(new IconViewModel(image.Key, image.Value, this));
            }
        }
    }
}
