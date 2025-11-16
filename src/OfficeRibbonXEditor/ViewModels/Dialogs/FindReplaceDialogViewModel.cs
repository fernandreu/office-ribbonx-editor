using System.Drawing;
using System.Text.RegularExpressions;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeRibbonXEditor.Events;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Resources;
using OfficeRibbonXEditor.Views.Controls.Forms;
using ScintillaNET;
using CharacterRange = OfficeRibbonXEditor.Helpers.CharacterRange;

#pragma warning disable 8618 // Uninitialized non-nullable fields. They are all set in OnLoaded instead

namespace OfficeRibbonXEditor.ViewModels.Dialogs;

// Using a singleton for this one ensures that the search criteria is preserved, which is especially
// important for find next / previous commands
[Export(Lifetime = Lifetime.Singleton)]
public partial class FindReplaceDialogViewModel : DialogBase, IContentDialog<ValueTuple<Scintilla, FindReplaceAction, FindReplace.FindAllResultsEventHandler>>
{
    public static readonly TimeSpan DefaultRegexTimeout = TimeSpan.FromSeconds(1);
    
    public event EventHandler<PointEventArgs>? MoveDialogAway;

    public RecentListViewModel<string> RecentFinds { get; } = new();

    public RecentListViewModel<string> RecentReplaces { get; } = new();

    /// <summary>
    /// Gets the internal <see cref="OfficeRibbonXEditor.Helpers.FindReplace"/> instance that performs the actual find / replace
    /// operations. Only available after <see cref="Scintilla"/> has been set to a non-null value, which is
    /// usually done when calling <see cref="OnLoaded"/>.
    /// </summary>
    public FindReplace FindReplace { get; private set; }

    public IncrementalSearcher? IncrementalSearcher { get; private set; }

    [ObservableProperty]
    public partial Scintilla? Scintilla { get; set; }

    partial void OnScintillaChanging(Scintilla? value)
    {
        if (value != null && IncrementalSearcher != null)
        {
            value.Controls.Remove(IncrementalSearcher);
        }
    }

    partial void OnScintillaChanged(Scintilla? value)
    {
        if (value is null)
        {
            return;
        }
        
        FindReplace = new(value);
        FindReplace.FindAllResults += FindAllHandler;

        IncrementalSearcher = new()
        {
            Scintilla = value,
            FindReplace = this,
            Visible = false
        };
        value.Controls.Add(IncrementalSearcher);
    }
        
    private FindReplace.FindAllResultsEventHandler FindAllHandler { get; set; }

    [ObservableProperty]
    public partial bool AutoPosition { get; set; } = true;

    [ObservableProperty]
    public partial bool IsFindTabSelected { get; set; } = true;

    [ObservableProperty]
    public partial bool IsStandardSearch { get; set; } = true;

    [ObservableProperty]
    public partial bool IsExtendedSearch { get; set; }

    [ObservableProperty]
    public partial bool IsRegExSearch { get; set; }

    [ObservableProperty]
    public partial bool IgnoreCase { get; set; }

    [ObservableProperty]
    public partial bool WholeWord { get; set; }

    [ObservableProperty]
    public partial bool WordStart { get; set; }

    [ObservableProperty]
    public partial bool IsCompiled { get; set; }

    [ObservableProperty]
    public partial bool IsCultureInvariant { get; set; }

    [ObservableProperty]
    public partial bool IsEcmaScript { get; set; }

    [ObservableProperty]
    public partial bool IsExplicitCapture { get; set; }

    [ObservableProperty]
    public partial bool IgnorePatternWhitespace { get; set; }

    [ObservableProperty]
    public partial bool IsMultiline { get; set; }

    [ObservableProperty]
    public partial bool IsRightToLeft { get; set; }

    [ObservableProperty]
    public partial bool IsSingleLine { get; set; }

    [ObservableProperty]
    public partial bool Wrap { get; set; }

    [ObservableProperty]
    public partial bool SearchSelection { get; set; }

    [ObservableProperty]
    public partial bool MarkLine { get; set; }

    [ObservableProperty]
    public partial bool HighlightMatches { get; set; }

    [ObservableProperty]
    public partial string FindText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReplaceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StatusText { get; set; } = string.Empty;

    [RelayCommand]
    private void FindAll()
    {
        Guard.IsNotNull(Scintilla);
        
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
                rr = new Regex(FindText, GetRegexOptions(), DefaultRegexTimeout);
            }
            catch (ArgumentException ex)
            {
                StatusText = $"{Strings.FindReplace_Status_RegExError}: {ex.Message}";
                return;
            }

            if (SearchSelection)
            {
                var searchRange = new CharacterRange(Scintilla.Selections[0].Start, Scintilla.Selections[0].End);
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
                var searchRange = new CharacterRange(Scintilla.Selections[0].Start, Scintilla.Selections[0].End);
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
        Guard.IsNotNull(Scintilla);
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
                rr = new Regex(FindText, GetRegexOptions(), DefaultRegexTimeout);
            }
            catch (ArgumentException ex)
            {
                StatusText = $"{Strings.FindReplace_Status_RegExError}: {ex.Message}";
                return;
            }

            if (SearchSelection)
            {
                var searchRange = new CharacterRange(Scintilla.Selections[0].Start, Scintilla.Selections[0].End);
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
                var searchRange = new CharacterRange(Scintilla.Selections[0].Start, Scintilla.Selections[0].End);
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
        Guard.IsNotNull(Scintilla);
        Scintilla.MarkerDeleteAll(FindReplace.Marker.Index);
        FindReplace.ClearAllHighlights();
    }

    [RelayCommand]
    private void FindPrevious() => FindWrapper(true);

    private void FindWrapper(bool searchUp)
    {
        Guard.IsNotNull(Scintilla);
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

    [RelayCommand]
    private void FindNext() => FindWrapper(false);

    private CharacterRange FindNext(bool searchUp)
    {
        Regex? rr = null;
        return FindNext(searchUp, ref rr);
    }

    private CharacterRange FindNext(bool searchUp, ref Regex? rr)
    {
        Guard.IsNotNull(Scintilla);
        CharacterRange foundRange;

        if (IsRegExSearch)
        {
            rr = new Regex(FindText, GetRegexOptions(), DefaultRegexTimeout);

            if (SearchSelection)
            {
                var searchRange = new CharacterRange(Scintilla.Selections[0].Start, Scintilla.Selections[0].End);
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
                var searchRange = new CharacterRange(Scintilla.Selections[0].Start, Scintilla.Selections[0].End);
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
    private void ReplacePrevious() => ReplaceWrapper(true);

    private void ReplaceWrapper(bool searchUp)
    {
        Guard.IsNotNull(Scintilla);
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

    [RelayCommand]
    private void ReplaceNext() => ReplaceWrapper(false);

    private CharacterRange ReplaceNext(bool searchUp)
    {
        Guard.IsNotNull(Scintilla);
        Regex? rr = null;
        var selRange = new CharacterRange(Scintilla.Selections[0].Start, Scintilla.Selections[0].End);

        //	We only do the actual replacement if the current selection exactly
        //	matches the find.
        if (selRange.MaxPosition - selRange.MinPosition > 0)
        {
            if (IsRegExSearch)
            {
                rr = new Regex(FindText, GetRegexOptions(), DefaultRegexTimeout);
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
                        Scintilla.SelectionStart = selRange.MinPosition;
                        Scintilla.SelectionEnd = selRange.MaxPosition;
                        Scintilla.ReplaceSelection(rr.Replace(selRangeText, ReplaceText));
                        Scintilla.GotoPosition(selRange.MinPosition);
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
                        Scintilla.SelectionStart = selRange.MinPosition;
                        Scintilla.SelectionEnd = selRange.MaxPosition;
                        Scintilla.ReplaceSelection(textToReplace);

                        Scintilla.GotoPosition(selRange.MinPosition);
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
        Guard.IsNotNull(Scintilla);
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