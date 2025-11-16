using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Dialogs;

public partial class DialogBase : ObservableObject, IContentDialogBase
{
    public bool IsUnique => true;

    public bool IsClosed { get; protected set; }

    public bool IsCancelled { get; protected set; } = true;

    public event EventHandler? Closed;

    [RelayCommand]
    public void Close()
    {
        this.Closed?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Closing(CancelEventArgs args)
    {
        this.OnClosing(args);
        if (args.Cancel)
        {
            return;
        }

        this.IsClosed = true;
    }

    protected virtual void OnClosing(CancelEventArgs args)
    {
    }
}