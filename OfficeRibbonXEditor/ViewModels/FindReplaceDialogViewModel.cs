namespace OfficeRibbonXEditor.ViewModels
{
    public class FindReplaceDialogViewModel : DialogBase
    {
        private bool isStandardSearch = true;

        public bool IsStandardSearch
        {
            get => this.isStandardSearch;
            set => this.Set(ref this.isStandardSearch, value);
        }

        private bool isExtendedSearch;

        public bool IsExtendedSearch
        {
            get => this.isExtendedSearch;
            set => this.Set(ref this.isExtendedSearch, value);
        }

        private bool isRegExSearch;

        public bool IsRegExSearch
        {
            get => this.isRegExSearch;
            set => this.Set(ref this.isRegExSearch, value);
        }

        private bool matchCase;

        public bool MatchCase
        {
            get => this.matchCase;
            set => this.Set(ref this.matchCase, value);
        }

        private bool wholeWord;

        public bool WholeWord
        {
            get => this.wholeWord;
            set => this.Set(ref this.wholeWord, value);
        }

        private bool wordStart;

        public bool WordStart
        {
            get => this.wordStart;
            set => this.Set(ref this.wordStart, value);
        }

        private bool wrap;

        public bool Wrap
        {
            get => this.wrap;
            set => this.Set(ref this.wrap, value);
        }

        private bool searchSelection;

        public bool SearchSelection
        {
            get => this.searchSelection;
            set => this.Set(ref this.searchSelection, value);
        }

        private bool markLine;

        public bool MarkLine
        {
            get => this.markLine;
            set => this.Set(ref this.markLine, value);
        }

        private bool highlightMatches;

        public bool HighlightMatches
        {
            get => this.highlightMatches;
            set => this.Set(ref this.highlightMatches, value);
        }
    }
}
