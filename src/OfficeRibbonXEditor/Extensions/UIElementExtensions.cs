using System.Windows;

namespace OfficeRibbonXEditor.Extensions
{
    public static class UiElementExtensions
    {
        public static bool GetIsFocused(DependencyObject d)
        {
            return (bool?) d?.GetValue(IsFocusedProperty) ?? false;
        }

        public static void SetIsFocused(DependencyObject d, bool value)
        {
            d?.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused", 
                typeof(bool), 
                typeof(UiElementExtensions),
                new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        private static void OnIsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uie = (UIElement) d;
            if ((bool) e.NewValue)
            {
                uie.Focus(); // Don't care about false values.
            }
        }
    }
}
