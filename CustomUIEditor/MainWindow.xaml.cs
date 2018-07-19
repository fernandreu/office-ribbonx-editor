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
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Remoting.Channels;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Xml;
    using System.Xml.Schema;

    using Data;
    using Microsoft.Win32;
    using Model;

    using RichTextBox = Xceed.Wpf.Toolkit.RichTextBox;

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
        
        private Hashtable customUiSchemas;

        private string statusMessage;

        /// <summary>
        /// Used during the XML validation to flag whether there was any error during the process
        /// </summary>
        private bool hasXmlError;

        #endregion // Fields

        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;

            var applicationFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.LoadXmlSchemas(applicationFolder + @"\Schemas\");

            this.RecentFileList.MenuClick += (sender, args) => this.FinishOpeningFile(args.Filepath);
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
            set => this.SetField(ref this.reloadOnSave, value, this.PropertyChanged, nameof(this.ReloadOnSave));
        }

        /// <summary>
        /// Gets or sets a value indicating the message that will appear at the bottom-left corner of the main window
        /// </summary>
        public string StatusMessage
        {
            get => this.statusMessage;
            set => this.SetField(ref this.statusMessage, value, this.PropertyChanged, nameof(this.StatusMessage));
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

            ofd.FileOk += (sender, e) => this.FinishOpeningFile(((OpenFileDialog)sender).FileName);
            ofd.ShowDialog(this);
        }

        private void FinishOpeningFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            try
            {
                Debug.WriteLine("Opening " + fileName + "...");

                var doc = new OfficeDocument(fileName);
                var model = new OfficeDocumentViewModel(doc);
                if (model.Children.Count > 0)
                {
                    model.Children[0].IsSelected = true;
                }

                this.DocumentList.Add(model);
                this.RecentFileList.InsertFile(fileName);
                
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
            
            var filters = new List<string>();
            for (;;)
            {
                var filter = StringsResource.ResourceManager.GetString("idsFilterSaveAs" + filters.Count);
                if (filter == null)
                {
                    break;
                }

                filters.Add(filter);
            }

            filters.Add(StringsResource.idsFilterAllFiles);

            var sfd = new SaveFileDialog { Title = StringsResource.idsSaveDocumentAsDialogTitle, Filter = string.Join("|", filters), FileName = doc.Name };

            var ext = Path.GetExtension(doc.Name);

            // Find the appropriate FilterIndex
            int i;
            for (i = 0; i < filters.Count - 1; i++)
            {
                // -1 to exclude all files
                var otherExt = filters[i].Split('|')[1].Substring(1);
                if (ext == otherExt)
                {
                    break;
                }
            }

            sfd.FilterIndex = i + 1;

            sfd.FileOk += (sender, args) => this.FinishSavingFile(((SaveFileDialog)sender).FileName);
            sfd.ShowDialog(this);
        }

        private void FinishSavingFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            try
            {
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
                this.RecentFileList.InsertFile(fileName);
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
                    var result = MessageBox.Show(string.Format(StringsResource.idsCloseWarningMessage, doc.Name), StringsResource.idsCloseWarningTitle, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
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

        private void InsertXml14Click(object sender, RoutedEventArgs e)
        {
            var doc = this.CurrentDocument;
            doc?.InsertPart(XmlParts.RibbonX14);
        }

        private void InsertXml12Click(object sender, RoutedEventArgs e)
        {
            var doc = this.CurrentDocument;
            doc?.InsertPart(XmlParts.RibbonX12);
        }

        private void LoadXmlSchemas(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                Debug.Print("folderName is null / empty");
                return;
            }

            try
            {
                var schemas = Directory.GetFiles(folderName, "CustomUI*.xsd");

                if (schemas.Length == 0)
                {
                    return;
                }

                this.customUiSchemas = new Hashtable(schemas.Length);

                foreach (var schema in schemas)
                {
                    var partType = schema.Contains("14") ? XmlParts.RibbonX14 : XmlParts.RibbonX12;
                    var reader = new StreamReader(schema);
                    this.customUiSchemas.Add(partType, XmlSchema.Read(reader, null));

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }

        private void ValidateXmlClick(object sender, RoutedEventArgs e)
        {
            this.ValidateXml(true);
        }

        private bool ValidateXml(bool showValidMessage)
        {
            if (!(this.DocumentView.SelectedItem is OfficePartViewModel part))
            {
                return false;
            }
            
            // Test to see if text is XML first
            try
            {
                var xmlDoc = new XmlDocument();

                if (!(this.customUiSchemas[part.Part.PartType] is XmlSchema targetSchema))
                {
                    return false;
                }

                xmlDoc.Schemas.Add(targetSchema);

                xmlDoc.LoadXml(part.Contents);

                if (xmlDoc.DocumentElement == null)
                {
                    // TODO: ShowError call with an actual message perhaps? Will this ever be null
                    return false;
                }

                if (xmlDoc.DocumentElement.NamespaceURI != targetSchema.TargetNamespace)
                {
                    var errorText = new StringBuilder();
                    errorText.Append(string.Format(StringsResource.idsUnknownNamespace, xmlDoc.DocumentElement.NamespaceURI));
                    errorText.Append("\n" + string.Format(StringsResource.idsCustomUINamespace, targetSchema.TargetNamespace));

                    this.ShowError(errorText.ToString());
                    return false;
                }

                this.hasXmlError = false;
                xmlDoc.Validate(this.XmlValidationEventHandler);
            }
            catch (XmlException ex)
            {
                this.ShowError(StringsResource.idsInvalidXml + "\n" + ex.Message);
                return false;
            }
            
            if (!this.hasXmlError)
            {
                if (showValidMessage)
                {
                    MessageBox.Show(
                        this,
                        StringsResource.idsValidXml,
                        this.Title,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                return true;
            }

            return false;
        }

        private void XmlValidationEventHandler(object sender, ValidationEventArgs e)
        {
            lock (this)
            {
                this.hasXmlError = true;
            }

            MessageBox.Show(
                this,
                e.Message,
                e.Severity.ToString(),
                MessageBoxButton.OK,
                e.Severity == XmlSeverityType.Error ? MessageBoxImage.Error : MessageBoxImage.Warning);
        }
        
        private void ShowError(string errorText)
        {
            Debug.Assert(!string.IsNullOrEmpty(errorText), "Error message is empty");

            MessageBox.Show(
                this,
                errorText,
                this.Title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private void EditorSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!(sender is RichTextBox editor))
            {
                Debug.Print("This should only be used with a RichTextBox");
                return;
            }
            
            var txt = editor.Text;

            if (string.IsNullOrEmpty(txt))
            {
                this.LineBox.Text = "Ln 0  Col 0";
                return;
            }

            editor.CaretPosition.GetLineStartPosition(-int.MaxValue, out var lineCount);
            var colCount = editor.CaretPosition.GetLineStartPosition(0)?.GetOffsetToPosition(editor.CaretPosition) ?? 0;
            ////if (lineCount == 0)
            ////{
            ////    colCount--;
            ////}
            
            this.LineBox.Text = $"Ln {-lineCount + 1},  Col {colCount}";
        }

        private void GenerateCallbacks(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This feature is not implemented yet");
        }
    }
}