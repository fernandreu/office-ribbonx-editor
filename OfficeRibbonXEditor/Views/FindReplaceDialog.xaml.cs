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

            this.FindBox.TextChanged += this.OnTextChanged;
            this.ReplaceBox.TextChanged += this.OnTextChanged;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property != DataContextProperty || !(e.NewValue is FindReplaceDialogViewModel vm))
            {
                return;
            }

            if (vm.IsFindTabSelected)
            {
                this.FindBox.Focus();
            }
            else
            {
                this.ReplaceBox.Focus();
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = (TextBox) sender;
            txt.TextChanged -= this.OnTextChanged;
            txt.Focus();
        }
    }
}
