using System.Collections.Generic;
using System.Windows;
using Autofac;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Models;
using OfficeRibbonXEditor.Services;
using OfficeRibbonXEditor.ViewModels;
using OfficeRibbonXEditor.Views;

namespace OfficeRibbonXEditor
{
    /// <summary>
    /// Interaction logic for App
    /// </summary>
    public partial class App : Application
    {
        private readonly IContainer container;

        private readonly Dictionary<IContentDialogBase, DialogHost> dialogs = new Dictionary<IContentDialogBase, DialogHost>();

        public App()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<MessageBoxService>().As<IMessageBoxService>();
            builder.RegisterType<FileDialogService>().As<IFileDialogService>();
            builder.RegisterType<VersionChecker>().As<IVersionChecker>();
            builder.RegisterType<DialogProvider>().As<IDialogProvider>();

            builder.RegisterType<MainWindowViewModel>();
            DialogHostBase.RegisterDialogViewModels(builder);

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
            windowModel.LaunchingDialog += (o, e) => this.LaunchDialog(window, e.Content, e.ShowDialog);
            windowModel.Closed += (o, e) => window.Close();
            window.Show();
        }

        private void LaunchDialog(Window mainWindow, IContentDialogBase content, bool showDialog)
        {
            if (content.IsUnique && !content.IsClosed && this.dialogs.TryGetValue(content, out var dialog))
            {
                dialog.Activate();
                return;
            }

            var dialogModel = this.container.Resolve<DialogHostViewModel>();
            dialog = new DialogHost {DataContext = dialogModel, Owner = mainWindow};
            dialogModel.Content = content;
            content.Closed += (o, e) => dialog.Close();
            dialogModel.Closed += (o, e) => dialog.Close();

            if (content.IsUnique)
            {
                this.dialogs[content] = dialog;
            }

            if (showDialog)
            {
                dialog.ShowDialog();
            }
            else
            {
                dialog.Show();
            }
        }
    }
}
