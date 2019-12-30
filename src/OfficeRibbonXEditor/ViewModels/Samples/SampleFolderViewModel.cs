using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Samples
{
    public class SampleFolderViewModel : ViewModelBase, ISampleMenuItem
    {
        public string Name { get; set; } = string.Empty;

        public ObservableCollection<ISampleMenuItem> Items { get; } = new ObservableCollection<ISampleMenuItem>();
    }
}
