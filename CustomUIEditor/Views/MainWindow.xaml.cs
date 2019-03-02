// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Fernando Andreu">
//   Fernando Andreu
// </copyright>
// <summary>
//   Interaction logic for MainWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;

    using CustomUIEditor.Models;

    using ScintillaNET;

    using ViewModels;

    using KeyEventArgs = System.Windows.Input.KeyEventArgs;
    using TextBox = System.Windows.Controls.TextBox;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow
    {
        private readonly MainWindowViewModel viewModel;

        private readonly XmlLexer lexer;

        private bool suppressRequestBringIntoView;

        public MainWindow()
        {
            this.InitializeComponent();

            this.viewModel = (MainWindowViewModel)this.DataContext;

            this.viewModel.ShowSettings += (o, e) => this.ShowSettings();
            this.viewModel.ShowCallbacks += (o, e) => this.ShowCallbacks(e.Data);
            this.viewModel.ReadEditorInfo += (o, e) => e.Data = new EditorInfo { Text = this.Editor.Text, Selection = Tuple.Create(this.Editor.SelectionStart, this.Editor.SelectionEnd) };
            this.viewModel.InsertRecentFile += (o, e) => this.RecentFileList.InsertFile(e.Data);
            this.viewModel.UpdateLexer += (o, e) => this.lexer.Update();
            this.viewModel.UpdateEditor += (o, e) =>
                {
                    this.Editor.DeleteRange(e.Start, (e.End >= 0 ? e.End : this.Editor.TextLength) - e.Start);
                    this.Editor.InsertText(e.Start, e.NewText);
                    if (e.UpdateSelection)
                    {
                        this.Editor.SetSelection(e.Start, e.Start + e.NewText.Length);
                    }

                    if (e.ResetUndoHistory)
                    {
                        this.Editor.EmptyUndoBuffer();
                    }
                };

            this.lexer = new XmlLexer { Editor = this.Editor };
            
            this.Editor.Scintilla.KeyDown += (o, e) =>
                {
                    // For some reason, the Scintilla editor always has preference over the input gestures.
                    // The only solution (so far) is to execute those when appropriate from this event handler.
                    foreach (var ib in this.InputBindings)
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
                        
                        e.SuppressKeyPress = true;
                        e.Handled = true;
                        kb.Command.Execute(null);
                        return;
                    }
                };
        }
        
        /// <summary>
        /// Finds the first TreeViewItem which is a parent of the given source.
        /// See <a href="https://stackoverflow.com/questions/592373/select-treeview-node-on-right-click-before-displaying-contextmenu">this</a>
        /// </summary>
        /// <param name="source">The starting source where the TreeViewItem will be searched</param>
        /// <returns>The item found, or null otherwise</returns>
        private static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }

            return source as TreeViewItem;
        }

        #region Disable horizontal scrolling when selecting TreeView item

        private void TreeViewItemRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            // Ignore re-entrant calls
            if (this.suppressRequestBringIntoView)
            {
                return;
            }

            // Cancel the current scroll attempt
            e.Handled = true;

            // Call BringIntoView using a rectangle that extends into "negative space" to the left of our
            // actual control. This allows the vertical scrolling behaviour to operate without adversely
            // affecting the current horizontal scroll position.
            this.suppressRequestBringIntoView = true;

            if (sender is TreeViewItem item)
            {
                var newTargetRect = new Rect(-1000, 0, item.ActualWidth + 1000, item.ActualHeight);
                item.BringIntoView(newTargetRect);
            }

            this.suppressRequestBringIntoView = false;
        }

        // The call to BringIntoView() in TreeViewItem_OnSelected is also important
        private void TreeViewItem_OnSelected(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;

            // Correctly handle horizontally scrolling for programmatically selected items
            item.BringIntoView();
            e.Handled = true;
        }

        #endregion

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DocumentViewSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var newItem = e.NewValue as TreeViewItemViewModel;

            this.viewModel.SelectedItem = newItem;

            if (newItem == null || !newItem.CanHaveContents)
            {
                this.Editor.Text = string.Empty;
            }
            else
            {
                this.Editor.Text = newItem.Contents;
            }

            // In any case, reset the undo history
            this.Editor.EmptyUndoBuffer();
        }

        private void ScintillaUpdateUi(object sender, UpdateUIEventArgs e)
        {
            if (!this.Editor.IsEnabled)
            {
                this.LineBox.Text = "Ln 0, Col 0";
            }
            else
            {
                var pos = this.Editor.CurrentPosition;
                var line = this.Editor.LineFromPosition(pos);
                var col = this.Editor.GetColumn(pos);

                this.LineBox.Text = $"Ln {line + 1},  Col {col + 1}";
            }
        }

        private void ShowSettings()
        {
            new SettingsDialog(this.lexer) { Owner = this }.ShowDialog();
        }

        private void ShowCallbacks(string code)
        {
            new CallbackWindow(code) { Owner = this }.ShowDialog();
        }

        private void EditorZoomChanged(object sender, EventArgs e)
        {
            this.ZoomBox.Value = this.Editor.Zoom;
        }

        /// <summary>
        /// Ensures that a TreeViewItem is selected before the context menu is displayed
        /// See <a href="https://stackoverflow.com/questions/592373/select-treeview-node-on-right-click-before-displaying-contextmenu">this</a>
        /// </summary>
        /// <param name="sender">The sender of the right click</param>
        /// <param name="e">The event arguments</param>
        private void OnTreeViewRightClick(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        private void ShowAboutDialog(object sender, RoutedEventArgs e)
        {
            new AboutDialog { Owner = this }.ShowDialog();
        }

        private void ChangeIdTextDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            if (!(sender is TextBox textBox))
            {
                return;
            }
            
            textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void IdTextVisible(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                return;
            }

            if (!(sender is TextBox textBox))
            {
                return;
            }

            textBox.Focus();
            textBox.SelectAll();
        }
    }
}