using System.Windows.Forms;
using OfficeRibbonXEditor.Interfaces;
using ScintillaNET;
using UserControl = System.Windows.Controls.UserControl;

namespace OfficeRibbonXEditor.Views.Controls
{
    /// <summary>
    /// Interaction logic for ResultsPanel.xaml
    /// </summary>
    public partial class ResultsPanel : UserControl
    {
        public ResultsPanel()
        {
            InitializeComponent();

            ResultsScintilla.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            ResultsScintilla.Styles[ScintillaNET.Style.Default].Size = 10;

            ResultsScintilla.ClearAll();

            ResultsScintilla.Scintilla.KeyUp += KeyUpEventHandler;
            ResultsScintilla.Scintilla.MouseClick += MouseClickEventHandler;
            ResultsScintilla.Scintilla.MouseDoubleClick += MouseDoubleClickEventHandler;
        }

        public IResultCollection? Results { get; private set; }

        public Scintilla? Scintilla { get; set; }

        /// <summary>
        /// Updates the find all results panel
        /// </summary>
        /// <param name="results"></param>
        public void UpdateFindAllResults(IResultCollection? results)
        {
            if (results == null || Scintilla == null)
            {
                return;
            }

            Results = results;
            Results.AddToPanel(Scintilla, ResultsScintilla.Scintilla);
        }

        private void KeyUpEventHandler(object? sender, KeyEventArgs e)
        {
            if (Scintilla == null)
            {
                return;
            }

            var pos = ResultsScintilla.CurrentPosition;
            Results?.GoToPosition(pos, Scintilla, ResultsScintilla.Scintilla);
        }

        private void MouseClickEventHandler(object? sender, MouseEventArgs e)
        {
            if (Scintilla == null)
            {
                return;
            }

            var pos = ResultsScintilla.CharPositionFromPointClose(e.Location.X, e.Location.Y);
            if (pos == -1)
            {
                return;
            }

            Results?.GoToPosition(pos, Scintilla, ResultsScintilla.Scintilla);
        }

        private void MouseDoubleClickEventHandler(object? sender, MouseEventArgs e)
        {
            if (Scintilla == null)
            {
                return;
            }

            var pos = ResultsScintilla.CharPositionFromPointClose(e.Location.X, e.Location.Y);
            if (pos == -1)
            {
                return;
            }
            
            Results?.GoToPosition(pos, Scintilla, ResultsScintilla.Scintilla);
        }
    }
}
