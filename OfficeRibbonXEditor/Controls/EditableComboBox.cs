using System.Windows;
using System.Windows.Controls;

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
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            this.TextBox?.Focus();
            this.TextBox?.SelectAll();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = (TextBox) sender;
            txt.TextChanged -= this.OnTextChanged;
            txt.Focus();
            txt.SelectAll();
        }
    }
}
