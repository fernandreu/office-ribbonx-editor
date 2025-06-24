using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using OfficeRibbonXEditor.Views.Controls;

namespace OfficeRibbonXEditor.Views.Dialogs;

/// <summary>
/// Interaction logic for SettingsDialog
/// </summary>
[ExportView(typeof(SettingsDialogViewModel))]
public partial class SettingsDialog : DialogControl
{
    public SettingsDialog()
    {
        InitializeComponent();

        WrapModeBox.ItemsSource = Enum.GetValues<ScintillaNET.WrapMode>();
    }
}