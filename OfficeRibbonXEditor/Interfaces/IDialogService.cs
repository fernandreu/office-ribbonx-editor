namespace OfficeRibbonXEditor.Interfaces
{
    using System.Threading.Tasks;

    /// <summary>
    /// Defines how UI view models should launch dialogs by referencing their corresponding view models
    /// </summary>
    public interface IDialogService
    {
        Task<TResult> ShowDialogAsync<TDialog, TPayload, TResult>(TPayload payload)
            where TDialog : IContentDialog<TPayload, TResult>;
    }
}
