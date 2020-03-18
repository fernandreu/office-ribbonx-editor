using System.ComponentModel;
using System.Drawing;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Properties;
using OfficeRibbonXEditor.ViewModels.Documents;
using OfficeRibbonXEditor.ViewModels.Windows;

namespace OfficeRibbonXEditor.ViewModels.Tabs
{
    public class IconTabViewModel : ViewModelBase, ITabItemViewModel
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. The icon field is actually initialized when setting the Icon property
        public IconTabViewModel(IconViewModel icon, MainWindowViewModel mainWindow)
        {
            this.Icon = icon;
            this.MainWindow = mainWindow;
            this.ResetGridCommand = new RelayCommand(ResetGridSettings);
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public RelayCommand ResetGridCommand { get; }

        private string title = string.Empty;

        public string Title
        {
            get => this.title;
            set => this.Set(ref this.title, value);
        }

        public string? StatusText
        {
            get
            {
                if (this.Icon?.Image == null)
                {
                    return null;
                }

                return $"Size: {this.Icon.Image.Width:F0}x{(int) this.Icon.Image.Height:F0}";
            }
        }

        private IconViewModel icon;

        public IconViewModel Icon
        {
            get => this.icon;
            set
            {
                var previous = this.icon;
                if (!this.Set(ref this.icon, value))
                {
                    return;
                }

                if (previous != null)
                {
                    previous.PropertyChanged -= this.OnIconPropertyChanged;
                }

                if (value != null)
                {
                    value.PropertyChanged += this.OnIconPropertyChanged;
                }

                this.RaisePropertyChanged(nameof(this.StatusText));
                this.RaisePropertyChanged(nameof(this.Item));
            }
        }

        public TreeViewItemViewModel Item => this.Icon;

        private int zoom;

        public int Zoom
        {
            get => this.zoom;
            set => this.Set(ref this.zoom, value);
        }

        public MainWindowViewModel MainWindow { get; set; }

        public void ApplyChanges()
        {
            // No changes need to be applied in an icon tab
        }
        
        private void OnIconPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IconViewModel.Name))
            {
                return;
            }

            this.MainWindow.AdjustTabTitles();
        }

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
