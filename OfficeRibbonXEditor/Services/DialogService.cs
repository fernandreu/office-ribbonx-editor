namespace OfficeRibbonXEditor.Services
{
    using System.Threading.Tasks;
    using OfficeRibbonXEditor.Interfaces;

    public class DialogService : IDialogService
    {
        public void ShowDialog<TDialog, TPayload>(TPayload payload) where TDialog : IContentDialog<TPayload>
        {
            throw new System.NotImplementedException();
        }
    }
}
