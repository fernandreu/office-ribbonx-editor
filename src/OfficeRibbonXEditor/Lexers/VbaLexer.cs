using System.Drawing;
using ScintillaNET;

namespace OfficeRibbonXEditor.Lexers;

public class VbaLexer : ScintillaLexer
{
    private const string Keywords = 
        "debug\nrelease\naddhandler\naddressof\naggregate\nalias\nand\nandalso\nansi\nas\nassembly\nauto\nbinary\nboolean\nbyref\nbyte\nbyval\ncall\ncase\n" + 
        "catch\ncbool\ncbyte\ncchar\ncdate\ncdbl\ncdec\nchar\ncint\nclass\nclng\ncobj\ncompare\nconst\ncontinue\ncsbyte\ncshort\ncsng\ncstr\nctype\ncuint\nculng\n" + 
        "cushort\ncustom\ndate\ndecimal\ndeclare\ndefault\ndelegate\ndim\ndirectcast\ndistinct\ndo\ndouble\neach\nelse\nelseif\nend\nendif\nenum\nequals\nerase\n" + 
        "error\nevent\nexit\nexplicit\nfalse\nfinally\nfor\nfriend\nfrom\nfunction\nget\ngettype\ngetxmlnamespace\nglobal\ngosub\ngoto\ngroup\nhandles\nif\n" + 
        "implements\nimports\nin\ninherits\ninteger\ninterface\ninto\nis\nisfalse\nisnot\nistrue\njoin\nkey\nlet\nlib\nlike\nlong\nloop\nme\nmid\nmod\nmodule\n" + 
        "mustinherit\nmustoverride\nmy\nmybase\nmyclass\nnamespace\nnarrowing\nnew\nnext\nnot\nnothing\nnotinheritable\nnotoverridable\nobject\nof\noff\non\n" + 
        "operator\noption\noptional\nor\norder\norelse\noverloads\noverridable\noverrides\nparamarray\npartial\npreserve\nprivate\nproperty\nprotected\npublic\n" + 
        "raiseevent\nreadonly\nredim\nrem\nremovehandler\nresume\nreturn\nsbyte\nselect\nset\nshadows\nshared\nshort\nsingle\nskip\nstatic\nstep\nstop\nstrict\n" + 
        "string\nstructure\nsub\nsynclock\ntake\ntext\nthen\nthrow\nto\ntrue\ntry\ntrycast\ntypeof\nuinteger\nulong\nunicode\nuntil\nushort\nusing\nvariant\nwend\n" + 
        "when\nwhere\nwhile\nwidening\nwith\nwithevents\nwriteonly\nxor";

    private const string Literals = "!\n#\n%\n@\n&amp;\ni\nd\nf\nl\nr\ns\nui\nul\nus";

    private const string Other = "_";
        
    protected override void UpdateImplementation()
    {
        if (Editor == null)
        {
            return;
        }

        var editor = Editor;
            
        editor.TabWidth = Properties.Settings.Default.TabWidth;
        editor.WrapMode = Properties.Settings.Default.WrapMode;
        editor.ViewEol = Properties.Settings.Default.ShowWhitespace;
        editor.ViewWhitespace = Properties.Settings.Default.ShowWhitespace ? WhitespaceMode.VisibleAlways : WhitespaceMode.Invisible;

        // Set the VB Lexer
        editor.Lexer = Lexer.Vb;

        // Enable folding
        editor.SetProperty("fold", "1");
        editor.SetProperty("fold.compact", "1");
        editor.SetProperty("fold.html", "1");

        // Use Margin 2 for fold markers
        editor.Margins[2].Type = MarginType.Symbol;
        editor.Margins[2].Mask = Marker.MaskFolders;
        editor.Margins[2].Sensitive = true;
        editor.Margins[2].Width = 20;

        // Reset folder markers
        for (int i = Marker.FolderEnd; i <= Marker.FolderOpen; i++)
        {
            editor.Markers[i].SetForeColor(SystemColors.ControlLightLight);
            editor.Markers[i].SetBackColor(SystemColors.ControlDark);
        }

        // Style the folder markers
        editor.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
        editor.Markers[Marker.Folder].SetBackColor(SystemColors.ControlText);
        editor.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
        editor.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
        editor.Markers[Marker.FolderEnd].SetBackColor(SystemColors.ControlText);
        editor.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
        editor.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
        editor.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
        editor.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

        // Enable automatic folding
        editor.AutomaticFold = AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change;

        // Set the Styles
        editor.StyleResetDefault();

        editor.Styles[Style.Default].Font = "Consolas";
        editor.Styles[Style.Default].Size = Properties.Settings.Default.EditorFontSize;
        editor.Styles[Style.Default].ForeColor = Color.Black;
        editor.Styles[Style.Default].BackColor = Color.White;
        editor.StyleClearAll();
            
        editor.Styles[Style.Vb.BinNumber].ForeColor = Color.OrangeRed;
        editor.Styles[Style.Vb.Comment].ForeColor = Color.Green;
        editor.Styles[Style.Vb.CommentBlock].ForeColor = Color.Green;
        editor.Styles[Style.Vb.Constant].ForeColor = Color.DarkMagenta;
        editor.Styles[Style.Vb.Date].ForeColor = Color.DarkMagenta;
        editor.Styles[Style.Vb.Default].ForeColor = Color.Black;
        editor.Styles[Style.Vb.DocBlock].ForeColor = Color.Green;
        editor.Styles[Style.Vb.DocKeyword].ForeColor = Color.FromArgb(255, 64, 47, 241);
        editor.Styles[Style.Vb.DocLine].ForeColor = Color.Green;
        editor.Styles[Style.Vb.Error].ForeColor = Color.DarkRed;
        editor.Styles[Style.Vb.Error].Bold = true;
        editor.Styles[Style.Vb.HexNumber].ForeColor = Color.DarkOrange;
        editor.Styles[Style.Vb.Identifier].ForeColor = Color.Black;
        editor.Styles[Style.Vb.Keyword].ForeColor = Color.FromArgb(255, 72, 64, 213);
        editor.Styles[Style.Vb.Keyword2].ForeColor = Color.DarkCyan;
        editor.Styles[Style.Vb.Keyword3].ForeColor = Color.OrangeRed;
        editor.Styles[Style.Vb.Keyword4].ForeColor = Color.DarkRed;
        editor.Styles[Style.Vb.Label].ForeColor = Color.FromArgb(255, 72, 64, 213);
        editor.Styles[Style.Vb.Number].ForeColor = Color.OrangeRed;
        editor.Styles[Style.Vb.Operator].ForeColor = Color.Gray;
        editor.Styles[Style.Vb.Operator].Bold = true;
        editor.Styles[Style.Vb.Preprocessor].ForeColor = Color.Gray;
        editor.Styles[Style.Vb.String].ForeColor = Color.Brown;
        editor.Styles[Style.Vb.StringEol].ForeColor = Color.Black;
        editor.Styles[Style.Vb.StringEol].FillLine = true;

        editor.SetKeywords(0, Keywords);
        editor.SetKeywords(2, Literals);
        editor.SetKeywords(3, Other);
    }
}