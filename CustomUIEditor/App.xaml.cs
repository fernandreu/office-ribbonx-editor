// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Interaction logic for App.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor
{
    using System.Windows;
    
    /// <summary>
    /// Interaction logic for App
    /// </summary>
    public partial class App : Application
    {
        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            CustomUIEditor.Properties.Settings.Default.Save();
        }
    }
}
