using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text.RegularExpressions;
using OfficeRibbonXEditor.Events;
using OfficeRibbonXEditor.Interfaces;
using ScintillaNET;

namespace OfficeRibbonXEditor.Helpers;

using ResultsEventArgs = DataEventArgs<IResultCollection>;

public class FindReplace
{
	private const SearchFlags Flags = SearchFlags.None;

	public Scintilla Scintilla { get; }

	public FindReplace(Scintilla scintilla)
	{
		Scintilla = scintilla;

		Marker = Scintilla.Markers[10];
		Marker.Symbol = MarkerSymbol.Circle;
		Marker.SetForeColor(Color.Black);
		Marker.SetBackColor(Color.Blue);
		Indicator = Scintilla.Indicators[16];
		Indicator.ForeColor = Color.Red;
		//_indicator.ForeColor = Color.LawnGreen; //Smart highlight
		Indicator.Alpha = 100;
		Indicator.Style = IndicatorStyle.RoundBox;
		Indicator.Under = true;
	}

	public Indicator Indicator { get; set; }

	public Marker Marker { get; set; }
		
	public event FindAllResultsEventHandler? FindAllResults;

	public event ReplaceAllResultsEventHandler? ReplaceAllResults;

	public delegate void FindAllResultsEventHandler(object? sender, ResultsEventArgs findAllResults);

	public delegate void ReplaceAllResultsEventHandler(object? sender, ResultsEventArgs replaceAllResults);

	public void ClearAllHighlights()
	{
		var currentIndicator = Scintilla.IndicatorCurrent;

		Scintilla.IndicatorCurrent = Indicator.Index;
		Scintilla.IndicatorClearRange(0, Scintilla.TextLength);

		Scintilla.IndicatorCurrent = currentIndicator;
	}

	public CharacterRange Find(int startPos, int endPos, Regex findExpression)
	{
		return Find(new CharacterRange(startPos, endPos), findExpression, false);
	}

	public CharacterRange Find(int startPos, int endPos, Regex findExpression, bool searchUp)
	{
		return Find(new CharacterRange(startPos, endPos), findExpression, searchUp);
	}

	public CharacterRange Find(int startPos, int endPos, string searchString, SearchFlags flags)
	{
		if (string.IsNullOrEmpty(searchString))
			return new CharacterRange();

		Scintilla.TargetStart = startPos;
		Scintilla.TargetEnd = endPos;
		Scintilla.SearchFlags = flags;
		var pos = Scintilla.SearchInTarget(searchString);
		return pos == -1 ? new CharacterRange() : new CharacterRange(Scintilla.TargetStart, Scintilla.TargetEnd);
	}

	public CharacterRange Find(CharacterRange r, Regex findExpression)
	{
		return Find(r, findExpression, false);
	}

	public CharacterRange Find(CharacterRange r, Regex findExpression, bool searchUp)
	{
		// Single line and Multi Line in RegExp doesn't really effect
		// whether or not a match will include newline characters. This
		// means we can't do a line by line search. We have to search
		// the entire range because it could potentially match the
		// entire range.

		var text = Scintilla.GetTextRange(r.MinPosition, r.MaxPosition - r.MinPosition);

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
				var start = r.MinPosition + text[..m.Index].Length;
				var end = text.Substring(m.Index, m.Length).Length;

				range = new CharacterRange(start, start + end);
				m = m.NextMatch();
			}

			return range;
		}
		else
		{
			var start = r.MinPosition + text[..m.Index].Length;
			var end = text.Substring(m.Index, m.Length).Length;

			return new CharacterRange(start, start + end);
		}
	}

	public CharacterRange Find(CharacterRange rangeToSearch, string searchString)
	{
		return Find(rangeToSearch.MinPosition, rangeToSearch.MaxPosition, searchString, Flags);
	}

	public CharacterRange Find(CharacterRange rangeToSearch, string searchString, bool searchUp)
	{
		if (searchUp)
			return Find(rangeToSearch.MaxPosition, rangeToSearch.MinPosition, searchString, Flags);
		else
			return Find(rangeToSearch.MinPosition, rangeToSearch.MaxPosition, searchString, Flags);
	}

	public CharacterRange Find(CharacterRange rangeToSearch, string searchString, SearchFlags searchflags)
	{
		return Find(rangeToSearch.MinPosition, rangeToSearch.MaxPosition, searchString, searchflags);
	}

	public CharacterRange Find(CharacterRange rangeToSearch, string searchString, SearchFlags searchflags, bool searchUp)
	{
		if (searchUp)
			return Find(rangeToSearch.MaxPosition, rangeToSearch.MinPosition, searchString, searchflags);
		else
			return Find(rangeToSearch.MinPosition, rangeToSearch.MaxPosition, searchString, searchflags);
	}

	public CharacterRange Find(Regex findExpression)
	{
		return Find(new CharacterRange(0, Scintilla.TextLength), findExpression, false);
	}

	public CharacterRange Find(Regex findExpression, bool searchUp)
	{
		return Find(new CharacterRange(0, Scintilla.TextLength), findExpression, searchUp);
	}

	public CharacterRange Find(string searchString)
	{
		return Find(0, Scintilla.TextLength, searchString, Flags);
	}

	public CharacterRange Find(string searchString, bool searchUp)
	{
		if (searchUp)
			return Find(Scintilla.TextLength, 0, searchString, Flags);
		else
			return Find(0, Scintilla.TextLength, searchString, Flags);
	}

	public CharacterRange Find(string searchString, SearchFlags searchFlags)
	{
		return Find(0, Scintilla.TextLength, searchString, searchFlags);
	}

	public CharacterRange Find(string searchString, SearchFlags searchFlags, bool searchUp)
	{
		if (searchUp)
			return Find(Scintilla.TextLength, 0, searchString, searchFlags);
		else
			return Find(0, Scintilla.TextLength, searchString, searchFlags);
	}

	public CharacterRange FindNext(Regex findExpression)
	{
		return FindNext(findExpression, false);
	}

	public CharacterRange FindNext(Regex findExpression, bool wrap)
	{
		var r = Find(Scintilla.CurrentPosition, Scintilla.TextLength, findExpression);
		if (r.MinPosition != r.MaxPosition)
			return r;
		else if (wrap)
			return Find(0, Scintilla.CurrentPosition, findExpression);
		else
			return new CharacterRange();
	}

	public CharacterRange FindNext(Regex findExpression, bool wrap, CharacterRange searchRange)
	{
		var caret = Scintilla.CurrentPosition;
		if (!(caret >= searchRange.MinPosition && caret <= searchRange.MaxPosition))
			return Find(searchRange.MinPosition, searchRange.MaxPosition, findExpression, false);

		var r = Find(caret, searchRange.MaxPosition, findExpression);
		if (r.MinPosition != r.MaxPosition)
			return r;
		else if (wrap)
			return Find(searchRange.MinPosition, caret, findExpression);
		else
			return new CharacterRange();
	}

	public CharacterRange FindNext(string searchString)
	{
		return FindNext(searchString, true, Flags);
	}

	public CharacterRange FindNext(string searchString, bool wrap)
	{
		return FindNext(searchString, wrap, Flags);
	}

	public CharacterRange FindNext(string searchString, bool wrap, SearchFlags flags)
	{
		var r = Find(Scintilla.CurrentPosition, Scintilla.TextLength, searchString, flags);
		if (r.MinPosition != r.MaxPosition)
			return r;
		else if (wrap)
			return Find(0, Scintilla.CurrentPosition, searchString, flags);
		else
			return new CharacterRange();
	}

	public CharacterRange FindNext(string searchString, bool wrap, SearchFlags flags, CharacterRange searchRange)
	{
		var caret = Scintilla.CurrentPosition;
		if (!(caret >= searchRange.MinPosition && caret <= searchRange.MaxPosition))
			return Find(searchRange.MinPosition, searchRange.MaxPosition, searchString, flags);

		var r = Find(caret, searchRange.MaxPosition, searchString, flags);
		if (r.MinPosition != r.MaxPosition)
			return r;
		else if (wrap)
			return Find(searchRange.MinPosition, caret, searchString, flags);
		else
			return new CharacterRange();
	}

	public CharacterRange FindNext(string searchString, SearchFlags flags)
	{
		return FindNext(searchString, true, flags);
	}

	public CharacterRange FindPrevious(Regex findExpression)
	{
		return FindPrevious(findExpression, false);
	}

	public CharacterRange FindPrevious(Regex findExpression, bool wrap)
	{
		var r = Find(0, Scintilla.AnchorPosition, findExpression, true);
		if (r.MinPosition != r.MaxPosition)
			return r;
		else if (wrap)
			return Find(Scintilla.CurrentPosition, Scintilla.TextLength, findExpression, true);
		else
			return new CharacterRange();
	}

	public CharacterRange FindPrevious(Regex findExpression, bool wrap, CharacterRange searchRange)
	{
		var caret = Scintilla.CurrentPosition;
		if (!(caret >= searchRange.MinPosition && caret <= searchRange.MaxPosition))
			return Find(searchRange.MinPosition, searchRange.MaxPosition, findExpression, true);

		var anchor = Scintilla.AnchorPosition;
		if (!(anchor >= searchRange.MinPosition && anchor <= searchRange.MaxPosition))
			anchor = caret;

		var r = Find(searchRange.MinPosition, anchor, findExpression, true);
		if (r.MinPosition != r.MaxPosition)
			return r;
		else if (wrap)
			return Find(anchor, searchRange.MaxPosition, findExpression, true);
		else
			return new CharacterRange();
	}

	public CharacterRange FindPrevious(string searchString)
	{
		return FindPrevious(searchString, true, Flags);
	}

	public CharacterRange FindPrevious(string searchString, bool wrap)
	{
		return FindPrevious(searchString, wrap, Flags);
	}

	public CharacterRange FindPrevious(string searchString, bool wrap, SearchFlags flags)
	{
		var r = Find(Scintilla.AnchorPosition, 0, searchString, flags);
		if (r.MinPosition != r.MaxPosition)
			return r;
		else if (wrap)
			return Find(Scintilla.TextLength, Scintilla.CurrentPosition, searchString, flags);
		else
			return new CharacterRange();
	}

	public CharacterRange FindPrevious(string searchString, bool wrap, SearchFlags flags, CharacterRange searchRange)
	{
		var caret = Scintilla.CurrentPosition;
		if (!(caret >= searchRange.MinPosition && caret <= searchRange.MaxPosition))
			return Find(searchRange.MaxPosition, searchRange.MinPosition, searchString, flags);

		var anchor = Scintilla.AnchorPosition;
		if (!(anchor >= searchRange.MinPosition && anchor <= searchRange.MaxPosition))
			anchor = caret;

		var r = Find(anchor, searchRange.MinPosition, searchString, flags);
		if (r.MinPosition != r.MaxPosition)
			return r;
		else if (wrap)
			return Find(searchRange.MaxPosition, anchor, searchString, flags);
		else
			return new CharacterRange();
	}

	public CharacterRange FindPrevious(string searchString, SearchFlags flags)
	{
		return FindPrevious(searchString, true, flags);
	}

	public List<CharacterRange> FindAll(int startPos, int endPos, string searchString, SearchFlags flags, bool mark, bool highlight)
	{
		var results = new List<CharacterRange>();

		Scintilla.IndicatorCurrent = Indicator.Index;

		var findCount = 0;
		var lastLine = -1;
		while (true)
		{
			var r = Find(startPos, endPos, searchString, flags);
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
				var line = new Line(Scintilla, Scintilla.LineFromPosition(r.MinPosition));
				if (line.Position > lastLine)
					line.MarkerAdd(Marker.Index);
				lastLine = line.Position;
			}
			if (highlight)
			{
				Scintilla.IndicatorFillRange(r.MinPosition, r.MaxPosition - r.MinPosition);
			}
			startPos = r.MaxPosition;
		}

		FindAllResults?.Invoke(this, new ResultsEventArgs(new FindResults(results)));

		return results;
	}

	public List<CharacterRange> FindAll(CharacterRange rangeToSearch, Regex findExpression, bool mark, bool highlight)
	{
		var results = new List<CharacterRange>();

		Scintilla.IndicatorCurrent = Indicator.Index;

		var findCount = 0;
		var lastLine = -1;

		while (true)
		{
			var r = Find(rangeToSearch, findExpression);
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
				var line = new Line(Scintilla, Scintilla.LineFromPosition(r.MinPosition));
				if (line.Position > lastLine)
					line.MarkerAdd(Marker.Index);
				lastLine = line.Position;
			}
			if (highlight)
			{
				Scintilla.IndicatorFillRange(r.MinPosition, r.MaxPosition - r.MinPosition);
			}
			rangeToSearch = new CharacterRange(r.MaxPosition, rangeToSearch.MaxPosition);
		}

		FindAllResults?.Invoke(this, new ResultsEventArgs(new FindResults(results)));

		return results;
	}

	public List<CharacterRange> FindAll(int startPos, int endPos, Regex findExpression, bool mark, bool highlight)
	{
		return FindAll(new CharacterRange(startPos, endPos), findExpression, mark, highlight);
	}

	public List<CharacterRange> FindAll(CharacterRange rangeToSearch, string searchString, SearchFlags flags, bool mark, bool highlight)
	{
		return FindAll(rangeToSearch.MinPosition, rangeToSearch.MaxPosition, searchString, flags, mark, highlight);
	}

	public List<CharacterRange> FindAll(Regex findExpression, bool mark, bool highlight)
	{
		return FindAll(0, Scintilla.TextLength, findExpression, mark, highlight);
	}

	public List<CharacterRange> FindAll(string searchString, bool mark, bool highlight)
	{
		return FindAll(searchString, Flags, mark, highlight);
	}

	public List<CharacterRange> FindAll(string searchString)
	{
		return FindAll(searchString, Flags, false, false);
	}

	public List<CharacterRange> FindAll(string searchString, SearchFlags flags, bool mark, bool highlight)
	{
		return FindAll(0, Scintilla.TextLength, searchString, flags, mark, highlight);
	}

	public List<CharacterRange> ReplaceAll(int startPos, int endPos, string searchString, string replaceString, SearchFlags flags, bool mark, bool highlight)
	{
		var results = new List<CharacterRange>();

		Scintilla.IndicatorCurrent = Indicator.Index;

		var findCount = 0;
		var lastLine = -1;

		Scintilla.BeginUndoAction();

		var diff = replaceString.Length - searchString.Length;
		while (true)
		{
			var r = Find(startPos, endPos, searchString, flags);
			if (r.MinPosition == r.MaxPosition)
			{
				break;
			}

			Scintilla.SelectionStart = r.MinPosition;
			Scintilla.SelectionEnd = r.MaxPosition;
			Scintilla.ReplaceSelection(replaceString);
			startPos = r.MinPosition + replaceString.Length;
			r = new CharacterRange(r.MinPosition, startPos);
			endPos += diff;

			results.Add(r);
			findCount++;

			if (mark)
			{
				//	We can of course have multiple instances of a find on a single
				//	line. We don't want to mark this line more than once.
				var line = new Line(Scintilla, Scintilla.LineFromPosition(r.MinPosition));
				if (line.Position > lastLine)
					line.MarkerAdd(Marker.Index);
				lastLine = line.Position;
			}
			if (highlight)
			{
				Scintilla.IndicatorFillRange(r.MinPosition, r.MaxPosition - r.MinPosition);
			}
		}

		Scintilla.EndUndoAction();

		ReplaceAllResults?.Invoke(this, new ResultsEventArgs(new FindResults(results)));

		return results;
	}
		
	public List<CharacterRange> ReplaceAll(CharacterRange rangeToSearch, Regex findExpression, string replaceExpression, bool mark, bool highlight)
	{
		var results = new List<CharacterRange>();

		Scintilla.IndicatorCurrent = Indicator.Index;

		var lastLine = -1;

		Scintilla.BeginUndoAction();

		while (true)
		{
			var r = Find(rangeToSearch, findExpression);
			if (r.MinPosition == r.MaxPosition)
			{
				break;
			}

			var findString = Scintilla.GetTextRange(r.MinPosition, r.MaxPosition - r.MinPosition);
			var replaceString = findExpression.Replace(findString, replaceExpression);
			Scintilla.SelectionStart = r.MinPosition;
			Scintilla.SelectionEnd = r.MaxPosition;
			Scintilla.ReplaceSelection(replaceString);
			r = new CharacterRange(r.MinPosition, r.MinPosition + replaceString.Length);
			rangeToSearch = new CharacterRange(r.MaxPosition, rangeToSearch.MaxPosition + replaceString.Length - findString.Length);

			results.Add(r);

			if (mark)
			{
				//	We can of course have multiple instances of a find on a single
				//	line. We don't want to mark this line more than once.
				var line = new Line(Scintilla, Scintilla.LineFromPosition(r.MinPosition));
				if (line.Position > lastLine)
					line.MarkerAdd(Marker.Index);
				lastLine = line.Position;
			}
			if (highlight)
			{
				Scintilla.IndicatorFillRange(r.MinPosition, r.MaxPosition - r.MinPosition);
			}
			rangeToSearch = new CharacterRange(r.MaxPosition, rangeToSearch.MaxPosition);
		}

		Scintilla.EndUndoAction();

		ReplaceAllResults?.Invoke(this, new ResultsEventArgs(new FindResults(results)));

		return results;
	}
		
	public List<CharacterRange> ReplaceAll(int startPos, int endPos, Regex findExpression, string replaceExpression, bool mark, bool highlight)
	{
		return ReplaceAll(new CharacterRange(startPos, endPos), findExpression, replaceExpression, mark, highlight);
	}

	public List<CharacterRange> ReplaceAll(CharacterRange rangeToSearch, string searchString, string replaceString, SearchFlags flags, bool mark, bool highlight)
	{
		return ReplaceAll(rangeToSearch.MinPosition, rangeToSearch.MaxPosition, searchString, replaceString, flags, mark, highlight);
	}

	public List<CharacterRange> ReplaceAll(Regex findExpression, string replaceExpression, bool mark, bool highlight)
	{
		return ReplaceAll(0, Scintilla.TextLength, findExpression, replaceExpression, mark, highlight);
	}

	public List<CharacterRange> ReplaceAll(string searchString, string replaceString, bool mark, bool highlight)
	{
		return ReplaceAll(searchString, replaceString, Flags, mark, highlight);
	}

	public List<CharacterRange> ReplaceAll(string searchString, string replaceString)
	{
		return ReplaceAll(searchString, replaceString, Flags, false, false);
	}

	public List<CharacterRange> ReplaceAll(string searchString, string replaceString, SearchFlags flags, bool mark, bool highlight)
	{
		return ReplaceAll(0, Scintilla.TextLength, searchString, replaceString, flags, mark, highlight);
	}

	[SuppressMessage("Globalization", "CA1307:Specify StringComparison", Justification = "Suggested fix does not work for .NET Framework 4.6.1")]
	public static string Transform(string data)
	{
		var result = data;
		const char nullChar = (char)0;
		const char cr = (char)13;
		const char lf = (char)10;
		const char tab = (char)9;

		result = result.Replace("\\r\\n", Environment.NewLine);
		result = result.Replace("\\r", $"{cr}");
		result = result.Replace("\\n", $"{lf}");
		result = result.Replace("\\t", $"{tab}");
		result = result.Replace("\\0", $"{nullChar}");

		return result;
	}
}