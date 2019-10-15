using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private string statusText;

        public string StatusText
        {
            get => this.statusText;
            set => this.Set(ref this.statusText, value);
        }

        private IconViewModel icon;

        public IconViewModel Icon
        {
            get => this.icon;
            set => this.Set(ref this.icon, value);
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
