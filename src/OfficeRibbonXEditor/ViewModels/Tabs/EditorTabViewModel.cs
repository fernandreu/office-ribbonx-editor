using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Models;
using OfficeRibbonXEditor.Models.Events;
using OfficeRibbonXEditor.Models.Lexers;
using OfficeRibbonXEditor.ViewModels.Documents;
using OfficeRibbonXEditor.ViewModels.Windows;

namespace OfficeRibbonXEditor.ViewModels.Tabs
{
    using ResultsEventArgs = DataEventArgs<IResultCollection>;

    public class EditorTabViewModel : ViewModelBase, ITabItemViewModel
    {
        public EditorTabViewModel(OfficePartViewModel part, MainWindowViewModel mainWindow)
        {
            this.part = part;
            this.MainWindow = mainWindow;

            this.CutCommand = new RelayCommand(() => this.Cut?.Invoke(this, EventArgs.Empty));
            this.CopyCommand = new RelayCommand(() => this.Copy?.Invoke(this, EventArgs.Empty));
            this.PasteCommand = new RelayCommand(() => this.Paste?.Invoke(this, EventArgs.Empty));
            this.UndoCommand = new RelayCommand(() => this.Undo?.Invoke(this, EventArgs.Empty));
            this.RedoCommand = new RelayCommand(() => this.Redo?.Invoke(this, EventArgs.Empty));
            this.SelectAllCommand = new RelayCommand(() => this.SelectAll?.Invoke(this, EventArgs.Empty));
            this.FoldCommand = new RelayCommand<int>(level => this.Fold?.Invoke(this, new FoldEventArgs(level)));
            this.UnfoldCommand = new RelayCommand<int>(level => this.Fold?.Invoke(this, new FoldEventArgs(level, true)));
            this.FoldCurrentCommand = new RelayCommand(() => this.Fold?.Invoke(this, new FoldEventArgs(true)));
            this.UnfoldCurrentCommand = new RelayCommand(() => this.Fold?.Invoke(this, new FoldEventArgs(true, true)));
        }

        public RelayCommand CutCommand { get; }

        public RelayCommand CopyCommand { get; }

        public RelayCommand PasteCommand { get; }

        public RelayCommand UndoCommand { get; }

        public RelayCommand RedoCommand { get; }

        public RelayCommand SelectAllCommand { get; }

        public RelayCommand<int> FoldCommand { get; }

        public RelayCommand<int> UnfoldCommand { get; }

        public RelayCommand FoldCurrentCommand { get; }

        public RelayCommand UnfoldCurrentCommand { get; }

        public event EventHandler? Cut;

        public event EventHandler? Copy;

        public event EventHandler? Paste;

        public event EventHandler? Undo;

        public event EventHandler? Redo;

        public event EventHandler? SelectAll;

        public event EventHandler<FoldEventArgs>? Fold;  

        private string title = string.Empty;

        public string Title
        {
            get => this.title;
            set => this.Set(ref this.title, value);
        }

        private string? statusText;

        public string? StatusText
        {
            get => this.statusText;
            set => this.Set(ref this.statusText, value);
        }

        private int zoom;

        public int Zoom
        {
            get => this.zoom;
            set => this.Set(ref this.zoom, value);
        }

        public ScintillaLexer? Lexer { get; set; }

        private OfficePartViewModel part;

        public OfficePartViewModel Part
        {
            get => this.part;
            set
            {
                if (!this.Set(ref this.part, value))
                {
                    return;
                }

                this.RaisePropertyChanged(nameof(this.StatusText));
                this.RaisePropertyChanged(nameof(this.Item));
            }
        }

        public TreeViewItemViewModel Item => this.Part;

        public MainWindowViewModel MainWindow { get; set; }

        /// <summary>
        /// This event will be fired whenever key editor properties (including current text and selection) need to be known. It is the
        /// listener who will need to specify the argument.
        /// </summary>
        public event EventHandler<DataEventArgs<EditorInfo>>? ReadEditorInfo;
        
        public event EventHandler<ResultsEventArgs>? ShowResults;

        /// <summary>
        /// This event will be fired when the contents of the editor need to be updated
        /// </summary>
        public event EventHandler<EditorChangeEventArgs>? UpdateEditor;

        public EditorInfo? EditorInfo
        {
            get
            {
                var e = new DataEventArgs<EditorInfo>();
                this.ReadEditorInfo?.Invoke(this, e);
                return e.Data;
            }
        }

        public void OnShowResults(ResultsEventArgs e)
        {
            this.ShowResults?.Invoke(this, e);
        }

        public void OnUpdateEditor(EditorChangeEventArgs e)
        {
            this.UpdateEditor?.Invoke(this, e);
        }

        public void ApplyChanges()
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
