// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Thomas Levesque" file="">
//    This is taken from: http://www.thomaslevesque.com/2008/11/18/wpf-binding-to-application-settings-using-a-markup-extension/
// </copyright>
// <summary>
//   
// </summary>
// 
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Extensions
{
    using System.Windows.Data;

    public class SettingBindingExtension : Binding
    {
        public SettingBindingExtension()
        {
            this.Initialize();
        }

        public SettingBindingExtension(string path)
            : base(path)
        {
            this.Initialize();
        }

        private void Initialize()
        {
            this.Source = Properties.Settings.Default;

            // Beware: TwoWay or OneWayToSource does not work with app settings
            this.Mode = BindingMode.TwoWay;
        }
    }
}
