using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text.RegularExpressions;
using OfficeRibbonXEditor.Events;
using OfficeRibbonXEditor.Interfaces;
using ScintillaNET;

namespace OfficeRibbonXEditor.Helpers
{
	using ResultsEventArgs = DataEventArgs<IResultCollection>;

    public class FindReplace
    {
	    private const SearchFlags Flags = SearchFlags.None;

	    public Scintilla Scintilla { get; }

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
		
		public event FindAllResultsEventHandler? FindAllResults;

		public event ReplaceAllResultsEventHandler? ReplaceAllResults;

		public delegate void FindAllResultsEventHandler(object sender, ResultsEventArgs findAllResults);

		public delegate void ReplaceAllResultsEventHandler(object sender, ResultsEventArgs replaceAllResults);

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

			var text = this.Scintilla.GetTextRange(r.MinPosition, r.MaxPosition - r.MinPosition);

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
					var start = r.MinPosition + text.Substring(0, m.Index).Length;
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
				var start = r.MinPosition + text.Substring(0, m.Index).Length;
				var end = text.Substring(m.Index, m.Length).Length;

				return new CharacterRange(start, start + end);
			}
		}

		public CharacterRange Find(CharacterRange rangeToSearch, string searchString)
		{
			return this.Find(rangeToSearch.MinPosition, rangeToSearch.MaxPosition, searchString, Flags);
		}

		public CharacterRange Find(CharacterRange rangeToSearch, string searchString, bool searchUp)
		{
			if (searchUp)
				return this.Find(rangeToSearch.MaxPosition, rangeToSearch.MinPosition, searchString, Flags);
			else
				return this.Find(rangeToSearch.MinPosition, rangeToSearch.MaxPosition, searchString, Flags);
		}

		public CharacterRange Find(CharacterRange rangeToSearch, string searchString, SearchFlags searchflags)
		{
			return this.Find(rangeToSearch.MinPosition, rangeToSearch.MaxPosition, searchString, searchflags);
		}

		public CharacterRange Find(CharacterRange rangeToSearch, string searchString, SearchFlags searchflags, bool searchUp)
		{
			if (searchUp)
				return this.Find(rangeToSearch.MaxPosition, rangeToSearch.MinPosition, searchString, searchflags);
			else
				return this.Find(rangeToSearch.MinPosition, rangeToSearch.MaxPosition, searchString, searchflags);
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
			if (r.MinPosition != r.MaxPosition)
				return r;
			else if (wrap)
				return this.Find(0, this.Scintilla.CurrentPosition, findExpression);
			else
				return new CharacterRange();
		}

		public CharacterRange FindNext(Regex findExpression, bool wrap, CharacterRange searchRange)
		{
			var caret = this.Scintilla.CurrentPosition;
			if (!(caret >= searchRange.MinPosition && caret <= searchRange.MaxPosition))
				return this.Find(searchRange.MinPosition, searchRange.MaxPosition, findExpression, false);

			var r = this.Find(caret, searchRange.MaxPosition, findExpression);
			if (r.MinPosition != r.MaxPosition)
				return r;
			else if (wrap)
				return this.Find(searchRange.MinPosition, caret, findExpression);
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
			if (r.MinPosition != r.MaxPosition)
				return r;
			else if (wrap)
				return this.Find(0, this.Scintilla.CurrentPosition, searchString, flags);
			else
				return new CharacterRange();
		}

		public CharacterRange FindNext(string searchString, bool wrap, SearchFlags flags, CharacterRange searchRange)
		{
			var caret = this.Scintilla.CurrentPosition;
			if (!(caret >= searchRange.MinPosition && caret <= searchRange.MaxPosition))
				return this.Find(searchRange.MinPosition, searchRange.MaxPosition, searchString, flags);

			var r = this.Find(caret, searchRange.MaxPosition, searchString, flags);
			if (r.MinPosition != r.MaxPosition)
				return r;
			else if (wrap)
				return this.Find(searchRange.MinPosition, caret, searchString, flags);
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
			if (r.MinPosition != r.MaxPosition)
				return r;
			else if (wrap)
				return this.Find(this.Scintilla.CurrentPosition, this.Scintilla.TextLength, findExpression, true);
			else
				return new CharacterRange();
		}

		public CharacterRange FindPrevious(Regex findExpression, bool wrap, CharacterRange searchRange)
		{
			var caret = this.Scintilla.CurrentPosition;
			if (!(caret >= searchRange.MinPosition && caret <= searchRange.MaxPosition))
				return this.Find(searchRange.MinPosition, searchRange.MaxPosition, findExpression, true);

			var anchor = this.Scintilla.AnchorPosition;
			if (!(anchor >= searchRange.MinPosition && anchor <= searchRange.MaxPosition))
				anchor = caret;

			var r = this.Find(searchRange.MinPosition, anchor, findExpression, true);
			if (r.MinPosition != r.MaxPosition)
				return r;
			else if (wrap)
				return this.Find(anchor, searchRange.MaxPosition, findExpression, true);
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
			if (r.MinPosition != r.MaxPosition)
				return r;
			else if (wrap)
				return this.Find(this.Scintilla.TextLength, this.Scintilla.CurrentPosition, searchString, flags);
			else
				return new CharacterRange();
		}

		public CharacterRange FindPrevious(string searchString, bool wrap, SearchFlags flags, CharacterRange searchRange)
		{
			var caret = this.Scintilla.CurrentPosition;
			if (!(caret >= searchRange.MinPosition && caret <= searchRange.MaxPosition))
				return this.Find(searchRange.MaxPosition, searchRange.MinPosition, searchString, flags);

			var anchor = this.Scintilla.AnchorPosition;
			if (!(anchor >= searchRange.MinPosition && anchor <= searchRange.MaxPosition))
				anchor = caret;

			var r = this.Find(anchor, searchRange.MinPosition, searchString, flags);
			if (r.MinPosition != r.MaxPosition)
				return r;
			else if (wrap)
				return this.Find(searchRange.MaxPosition, anchor, searchString, flags);
			else
				return new CharacterRange();
		}

		public CharacterRange FindPrevious(string searchString, SearchFlags flags)
		{
			return this.FindPrevious(searchString, true, flags);
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
				if (r.MinPosition == r.MaxPosition)
				{
					break;
				}

				results.Add(r);
				findCount++;
				if (mark)
				{
					//	We can of course have multiple instances of a find on a single
					//	line. We don't want to mark this line more than once.
					var line = new Line(this.Scintilla, this.Scintilla.LineFromPosition(r.MinPosition));
					if (line.Position > lastLine)
						line.MarkerAdd(this.Marker.Index);
					lastLine = line.Position;
				}
				if (highlight)
				{
					this.Scintilla.IndicatorFillRange(r.MinPosition, r.MaxPosition - r.MinPosition);
				}
				startPos = r.MaxPosition;
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
				if (r.MinPosition == r.MaxPosition)
				{
					break;
				}

				results.Add(r);
				findCount++;
				if (mark)
				{
					//	We can of course have multiple instances of a find on a single
					//	line. We don't want to mark this line more than once.
					var line = new Line(this.Scintilla, this.Scintilla.LineFromPosition(r.MinPosition));
					if (line.Position > lastLine)
						line.MarkerAdd(this.Marker.Index);
					lastLine = line.Position;
				}
				if (highlight)
				{
					this.Scintilla.IndicatorFillRange(r.MinPosition, r.MaxPosition - r.MinPosition);
				}
				rangeToSearch = new CharacterRange(r.MaxPosition, rangeToSearch.MaxPosition);
			}

            this.FindAllResults?.Invoke(this, new ResultsEventArgs(new FindResults(results)));

            return results;
		}

		public List<CharacterRange> FindAll(int startPos, int endPos, Regex findExpression, bool mark, bool highlight)
		{
			return this.FindAll(new CharacterRange(startPos, endPos), findExpression, mark, highlight);
		}

		public List<CharacterRange> FindAll(CharacterRange rangeToSearch, string searchString, SearchFlags flags, bool mark, bool highlight)
		{
			return this.FindAll(rangeToSearch.MinPosition, rangeToSearch.MaxPosition, searchString, flags, mark, highlight);
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

		public List<CharacterRange> ReplaceAll(int startPos, int endPos, string searchString, string replaceString, SearchFlags flags, bool mark, bool highlight)
		{
			var results = new List<CharacterRange>();

			this.Scintilla.IndicatorCurrent = this.Indicator.Index;

			var findCount = 0;
			var lastLine = -1;

			this.Scintilla.BeginUndoAction();

			var diff = replaceString.Length - searchString.Length;
			while (true)
			{
				var r = this.Find(startPos, endPos, searchString, flags);
				if (r.MinPosition == r.MaxPosition)
				{
					break;
				}

				this.Scintilla.SelectionStart = r.MinPosition;
				this.Scintilla.SelectionEnd = r.MaxPosition;
				this.Scintilla.ReplaceSelection(replaceString);
				r = new CharacterRange(r.MinPosition, startPos = r.MinPosition + replaceString.Length);
				endPos += diff;

				results.Add(r);
				findCount++;

				if (mark)
				{
					//	We can of course have multiple instances of a find on a single
					//	line. We don't want to mark this line more than once.
					var line = new Line(this.Scintilla, this.Scintilla.LineFromPosition(r.MinPosition));
					if (line.Position > lastLine)
						line.MarkerAdd(this.Marker.Index);
					lastLine = line.Position;
				}
				if (highlight)
				{
					this.Scintilla.IndicatorFillRange(r.MinPosition, r.MaxPosition - r.MinPosition);
				}
			}

			this.Scintilla.EndUndoAction();

			this.ReplaceAllResults?.Invoke(this, new ResultsEventArgs(new FindResults(results)));

			return results;
		}
		
		public List<CharacterRange> ReplaceAll(CharacterRange rangeToSearch, Regex findExpression, string replaceExpression, bool mark, bool highlight)
		{
			var results = new List<CharacterRange>();

			this.Scintilla.IndicatorCurrent = this.Indicator.Index;

			var lastLine = -1;

			this.Scintilla.BeginUndoAction();

			while (true)
			{
				var r = this.Find(rangeToSearch, findExpression);
				if (r.MinPosition == r.MaxPosition)
				{
					break;
				}

				var findString = this.Scintilla.GetTextRange(r.MinPosition, r.MaxPosition - r.MinPosition);
				var replaceString = findExpression.Replace(findString, replaceExpression);
				this.Scintilla.SelectionStart = r.MinPosition;
				this.Scintilla.SelectionEnd = r.MaxPosition;
				this.Scintilla.ReplaceSelection(replaceString);
				r = new CharacterRange(r.MinPosition, r.MinPosition + replaceString.Length);
				rangeToSearch = new CharacterRange(r.MaxPosition, rangeToSearch.MaxPosition + replaceString.Length - findString.Length);

				results.Add(r);

				if (mark)
				{
					//	We can of course have multiple instances of a find on a single
					//	line. We don't want to mark this line more than once.
					var line = new Line(this.Scintilla, this.Scintilla.LineFromPosition(r.MinPosition));
					if (line.Position > lastLine)
						line.MarkerAdd(this.Marker.Index);
					lastLine = line.Position;
				}
				if (highlight)
				{
					this.Scintilla.IndicatorFillRange(r.MinPosition, r.MaxPosition - r.MinPosition);
				}
				rangeToSearch = new CharacterRange(r.MaxPosition, rangeToSearch.MaxPosition);
			}

			this.Scintilla.EndUndoAction();

			this.ReplaceAllResults?.Invoke(this, new ResultsEventArgs(new FindResults(results)));

			return results;
		}
		
		public List<CharacterRange> ReplaceAll(int startPos, int endPos, Regex findExpression, string replaceExpression, bool mark, bool highlight)
		{
			return this.ReplaceAll(new CharacterRange(startPos, endPos), findExpression, replaceExpression, mark, highlight);
		}

		public List<CharacterRange> ReplaceAll(CharacterRange rangeToSearch, string searchString, string replaceString, SearchFlags flags, bool mark, bool highlight)
		{
			return this.ReplaceAll(rangeToSearch.MinPosition, rangeToSearch.MaxPosition, searchString, replaceString, flags, mark, highlight);
		}

		public List<CharacterRange> ReplaceAll(Regex findExpression, string replaceExpression, bool mark, bool highlight)
		{
			return this.ReplaceAll(0, this.Scintilla.TextLength, findExpression, replaceExpression, mark, highlight);
		}

		public List<CharacterRange> ReplaceAll(string searchString, string replaceString, bool mark, bool highlight)
		{
			return this.ReplaceAll(searchString, replaceString, Flags, mark, highlight);
		}

		public List<CharacterRange> ReplaceAll(string searchString, string replaceString)
		{
			return this.ReplaceAll(searchString, replaceString, Flags, false, false);
		}

		public List<CharacterRange> ReplaceAll(string searchString, string replaceString, SearchFlags flags, bool mark, bool highlight)
		{
			return this.ReplaceAll(0, this.Scintilla.TextLength, searchString, replaceString, flags, mark, highlight);
		}

		[SuppressMessage("Globalization", "CA1307:Specify StringComparison", Justification = "Suggested fix does not work for .NET Framework 4.6.1")]
		public static string Transform(string data)
		{
			var result = data;
			var nullChar = (char)0;
			var cr = (char)13;
			var lf = (char)10;
			var tab = (char)9;

			result = result.Replace("\\r\\n", Environment.NewLine);
			result = result.Replace("\\r", $"{cr}");
			result = result.Replace("\\n", $"{lf}");
			result = result.Replace("\\t", $"{tab}");
			result = result.Replace("\\0", $"{nullChar}");

			return result;
		}
    }
}
