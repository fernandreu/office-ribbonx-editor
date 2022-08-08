using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OfficeRibbonXEditor.Interfaces
{
    public interface IContentDialogBase
    {
        /// <summary>
        /// Gets Whether only one instance of this content (i.e. of its containing dialog) should be available
        /// at all times. Subsequent attempts should either do nothing or activate the already existing dialog.
        /// </summary>
        bool IsUnique { get; }

        /// <summary>
        /// Gets whether the dialog was already closed. This is used to check whether a new unique dialog
        /// should be created, or whether the same one should be reactivated on screen.
        /// </summary>
        bool IsClosed { get; }

        /// <summary>
        /// Gets whether the dialog was closed without following the expected OK / Accept / etc. action. This
        /// can be useful if the dialog merely shows some settings which have to be then processed by the
        /// calling window.
        /// </summary>
        bool IsCancelled { get; }

        IRelayCommand<CancelEventArgs> ClosingCommand { get; }

        event EventHandler Closed;
    }

    public interface IContentDialog<in TPayload> : IContentDialogBase
    {
        bool OnLoaded(TPayload payload);
    }
}
