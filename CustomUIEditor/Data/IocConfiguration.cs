// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IocConfiguration.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the IocConfiguration type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Data
{
    using CustomUIEditor.Services;
    using CustomUIEditor.ViewModels;

    using Ninject.Modules;

    public class IocConfiguration : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IMessageBoxService>().To<MessageBoxService>().InSingletonScope();
            this.Bind<IFileDialogService>().To<FileDialogService>().InSingletonScope();
            this.Bind<MainWindowViewModel>().ToSelf().InTransientScope();
        }
    }
}
