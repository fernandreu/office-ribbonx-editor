using System.Windows;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Lexers;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using OfficeRibbonXEditor.Views.Controls;

namespace OfficeRibbonXEditor.Views.Dialogs;

/// <summary>
/// Interaction logic for CallbackWindow
/// </summary>
[ExportView(typeof(CallbackDialogViewModel))]
public partial class CallbackDialog : DialogControl
{
    public CallbackDialog()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property != DataContextProperty || e.NewValue is not CallbackDialogViewModel model)
        {
            return;
        }

        model.Lexer = new VbaLexer {Editor = Editor};
    }
}