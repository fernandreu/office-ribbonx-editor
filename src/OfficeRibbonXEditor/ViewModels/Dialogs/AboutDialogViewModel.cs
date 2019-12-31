using System.Diagnostics;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{
    public class AboutDialogViewModel : DialogBase
    {
        private readonly IMessageBoxService messageBoxService;

        public AboutDialogViewModel(IMessageBoxService messageBoxService, IToolInfo info)
        {
            this.messageBoxService = messageBoxService;
            this.Info = info;
            this.SubmitIssueCommand = new RelayCommand(ExecuteSubmitIssueCommand);
            this.CopyInfoCommand = new RelayCommand(this.ExecuteCopyInfoCommand);
        }

        public IToolInfo Info { get; }

        public RelayCommand SubmitIssueCommand { get; }

        public RelayCommand CopyInfoCommand { get; }

        private static void ExecuteSubmitIssueCommand()
        {
            Process.Start("https://github.com/fernandreu/office-ribbonx-editor/issues/new/choose");
        }

        private void ExecuteCopyInfoCommand()
        {
            Clipboard.SetText(
                $"Version: {this.Info.AssemblyVersion}\n" +
                $"Runtime:\n {this.Info.RuntimeVersion}\n " +
                $"Operating System: {this.Info.OperatingSystemVersion}");

            this.messageBoxService.Show(
                "The version information has been copied to the clipboard.",
                "Version Information Copied",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
