using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OfficeRibbonXEditor.Views.Controls
{
    public class EditableComboBox : ComboBox
    {
        public EditableComboBox()
        {
            IsEditable = true;
        }

        private TextBox? _textBox;

        public TextBox? TextBox
        {
            get
            {
                if (_textBox != null)
                {
                    return _textBox;
                }

                ApplyTemplate();
                _textBox = Template.FindName("PART_EditableTextBox", this) as TextBox;
                if (_textBox == null)
                {
                    throw new InvalidOperationException($"Make sure IsEditable is set to true when using an {nameof(EditableComboBox)}");
                }

                return _textBox;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (TextBox != null)
            {
                TextBox.TextChanged += OnTextChanged;
            }

            AddHandler(PreviewMouseLeftButtonDownEvent, 
                new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
            AddHandler(GotKeyboardFocusEvent, 
                new RoutedEventHandler(SelectAllText), true);
            AddHandler(MouseDoubleClickEvent, 
                new RoutedEventHandler(SelectAllText), true);
        }

        private static void SelectivelyIgnoreMouseButton(object sender, 
            MouseButtonEventArgs e)
        {
            // Find the TextBox
            DependencyObject? parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBox))
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

            TextBox?.Focus();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = (TextBox) sender;
            txt.TextChanged -= OnTextChanged;
            txt.Focus();
        }
    }
}
