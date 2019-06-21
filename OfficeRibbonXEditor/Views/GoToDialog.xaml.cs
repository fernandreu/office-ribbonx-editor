using OfficeRibbonXEditor.Controls;

namespace OfficeRibbonXEditor.Views
{
    /// <summary>
    /// Interaction logic for GoToDialog.xaml
    /// </summary>
    public partial class GoToDialog : DialogControl
    {
        public GoToDialog()
        {
            this.InitializeComponent();
            this.TargetBox.Focus();
        }
    }
}
