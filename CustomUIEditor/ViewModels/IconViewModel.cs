// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IconViewModel.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the OfficePartViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.ViewModels
{
    using System.IO;
    using System.Windows.Media.Imaging;
    

    public class IconViewModel : TreeViewItemViewModel
    {
        private string id;

        private bool isEditingId;

        public IconViewModel(string id, BitmapImage image, OfficePartViewModel parent)
            : base(parent, false, false)
        {
            this.Image = image;
            this.id = id;
        }

        public IconViewModel(string id, string filePath, OfficePartViewModel parent) 
            : this(id, LoadImageFromPath(filePath), parent)
        {
        }

        public BitmapImage Image { get; }

        public string Id
        {
            get => this.id;
            set
            {
                // Make sure this.ChangeId() is called with the previous ID and not the new one already. Otherwise,
                // the icon will actually not be updated inside the part
                var previousId = this.id;
                this.SetProperty(ref this.id, value, () => this.ChangeId(previousId, value));
            }
        }

        public bool IsEditingId
        {
            get => this.isEditingId;
            set => this.SetProperty(ref this.isEditingId, value);
        }
        
        protected override void SelectionLost()
        {
            this.IsEditingId = false;
        }

        private static BitmapImage LoadImageFromPath(string filePath)
        {
            var image = new BitmapImage();
            using (var stream = File.OpenRead(filePath))
            {
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
            }
            
            return image;
        }

        private void ChangeId(string oldId, string newId)
        {
            var viewModel = (OfficePartViewModel)this.Parent;
            viewModel.Part.ChangeImageId(oldId, newId);
            this.IsEditingId = false;
            viewModel.IconsChanged = true;
        }
    }
}
