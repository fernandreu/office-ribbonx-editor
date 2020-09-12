using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using Autofac;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Properties;
using OfficeRibbonXEditor.Services;
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
        private readonly IContainer container = new AppContainerBuilder().Build();

        private readonly Dictionary<IContentDialogBase, DialogHost> dialogs = new Dictionary<IContentDialogBase, DialogHost>();

        public App()
        {
            this.Dispatcher.UnhandledException += this.OnUnhandledException;

            InitializeCultures();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.LaunchMainWindow();
        }

        private static void InitializeCultures()
        {
            if (!string.IsNullOrEmpty(Settings.Default.Culture))
            {
                LocalizeDictionary.Instance.Culture
                    = Thread.CurrentThread.CurrentCulture
                        = new CultureInfo(Settings.Default.Culture);
            }

            if (!string.IsNullOrEmpty(Settings.Default.UICulture))
            {
                LocalizeDictionary.Instance.Culture
                    = Thread.CurrentThread.CurrentUICulture
                        = new CultureInfo(Settings.Default.UICulture);
            }

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)));
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            OfficeRibbonXEditor.Properties.Settings.Default.Save();
        }

        private void LaunchMainWindow()
        {
            var windowModel = this.container.Resolve<MainWindowViewModel>();
            var window = new MainWindow();
            window.DataContext = windowModel;
            windowModel.LaunchingDialog += (o, e) => this.LaunchDialog(window, e.Content, e.ShowDialog);
            windowModel.Closed += (o, e) => window.Close();
            windowModel.OnLoaded();
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

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            var ex = e.Exception;

            if (ex is TargetInvocationException targetEx && targetEx.InnerException != null)
            {
                ex = targetEx.InnerException;
            }

            var dialog = this.container.Resolve<ExceptionDialogViewModel>();
            dialog.OnLoaded(ex);
            this.LaunchDialog(this.MainWindow, dialog, true);
        }
    }
}
