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
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Xml;

    using CustomUIEditor.Extensions;

    using Data;

    using ScintillaNET;

    using ViewModels;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow
    {
        private bool suppressRequestBringIntoView;
        
        private int maxLineNumberCharLength;

        private MainWindowViewModel model;

        public MainWindow()
        {
            this.InitializeComponent();

            this.model = (MainWindowViewModel)this.DataContext;
            this.model.View = this;

            this.SetScintillaLexer();
            
            // TODO: Can a Command be used for the menu click? So that FinishOpeningFile doesn't need to be public
            this.RecentFileList.MenuClick += (sender, args) => this.model.FinishOpeningFile(args.Filepath);
        }

        public void SetScintillaLexer()
        {
            var scintilla = this.Editor;

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
        
        private void ShowHelpCustomizeTheRibbon(object sender, RoutedEventArgs e)
        {
            Process.Start("https://msdn.microsoft.com/en-us/library/aa338202(v=office.14).aspx");
        }

        private void ShowHelpCustomizeTheOustpace(object sender, RoutedEventArgs e)
        {
            Process.Start("https://msdn.microsoft.com/en-us/library/ee691833(office.14).aspx");
        }

        private void ShowHelpCommandIdentifiers(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/OfficeDev/office-fluent-ui-command-identifiers");
        }

        private void ShowHelpVsto(object sender, RoutedEventArgs e)
        {
            Process.Start("https://msdn.microsoft.com/en-us/library/jj620922.aspx");
        }

        private void ShowHelpOfficeDevCenter(object sender, RoutedEventArgs e)
        {
            Process.Start("https://dev.office.com/");
        }

        private void ShowHelpRepurposingControls(object sender, RoutedEventArgs e)
        {
            Process.Start("https://blogs.technet.microsoft.com/the_microsoft_excel_support_team_blog/2012/06/18/how-to-repurpose-a-button-in-excel-2007-or-2010/");
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            foreach (var doc in this.model.DocumentList)
            {
                if (doc.HasUnsavedChanges)
                {
                    var result = MessageBox.Show(string.Format(StringsResource.idsCloseWarningMessage, doc.Name), StringsResource.idsCloseWarningTitle, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        this.model.SaveCommand.Execute();
                    }
                    else if (result == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

            // Now that it is clear we can leave the program, dispose all documents (i.e. delete the temporary unzipped files)
            foreach (var doc in this.model.DocumentList)
            {
                doc.Document.Dispose();
            }
        }

        public void ShowError(string errorText)
        {
            Debug.Assert(!string.IsNullOrEmpty(errorText), "Error message is empty");
            
            MessageBox.Show(
                this,
                errorText,
                this.Title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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
            if (this.model.SelectedItem != null && this.model.SelectedItem.CanHaveContents)
            {
                // If applicable, save the contents currently shown in the editor to that item
                this.model.SelectedItem.Contents = this.Editor.Text;
            }

            this.model.SelectedItem = e.NewValue as TreeViewItemViewModel;

            if (this.model.SelectedItem == null)
            {
                this.Editor.Text = string.Empty;
                return;
            }

            // Load contents of this item
            if (this.model.SelectedItem.CanHaveContents)
            {
                this.Editor.Text = this.model.SelectedItem.Contents;
            }
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

        private void ShowSettings(object sender, RoutedEventArgs e)
        {
            var dlg = new SettingsWindow { Owner = this };
            dlg.ShowDialog();
            this.SetScintillaLexer();  // In case settings changed
        }

        private void EditorZoomChanged(object sender, EventArgs e)
        {
            this.ZoomBox.Value = this.Editor.Zoom;
        }
    }
}