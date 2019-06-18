#region Using Directives

#endregion Using Directives


namespace OfficeRibbonXEditor.Dialogs.FindReplace.FindReplace
{
    using System.Windows.Forms;
    using ScintillaNET;

    public class ToolStripIncrementalSearcher : ToolStripControlHost
    {
        #region Properties

        public Scintilla Scintilla
        {
            get { return this.Searcher.Scintilla; }
            set { this.Searcher.Scintilla = value; }
        }


        public IncrementalSearcher Searcher
        {
            get { return this.Control as IncrementalSearcher; }
        }

        #endregion Properties


        #region Constructors

        public ToolStripIncrementalSearcher() : base(new IncrementalSearcher(true))
        {
        }

        #endregion Constructors
    }
}
