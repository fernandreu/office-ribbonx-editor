using System.Windows;
using OfficeRibbonXEditor.Lexers;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using OfficeRibbonXEditor.Views.Controls;

namespace OfficeRibbonXEditor.Views.Dialogs
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
