using Autofac;

namespace OfficeRibbonXEditor.FunctionalTests.Helpers;

public class ViewModelWrapper<T> : ContainerWrapper
    where T : notnull
{
    public T ViewModel => field ??= Container.Resolve<T>();
}