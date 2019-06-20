using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Extensions;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels
{
    public class DialogBase : ViewModelBase, IContentDialogBase
    {
        public DialogBase(string title, Bitmap icon = null)
        {
            this.Title = title;
            this.Icon = icon?.AsBitmapImage();
            this.ClosingCommand = new RelayCommand<CancelEventArgs>(this.ExecuteClosingCommand);
            this.CloseCommand = new RelayCommand(this.Close);
        }

        public bool Cancelled { get; protected set; } = true;

        public virtual ResizeMode ResizeMode => ResizeMode.NoResize;

        public string Title { get; }

        public ImageSource Icon { get; }

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
