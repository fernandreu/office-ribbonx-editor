namespace OfficeRibbonXEditor.Services
{
    using System.Threading.Tasks;
    using OfficeRibbonXEditor.Interfaces;

    public class DialogService : IDialogService
    {
        public Task<TResult> ShowDialogAsync<TDialog, TPayload, TResult>(TPayload payload) where TDialog : IContentDialog<TPayload, TResult>
        {
            throw new System.NotImplementedException();
        }
    }
}
