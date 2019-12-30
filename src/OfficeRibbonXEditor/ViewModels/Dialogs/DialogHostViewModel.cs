using System;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{
    public class DialogHostViewModel : ViewModelBase
    {
        private IContentDialogBase? content;

        public DialogHostViewModel()
        {
            this.ClosingCommand = new RelayCommand<CancelEventArgs>(this.ExecuteClosingCommand);
        }

        public event EventHandler? ShowingDialog;

        public RelayCommand<CancelEventArgs> ClosingCommand { get; }

        public event EventHandler? Closed;

        public IContentDialogBase? Content
        {
            get => this.content;
            set => this.Set(ref this.content, value);
        }

        private void ExecuteClosingCommand(CancelEventArgs args)
        {
            this.content?.ClosingCommand.Execute(args);
        }

        public void ShowDialog()
        {
            this.ShowingDialog?.Invoke(this, EventArgs.Empty);
        }

        public void Close()
        {
            var args = new CancelEventArgs();
            this.Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}
