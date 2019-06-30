using System.Windows;
using System.Windows.Controls;
using OfficeRibbonXEditor.Controls;
using OfficeRibbonXEditor.ViewModels;

namespace OfficeRibbonXEditor.Views
{
    /// <summary>
    /// Interaction logic for FindReplaceDialog.xaml
    /// </summary>
    public partial class FindReplaceDialog : DialogControl
    {
        public FindReplaceDialog()
        {
            this.InitializeComponent();

            this.FindBox.TextChanged += this.OnValueChanged;
            this.ReplaceBox.TextChanged += this.OnValueChanged;
        }

        private void OnValueChanged(object sender, TextChangedEventArgs e)
        {
            var vm = (FindReplaceDialogViewModel) this.DataContext;

            var txt = (TextBox) sender;
            txt.TextChanged -= this.OnValueChanged;
            txt.SelectAll();
            if (txt == this.FindBox && vm.IsFindTabSelected || txt == this.ReplaceBox && !vm.IsFindTabSelected)
            {
                txt.Focus();
            }
        }
    }
}
