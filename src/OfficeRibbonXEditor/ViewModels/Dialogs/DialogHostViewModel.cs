using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Dialogs;

[Export]
public partial class DialogHostViewModel : ObservableObject
{
    public event EventHandler? ShowingDialog;

    public event EventHandler? Closed;

    [ObservableProperty]
    public partial IContentDialogBase? Content { get; set; }

    [RelayCommand]
    private void Closing(CancelEventArgs args)
    {
        Content?.ClosingCommand.Execute(args);
    }

    public void ShowDialog()
    {
        ShowingDialog?.Invoke(this, EventArgs.Empty);
    }

    public void Close()
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }
}