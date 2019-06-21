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

                this.Target = this.CurrentLineNumber.ToString();
            }
        }

        public int CurrentLineNumber => (this.Lexer?.Editor.CurrentLine ?? -1) + 1;

        public int MaximumLineNumber => this.Lexer?.Editor.Lines.Count ?? 0;

        private string target;

        public string Target
        {
            get => this.target;
            set => this.Set(ref this.target, value);
        }

        public void OnLoaded(ScintillaLexer payload)
        {
            this.Lexer = payload;
        }

        private void ExecuteAcceptCommand()
        {
            if (!int.TryParse(this.Target, out var line))
            {
                // TODO: Error message (original dialog shows an icon with a tooltip)
                return;
            }

            // Translate it to 0-based line number
            line--;

            if (line >= this.MaximumLineNumber)
            {
                line = this.MaximumLineNumber - 1;
            }

            if (line < 0)
            {
                line = 0;
            }

            this.Lexer.Editor.Scintilla.Lines[line].Goto();
            this.Close();
            this.Lexer.Editor.Scintilla.Focus();
        }
    }
}
