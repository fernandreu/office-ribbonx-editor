using OfficeRibbonXEditor.Models;

namespace OfficeRibbonXEditor.Dialogs.FindReplace.FindReplace
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using ScintillaNET;
    using CharacterRange = CharacterRange;

    public partial class FindReplaceDialog : Form
    {
        #region Fields

        private bool autoPosition;
        private BindingSource bindingSourceFind = new BindingSource();
        private BindingSource bindingSourceReplace = new BindingSource();
        private List<string> mruFind;
        private int mruMaxCount = 10;
        private List<string> mruReplace;
        private Scintilla scintilla;
        private CharacterRange searchRange;

        #endregion Fields

        public event KeyPressedHandler KeyPressed;

        public delegate void KeyPressedHandler(object sender, KeyEventArgs e);

        #region Constructors

        public FindReplaceDialog()
        {
            this.InitializeComponent();

            this.autoPosition = true;
            this.mruFind = new List<string>();
            this.mruReplace = new List<string>();
            this.bindingSourceFind.DataSource = this.mruFind;
            this.bindingSourceReplace.DataSource = this.mruReplace;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets whether the dialog should automatically move away from the current
        /// selection to prevent obscuring it.
        /// </summary>
        /// <returns>true to automatically move away from the current selection; otherwise, false.</returns>
        public bool AutoPosition
        {
            get => this.autoPosition;
            set => this.autoPosition = value;
        }

        public List<string> MruFind
        {
            get => this.mruFind;
            set
            {
                this.mruFind = value;
                this.bindingSourceFind.DataSource = this.mruFind;
            }
        }

        public int MruMaxCount
        {
            get => this.mruMaxCount;
            set => this.mruMaxCount = value;
        }

        public List<string> MruReplace
        {
            get => this.mruReplace;
            set
            {
                this.mruReplace = value;
                this.bindingSourceReplace.DataSource = this.mruReplace;
            }
        }

        public Scintilla Scintilla
        {
            get => this.scintilla;
            set => this.scintilla = value;
        }

        public FindReplace FindReplace { get; set; }

        #endregion Properties

        #region Form Event Handlers

        private void FindReplaceDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        #endregion Form Event Handlers

        #region Event Handlers

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.scintilla.MarkerDeleteAll(this.FindReplace.Marker.Index);
            this.FindReplace.ClearAllHighlights();
        }

        private void btnFindAll_Click(object sender, EventArgs e)
        {
            if (this.txtFindF.Text == string.Empty)
                return;

            this.AddFindMru();

            this.lblStatus.Text = string.Empty;

            this.btnClear_Click(null, null);
            int foundCount = 0;

            #region RegEx

            if (this.rdoRegexF.Checked)
            {
                Regex rr = null;
                try
                {
                    rr = new Regex(this.txtFindF.Text, this.GetRegexOptions());
                }
                catch (ArgumentException ex)
                {
                    this.lblStatus.Text = "Error in Regular Expression: " + ex.Message;
                    return;
                }

                if (this.chkSearchSelectionF.Checked)
                {
                    if (this.searchRange.cpMin == this.searchRange.cpMax)
                    {
                        this.searchRange = new CharacterRange(this.scintilla.Selections[0].Start, this.scintilla.Selections[0].End);
                    }

                    foundCount = this.FindReplace.FindAll(this.searchRange, rr, this.chkMarkLine.Checked, this.chkHighlightMatches.Checked).Count;
                }
                else
                {
                    this.searchRange = new CharacterRange();
                    foundCount = this.FindReplace.FindAll(rr, this.chkMarkLine.Checked, this.chkHighlightMatches.Checked).Count;
                }
            }

            #endregion

            #region Non-RegEx

            if (!this.rdoRegexF.Checked)
            {
                if (this.chkSearchSelectionF.Checked)
                {
                    if (this.searchRange.cpMin == this.searchRange.cpMax) this.searchRange = new CharacterRange(this.scintilla.Selections[0].Start, this.scintilla.Selections[0].End);

                    string textToFind = this.rdoExtendedF.Checked ? this.FindReplace.Transform(this.txtFindF.Text) : this.txtFindF.Text;
                    foundCount = this.FindReplace.FindAll(this.searchRange, textToFind, this.GetSearchFlags(), this.chkMarkLine.Checked, this.chkHighlightMatches.Checked).Count;
                }
                else
                {
                    this.searchRange = new CharacterRange();
                    string textToFind = this.rdoExtendedF.Checked ? this.FindReplace.Transform(this.txtFindF.Text) : this.txtFindF.Text;
                    foundCount = this.FindReplace.FindAll(textToFind, this.GetSearchFlags(), this.chkMarkLine.Checked, this.chkHighlightMatches.Checked).Count;
                }
            }

            #endregion

            this.lblStatus.Text = "Total found: " + foundCount.ToString();
        }

        private void btnFindNext_Click(object sender, EventArgs e)
        {
            this.FindNext();
        }

        private void btnFindPrevious_Click(object sender, EventArgs e)
        {
            this.FindPrevious();
        }

        private void btnReplaceAll_Click(object sender, EventArgs e)
        {
            if (this.txtFindR.Text == string.Empty)
                return;

            this.AddReplaceMru();
            this.lblStatus.Text = string.Empty;
            int foundCount = 0;

            #region RegEx

            if (this.rdoRegexR.Checked)
            {
                Regex rr = null;
                try
                {
                    rr = new Regex(this.txtFindR.Text, this.GetRegexOptions());
                }
                catch (ArgumentException ex)
                {
                    this.lblStatus.Text = "Error in Regular Expression: " + ex.Message;
                    return;
                }

                if (this.chkSearchSelectionR.Checked)
                {
                    if (this.searchRange.cpMin == this.searchRange.cpMax)
                    {
                        this.searchRange = new CharacterRange(this.scintilla.Selections[0].Start, this.scintilla.Selections[0].End);
                    }

                    foundCount = this.FindReplace.ReplaceAll(this.searchRange, rr, this.txtReplace.Text, false, false);
                }
                else
                {
                    this.searchRange = new CharacterRange();
                    foundCount = this.FindReplace.ReplaceAll(rr, this.txtReplace.Text, false, false);
                }
            }

            #endregion

            #region Non-RegEx

            if (!this.rdoRegexR.Checked)
            {
                if (this.chkSearchSelectionR.Checked)
                {
                    if (this.searchRange.cpMin == this.searchRange.cpMax) this.searchRange = new CharacterRange(this.scintilla.Selections[0].Start, this.scintilla.Selections[0].End);

                    string textToFind = this.rdoExtendedR.Checked ? this.FindReplace.Transform(this.txtFindR.Text) : this.txtFindR.Text;
                    string textToReplace = this.rdoExtendedR.Checked ? this.FindReplace.Transform(this.txtReplace.Text) : this.txtReplace.Text;
                    foundCount = this.FindReplace.ReplaceAll(this.searchRange, textToFind, textToReplace, this.GetSearchFlags(), false, false);
                }
                else
                {
                    this.searchRange = new CharacterRange();
                    string textToFind = this.rdoExtendedR.Checked ? this.FindReplace.Transform(this.txtFindR.Text) : this.txtFindR.Text;
                    string textToReplace = this.rdoExtendedR.Checked ? this.FindReplace.Transform(this.txtReplace.Text) : this.txtReplace.Text;
                    foundCount = this.FindReplace.ReplaceAll(textToFind, textToReplace, this.GetSearchFlags(), false, false);
                }
            }

            #endregion

            this.lblStatus.Text = "Total Replaced: " + foundCount.ToString();
        }

        private void btnReplaceNext_Click(object sender, EventArgs e)
        {
            this.ReplaceNext();
        }

        private void btnReplacePrevious_Click(object sender, EventArgs e)
        {
            if (this.txtFindR.Text == string.Empty)
                return;

            this.AddReplaceMru();
            this.lblStatus.Text = string.Empty;

            CharacterRange nextRange;
            try
            {
                nextRange = this.ReplaceNext(true);
            }
            catch (ArgumentException ex)
            {
                this.lblStatus.Text = "Error in Regular Expression: " + ex.Message;
                return;
            }

            if (nextRange.cpMin == nextRange.cpMax)
            {
                this.lblStatus.Text = "Match could not be found";
            }
            else
            {
                if (nextRange.cpMin > this.scintilla.AnchorPosition)
                {
                    if (this.chkSearchSelectionR.Checked)
                        this.lblStatus.Text = "Search match wrapped to the beginning of the selection";
                    else
                        this.lblStatus.Text = "Search match wrapped to the beginning of the document";
                }

                this.scintilla.SetSel(nextRange.cpMin, nextRange.cpMax);
                this.MoveFormAwayFromSelection();
            }
        }

        private void chkEcmaScript_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                this.chkExplicitCaptureF.Checked = false;
                this.chkExplicitCaptureR.Checked = false;
                this.chkExplicitCaptureF.Enabled = false;
                this.chkExplicitCaptureR.Enabled = false;
                this.chkIgnorePatternWhitespaceF.Checked = false;
                this.chkIgnorePatternWhitespaceR.Checked = false;
                this.chkIgnorePatternWhitespaceF.Enabled = false;
                this.chkIgnorePatternWhitespaceR.Enabled = false;
                this.chkRightToLeftF.Checked = false;
                this.chkRightToLeftR.Checked = false;
                this.chkRightToLeftF.Enabled = false;
                this.chkRightToLeftR.Enabled = false;
                this.chkSinglelineF.Checked = false;
                this.chkSinglelineR.Checked = false;
                this.chkSinglelineF.Enabled = false;
                this.chkSinglelineR.Enabled = false;
            }
            else
            {
                this.chkExplicitCaptureF.Enabled = true;
                this.chkIgnorePatternWhitespaceF.Enabled = true;
                this.chkRightToLeftF.Enabled = true;
                this.chkSinglelineF.Enabled = true;
                this.chkExplicitCaptureR.Enabled = true;
                this.chkIgnorePatternWhitespaceR.Enabled = true;
                this.chkRightToLeftR.Enabled = true;
                this.chkSinglelineR.Enabled = true;
            }
        }

        private void cmdRecentFindF_Click(object sender, EventArgs e)
        {
            this.mnuRecentFindF.Items.Clear();
            foreach (var item in this.MruFind)
            {
                ToolStripItem newItem = this.mnuRecentFindF.Items.Add(item);
                newItem.Tag = item;
            }

            this.mnuRecentFindF.Items.Add("-");
            this.mnuRecentFindF.Items.Add("Clear History");
            this.mnuRecentFindF.Show(this.cmdRecentFindF.PointToScreen(this.cmdRecentFindF.ClientRectangle.Location));
        }

        private void cmdRecentFindR_Click(object sender, EventArgs e)
        {
            this.mnuRecentFindR.Items.Clear();
            foreach (var item in this.MruFind)
            {
                ToolStripItem newItem = this.mnuRecentFindR.Items.Add(item);
                newItem.Tag = item;
            }

            this.mnuRecentFindR.Items.Add("-");
            this.mnuRecentFindR.Items.Add("Clear History");
            this.mnuRecentFindR.Show(this.cmdRecentFindR.PointToScreen(this.cmdRecentFindR.ClientRectangle.Location));
        }

        private void cmdRecentReplace_Click(object sender, EventArgs e)
        {
            this.mnuRecentReplace.Items.Clear();
            foreach (var item in this.MruReplace)
            {
                ToolStripItem newItem = this.mnuRecentReplace.Items.Add(item);
                newItem.Tag = item;
            }

            this.mnuRecentReplace.Items.Add("-");
            this.mnuRecentReplace.Items.Add("Clear History");
            this.mnuRecentReplace.Show(this.cmdRecentReplace.PointToScreen(this.cmdRecentReplace.ClientRectangle.Location));
        }

        private void cmdExtendedCharFindF_Click(object sender, EventArgs e)
        {
            if (this.rdoExtendedF.Checked)
            {
                this.mnuExtendedCharFindF.Show(this.cmdExtCharAndRegExFindF.PointToScreen(this.cmdExtCharAndRegExFindF.ClientRectangle.Location));
            }
            else if (this.rdoRegexF.Checked)
            {
                this.mnuRegExCharFindF.Show(this.cmdExtCharAndRegExFindF.PointToScreen(this.cmdExtCharAndRegExFindF.ClientRectangle.Location));
            }
        }

        private void cmdExtendedCharFindR_Click(object sender, EventArgs e)
        {
            if (this.rdoExtendedR.Checked)
            {
                this.mnuExtendedCharFindR.Show(this.cmdExtCharAndRegExFindR.PointToScreen(this.cmdExtCharAndRegExFindR.ClientRectangle.Location));
            }
            else if (this.rdoRegexR.Checked)
            {
                this.mnuRegExCharFindR.Show(this.cmdExtCharAndRegExFindR.PointToScreen(this.cmdExtCharAndRegExFindR.ClientRectangle.Location));
            }
        }

        private void cmdExtendedCharReplace_Click(object sender, EventArgs e)
        {
            if (this.rdoExtendedR.Checked)
            {
                this.mnuExtendedCharReplace.Show(this.cmdExtCharAndRegExReplace.PointToScreen(this.cmdExtCharAndRegExReplace.ClientRectangle.Location));
            }
            else if (this.rdoRegexR.Checked)
            {
                this.mnuRegExCharReplace.Show(this.cmdExtCharAndRegExReplace.PointToScreen(this.cmdExtCharAndRegExReplace.ClientRectangle.Location));
            }
        }

        private void rdoStandardF_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdoStandardF.Checked)
                this.pnlStandardOptionsF.BringToFront();
            else
                this.pnlRegexpOptionsF.BringToFront();

            this.cmdExtCharAndRegExFindF.Enabled = !this.rdoStandardF.Checked;
        }

        private void rdoStandardR_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdoStandardR.Checked)
                this.pnlStandardOptionsR.BringToFront();
            else
                this.pnlRegexpOptionsR.BringToFront();

            this.cmdExtCharAndRegExFindR.Enabled = !this.rdoStandardR.Checked;
            this.cmdExtCharAndRegExReplace.Enabled = !this.rdoStandardR.Checked;
        }

        private void tabAll_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabAll.SelectedTab == this.tpgFind)
            {
                this.txtFindF.Text = this.txtFindR.Text;

                this.rdoStandardF.Checked = this.rdoStandardR.Checked;
                this.rdoExtendedF.Checked = this.rdoExtendedR.Checked;
                this.rdoRegexF.Checked = this.rdoRegexR.Checked;

                this.chkWrapF.Checked = this.chkWrapR.Checked;
                this.chkSearchSelectionF.Checked = this.chkSearchSelectionR.Checked;

                this.chkMatchCaseF.Checked = this.chkMatchCaseR.Checked;
                this.chkWholeWordF.Checked = this.chkWholeWordR.Checked;
                this.chkWordStartF.Checked = this.chkWordStartR.Checked;

                this.chkCompiledF.Checked = this.chkCompiledR.Checked;
                this.chkCultureInvariantF.Checked = this.chkCultureInvariantR.Checked;
                this.chkEcmaScriptF.Checked = this.chkEcmaScriptR.Checked;
                this.chkExplicitCaptureF.Checked = this.chkExplicitCaptureR.Checked;
                this.chkIgnoreCaseF.Checked = this.chkIgnoreCaseR.Checked;
                this.chkIgnorePatternWhitespaceF.Checked = this.chkIgnorePatternWhitespaceR.Checked;
                this.chkMultilineF.Checked = this.chkMultilineR.Checked;
                this.chkRightToLeftF.Checked = this.chkRightToLeftR.Checked;
                this.chkSinglelineF.Checked = this.chkSinglelineR.Checked;

                this.AcceptButton = this.btnFindNextF;
            }
            else
            {
                this.txtFindR.Text = this.txtFindF.Text;

                this.rdoStandardR.Checked = this.rdoStandardF.Checked;
                this.rdoExtendedR.Checked = this.rdoExtendedF.Checked;
                this.rdoRegexR.Checked = this.rdoRegexF.Checked;

                this.chkWrapR.Checked = this.chkWrapF.Checked;
                this.chkSearchSelectionR.Checked = this.chkSearchSelectionF.Checked;

                this.chkMatchCaseR.Checked = this.chkMatchCaseF.Checked;
                this.chkWholeWordR.Checked = this.chkWholeWordF.Checked;
                this.chkWordStartR.Checked = this.chkWordStartF.Checked;

                this.chkCompiledR.Checked = this.chkCompiledF.Checked;
                this.chkCultureInvariantR.Checked = this.chkCultureInvariantF.Checked;
                this.chkEcmaScriptR.Checked = this.chkEcmaScriptF.Checked;
                this.chkExplicitCaptureR.Checked = this.chkExplicitCaptureF.Checked;
                this.chkIgnoreCaseR.Checked = this.chkIgnoreCaseF.Checked;
                this.chkIgnorePatternWhitespaceR.Checked = this.chkIgnorePatternWhitespaceF.Checked;
                this.chkMultilineR.Checked = this.chkMultilineF.Checked;
                this.chkRightToLeftR.Checked = this.chkRightToLeftF.Checked;
                this.chkSinglelineR.Checked = this.chkSinglelineF.Checked;

                this.AcceptButton = this.btnReplaceNext;
            }
        }

        #endregion Event Handlers

        #region Methods

        public void FindNext()
        {
            this.SyncSearchText();

            if (this.txtFindF.Text == string.Empty)
                return;

            this.AddFindMru();
            this.lblStatus.Text = string.Empty;

            CharacterRange foundRange;

            try
            {
                foundRange = this.FindNextF(false);
            }
            catch (ArgumentException ex)
            {
                this.lblStatus.Text = "Error in Regular Expression: " + ex.Message;
                return;
            }

            if (foundRange.cpMin == foundRange.cpMax)
            {
                this.lblStatus.Text = "Match could not be found";
            }
            else
            {
                if (foundRange.cpMin < this.Scintilla.AnchorPosition)
                {
                    if (this.chkSearchSelectionF.Checked)
                        this.lblStatus.Text = "Search match wrapped to the beginning of the selection";
                    else
                        this.lblStatus.Text = "Search match wrapped to the beginning of the document";
                }

                this.Scintilla.SetSel(foundRange.cpMin, foundRange.cpMax);
                this.MoveFormAwayFromSelection();
            }
        }

        public void FindPrevious()
        {
            this.SyncSearchText();

            if (this.txtFindF.Text == string.Empty)
                return;

            this.AddFindMru();
            this.lblStatus.Text = string.Empty;
            CharacterRange foundRange;
            try
            {
                foundRange = this.FindNextF(true);
            }
            catch (ArgumentException ex)
            {
                this.lblStatus.Text = "Error in Regular Expression: " + ex.Message;
                return;
            }

            if (foundRange.cpMin == foundRange.cpMax)
            {
                this.lblStatus.Text = "Match could not be found";
            }
            else
            {
                if (foundRange.cpMin > this.Scintilla.CurrentPosition)
                {
                    if (this.chkSearchSelectionF.Checked)
                        this.lblStatus.Text = "Search match wrapped to the _end of the selection";
                    else
                        this.lblStatus.Text = "Search match wrapped to the _end of the document";
                }

                this.Scintilla.SetSel(foundRange.cpMin, foundRange.cpMax);
                this.MoveFormAwayFromSelection();
            }
        }

        public RegexOptions GetRegexOptions()
        {
            RegexOptions ro = RegexOptions.None;

            if (this.tabAll.SelectedTab == this.tpgFind)
            {
                if (this.chkCompiledF.Checked)
                    ro |= RegexOptions.Compiled;

                if (this.chkCultureInvariantF.Checked)
                    ro |= RegexOptions.Compiled;

                if (this.chkEcmaScriptF.Checked)
                    ro |= RegexOptions.ECMAScript;

                if (this.chkExplicitCaptureF.Checked)
                    ro |= RegexOptions.ExplicitCapture;

                if (this.chkIgnoreCaseF.Checked)
                    ro |= RegexOptions.IgnoreCase;

                if (this.chkIgnorePatternWhitespaceF.Checked)
                    ro |= RegexOptions.IgnorePatternWhitespace;

                if (this.chkMultilineF.Checked)
                    ro |= RegexOptions.Multiline;

                if (this.chkRightToLeftF.Checked)
                    ro |= RegexOptions.RightToLeft;

                if (this.chkSinglelineF.Checked)
                    ro |= RegexOptions.Singleline;
            }
            else
            {
                if (this.chkCompiledR.Checked)
                    ro |= RegexOptions.Compiled;

                if (this.chkCultureInvariantR.Checked)
                    ro |= RegexOptions.Compiled;

                if (this.chkEcmaScriptR.Checked)
                    ro |= RegexOptions.ECMAScript;

                if (this.chkExplicitCaptureR.Checked)
                    ro |= RegexOptions.ExplicitCapture;

                if (this.chkIgnoreCaseR.Checked)
                    ro |= RegexOptions.IgnoreCase;

                if (this.chkIgnorePatternWhitespaceR.Checked)
                    ro |= RegexOptions.IgnorePatternWhitespace;

                if (this.chkMultilineR.Checked)
                    ro |= RegexOptions.Multiline;

                if (this.chkRightToLeftR.Checked)
                    ro |= RegexOptions.RightToLeft;

                if (this.chkSinglelineR.Checked)
                    ro |= RegexOptions.Singleline;
            }

            return ro;
        }

        public SearchFlags GetSearchFlags()
        {
            SearchFlags sf = SearchFlags.None;

            if (this.tabAll.SelectedTab == this.tpgFind)
            {
                if (this.chkMatchCaseF.Checked)
                    sf |= SearchFlags.MatchCase;

                if (this.chkWholeWordF.Checked)
                    sf |= SearchFlags.WholeWord;

                if (this.chkWordStartF.Checked)
                    sf |= SearchFlags.WordStart;
            }
            else
            {
                if (this.chkMatchCaseR.Checked)
                    sf |= SearchFlags.MatchCase;

                if (this.chkWholeWordR.Checked)
                    sf |= SearchFlags.WholeWord;

                if (this.chkWordStartR.Checked)
                    sf |= SearchFlags.WordStart;
            }

            return sf;
        }

        public virtual void MoveFormAwayFromSelection()
        {
            if (!this.Visible)
                return;

            if (!this.AutoPosition)
                return;

            int pos = this.Scintilla.CurrentPosition;
            int x = this.Scintilla.PointXFromPosition(pos);
            int y = this.Scintilla.PointYFromPosition(pos);

            Point cursorPoint = this.Scintilla.PointToScreen(new Point(x, y));

            Rectangle r = new Rectangle(this.Location, this.Size);
            if (r.Contains(cursorPoint))
            {
                Point newLocation;
                if (cursorPoint.Y < (Screen.PrimaryScreen.Bounds.Height / 2))
                {
                    //TODO - replace lineheight with ScintillaNET command, when added
                    int sciTextheight = 2279;
                    int lineHeight = this.Scintilla.DirectMessage(sciTextheight, IntPtr.Zero, IntPtr.Zero).ToInt32();
                    // int lineHeight = Scintilla.Lines[Scintilla.LineFromPosition(pos)].Height;
                    
                    // Top half of the screen
                    newLocation = this.Scintilla.PointToClient(
                        new Point(this.Location.X, cursorPoint.Y + lineHeight * 2));
                }
                else
                {
                    //TODO - replace lineheight with ScintillaNET command, when added
                    int sciTextheight = 2279;
                    int lineHeight = this.Scintilla.DirectMessage(sciTextheight, IntPtr.Zero, IntPtr.Zero).ToInt32();
                    // int lineHeight = Scintilla.Lines[Scintilla.LineFromPosition(pos)].Height;
                    
                    // Bottom half of the screen
                    newLocation = this.Scintilla.PointToClient(
                        new Point(this.Location.X, cursorPoint.Y - this.Height - (lineHeight * 2)));
                }
                newLocation = this.Scintilla.PointToScreen(newLocation);
                this.Location = newLocation;
            }
        }

        public void ReplaceNext()
        {
            if (this.txtFindR.Text == string.Empty)
                return;

            this.AddReplaceMru();
            this.lblStatus.Text = string.Empty;

            CharacterRange nextRange;
            try
            {
                nextRange = this.ReplaceNext(false);
            }
            catch (ArgumentException ex)
            {
                this.lblStatus.Text = "Error in Regular Expression: " + ex.Message;
                return;
            }

            if (nextRange.cpMin == nextRange.cpMax)
            {
                this.lblStatus.Text = "Match could not be found";
            }
            else
            {
                if (nextRange.cpMin < this.Scintilla.AnchorPosition)
                {
                    if (this.chkSearchSelectionR.Checked)
                        this.lblStatus.Text = "Search match wrapped to the beginning of the selection";
                    else
                        this.lblStatus.Text = "Search match wrapped to the beginning of the document";
                }

                this.Scintilla.SetSel(nextRange.cpMin, nextRange.cpMax);
                this.MoveFormAwayFromSelection();
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            if (this.Scintilla.Selections.Count > 0)
            {
                this.chkSearchSelectionF.Enabled = true;
                this.chkSearchSelectionR.Enabled = true;
            }
            else
            {
                this.chkSearchSelectionF.Enabled = false;
                this.chkSearchSelectionR.Enabled = false;
                this.chkSearchSelectionF.Checked = false;
                this.chkSearchSelectionR.Checked = false;
            }

            //	if they leave the dialog and come back any "Search Selection"
            //	range they might have had is invalidated
            this.searchRange = new CharacterRange();

            this.lblStatus.Text = string.Empty;

            this.MoveFormAwayFromSelection();

            base.OnActivated(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            //	So what we're doing here is testing for any of the find/replace
            //	command shortcut bindings. If the key combination matches we send
            //	the KeyEventArgs back to Scintilla so it can be processed. That
            //	way things like Find Next, Show Replace are all available from
            //	the dialog using Scintilla's configured Shortcuts

            //List<KeyBinding> findNextBinding = Scintilla.Commands.GetKeyBindings(BindableCommand.FindNext);
            //List<KeyBinding> findPrevBinding = Scintilla.Commands.GetKeyBindings(BindableCommand.FindPrevious);
            //List<KeyBinding> showFindBinding = Scintilla.Commands.GetKeyBindings(BindableCommand.ShowFind);
            //List<KeyBinding> showReplaceBinding = Scintilla.Commands.GetKeyBindings(BindableCommand.ShowReplace);

            //KeyBinding kb = new KeyBinding(e.KeyCode, e.Modifiers);

            //if (findNextBinding.Contains(kb) || findPrevBinding.Contains(kb) || showFindBinding.Contains(kb) || showReplaceBinding.Contains(kb))
            //{
            //Scintilla. FireKeyDown(e);
            //}

            if (this.KeyPressed != null) this.KeyPressed(this, e);

            if (e.KeyCode == Keys.Escape) this.Hide();

            base.OnKeyDown(e);
        }

        private void SyncSearchText()
        {
            if (this.tabAll.SelectedTab == this.tpgFind)
                this.txtFindR.Text = this.txtFindF.Text;
            else
                this.txtFindF.Text = this.txtFindR.Text;
        }

        private void AddFindMru()
        {
            string find = this.txtFindF.Text;
            this.mruFind.Remove(find);

            this.mruFind.Insert(0, find);

            if (this.mruFind.Count > this.mruMaxCount) this.mruFind.RemoveAt(this.mruFind.Count - 1);

            this.bindingSourceFind.ResetBindings(false);
        }

        private void AddReplaceMru()
        {
            string find = this.txtFindR.Text;
            this.mruFind.Remove(find);

            this.mruFind.Insert(0, find);

            if (this.mruFind.Count > this.mruMaxCount) this.mruFind.RemoveAt(this.mruFind.Count - 1);

            string replace = this.txtReplace.Text;
            if (replace != string.Empty)
            {
                this.mruReplace.Remove(replace);

                this.mruReplace.Insert(0, replace);

                if (this.mruReplace.Count > this.mruMaxCount) this.mruReplace.RemoveAt(this.mruReplace.Count - 1);
            }

            this.bindingSourceFind.ResetBindings(false);
            this.bindingSourceReplace.ResetBindings(false);
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //Insert the string value held in the menu items Tag field (\t, \n, etc.)
            this.txtFindF.SelectedText = e.ClickedItem.Tag.ToString();
        }

        private CharacterRange FindNextF(bool searchUp)
        {
            CharacterRange foundRange;

            if (this.rdoRegexF.Checked)
            {
                Regex rr = new Regex(this.txtFindF.Text, this.GetRegexOptions());

                if (this.chkSearchSelectionF.Checked)
                {
                    if (this.searchRange.cpMin == this.searchRange.cpMax) this.searchRange = new CharacterRange(this.scintilla.Selections[0].Start, this.scintilla.Selections[0].End);

                    if (searchUp)
                        foundRange = this.FindReplace.FindPrevious(rr, this.chkWrapF.Checked, this.searchRange);
                    else
                        foundRange = this.FindReplace.FindNext(rr, this.chkWrapF.Checked, this.searchRange);
                }
                else
                {
                    this.searchRange = new CharacterRange();
                    if (searchUp)
                        foundRange = this.FindReplace.FindPrevious(rr, this.chkWrapF.Checked);
                    else
                        foundRange = this.FindReplace.FindNext(rr, this.chkWrapF.Checked);
                }
            }
            else
            {
                if (this.chkSearchSelectionF.Checked)
                {
                    if (this.searchRange.cpMin == this.searchRange.cpMax) this.searchRange = new CharacterRange(this.scintilla.Selections[0].Start, this.scintilla.Selections[0].End);

                    if (searchUp)
                    {
                        string textToFind = this.rdoExtendedF.Checked ? this.FindReplace.Transform(this.txtFindF.Text) : this.txtFindF.Text;
                        foundRange = this.FindReplace.FindPrevious(textToFind, this.chkWrapF.Checked, this.GetSearchFlags(), this.searchRange);
                    }
                    else
                    {
                        string textToFind = this.rdoExtendedF.Checked ? this.FindReplace.Transform(this.txtFindF.Text) : this.txtFindF.Text;
                        foundRange = this.FindReplace.FindNext(textToFind, this.chkWrapF.Checked, this.GetSearchFlags(), this.searchRange);
                    }
                }
                else
                {
                    this.searchRange = new CharacterRange();
                    if (searchUp)
                    {
                        string textToFind = this.rdoExtendedF.Checked ? this.FindReplace.Transform(this.txtFindF.Text) : this.txtFindF.Text;
                        foundRange = this.FindReplace.FindPrevious(textToFind, this.chkWrapF.Checked, this.GetSearchFlags());
                    }
                    else
                    {
                        string textToFind = this.rdoExtendedF.Checked ? this.FindReplace.Transform(this.txtFindF.Text) : this.txtFindF.Text;
                        foundRange = this.FindReplace.FindNext(textToFind, this.chkWrapF.Checked, this.GetSearchFlags());
                    }
                }
            }
            return foundRange;
        }

        private CharacterRange FindNextR(bool searchUp, ref Regex rr)
        {
            CharacterRange foundRange;

            if (this.rdoRegexR.Checked)
            {
                if (rr == null)
                    rr = new Regex(this.txtFindR.Text, this.GetRegexOptions());

                if (this.chkSearchSelectionR.Checked)
                {
                    if (this.searchRange.cpMin == this.searchRange.cpMax) this.searchRange = new CharacterRange(this.scintilla.Selections[0].Start, this.scintilla.Selections[0].End);

                    if (searchUp)
                        foundRange = this.FindReplace.FindPrevious(rr, this.chkWrapR.Checked, this.searchRange);
                    else
                        foundRange = this.FindReplace.FindNext(rr, this.chkWrapR.Checked, this.searchRange);
                }
                else
                {
                    this.searchRange = new CharacterRange();
                    if (searchUp)
                        foundRange = this.FindReplace.FindPrevious(rr, this.chkWrapR.Checked);
                    else
                        foundRange = this.FindReplace.FindNext(rr, this.chkWrapR.Checked);
                }
            }
            else
            {
                if (this.chkSearchSelectionF.Checked)
                {
                    if (this.searchRange.cpMin == this.searchRange.cpMax) this.searchRange = new CharacterRange(this.scintilla.Selections[0].Start, this.scintilla.Selections[0].End);

                    if (searchUp)
                    {
                        string textToFind = this.rdoExtendedR.Checked ? this.FindReplace.Transform(this.txtFindR.Text) : this.txtFindR.Text;
                        foundRange = this.FindReplace.FindPrevious(textToFind, this.chkWrapR.Checked, this.GetSearchFlags(), this.searchRange);
                    }
                    else
                    {
                        string textToFind = this.rdoExtendedR.Checked ? this.FindReplace.Transform(this.txtFindR.Text) : this.txtFindR.Text;
                        foundRange = this.FindReplace.FindNext(textToFind, this.chkWrapR.Checked, this.GetSearchFlags(), this.searchRange);
                    }
                }
                else
                {
                    this.searchRange = new CharacterRange();
                    if (searchUp)
                    {
                        string textToFind = this.rdoExtendedR.Checked ? this.FindReplace.Transform(this.txtFindR.Text) : this.txtFindR.Text;
                        foundRange = this.FindReplace.FindPrevious(textToFind, this.chkWrapF.Checked, this.GetSearchFlags());
                    }
                    else
                    {
                        string textToFind = this.rdoExtendedR.Checked ? this.FindReplace.Transform(this.txtFindR.Text) : this.txtFindR.Text;
                        foundRange = this.FindReplace.FindNext(textToFind, this.chkWrapF.Checked, this.GetSearchFlags());
                    }
                }
            }
            return foundRange;
        }

        private void FindReplaceDialog_Activated(object sender, EventArgs e)
        {
            this.Opacity = 1.0;
        }

        private void FindReplaceDialog_Deactivate(object sender, EventArgs e)
        {
            this.Opacity = 0.6;
        }

        private void mnuRecentFindF_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //Insert the string value held in the menu items Tag field (\t, \n, etc.)
            if (e.ClickedItem.Text == "Clear History")
            {
                this.MruFind.Clear();
            }
            else
            {
                this.txtFindF.Text = e.ClickedItem.Tag.ToString();
            }
        }

        private void mnuRecentFindR_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //Insert the string value held in the menu items Tag field (\t, \n, etc.)
            if (e.ClickedItem.Text == "Clear History")
            {
                this.MruFind.Clear();
            }
            else
            {
                this.txtFindR.Text = e.ClickedItem.Tag.ToString();
            }
        }

        private void mnuRecentReplace_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //Insert the string value held in the menu items Tag field (\t, \n, etc.)
            if (e.ClickedItem.Text == "Clear History")
            {
                this.MruReplace.Clear();
            }
            else
            {
                this.txtReplace.Text = e.ClickedItem.Tag.ToString();
            }
        }

        private void mnuExtendedCharFindR_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //Insert the string value held in the menu items Tag field (\t, \n, etc.)
            this.txtFindR.SelectedText = e.ClickedItem.Tag.ToString();
        }

        private void mnuExtendedCharReplace_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //Insert the string value held in the menu items Tag field (\t, \n, etc.)
            this.txtReplace.SelectedText = e.ClickedItem.Tag.ToString();
        }

        private CharacterRange ReplaceNext(bool searchUp)
        {
            Regex rr = null;
            CharacterRange selRange = new CharacterRange(this.scintilla.Selections[0].Start, this.scintilla.Selections[0].End);

            //	We only do the actual replacement if the current selection exactly
            //	matches the find.
            if (selRange.cpMax - selRange.cpMin > 0)
            {
                if (this.rdoRegexR.Checked)
                {
                    rr = new Regex(this.txtFindR.Text, this.GetRegexOptions());
                    string selRangeText = this.Scintilla.GetTextRange(selRange.cpMin, selRange.cpMax - selRange.cpMin + 1);

                    if (selRange.Equals(this.FindReplace.Find(selRange, rr, false)))
                    {
                        //	If searching up we do the replacement using the range object.
                        //	Otherwise we use the selection object. The reason being if
                        //	we use the range the caret is positioned before the replaced
                        //	text. Conversely if we use the selection object the caret will
                        //	be positioned after the replaced text. This is very important
                        //	becuase we don't want the new text to be potentially matched
                        //	in the next search.
                        if (searchUp)
                        {
                            this.scintilla.SelectionStart = selRange.cpMin;
                            this.scintilla.SelectionEnd = selRange.cpMax;
                            this.scintilla.ReplaceSelection(rr.Replace(selRangeText, this.txtReplace.Text));
                            this.scintilla.GotoPosition(selRange.cpMin);
                        }
                        else
                            this.Scintilla.ReplaceSelection(rr.Replace(selRangeText, this.txtReplace.Text));
                    }
                }
                else
                {
                    string textToFind = this.rdoExtendedR.Checked ? this.FindReplace.Transform(this.txtFindR.Text) : this.txtFindR.Text;
                    if (selRange.Equals(this.FindReplace.Find(selRange, textToFind, false)))
                    {
                        //	If searching up we do the replacement using the range object.
                        //	Otherwise we use the selection object. The reason being if
                        //	we use the range the caret is positioned before the replaced
                        //	text. Conversely if we use the selection object the caret will
                        //	be positioned after the replaced text. This is very important
                        //	becuase we don't want the new text to be potentially matched
                        //	in the next search.
                        if (searchUp)
                        {
                            string textToReplace = this.rdoExtendedR.Checked ? this.FindReplace.Transform(this.txtReplace.Text) : this.txtReplace.Text;
                            this.scintilla.SelectionStart = selRange.cpMin;
                            this.scintilla.SelectionEnd = selRange.cpMax;
                            this.scintilla.ReplaceSelection(textToReplace);

                            this.scintilla.GotoPosition(selRange.cpMin);
                        }
                        else
                        {
                            string textToReplace = this.rdoExtendedR.Checked ? this.FindReplace.Transform(this.txtReplace.Text) : this.txtReplace.Text;
                            this.Scintilla.ReplaceSelection(textToReplace);
                        }
                    }
                }
            }
            return this.FindNextR(searchUp, ref rr);
        }

        #endregion Methods
    }
}