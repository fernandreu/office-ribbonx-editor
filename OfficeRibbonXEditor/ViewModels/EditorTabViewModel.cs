using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using OfficeRibbonXEditor.Models;

namespace OfficeRibbonXEditor.ViewModels
{
    public class EditorTabViewModel : ViewModelBase
    {
        private string title;

        public string Title
        {
            get => this.title;
            set => this.Set(ref this.title, value);
        }

        private string lineStatus;

        public string LineStatus
        {
            get => this.lineStatus;
            set
            {
                if (!this.Set(ref this.lineStatus, value))
                {
                    return;
                }

                this.RaisePropertyChanged(nameof(this.Title));
            }
        }

        private int zoom;

        public int Zoom
        {
            get => this.zoom;
            set => this.Set(ref this.zoom, value);
        }

        public ScintillaLexer Lexer { get; set; }

        public OfficePartViewModel Part { get; set; }

        public MainWindowViewModel MainWindow { get; set; }
    }
}
