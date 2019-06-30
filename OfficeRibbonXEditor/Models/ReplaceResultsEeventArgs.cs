using System;
using System.Collections.Generic;
using OfficeRibbonXEditor.Dialogs.FindReplace;

namespace OfficeRibbonXEditor.Models
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
