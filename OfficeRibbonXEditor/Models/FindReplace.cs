using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using OfficeRibbonXEditor.Interfaces;
using ScintillaNET;

namespace OfficeRibbonXEditor.Models
{
	using ResultsEventArgs = DataEventArgs<IResultCollection>;

    public class FindReplace
    {
	    private const SearchFlags Flags = SearchFlags.None;

	    public readonly Scintilla Scintilla;

	    public FindReplace(Scintilla scintilla)
	    {
		    this.Scintilla = scintilla;

		    this.Marker = this.Scintilla.Markers[10];
		    this.Marker.Symbol = MarkerSymbol.Circle;
		    this.Marker.SetForeColor(Color.Black);
		    this.Marker.SetBackColor(Color.Blue);
		    this.Indicator = this.Scintilla.Indicators[16];
		    this.Indicator.ForeColor = Color.Red;
		    //_indicator.ForeColor = Color.LawnGreen; //Smart highlight
		    this.Indicator.Alpha = 100;
		    this.Indicator.Style = IndicatorStyle.RoundBox;
		    this.Indicator.Under = true;

            ////this.window.Scintilla = this.scintilla;
            ////this.window.FindReplace = this;
            ////this.window.KeyPressed += this._window_KeyPressed;
        }

        public Indicator Indicator { get; set; }

		public Marker Marker { get; set; }
		
		public event FindAllResultsEventHandler FindAllResults;

		public event ReplaceAllResultsEventHandler ReplaceAllResults;

		public delegate void FindAllResultsEventHandler(object sender, ResultsEventArgs findAllResults);

		public delegate void ReplaceAllResultsEventHandler(object sender, ReplaceResultsEventArgs findAllResults);

		public void ClearAllHighlights()
		{
			var currentIndicator = this.Scintilla.IndicatorCurrent;

			this.Scintilla.IndicatorCurrent = this.Indicator.Index;
			this.Scintilla.IndicatorClearRange(0, this.Scintilla.TextLength);

			this.Scintilla.IndicatorCurrent = currentIndicator;
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

			this.Scintilla.TargetStart = startPos;
			this.Scintilla.TargetEnd = endPos;
			this.Scintilla.SearchFlags = flags;
			var pos = this.Scintilla.SearchInTarget(searchString);
			return pos == -1 ? new CharacterRange() : new CharacterRange(this.Scintilla.TargetStart, this.Scintilla.TargetEnd);
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

			var text = this.Scintilla.GetTextRange(r.cpMin, r.cpMax - r.cpMin + 1);

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
			return this.Find(new CharacterRange(0, this.Scintilla.TextLength), findExpression, false);
		}

		public CharacterRange Find(Regex findExpression, bool searchUp)
		{
			return this.Find(new CharacterRange(0, this.Scintilla.TextLength), findExpression, searchUp);
		}

		public CharacterRange Find(string searchString)
		{
			return this.Find(0, this.Scintilla.TextLength, searchString, Flags);
		}

		public CharacterRange Find(string searchString, bool searchUp)
		{
			if (searchUp)
				return this.Find(this.Scintilla.TextLength, 0, searchString, Flags);
			else
				return this.Find(0, this.Scintilla.TextLength, searchString, Flags);
		}

		public CharacterRange Find(string searchString, SearchFlags searchFlags)
		{
			return this.Find(0, this.Scintilla.TextLength, searchString, searchFlags);
		}

		public CharacterRange Find(string searchString, SearchFlags searchFlags, bool searchUp)
		{
			if (searchUp)
				return this.Find(this.Scintilla.TextLength, 0, searchString, searchFlags);
			else
				return this.Find(0, this.Scintilla.TextLength, searchString, searchFlags);
		}

		public CharacterRange FindNext(Regex findExpression)
		{
			return this.FindNext(findExpression, false);
		}

		public CharacterRange FindNext(Regex findExpression, bool wrap)
		{
			var r = this.Find(this.Scintilla.CurrentPosition, this.Scintilla.TextLength, findExpression);
			if (r.cpMin != r.cpMax)
				return r;
			else if (wrap)
				return this.Find(0, this.Scintilla.CurrentPosition, findExpression);
			else
				return new CharacterRange();
		}

		public CharacterRange FindNext(Regex findExpression, bool wrap, CharacterRange searchRange)
		{
			var caret = this.Scintilla.CurrentPosition;
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
			var r = this.Find(this.Scintilla.CurrentPosition, this.Scintilla.TextLength, searchString, flags);
			if (r.cpMin != r.cpMax)
				return r;
			else if (wrap)
				return this.Find(0, this.Scintilla.CurrentPosition, searchString, flags);
			else
				return new CharacterRange();
		}

		public CharacterRange FindNext(string searchString, bool wrap, SearchFlags flags, CharacterRange searchRange)
		{
			var caret = this.Scintilla.CurrentPosition;
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
			var r = this.Find(0, this.Scintilla.AnchorPosition, findExpression, true);
			if (r.cpMin != r.cpMax)
				return r;
			else if (wrap)
				return this.Find(this.Scintilla.CurrentPosition, this.Scintilla.TextLength, findExpression, true);
			else
				return new CharacterRange();
		}

		public CharacterRange FindPrevious(Regex findExpression, bool wrap, CharacterRange searchRange)
		{
			var caret = this.Scintilla.CurrentPosition;
			if (!(caret >= searchRange.cpMin && caret <= searchRange.cpMax))
				return this.Find(searchRange.cpMin, searchRange.cpMax, findExpression, true);

			var anchor = this.Scintilla.AnchorPosition;
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
			var r = this.Find(this.Scintilla.AnchorPosition, 0, searchString, flags);
			if (r.cpMin != r.cpMax)
				return r;
			else if (wrap)
				return this.Find(this.Scintilla.TextLength, this.Scintilla.CurrentPosition, searchString, flags);
			else
				return new CharacterRange();
		}

		public CharacterRange FindPrevious(string searchString, bool wrap, SearchFlags flags, CharacterRange searchRange)
		{
			var caret = this.Scintilla.CurrentPosition;
			if (!(caret >= searchRange.cpMin && caret <= searchRange.cpMax))
				return this.Find(searchRange.cpMax, searchRange.cpMin, searchString, flags);

			var anchor = this.Scintilla.AnchorPosition;
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

		public List<CharacterRange> FindAll(int startPos, int endPos, Regex findExpression, bool mark, bool highlight)
		{
			return this.FindAll(new CharacterRange(startPos, endPos), findExpression, mark, highlight);
		}

		public List<CharacterRange> FindAll(int startPos, int endPos, string searchString, SearchFlags flags, bool mark, bool highlight)
		{
			var results = new List<CharacterRange>();

			this.Scintilla.IndicatorCurrent = this.Indicator.Index;

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
					var line = new Line(this.Scintilla, this.Scintilla.LineFromPosition(r.cpMin));
					if (line.Position > lastLine)
						line.MarkerAdd(this.Marker.Index);
					lastLine = line.Position;
				}
				if (highlight)
				{
					this.Scintilla.IndicatorFillRange(r.cpMin, r.cpMax - r.cpMin);
				}
				startPos = r.cpMax;
			}

            this.FindAllResults?.Invoke(this, new ResultsEventArgs(new FindResults(results)));

            return results;
		}

		public List<CharacterRange> FindAll(CharacterRange rangeToSearch, Regex findExpression, bool mark, bool highlight)
		{
			var results = new List<CharacterRange>();

			this.Scintilla.IndicatorCurrent = this.Indicator.Index;

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
					var line = new Line(this.Scintilla, this.Scintilla.LineFromPosition(r.cpMin));
					if (line.Position > lastLine)
						line.MarkerAdd(this.Marker.Index);
					lastLine = line.Position;
				}
				if (highlight)
				{
					this.Scintilla.IndicatorFillRange(r.cpMin, r.cpMax - r.cpMin);
				}
				rangeToSearch = new CharacterRange(r.cpMax, rangeToSearch.cpMax);
			}

            this.FindAllResults?.Invoke(this, new ResultsEventArgs(new FindResults(results)));

            return results;
		}

		public List<CharacterRange> FindAll(CharacterRange rangeToSearch, string searchString, SearchFlags flags, bool mark, bool highlight)
		{
			return this.FindAll(rangeToSearch.cpMin, rangeToSearch.cpMax, searchString, Flags, mark, highlight);
		}

		public List<CharacterRange> FindAll(Regex findExpression, bool mark, bool highlight)
		{
			return this.FindAll(0, this.Scintilla.TextLength, findExpression, mark, highlight);
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
			return this.FindAll(0, this.Scintilla.TextLength, searchString, flags, mark, highlight);
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
    }
}
