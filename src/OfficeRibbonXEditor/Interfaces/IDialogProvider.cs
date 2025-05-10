namespace OfficeRibbonXEditor.Interfaces;

public interface IDialogProvider
{
    T ResolveDialog<T>() where T : IContentDialogBase;
}