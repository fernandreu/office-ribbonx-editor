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
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Input;
    using System.Xml;
    using System.Xml.Schema;

    using CustomUIEditor.Data;
    using CustomUIEditor.Extensions;
    using CustomUIEditor.Views;

    using Microsoft.Win32;

    using Prism.Commands;
    using Prism.Mvvm;

    public class MainWindowViewModel : BindableBase
    {
        /// <summary>
        /// Whether documents should be reloaded right before being saved.
        /// </summary>
        private bool reloadOnSave = true;
        
        private Hashtable customUiSchemas;

        private string statusMessage;

        private TreeViewItemViewModel selectedItem = null;

        /// <summary>
        /// Used during the XML validation to flag whether there was any error during the process
        /// </summary>
        private bool hasXmlError;

        public MainWindowViewModel()
        {
            this.OpenCommand = new DelegateCommand(this.OpenFile);
            this.SaveCommand = new DelegateCommand(this.Save);
            this.SaveAllCommand = new DelegateCommand(this.SaveAll);
            this.SaveAsCommand = new DelegateCommand(this.SaveAs);
            this.InsertXml14Command = new DelegateCommand(() => this.CurrentDocument?.InsertPart(XmlParts.RibbonX14));
            this.InsertXml12Command = new DelegateCommand(() => this.CurrentDocument?.InsertPart(XmlParts.RibbonX12));
            this.ValidateCommand = new DelegateCommand(() => this.ValidateXml(true));

            var applicationFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }
#endif
            this.LoadXmlSchemas(applicationFolder + @"\Schemas\");
        }
        
        public MainWindow View { get; set; }

        public ObservableCollection<OfficeDocumentViewModel> DocumentList { get; } = new ObservableCollection<OfficeDocumentViewModel>();

        /// <summary>
        /// Gets or sets a value indicating whether documents should be reloaded right before being saved.
        /// </summary>
        public bool ReloadOnSave
        {
            get => this.reloadOnSave;
            set => this.SetProperty(ref this.reloadOnSave, value);
        }

        /// <summary>
        /// Gets or sets a value indicating the message that will appear at the bottom-left corner of the main window
        /// </summary>
        public string StatusMessage
        {
            get => this.statusMessage;
            set => this.SetProperty(ref this.statusMessage, value);
        }

        public TreeViewItemViewModel SelectedItem
        {
            get => this.selectedItem;
            set => this.SetProperty(ref this.selectedItem, value, () => this.RaisePropertyChanged(nameof(this.CurrentDocument)));
        }

        public DelegateCommand OpenCommand { get; set; }

        public DelegateCommand SaveCommand { get; set; }

        public DelegateCommand SaveAllCommand { get; set; }

        public DelegateCommand SaveAsCommand { get; set; }

        public DelegateCommand InsertXml14Command { get; set; }
        
        public DelegateCommand InsertXml12Command { get; set; }

        public DelegateCommand ValidateCommand { get; set; }

        /// <summary>
        /// Gets the View model of the OfficeDocument currently active (selected) on the application
        /// </summary>
        private OfficeDocumentViewModel CurrentDocument
        {
            get
            {
                // Get currently active document
                if (!(this.SelectedItem is TreeViewItemViewModel elem))
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
            ofd.ShowDialog(this.View);
        }

        public void FinishOpeningFile(string fileName)
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
                this.View.RecentFileList.InsertFile(fileName);  // TODO: Call from View
                
                // UndoRedo
                ////_commands = new UndoRedo.Control.Commands(rtbCustomUI.Rtf);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error opening Office document");
            }
        }

        private void Save()
        {
            this.CurrentDocument?.Save(this.ReloadOnSave);
        }

        private void SaveAll()
        {
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
            sfd.ShowDialog(this.View);
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
                this.View.RecentFileList.InsertFile(fileName);  // TODO: Call from View
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error saving Office document");
            }
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
        
        private bool ValidateXml(bool showValidMessage)
        {
            if (!(this.SelectedItem is OfficePartViewModel part))
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

                    this.View.ShowError(errorText.ToString());  // TODO: Call from View
                    return false;
                }

                this.hasXmlError = false;
                xmlDoc.Validate(this.XmlValidationEventHandler);
            }
            catch (XmlException ex)
            {
                this.View.ShowError(StringsResource.idsInvalidXml + "\n" + ex.Message);  // TODO: Call from View
                return false;
            }
            
            if (!this.hasXmlError)
            {
                if (showValidMessage)
                {
                    MessageBox.Show(
                        this.View,
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

            MessageBox.Show(
                this.View,
                e.Message,
                e.Severity.ToString(),
                MessageBoxButton.OK,
                e.Severity == XmlSeverityType.Error ? MessageBoxImage.Error : MessageBoxImage.Warning);
        }
    }
}
