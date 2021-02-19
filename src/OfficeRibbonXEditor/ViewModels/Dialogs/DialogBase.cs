using System;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{
    public class DialogBase : ViewModelBase, IContentDialogBase
    {
        public bool IsUnique => true;

        public bool IsClosed { get; protected set; }

        public bool IsCancelled { get; protected set; } = true;

        private RelayCommand<CancelEventArgs>? _closingCommand;
        public RelayCommand<CancelEventArgs> ClosingCommand => _closingCommand ??= new RelayCommand<CancelEventArgs>(ExecuteClosingCommand);

        private RelayCommand? _closeCommand;
        public RelayCommand CloseCommand => _closeCommand ??= new RelayCommand(Close);

        public event EventHandler? Closed;

        public void Close()
        {
            this.Closed?.Invoke(this, EventArgs.Empty);
        }

        private void ExecuteClosingCommand(CancelEventArgs args)
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
}
