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
        public IconViewModel(string id, BitmapImage image, OfficePartViewModel parent)
            : base(parent, false, false)
        {
            this.Image = image;
            this.Id = id;
        }

        public IconViewModel(string id, string filePath, OfficePartViewModel parent) 
            : this(id, LoadImageFromPath(filePath), parent)
        {
        }

        public BitmapImage Image { get; }

        public string Id { get; }

        private static BitmapImage LoadImageFromPath(string filePath)
        {
            var stream = File.OpenRead(filePath);

            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();

            return image;
        }
    }
}
