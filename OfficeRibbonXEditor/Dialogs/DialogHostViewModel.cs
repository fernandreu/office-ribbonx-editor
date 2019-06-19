namespace OfficeRibbonXEditor.Dialogs
{
    using System;
    using System.ComponentModel;
    using GalaSoft.MvvmLight;
    using OfficeRibbonXEditor.Interfaces;

    public class DialogHostViewModel : ViewModelBase
    {
        private IContentDialogBase content;

        public event EventHandler ShowingDialog;

        public event EventHandler<CancelEventArgs> Closing;

        public event EventHandler Closed;

        public IContentDialogBase Content
        {
            get => this.content;
            set => this.Set(ref this.content, value);
        }

        public void ShowDialog()
        {
            this.ShowingDialog?.Invoke(this, EventArgs.Empty);
        }

        public void Close()
        {
            var args = new CancelEventArgs();
            this.Closing?.Invoke(this, args);
            if (args.Cancel)
            {
                return;
            }

            this.Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}
