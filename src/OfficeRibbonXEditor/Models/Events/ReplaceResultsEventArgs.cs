using System;
using System.Collections.Generic;

namespace OfficeRibbonXEditor.Models.Events
{
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
