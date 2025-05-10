using System.Media;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using OfficeRibbonXEditor.Views.Controls;

namespace OfficeRibbonXEditor.Views.Dialogs;

/// <summary>
/// About dialog for the tool
/// </summary>
[ExportView(typeof(ExceptionDialogViewModel))]
public partial class ExceptionDialog : DialogControl
{
    public ExceptionDialog()
    {
        InitializeComponent();

        Loaded += (o, e) => SystemSounds.Asterisk.Play();
    }
}