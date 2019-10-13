using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Models;

namespace OfficeRibbonXEditor.ViewModels
{
    using ResultsEventArgs = DataEventArgs<IResultCollection>;

    public class EditorTabViewModel : ViewModelBase
    {
        private string title;

        public string Title
        {
            get => this.title;
            set => this.Set(ref this.title, value);
        }

        private string lineStatus;

        public string LineStatus
        {
            get => this.lineStatus;
            set => this.Set(ref this.lineStatus, value);
        }

        private int zoom;

        public int Zoom
        {
            get => this.zoom;
            set => this.Set(ref this.zoom, value);
        }

        public ScintillaLexer Lexer { get; set; }

        public OfficePartViewModel Part { get; set; }

        public MainWindowViewModel MainWindow { get; set; }

        /// <summary>
        /// This event will be fired whenever key editor properties (including current text and selection) need to be known. It is the
        /// listener who will need to specify the argument.
        /// </summary>
        public event EventHandler<DataEventArgs<EditorInfo>> ReadEditorInfo;
        
        public event EventHandler<ResultsEventArgs> ShowResults;

        /// <summary>
        /// This event will be fired when the contents of the editor need to be updated
        /// </summary>
        public event EventHandler<EditorChangeEventArgs> UpdateEditor;

        public EditorInfo EditorInfo
        {
            get
            {
                var e = new DataEventArgs<EditorInfo>();
                this.ReadEditorInfo?.Invoke(this, e);
                return e.Data;
            }
        }

        public void RaiseShowResults(ResultsEventArgs e)
        {
            this.ShowResults?.Invoke(this, e);
        }

        public void RaiseUpdateEditor(EditorChangeEventArgs e)
        {
            this.UpdateEditor?.Invoke(this, e);
        }

        public void ApplyCurrentText()
        {
            if (!this.Part.CanHaveContents)
            {
                return;
            }

            var e = new DataEventArgs<EditorInfo>();
            this.ReadEditorInfo?.Invoke(this, e);
            if (e.Data == null)
            {
                // This means that event handler was not listened by any view, or the view did not pass the editor contents back for some reason
                return;
            }
            
            this.Part.Contents = e.Data.Text;
        }
    }
}
