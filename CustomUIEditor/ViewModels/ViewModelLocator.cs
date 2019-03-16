// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewModelLocator.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the ViewModelLocator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.ViewModels
{
    using CommonServiceLocator;

    using CustomUIEditor.Services;

    using GalaSoft.MvvmLight.Ioc;

    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainWindowViewModel>();
            SimpleIoc.Default.Register<IMessageBoxService, MessageBoxService>();
            SimpleIoc.Default.Register<IFileDialogService, FileDialogService>();
            SimpleIoc.Default.Register<IVersionChecker, VersionChecker>();
        }
        
        public MainWindowViewModel Main => ServiceLocator.Current.GetInstance<MainWindowViewModel>();
    }
}
