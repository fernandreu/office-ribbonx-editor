using System;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{
    public class DialogBase : ViewModelBase, IContentDialogBase
    {
        public DialogBase()
        {
            this.ClosingCommand = new RelayCommand<CancelEventArgs>(this.ExecuteClosingCommand);
            this.CloseCommand = new RelayCommand(this.Close);
        }

        public bool IsUnique => true;

        public bool IsClosed { get; protected set; } = false;

        public bool IsCancelled { get; protected set; } = true;

        public RelayCommand<CancelEventArgs> ClosingCommand { get; }

        public RelayCommand CloseCommand { get; }

        public event EventHandler Closed;

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
