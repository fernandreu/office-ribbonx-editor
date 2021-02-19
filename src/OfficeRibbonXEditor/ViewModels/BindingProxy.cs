using System.Windows;

namespace OfficeRibbonXEditor.ViewModels
{
    public class BindingProxy : Freezable
    {
        // Using a DependencyProperty as the backing store for Data.
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(
                nameof(Data), 
                typeof(object), 
                typeof(BindingProxy), 
                new UIPropertyMetadata(null));

        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

    }
}
