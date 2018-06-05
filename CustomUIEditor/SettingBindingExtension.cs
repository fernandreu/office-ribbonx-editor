// This is taken from: http://www.thomaslevesque.com/2008/11/18/wpf-binding-to-application-settings-using-a-markup-extension/
namespace CustomUIEditor
{
    using System.Windows.Data;

    class SettingBindingExtension : Binding
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
            this.Source = Properties.Settings.Default;

            // Beware: TwoWay or OneWayToSource does not work with app settings
            this.Mode = BindingMode.TwoWay;
        }
    }
}
