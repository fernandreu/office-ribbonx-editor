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

    using CustomUIEditor.Data;
    using CustomUIEditor.Views;

    using Ninject;

    /// <summary>
    /// Interaction logic for App
    /// </summary>
    public partial class App : Application
    {
        private IKernel iocKernel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.iocKernel = new StandardKernel();
            this.iocKernel.Load(new IocConfiguration());

            Current.MainWindow = this.iocKernel.Get<MainWindow>();
            Current.MainWindow.Show();
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            CustomUIEditor.Properties.Settings.Default.Save();
        }
    }
}
