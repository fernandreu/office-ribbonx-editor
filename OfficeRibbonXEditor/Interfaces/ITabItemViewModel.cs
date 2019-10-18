using OfficeRibbonXEditor.ViewModels;

namespace OfficeRibbonXEditor.Interfaces
{
    public interface ITabItemViewModel
    {
        string Title { get; set; }

        TreeViewItemViewModel Item { get; }

        string StatusText { get; }

        int Zoom { get; }

        MainWindowViewModel MainWindow { get; }

        void ApplyChanges();
    }
}
