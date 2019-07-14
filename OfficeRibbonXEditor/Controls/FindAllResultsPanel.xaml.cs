using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using OfficeRibbonXEditor.Models;
using ScintillaNET;
using CharacterRange = OfficeRibbonXEditor.Models.CharacterRange;
using UserControl = System.Windows.Controls.UserControl;

namespace OfficeRibbonXEditor.Controls
{
    /// <summary>
    /// Interaction logic for FindAllResultsPanel.xaml
    /// </summary>
    public partial class FindAllResultsPanel : UserControl
    {
        private List<CharacterRange> findAllResults;

        public FindAllResultsPanel()
        {
            this.InitializeComponent();

            this.FindResultsScintilla.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            this.FindResultsScintilla.Styles[ScintillaNET.Style.Default].Size = 10;

            this.FindResultsScintilla.ClearAll();

            this.FindResultsScintilla.Scintilla.KeyUp += this.FindResultsScintilla_KeyUp;
            this.FindResultsScintilla.Scintilla.MouseClick += this.FindResultsScintilla_MouseClick;
            this.FindResultsScintilla.Scintilla.MouseDoubleClick += this.FindResultsScintilla_MouseDoubleClick;
        }

        public Scintilla Scintilla { get; set; }
        
        /// <summary>
        /// Updates the find all results panel
        /// </summary>
        /// <param name="findReplace">The FindReplace instance used to generate the find results.</param>
        /// <param name="results"></param>
        public void UpdateFindAllResults(FindReplace findReplace, List<CharacterRange> results)
        {
            if (findReplace.Scintilla == null)
                return;
            else
                this.Scintilla = findReplace.Scintilla;

            this.findAllResults = new List<CharacterRange>(results);

            this.FindResultsScintilla.ClearAll();

            var indicator = this.FindResultsScintilla.Indicators[16];
            indicator.ForeColor = Color.Red;
            indicator.Alpha = 100;
            indicator.Style = IndicatorStyle.RoundBox;
            indicator.Under = true;

            this.FindResultsScintilla.IndicatorCurrent = indicator.Index;

            //Write lines
            foreach (var item in this.findAllResults)
            {
                var startLine = this.Scintilla.LineFromPosition(item.cpMin);
                var endLine = this.Scintilla.LineFromPosition(item.cpMax);

                if (startLine == endLine)
                {
                    var resultsLinePrefix = $"Line {startLine + 1}: ";

                    this.FindResultsScintilla.AppendText($"{resultsLinePrefix}{this.Scintilla.Lines[startLine].Text}");
                }
            }

            //Highlight
            var resultLineIndex = 0;
            foreach (var item in this.findAllResults)
            {
                var startLine = this.Scintilla.LineFromPosition(item.cpMin);
                var endLine = this.Scintilla.LineFromPosition(item.cpMax);

                if (startLine == endLine)
                {
                    var resultsLinePrefix = $"Line {startLine + 1}: ";

                    var linePos = this.Scintilla.Lines[startLine].Position;
                    var startPosInLine = item.cpMin - linePos;

                    var lastLineStartPos = this.FindResultsScintilla.Lines[resultLineIndex].Position;

                    this.FindResultsScintilla.IndicatorFillRange(lastLineStartPos + resultsLinePrefix.Length + startPosInLine, item.cpMax - item.cpMin);

                    resultLineIndex++;
                }
            }
        }

        private void FindResultsScintilla_KeyUp(object sender, KeyEventArgs e)
        {
            var pos = this.FindResultsScintilla.CurrentPosition;
            this.GoToPosition(pos);
        }

        private void FindResultsScintilla_MouseClick(object sender, MouseEventArgs e)
        {
            var pos = this.FindResultsScintilla.CharPositionFromPointClose((e.Location).X, (e.Location).Y);
            if (pos == -1)
                return;
            this.GoToPosition(pos);
        }

        private void FindResultsScintilla_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var pos = this.FindResultsScintilla.CharPositionFromPointClose((e.Location).X, (e.Location).Y);
            if (pos == -1)
                return;
            this.GoToPosition(pos);
        }

        private void GoToPosition(int pos)
        {
            var selectedLine = this.FindResultsScintilla.LineFromPosition(pos);

            var charRange = this.findAllResults[selectedLine];
            this.Scintilla.GotoPosition(charRange.cpMax);
            this.Scintilla.GotoPosition(charRange.cpMin);
            this.Scintilla.SetSelection(charRange.cpMin, charRange.cpMax);
            this.Scintilla.ScrollCaret();
            this.Scintilla.Focus();
        }
    }
}
