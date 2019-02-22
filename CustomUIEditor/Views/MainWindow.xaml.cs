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
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Xml;

    using CustomUIEditor.Models;

    using ScintillaNET;

    using ViewModels;

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
            this.viewModel.ReadCurrentText += (o, e) => e.Data = this.Editor.Text;
            this.viewModel.InsertRecentFile += (o, e) => this.RecentFileList.InsertFile(e.Data);
            this.viewModel.UpdateEditor += (o, e) => this.Editor.Text = e.Data;

            this.lexer = new XmlLexer { Editor = this.Editor };
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

        private void GenerateCallbacks(object sender, RoutedEventArgs e)
        {
            // First, check whether any text is selected
            try
            {
                var customUi = new XmlDocument();
                customUi.LoadXml(this.Editor.Text);

                var callbacks = CallbacksBuilder.GenerateCallback(customUi);
                if (callbacks == null || callbacks.Length == 0)
                {
                    MessageBox.Show(StringsResource.idsNoCallback, "Generate Callbacks", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var window = new CallbackWindow(callbacks.ToString()) { Owner = this };
                window.Show();
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
            }
        }

        private void DocumentViewSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var newItem = e.NewValue as TreeViewItemViewModel;

            this.viewModel.SelectedItem = newItem;

            if (newItem == null || !newItem.CanHaveContents)
            {
                this.Editor.Text = string.Empty;
                return;
            }

            this.Editor.Text = newItem.Contents;
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
            var dlg = new SettingsDialog(this.lexer) { Owner = this };
            dlg.ShowDialog();
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