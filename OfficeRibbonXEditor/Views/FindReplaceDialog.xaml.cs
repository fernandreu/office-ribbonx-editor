using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OfficeRibbonXEditor.Controls;
using OfficeRibbonXEditor.Models;
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
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property != DataContextProperty || !(e.NewValue is FindReplaceDialogViewModel vm))
            {
                return;
            }

            EditableComboBox target;
            if (vm.IsFindTabSelected)
            {
                target = this.FindBox;
            }
            else
            {
                target = this.ReplaceBox;
            }

            target.TextBox?.Focus();
            target.TextBox?.SelectAll();
        }
    }
}
