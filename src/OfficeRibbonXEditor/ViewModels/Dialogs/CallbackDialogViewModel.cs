using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Lexers;

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{
    [Export]
    public class CallbackDialogViewModel : DialogBase, IContentDialog<string?>
    {
        private VbaLexer? _lexer;

        private string? _code;

        public VbaLexer? Lexer
        {
            get => _lexer;
            set
            {
                if (!Set(ref _lexer, value) || Code == null)
                {
                    return;
                }

                if (Lexer?.Editor == null)
                {
                    return;
                }

                Lexer.Editor.Text = Code;
            }
        }

        public string? Code
        {
            get => _code;
            set
            {
                if (!Set(ref _code, value) || Lexer == null)
                {
                    return;
                }

                if (Lexer?.Editor == null)
                {
                    return;
                }

                Lexer.Editor.Text = Code;
            }
        }

        public bool OnLoaded(string? payload)
        {
            Code = payload;
            return true;
        }
    }
}
