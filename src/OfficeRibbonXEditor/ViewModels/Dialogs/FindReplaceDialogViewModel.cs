using System;
using System.Drawing;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.Input;
using OfficeRibbonXEditor.Events;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Resources;
using OfficeRibbonXEditor.Views.Controls.Forms;
using ScintillaNET;
using CharacterRange = OfficeRibbonXEditor.Helpers.CharacterRange;

#pragma warning disable 8618 // Uninitialized non-nullable fields. They are all set in OnLoaded instead

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{
    // Using a singleton for this one ensures that the search criteria is preserved, which is especially
    // important for find next / previous commands
    [Export(Lifetime = Lifetime.Singleton)]
    public partial class FindReplaceDialogViewModel : DialogBase, IContentDialog<ValueTuple<Scintilla, FindReplaceAction, FindReplace.FindAllResultsEventHandler>>
    {
        public event EventHandler<PointEventArgs>? MoveDialogAway;

        public RecentListViewModel<string> RecentFinds { get; } = new RecentListViewModel<string>();

        public RecentListViewModel<string> RecentReplaces { get; } = new RecentListViewModel<string>();

        /// <summary>
        /// Gets the internal <see cref="OfficeRibbonXEditor.Helpers.FindReplace"/> instance that performs the actual find / replace
        /// operations. Only available after <see cref="Scintilla"/> has been set to a non-null value, which is
        /// usually done when calling <see cref="OnLoaded"/>.
        /// </summary>
        public FindReplace FindReplace { get; private set; }

        public IncrementalSearcher IncrementalSearcher { get; private set; }

        private Scintilla _scintilla;

        public Scintilla Scintilla
        {
            get => _scintilla;
            set
            {
                var previous = _scintilla;
                if (!SetProperty(ref _scintilla, value))
                {
                    return;
                }

                if (previous != null && IncrementalSearcher != null)
                {
                    previous.Controls.Remove(IncrementalSearcher);
                }

                FindReplace = new FindReplace(_scintilla);
                FindReplace.FindAllResults += FindAllHandler;

                IncrementalSearcher = new IncrementalSearcher
                {
                    Scintilla = Scintilla,
                    FindReplace = this,
                    Visible = false
                };
                Scintilla.Controls.Add(IncrementalSearcher);
            }
        }

        private FindReplace.FindAllResultsEventHandler FindAllHandler { get; set; }

        private bool _autoPosition = true;
        public bool AutoPosition
        {
            get => _autoPosition;
            set => SetProperty(ref _autoPosition, value);
        }

        private bool _isFindTabSelected = true;
        public bool IsFindTabSelected
        {
            get => _isFindTabSelected;
            set => SetProperty(ref _isFindTabSelected, value);
        }

        private bool _isStandardSearch = true;
        public bool IsStandardSearch
        {
            get => _isStandardSearch;
            set => SetProperty(ref _isStandardSearch, value);
        }

        private bool _isExtendedSearch;
        public bool IsExtendedSearch
        {
            get => _isExtendedSearch;
            set => SetProperty(ref _isExtendedSearch, value);
        }

        private bool _isRegExSearch;
        public bool IsRegExSearch
        {
            get => _isRegExSearch;
            set => SetProperty(ref _isRegExSearch, value);
        }

        private bool _ignoreCase;
        public bool IgnoreCase
        {
            get => _ignoreCase;
            set => SetProperty(ref _ignoreCase, value);
        }

        private bool _wholeWord;
        public bool WholeWord
        {
            get => _wholeWord;
            set => SetProperty(ref _wholeWord, value);
        }

        private bool _wordStart;
        public bool WordStart
        {
            get => _wordStart;
            set => SetProperty(ref _wordStart, value);
        }

        private bool _isCompiled;
        public bool IsCompiled
        {
            get => _isCompiled;
            set => SetProperty(ref _isCompiled, value);
        }

        private bool _isCultureInvariant;
        public bool IsCultureInvariant
        {
            get => _isCultureInvariant;
            set => SetProperty(ref _isCultureInvariant, value);
        }

        private bool _isEcmaScript;
        public bool IsEcmaScript
        {
            get => _isEcmaScript;
            set => SetProperty(ref _isEcmaScript, value);
        }

        private bool _isExplicitCapture;
        public bool IsExplicitCapture
        {
            get => _isExplicitCapture;
            set => SetProperty(ref _isExplicitCapture, value);
        }

        private bool _ignorePatternWhitespace;
        public bool IgnorePatternWhitespace
        {
            get => _ignorePatternWhitespace;
            set => SetProperty(ref _ignorePatternWhitespace, value);
        }

        private bool _isMultiline;
        public bool IsMultiline
        {
            get => _isMultiline;
            set => SetProperty(ref _isMultiline, value);
        }

        private bool _isRightToLeft;
        public bool IsRightToLeft
        {
            get => _isRightToLeft;
            set => SetProperty(ref _isRightToLeft, value);
        }

        private bool _isSingleLine;
        public bool IsSingleLine
        {
            get => _isSingleLine;
            set => SetProperty(ref _isSingleLine, value);
        }

        private bool _wrap;
        public bool Wrap
        {
            get => _wrap;
            set => SetProperty(ref _wrap, value);
        }

        private bool _searchSelection;
        public bool SearchSelection
        {
            get => _searchSelection;
            set => SetProperty(ref _searchSelection, value);
        }

        private bool _markLine;
        public bool MarkLine
        {
            get => _markLine;
            set => SetProperty(ref _markLine, value);
        }

        private bool _highlightMatches;
        public bool HighlightMatches
        {
            get => _highlightMatches;
            set => SetProperty(ref _highlightMatches, value);
        }

        private string _findText = string.Empty;
        public string FindText
        {
            get => _findText;
            set => SetProperty(ref _findText, value);
        }

        private string _replaceText = string.Empty;
        public string ReplaceText
        {
            get => _replaceText;
            set => SetProperty(ref _replaceText, value);
        }

        private string _statusText = string.Empty;
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        [RelayCommand]
        private void FindAll()
        {
            if (string.IsNullOrEmpty(FindText))
            {
                return;
            }

            StatusText = string.Empty;

            Clear();
            int foundCount;

            if (IsRegExSearch)
            {
                Regex rr;
                try
                {
                    rr = new Regex(FindText, GetRegexOptions());
                }
                catch (ArgumentException ex)
                {
                    StatusText = $"{Strings.FindReplace_Status_RegExError}: {ex.Message}";
                    return;
                }

                if (SearchSelection)
                {
                    var searchRange = new CharacterRange(_scintilla.Selections[0].Start, _scintilla.Selections[0].End);
                    foundCount = FindReplace.FindAll(searchRange, rr, MarkLine, HighlightMatches).Count;
                }
                else
                {
                    foundCount = FindReplace.FindAll(rr, MarkLine, HighlightMatches).Count;
                }
            }
            else
            {
                var textToFind = IsExtendedSearch ? FindReplace.Transform(FindText) : FindText;
                if (SearchSelection)
                {
                    var searchRange = new CharacterRange(_scintilla.Selections[0].Start, _scintilla.Selections[0].End);
                    foundCount = FindReplace.FindAll(searchRange, textToFind, GetSearchFlags(), MarkLine, HighlightMatches).Count;
                }
                else
                {
                    foundCount = FindReplace.FindAll(textToFind, GetSearchFlags(), MarkLine, HighlightMatches).Count;
                }
            }

            StatusText = $"{Strings.FindReplace_Status_TotalFound}: {foundCount}";
            AddRecentFind();
        }

        [RelayCommand]
        private void ReplaceAll()
        {
            if (string.IsNullOrEmpty(FindText))
            {
                return;
            }

            StatusText = string.Empty;

            Clear();
            int foundCount;

            var textToReplace = IsExtendedSearch ? FindReplace.Transform(ReplaceText) : ReplaceText;

            if (IsRegExSearch)
            {
                Regex rr;
                try
                {
                    rr = new Regex(FindText, GetRegexOptions());
                }
                catch (ArgumentException ex)
                {
                    StatusText = $"{Strings.FindReplace_Status_RegExError}: {ex.Message}";
                    return;
                }

                if (SearchSelection)
                {
                    var searchRange = new CharacterRange(_scintilla.Selections[0].Start, _scintilla.Selections[0].End);
                    foundCount = FindReplace.ReplaceAll(searchRange, rr, textToReplace, MarkLine, HighlightMatches).Count;
                }
                else
                {
                    foundCount = FindReplace.ReplaceAll(rr, textToReplace, MarkLine, HighlightMatches).Count;
                }
            }
            else
            {
                var textToFind = IsExtendedSearch ? FindReplace.Transform(FindText) : FindText;
                if (SearchSelection)
                {
                    var searchRange = new CharacterRange(_scintilla.Selections[0].Start, _scintilla.Selections[0].End);
                    foundCount = FindReplace.ReplaceAll(searchRange, textToFind, textToReplace, GetSearchFlags(), MarkLine, HighlightMatches).Count;
                }
                else
                {
                    foundCount = FindReplace.ReplaceAll(textToFind, textToReplace, GetSearchFlags(), MarkLine, HighlightMatches).Count;
                }
            }

            StatusText = $"{Strings.FindReplace_Status_TotalReplaced}: {foundCount}";
            AddRecentFind();
            AddRecentReplace();
        }

        [RelayCommand]
        private void Clear()
        {
            Scintilla.MarkerDeleteAll(FindReplace.Marker.Index);
            FindReplace.ClearAllHighlights();
        }

        [RelayCommand]
        private void FindNext() => FindWrapper(false);

        [RelayCommand]
        private void FindPrevious() => FindWrapper(true);

        private void FindWrapper(bool searchUp)
        {
            if (string.IsNullOrEmpty(FindText))
            {
                return;
            }

            StatusText = string.Empty;

            CharacterRange foundRange;

            try
            {
                foundRange = FindNext(searchUp);
            }
            catch (ArgumentException ex)
            {
                StatusText = $"{Strings.FindReplace_Status_RegExError}: {ex.Message}";
                return;
            }

            if (foundRange.MinPosition == foundRange.MaxPosition)
            {
                StatusText = Strings.FindReplace_Status_NoMatch;
            }
            else
            {
                if ((searchUp && foundRange.MinPosition < Scintilla.AnchorPosition) || (!searchUp && foundRange.MinPosition > Scintilla.CurrentPosition))
                {
                    StatusText = Strings.FindReplace_Status_Wrap;
                }

                // This should ensure the entire text is visible before it is selected
                Scintilla.GotoPosition(foundRange.MaxPosition);
                Scintilla.GotoPosition(foundRange.MinPosition);

                Scintilla.SetSel(foundRange.MinPosition, foundRange.MaxPosition);
                MoveDialogAwayFromSelection();
            }

            AddRecentFind();
        }

        private void AddRecentFind()
        {
            // The way a ComboBox works, there is a chance this resets the FindText. This is due to removing the item from the list
            // just temporarily before putting it at the top
            var text = FindText;
            RecentFinds.Add(text);
            FindText = text;
        }

        private void AddRecentReplace()
        {
            // The way a ComboBox works, there is a chance this resets the ReplaceText. This is due to removing the item from the list
            // just temporarily before putting it at the top
            var text = ReplaceText;
            RecentReplaces.Add(text);
            ReplaceText = text;
        }

        private CharacterRange FindNext(bool searchUp)
        {
            Regex? rr = null;
            return FindNext(searchUp, ref rr);
        }

        private CharacterRange FindNext(bool searchUp, ref Regex? rr)
        {
            CharacterRange foundRange;

            if (IsRegExSearch)
            {
                rr = new Regex(FindText, GetRegexOptions());

                if (SearchSelection)
                {
                    var searchRange = new CharacterRange(_scintilla.Selections[0].Start, _scintilla.Selections[0].End);
                    if (searchUp)
                        foundRange = FindReplace.FindPrevious(rr, Wrap, searchRange);
                    else
                        foundRange = FindReplace.FindNext(rr, Wrap, searchRange);
                }
                else
                {
                    if (searchUp)
                        foundRange = FindReplace.FindPrevious(rr, Wrap);
                    else
                        foundRange = FindReplace.FindNext(rr, Wrap);
                }
            }
            else
            {
                if (SearchSelection)
                {
                    var searchRange = new CharacterRange(_scintilla.Selections[0].Start, _scintilla.Selections[0].End);
                    if (searchUp)
                    {
                        var textToFind = IsExtendedSearch ? FindReplace.Transform(FindText) : FindText;
                        foundRange = FindReplace.FindPrevious(textToFind, Wrap, GetSearchFlags(), searchRange);
                    }
                    else
                    {
                        var textToFind = IsExtendedSearch ? FindReplace.Transform(FindText) : FindText;
                        foundRange = FindReplace.FindNext(textToFind, Wrap, GetSearchFlags(), searchRange);
                    }
                }
                else
                {
                    if (searchUp)
                    {
                        var textToFind = IsExtendedSearch ? FindReplace.Transform(FindText) : FindText;
                        foundRange = FindReplace.FindPrevious(textToFind, Wrap, GetSearchFlags());
                    }
                    else
                    {
                        var textToFind = IsExtendedSearch ? FindReplace.Transform(FindText) : FindText;
                        foundRange = FindReplace.FindNext(textToFind, Wrap, GetSearchFlags());
                    }
                }
            }

            return foundRange;
        }

        [RelayCommand]
        private void ReplaceNext() => ReplaceWrapper(false);

        [RelayCommand]
        private void ReplacePrevious() => ReplaceWrapper(true);

        private void ReplaceWrapper(bool searchUp)
        {
            if (string.IsNullOrEmpty(FindText))
            {
                return;
            }

            StatusText = string.Empty;

            CharacterRange nextRange;
            try
            {
                nextRange = ReplaceNext(searchUp);
            }
            catch (ArgumentException ex)
            {
                StatusText = $"{Strings.FindReplace_Status_RegExError}: {ex.Message}";
                return;
            }

            if (nextRange.MinPosition == nextRange.MaxPosition)
            {
                StatusText = Strings.FindReplace_Status_NoMatch;
            }
            else
            {
                if (nextRange.MinPosition < Scintilla.AnchorPosition)
                {
                    StatusText = Strings.FindReplace_Status_Wrap;
                }
                
                // This should ensure the entire text is visible before it is selected
                Scintilla.GotoPosition(nextRange.MaxPosition);
                Scintilla.GotoPosition(nextRange.MinPosition);

                Scintilla.SetSel(nextRange.MinPosition, nextRange.MaxPosition);

                MoveDialogAwayFromSelection();
            }

            AddRecentFind();
            AddRecentReplace();
        }

        private CharacterRange ReplaceNext(bool searchUp)
        {
            Regex? rr = null;
            var selRange = new CharacterRange(_scintilla.Selections[0].Start, _scintilla.Selections[0].End);

            //	We only do the actual replacement if the current selection exactly
            //	matches the find.
            if (selRange.MaxPosition - selRange.MinPosition > 0)
            {
                if (IsRegExSearch)
                {
                    rr = new Regex(FindText, GetRegexOptions());
                    var selRangeText = Scintilla.GetTextRange(selRange.MinPosition, selRange.MaxPosition - selRange.MinPosition);

                    if (selRange.Equals(FindReplace.Find(selRange, rr, false)))
                    {
                        //	If searching up we do the replacement using the range object.
                        //	Otherwise we use the selection object. The reason being if
                        //	we use the range the caret is positioned before the replaced
                        //	text. Conversely if we use the selection object the caret will
                        //	be positioned after the replaced text. This is very important
                        //	because we don't want the new text to be potentially matched
                        //	in the next search.
                        if (searchUp)
                        {
                            _scintilla.SelectionStart = selRange.MinPosition;
                            _scintilla.SelectionEnd = selRange.MaxPosition;
                            _scintilla.ReplaceSelection(rr.Replace(selRangeText, ReplaceText));
                            _scintilla.GotoPosition(selRange.MinPosition);
                        }
                        else
                            Scintilla.ReplaceSelection(rr.Replace(selRangeText, ReplaceText));
                    }
                }
                else
                {
                    var textToFind = IsExtendedSearch ? FindReplace.Transform(FindText) : FindText;
                    if (selRange.Equals(FindReplace.Find(selRange, textToFind, false)))
                    {
                        //	If searching up we do the replacement using the range object.
                        //	Otherwise we use the selection object. The reason being if
                        //	we use the range the caret is positioned before the replaced
                        //	text. Conversely if we use the selection object the caret will
                        //	be positioned after the replaced text. This is very important
                        //	because we don't want the new text to be potentially matched
                        //	in the next search.
                        if (searchUp)
                        {
                            var textToReplace = IsExtendedSearch ? FindReplace.Transform(ReplaceText) : ReplaceText;
                            _scintilla.SelectionStart = selRange.MinPosition;
                            _scintilla.SelectionEnd = selRange.MaxPosition;
                            _scintilla.ReplaceSelection(textToReplace);

                            _scintilla.GotoPosition(selRange.MinPosition);
                        }
                        else
                        {
                            var textToReplace = IsExtendedSearch ? FindReplace.Transform(ReplaceText) : ReplaceText;
                            Scintilla.ReplaceSelection(textToReplace);
                        }
                    }
                }
            }

            AddRecentFind();
            AddRecentReplace();

            return FindNext(searchUp, ref rr);
        }

        public RegexOptions GetRegexOptions()
        {
            var ro = RegexOptions.None;

            if (IsCompiled)
            {
                ro |= RegexOptions.Compiled;
            }

            if (IsCultureInvariant)
            {
                ro |= RegexOptions.CultureInvariant;
            }

            if (IsEcmaScript)
            {
                ro |= RegexOptions.ECMAScript;
            }

            if (IsExplicitCapture)
            {
                ro |= RegexOptions.ExplicitCapture;
            }

            if (IgnoreCase)
            {
                ro |= RegexOptions.IgnoreCase;
            }

            if (IgnorePatternWhitespace)
            {
                ro |= RegexOptions.IgnorePatternWhitespace;
            }

            if (IsMultiline)
            {
                ro |= RegexOptions.Multiline;
            }

            if (IsRightToLeft)
            {
                ro |= RegexOptions.RightToLeft;
            }

            if (IsSingleLine)
            {
                ro |= RegexOptions.Singleline;
            }

            return ro;
        }

        public SearchFlags GetSearchFlags()
        {
            var sf = SearchFlags.None;

            if (!IgnoreCase)
            {
                sf |= SearchFlags.MatchCase;
            }

            if (WholeWord)
            {
                sf |= SearchFlags.WholeWord;
            }

            if (WordStart)
            {
                sf |= SearchFlags.WordStart;
            }

            return sf;
        }

        public bool OnLoaded((Scintilla, FindReplaceAction, FindReplace.FindAllResultsEventHandler) payload)
        {
            var (editor, action, handler) = payload;
            FindAllHandler = handler;
            Scintilla = editor;

            switch (action)
            {
                case FindReplaceAction.Find:
                    IsFindTabSelected = true;
                    break;
                case FindReplaceAction.Replace:
                    IsFindTabSelected = false;
                    break;
                case FindReplaceAction.FindNext:
                    FindWrapper(false);
                    return false;
                case FindReplaceAction.FindPrevious:
                    FindWrapper(true);
                    return false;
                case FindReplaceAction.IncrementalSearch:
                    IncrementalSearcher?.Show();
                    return false;
                default:
                    throw new NotImplementedException($"Unhandled loading action: {action}");
            }

            return true;
        }

        public virtual void MoveDialogAwayFromSelection()
        {
            if (!AutoPosition)
            {
                return;
            }

            var pos = Scintilla.CurrentPosition;
            var x = Scintilla.PointXFromPosition(pos);
            var y = Scintilla.PointYFromPosition(pos);

            var cursorPoint = Scintilla.PointToScreen(new Point(x, y));
            MoveDialogAway?.Invoke(this, new PointEventArgs(cursorPoint));
        }
    }
}
