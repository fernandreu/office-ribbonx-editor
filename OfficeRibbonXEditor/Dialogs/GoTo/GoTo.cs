namespace OfficeRibbonXEditor.Dialogs.GoTo
{
	using ScintillaNET;

	public class GoTo
	{
		private Scintilla _scintilla;
		private GoToDialog _window;

		#region Methods

		public void Line(int number)
		{
			this._scintilla.Lines[number].Goto();
		}

		public void Position(int pos)
		{
			this._scintilla.GotoPosition(pos);
		}

		public void ShowGoToDialog()
		{
			//GoToDialog gd = new GoToDialog();
			GoToDialog gd = this._window;

			gd.CurrentLineNumber = this._scintilla.CurrentLine;
			gd.MaximumLineNumber = this._scintilla.Lines.Count;
			gd.Scintilla = this._scintilla;

			if (!this._window.Visible)
				this._window.Show(this._scintilla.FindForm());

			//_window.ShowDialog(_scintilla.FindForm());
			//_window.Show(_scintilla.FindForm());

			//if (gd.ShowDialog() == DialogResult.OK)
			//Line(gd.GotoLineNumber);

			//gd.ShowDialog();
			//gd.Show();

			this._scintilla.Focus();
		}

		#endregion Methods

		#region Constructors

		protected virtual GoToDialog CreateWindowInstance()
		{
			return new GoToDialog();
		}

		public GoTo(Scintilla scintilla)
		{
			this._scintilla = scintilla;
			this._window = this.CreateWindowInstance();
			this._window.Scintilla = scintilla;
		}

		#endregion Constructors
	}
}