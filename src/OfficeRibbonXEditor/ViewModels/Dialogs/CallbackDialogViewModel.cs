using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Lexers;

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{

    public class CallbackDialogViewModel : DialogBase, IContentDialog<string?>
    {
        private VbaLexer? lexer;

        private string? code;

        public VbaLexer? Lexer
        {
            get => this.lexer;
            set
            {
                if (!this.Set(ref this.lexer, value) || this.Code == null)
                {
                    return;
                }

                if (this.Lexer?.Editor == null)
                {
                    return;
                }

                this.Lexer.Editor.Text = this.Code;
            }
        }

        public string? Code
        {
            get => this.code;
            set
            {
                if (!this.Set(ref this.code, value) || this.Lexer == null)
                {
                    return;
                }

                if (this.Lexer?.Editor == null)
                {
                    return;
                }

                this.Lexer.Editor.Text = this.Code;
            }
        }

        public bool OnLoaded(string? payload)
        {
            this.Code = payload;
            return true;
        }
    }
}
