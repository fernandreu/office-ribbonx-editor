using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using OfficeRibbonXEditor.Views.Controls;

namespace OfficeRibbonXEditor.Views.Dialogs
{
    /// <summary>
    /// About dialog for the tool
    /// </summary>
    [ExportView(typeof(AboutDialogViewModel))]
    public partial class AboutDialog : DialogControl
    {
        public AboutDialog()
        {
            this.InitializeComponent();
        }
    }
}
