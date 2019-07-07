using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OfficeRibbonXEditor.Models;
using ScintillaNET;
using CharacterRange = OfficeRibbonXEditor.Models.CharacterRange;

namespace OfficeRibbonXEditor.Controls.Forms
{
    public partial class FindAllResultsPanel : UserControl
    {
        #region Fields

        private List<CharacterRange> _findAllResults;
        private Scintilla _scintilla;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates a new instance of FindAllResultsPanel
        /// </summary>
        public FindAllResultsPanel()
        {
            this.InitializeComponent();

            this.FindResultsScintilla.Styles[Style.Default].Font = "Consolas";
            this.FindResultsScintilla.Styles[Style.Default].Size = 10;

            this.FindResultsScintilla.ClearAll();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the Scintilla control that was searched to generate the find results.
        /// Allows the FindAllResults list to be double clicked and results indicated in the original Scintilla.
        /// </summary>
        public Scintilla Scintilla
        {
            get { return this._scintilla; }
            set { this._scintilla = value; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Updates the find all results panel
        /// </summary>
        /// <param name="FindReplace">The FindReplace instance used to generate the find results.</param>
        /// <param name="FindAllResults"></param>
        public void UpdateFindAllResults(FindReplace FindReplace, List<CharacterRange> FindAllResults)
        {
            if (FindReplace.Scintilla == null)
                return;
            else
                this.Scintilla = FindReplace.Scintilla;

            this._findAllResults = new List<CharacterRange>(FindAllResults);

            this.FindResultsScintilla.ClearAll();

            Indicator _indicator = this.FindResultsScintilla.Indicators[16];
            _indicator.ForeColor = Color.Red;
            _indicator.Alpha = 100;
            _indicator.Style = IndicatorStyle.RoundBox;
            _indicator.Under = true;

            this.FindResultsScintilla.IndicatorCurrent = _indicator.Index;

            //Write lines
            foreach (var item in this._findAllResults)
            {
                int startLine = this.Scintilla.LineFromPosition(item.cpMin);
                int endLine = this.Scintilla.LineFromPosition(item.cpMax);

                if (startLine == endLine)
                {
                    string resultsLinePrefix = string.Format("Line {0}: ", startLine + 1);

                    this.FindResultsScintilla.AppendText(string.Format("{0}{1}",
                        resultsLinePrefix, this.Scintilla.Lines[startLine].Text));
                }
            }

            //Highlight
            int resultLineIndex = 0;
            foreach (var item in this._findAllResults)
            {
                int startLine = this.Scintilla.LineFromPosition(item.cpMin);
                int endLine = this.Scintilla.LineFromPosition(item.cpMax);

                if (startLine == endLine)
                {
                    string resultsLinePrefix = string.Format("Line {0}: ", startLine + 1);

                    int LinePos = this.Scintilla.Lines[startLine].Position;
                    int startPosInLine = item.cpMin - LinePos;

                    int lastLineStartPos = this.FindResultsScintilla.Lines[resultLineIndex].Position;

                    this.FindResultsScintilla.IndicatorFillRange(lastLineStartPos + resultsLinePrefix.Length + startPosInLine, item.cpMax - item.cpMin);

                    resultLineIndex++;
                }
            }
        }

        private void FindResultsScintilla_KeyUp(object sender, KeyEventArgs e)
        {
            int pos = this.FindResultsScintilla.CurrentPosition;
            int selectedLine = this.FindResultsScintilla.LineFromPosition(pos);

            if (this._findAllResults.Count > selectedLine)
            {
                CharacterRange CharRange = this._findAllResults[selectedLine];
                this.Scintilla.SetSelection(CharRange.cpMin, CharRange.cpMax);
                this.Scintilla.ScrollCaret();
            }
        }

        private void FindResultsScintilla_MouseClick(object sender, MouseEventArgs e)
        {
            int pos = this.FindResultsScintilla.CharPositionFromPointClose((e.Location).X, (e.Location).Y);
            if (pos == -1)
                return;

            int selectedLine = this.FindResultsScintilla.LineFromPosition(pos);

            CharacterRange CharRange = this._findAllResults[selectedLine];
            this.Scintilla.SetSelection(CharRange.cpMin, CharRange.cpMax);
            this.Scintilla.ScrollCaret();
        }

        private void FindResultsScintilla_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int pos = this.FindResultsScintilla.CharPositionFromPointClose((e.Location).X, (e.Location).Y);
            if (pos == -1)
                return;

            int selectedLine = this.FindResultsScintilla.LineFromPosition(pos);

            CharacterRange CharRange = this._findAllResults[selectedLine];
            this.Scintilla.SetSelection(CharRange.cpMin, CharRange.cpMax);
            this.Scintilla.ScrollCaret();
            this.Scintilla.Focus();
        }

        #endregion Methods
    }
}