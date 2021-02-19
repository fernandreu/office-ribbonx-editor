// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Thomas Levesque" file="SettingBindingExtension.cs">
//    This is taken from: http://www.thomaslevesque.com/2008/11/18/wpf-binding-to-application-settings-using-a-markup-extension/
// </copyright>
// <summary>
//   
// </summary>
// 
// --------------------------------------------------------------------------------------------------------------------

using System.Windows.Data;

namespace OfficeRibbonXEditor.Extensions
{
    public class SettingBindingExtension : Binding
    {
        public SettingBindingExtension()
        {
            Initialize();
        }

        public SettingBindingExtension(string path)
            : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            Source = Properties.Settings.Default;

            // Beware: TwoWay or OneWayToSource does not work with app settings
            Mode = BindingMode.TwoWay;
        }
    }
}
