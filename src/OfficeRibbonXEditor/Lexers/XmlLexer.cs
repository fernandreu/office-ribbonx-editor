using ScintillaNET;

namespace OfficeRibbonXEditor.Lexers
{
    public class XmlLexer : ScintillaLexer
    {
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

            // Set the XML Lexer
            editor.Lexer = Lexer.Xml;

            // Show line numbers (this is now done in TextChanged so that width depends on number of digits)
            ////scintilla.Margins[0].Width = 10;

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
            for (var i = Marker.FolderEnd; i <= Marker.FolderOpen; i++)
            {
                editor.Markers[i].SetForeColor(System.Drawing.SystemColors.ControlLightLight);
                editor.Markers[i].SetBackColor(System.Drawing.SystemColors.ControlDark);
            }

            // Style the folder markers
            editor.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            editor.Markers[Marker.Folder].SetBackColor(System.Drawing.SystemColors.ControlText);
            editor.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            editor.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            editor.Markers[Marker.FolderEnd].SetBackColor(System.Drawing.SystemColors.ControlText);
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
            editor.Styles[Style.Default].ForeColor = Properties.Settings.Default.TextColor;
            editor.Styles[Style.Default].BackColor = Properties.Settings.Default.BackgroundColor;
            editor.StyleClearAll();
            editor.Styles[Style.Xml.Attribute].ForeColor = Properties.Settings.Default.AttributeColor;
            editor.Styles[Style.Xml.Entity].ForeColor = Properties.Settings.Default.AttributeColor;
            editor.Styles[Style.Xml.Comment].ForeColor = Properties.Settings.Default.CommentColor;
            editor.Styles[Style.Xml.Tag].ForeColor = Properties.Settings.Default.TagColor;
            editor.Styles[Style.Xml.TagEnd].ForeColor = Properties.Settings.Default.TagColor;
            editor.Styles[Style.Xml.DoubleString].ForeColor = Properties.Settings.Default.StringColor;
            editor.Styles[Style.Xml.SingleString].ForeColor = Properties.Settings.Default.StringColor;
        }
    }
}
