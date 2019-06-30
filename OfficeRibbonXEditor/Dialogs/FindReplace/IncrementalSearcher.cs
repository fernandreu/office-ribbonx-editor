using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ScintillaNET;
using CharacterRange = OfficeRibbonXEditor.Models.CharacterRange;

namespace OfficeRibbonXEditor.Dialogs.FindReplace
{
    public partial class IncrementalSearcher : UserControl
    {
        #region Fields

        private bool _autoPosition = true;
        private Scintilla _scintilla;
        private bool _toolItem = false;
        private FindReplace.FindReplace _findReplace;

        #endregion Fields

        #region Constructors

        public IncrementalSearcher()
        {
            this.InitializeComponent();
        }

        public IncrementalSearcher(bool toolItem)
        {
            this.InitializeComponent();
            this._toolItem = toolItem;
            if (toolItem)
                this.BackColor = Color.Transparent;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets whether the control should automatically move away from the current
        /// selection to prevent obscuring it.
        /// </summary>
        /// <returns>true to automatically move away from the current selection; otherwise, false.
        /// If ToolItem is enabled, this defaults to false.</returns>
        public bool AutoPosition
        {
            get
            {
                return this._autoPosition;
            }
            set
            {
                if (!this.ToolItem)
                    this._autoPosition = value;
                else
                    this._autoPosition = false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public FindReplace.FindReplace FindReplace
        {
            get
            {
                return this._findReplace;
            }
            set
            {
                this._findReplace = value;
                if (value!=null)
                {
                this._scintilla = this._findReplace.Scintilla;
                }
                else
                {
                    this._scintilla = null;
                }
            }
        }

        [Browsable(false)]
        public Scintilla Scintilla
        {
            get
            {
                return this._scintilla;
            }
            set
            {
                this._scintilla = value;
            }
        }

        public bool ToolItem
        {
            get { return this._toolItem; }
            set
            {
                this._toolItem = value;
                if (this._toolItem)
                    this.BackColor = Color.Transparent;
                else
                    this.BackColor = Color.LightSteelBlue;
            }
        }

        #endregion Properties

        #region Event Handlers

        private void brnPrevious_Click(object sender, EventArgs e)
        {
            this.findPrevious();
        }

        private void btnClearHighlights_Click(object sender, EventArgs e)
        {
            if (this._scintilla == null)
                return;
            this._findReplace.ClearAllHighlights();
        }

        private void btnHighlightAll_Click(object sender, EventArgs e)
        {
            if (this.txtFind.Text == string.Empty)
                return;
            if (this._scintilla == null)
                return;

            int foundCount = this._findReplace.FindAll(this.txtFind.Text, false, true).Count;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            this.findNext();
        }

        #endregion Event Handlers

        #region Methods

        public virtual void MoveFormAwayFromSelection()
        {
            if (!this.Visible || this._scintilla == null)
                return;

            if (!this.AutoPosition)
                return;

            int pos = this._scintilla.CurrentPosition;
            int x = this._scintilla.PointXFromPosition(pos);
            int y = this._scintilla.PointYFromPosition(pos);

            Point cursorPoint = new Point(x, y);

            Rectangle r = new Rectangle(this.Location, this.Size);

            if (this._scintilla != null)
            {
                r.Location = new Point(this._scintilla.ClientRectangle.Right - this.Size.Width, 0);
            }

            if (r.Contains(cursorPoint))
            {
                Point newLocation;
                if (cursorPoint.Y < (Screen.PrimaryScreen.Bounds.Height / 2))
                {
                    //TODO - replace lineheight with ScintillaNET command, when added
                    int SCI_TEXTHEIGHT = 2279;
                    int lineHeight = this._scintilla.DirectMessage(SCI_TEXTHEIGHT, IntPtr.Zero, IntPtr.Zero).ToInt32();
                    // Top half of the screen
                    newLocation = new Point(r.X, cursorPoint.Y + lineHeight * 2);
                }
                else
                {
                    //TODO - replace lineheight with ScintillaNET command, when added
                    int SCI_TEXTHEIGHT = 2279;
                    int lineHeight = this._scintilla.DirectMessage(SCI_TEXTHEIGHT, IntPtr.Zero, IntPtr.Zero).ToInt32();
                    // Bottom half of the screen
                    newLocation = new Point(r.X, cursorPoint.Y - this.Height - (lineHeight * 2));
                }

                this.Location = newLocation;
            }
            else
            {
                this.Location = r.Location;
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            this.MoveFormAwayFromSelection();
            this.txtFind.Focus();
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLostFocus(e);
            if (!this._toolItem)
                this.Hide();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            this.txtFind.Text = string.Empty;
            this.txtFind.BackColor = SystemColors.Window;

            this.MoveFormAwayFromSelection();

            if (this.Visible)
                this.txtFind.Focus();
            else if (this._scintilla != null)
                this._scintilla.Focus();
        }

        private void findNext()
        {
            if (this.txtFind.Text == string.Empty)
                return;
            if (this._scintilla == null)
                return;

            CharacterRange r = this._findReplace.FindNext(this.txtFind.Text, true, this._findReplace.Window.GetSearchFlags());
            if (r.cpMin != r.cpMax)
                this._scintilla.SetSel(r.cpMin, r.cpMax);

            this.MoveFormAwayFromSelection();
        }

        private void findPrevious()
        {
            if (this.txtFind.Text == string.Empty)
                return;
            if (this._scintilla == null)
                return;

            CharacterRange r = this._findReplace.FindPrevious(this.txtFind.Text, true, this._findReplace.Window.GetSearchFlags());
            if (r.cpMin != r.cpMax)
                this._scintilla.SetSel(r.cpMin, r.cpMax);

            this.MoveFormAwayFromSelection();
        }

        private void txtFind_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                case Keys.Down:
                    this.findNext();
                    e.Handled = true;
                    break;

                case Keys.Up:
                    this.findPrevious();
                    e.Handled = true;
                    break;

                case Keys.Escape:
                    if (!this._toolItem)
                        this.Hide();
                    break;
            }
        }

        private void txtFind_TextChanged(object sender, EventArgs e)
        {
            this.txtFind.BackColor = SystemColors.Window;
            if (this.txtFind.Text == string.Empty)
                return;
            if (this._scintilla == null)
                return;

            int pos = Math.Min(this._scintilla.CurrentPosition, this._scintilla.AnchorPosition);
            CharacterRange r = this._findReplace.Find(pos, this._scintilla.TextLength, this.txtFind.Text, this._findReplace.Window.GetSearchFlags());
            if (r.cpMin == r.cpMax)
                r = this._findReplace.Find(0, pos, this.txtFind.Text, this._findReplace.Window.GetSearchFlags());

            if (r.cpMin != r.cpMax)
                this._scintilla.SetSel(r.cpMin, r.cpMax);
            else
                this.txtFind.BackColor = Color.Tomato;

            this.MoveFormAwayFromSelection();
        }

        #endregion Methods
    }
}