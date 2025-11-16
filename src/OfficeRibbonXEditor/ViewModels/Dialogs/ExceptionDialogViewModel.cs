using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Dialogs;

[Export]
public partial class ExceptionDialogViewModel(IToolInfo info, IUrlHelper urlHelper) : DialogBase, IContentDialog<Exception?>
{
    private readonly IToolInfo _info = info;

    private readonly IUrlHelper _urlHelper = urlHelper;

    [ObservableProperty]
    public partial Exception? Exception { get; set; }

    public bool OnLoaded(Exception? payload)
    {
        Exception = payload;
        return payload != null;
    }

    [RelayCommand]
    private void Shutdown()
    {
        Application.Current.MainWindow?.Close();
    }

    [RelayCommand]
    private void SubmitBug()
    {
        var title =$"{Exception?.GetType().Name}: {Exception?.Message}";
        var body = 
            "**Describe the bug**\n\n" +
            $"```\n{Exception}\n```\n\n" +
            "**To Reproduce**\n" +
            "Please describe the steps that lead to the unhandled exception here, including example files if relevant.\n\n" +
            "**Screenshots**\n" +
            "If applicable, add screenshots to help explain how the unhandled exception occurred.\n\n" +
            "**Additional context**\n\n" +
            $"- Version: {_info.AssemblyVersion}\n" +
            $"- Runtime: {_info.RuntimeVersion}\n" +
            $"- Operating System: {_info.OperatingSystemVersion}\n";
        _urlHelper.OpenBug(title, body);
    }
}