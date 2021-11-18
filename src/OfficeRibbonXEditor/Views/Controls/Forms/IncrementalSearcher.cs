using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using ScintillaNET;

namespace OfficeRibbonXEditor.Views.Controls.Forms
{
    public sealed partial class IncrementalSearcher : UserControl
    {
        private FindReplaceDialogViewModel? _findReplace;

        public IncrementalSearcher()
        {
            InitializeComponent();
        }

        public IncrementalSearcher(bool toolItem)
        {
            InitializeComponent();
            _toolItem = toolItem;
            if (toolItem)
                BackColor = Color.Transparent;
        }

        #region Properties

        private bool _autoPosition = true;
        /// <summary>
        /// Gets or sets whether the control should automatically move away from the current
        /// selection to prevent obscuring it.
        /// </summary>
        /// <returns>true to automatically move away from the current selection; otherwise, false.
        /// If ToolItem is enabled, this defaults to false.</returns>
        public bool AutoPosition
        {
            get => _autoPosition;
            set => _autoPosition = !ToolItem && value;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public FindReplaceDialogViewModel? FindReplace
        {
            get => _findReplace;
            set
            {
                _findReplace = value;
                Scintilla = value?.Scintilla;
            }
        }

        [Browsable(false)]
        public Scintilla? Scintilla { get; set; }

        private bool _toolItem;
        public bool ToolItem
        {
            get => _toolItem;
            set
            {
                _toolItem = value;
                BackColor = _toolItem ? Color.Transparent : Color.LightSteelBlue;
            }
        }

        #endregion Properties

        #region Event Handlers

        private void brnPrevious_Click(object? sender, EventArgs e)
        {
            FindPrevious();
        }

        private void btnClearHighlights_Click(object? sender, EventArgs e)
        {
            if (Scintilla == null)
                return;
            _findReplace?.FindReplace.ClearAllHighlights();
        }

        private void btnHighlightAll_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFind.Text))
                return;
            if (Scintilla == null)
                return;

            _findReplace?.FindReplace.FindAll(txtFind.Text, false, true);
        }

        private void btnNext_Click(object? sender, EventArgs e)
        {
            FindNext();
        }

        #endregion Event Handlers

        #region Methods

        public void MoveFormAwayFromSelection()
        {
            if (!Visible || Scintilla == null)
                return;

            if (!AutoPosition)
                return;

            int pos = Scintilla.CurrentPosition;
            int x = Scintilla.PointXFromPosition(pos);
            int y = Scintilla.PointYFromPosition(pos);

            var cursorPoint = new Point(x, y);

            var r = new Rectangle(
                new Point(Scintilla.ClientRectangle.Right - Size.Width, 0), 
                Size);

            const int sciTextHeight = 2279;
            if (r.Contains(cursorPoint))
            {
                Point newLocation;
                if (cursorPoint.Y < (Screen.PrimaryScreen.Bounds.Height / 2))
                {
                    var lineHeight = Scintilla.DirectMessage(sciTextHeight, IntPtr.Zero, IntPtr.Zero).ToInt32();
                    // Top half of the screen
                    newLocation = new Point(r.X, cursorPoint.Y + lineHeight * 2);
                }
                else
                {
                    int lineHeight = Scintilla.DirectMessage(sciTextHeight, IntPtr.Zero, IntPtr.Zero).ToInt32();
                    // Bottom half of the screen
                    newLocation = new Point(r.X, cursorPoint.Y - Height - (lineHeight * 2));
                }

                Location = newLocation;
            }
            else
            {
                Location = r.Location;
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            MoveFormAwayFromSelection();
            txtFind.Focus();
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLostFocus(e);
            if (!_toolItem)
                Hide();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            txtFind.Text = string.Empty;
            txtFind.BackColor = SystemColors.Window;

            MoveFormAwayFromSelection();

            if (Visible)
            {
                txtFind.Focus();
            }
            else
            {
                Scintilla?.Focus();
            }
        }

        private void FindNext()
        {
            if (string.IsNullOrEmpty(txtFind.Text))
                return;
            if (Scintilla == null)
                return;

            var r = _findReplace?.FindReplace.FindNext(txtFind.Text, true, _findReplace.GetSearchFlags());
            if (r != null && r.Value.MinPosition !=  r.Value.MaxPosition)
                Scintilla.SetSel(r.Value.MinPosition, r.Value.MaxPosition);

            MoveFormAwayFromSelection();
        }

        private void FindPrevious()
        {
            if (string.IsNullOrEmpty(txtFind.Text))
                return;
            if (Scintilla == null)
                return;

            var r = _findReplace?.FindReplace.FindPrevious(txtFind.Text, true, _findReplace.GetSearchFlags());
            if (r != null && r.Value.MinPosition != r.Value.MaxPosition)
            {
                Scintilla.SetSel(r.Value.MinPosition, r.Value.MaxPosition);
            }

            MoveFormAwayFromSelection();
        }

        private void FindKeyDownEventHandler(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                case Keys.Down:
                    FindNext();
                    e.Handled = true;
                    break;

                case Keys.Up:
                    FindPrevious();
                    e.Handled = true;
                    break;

                case Keys.Escape:
                    if (!_toolItem)
                        Hide();
                    break;
            }
        }

        private void FindTextChangedEventHandler(object? sender, EventArgs e)
        {
            txtFind.BackColor = SystemColors.Window;
            if (string.IsNullOrEmpty(txtFind.Text))
                return;
            if (Scintilla == null)
                return;

            int pos = Math.Min(Scintilla.CurrentPosition, Scintilla.AnchorPosition);
            var r = _findReplace?.FindReplace.Find(pos, Scintilla.TextLength, txtFind.Text, _findReplace.GetSearchFlags());
            if (r == null || r.Value.MinPosition == r.Value.MaxPosition)
                r = _findReplace?.FindReplace.Find(0, pos, txtFind.Text, _findReplace.GetSearchFlags());

            if (r != null && r.Value.MinPosition != r.Value.MaxPosition)
                Scintilla.SetSel(r.Value.MinPosition, r.Value.MaxPosition);
            else
                txtFind.BackColor = Color.Tomato;

            MoveFormAwayFromSelection();
        }

        #endregion Methods
    }
}