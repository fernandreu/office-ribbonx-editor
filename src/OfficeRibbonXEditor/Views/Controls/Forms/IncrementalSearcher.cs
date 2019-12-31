using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using ScintillaNET;

namespace OfficeRibbonXEditor.Views.Controls.Forms
{
    public partial class IncrementalSearcher : UserControl
    {
        #region Fields

        private bool autoPosition = true;
        private Scintilla? scintilla;
        private bool toolItem = false;
        private FindReplaceDialogViewModel? findReplace;

        #endregion Fields

        #region Constructors

        public IncrementalSearcher()
        {
            this.InitializeComponent();
        }

        public IncrementalSearcher(bool toolItem)
        {
            this.InitializeComponent();
            this.toolItem = toolItem;
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
                return this.autoPosition;
            }
            set
            {
                if (!this.ToolItem)
                    this.autoPosition = value;
                else
                    this.autoPosition = false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public FindReplaceDialogViewModel? FindReplace
        {
            get => this.findReplace;
            set
            {
                this.findReplace = value;
                this.scintilla = value?.Scintilla;
            }
        }

        [Browsable(false)]
        public Scintilla? Scintilla
        {
            get => this.scintilla;
            set => this.scintilla = value;
        }

        public bool ToolItem
        {
            get { return this.toolItem; }
            set
            {
                this.toolItem = value;
                if (this.toolItem)
                    this.BackColor = Color.Transparent;
                else
                    this.BackColor = Color.LightSteelBlue;
            }
        }

        #endregion Properties

        #region Event Handlers

        private void brnPrevious_Click(object sender, EventArgs e)
        {
            this.FindPrevious();
        }

        private void btnClearHighlights_Click(object sender, EventArgs e)
        {
            if (this.scintilla == null)
                return;
            this.findReplace?.FindReplace.ClearAllHighlights();
        }

        private void btnHighlightAll_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtFind.Text))
                return;
            if (this.scintilla == null)
                return;

            this.findReplace?.FindReplace.FindAll((string) this.txtFind.Text, false, true);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            this.FindNext();
        }

        #endregion Event Handlers

        #region Methods

        public virtual void MoveFormAwayFromSelection()
        {
            if (!this.Visible || this.scintilla == null)
                return;

            if (!this.AutoPosition)
                return;

            int pos = this.scintilla.CurrentPosition;
            int x = this.scintilla.PointXFromPosition(pos);
            int y = this.scintilla.PointYFromPosition(pos);

            var cursorPoint = new Point(x, y);

            var r = new Rectangle(
                new Point(this.scintilla.ClientRectangle.Right - this.Size.Width, 0), 
                this.Size);

            const int sciTextHeight = 2279;
            if (r.Contains(cursorPoint))
            {
                Point newLocation;
                if (cursorPoint.Y < (Screen.PrimaryScreen.Bounds.Height / 2))
                {
                    //TODO - replace line height with ScintillaNET command, when added
                    var lineHeight = this.scintilla.DirectMessage(sciTextHeight, IntPtr.Zero, IntPtr.Zero).ToInt32();
                    // Top half of the screen
                    newLocation = new Point(r.X, cursorPoint.Y + lineHeight * 2);
                }
                else
                {
                    //TODO - replace line height with ScintillaNET command, when added
                    int lineHeight = this.scintilla.DirectMessage(sciTextHeight, IntPtr.Zero, IntPtr.Zero).ToInt32();
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
            if (!this.toolItem)
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
            else if (this.scintilla != null)
                this.scintilla.Focus();
        }

        private void FindNext()
        {
            if (string.IsNullOrEmpty(this.txtFind.Text))
                return;
            if (this.scintilla == null)
                return;

            var r = this.findReplace?.FindReplace.FindNext(this.txtFind.Text, true, this.findReplace.GetSearchFlags());
            if (r != null && r.Value.MinPosition !=  r.Value.MaxPosition)
                this.scintilla.SetSel(r.Value.MinPosition, r.Value.MaxPosition);

            this.MoveFormAwayFromSelection();
        }

        private void FindPrevious()
        {
            if (string.IsNullOrEmpty(this.txtFind.Text))
                return;
            if (this.scintilla == null)
                return;

            var r = this.findReplace?.FindReplace.FindPrevious(this.txtFind.Text, true, this.findReplace.GetSearchFlags());
            if (r != null && r.Value.MinPosition != r.Value.MaxPosition)
            {
                this.scintilla.SetSel(r.Value.MinPosition, r.Value.MaxPosition);
            }

            this.MoveFormAwayFromSelection();
        }

        private void FindKeyDownEventHandler(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                case Keys.Down:
                    this.FindNext();
                    e.Handled = true;
                    break;

                case Keys.Up:
                    this.FindPrevious();
                    e.Handled = true;
                    break;

                case Keys.Escape:
                    if (!this.toolItem)
                        this.Hide();
                    break;
            }
        }

        private void FindTextChangedEventHandler(object sender, EventArgs e)
        {
            this.txtFind.BackColor = SystemColors.Window;
            if (string.IsNullOrEmpty(this.txtFind.Text))
                return;
            if (this.scintilla == null)
                return;

            int pos = Math.Min(this.scintilla.CurrentPosition, this.scintilla.AnchorPosition);
            var r = this.findReplace?.FindReplace.Find(pos, this.scintilla.TextLength, this.txtFind.Text, this.findReplace.GetSearchFlags());
            if (r == null || r.Value.MinPosition == r.Value.MaxPosition)
                r = this.findReplace?.FindReplace.Find(0, pos, this.txtFind.Text, this.findReplace.GetSearchFlags());

            if (r != null && r.Value.MinPosition != r.Value.MaxPosition)
                this.scintilla.SetSel(r.Value.MinPosition, r.Value.MaxPosition);
            else
                this.txtFind.BackColor = Color.Tomato;

            this.MoveFormAwayFromSelection();
        }

        #endregion Methods
    }
}