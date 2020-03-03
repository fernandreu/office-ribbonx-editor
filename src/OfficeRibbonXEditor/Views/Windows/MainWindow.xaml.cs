using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.ViewModels.Documents;
using OfficeRibbonXEditor.ViewModels.Windows;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;

namespace OfficeRibbonXEditor.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow
    {
        private MainWindowViewModel? viewModel;

        private bool suppressRequestBringIntoView;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            if (args.Property != DataContextProperty || !(args.NewValue is MainWindowViewModel model))
            {
                return;
            }

            this.viewModel = model;

            this.viewModel.InsertRecentFile += (o, e) =>
            {
                if (e.Data != null)
                {
                    this.RecentFileList.InsertFile(e.Data);
                }
            };

            this.viewModel.SetGlobalCursor += (o, e) => Mouse.OverrideCursor = e.Data;

            // These no longer use interactions in XAML due to the conversion needed for the command parameter
            this.PreviewDragEnter += this.OnPreviewDragEnter;
            this.Drop += this.OnDrop;
        }

        private void OnPreviewDragEnter(object sender, DragEventArgs e)
        {
            var data = new DragData(e.Data);
            this.viewModel?.PreviewDragEnterCommand.Execute(data);
            if (data.Handled)
            {
                e.Handled = true;
            }
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            var data = new DragData(e.Data);
            this.viewModel?.DropCommand.Execute(data);
            if (data.Handled)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Finds the first TreeViewItem which is a parent of the given source.
        /// See <a href="https://stackoverflow.com/questions/592373/select-treeview-node-on-right-click-before-displaying-contextmenu">this</a>
        /// </summary>
        /// <param name="source">The starting source where the TreeViewItem will be searched</param>
        /// <returns>The item found, or null otherwise</returns>
        private static TreeViewItem? VisualUpwardSearch(DependencyObject? source)
        {
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }

            return source as TreeViewItem;
        }

        #region Disable horizontal scrolling when selecting TreeView item

        private void OnTreeViewItemRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
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

        // The call to BringIntoView() in OnTreeViewItemSelected is also important
        private void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;

            // Correctly handle horizontally scrolling for programmatically selected items
            item.BringIntoView();
            e.Handled = true;
        }

        #endregion

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

        private void OnChangeIdTextDown(object sender, KeyEventArgs e)
        {
            if (!(sender is TextBox textBox))
            {
                return;
            }

            if (!(textBox.DataContext is IconViewModel icon))
            {
                return;
            }

            if (e.Key == Key.Enter)
            {
                icon.CommitIdChange();
            }
            else if (e.Key == Key.Escape)
            {
                icon.DiscardIdChange();
            }
        }

        private void OnIdTextVisible(object sender, DependencyPropertyChangedEventArgs e)
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

        private void OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            this.viewModel?.OpenTabCommand.Execute(null);
        }
        
        private void OnDocumentViewSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var newItem = e.NewValue as TreeViewItemViewModel;
            if (this.viewModel != null)
            {
                this.viewModel.SelectedItem = newItem;
            }
        }

        private void OnToolBarLoaded(object sender, RoutedEventArgs e)
        {
            // This removes overflow icon at the right (see https://stackoverflow.com/questions/1050953)
            var toolBar = sender as ToolBar;
            if (toolBar?.Template.FindName("OverflowGrid", toolBar) is FrameworkElement overflowGrid)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
            if (toolBar?.Template.FindName("MainPanelBorder", toolBar) is FrameworkElement mainPanelBorder)
            {
                mainPanelBorder.Margin = new Thickness();
            }
        }
    }
}