using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{
    [Export]
    public partial class DialogHostViewModel : ObservableObject
    {
        private IContentDialogBase? _content;

        public event EventHandler? ShowingDialog;

        public event EventHandler? Closed;

        public IContentDialogBase? Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        [RelayCommand]
        private void Closing(CancelEventArgs args)
        {
            _content?.ClosingCommand.Execute(args);
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
}
