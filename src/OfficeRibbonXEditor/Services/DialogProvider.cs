using Autofac;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.Services;

[Export<IDialogProvider>]
public class DialogProvider(ILifetimeScope container) : IDialogProvider
{
    private readonly ILifetimeScope _container = container;

    public T ResolveDialog<T>() where T : IContentDialogBase
    {
        return _container.Resolve<T>();
    }
}