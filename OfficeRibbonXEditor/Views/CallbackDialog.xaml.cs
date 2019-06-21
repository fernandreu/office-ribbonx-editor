using System.Windows;
using OfficeRibbonXEditor.Controls;
using OfficeRibbonXEditor.Models;
using OfficeRibbonXEditor.ViewModels;

namespace OfficeRibbonXEditor.Views
{

    /// <summary>
    /// Interaction logic for CallbackWindow
    /// </summary>
    public partial class CallbackDialog : DialogControl
    {
        public CallbackDialog()
        {
            this.InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            if (args.Property != DataContextProperty || !(args.NewValue is CallbackDialogViewModel model))
            {
                return;
            }

            model.Lexer = new VbaLexer {Editor = this.Editor};
        }
    }
}
