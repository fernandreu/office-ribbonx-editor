using System.Windows;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Resources;

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{
    [Export]
    public class AboutDialogViewModel : DialogBase
    {
        private readonly IMessageBoxService _messageBoxService;

        private readonly IUrlHelper _urlHelper;

        public AboutDialogViewModel(IMessageBoxService messageBoxService, IToolInfo info, IUrlHelper urlHelper)
        {
            _messageBoxService = messageBoxService;
            Info = info;
            _urlHelper = urlHelper;
        }

        public IToolInfo Info { get; }

        private RelayCommand? _submitIssueCommand;
        public RelayCommand SubmitIssueCommand => _submitIssueCommand ??= new RelayCommand(ExecuteSubmitIssueCommand);

        private RelayCommand? _copyInfoCommand;
        public RelayCommand CopyInfoCommand => _copyInfoCommand ??= new RelayCommand(ExecuteCopyInfoCommand);

        private void ExecuteSubmitIssueCommand()
        {
            _urlHelper.OpenIssue();
        }

        private void ExecuteCopyInfoCommand()
        {
            Clipboard.SetText(
                $"Version: {Info.AssemblyVersion}\n" +
                $"Runtime:\n {Info.RuntimeVersion}\n " +
                $"Operating System: {Info.OperatingSystemVersion}");

            _messageBoxService.Show(
                Strings.Message_VersionCopy_Text,
                Strings.Message_VersionCopy_Title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
