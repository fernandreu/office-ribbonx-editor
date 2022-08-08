using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Samples
{
    public class SampleFolderViewModel : ObservableObject, ISampleMenuItem
    {
        public string Name { get; set; } = string.Empty;

        public ObservableCollection<ISampleMenuItem> Items { get; } = new ObservableCollection<ISampleMenuItem>();
    }
}
