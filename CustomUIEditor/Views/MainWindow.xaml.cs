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

    using Data;

    using GalaSoft.MvvmLight.CommandWpf;

    using ScintillaNET;

    using ViewModels;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow
    {
        private readonly MainWindowViewModel viewModel;

        private bool suppressRequestBringIntoView;
        
        private int maxLineNumberCharLength;

        public MainWindow()
        {
            // To keep things simpler, commands need to be defined before calling InitializeComponent.
            // Otherwise, all command bindings will be set to null. The alternative is to create a
            // DependencyProperty for each command, but it seems an overkill.
            // Because Editor is still null, an intermediate lambda function needs to be used.
            this.CutCommand = new RelayCommand(() => this.Editor.Cut());
            this.CopyCommand = new RelayCommand(() => this.Editor.Copy());
            this.PasteCommand = new RelayCommand(() => this.Editor.Paste());
            this.UndoCommand = new RelayCommand(() => this.Editor.Undo());
            this.RedoCommand = new RelayCommand(() => this.Editor.Redo());
            this.SelectAllCommand = new RelayCommand(() => this.Editor.SelectAll());

            this.InitializeComponent();

            this.viewModel = (MainWindowViewModel)this.DataContext;

            this.viewModel.ShowSettings += (o, e) => this.ShowSettings();
            this.viewModel.ReadCurrentText += (o, e) => e.Data = this.Editor.Text;
            this.viewModel.InsertRecentFile += (o, e) => this.RecentFileList.InsertFile(e.Data);
            this.viewModel.UpdateEditor += (o, e) => this.Editor.Text = e.Data;

            this.SetScintillaLexer();
        }

        #region View-specific commands

        public ICommand CutCommand { get; }

        public ICommand CopyCommand { get; }

        public ICommand PasteCommand { get; }

        public ICommand UndoCommand { get; }

        public ICommand RedoCommand { get; }

        public ICommand SelectAllCommand { get; }

        #endregion

        public void SetScintillaLexer()
        {
            var scintilla = this.Editor;

            scintilla.TabWidth = Properties.Settings.Default.TabWidth;
            scintilla.WrapMode = Properties.Settings.Default.WrapMode;

            // Set the XML Lexer
            scintilla.Lexer = Lexer.Xml;

            // Show line numbers (this is now done in TextChanged so that width depends on number of digits)
            ////scintilla.Margins[0].Width = 10;

            // Enable folding
            scintilla.SetProperty("fold", "1");
            scintilla.SetProperty("fold.compact", "1");
            scintilla.SetProperty("fold.html", "1");

            // Use Margin 2 for fold markers
            scintilla.Margins[2].Type = MarginType.Symbol;
            scintilla.Margins[2].Mask = Marker.MaskFolders;
            scintilla.Margins[2].Sensitive = true;
            scintilla.Margins[2].Width = 20;

            // Reset folder markers
            for (int i = Marker.FolderEnd; i <= Marker.FolderOpen; i++)
            {
                scintilla.Markers[i].SetForeColor(System.Drawing.SystemColors.ControlLightLight);
                scintilla.Markers[i].SetBackColor(System.Drawing.SystemColors.ControlDark);
            }

            // Style the folder markers
            scintilla.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            scintilla.Markers[Marker.Folder].SetBackColor(System.Drawing.SystemColors.ControlText);
            scintilla.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            scintilla.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            scintilla.Markers[Marker.FolderEnd].SetBackColor(System.Drawing.SystemColors.ControlText);
            scintilla.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            scintilla.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
            scintilla.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            scintilla.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            scintilla.AutomaticFold = AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change;

            // Set the Styles
            scintilla.StyleResetDefault();

            scintilla.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            scintilla.Styles[ScintillaNET.Style.Default].Size = Properties.Settings.Default.EditorFontSize;
            scintilla.Styles[ScintillaNET.Style.Default].ForeColor = Properties.Settings.Default.TextColor;
            scintilla.Styles[ScintillaNET.Style.Default].BackColor = Properties.Settings.Default.BackgroundColor;
            scintilla.StyleClearAll();
            scintilla.Styles[ScintillaNET.Style.Xml.Attribute].ForeColor = Properties.Settings.Default.AttributeColor;
            scintilla.Styles[ScintillaNET.Style.Xml.Entity].ForeColor = Properties.Settings.Default.AttributeColor;
            scintilla.Styles[ScintillaNET.Style.Xml.Comment].ForeColor = Properties.Settings.Default.CommentColor;
            scintilla.Styles[ScintillaNET.Style.Xml.Tag].ForeColor = Properties.Settings.Default.TagColor;
            scintilla.Styles[ScintillaNET.Style.Xml.TagEnd].ForeColor = Properties.Settings.Default.TagColor;
            scintilla.Styles[ScintillaNET.Style.Xml.DoubleString].ForeColor = Properties.Settings.Default.StringColor;
            scintilla.Styles[ScintillaNET.Style.Xml.SingleString].ForeColor = Properties.Settings.Default.StringColor;
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

            // Did the number of characters in the line number display change?
            // i.e. nnn VS nn, or nnnn VS nn, etc...
            var charLength = this.Editor.Lines.Count.ToString().Length;
            if (charLength == this.maxLineNumberCharLength)
            {
                return;
            }

            // Calculate the width required to display the last line number
            // and include some padding for good measure.
            const int LinePadding = 2;

            this.Editor.Margins[0].Width = this.Editor.TextWidth(ScintillaNET.Style.LineNumber, new string('9', charLength + 1)) + LinePadding;
            this.maxLineNumberCharLength = charLength;
        }

        private void ShowSettings()
        {
            var dlg = new SettingsDialog { Owner = this };
            dlg.ShowDialog();
            this.SetScintillaLexer();  // In case settings changed
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