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
        private string lineStatus;

        public string LineStatus
        {
            get => this.lineStatus;
            set => this.Set(ref this.lineStatus, value);
        }

        public OfficePartViewModel Part { get; set; }
    }
}
