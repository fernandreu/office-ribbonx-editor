using Autofac;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.Services
{
    [Export(typeof(IDialogProvider))]
    public class DialogProvider : IDialogProvider
    {
        private readonly ILifetimeScope container;

        public DialogProvider(ILifetimeScope container)
        {
            this.container = container;
        }

        public T ResolveDialog<T>() where T : IContentDialogBase
        {
            return this.container.Resolve<T>();
        }
    }
}
