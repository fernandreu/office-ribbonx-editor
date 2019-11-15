using System.Windows;
using OfficeRibbonXEditor.Views.Controls;

namespace OfficeRibbonXEditor.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for GoToDialog.xaml
    /// </summary>
    public partial class GoToDialog : DialogControl
    {
        public GoToDialog()
        {
            this.InitializeComponent();

            // This will set the focus on the TargetBox with the value selected when it first gets initialized. Calling
            // Focus() directly may cause issues due to the ordering of the text set, the dialog displayed, etc
            this.TargetBox.ValueChanged += this.OnValueChanged;
        }

        private void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.TargetBox.ValueChanged -= this.OnValueChanged;
            this.TargetBox.Focus();
        }
    }
}
