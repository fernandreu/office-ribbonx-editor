using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Lexers;

namespace OfficeRibbonXEditor.ViewModels.Dialogs;

[Export]
public partial class GoToDialogViewModel : DialogBase, IContentDialog<ScintillaLexer>
{
    private ScintillaLexer? _lexer;
    public ScintillaLexer? Lexer
    {
        get => _lexer;
        set
        {
            if (!SetProperty(ref _lexer, value))
            {
                return;
            }

            OnPropertyChanged(nameof(CurrentLineNumber));
            OnPropertyChanged(nameof(MaximumLineNumber));

            if (Lexer == null)
            {
                return;
            }

            Target = CurrentLineNumber;
            LexerChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? LexerChanged;

    public int CurrentLineNumber => (Lexer?.Editor?.CurrentLine ?? -1) + 1;

    public int MaximumLineNumber => Lexer?.Editor?.Lines.Count ?? 0;

    [ObservableProperty]
    private int _target;

    public bool OnLoaded(ScintillaLexer payload)
    {
        Lexer = payload;
        return true;
    }

    [RelayCommand]
    private void Accept()
    {
        // Translate it to 0-based line number
        var line = Target - 1;

        // This trimming might not be necessary due to how the IntegerUpDown control will clamp the target 
        if (line > MaximumLineNumber)
        {
            line = MaximumLineNumber;
        }
        else if (line < 0)
        {
            line = 0;
        }

        Lexer?.Editor?.Scintilla.Lines[line].Goto();
        Close();
        Lexer?.Editor?.Scintilla.Focus();
    }
}