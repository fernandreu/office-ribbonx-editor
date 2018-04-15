// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Fernando Andreu">
//   Fernando Andreu
// </copyright>
// <summary>
//   Interaction logic for MainWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;

    using CustomUIEditor.Data;
    using CustomUIEditor.Model;

    using Microsoft.Win32;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Whether documents should be reloaded right before being saved.
        /// </summary>
        private bool reloadOnSave = true;
        
        private bool suppressRequestBringIntoView;

        #endregion // Fields

        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties

        public ObservableCollection<OfficeDocumentViewModel> DocumentList { get; } = new ObservableCollection<OfficeDocumentViewModel>();

        /// <summary>
        /// Gets or sets a value indicating whether documents should be reloaded right before being saved.
        /// </summary>
        public bool ReloadOnSave
        {
            get => this.reloadOnSave;
            set => this.SetField(ref this.reloadOnSave, value, nameof(this.ReloadOnSave), this.PropertyChanged);
        }

        /// <summary>
        /// Gets the view model of the OfficeDocument currently active (selected) on the application
        /// </summary>
        private OfficeDocumentViewModel CurrentDocument
        {
            get
            {
                // Get currently active document
                if (!(this.DocumentView.SelectedItem is TreeViewItemViewModel elem))
                {
                    return null;
                }

                // Find the root document
                if (elem is OfficePartViewModel)
                {
                    return (OfficeDocumentViewModel)elem.Parent;
                }

                if (elem is OfficeDocumentViewModel doc)
                {
                    return doc;
                }

                return null;
            }
        }
        
        #endregion // Properties

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            this.OpenFile();
        }

        private void OpenFile()
        {
            var ofd = new OpenFileDialog();
            
            ofd.Title = StringsResource.idsOpenDocumentDialogTitle;
            string[] filters =
            {
                StringsResource.idsFilterAllOfficeDocuments,
                StringsResource.idsFilterWordDocuments,
                StringsResource.idsFilterExcelDocuments,
                StringsResource.idsFilterPPTDocuments,
                StringsResource.idsFilterAllFiles,
            };
            ofd.Filter = string.Join("|", filters);
            ofd.FilterIndex = 0;
            ofd.RestoreDirectory = true;

            ofd.FileOk += this.FinishOpeningFile;
            ofd.ShowDialog(this);
        }

        private void FinishOpeningFile(object sender, CancelEventArgs e)
        {
            try
            {
                string fileName = ((OpenFileDialog)sender).FileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }

                Debug.WriteLine("Opening " + fileName + "...");

                var doc = new OfficeDocument(fileName);
                var model = new OfficeDocumentViewModel(doc);
                if (model.Children.Count > 0)
                {
                    model.Children[0].IsSelected = true;
                }

                this.DocumentList.Add(model);
                
                // UndoRedo
                ////_commands = new UndoRedo.Control.Commands(rtbCustomUI.Rtf);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title);
            }
        }

        private void SaveAsClick(object sender, RoutedEventArgs e)
        {
            this.SaveAs();
        }

        private void SaveAs()
        {
            var doc = this.CurrentDocument;
            if (doc == null)
            {
                return;
            }

            var sfd = new SaveFileDialog();
            
            sfd.Title = "Save OOXML Document";
            string[] filters =
            {
                "Excel Workbook|*.xlsx",
                "Excel Macro-Enabled Workbook|*.xlsm",
                "Excel Binary Workbook|*.xlsb",
                "Excel Template|*.xltx",
                "Excel Macro-Enabled Template|*.xltm",
                "Excel Add-in|*.xlam",
                "PowerPoint Presentation|*.pptx",
                "PowerPoint Macro-Enabled Presentation|*.pptm",
                "PowerPoint Template|*.potx",
                "PowerPoint Macro-Enabled Template|*.potm",
                "PowerPoint Show|*.ppsx",
                "PowerPoint Macro-Enabled Show|*.ppsm",
                "PowerPoint Add-in|*.ppam",
                "Word Document|*.docx",
                "Word Macro-Enabled Document|*.docm",
                "Word Template|*.dotx",
                "Word Macro-Enabled Template|*.dotm",
                StringsResource.idsFilterAllFiles,
            };
            sfd.Filter = string.Join("|", filters);
            sfd.FileName = doc.Name;

            var ext = Path.GetExtension(doc.Name);

            // Find the appropriate FilterIndex
            int i;
            for (i = 0; i < filters.Length - 1; i++)
            {
                // -1 to exclude all files
                var otherExt = filters[i].Split('|')[1].Substring(1);
                if (ext == otherExt)
                {
                    break;
                }
            }

            sfd.FilterIndex = i + 1;

            sfd.FileOk += this.FinishSavingFile;
            sfd.ShowDialog(this);
        }

        private void FinishSavingFile(object sender, CancelEventArgs e)
        {
            try
            {
                string fileName = ((SaveFileDialog)sender).FileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }

                // Note: We are assuming that no UI events happen between the SaveFileDialog was
                // shown and this is called. Otherwise, selection might have changed
                var doc = this.CurrentDocument;
                Debug.Assert(doc != null, "Selected document seems to have changed between showing file dialog and closing it");

                if (!Path.HasExtension(fileName))
                {
                    fileName = Path.ChangeExtension(fileName, Path.GetExtension(doc.Name));
                }

                Debug.WriteLine("Saving " + fileName + "...");

                doc.Save(this.reloadOnSave, fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title);
            }
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

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            this.CurrentDocument?.Save(this.ReloadOnSave);
        }

        private void SaveAllClick(object sender, RoutedEventArgs e)
        {
            foreach (var doc in this.DocumentList)
            {
                doc.Save(this.reloadOnSave);
            }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            foreach (var doc in this.DocumentList)
            {
                if (doc.HasUnsavedChanges)
                {
                    var result = MessageBox.Show($"File '{doc.Name}' has unsaved changes. Do you want to save them before existing the program?", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        doc.Save(this.reloadOnSave);
                    }
                    else if (result == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

            // Now that it is clear we can leave the program, dispose all documents (i.e. delete the temporary unzipped files)
            foreach (var doc in this.DocumentList)
            {
                doc.Document.Dispose();
            }
        }
    }
}