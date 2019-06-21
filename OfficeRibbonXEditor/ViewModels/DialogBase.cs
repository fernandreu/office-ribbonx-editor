using System;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels
{
    public class DialogBase : ViewModelBase, IContentDialogBase
    {
        public DialogBase()
        {
            this.ClosingCommand = new RelayCommand<CancelEventArgs>(this.ExecuteClosingCommand);
            this.CloseCommand = new RelayCommand(this.Close);
        }

        public bool Cancelled { get; protected set; } = true;

        public RelayCommand<CancelEventArgs> ClosingCommand { get; }

        public RelayCommand CloseCommand { get; }

        public event EventHandler Closed;

        public void Close()
        {
            this.Closed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void ExecuteClosingCommand(CancelEventArgs args)
        {
        }
    }
}
