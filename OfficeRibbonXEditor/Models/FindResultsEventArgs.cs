using System;
using System.Collections.Generic;

namespace OfficeRibbonXEditor.Models
{
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
}
