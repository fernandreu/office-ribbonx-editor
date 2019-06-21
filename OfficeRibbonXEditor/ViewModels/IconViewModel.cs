using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;

using CommonServiceLocator;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels
{
    public class IconViewModel : TreeViewItemViewModel
    {
        private string id;

        private string newId;
        
        private bool isEditingId;

        public IconViewModel(string id, BitmapImage image, OfficePartViewModel parent)
            : base(parent, false, false)
        {
            this.Image = image;
            this.id = id;
            this.newId = id;
        }

        public IconViewModel(string id, string filePath, OfficePartViewModel parent) 
            : this(id, LoadImageFromPath(filePath), parent)
        {
        }

        public BitmapImage Image { get; }

        /// <summary>
        /// Gets or sets the Id of the icon, applying the changes directly to the underlying model
        /// </summary>
        public string Id
        {
            get => this.id;
            set
            {
                if (!IsValidId(value))
                {
                    return;
                }
                
                // Make sure this.ChangeId() is later called with the previous ID and not the new one
                // already. Otherwise, the icon will actually not be updated inside the part
                var previousId = this.id;

                if (!this.Set(ref this.id, value))
                {
                    return;
                }
                
                this.ChangeId(previousId, value);
            }
        }

        /// <summary>
        /// Gets or sets the potentially new ID to be used for the icon. This is used, for example, in
        /// editing mode before committing to use the new ID (in case the user discard the changes)
        /// </summary>
        public string NewId
        {
            get => this.newId;
            set => this.Set(ref this.newId, value);
        }

        public bool IsEditingId
        {
            get => this.isEditingId;
            set => this.Set(ref this.isEditingId, value);
        }

        /// <summary>
        /// Attempts to apply the NewId property to the Id one, but cancels the action if the ID is invalid
        /// </summary>
        public void CommitIdChange()
        {
            this.IsEditingId = false;

            if (!IsValidId(this.NewId, out var errorMessage))
            {
                // Revert back the change
                this.NewId = this.Id;
                ServiceLocator.Current.GetInstance<IMessageBoxService>()?.Show(
                    errorMessage, 
                    "Error Changing Icon ID", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return;
            }

            this.Id = this.NewId;
        }

        /// <summary>
        /// Ignores the current value of NewId, setting it back to be equal to Id
        /// </summary>
        public void DiscardIdChange()
        {
            this.NewId = this.Id;
            this.IsEditingId = false;
        }

        protected override void SelectionLost()
        {
            this.IsEditingId = false;
        }
        
        private static bool IsValidId(string id)
        {
            return IsValidId(id, out _);
        }

        private static bool IsValidId(string id, out string errorMessage)
        {
            try
            {
                XmlConvert.VerifyName(id);
            }
            catch (XmlException e)
            {
                errorMessage = e.Message;
                return false;
            }
            
            errorMessage = string.Empty;
            return true;
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
