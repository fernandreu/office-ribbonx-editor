using System;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Properties;

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{
    public class AboutDialogViewModel : DialogBase
    {
        private readonly IMessageBoxService messageBoxService;

        private readonly IUrlHelper urlHelper;

        public AboutDialogViewModel(IMessageBoxService messageBoxService, IToolInfo info, IUrlHelper urlHelper)
        {
            this.messageBoxService = messageBoxService;
            this.Info = info;
            this.urlHelper = urlHelper;
            this.SubmitIssueCommand = new RelayCommand(this.ExecuteSubmitIssueCommand);
            this.CopyInfoCommand = new RelayCommand(this.ExecuteCopyInfoCommand);
        }

        public IToolInfo Info { get; }

        public RelayCommand SubmitIssueCommand { get; }

        public RelayCommand CopyInfoCommand { get; }

        private void ExecuteSubmitIssueCommand()
        {
            this.urlHelper.OpenIssue();
        }

        private void ExecuteCopyInfoCommand()
        {
            Clipboard.SetText(
                $"Version: {this.Info.AssemblyVersion}\n" +
                $"Runtime:\n {this.Info.RuntimeVersion}\n " +
                $"Operating System: {this.Info.OperatingSystemVersion}");

            this.messageBoxService.Show(
                Strings.Message_VersionCopy_Text,
                Strings.Message_VersionCopy_Title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
