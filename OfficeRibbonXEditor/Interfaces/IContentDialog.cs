namespace OfficeRibbonXEditor.Interfaces
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Media;
    using GalaSoft.MvvmLight.Command;

    public interface IContentDialogBase
    {
        bool Cancelled { get; }

        string Title { get; }

        ImageSource Icon { get; }

        ResizeMode ResizeMode { get; }

        RelayCommand<CancelEventArgs> ClosingCommand { get; }

        event EventHandler Closed;
    }

    public interface IContentDialog<in TPayload> : IContentDialogBase
    {
        void OnLoaded(TPayload payload);
    }
}
