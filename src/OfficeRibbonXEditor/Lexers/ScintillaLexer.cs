using System.Globalization;
using ScintillaNET;
using ScintillaNET.WPF;

namespace OfficeRibbonXEditor.Lexers
{
    /// <summary>
    /// Abstract class to handle any update in syntax highlighting (or formatting in general) of the Scintilla editor
    /// without having to explicitly reference the editor itself. This is useful for ViewModels, as they  
    /// </summary>
    public abstract class ScintillaLexer
    {
        private ScintillaWPF? _editor;

        private int _maxLineNumberCharLength;

        public ScintillaWPF? Editor
        {
            get => _editor;
            set
            {
                if (_editor == value)
                {
                    return;
                }

                if (_editor != null)
                {
                    _editor.UpdateUI -= ScintillaUpdateUi;
                }

                _editor = value;

                if (_editor != null)
                {
                    _editor.UpdateUI += ScintillaUpdateUi;
                }

                Update();
            }
        }

        /// <summary>
        /// Updates the Scintilla editor with the properties of this lexer
        /// </summary>
        public void Update()
        {
            if (Editor == null)
            {
                return;
            }

            UpdateImplementation();

            // Ensure cursor is visible by making it have the same color as the normal text
            Editor.Scintilla.CaretForeColor = Editor.Styles[Style.Default].ForeColor;
        }

        protected abstract void UpdateImplementation();

        private void ScintillaUpdateUi(object? sender, UpdateUIEventArgs e)
        {
            if (Editor == null)
            {
                return;
            }

            // Did the number of characters in the line number display change?
            // i.e. nnn VS nn, or nnnn VS nn, etc...
            var charLength = Editor.Lines.Count.ToString(CultureInfo.CurrentCulture).Length;
            if (charLength == _maxLineNumberCharLength)
            {
                return;
            }

            // Calculate the width required to display the last line number
            // and include some padding for good measure.
            const int linePadding = 2;

            Editor.Margins[0].Width = Editor.TextWidth(Style.LineNumber, new string('9', charLength + 1)) + linePadding;
            _maxLineNumberCharLength = charLength;
        }
    }
}
