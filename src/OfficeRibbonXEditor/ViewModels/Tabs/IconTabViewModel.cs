using System.ComponentModel;
using System.Drawing;
using GalaSoft.MvvmLight;
using Generators;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Properties;
using OfficeRibbonXEditor.ViewModels.Documents;
using OfficeRibbonXEditor.ViewModels.Windows;

namespace OfficeRibbonXEditor.ViewModels.Tabs
{
    public partial class IconTabViewModel : ViewModelBase, ITabItemViewModel
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. The icon field is actually initialized when setting the Icon property
        public IconTabViewModel(IconViewModel icon, MainWindowViewModel mainWindow)
        {
            Icon = icon;
            MainWindow = mainWindow;
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        public string? StatusText
        {
            get
            {
                if (Icon?.Image == null)
                {
                    return null;
                }

                return $"Size: {Icon.Image.Width:F0}x{(int) Icon.Image.Height:F0}";
            }
        }

        private IconViewModel _icon;
        public IconViewModel Icon
        {
            get => _icon;
            set
            {
                var previous = _icon;
                if (!Set(ref _icon, value))
                {
                    return;
                }

                if (previous != null)
                {
                    previous.PropertyChanged -= OnIconPropertyChanged;
                }

                if (value != null)
                {
                    value.PropertyChanged += OnIconPropertyChanged;
                }

                RaisePropertyChanged(nameof(StatusText));
                RaisePropertyChanged(nameof(Item));
            }
        }

        public TreeViewItemViewModel Item => Icon;

        private int _zoom;
        public int Zoom
        {
            get => _zoom;
            set => Set(ref _zoom, value);
        }

        public MainWindowViewModel MainWindow { get; set; }

        public void ApplyChanges()
        {
            // No changes need to be applied in an icon tab
        }
        
        private void OnIconPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IconViewModel.Name))
            {
                return;
            }

            MainWindow.AdjustTabTitles();
        }

        [GenerateCommand(Name = "ResetGridCommand")]
        public static void ResetGridSettings()
        {
            Settings.Default.IconGridMargin = 0;
            Settings.Default.IconGridMainColor = Color.White;
            Settings.Default.IconGridSecondaryColor = Color.Gray;
            Settings.Default.IconGridTwoColors = true;
            Settings.Default.IconGridSize = 16;
            Settings.Default.Save();
        }
    }
}
