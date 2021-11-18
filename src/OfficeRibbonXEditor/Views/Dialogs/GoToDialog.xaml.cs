using System.Windows;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using OfficeRibbonXEditor.Views.Controls;

namespace OfficeRibbonXEditor.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for GoToDialog.xaml
    /// </summary>
    [ExportView(typeof(GoToDialogViewModel))]
    public partial class GoToDialog : DialogControl
    {
        public GoToDialog()
        {
            InitializeComponent();

            // This will set the focus on the TargetBox with the value selected when it first gets initialized. Calling
            // Focus() directly may cause issues due to the ordering of the text set, the dialog displayed, etc
            TargetBox.ValueChanged += OnValueChanged;
        }

        private void OnValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TargetBox.ValueChanged -= OnValueChanged;
            TargetBox.Focus();
        }
    }
}
