#region Using Directives

#endregion Using Directives

namespace OfficeRibbonXEditor.Dialogs.GoTo
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using ScintillaNET;

    public partial class GoToDialog : Form
    {
        #region Fields

        private Scintilla _scintilla;
        private int _currentLineNumber;
        private int _maximumLineNumber;
        private int _gotoLineNumber;

        #endregion Fields

        #region Methods

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (int.TryParse(this.txtGotoLine.Text, out this._gotoLineNumber))
            {
                //	Line #s are 0 based but the users don't think that way
                this._gotoLineNumber--;
                if (this._gotoLineNumber < 0 || this._gotoLineNumber >= this._maximumLineNumber)
                    this.err.SetError(this.txtGotoLine, "Go to line # must be greater than 0 and less than " + (this._maximumLineNumber + 1).ToString());
                else
                {
                    this._scintilla.Lines[this._gotoLineNumber].Goto();
                    //Line(GotoLineNumber);
                    this.Hide();
                    //DialogResult = DialogResult.OK;
                }
            }
            else
            {
                this.err.SetError(this.txtGotoLine, "Go to line # must be a numeric value");
            }
        }

        // This was taken from FindReplaceDialog. Obviously some refactoring is called for
        // since we have common code. However I'm holding off on this because I'm coming
        // up with some other ideas for the FindReplaceDialog. Right now every scintilla
        // gets its own FindReplaceDialog, but they really need to be sharable across
        // multiple scintillas much like how DropMarkers work.

        private void MoveFormAwayFromSelection()
        {
            if (!this.Visible)
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
                    int SCI_TEXTHEIGHT = 2279;
                    int lineHeight = this.Scintilla.DirectMessage(SCI_TEXTHEIGHT, IntPtr.Zero, IntPtr.Zero).ToInt32();
                    // Top half of the screen
                    newLocation = this.Scintilla.PointToClient(
                        new Point(this.Location.X, cursorPoint.Y + lineHeight * 2)
                        );
                }
                else
                {
                    //TODO - replace lineheight with ScintillaNET command, when added
                    int SCI_TEXTHEIGHT = 2279;
                    int lineHeight = this.Scintilla.DirectMessage(SCI_TEXTHEIGHT, IntPtr.Zero, IntPtr.Zero).ToInt32();
                    // Bottom half of the screen
                    newLocation = this.Scintilla.PointToClient(
                        new Point(this.Location.X, cursorPoint.Y - this.Height - (lineHeight * 2))
                        );
                }
                newLocation = this.Scintilla.PointToScreen(newLocation);
                this.Location = newLocation;
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            this.MoveFormAwayFromSelection();
        }

        #endregion Methods

        #region Properties

        public int CurrentLineNumber
        {
            get { return this._currentLineNumber; }
            set
            {
                this._currentLineNumber = value;
            }
        }

        public int GotoLineNumber
        {
            get { return this._gotoLineNumber; }
            set
            {
                this._gotoLineNumber = value;
            }
        }

        public int MaximumLineNumber
        {
            get { return this._maximumLineNumber; }
            set
            {
                this._maximumLineNumber = value;
            }
        }

        public Scintilla Scintilla
        {
            get { return this._scintilla; }
            set { this._scintilla = value; }
        }

        #endregion Properties

        #region Constructors

        public GoToDialog()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        private void GoToDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void GoToDialog_Activated(object sender, EventArgs e)
        {
            string displayLine = (this._currentLineNumber + 1).ToString();

            this.txtCurrentLine.Text = displayLine;
            this.txtMaxLine.Text = this._maximumLineNumber.ToString();
            this.txtGotoLine.Text = displayLine;

            this.txtGotoLine.Select();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}