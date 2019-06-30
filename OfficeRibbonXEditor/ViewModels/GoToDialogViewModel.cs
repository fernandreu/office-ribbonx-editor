using System;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Models;

namespace OfficeRibbonXEditor.ViewModels
{
    public class GoToDialogViewModel : DialogBase, IContentDialog<ScintillaLexer>
    {
        public GoToDialogViewModel()
        {
            this.AcceptCommand = new RelayCommand(this.ExecuteAcceptCommand);
        }

        public RelayCommand AcceptCommand { get; }

        private ScintillaLexer lexer;

        public ScintillaLexer Lexer
        {
            get => this.lexer;
            set
            {
                if (!this.Set(ref this.lexer, value))
                {
                    return;
                }

                this.RaisePropertyChanged(nameof(this.CurrentLineNumber));
                this.RaisePropertyChanged(nameof(this.MaximumLineNumber));

                if (this.Lexer == null)
                {
                    return;
                }

                this.Target = this.CurrentLineNumber;
                this.LexerChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler LexerChanged;

        public int CurrentLineNumber => (this.Lexer?.Editor.CurrentLine ?? -1) + 1;

        public int MaximumLineNumber => this.Lexer?.Editor.Lines.Count ?? 0;

        private int target;

        public int Target
        {
            get => this.target;
            set => this.Set(ref this.target, value);
        }

        public bool OnLoaded(ScintillaLexer payload)
        {
            this.Lexer = payload;
            return true;
        }

        private void ExecuteAcceptCommand()
        {
            // Translate it to 0-based line number
            var line = this.Target - 1;

            // This trimming might not be necessary due to how the IntegerUpDown control will clamp the target 
            if (line > this.MaximumLineNumber)
            {
                line = this.MaximumLineNumber;
            }
            else if (line < 0)
            {
                line = 0;
            }

            this.Lexer.Editor.Scintilla.Lines[line].Goto();
            this.Close();
            this.Lexer.Editor.Scintilla.Focus();
        }
    }
}
