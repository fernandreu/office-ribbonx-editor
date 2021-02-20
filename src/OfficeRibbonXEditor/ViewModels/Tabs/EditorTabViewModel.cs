using System;
using GalaSoft.MvvmLight;
using Generators;
using OfficeRibbonXEditor.Events;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Lexers;
using OfficeRibbonXEditor.ViewModels.Documents;
using OfficeRibbonXEditor.ViewModels.Windows;

namespace OfficeRibbonXEditor.ViewModels.Tabs
{
    using ResultsEventArgs = DataEventArgs<IResultCollection>;

    public partial class EditorTabViewModel : ViewModelBase, ITabItemViewModel
    {
        public EditorTabViewModel(OfficePartViewModel part, MainWindowViewModel mainWindow)
        {
            _part = part;
            MainWindow = mainWindow;
        }

        public event EventHandler? Cut;

        public event EventHandler? Copy;

        public event EventHandler? Paste;

        public event EventHandler? Undo;

        public event EventHandler? Redo;

        public event EventHandler? SelectAll;

        public event EventHandler<FoldEventArgs>? Fold;

        public event EventHandler? DuplicateLine;

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        private string? _statusText;
        public string? StatusText
        {
            get => _statusText;
            set => Set(ref _statusText, value);
        }

        private int _zoom;
        public int Zoom
        {
            get => _zoom;
            set => Set(ref _zoom, value);
        }

        public ScintillaLexer? Lexer { get; set; }

        private OfficePartViewModel _part;
        public OfficePartViewModel Part
        {
            get => _part;
            set
            {
                if (!Set(ref _part, value))
                {
                    return;
                }

                RaisePropertyChanged(nameof(StatusText));
                RaisePropertyChanged(nameof(Item));
            }
        }

        public TreeViewItemViewModel Item => Part;

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
                ReadEditorInfo?.Invoke(this, e);
                return e.Data;
            }
        }

        public void OnShowResults(ResultsEventArgs e)
        {
            ShowResults?.Invoke(this, e);
        }

        public void OnUpdateEditor(EditorChangeEventArgs e)
        {
            UpdateEditor?.Invoke(this, e);
        }

        public void ApplyChanges()
        {
            if (!Part.CanHaveContents)
            {
                return;
            }

            var e = new DataEventArgs<EditorInfo>();
            ReadEditorInfo?.Invoke(this, e);
            if (e.Data == null)
            {
                // This means that event handler was not listened by any view, or the view did not pass the editor contents back for some reason
                return;
            }
            
            Part.Contents = e.Data.Text;
        }

        [GenerateCommand]
        private void RaiseCut() => Cut?.Invoke(this, EventArgs.Empty);

        [GenerateCommand]
        private void RaiseCopy() => Copy?.Invoke(this, EventArgs.Empty);

        [GenerateCommand]
        private void RaisePaste() => Paste?.Invoke(this, EventArgs.Empty);

        [GenerateCommand]
        private void RaiseUndo() => Undo?.Invoke(this, EventArgs.Empty);

        [GenerateCommand]
        private void RaiseRedo() => Redo?.Invoke(this, EventArgs.Empty);

        [GenerateCommand]
        private void RaiseSelectAll() => SelectAll?.Invoke(this, EventArgs.Empty);

        [GenerateCommand]
        private void RaiseFold(int level) => Fold?.Invoke(this, new FoldEventArgs(level));

        [GenerateCommand]
        private void RaiseUnfold(int level) => Fold?.Invoke(this, new FoldEventArgs(level, true));

        [GenerateCommand]
        private void RaiseFoldCurrent() => Fold?.Invoke(this, new FoldEventArgs(true));

        [GenerateCommand]
        private void RaiseUnfoldCurrent() => Fold?.Invoke(this, new FoldEventArgs(true, true));

        [GenerateCommand]
        private void RaiseDuplicateLine() => DuplicateLine?.Invoke(this, EventArgs.Empty);

    }
}
