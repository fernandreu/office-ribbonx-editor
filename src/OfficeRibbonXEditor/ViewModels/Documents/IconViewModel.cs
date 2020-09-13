using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;
using CommonServiceLocator;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Resources;

namespace OfficeRibbonXEditor.ViewModels.Documents
{
    public class IconViewModel : TreeViewItemViewModel
    {
        private string name;

        private string newName;
        
        private bool isEditingId;

        public IconViewModel(string name, BitmapImage image, OfficePartViewModel parent)
            : base(parent, false, false)
        {
            this.Image = image;
            this.name = name;
            this.newName = name;
        }

        public IconViewModel(string name, string filePath, OfficePartViewModel parent) 
            : this(name, LoadImageFromPath(filePath), parent)
        {
        }

        public BitmapImage Image { get; }

        /// <summary>
        /// Gets or sets the Id of the icon, applying the changes directly to the underlying model
        /// </summary>
        public override string Name
        {
            get => this.name;
            set
            {
                if (!IsValidId(value))
                {
                    return;
                }
                
                // Make sure this.ChangeId() is later called with the previous ID and not the new one
                // already. Otherwise, the icon will actually not be updated inside the part
                var previousId = this.name;

                if (!this.Set(ref this.name, value))
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
        public string NewName
        {
            get => this.newName;
            set => this.Set(ref this.newName, value);
        }

        public bool IsEditingId
        {
            get => this.isEditingId;
            set => this.Set(ref this.isEditingId, value);
        }

        /// <summary>
        /// Attempts to apply the NewName property to the Id one, but cancels the action if the ID is invalid
        /// </summary>
        public void CommitIdChange()
        {
            this.IsEditingId = false;

            if (!IsValidId(this.NewName, out var errorMessage))
            {
                // Revert back the change
                this.NewName = this.Name;
                ServiceLocator.Current.GetInstance<IMessageBoxService>()?.Show(
                    errorMessage, 
                    Strings.Message_ChangeIdError_Title, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return;
            }

            this.Name = this.NewName;
        }

        /// <summary>
        /// Ignores the current value of NewName, setting it back to be equal to Id
        /// </summary>
        public void DiscardIdChange()
        {
            this.NewName = this.Name;
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
            var viewModel = this.Parent as OfficePartViewModel;
            viewModel?.Part?.ChangeImageId(oldId, newId);
            this.IsEditingId = false;

            if (viewModel != null)
            {
                viewModel.IconsChanged = true;
            }
        }
    }
}
