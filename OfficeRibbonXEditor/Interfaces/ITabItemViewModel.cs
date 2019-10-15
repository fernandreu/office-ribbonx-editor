using OfficeRibbonXEditor.ViewModels;

namespace OfficeRibbonXEditor.Interfaces
{
    public interface ITabItemViewModel
    {
        string Title { get; set; }

        string StatusText { get; }

        int Zoom { get; }

        MainWindowViewModel MainWindow { get; }

        void ApplyChanges();
    }
}
