using System.Windows;
using OfficeRibbonXEditor.ViewModels.Dialogs;

namespace OfficeRibbonXEditor.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for DialogHost.xaml
    /// </summary>
    public partial class DialogHost : DialogHostBase
    {
        public DialogHost()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == DataContextProperty && DataContext is DialogHostViewModel viewModel)
            {
                Closing += (o, args) => viewModel.ClosingCommand.Execute(args);
            }
        }
    }
}
