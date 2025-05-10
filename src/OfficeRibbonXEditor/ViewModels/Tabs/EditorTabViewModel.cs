using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeRibbonXEditor.Events;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Lexers;
using OfficeRibbonXEditor.ViewModels.Documents;
using OfficeRibbonXEditor.ViewModels.Windows;

namespace OfficeRibbonXEditor.ViewModels.Tabs;

using ResultsEventArgs = DataEventArgs<IResultCollection>;

public partial class EditorTabViewModel : ObservableObject, ITabItemViewModel
{
    public EditorTabViewModel(OfficePartViewModel part, MainWindowViewModel mainWindow)
    {
        _part = part;
        MainWindow = mainWindow;
    }

    public event EventHandler? DidCut;

    public event EventHandler? DidCopy;

    public event EventHandler? DidPaste;

    public event EventHandler? DidUndo;

    public event EventHandler? DidRedo;

    public event EventHandler? DidSelectAll;

    public event EventHandler<FoldEventArgs>? DidFold;

    public event EventHandler? DidDuplicateLine;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _statusText;

    [ObservableProperty]
    private int _zoom;

    public ScintillaLexer? Lexer { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(Item))]
    private OfficePartViewModel _part;
        
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

    [RelayCommand]
    private void Cut() => DidCut?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void Copy() => DidCopy?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void Paste() => DidPaste?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void Undo() => DidUndo?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void Redo() => DidRedo?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void SelectAll() => DidSelectAll?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void Fold(int level) => DidFold?.Invoke(this, new FoldEventArgs(level));

    [RelayCommand]
    private void Unfold(int level) => DidFold?.Invoke(this, new FoldEventArgs(level, true));

    [RelayCommand]
    private void FoldCurrent() => DidFold?.Invoke(this, new FoldEventArgs(true));

    [RelayCommand]
    private void UnfoldCurrent() => DidFold?.Invoke(this, new FoldEventArgs(true, true));

    [RelayCommand]
    private void DuplicateLine() => DidDuplicateLine?.Invoke(this, EventArgs.Empty);
}