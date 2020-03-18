using Autofac;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Services;
using OfficeRibbonXEditor.ViewModels.Windows;
using OfficeRibbonXEditor.Views.Dialogs;

namespace OfficeRibbonXEditor.Helpers
{
    public class AppContainerBuilder : ContainerBuilder
    {
        public AppContainerBuilder()
        {
            this.RegisterType<MessageBoxService>().As<IMessageBoxService>();
            this.RegisterType<FileDialogService>().As<IFileDialogService>();
            this.RegisterType<VersionChecker>().As<IVersionChecker>();
            this.RegisterType<DialogProvider>().As<IDialogProvider>();
            this.RegisterType<ToolInfo>().As<IToolInfo>();
            this.RegisterType<UrlHelper>().As<IUrlHelper>();

            this.RegisterType<MainWindowViewModel>();
            DialogHostBase.RegisterDialogViewModels(this);
        }
    }
}
