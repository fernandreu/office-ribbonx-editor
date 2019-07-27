using ScintillaNET;

namespace OfficeRibbonXEditor.Interfaces
{
    public interface IResultCollection
    {
        void AddToPanel(Scintilla editor, Scintilla resultsPanel);

        void GoToPosition(int pos, Scintilla editor, Scintilla resultsPanel);
    }
}
