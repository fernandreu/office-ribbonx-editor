using OfficeRibbonXEditor.Helpers;

namespace OfficeRibbonXEditor.Events;

public class ReplaceResultsEventArgs(FindReplace findReplace, List<CharacterRange> replaceAllResults)
    : EventArgs
{
    public FindReplace FindReplace { get; set; } = findReplace;

    public List<CharacterRange> ReplaceAllResults { get; } = replaceAllResults;
}