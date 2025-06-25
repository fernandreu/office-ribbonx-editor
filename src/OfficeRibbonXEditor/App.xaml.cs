using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Properties;
using OfficeRibbonXEditor.Resources;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using OfficeRibbonXEditor.ViewModels.Windows;
using OfficeRibbonXEditor.Views.Dialogs;
using OfficeRibbonXEditor.Views.Windows;
using WPFLocalizeExtension.Engine;

namespace OfficeRibbonXEditor;

/// <summary>
/// Interaction logic for App
/// </summary>
public partial class App
{
    public IContainer Container { get; } = CreateContainer();

    private readonly Dictionary<IContentDialogBase, DialogHost> _dialogs = [];

    public App()
    {
        this.Dispatcher.UnhandledException += this.OnUnhandledException;
    }

    private static IContainer CreateContainer()
    {
        return CreateContainerBuilder().Build();
    }

    internal static ContainerBuilder CreateContainerBuilder()
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

        return result;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (!UpgradeSettings())
        {
            return;
        }

        InitializeCultures();

        this.LaunchMainWindow();
    }

    private void Restart()
    {
        System.Windows.Forms.Application.Restart();
        Shutdown();
    }

    private bool UpgradeSettings()
    {
        try
        {
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }
        }
        catch (ConfigurationErrorsException)
        {
            // The user settings file must be corrupted; delete it and restart the app

            string path;
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                path = config.FilePath;
            }
            catch (ConfigurationErrorsException ex)
            {
                path = ex.Filename;
            }

            try
            {
                File.Delete(path);
                Restart();
            }
            catch (IOException)
            {
                MessageBox.Show(
                    string.Format(CultureInfo.InvariantCulture, Strings.Message_CorruptedSettings_Text, path),
                    Strings.Message_CorruptedSettings_Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            return false;
        }

        return true;
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

    private static void ApplicationExit(object? sender, ExitEventArgs e)
    {
        Settings.Default.Save();
    }

    private void LaunchMainWindow()
    {
        var windowModel = this.Container.Resolve<MainWindowViewModel>();
        var window = new MainWindow();
        window.DataContext = windowModel;
        windowModel.LaunchingDialog += (o, e) => this.LaunchDialog(window, e.Content, e.ShowDialog);
        windowModel.Closed += (o, e) => window.Close();
        if (!windowModel.OnLoaded())
        {
            window.Close();
            return;
        }

        window.Show();
    }

    private void LaunchDialog(Window mainWindow, IContentDialogBase content, bool showDialog)
    {
        if (content.IsUnique && !content.IsClosed && this._dialogs.TryGetValue(content, out var dialog))
        {
            dialog.Activate();
            return;
        }

        var dialogModel = this.Container.Resolve<DialogHostViewModel>();
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

    private void OnUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        var ex = e.Exception;

        if (ex is TargetInvocationException targetEx && targetEx.InnerException != null)
        {
            ex = targetEx.InnerException;
        }

        var dialog = this.Container.Resolve<ExceptionDialogViewModel>();
        dialog.OnLoaded(ex);
        this.LaunchDialog(this.MainWindow, dialog, true);
    }
}