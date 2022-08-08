using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.ComponentModel;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Resources;

namespace OfficeRibbonXEditor.ViewModels.Documents
{
    public partial class IconViewModel : TreeViewItemViewModel
    {
        public IconViewModel(string name, BitmapImage image, OfficePartViewModel parent)
            : base(parent, false, false)
        {
            Image = image;
            _name = name;
            _newName = name;
        }

        public IconViewModel(string name, string filePath, OfficePartViewModel parent) 
            : this(name, LoadImageFromPath(filePath), parent)
        {
        }

        public BitmapImage Image { get; }

        private string _name;
        /// <summary>
        /// Gets or sets the Id of the icon, applying the changes directly to the underlying model
        /// </summary>
        public override string Name
        {
            get => _name;
            set
            {
                if (!IsValidId(value))
                {
                    NewName = _name;
                    return;
                }
                
                if (Parent?.Children?.OfType<IconViewModel>()?.Any(x => x.Name == value) ?? false)
                {
                    // There is already an icon with the same name. It could be this same icon and not
                    // a sibling, in which case the Set() call below would have returned false anyway
                    NewName = _name;
                    return;
                }

                // Make sure this.ChangeId() is later called with the previous ID and not the new one
                // already. Otherwise, the icon will actually not be updated inside the part
                var previousId = _name;

                if (!SetProperty(ref _name, value))
                {
                    NewName = _name; // This should do nothing, since both should coincide
                    return;
                }
                
                ChangeId(previousId, value);
            }
        }

        /// <summary>
        /// Gets or sets the potentially new ID to be used for the icon. This is used, for example, in
        /// editing mode before committing to use the new ID (in case the user discard the changes)
        /// </summary>
        [ObservableProperty]
        private string _newName;

        [ObservableProperty]
        private bool _isEditingId;

        /// <summary>
        /// Attempts to apply the NewName property to the Id one, but cancels the action if the ID is invalid
        /// </summary>
        public void CommitIdChange()
        {
            IsEditingId = false;

            if (!IsValidId(NewName, out var errorMessage))
            {
                // Revert back the change
                NewName = Name;
                ServiceLocator.Current.GetInstance<IMessageBoxService>()?.Show(
                    errorMessage, 
                    Strings.Message_ChangeIdError_Title, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return;
            }

            Name = NewName;
        }

        /// <summary>
        /// Ignores the current value of NewName, setting it back to be equal to Id
        /// </summary>
        public void DiscardIdChange()
        {
            NewName = Name;
            IsEditingId = false;
        }

        protected override void SelectionLost()
        {
            IsEditingId = false;
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
            if (Parent is OfficePartViewModel viewModel)
            {
                viewModel?.ChangeIconId(oldId, newId);
            }

            IsEditingId = false;
        }
    }
}
