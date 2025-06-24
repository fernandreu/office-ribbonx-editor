using Autofac;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.Services;

[Export(typeof(IDialogProvider))]
public class DialogProvider : IDialogProvider
{
    private readonly ILifetimeScope _container;

    public DialogProvider(ILifetimeScope container)
    {
        _container = container;
    }

    public T ResolveDialog<T>() where T : IContentDialogBase
    {
        return _container.Resolve<T>();
    }
}