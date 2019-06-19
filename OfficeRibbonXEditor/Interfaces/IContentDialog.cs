namespace OfficeRibbonXEditor.Interfaces
{
    using System;
    using System.Threading.Tasks;

    public interface IContentDialogBase
    {
        bool Cancelled { get; }

        event EventHandler Closed;
    }

    public interface IContentDialog<in TPayload, out TResult> : IContentDialogBase
    {
        Task OnLoadedAsync(TPayload payload);

        TResult Result { get; }
    }
}
