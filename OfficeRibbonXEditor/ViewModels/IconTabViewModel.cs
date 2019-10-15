using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels
{
    public class IconTabViewModel : ViewModelBase, ITabItemViewModel
    {
        private string title;

        public string Title
        {
            get => this.title;
            set => this.Set(ref this.title, value);
        }

        public string StatusText
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
                if (!this.Set(ref this.icon, value))
                {
                    return;
                }

                this.RaisePropertyChanged(nameof(StatusText));
            }
        }

        private int zoom;

        public int Zoom
        {
            get => this.zoom;
            set => this.Set(ref this.zoom, value);
        }

        public MainWindowViewModel MainWindow { get; set; }

        public void ApplyChanges()
        {
        }
    }
}
