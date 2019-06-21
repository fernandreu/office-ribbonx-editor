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
    using System;
    using System.Windows;
    using System.Windows.Data;
    using Autofac;
    using GalaSoft.MvvmLight.Ioc;
    using OfficeRibbonXEditor.Interfaces;
    using OfficeRibbonXEditor.Models;
    using OfficeRibbonXEditor.Services;
    using OfficeRibbonXEditor.ViewModels;
    using OfficeRibbonXEditor.Views;

    /// <summary>
    /// Interaction logic for App
    /// </summary>
    public partial class App : Application
    {
        private readonly IContainer container;

        public App()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<MessageBoxService>().As<IMessageBoxService>();
            builder.RegisterType<FileDialogService>().As<IFileDialogService>();
            builder.RegisterType<VersionChecker>().As<IVersionChecker>();
            builder.RegisterType<DialogProvider>().As<IDialogProvider>();

            builder.RegisterType<MainWindowViewModel>();
            builder.RegisterType<DialogHostViewModel>();
            builder.RegisterType<SettingsDialogViewModel>();
            builder.RegisterType<AboutDialogViewModel>();
            builder.RegisterType<CallbackDialogViewModel>();

            this.container = builder.Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.LaunchMainWindow();
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            OfficeRibbonXEditor.Properties.Settings.Default.Save();
        }

        private void LaunchMainWindow()
        {
            var windowModel = this.container.Resolve<MainWindowViewModel>();
            var window = new MainWindow();
            windowModel.Lexer = new XmlLexer {Editor = window.Editor};
            window.DataContext = windowModel;
            windowModel.LaunchingDialog += (o, e) => this.LaunchDialog(window, e.Data);
            windowModel.Closed += (o, e) => window.Close();
            window.Show();
        }
        
        private void LaunchDialog(Window mainWindow, IContentDialogBase content)
        {
            var dialogModel = this.container.Resolve<DialogHostViewModel>();
            var dialog = new DialogHost {DataContext = dialogModel, Owner = mainWindow};
            dialogModel.Content = content;
            content.Closed += (o, e) => dialog.Close();
            dialogModel.Closed += (o, e) => dialog.Close();
            dialog.ShowDialog();
        }
    }
}
