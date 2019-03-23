// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Interaction logic for App.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OfficeRibbonXEditor
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for App
    /// </summary>
    public partial class App : Application
    {
        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            OfficeRibbonXEditor.Properties.Settings.Default.Save();
        }
    }
}
