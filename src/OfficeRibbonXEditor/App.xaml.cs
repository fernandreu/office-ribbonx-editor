using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Properties;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using OfficeRibbonXEditor.ViewModels.Windows;
using OfficeRibbonXEditor.Views.Dialogs;
using OfficeRibbonXEditor.Views.Windows;
using WPFLocalizeExtension.Engine;

namespace OfficeRibbonXEditor
{
    /// <summary>
    /// Interaction logic for App
    /// </summary>
    public partial class App : Application
    {
        private readonly IContainer _container = CreateContainer();

        private readonly Dictionary<IContentDialogBase, DialogHost> _dialogs = new Dictionary<IContentDialogBase, DialogHost>();

        public App()
        {
            this.Dispatcher.UnhandledException += this.OnUnhandledException;

            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            InitializeCultures();
        }

        public static IContainer CreateContainer()
        {
            var result = new ContainerBuilder();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var attribute = type.GetCustomAttribute<ExportAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                var registration = attribute.InterfaceType == null ? result.RegisterType(type) : result.RegisterType(type).As(attribute.InterfaceType);

                if (attribute.Lifetime == Lifetime.Singleton)
                {
                    registration.SingleInstance();
                }
            }

            return result.Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.LaunchMainWindow();
        }

        private static void InitializeCultures()
        {
            if (string.IsNullOrEmpty(Settings.Default.UICulture))
            {
                Settings.Default.UICulture = CultureInfo.CurrentUICulture.Name;
            }

            LocalizeDictionary.Instance.Culture
                = CultureInfo.CurrentUICulture
                    = CultureInfo.DefaultThreadCurrentUICulture
                        = new CultureInfo(Settings.Default.UICulture);
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            Settings.Default.Save();
        }

        private void LaunchMainWindow()
        {
            var windowModel = this._container.Resolve<MainWindowViewModel>();
            var window = new MainWindow();
            window.DataContext = windowModel;
            windowModel.LaunchingDialog += (o, e) => this.LaunchDialog(window, e.Content, e.ShowDialog);
            windowModel.Closed += (o, e) => window.Close();
            windowModel.OnLoaded();
            window.Show();
        }

        private void LaunchDialog(Window mainWindow, IContentDialogBase content, bool showDialog)
        {
            if (content.IsUnique && !content.IsClosed && this._dialogs.TryGetValue(content, out var dialog))
            {
                dialog.Activate();
                return;
            }

            var dialogModel = this._container.Resolve<DialogHostViewModel>();
            dialog = new DialogHost {DataContext = dialogModel, Owner = mainWindow};
            dialogModel.Content = content;
            content.Closed += (o, e) => dialog.Close();
            dialogModel.Closed += (o, e) => dialog.Close();

            if (content.IsUnique)
            {
                this._dialogs[content] = dialog;
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

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            var ex = e.Exception;

            if (ex is TargetInvocationException targetEx && targetEx.InnerException != null)
            {
                ex = targetEx.InnerException;
            }

            var dialog = this._container.Resolve<ExceptionDialogViewModel>();
            dialog.OnLoaded(ex);
            this.LaunchDialog(this.MainWindow, dialog, true);
        }
    }
}
