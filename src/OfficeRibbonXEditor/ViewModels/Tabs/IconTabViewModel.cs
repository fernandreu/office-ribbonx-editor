using System.ComponentModel;
using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Properties;
using OfficeRibbonXEditor.ViewModels.Documents;
using OfficeRibbonXEditor.ViewModels.Windows;

namespace OfficeRibbonXEditor.ViewModels.Tabs
{
    public partial class IconTabViewModel : ObservableObject, ITabItemViewModel
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. The icon field is actually initialized when setting the Icon property
        public IconTabViewModel(IconViewModel icon, MainWindowViewModel mainWindow)
        {
            Icon = icon;
            MainWindow = mainWindow;
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        [ObservableProperty]
        private string _title = string.Empty;

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

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StatusText))]
        [NotifyPropertyChangedFor(nameof(Item))]
        private IconViewModel _icon;

        partial void OnIconChanging(IconViewModel value)
        {
            if (value != null)
            {
                value.PropertyChanged -= OnIconPropertyChanged;
            }
        }

        partial void OnIconChanged(IconViewModel value)
        {
            if (value != null)
            {
                value.PropertyChanged += OnIconPropertyChanged;
            }
        }
        
        public TreeViewItemViewModel Item => Icon;

        [ObservableProperty]
        private int _zoom;

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

        [RelayCommand]
        public static void ResetGrid()
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
