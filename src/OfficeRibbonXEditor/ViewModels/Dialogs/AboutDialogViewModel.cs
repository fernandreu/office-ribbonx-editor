using System.Windows;
using CommunityToolkit.Mvvm.Input;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Resources;

namespace OfficeRibbonXEditor.ViewModels.Dialogs;

[Export]
public partial class AboutDialogViewModel : DialogBase
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

    [RelayCommand]
    private void SubmitIssue()
    {
        _urlHelper.OpenIssue();
    }

    /// <summary>
    /// Copies some useful machine info that can be put into a GitHub issue
    /// </summary>
    [RelayCommand]
    private void CopyInfo()
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