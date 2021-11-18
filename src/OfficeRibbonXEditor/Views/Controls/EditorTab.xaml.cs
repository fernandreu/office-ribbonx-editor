using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OfficeRibbonXEditor.Events;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Lexers;
using OfficeRibbonXEditor.ViewModels.Tabs;
using ScintillaNET;

namespace OfficeRibbonXEditor.Views.Controls
{
    /// <summary>
    /// Interaction logic for EditorTab.xaml
    /// </summary>
    public partial class EditorTab : UserControl
    {
        private GridLength _lastResultsHeight = new GridLength(150);

        private EditorTabViewModel? _viewModel;

        public EditorTab()
        {
            InitializeComponent();

            ResultsPanel.Scintilla = Editor.Scintilla;
        }
        
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property != DataContextProperty)
            {
                return;
            }

            if (e.OldValue is EditorTabViewModel previousModel)
            {
                // The way the DataTemplate works means the same control can be re-used for the different tabs, just with a different
                // DataContext. It is important to ensure the previous part is saved first 
                previousModel.Part.Contents = Editor.Text;

                previousModel.ReadEditorInfo -= OnReadEditorInfo;
                previousModel.UpdateEditor -= OnUpdateEditor;
                previousModel.ShowResults -= OnShowResults;

                previousModel.Cut -= OnCut;
                previousModel.Copy -= OnCopy;
                previousModel.Paste -= OnPaste;
                previousModel.Undo -= OnUndo;
                previousModel.Redo -= OnRedo;
                previousModel.SelectAll -= OnSelectAll;
                previousModel.Fold -= OnFold;
                previousModel.DuplicateLine -= OnDuplicateLine;
            }

            if (e.NewValue is not EditorTabViewModel model)
            {
                _viewModel = null;
                return;
            }

            _viewModel = model;
            _viewModel.Lexer = new XmlLexer
            {
                Editor = Editor,
            };

            Editor.Text = _viewModel.Part.Contents;
            Editor.EmptyUndoBuffer();

            _viewModel.ReadEditorInfo += OnReadEditorInfo;
            _viewModel.UpdateEditor += OnUpdateEditor;
            _viewModel.ShowResults += OnShowResults;

            _viewModel.Cut += OnCut;
            _viewModel.Copy += OnCopy;
            _viewModel.Paste += OnPaste;
            _viewModel.Undo += OnUndo;
            _viewModel.Redo += OnRedo;
            _viewModel.SelectAll += OnSelectAll;
            _viewModel.Fold += OnFold;
            _viewModel.DuplicateLine += OnDuplicateLine;
        }

        private void OnReadEditorInfo(object? sender, DataEventArgs<EditorInfo> e)
        {
            e.Data = new EditorInfo(
                Editor.Text,
                Tuple.Create(Editor.SelectionStart, Editor.SelectionEnd));
        }

        private void OnUpdateEditor(object? sender, EditorChangeEventArgs e)
        {
            Editor.DeleteRange(e.Start, (e.End >= 0 ? e.End : Editor.TextLength) - e.Start);
            Editor.InsertText(e.Start, e.NewText);
            if (e.UpdateSelection)
            {
                Editor.SetSelection(e.Start, e.Start + e.NewText.Length);
            }

            if (e.ResetUndoHistory)
            {
                Editor.EmptyUndoBuffer();
            }
        }

        private void OnShowResults(object? sender, DataEventArgs<IResultCollection> e)
        {
            if (e.Data?.IsEmpty ?? true)
            {
                // No reason to update the panel; simply close it if appropriate
                if (ResultsSplitter.Visibility != Visibility.Collapsed && e.Data?.GetType() == ResultsPanel.Results?.GetType())
                {
                    // In this case, appropriate means: it was previously open and showing data of the same type
                    OnCloseFindResults(sender, e);
                }

                return;
            }

            ResultsSplitter.Visibility = Visibility.Visible;
            ResultsRow.Height = _lastResultsHeight;
            ResultsHeader.Content = e.Data.Header;
            ResultsPanel.UpdateFindAllResults(e.Data);
        }

        private void OnCut(object? sender, EventArgs e)
        {
            Editor.Cut();
        }

        private void OnCopy(object? sender, EventArgs e)
        {
            Editor.Copy();
        }

        private void OnPaste(object? sender, EventArgs e)
        {
            Editor.Paste();
        }

        private void OnUndo(object? sender, EventArgs e)
        {
            Editor.Undo();
        }

        private void OnRedo(object? sender, EventArgs e)
        {
            Editor.Redo();
        }

        private void OnSelectAll(object? sender, EventArgs e)
        {
            Editor.SelectAll();
        }

        private void OnScintillaUpdateUi(object? sender, UpdateUIEventArgs e)
        {
            if (!(DataContext is EditorTabViewModel vm))
            {
                return;
            }

            if (!Editor.IsEnabled)
            {
                vm.StatusText = "Ln 0, Col 0";
            }
            else
            {
                var pos = Editor.CurrentPosition;
                var line = Editor.LineFromPosition(pos);
                var col = Editor.GetColumn(pos);

                vm.StatusText = $"Ln {line + 1},  Col {col + 1}";
            }
        }

        private void OnFold(object? sender, FoldEventArgs e)
        {
            var action = e.Unfold ? FoldAction.Expand : FoldAction.Contract;
            if (e.CurrentOnly)
            {
                var index = Editor.LineFromPosition(Editor.CurrentPosition);
                var line = Editor.Lines[index];
                line.FoldChildren(action);
                return;
            }

            if (e.Level <= 0)
            {
                Editor.FoldAll(action);
                return;
            }

            foreach (var line in Editor.Lines)
            {
                // Fold levels internally start at 1024: https://github.com/jacobslusser/ScintillaNET/issues/307#issuecomment-280809695
                if (line.FoldLevel - 1023 < e.Level)
                {
                    continue;
                }

                line.FoldLine(action);
            }
        }

        private void OnDuplicateLine(object? sender, EventArgs e)
        {
            // The Scintilla editor has an equivalent Ctrl+D shortcut, but it messes up the automatic indentation, so we do our own version instead
            var index = Editor.LineFromPosition(Editor.CurrentPosition);
            var line = Editor.Lines[index];
            Editor.InsertText(line.Position, line.Text);
            Editor.ScrollCaret();
        }

        // For some reason, the Scintilla editor always seems to have preference over the input gestures.
        // The only solution (so far) is to execute those when appropriate from this event handler.
        private void OnEditorKeyDown(object? sender, System.Windows.Forms.KeyEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window == null)
            {
                return;
            }

            foreach (var ib in window.InputBindings)
            {
                if (!(ib is KeyBinding kb))
                {
                    return;
                }
                        
                var inputKey = KeyInterop.KeyFromVirtualKey(e.KeyValue);
                if (kb.Key != inputKey)
                {
                    continue;
                }

                if (kb.Modifiers.HasFlag(ModifierKeys.Control) != e.Control)
                {
                    continue;
                }
                        
                if (kb.Modifiers.HasFlag(ModifierKeys.Shift) != e.Shift)
                {
                    continue;
                }
                        
                if (kb.Modifiers.HasFlag(ModifierKeys.Alt) != e.Alt)
                {
                    continue;
                }

                if (kb.Command == null)
                {
                    return;
                }

                e.SuppressKeyPress = true;
                e.Handled = true;

                // If this is not async, the lines above seem to be ignored and the character is still printed when
                // launching a dialog (e.g. Ctrl-O). See: https://github.com/fernandreu/office-ribbonx-editor/issues/20
                Dispatcher?.InvokeAsync(() => kb.Command?.Execute(null));

                return;
            }
        }

        private void OnEditorInsertCheck(object? sender, InsertCheckEventArgs e)
        {
            if (!Properties.Settings.Default.AutoIndent)
            {
                return;
            }

            if (!e.Text.EndsWith("\r", StringComparison.OrdinalIgnoreCase) && !e.Text.EndsWith("\n", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var startPos = Editor.Lines[Editor.LineFromPosition(Editor.CurrentPosition)].Position;
            var endPos = e.Position;
            var curLineText = Editor.GetTextRange(startPos, endPos - startPos); // Text until the caret
            var indent = Regex.Match(curLineText, "^[ \\t]*");
            e.Text += indent.Value;
            if (Regex.IsMatch(curLineText, "[^/]>\\s*$"))
            {
                // If the previous line finished with an open tag, add an indentation level to the next one 
                e.Text += new string(' ', Properties.Settings.Default.TabWidth);
            }
        }

        private void OnCloseFindResults(object? sender, EventArgs e)
        {
            if (ResultsRow.Height.Value > new GridLength(50).Value)
            {
                _lastResultsHeight = ResultsRow.Height;
            }

            ResultsRow.Height = new GridLength(0);
            ResultsSplitter.Visibility = Visibility.Collapsed;
        }
    }
}
