namespace OfficeRibbonXEditor.Interfaces
{
    using System.Threading.Tasks;

    /// <summary>
    /// Defines how UI view models should launch dialogs by referencing their corresponding view models
    /// </summary>
    public interface IDialogService
    {
        void ShowDialog<TDialog, TPayload>(TPayload payload) where TDialog : IContentDialog<TPayload>;
    }
}
