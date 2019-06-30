using OfficeRibbonXEditor.Models;

namespace OfficeRibbonXEditor.Dialogs.FindReplace.FindReplace
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Drawing;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using ScintillaNET;
	using CharacterRange = CharacterRange;

	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class FindReplace : Component
	{
		#region Fields

		private const SearchFlags Flags = SearchFlags.None;

		private IncrementalSearcher incrementalSearcher;

		private Indicator indicator;

		private int lastReplaceAllOffset = 0;

		private CharacterRange lastReplaceAllRangeToSearch;

		private string lastReplaceAllReplaceString = "";

		private int lastReplaceCount = 0;

		private Marker marker;

		private Scintilla scintilla;

		private FindReplaceDialog window;

		#endregion Fields

		public event FindAllResultsEventHandler FindAllResults;

		public event ReplaceAllResultsEventHandler ReplaceAllResults;

		public delegate void FindAllResultsEventHandler(object sender, FindResultsEventArgs findAllResults);

		public delegate void ReplaceAllResultsEventHandler(object sender, ReplaceResultsEventArgs findAllResults);

		#region Properties

		public Scintilla Scintilla
		{
			get => this.scintilla;
			set
			{
				if (this.window == null)
				{
					this.window = this.CreateWindowInstance();
				}

				this.scintilla = value;
				this.marker = this.scintilla.Markers[10];
				this.marker.Symbol = MarkerSymbol.Circle;
				this.marker.SetForeColor(Color.Black);
				this.marker.SetBackColor(Color.Blue);
				this.indicator = this.scintilla.Indicators[16];
				this.indicator.ForeColor = Color.Red;
				//_indicator.ForeColor = Color.LawnGreen; //Smart highlight
				this.indicator.Alpha = 100;
				this.indicator.Style = IndicatorStyle.RoundBox;
				this.indicator.Under = true;

				this.window.Scintilla = this.scintilla;
				this.window.FindReplace = this;
				this.window.KeyPressed += this._window_KeyPressed;

				this.incrementalSearcher = this.CreateIncrementalSearcherInstance();
				this.incrementalSearcher.Scintilla = this.scintilla;
				this.incrementalSearcher.FindReplace = this;
				this.incrementalSearcher.Visible = false;
				this.scintilla.Controls.Add(this.incrementalSearcher);
			}
		}

		/// <summary>
		/// Triggered when a key is pressed on the Find and Replace Dialog.
		/// </summary>
		public event KeyPressedHandler KeyPressed;

		/// <summary>
		/// Handler for the key press on a Find and Replace Dialog.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The key info of the key(s) pressed.</param>
		public delegate void KeyPressedHandler(object sender, KeyEventArgs e);

		private void _window_KeyPressed(object sender, KeyEventArgs e)
		{
			this.KeyPressed?.Invoke(this, e);
		}

		//[Editor(typeof(ScintillaNET.Design.FlagEnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
		//public SearchFlags Flags
		//{
		//	get
		//	{
		//		return _flags;
		//	}
		//	set
		//	{
		//		_flags = value;
		//	}
		//}
		[Browsable(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IncrementalSearcher IncrementalSearcher
		{
			get => this.incrementalSearcher;
			set => this.incrementalSearcher = value;
		}

		public Indicator Indicator
		{
			get => this.indicator;
			set => this.indicator = value;
		}

		public Marker Marker
		{
			get => this.marker;
			set => this.marker = value;
		}

		[Browsable(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public FindReplaceDialog Window
		{
			get => this.window;
			set => this.window = value;
		}

		[Browsable(false)]
		public bool LastReplaceHighlight
		{
			get; set;
		}

		[Browsable(false)]
		public int LastReplaceLastLine
		{
			get; set;
		}

		[Browsable(false)]
		public bool LastReplaceMark
		{
			get; set;
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Clears highlights from the entire document
		/// </summary>
		public void ClearAllHighlights()
		{
			var currentIndicator = this.scintilla.IndicatorCurrent;

			this.scintilla.IndicatorCurrent = this.Indicator.Index;
			this.scintilla.IndicatorClearRange(0, this.scintilla.TextLength);

			this.scintilla.IndicatorCurrent = currentIndicator;
		}

		/// <summary>
		/// Highlight ranges in the document.
		/// </summary>
		/// <param name="ranges">List of ranges to which highlighting should be applied.</param>
		public void HighlightAll(List<CharacterRange> ranges)
		{
			this.scintilla.IndicatorCurrent = this.Indicator.Index;

			foreach (var r in ranges)
			{
				this.scintilla.IndicatorFillRange(r.cpMin, r.cpMax - r.cpMin);
			}
		}

		public CharacterRange Find(int startPos, int endPos, Regex findExpression)
		{
			return this.Find(new CharacterRange(startPos, endPos), findExpression, false);
		}

		public CharacterRange Find(int startPos, int endPos, Regex findExpression, bool searchUp)
		{
			return this.Find(new CharacterRange(startPos, endPos), findExpression, searchUp);
		}

		public CharacterRange Find(int startPos, int endPos, string searchString, SearchFlags flags)
		{
			if (string.IsNullOrEmpty(searchString))
				return new CharacterRange();

			this.scintilla.TargetStart = startPos;
			this.scintilla.TargetEnd = endPos;
			this.scintilla.SearchFlags = flags;
			var pos = this.scintilla.SearchInTarget(searchString);
			return pos == -1 ? new CharacterRange() : new CharacterRange(this.scintilla.TargetStart, this.scintilla.TargetEnd);
		}

		public CharacterRange Find(CharacterRange r, Regex findExpression)
		{
			return this.Find(r, findExpression, false);
		}

		public CharacterRange Find(CharacterRange r, Regex findExpression, bool searchUp)
		{
			// Single line and Multi Line in RegExp doesn't really effect
			// whether or not a match will include newline characters. This
			// means we can't do a line by line search. We have to search
			// the entire range because it could potentially match the
			// entire range.

			var text = this.scintilla.GetTextRange(r.cpMin, r.cpMax - r.cpMin + 1);

			var m = findExpression.Match(text);

			if (!m.Success)
				return new CharacterRange();

			if (searchUp)
			{
				// Since we can't search backwards with RegExp we
				// have to search the entire string and return the
				// last match. Not the most efficient way of doing
				// things but it works.
				var range = new CharacterRange();
				while (m.Success)
				{
					//TODO - check that removing the byte count does not upset anything
					//int start = r.cpMin + _scintilla.Encoding.GetByteCount(text.Substring(0, m.Index));
					//int end = _scintilla.Encoding.GetByteCount(text.Substring(m.Index, m.Length));
					var start = r.cpMin + text.Substring(0, m.Index).Length;
					var end = text.Substring(m.Index, m.Length).Length;

					range = new CharacterRange(start, start + end);
					m = m.NextMatch();
				}

				return range;
			}
			else
			{
				//TODO - check that removing the byte count does not upset anything
				//int start = r.cpMin + _scintilla.Encoding.GetByteCount(text.Substring(0, m.Index));
				//int end = _scintilla.Encoding.GetByteCount(text.Substring(m.Index, m.Length));
				var start = r.cpMin + text.Substring(0, m.Index).Length;
				var end = text.Substring(m.Index, m.Length).Length;

				return new CharacterRange(start, start + end);
			}
		}

		public CharacterRange Find(CharacterRange rangeToSearch, string searchString)
		{
			return this.Find(rangeToSearch.cpMin, rangeToSearch.cpMax, searchString, Flags);
		}

		public CharacterRange Find(CharacterRange rangeToSearch, string searchString, bool searchUp)
		{
			if (searchUp)
				return this.Find(rangeToSearch.cpMax, rangeToSearch.cpMin, searchString, Flags);
			else
				return this.Find(rangeToSearch.cpMin, rangeToSearch.cpMax, searchString, Flags);
		}

		public CharacterRange Find(CharacterRange rangeToSearch, string searchString, SearchFlags searchflags)
		{
			return this.Find(rangeToSearch.cpMin, rangeToSearch.cpMax, searchString, searchflags);
		}

		public CharacterRange Find(CharacterRange rangeToSearch, string searchString, SearchFlags searchflags, bool searchUp)
		{
			if (searchUp)
				return this.Find(rangeToSearch.cpMax, rangeToSearch.cpMin, searchString, searchflags);
			else
				return this.Find(rangeToSearch.cpMin, rangeToSearch.cpMax, searchString, searchflags);
		}

		public CharacterRange Find(Regex findExpression)
		{
			return this.Find(new CharacterRange(0, this.scintilla.TextLength), findExpression, false);
		}

		public CharacterRange Find(Regex findExpression, bool searchUp)
		{
			return this.Find(new CharacterRange(0, this.scintilla.TextLength), findExpression, searchUp);
		}

		public CharacterRange Find(string searchString)
		{
			return this.Find(0, this.scintilla.TextLength, searchString, Flags);
		}

		public CharacterRange Find(string searchString, bool searchUp)
		{
			if (searchUp)
				return this.Find(this.scintilla.TextLength, 0, searchString, Flags);
			else
				return this.Find(0, this.scintilla.TextLength, searchString, Flags);
		}

		public CharacterRange Find(string searchString, SearchFlags searchflags)
		{
			return this.Find(0, this.scintilla.TextLength, searchString, searchflags);
		}

		public CharacterRange Find(string searchString, SearchFlags searchflags, bool searchUp)
		{
			if (searchUp)
				return this.Find(this.scintilla.TextLength, 0, searchString, searchflags);
			else
				return this.Find(0, this.scintilla.TextLength, searchString, searchflags);
		}

		public List<CharacterRange> FindAll(int startPos, int endPos, Regex findExpression, bool mark, bool highlight)
		{
			return this.FindAll(new CharacterRange(startPos, endPos), findExpression, mark, highlight);
		}

		public List<CharacterRange> FindAll(int startPos, int endPos, string searchString, SearchFlags flags, bool mark, bool highlight)
		{
			var results = new List<CharacterRange>();

			this.scintilla.IndicatorCurrent = this.Indicator.Index;

			var findCount = 0;
			var lastLine = -1;
			while (true)
			{
				var r = this.Find(startPos, endPos, searchString, flags);
				if (r.cpMin == r.cpMax)
				{
					break;
				}

				results.Add(r);
				findCount++;
				if (mark)
				{
					//	We can of course have multiple instances of a find on a single
					//	line. We don't want to mark this line more than once.
					var line = new Line(this.scintilla, this.scintilla.LineFromPosition(r.cpMin));
					if (line.Position > lastLine)
						line.MarkerAdd(this.marker.Index);
					lastLine = line.Position;
				}
				if (highlight)
				{
					this.scintilla.IndicatorFillRange(r.cpMin, r.cpMax - r.cpMin);
				}
				startPos = r.cpMax;
			}
			//return findCount;

			this.FindAllResults?.Invoke(this, new FindResultsEventArgs(this, results));

			return results;
		}

		public List<CharacterRange> FindAll(CharacterRange rangeToSearch, Regex findExpression, bool mark, bool highlight)
		{
			var results = new List<CharacterRange>();

			this.scintilla.IndicatorCurrent = this.Indicator.Index;

			var findCount = 0;
			var lastLine = -1;

			while (true)
			{
				var r = this.Find(rangeToSearch, findExpression);
				if (r.cpMin == r.cpMax)
				{
					break;
				}

				results.Add(r);
				findCount++;
				if (mark)
				{
					//	We can of course have multiple instances of a find on a single
					//	line. We don't want to mark this line more than once.
					var line = new Line(this.scintilla, this.scintilla.LineFromPosition(r.cpMin));
					if (line.Position > lastLine)
						line.MarkerAdd(this.marker.Index);
					lastLine = line.Position;
				}
				if (highlight)
				{
					this.scintilla.IndicatorFillRange(r.cpMin, r.cpMax - r.cpMin);
				}
				rangeToSearch = new CharacterRange(r.cpMax, rangeToSearch.cpMax);
			}
			//return findCount;
			this.FindAllResults?.Invoke(this, new FindResultsEventArgs(this, results));

			return results;
		}

		public List<CharacterRange> FindAll(CharacterRange rangeToSearch, string searchString, SearchFlags flags, bool mark, bool highlight)
		{
			return this.FindAll(rangeToSearch.cpMin, rangeToSearch.cpMax, searchString, FindReplace.Flags, mark, highlight);
		}

		public List<CharacterRange> FindAll(Regex findExpression, bool mark, bool highlight)
		{
			return this.FindAll(0, this.scintilla.TextLength, findExpression, mark, highlight);
		}

		public List<CharacterRange> FindAll(string searchString, bool mark, bool highlight)
		{
			return this.FindAll(searchString, Flags, mark, highlight);
		}

		public List<CharacterRange> FindAll(string searchString)
		{
			return this.FindAll(searchString, Flags, false, false);
		}

		public List<CharacterRange> FindAll(string searchString, SearchFlags flags, bool mark, bool highlight)
		{
			return this.FindAll(0, this.scintilla.TextLength, searchString, flags, mark, highlight);
		}

		public CharacterRange FindNext(Regex findExpression)
		{
			return this.FindNext(findExpression, false);
		}

		public CharacterRange FindNext(Regex findExpression, bool wrap)
		{
			var r = this.Find(this.scintilla.CurrentPosition, this.scintilla.TextLength, findExpression);
			if (r.cpMin != r.cpMax)
				return r;
			else if (wrap)
				return this.Find(0, this.scintilla.CurrentPosition, findExpression);
			else
				return new CharacterRange();
		}

		public CharacterRange FindNext(Regex findExpression, bool wrap, CharacterRange searchRange)
		{
			var caret = this.scintilla.CurrentPosition;
			if (!(caret >= searchRange.cpMin && caret <= searchRange.cpMax))
				return this.Find(searchRange.cpMin, searchRange.cpMax, findExpression, false);

			var r = this.Find(caret, searchRange.cpMax, findExpression);
			if (r.cpMin != r.cpMax)
				return r;
			else if (wrap)
				return this.Find(searchRange.cpMin, caret, findExpression);
			else
				return new CharacterRange();
		}

		public CharacterRange FindNext(string searchString)
		{
			return this.FindNext(searchString, true, Flags);
		}

		public CharacterRange FindNext(string searchString, bool wrap)
		{
			return this.FindNext(searchString, wrap, Flags);
		}

		public CharacterRange FindNext(string searchString, bool wrap, SearchFlags flags)
		{
			var r = this.Find(this.scintilla.CurrentPosition, this.scintilla.TextLength, searchString, flags);
			if (r.cpMin != r.cpMax)
				return r;
			else if (wrap)
				return this.Find(0, this.scintilla.CurrentPosition, searchString, flags);
			else
				return new CharacterRange();
		}

		public CharacterRange FindNext(string searchString, bool wrap, SearchFlags flags, CharacterRange searchRange)
		{
			var caret = this.scintilla.CurrentPosition;
			if (!(caret >= searchRange.cpMin && caret <= searchRange.cpMax))
				return this.Find(searchRange.cpMin, searchRange.cpMax, searchString, flags);

			var r = this.Find(caret, searchRange.cpMax, searchString, flags);
			if (r.cpMin != r.cpMax)
				return r;
			else if (wrap)
				return this.Find(searchRange.cpMin, caret, searchString, flags);
			else
				return new CharacterRange();
		}

		public CharacterRange FindNext(string searchString, SearchFlags flags)
		{
			return this.FindNext(searchString, true, flags);
		}

		public CharacterRange FindPrevious(Regex findExpression)
		{
			return this.FindPrevious(findExpression, false);
		}

		public CharacterRange FindPrevious(Regex findExpression, bool wrap)
		{
			var r = this.Find(0, this.scintilla.AnchorPosition, findExpression, true);
			if (r.cpMin != r.cpMax)
				return r;
			else if (wrap)
				return this.Find(this.scintilla.CurrentPosition, this.scintilla.TextLength, findExpression, true);
			else
				return new CharacterRange();
		}

		public CharacterRange FindPrevious(Regex findExpression, bool wrap, CharacterRange searchRange)
		{
			var caret = this.scintilla.CurrentPosition;
			if (!(caret >= searchRange.cpMin && caret <= searchRange.cpMax))
				return this.Find(searchRange.cpMin, searchRange.cpMax, findExpression, true);

			var anchor = this.scintilla.AnchorPosition;
			if (!(anchor >= searchRange.cpMin && anchor <= searchRange.cpMax))
				anchor = caret;

			var r = this.Find(searchRange.cpMin, anchor, findExpression, true);
			if (r.cpMin != r.cpMax)
				return r;
			else if (wrap)
				return this.Find(anchor, searchRange.cpMax, findExpression, true);
			else
				return new CharacterRange();
		}

		public CharacterRange FindPrevious(string searchString)
		{
			return this.FindPrevious(searchString, true, Flags);
		}

		public CharacterRange FindPrevious(string searchString, bool wrap)
		{
			return this.FindPrevious(searchString, wrap, Flags);
		}

		public CharacterRange FindPrevious(string searchString, bool wrap, SearchFlags flags)
		{
			var r = this.Find(this.scintilla.AnchorPosition, 0, searchString, flags);
			if (r.cpMin != r.cpMax)
				return r;
			else if (wrap)
				return this.Find(this.scintilla.TextLength, this.scintilla.CurrentPosition, searchString, flags);
			else
				return new CharacterRange();
		}

		public CharacterRange FindPrevious(string searchString, bool wrap, SearchFlags flags, CharacterRange searchRange)
		{
			var caret = this.scintilla.CurrentPosition;
			if (!(caret >= searchRange.cpMin && caret <= searchRange.cpMax))
				return this.Find(searchRange.cpMax, searchRange.cpMin, searchString, flags);

			var anchor = this.scintilla.AnchorPosition;
			if (!(anchor >= searchRange.cpMin && anchor <= searchRange.cpMax))
				anchor = caret;

			var r = this.Find(anchor, searchRange.cpMin, searchString, flags);
			if (r.cpMin != r.cpMax)
				return r;
			else if (wrap)
				return this.Find(searchRange.cpMax, anchor, searchString, flags);
			else
				return new CharacterRange();
		}

		public CharacterRange FindPrevious(string searchString, SearchFlags flags)
		{
			return this.FindPrevious(searchString, true, flags);
		}

		public int ReplaceAll(int startPos, int endPos, Regex findExpression, string replaceString, bool mark, bool highlight)
		{
			return this.ReplaceAll(new CharacterRange(startPos, endPos), findExpression, replaceString, mark, highlight);
		}

		public int ReplaceAll(int startPos, int endPos, string searchString, string replaceString, SearchFlags flags, bool mark, bool highlight)
		{
			var results = new List<CharacterRange>();

			this.scintilla.IndicatorCurrent = this.Indicator.Index;

			var findCount = 0;
			var lastLine = -1;

			this.scintilla.BeginUndoAction();

			var diff = replaceString.Length - searchString.Length;
			while (true)
			{
				var r = this.Find(startPos, endPos, searchString, flags);
				if (r.cpMin == r.cpMax)
				{
					break;
				}
				else
				{
					this.scintilla.SelectionStart = r.cpMin;
					this.scintilla.SelectionEnd = r.cpMax;
					this.scintilla.ReplaceSelection(replaceString);
					r.cpMax = startPos = r.cpMin + replaceString.Length;
					endPos += diff;

					results.Add(r);
					findCount++;

					if (mark)
					{
						//	We can of course have multiple instances of a find on a single
						//	line. We don't want to mark this line more than once.
						var line = new Line(this.scintilla, this.scintilla.LineFromPosition(r.cpMin));
						if (line.Position > lastLine)
							line.MarkerAdd(this.marker.Index);
						lastLine = line.Position;
					}
					if (highlight)
					{
						this.scintilla.IndicatorFillRange(r.cpMin, r.cpMax - r.cpMin);
					}
				}
			}

			this.scintilla.EndUndoAction();

			this.ReplaceAllResults?.Invoke(this, new ReplaceResultsEventArgs(this, results));

			return findCount;
		}

		public int ReplaceAll(CharacterRange rangeToSearch, Regex findExpression, string replaceString, bool mark, bool highlight)
		{
			this.scintilla.IndicatorCurrent = this.Indicator.Index;
			this.scintilla.BeginUndoAction();

			//	I tried using an anonymous delegate for this but it didn't work too well.
			//	It's too bad because it was a lot cleaner than using member variables as
			//	psuedo globals.
			this.lastReplaceAllReplaceString = replaceString;
			this.lastReplaceAllRangeToSearch = rangeToSearch;
			this.lastReplaceAllOffset = 0;
			this.lastReplaceCount = 0;
			this.LastReplaceMark = mark;
			this.LastReplaceHighlight = highlight;

			var text = this.scintilla.GetTextRange(rangeToSearch.cpMin, rangeToSearch.cpMax - rangeToSearch.cpMin + 1);
			findExpression.Replace(text, new MatchEvaluator(this.ReplaceAllEvaluator));

			this.scintilla.EndUndoAction();

			//	No use having these values hanging around wasting memory :)
			this.lastReplaceAllReplaceString = null;
			this.lastReplaceAllRangeToSearch = new CharacterRange();

			return this.lastReplaceCount;
		}

		public int ReplaceAll(CharacterRange rangeToSearch, string searchString, string replaceString, SearchFlags flags, bool mark, bool highlight)
		{
			return this.ReplaceAll(rangeToSearch.cpMin, rangeToSearch.cpMax, searchString, replaceString, FindReplace.Flags, mark, highlight);
		}

		public int ReplaceAll(Regex findExpression, string replaceString, bool mark, bool highlight)
		{
			return this.ReplaceAll(0, this.scintilla.TextLength, findExpression, replaceString, mark, highlight);
		}

		public int ReplaceAll(string searchString, string replaceString, SearchFlags flags, bool mark, bool highlight)
		{
			return this.ReplaceAll(0, this.scintilla.TextLength, searchString, replaceString, flags, mark, highlight);
		}

		public CharacterRange ReplaceNext(string searchString, string replaceString)
		{
			return this.ReplaceNext(searchString, replaceString, true, Flags);
		}

		public CharacterRange ReplaceNext(string searchString, string replaceString, bool wrap)
		{
			return this.ReplaceNext(searchString, replaceString, wrap, Flags);
		}

		public CharacterRange ReplaceNext(string searchString, string replaceString, bool wrap, SearchFlags flags)
		{
			var r = this.FindNext(searchString, wrap, flags);

			if (r.cpMin != r.cpMax)
			{
				this.scintilla.SelectionStart = r.cpMin;
				this.scintilla.SelectionEnd = r.cpMax;
				this.scintilla.ReplaceSelection(replaceString);
				r.cpMax = r.cpMin + replaceString.Length;
			}

			return r;
		}

		public CharacterRange ReplaceNext(string searchString, string replaceString, SearchFlags flags)
		{
			return this.ReplaceNext(searchString, replaceString, true, flags);
		}

		public CharacterRange ReplacePrevious(string searchString, string replaceString)
		{
			return this.ReplacePrevious(searchString, replaceString, true, Flags);
		}

		public CharacterRange ReplacePrevious(string searchString, string replaceString, bool wrap)
		{
			return this.ReplacePrevious(searchString, replaceString, wrap, Flags);
		}

		public CharacterRange ReplacePrevious(string searchString, string replaceString, bool wrap, SearchFlags flags)
		{
			var r = this.FindPrevious(searchString, wrap, flags);

			if (r.cpMin != r.cpMax)
			{
				this.scintilla.SelectionStart = r.cpMin;
				this.scintilla.SelectionEnd = r.cpMax;
				this.scintilla.ReplaceSelection(replaceString);
				r.cpMax = r.cpMin + replaceString.Length;
			}

			return r;
		}

		public CharacterRange ReplacePrevious(string searchString, string replaceString, SearchFlags flags)
		{
			return this.ReplacePrevious(searchString, replaceString, true, flags);
		}

		//private void ResetFlags()
		//{
		//	_flags = SearchFlags.Empty;
		//}
		//private void ResetIndicator()
		//{
		//	_indicator. Reset();
		//}
		//private void ResetMarker()
		//{
		//	_marker.Reset();
		//	_marker.Number = 10;
		//}
		//internal bool ShouldSerialize()
		//{
		//	return ShouldSerializeFlags() ||
		//		ShouldSerializeIndicator() ||
		//		ShouldSerializeMarker();
		//}
		//private bool ShouldSerializeFlags()
		//{
		//	return _flags != SearchFlags.Empty;
		//}
		//private bool ShouldSerializeIndicator()
		//{
		//	return _indicator.Index != 16 || _indicator.Color != Color.Purple || _indicator.DrawMode != IndicatorDrawMode.Overlay;
		//}
		//private bool ShouldSerializeMarker()
		//{
		//	return _marker.Number != 10 || _marker.ForeColor != Color.White || _marker.BackColor != Color.Black || _marker.Symbol != MarkerSymbol.Arrows;
		//}
		public void ShowFind()
		{
			if (!this.window.Visible)
				this.window.Show(this.scintilla.FindForm());

			this.window.tabAll.SelectedTab = this.window.tabAll.TabPages["tpgFind"];

			if (this.scintilla.LineFromPosition(this.scintilla.Selections[0].Start) != this.scintilla.LineFromPosition(this.scintilla.Selections[0].End)) //selRange.IsMultiLine)
			{
				this.window.chkSearchSelectionF.Checked = true;
			}
			else if (this.scintilla.Selections[0].End > this.scintilla.Selections[0].Start)
			{
				this.window.txtFindF.Text = this.scintilla.SelectedText;
			}

			this.window.txtFindF.Select();
			this.window.txtFindF.SelectAll();
		}

		public void ShowIncrementalSearch()
		{
			this.incrementalSearcher.Show();
		}

		public void ShowReplace()
		{
			if (!this.window.Visible)
				this.window.Show(this.scintilla.FindForm());

			this.window.tabAll.SelectedTab = this.window.tabAll.TabPages["tpgReplace"];

			if (this.scintilla.LineFromPosition(this.scintilla.Selections[0].Start) != this.scintilla.LineFromPosition(this.scintilla.Selections[0].End)) //selRange.IsMultiLine)
			{
				this.window.chkSearchSelectionR.Checked = true;
			}
			else if (this.scintilla.Selections[0].End > this.scintilla.Selections[0].Start)
			{
				//_window.cboFindR.Text = selRange.Text;
				this.window.txtFindR.Text = this.scintilla.SelectedText;
			}

			this.window.txtFindR.Select();
			this.window.txtFindR.SelectAll();
		}

		public string Transform(string data)
		{
			var result = data;
			var nullChar = (char)0;
			var cr = (char)13;
			var lf = (char)10;
			var tab = (char)9;

			result = result.Replace("\\r\\n", Environment.NewLine);
			result = result.Replace("\\r", cr.ToString());
			result = result.Replace("\\n", lf.ToString());
			result = result.Replace("\\t", tab.ToString());
			result = result.Replace("\\0", nullChar.ToString());

			return result;
		}

		/// <summary>
		/// Creates and returns a new <see cref="IncrementalSearcher" /> object.
		/// </summary>
		/// <returns>A new <see cref="IncrementalSearcher" /> object.</returns>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual IncrementalSearcher CreateIncrementalSearcherInstance()
		{
			return new IncrementalSearcher();
		}

		/// <summary>
		/// Creates and returns a new <see cref="FindReplaceDialog" /> object.
		/// </summary>
		/// <returns>A new <see cref="FindReplaceDialog" /> object.</returns>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual FindReplaceDialog CreateWindowInstance()
		{
			return new FindReplaceDialog();
		}

		private string ReplaceAllEvaluator(Match m)
		{
			//	So this method is called for every match

			//	We make a replacement in the range based upon
			//	the match range.
			var replacement = m.Result(this.lastReplaceAllReplaceString);
			var start = this.lastReplaceAllRangeToSearch.cpMin + m.Index + this.lastReplaceAllOffset;
			var end = start + m.Length;

			var r = new CharacterRange(start, end);
			this.lastReplaceCount++;
			this.scintilla.SelectionStart = r.cpMin;
			this.scintilla.SelectionEnd = r.cpMax;
			this.scintilla.ReplaceSelection(replacement);

			if (this.LastReplaceMark)
			{
				//	We can of course have multiple instances of a find on a single
				//	line. We don't want to mark this line more than once.
				// TODO - Is determining the current line any more efficient that just setting the duplicate marker? LineFromPosition appears to have more code that MarkerAdd!
				var line = new Line(this.scintilla, this.scintilla.LineFromPosition(r.cpMin));
				if (line.Position > this.LastReplaceLastLine)
					line.MarkerAdd(this.marker.Index);
				this.LastReplaceLastLine = line.Position;
			}
			if (this.LastReplaceHighlight)
			{
				this.scintilla.IndicatorFillRange(r.cpMin, r.cpMax - r.cpMin);
			}

			//	But because we've modified the document, the RegEx
			//	match ranges are going to be different from the
			//	document ranges. We need to compensate
			this.lastReplaceAllOffset += replacement.Length - m.Value.Length;

			return replacement;
		}

		#endregion Methods
	}

	public class FindResultsEventArgs : EventArgs
	{
		public FindResultsEventArgs(FindReplace findReplace, List<CharacterRange> findAllResults)
		{
			this.FindReplace = findReplace;
			this.FindAllResults = findAllResults;
		}

		public FindReplace FindReplace { get; set; }
		public List<CharacterRange> FindAllResults { get; set; }
	}

	public class ReplaceResultsEventArgs : EventArgs
	{
		public ReplaceResultsEventArgs(FindReplace findReplace, List<CharacterRange> replaceAllResults)
		{
			this.FindReplace = findReplace;
			this.ReplaceAllResults = replaceAllResults;
		}

		public FindReplace FindReplace { get; set; }
		public List<CharacterRange> ReplaceAllResults { get; set; }
	}
}