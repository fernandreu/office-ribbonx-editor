using System;
using System.Collections.Generic;
using OfficeRibbonXEditor.Helpers;

namespace OfficeRibbonXEditor.Events
{
    public class ReplaceResultsEventArgs : EventArgs
    {
        public ReplaceResultsEventArgs(FindReplace findReplace, List<CharacterRange> replaceAllResults)
        {
            FindReplace = findReplace;
            ReplaceAllResults = replaceAllResults ?? new List<CharacterRange>();
        }

        public FindReplace FindReplace { get; set; }

        public List<CharacterRange> ReplaceAllResults { get; }
    }
}
