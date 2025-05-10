using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OfficeRibbonXEditor.Views.Controls;

public class ClickSelectTextBox : TextBox
{
    public ClickSelectTextBox()
    {
        AddHandler(PreviewMouseLeftButtonDownEvent, 
            new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
        AddHandler(GotKeyboardFocusEvent, 
            new RoutedEventHandler(SelectAllText), true);
        AddHandler(MouseDoubleClickEvent, 
            new RoutedEventHandler(SelectAllText), true);
    }

    private static void SelectivelyIgnoreMouseButton(object? sender, MouseButtonEventArgs e)
    {
        // Find the TextBox
        DependencyObject? parent = e.OriginalSource as UIElement;
        while (parent != null && parent is not TextBox)
            parent = VisualTreeHelper.GetParent(parent);

        if (parent != null)
        {
            var textBox = (TextBox)parent;
            if (!textBox.IsKeyboardFocusWithin)
            {
                // If the text box is not yet focused, give it the focus and
                // stop further processing of this click event.
                textBox.Focus();
                e.Handled = true;
            }
        }
    }

    private static void SelectAllText(object? sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }
}