using System;
using System.Linq;
using OfficeRibbonXEditor.Controls;

namespace OfficeRibbonXEditor.Views
{
    /// <summary>
    /// Interaction logic for SettingsDialog
    /// </summary>
    public partial class SettingsDialog : DialogControl
    {
        public SettingsDialog()
        {
            this.InitializeComponent();

            this.WrapModeBox.ItemsSource = Enum.GetValues(typeof(ScintillaNET.WrapMode)).Cast<ScintillaNET.WrapMode>();
        }
    }
}
