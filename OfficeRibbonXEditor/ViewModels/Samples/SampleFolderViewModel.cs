using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Samples
{
    public class SampleFolderViewModel : ViewModelBase, ISampleMenuItem
    {
        public string Name { get; set; }

        public ObservableCollection<ISampleMenuItem> Items { get; } = new ObservableCollection<ISampleMenuItem>();
    }
}
