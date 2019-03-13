// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindowViewModel.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the MainWindowViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.ViewModels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Windows;
    using System.Xml;
    using System.Xml.Schema;

    using CustomUIEditor.Extensions;
    using CustomUIEditor.Models;
    using CustomUIEditor.Resources;
    using CustomUIEditor.Services;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;

    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IMessageBoxService messageBoxService;

        private readonly IFileDialogService fileDialogService;

        /// <summary>
        /// Whether documents should be reloaded right before being saved.
        /// </summary>
        private bool reloadOnSave = true;

        /// <summary>
        /// Whether the editor should make the whitespace / EOL characters visible.
        /// </summary>
        private bool showWhitespaces = false;

        private Hashtable customUiSchemas;

        private TreeViewItemViewModel selectedItem = null;

        /// <summary>
        /// Used during the XML validation to flag whether there was any error during the process
        /// </summary>
        private bool hasXmlError;

        public MainWindowViewModel(IMessageBoxService messageBoxService, IFileDialogService fileDialogService)
        {
            this.messageBoxService = messageBoxService;
            this.fileDialogService = fileDialogService;

            this.OpenCommand = new RelayCommand(this.OpenFile);
            this.SaveCommand = new RelayCommand(this.Save);
            this.SaveAllCommand = new RelayCommand(this.SaveAll);
            this.SaveAsCommand = new RelayCommand(this.SaveAs);
            this.CloseCommand = new RelayCommand(this.CloseDocument);
            this.InsertXml14Command = new RelayCommand(() => this.CurrentDocument?.InsertPart(XmlParts.RibbonX14));
            this.InsertXml12Command = new RelayCommand(() => this.CurrentDocument?.InsertPart(XmlParts.RibbonX12));
            this.InsertXmlSampleCommand = new RelayCommand<string>(this.InsertXmlSample);
            this.InsertIconsCommand = new RelayCommand(this.InsertIcons);
            this.ChangeIconIdCommand = new RelayCommand(this.ChangeIconId);
            this.ToggleCommentCommand = new RelayCommand(this.ToggleComment);
            this.RemoveCommand = new RelayCommand(this.RemoveItem);
            this.ValidateCommand = new RelayCommand(() => this.ValidateXml(true));
            this.GenerateCallbacksCommand = new RelayCommand(this.GenerateCallbacks);
            this.ShowSettingsCommand = new RelayCommand(() => this.ShowSettings?.Invoke(this, EventArgs.Empty));
            this.RecentFileClickCommand = new RelayCommand<string>(this.FinishOpeningFile);
            this.ClosingCommand = new RelayCommand<CancelEventArgs>(this.QueryClose);
            
#if DEBUG
            if (this.IsInDesignMode)
            {
                return;
            }
#endif
            this.LoadXmlSchemas();
            this.LoadXmlSamples();
        }

        public event EventHandler ShowSettings;


        public event EventHandler<DataEventArgs<string>> ShowCallbacks;

        /// <summary>
        /// This event will be fired when the contents of the editor need to be updated
        /// </summary>
        public event EventHandler<EditorChangeEventArgs> UpdateEditor;

        /// <summary>
        /// This event will be fired when the styling of the editor needs to be updated
        /// </summary>
        public event EventHandler UpdateLexer;

        /// <summary>
        /// This event will be fired when a file needs to be added to the recent list. The argument will be the path to the file itself.
        /// </summary>
        public event EventHandler<DataEventArgs<string>> InsertRecentFile;

        /// <summary>
        /// This event will be fired whenever key editor properties (including current text and selection) need to be known. It is the
        /// listener who will need to specify the argument.
        /// </summary>
        public event EventHandler<DataEventArgs<EditorInfo>> ReadEditorInfo;
        
        public ObservableCollection<OfficeDocumentViewModel> DocumentList { get; } = new ObservableCollection<OfficeDocumentViewModel>();

        public ObservableCollection<XmlSampleViewModel> XmlSamples { get; } = new ObservableCollection<XmlSampleViewModel>();

        /// <summary>
        /// Gets or sets a value indicating whether documents should be reloaded right before being saved.
        /// </summary>
        public bool ReloadOnSave
        {
            get => this.reloadOnSave;
            set => this.Set(ref this.reloadOnSave, value);
        }

        public bool ShowWhitespaces
        {
            get => this.showWhitespaces;
            set
            {
                if (!this.Set(ref this.showWhitespaces, value))
                {
                    return;
                }

                Properties.Settings.Default.ShowWhitespace = value;
                this.UpdateLexer?.Invoke(this, EventArgs.Empty);
            }
        }

        public TreeViewItemViewModel SelectedItem
        {
            get => this.selectedItem;
            set
            {
                this.ApplyCurrentText();
                if (!this.Set(ref this.selectedItem, value))
                {
                    return;
                }

                if (this.SelectedItem != null)
                {
                    this.SelectedItem.IsSelected = true;
                }

                this.RaisePropertyChanged(nameof(this.CurrentDocument));
                this.RaisePropertyChanged(nameof(this.IsDocumentSelected));
                this.RaisePropertyChanged(nameof(this.IsPartSelected));
                this.RaisePropertyChanged(nameof(this.IsIconSelected));
                this.RaisePropertyChanged(nameof(this.CanInsertXml12Part));
                this.RaisePropertyChanged(nameof(this.CanInsertXml14Part));
            }
        }

        public bool IsDocumentSelected => this.SelectedItem is OfficeDocumentViewModel;

        public bool IsPartSelected => this.SelectedItem is OfficePartViewModel;

        public bool IsIconSelected => this.SelectedItem is IconViewModel;
        
        public bool CanInsertXml12Part => (this.SelectedItem is OfficeDocumentViewModel model) && model.Document.RetrieveCustomPart(XmlParts.RibbonX12) == null;

        public bool CanInsertXml14Part => (this.SelectedItem is OfficeDocumentViewModel model) && model.Document.RetrieveCustomPart(XmlParts.RibbonX14) == null;

        public RelayCommand OpenCommand { get; }

        public RelayCommand SaveCommand { get; }

        public RelayCommand SaveAllCommand { get; }

        public RelayCommand SaveAsCommand { get; }

        /// <summary>
        /// Gets the command that triggers the closing of a single document
        /// </summary>
        public RelayCommand CloseCommand { get; }

        public RelayCommand InsertXml14Command { get; }
        
        public RelayCommand InsertXml12Command { get; }

        public RelayCommand<string> InsertXmlSampleCommand { get; set; }

        public RelayCommand InsertIconsCommand { get; }

        public RelayCommand ChangeIconIdCommand { get; }

        public RelayCommand ToggleCommentCommand { get; }

        public RelayCommand RemoveCommand { get; }

        public RelayCommand ValidateCommand { get; }

        public RelayCommand ShowSettingsCommand { get; }

        public RelayCommand GenerateCallbacksCommand { get; }

        public RelayCommand<string> RecentFileClickCommand { get; }

        /// <summary>
        /// Gets the command that triggers the (cancellable) closing of the entire application
        /// </summary>
        public RelayCommand<CancelEventArgs> ClosingCommand { get; }

        public RelayCommand<string> OpenHelpLinkCommand { get; } = new RelayCommand<string>(url => Process.Start(url));

        /// <summary>
        /// Gets a list of headers which will be shown in the "Useful links" menu, together with the links they point to
        /// </summary>
        public IDictionary<string, string> HelpLinks { get; } = new Dictionary<string, string>
                                                                    {
                                                                        { "Customize the Ribbon", "https://msdn.microsoft.com/en-us/library/aa338202(v=office.14).aspx" },
                                                                        { "Customize the Backstage", "https://msdn.microsoft.com/en-us/library/ee691833(office.14).aspx" },
                                                                        { "Repurposing built-in commands", "https://blogs.technet.microsoft.com/the_microsoft_excel_support_team_blog/2012/06/18/how-to-repurpose-a-button-in-excel-2007-or-2010/" },
                                                                        { "Office Fluent UI Command Identifiers", "https://github.com/OfficeDev/office-fluent-ui-command-identifiers" },
                                                                        { "Creating Office add-ins using Visual Studio (VSTO)", "https://msdn.microsoft.com/en-us/library/jj620922.aspx" },
                                                                        { "Office Dev Center", "https://dev.office.com/" },
                                                                        { "BERT | ImageMSO List Reference", "https://bert-toolkit.com/imagemso-list.html" },
                                                                    };

        /// <summary>
        /// Gets the View model of the OfficeDocument currently active (selected) on the application
        /// </summary>
        public OfficeDocumentViewModel CurrentDocument
        {
            get
            {
                // Get currently active document
                if (!(this.SelectedItem is TreeViewItemViewModel elem))
                {
                    return null;
                }

                // Find the root document
                if (elem is IconViewModel)
                {
                    return (OfficeDocumentViewModel)elem.Parent.Parent;
                }

                if (elem is OfficePartViewModel)
                {
                    return (OfficeDocumentViewModel)elem.Parent;
                }

                if (elem is OfficeDocumentViewModel)
                {
                    return (OfficeDocumentViewModel)elem;
                }

                return null;
            }
        }

        private void CloseDocument()
        {
            var doc = this.CurrentDocument;
            if (doc == null)
            {
                // Nothing to close
                return;
            }

            if (doc.HasUnsavedChanges)
            {
                var result = this.messageBoxService.Show(string.Format(StringsResource.idsCloseWarningMessage, doc.Name), StringsResource.idsCloseWarningTitle, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    this.SaveCommand.Execute();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            doc.Document.Dispose();
            this.DocumentList.Remove(doc);
        }

        private void ApplyCurrentText()
        {
            if (this.SelectedItem == null || !this.SelectedItem.CanHaveContents)
            {
                return;
            }

            var e = new DataEventArgs<EditorInfo>();
            this.ReadEditorInfo?.Invoke(this, e);
            if (e.Data == null)
            {
                // This means that event handler was not listened by any view, or the view did not pass the editor contents back for some reason
                return;
            }
            
            this.SelectedItem.Contents = e.Data.Text;
        }

        private void InsertIcons()
        {
            if (!(this.SelectedItem is OfficePartViewModel))
            {
                return;
            }

            this.fileDialogService.OpenFilesDialog(StringsResource.idsInsertIconsDialogTitle, StringsResource.idsFilterAllSupportedImages + "|" + StringsResource.idsFilterAllFiles, this.FinishInsertingIcons);
        }

        /// <summary>
        /// This method does not change the icon Id per se, just enables the possibility of doing so in the view
        /// </summary>
        private void ChangeIconId()
        {
            if (!(this.SelectedItem is IconViewModel icon))
            {
                return;
            }

            icon.IsEditingId = true;
        }

        private void FinishInsertingIcons(IEnumerable<string> filePaths)
        {
            if (!(this.SelectedItem is OfficePartViewModel part))
            {
                // If OpenFileDialog opens modally, this should not happen
                return;
            }

            foreach (var path in filePaths)
            {
                part.InsertIcon(path);
            }
        }

        private void RemoveItem()
        {
            if (this.SelectedItem is OfficePartViewModel)
            {
                var result = this.messageBoxService.Show(
                    "This action cannot be undone. Are you sure you want to continue?", 
                    "Remove XML part", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                var part = (OfficePartViewModel)this.SelectedItem;
                var doc = (OfficeDocumentViewModel)part.Parent;
                doc.RemovePart(part.Part.PartType);
                return;
            }

            if (this.SelectedItem is IconViewModel icon)
            {
                var result = this.messageBoxService.Show(
                    "This action cannot be undone. Are you sure you want to continue?", 
                    "Remove Icon", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                var part = (OfficePartViewModel)icon.Parent;
                part.RemoveIcon(icon.Id);
            }
        }

        private void QueryClose(CancelEventArgs e)
        {
            this.ApplyCurrentText();
            foreach (var doc in this.DocumentList)
            {
                if (doc.HasUnsavedChanges)
                {
                    var result = this.messageBoxService.Show(
                        string.Format(StringsResource.idsCloseWarningMessage, doc.Name), 
                        StringsResource.idsCloseWarningTitle,
                        MessageBoxButton.YesNoCancel, 
                        MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        this.SaveCommand.Execute();
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

        private void OpenFile()
        {
            string[] filters =
                {
                    StringsResource.idsFilterAllOfficeDocuments,
                    StringsResource.idsFilterWordDocuments,
                    StringsResource.idsFilterExcelDocuments,
                    StringsResource.idsFilterPPTDocuments,
                    StringsResource.idsFilterAllFiles,
                };

            this.fileDialogService.OpenFileDialog(
                StringsResource.idsOpenDocumentDialogTitle, 
                string.Join("|", filters), 
                this.FinishOpeningFile);
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
                this.InsertRecentFile?.Invoke(this, new DataEventArgs<string> { Data = fileName });
                
                // UndoRedo
                ////_commands = new UndoRedo.Control.Commands(rtbCustomUI.Rtf);
            }
            catch (Exception ex)
            {
                this.messageBoxService.Show(ex.Message, "Error opening Office document", image: MessageBoxImage.Error);
            }
        }

        private void Save()
        {
            this.ApplyCurrentText();

            if (this.CurrentDocument == null)
            {
                return;
            }

            try
            {
                this.CurrentDocument.Save(this.ReloadOnSave);
            }
            catch (IOException ex)
            {
                this.messageBoxService.Show(ex.Message, "Error saving Office document", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAll()
        {
            this.ApplyCurrentText();
            foreach (var doc in this.DocumentList)
            {
                doc.Save(this.ReloadOnSave);
            }
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

            this.fileDialogService.SaveFileDialog(
                StringsResource.idsSaveDocumentAsDialogTitle, 
                string.Join("|", filters),
                this.FinishSavingFile, 
                doc.Name, 
                i + 1);
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
                this.InsertRecentFile?.Invoke(this, new DataEventArgs<string> { Data = fileName });
            }
            catch (Exception ex)
            {
                this.messageBoxService.Show(ex.Message, "Error saving Office document", image: MessageBoxImage.Error);
            }
        }

        private void LoadXmlSchemas()
        {
            try
            {
                this.customUiSchemas = new Hashtable(2);

                using (var reader = new StringReader(SchemasResource.customUI))
                {
                    this.customUiSchemas.Add(XmlParts.RibbonX12, XmlSchema.Read(reader, null));
                }
                    
                using (var reader = new StringReader(SchemasResource.customui14))
                {
                    this.customUiSchemas.Add(XmlParts.RibbonX14, XmlSchema.Read(reader, null));
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }

        private void LoadXmlSamples()
        {
            foreach (var sample in XmlSampleViewModel.GetFromAssembly())
            {
                this.XmlSamples.Add(sample);
            }
        }

        private void InsertXmlSample(string resourceName)
        {
            Debug.Assert(!string.IsNullOrEmpty(resourceName), "resourceName not passed");

            var newPart = false;

            if (this.SelectedItem is OfficeDocumentViewModel doc)
            {
                // See if there is already a part, and otherwise insert one
                if (doc.Children.Count == 0)
                {
                    doc.InsertPart(XmlParts.RibbonX12);
                    newPart = true;
                }

                this.SelectedItem = doc.Children[0];
            }
            
            if (!(this.SelectedItem is OfficePartViewModel part))
            {
                return;
            }
            
            // Show message box for confirmation
            if (!newPart)
            {
                var result = this.messageBoxService.Show(
                    "This will replace the contents of the current part. Are you sure you want to continue?", 
                    "Insert XML Sample", 
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }
                
            try
            {
                var data = XmlSampleViewModel.ReadContents(resourceName);
                part.Contents = data;

                // TODO: This should be automatically raised by the ViewModel when setting the part contents
                this.UpdateEditor?.Invoke(this, new EditorChangeEventArgs { Start = -1, End = -1, NewText = data });
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
                this.messageBoxService.Show(ex.Message, "Error inserting XML sample");
            }
        }

        private bool ValidateXml(bool showValidMessage)
        {
            if (!(this.SelectedItem is OfficePartViewModel part))
            {
                return false;
            }
            
            this.ApplyCurrentText();

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

                    this.messageBoxService.Show(errorText.ToString(), "Error validating XML", image: MessageBoxImage.Error);
                    return false;
                }

                this.hasXmlError = false;
                xmlDoc.Validate(this.XmlValidationEventHandler);
            }
            catch (XmlException ex)
            {
                this.messageBoxService.Show(StringsResource.idsInvalidXml + "\n" + ex.Message, "Error validating XML", image: MessageBoxImage.Error);
                return false;
            }
            
            if (!this.hasXmlError)
            {
                if (showValidMessage)
                {
                    this.messageBoxService.Show(
                        StringsResource.idsValidXml,
                        "XML is valid",
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

            this.messageBoxService.Show(
                e.Message,
                e.Severity.ToString(),
                MessageBoxButton.OK,
                e.Severity == XmlSeverityType.Error ? MessageBoxImage.Error : MessageBoxImage.Warning);
        }

        private void GenerateCallbacks()
        {
            // TODO: Check whether any text is selected, and generate callbacks only for that text
            this.ApplyCurrentText();

            if (!(this.SelectedItem is OfficePartViewModel part))
            {
                return;
            }

            try
            {
                var customUi = new XmlDocument();

                customUi.LoadXml(part.Contents);

                var callbacks = CallbacksBuilder.GenerateCallback(customUi);
                if (callbacks == null || callbacks.Length == 0)
                {
                    this.messageBoxService.Show(StringsResource.idsNoCallback, "Generate Callbacks", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                this.ShowCallbacks?.Invoke(this, new DataEventArgs<string> { Data = callbacks.ToString() });
            }
            catch (Exception ex)
            {
                this.messageBoxService.Show(ex.Message, "Error Generating Callbacks", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleComment()
        {
            var e = new DataEventArgs<EditorInfo>();
            this.ReadEditorInfo?.Invoke(this, e);

            if (e.Data == null)
            {
                // Nothing is probably listening to this event
                return;
            }
            
            // Extend the selection to pick full lines
            const string NewLine = "\n";
            var start = e.Data.Text.LastIndexOf(NewLine, e.Data.Selection.Item1, StringComparison.Ordinal) + 1;

            var end = e.Data.Text.IndexOf(NewLine, e.Data.Selection.Item2, StringComparison.Ordinal);
            if (end < 0)
            {
                end = e.Data.Text.Length;
            }

            // TODO: Use a StringBuilder
            var lines = e.Data.Text.Substring(start, end - start).Split(new[] { NewLine }, StringSplitOptions.None);
            for (var i = 0; i < lines.Length; ++i)
            {
                var trimmed = lines[i].Trim();
                if (trimmed.Length == 0)
                {
                    // Leave blank lines untouched
                    continue;
                }

                var index = lines[i].IndexOf(trimmed, StringComparison.Ordinal);
                if (trimmed.StartsWith("<!--") && trimmed.EndsWith("-->"))
                {
                    // Remove the comment characters
                    lines[i] = lines[i].Substring(0, index) + trimmed.Substring(4, trimmed.Length - 7) + lines[i].Substring(index + trimmed.Length);
                }
                else
                {
                    // Add the comment characters
                    lines[i] = lines[i].Substring(0, index) + "<!--" + trimmed + "-->" + lines[i].Substring(index + trimmed.Length);
                }
            }

            // Combine the lines and put them back
            var combined = string.Join(NewLine, lines);
            var result = e.Data.Text.Substring(0, start) + combined + e.Data.Text.Substring(end);

            // Update the selected item's current contents to that, and notify the editor
            this.SelectedItem.Contents = result;
            this.UpdateEditor?.Invoke(this, new EditorChangeEventArgs { Start = start, End = end, NewText = combined, UpdateSelection = true });
        }
    }
}
