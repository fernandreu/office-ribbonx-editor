using CommunityToolkit.Mvvm.ComponentModel;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Lexers;

namespace OfficeRibbonXEditor.ViewModels.Dialogs;

[Export]
public partial class CallbackDialogViewModel : DialogBase, IContentDialog<string?>
{
    [ObservableProperty]
    private VbaLexer? _lexer;

    partial void OnLexerChanged(VbaLexer? value) => OnConfigChanged();
        
    [ObservableProperty]
    private string? _code;

    partial void OnCodeChanged(string? value) => OnConfigChanged();
        
    private void OnConfigChanged()
    {
        if (Lexer == null || Code == null)
        {
            return;
        }
            
        if (Lexer?.Editor == null)
        {
            return;
        }

        Lexer.Editor.Text = Code;
    }
        
    public bool OnLoaded(string? payload)
    {
        Code = payload;
        return true;
    }
}