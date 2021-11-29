using Autofac;

namespace OfficeRibbonXEditor.FunctionalTests.Helpers
{
    public class ViewModelWrapper<T> : ContainerWrapper
        where T : notnull
    {
        private T? _viewModel;
        public T ViewModel => _viewModel ??= Container.Resolve<T>();
    }
}
