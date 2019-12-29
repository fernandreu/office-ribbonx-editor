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
            this.InitializeComponent();

            this.ResultsScintilla.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            this.ResultsScintilla.Styles[ScintillaNET.Style.Default].Size = 10;

            this.ResultsScintilla.ClearAll();

            this.ResultsScintilla.Scintilla.KeyUp += this.KeyUpEventHandler;
            this.ResultsScintilla.Scintilla.MouseClick += this.MouseClickEventHandler;
            this.ResultsScintilla.Scintilla.MouseDoubleClick += this.MouseDoubleClickEventHandler;
        }

        public IResultCollection Results { get; private set; }

        public Scintilla Scintilla { get; set; }

        /// <summary>
        /// Updates the find all results panel
        /// </summary>
        /// <param name="results"></param>
        public void UpdateFindAllResults(IResultCollection results)
        {
            if (results == null)
            {
                return;
            }

            this.Results = results;
            this.Results.AddToPanel(this.Scintilla, this.ResultsScintilla.Scintilla);
        }

        private void KeyUpEventHandler(object sender, KeyEventArgs e)
        {
            var pos = this.ResultsScintilla.CurrentPosition;
            this.Results.GoToPosition(pos, this.Scintilla, this.ResultsScintilla.Scintilla);
        }

        private void MouseClickEventHandler(object sender, MouseEventArgs e)
        {
            var pos = this.ResultsScintilla.CharPositionFromPointClose((e.Location).X, (e.Location).Y);
            if (pos == -1)
            {
                return;
            }

            this.Results.GoToPosition(pos, this.Scintilla, this.ResultsScintilla.Scintilla);
        }

        private void MouseDoubleClickEventHandler(object sender, MouseEventArgs e)
        {
            var pos = this.ResultsScintilla.CharPositionFromPointClose((e.Location).X, (e.Location).Y);
            if (pos == -1)
            {
                return;
            }
            
            this.Results.GoToPosition(pos, this.Scintilla, this.ResultsScintilla.Scintilla);
        }
    }
}
