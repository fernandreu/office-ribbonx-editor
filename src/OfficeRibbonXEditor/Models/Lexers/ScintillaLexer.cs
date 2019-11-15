using ScintillaNET;
using ScintillaNET.WPF;

namespace OfficeRibbonXEditor.Models.Lexers
{
    /// <summary>
    /// Abstract class to handle any update in syntax highlighting (or formatting in general) of the Scintilla editor
    /// without having to explicitly reference the editor itself. This is useful for ViewModels, as they  
    /// </summary>
    public abstract class ScintillaLexer
    {
        private ScintillaWPF editor;

        private int maxLineNumberCharLength;

        public ScintillaWPF Editor
        {
            get => this.editor;
            set
            {
                if (this.editor == value)
                {
                    return;
                }

                if (this.editor != null)
                {
                    this.editor.UpdateUI -= this.ScintillaUpdateUi;
                }

                this.editor = value;

                if (this.editor != null)
                {
                    this.editor.UpdateUI += this.ScintillaUpdateUi;
                }

                this.Update();
            }
        }

        /// <summary>
        /// Updates the Scintilla editor with the properties of this lexer
        /// </summary>
        public void Update()
        {
            if (this.Editor == null)
            {
                return;
            }

            this.UpdateImplementation();
        }

        protected abstract void UpdateImplementation();

        private void ScintillaUpdateUi(object sender, UpdateUIEventArgs e)
        {
            // Did the number of characters in the line number display change?
            // i.e. nnn VS nn, or nnnn VS nn, etc...
            var charLength = this.Editor.Lines.Count.ToString().Length;
            if (charLength == this.maxLineNumberCharLength)
            {
                return;
            }

            // Calculate the width required to display the last line number
            // and include some padding for good measure.
            const int LinePadding = 2;

            this.Editor.Margins[0].Width = this.Editor.TextWidth(ScintillaNET.Style.LineNumber, new string('9', charLength + 1)) + LinePadding;
            this.maxLineNumberCharLength = charLength;
        }
    }
}
