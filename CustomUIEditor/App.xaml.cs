namespace CustomUIEditor
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            CustomUIEditor.Properties.Settings.Default.Save();
        }
    }
}
