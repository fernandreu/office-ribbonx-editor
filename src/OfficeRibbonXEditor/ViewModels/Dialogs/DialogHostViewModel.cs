using System;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using Generators;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{
    [Export]
    public partial class DialogHostViewModel : ViewModelBase
    {
        private IContentDialogBase? _content;

        public event EventHandler? ShowingDialog;

        public event EventHandler? Closed;

        public IContentDialogBase? Content
        {
            get => _content;
            set => Set(ref _content, value);
        }

        [GenerateCommand]
        private void ExecuteClosingCommand(CancelEventArgs args)
        {
            _content?.ClosingCommand.Execute(args);
        }

        public void ShowDialog()
        {
            ShowingDialog?.Invoke(this, EventArgs.Empty);
        }

        public void Close()
        {
            var args = new CancelEventArgs();
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}
