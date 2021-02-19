using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Events;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Lexers;
using OfficeRibbonXEditor.ViewModels.Documents;
using OfficeRibbonXEditor.ViewModels.Windows;

namespace OfficeRibbonXEditor.ViewModels.Tabs
{
    using ResultsEventArgs = DataEventArgs<IResultCollection>;

    public class EditorTabViewModel : ViewModelBase, ITabItemViewModel
    {
        public EditorTabViewModel(OfficePartViewModel part, MainWindowViewModel mainWindow)
        {
            _part = part;
            MainWindow = mainWindow;
        }

        private RelayCommand? _cutCommand;
        public RelayCommand CutCommand => _cutCommand ??= new RelayCommand(() => Cut?.Invoke(this, EventArgs.Empty));

        private RelayCommand? _copyCommand;
        public RelayCommand CopyCommand => _copyCommand ??= new RelayCommand(() => Copy?.Invoke(this, EventArgs.Empty));

        private RelayCommand? _pasteCommand;
        public RelayCommand PasteCommand => _pasteCommand ??= new RelayCommand(() => Paste?.Invoke(this, EventArgs.Empty));

        private RelayCommand? _undoCommand;
        public RelayCommand UndoCommand => _undoCommand ??= new RelayCommand(() => Undo?.Invoke(this, EventArgs.Empty));

        private RelayCommand? _redoCommand;
        public RelayCommand RedoCommand => _redoCommand ??= new RelayCommand(() => Redo?.Invoke(this, EventArgs.Empty));

        private RelayCommand? _selectAllCommand;
        public RelayCommand SelectAllCommand => _selectAllCommand ??= new RelayCommand(() => SelectAll?.Invoke(this, EventArgs.Empty));

        private RelayCommand<int>? _foldCommand;
        public RelayCommand<int> FoldCommand => _foldCommand ??= new RelayCommand<int>(level => Fold?.Invoke(this, new FoldEventArgs(level)));

        private RelayCommand<int>? _unfoldCommand;
        public RelayCommand<int> UnfoldCommand => _unfoldCommand ??= new RelayCommand<int>(level => Fold?.Invoke(this, new FoldEventArgs(level, true)));

        private RelayCommand? _foldCurrentCommand;
        public RelayCommand FoldCurrentCommand => _foldCurrentCommand ??= new RelayCommand(() => Fold?.Invoke(this, new FoldEventArgs(true)));

        private RelayCommand? _unfoldCurrentCommand;
        public RelayCommand UnfoldCurrentCommand => _unfoldCurrentCommand ??= new RelayCommand(() => Fold?.Invoke(this, new FoldEventArgs(true, true)));

        private RelayCommand? _duplicateLineCommand;
        public RelayCommand DuplicateLineCommand => _duplicateLineCommand ??= new RelayCommand(() => DuplicateLine?.Invoke(this, EventArgs.Empty));

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
    }
}
