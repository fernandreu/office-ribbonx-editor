// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BindingProxy.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the BindingProxy type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OfficeRibbonXEditor.ViewModels
{
    using System.Windows;

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
            get => this.GetValue(DataProperty);
            set => this.SetValue(DataProperty, value);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

    }
}
