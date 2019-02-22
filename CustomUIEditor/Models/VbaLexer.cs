// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VbaLexer.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the VbaLexer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Models
{
    using System.Drawing;

    using ScintillaNET;

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
            if (this.Editor == null)
            {
                return;
            }

            var scintilla = this.Editor;

            // Set the VB Lexer
            scintilla.Lexer = Lexer.Vb;

            // Enable folding
            scintilla.SetProperty("fold", "1");
            scintilla.SetProperty("fold.compact", "1");
            scintilla.SetProperty("fold.html", "1");

            // Use Margin 2 for fold markers
            scintilla.Margins[2].Type = MarginType.Symbol;
            scintilla.Margins[2].Mask = Marker.MaskFolders;
            scintilla.Margins[2].Sensitive = true;
            scintilla.Margins[2].Width = 20;

            // Reset folder markers
            for (int i = Marker.FolderEnd; i <= Marker.FolderOpen; i++)
            {
                scintilla.Markers[i].SetForeColor(SystemColors.ControlLightLight);
                scintilla.Markers[i].SetBackColor(SystemColors.ControlDark);
            }

            // Style the folder markers
            scintilla.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            scintilla.Markers[Marker.Folder].SetBackColor(SystemColors.ControlText);
            scintilla.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            scintilla.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            scintilla.Markers[Marker.FolderEnd].SetBackColor(SystemColors.ControlText);
            scintilla.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            scintilla.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
            scintilla.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            scintilla.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            scintilla.AutomaticFold = AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change;

            // Set the Styles
            scintilla.StyleResetDefault();

            scintilla.Styles[Style.Default].Font = "Consolas";
            scintilla.Styles[Style.Default].Size = Properties.Settings.Default.EditorFontSize;
            scintilla.Styles[Style.Default].ForeColor = Color.Black;
            scintilla.Styles[Style.Default].BackColor = Color.White;
            scintilla.StyleClearAll();
            
            scintilla.Styles[Style.Vb.BinNumber].ForeColor = Color.OrangeRed;
            scintilla.Styles[Style.Vb.Comment].ForeColor = Color.Green;
            scintilla.Styles[Style.Vb.CommentBlock].ForeColor = Color.Green;
            scintilla.Styles[Style.Vb.Constant].ForeColor = Color.DarkMagenta;
            scintilla.Styles[Style.Vb.Date].ForeColor = Color.DarkMagenta;
            scintilla.Styles[Style.Vb.Default].ForeColor = Color.Black;
            scintilla.Styles[Style.Vb.DocBlock].ForeColor = Color.Green;
            scintilla.Styles[Style.Vb.DocKeyword].ForeColor = Color.FromArgb(255, 64, 47, 241);
            scintilla.Styles[Style.Vb.DocLine].ForeColor = Color.Green;
            scintilla.Styles[Style.Vb.Error].ForeColor = Color.DarkRed;
            scintilla.Styles[Style.Vb.Error].Bold = true;
            scintilla.Styles[Style.Vb.HexNumber].ForeColor = Color.DarkOrange;
            scintilla.Styles[Style.Vb.Identifier].ForeColor = Color.Black;
            scintilla.Styles[Style.Vb.Keyword].ForeColor = Color.FromArgb(255, 72, 64, 213);
            scintilla.Styles[Style.Vb.Keyword2].ForeColor = Color.DarkCyan;
            scintilla.Styles[Style.Vb.Keyword3].ForeColor = Color.OrangeRed;
            scintilla.Styles[Style.Vb.Keyword4].ForeColor = Color.DarkRed;
            scintilla.Styles[Style.Vb.Label].ForeColor = Color.FromArgb(255, 72, 64, 213);
            scintilla.Styles[Style.Vb.Number].ForeColor = Color.OrangeRed;
            scintilla.Styles[Style.Vb.Operator].ForeColor = Color.Gray;
            scintilla.Styles[Style.Vb.Operator].Bold = true;
            scintilla.Styles[Style.Vb.Preprocessor].ForeColor = Color.Gray;
            scintilla.Styles[Style.Vb.String].ForeColor = Color.Brown;
            scintilla.Styles[Style.Vb.StringEol].ForeColor = Color.Black;
            scintilla.Styles[Style.Vb.StringEol].FillLine = true;

            scintilla.SetKeywords(0, Keywords);
            scintilla.SetKeywords(2, Literals);
            scintilla.SetKeywords(3, Other);
        }
    }
}
