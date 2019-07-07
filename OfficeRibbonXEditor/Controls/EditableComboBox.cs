using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OfficeRibbonXEditor.Controls
{
    public class EditableComboBox : ComboBox
    {
        public TextBox TextBox
        {
            get
            {
                this.ApplyTemplate();
                return this.Template.FindName("PART_EditableTextBox", this) as TextBox;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.TextBox.TextChanged += this.OnTextChanged;
            this.AddHandler(PreviewMouseLeftButtonDownEvent, 
                new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
            this.AddHandler(GotKeyboardFocusEvent, 
                new RoutedEventHandler(SelectAllText), true);
            this.AddHandler(MouseDoubleClickEvent, 
                new RoutedEventHandler(SelectAllText), true);
        }

        private static void SelectivelyIgnoreMouseButton(object sender, 
            MouseButtonEventArgs e)
        {
            // Find the TextBox
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBox))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent != null)
            {
                var textBox = (TextBox)parent;
                if (!textBox.IsKeyboardFocusWithin)
                {
                    // If the text box is not yet focussed, give it the focus and
                    // stop further processing of this click event.
                    textBox.Focus();
                    e.Handled = true;
                }
            }
        }

        private static void SelectAllText(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            this.TextBox?.Focus();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = (TextBox) sender;
            txt.TextChanged -= this.OnTextChanged;
            txt.Focus();
        }
    }
}
