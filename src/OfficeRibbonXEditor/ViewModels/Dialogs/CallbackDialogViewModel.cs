using CommunityToolkit.Mvvm.ComponentModel;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Lexers;

namespace OfficeRibbonXEditor.ViewModels.Dialogs;

[Export]
public partial class CallbackDialogViewModel : DialogBase, IContentDialog<string?>
{
    [ObservableProperty]
    public partial VbaLexer? Lexer { get; set; }

    partial void OnLexerChanged(VbaLexer? value) => OnConfigChanged();

    [ObservableProperty]
    public partial string? Code { get; set; }

    partial void OnCodeChanged(string? value) => OnConfigChanged();
        
    private void OnConfigChanged()
    {
        if (Lexer == null || Code == null)
        {
            return;
        }

        Lexer?.Editor?.Text = Code;
    }
        
    public bool OnLoaded(string? payload)
    {
        Code = payload;
        return true;
    }
}