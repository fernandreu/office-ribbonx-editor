using System;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using Generators;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{
    public partial class DialogBase : ViewModelBase, IContentDialogBase
    {
        public bool IsUnique => true;

        public bool IsClosed { get; protected set; }

        public bool IsCancelled { get; protected set; } = true;

        public event EventHandler? Closed;

        [GenerateCommand(Name = "CloseCommand")]
        public void Close()
        {
            this.Closed?.Invoke(this, EventArgs.Empty);
        }

        [GenerateCommand]
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
