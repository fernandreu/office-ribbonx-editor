﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Dragablz;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Extensions;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Models;
using OfficeRibbonXEditor.Properties;
using OfficeRibbonXEditor.Resources;
using ScintillaNET;

namespace OfficeRibbonXEditor.ViewModels
{
    using ResultsEventArgs = DataEventArgs<IResultCollection>;

    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IMessageBoxService messageBoxService;

        private readonly IFileDialogService fileDialogService;

        private readonly IDialogProvider dialogProvider;

        private readonly Dictionary<Type, IContentDialogBase> dialogs = new Dictionary<Type, IContentDialogBase>();

        /// <summary>
        /// Whether documents should be reloaded right before being saved.
        /// </summary>
        private bool reloadOnSave = true;

        /// <summary>
        /// Whether the editor should make the whitespace / EOL characters visible.
        /// </summary>
        private bool showWhitespaces = false;

        /// <summary>
        /// The version string of a newer release, if available
        /// </summary>
        private string newerVersion = null;

        private Hashtable customUiSchemas;

        private TreeViewItemViewModel selectedItem = null;

        public MainWindowViewModel(IMessageBoxService messageBoxService, IFileDialogService fileDialogService, IVersionChecker versionChecker, IDialogProvider dialogProvider)
        {
            this.messageBoxService = messageBoxService;
            this.fileDialogService = fileDialogService;
            this.dialogProvider = dialogProvider;

            this.OpenDocumentCommand = new RelayCommand(this.ExecuteOpenDocumentCommand);
            this.OpenTabCommand = new RelayCommand<TreeViewItemViewModel>(this.ExecuteOpenTabCommand);
            this.SaveCommand = new RelayCommand(this.ExecuteSaveCommand);
            this.SaveAllCommand = new RelayCommand(this.ExecuteSaveAllCommand);
            this.SaveAsCommand = new RelayCommand(this.ExecuteSaveAsCommand);
            this.CloseDocumentCommand = new RelayCommand(this.ExecuteCloseDocumentCommand);
            this.InsertXml14Command = new RelayCommand(() => this.CurrentDocument?.InsertPart(XmlParts.RibbonX14));
            this.InsertXml12Command = new RelayCommand(() => this.CurrentDocument?.InsertPart(XmlParts.RibbonX12));
            this.InsertXmlSampleCommand = new RelayCommand<string>(this.ExecuteInsertXmlSampleCommand);
            this.InsertIconsCommand = new RelayCommand(this.ExecuteInsertIconsCommand);
            this.ChangeIconIdCommand = new RelayCommand(this.ExecuteChangeIconIdCommand);
            this.ToggleCommentCommand = new RelayCommand(this.ExecuteToggleCommentCommand);
            this.RemoveCommand = new RelayCommand(this.ExecuteRemoveItemCommand);
            this.ValidateCommand = new RelayCommand(() => this.ValidateXml(true));
            this.GenerateCallbacksCommand = new RelayCommand(this.ExecuteGenerateCallbacksCommand);
            this.GoToCommand = new RelayCommand(this.ExecuteGoToCommand);
            this.FindCommand = new RelayCommand(() => this.PerformFindReplaceAction(FindReplaceAction.Find));
            this.FindNextCommand = new RelayCommand(() => this.PerformFindReplaceAction(FindReplaceAction.FindNext));
            this.FindPreviousCommand = new RelayCommand(() => this.PerformFindReplaceAction(FindReplaceAction.FindPrevious));
            this.IncrementalSearchCommand = new RelayCommand(() => this.PerformFindReplaceAction(FindReplaceAction.IncrementalSearch));
            this.ReplaceCommand = new RelayCommand(() => this.PerformFindReplaceAction(FindReplaceAction.Replace));
            
            this.ShowSettingsCommand = new RelayCommand(() => this.LaunchDialog<SettingsDialogViewModel, ICollection<ITabItemViewModel>>(this.OpenTabs));
            this.ShowAboutCommand = new RelayCommand(() => this.LaunchDialog<AboutDialogViewModel>(true));
            this.RecentFileClickCommand = new RelayCommand<string>(this.FinishOpeningFile);
            this.ClosingCommand = new RelayCommand<CancelEventArgs>(this.ExecuteClosingCommand);
            this.CloseCommand = new RelayCommand(this.ExecuteCloseCommand);
            this.CloseTabCommand = new RelayCommand<ITabItemViewModel>(this.ExecuteCloseTabCommand);
            this.PreviewDragEnterCommand = new RelayCommand<DragEventArgs>(this.ExecutePreviewDragCommand);
            this.DropCommand = new RelayCommand<DragEventArgs>(this.ExecuteDropCommand);
            this.NewerVersionCommand = new RelayCommand(this.ExecuteNewerVersionCommand);

#if DEBUG
            if (this.IsInDesignMode)
            {
                return;
            }
#endif
            this.LoadXmlSchemas();
            this.LoadXmlSamples();

            foreach (var file in Environment.GetCommandLineArgs().Skip(1))
            {
                if (!File.Exists(file))
                {
                    continue;
                }

                this.FinishOpeningFile(file);
            }

            this.CheckVersionAsync(versionChecker);
        }

        /// <summary>
        /// This gets raised when there is a closed event originated from the ViewModel (e.g. programmatically)
        /// </summary>
        public event EventHandler Closed;

        public event EventHandler<LaunchDialogEventArgs> LaunchingDialog;

        /// <summary>
        /// This event will be fired when a file needs to be added to the recent list. The argument will be the path to the file itself.
        /// </summary>
        public event EventHandler<DataEventArgs<string>> InsertRecentFile;

        public ObservableCollection<OfficeDocumentViewModel> DocumentList { get; } = new ObservableCollection<OfficeDocumentViewModel>();

        public ObservableCollection<XmlSampleViewModel> XmlSamples { get; } = new ObservableCollection<XmlSampleViewModel>();

        public ObservableCollection<ITabItemViewModel> OpenTabs { get; } = new ObservableCollection<ITabItemViewModel>();

        private ITabItemViewModel selectedTab;

        public ITabItemViewModel SelectedTab
        {
            get => this.selectedTab;
            set
            {
                if (!this.Set(ref this.selectedTab, value))
                {
                    return;
                }

                this.RaisePropertyChanged(nameof(this.IsEditorTabSelected));
            }
        }

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
                foreach (var tab in this.OpenTabs.OfType<EditorTabViewModel>())
                {
                    tab.Lexer?.Update();
                }
            }
        }

        public string NewerVersion
        {
            get => this.newerVersion;
            set => this.Set(ref this.newerVersion, value);
        }

        public TreeViewItemViewModel SelectedItem
        {
            get => this.selectedItem;
            set
            {
                var previousItem = this.selectedItem;
                if (!this.Set(ref this.selectedItem, value))
                {
                    return;
                }

                if (previousItem is IconViewModel icon)
                {
                    // Stop showing the editing textbox when the focus changes to something else.
                    // See: https://github.com/fernandreu/office-ribbonx-editor/issues/32
                    icon.CommitIdChange();
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

        public bool IsEditorTabSelected => this.SelectedTab != null;

        public bool IsIconSelected => this.SelectedItem is IconViewModel;
        
        public bool CanInsertXml12Part => (this.SelectedItem is OfficeDocumentViewModel model) && model.Document.RetrieveCustomPart(XmlParts.RibbonX12) == null;

        public bool CanInsertXml14Part => (this.SelectedItem is OfficeDocumentViewModel model) && model.Document.RetrieveCustomPart(XmlParts.RibbonX14) == null;

        public RelayCommand OpenDocumentCommand { get; }

        public RelayCommand<TreeViewItemViewModel> OpenTabCommand { get; }

        public RelayCommand SaveCommand { get; }

        public RelayCommand SaveAllCommand { get; }

        public RelayCommand SaveAsCommand { get; }

        public RelayCommand CloseDocumentCommand { get; }

        public RelayCommand InsertXml14Command { get; }
        
        public RelayCommand InsertXml12Command { get; }

        public RelayCommand<string> InsertXmlSampleCommand { get; set; }

        public RelayCommand InsertIconsCommand { get; }

        public RelayCommand ChangeIconIdCommand { get; }

        public RelayCommand ToggleCommentCommand { get; }

        public RelayCommand RemoveCommand { get; }

        public RelayCommand ValidateCommand { get; }

        public RelayCommand ShowSettingsCommand { get; }

        public RelayCommand ShowAboutCommand { get; }

        public RelayCommand GenerateCallbacksCommand { get; }

        public RelayCommand GoToCommand { get; }

        public RelayCommand FindCommand { get; }

        public RelayCommand FindNextCommand { get; }

        public RelayCommand FindPreviousCommand { get; }

        public RelayCommand IncrementalSearchCommand { get; }

        public RelayCommand ReplaceCommand { get; }

        public RelayCommand<string> RecentFileClickCommand { get; }

        public RelayCommand NewerVersionCommand { get; }

        /// <summary>
        /// Gets the command that handles the (cancellable) closing of the entire application, getting typically triggered by the view
        /// </summary>
        public RelayCommand<CancelEventArgs> ClosingCommand { get; }

        /// <summary>
        /// Gets the command that triggers the closing of the view. If linked with the view, this will also trigger the ClosingCommand,
        /// and hence no checks of whether documents should be saved first will be done.
        /// </summary>
        public RelayCommand CloseCommand { get; }

        public RelayCommand<ITabItemViewModel> CloseTabCommand { get; }

        /// <summary>
        /// Gets the command that starts the drag / drop action for opening files
        /// </summary>
        public RelayCommand<DragEventArgs> PreviewDragEnterCommand { get; }

        /// <summary>
        /// Gets the command that finishes the drag / drop action for opening files
        /// </summary>
        public RelayCommand<DragEventArgs> DropCommand { get; }

        public RelayCommand<string> OpenHelpLinkCommand { get; } = new RelayCommand<string>(url => Process.Start(url));

        public ItemActionCallback CloseTabCallback { get; }

        /// <summary>
        /// Gets a list of headers which will be shown in the "Useful links" menu, together with the links they point to
        /// </summary>
        public IDictionary<string, string> HelpLinks { get; } = new Dictionary<string, string>
        {
            { "Change the Ribbon in Excel 2007 and up | Ron de Bruin Excel Automation", "http://www.rondebruin.nl/win/s2/win001.htm" },
            { "Customize the 2007 Office Fluent Ribbon for Developers | Microsoft Docs", "https://msdn.microsoft.com/en-us/library/aa338202(v=office.14).aspx" },
            { "Introduction to the Office 2010 Backstage View for Developers | Microsoft Docs", "https://msdn.microsoft.com/en-us/library/ee691833(office.14).aspx" },
            { "Office Fluent UI Command Identifiers | OfficeDev on GitHub", "https://github.com/OfficeDev/office-fluent-ui-command-identifiers" },
            { "Creating VSTO Add-ins for Office by using Visual Studio | Microsoft Docs", "https://msdn.microsoft.com/en-us/library/jj620922.aspx" },
            { "ImageMSO List Reference | BERT", "https://bert-toolkit.com/imagemso-list.html" },
            { "Office Dev Center", "https://developer.microsoft.com/en-us/office" },
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

        public IContentDialogBase LaunchDialog<TDialog>(bool showDialog = false) where TDialog : IContentDialogBase
        {
            if (!this.dialogs.TryGetValue(typeof(TDialog), out var content) || content.IsClosed)
            {
                // Resolve a new dialog, as any potentially existing one is not suitable
                content = this.dialogProvider.ResolveDialog<TDialog>();
            }
            
            this.LaunchingDialog?.Invoke(this, new LaunchDialogEventArgs { Content = content, ShowDialog = showDialog });
            if (content.IsUnique)
            {
                // Keep track of the new content
                this.dialogs[typeof(TDialog)] = content;
            }

            return content;
        }

        public IContentDialog<TPayload> LaunchDialog<TDialog, TPayload>(TPayload payload, bool showDialog = false) where TDialog : IContentDialog<TPayload>
        {
            if (!this.dialogs.TryGetValue(typeof(TDialog), out var baseContent) || baseContent.IsClosed)
            {
                // Resolve a new dialog, as any potentially existing one is not suitable
                baseContent = this.dialogProvider.ResolveDialog<TDialog>();
            }
            
            if (baseContent.IsUnique)
            {
                // Keep track of the new content
                this.dialogs[typeof(TDialog)] = baseContent;
            }

            var content = (TDialog) baseContent;
            if (!content.OnLoaded(payload))
            {
                // This might happen if the dialog has an associated instant action for which it doesn't need to stay open, e.g. 'Find Next' in a FindReplaceDialog
                return content;
            }
            
            this.LaunchingDialog?.Invoke(this, new LaunchDialogEventArgs { Content = content, ShowDialog = showDialog });

            return content;
        }

        public void PerformFindReplaceAction(FindReplaceAction action)
        {
            if (!(this.SelectedTab is EditorTabViewModel tab))
            {
                return;
            }

            var lexer = tab?.Lexer;
            if (lexer == null)
            {
                return;
            }

            this.LaunchDialog<FindReplaceDialogViewModel, (Scintilla, FindReplaceAction, FindReplace.FindAllResultsEventHandler)>((
                lexer.Editor.Scintilla,
                action,
                (o, e) => tab.RaiseShowResults(e)));
        }

        private void ExecuteCloseDocumentCommand()
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

            var tabs = this.OpenTabs.OfType<EditorTabViewModel>().Where(x => doc.Children.Contains(x.Part)).ToList();
            foreach (var tab in tabs)
            {
                this.ExecuteCloseTabCommand(tab);
            }

            doc.Document.Dispose();
            this.DocumentList.Remove(doc);
        }

        private void ExecuteOpenTabCommand(TreeViewItemViewModel viewModel = null)
        {
            if (viewModel == null)
            {
                viewModel = this.SelectedItem;
            }

            if (viewModel is OfficePartViewModel part)
            {
                this.OpenPartTab(part);
            }
            else if (viewModel is IconViewModel icon)
            {
                this.OpenIconTab(icon);
            }
        }

        public void ExecuteCloseTabCommand(ITabItemViewModel tab = null)
        {
            if (tab == null)
            {
                tab = this.SelectedTab;
            }

            var index = this.OpenTabs.IndexOf(tab);
            if (index == -1)
            {
                return;
            }

            tab.ApplyChanges();

            this.OpenTabs.RemoveAt(index);
            if (this.SelectedTab == tab)
            {
                this.SelectedTab = index < this.OpenTabs.Count ? this.OpenTabs[index] : index > 0 ? this.OpenTabs[index - 1] : null;
            }
        }

        private void ExecuteInsertIconsCommand()
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
        private void ExecuteChangeIconIdCommand()
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

        private void ExecuteRemoveItemCommand()
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

                foreach (var tab in this.OpenTabs.OfType<EditorTabViewModel>())
                {
                    if (tab.Part == part)
                    {
                        this.ExecuteCloseTabCommand(tab);
                        break;
                    }
                }

                foreach (var tab in this.OpenTabs.OfType<IconTabViewModel>().ToList())
                {
                    if (part.Children.Any(x => x == tab.Icon))
                    {
                        this.ExecuteCloseTabCommand(tab);
                    }
                }

                var doc = (OfficeDocumentViewModel)part.Parent;
                doc.RemovePart(part.Part.PartType);
                return;
            }

            if (this.SelectedItem is IconViewModel)
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

                var icon = (IconViewModel) this.SelectedItem;

                foreach (var tab in this.OpenTabs.OfType<IconTabViewModel>())
                {
                    if (tab.Icon == icon)
                    {
                        this.ExecuteCloseTabCommand(tab);
                        break;
                    }
                }

                var part = (OfficePartViewModel)icon.Parent;
                part.RemoveIcon(icon.Id);
            }
        }

        private void ExecuteClosingCommand(CancelEventArgs e)
        {
            foreach (var tab in this.OpenTabs)
            {
                tab.ApplyChanges();
            }

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

        private void ExecuteCloseCommand()
        {
            this.Closed?.Invoke(this, EventArgs.Empty);
        }

        private void ExecutePreviewDragCommand(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null)
            {
                return;
            }

            if (!files.Any(File.Exists))
            {
                return;
            }

            e.Handled = true;
        }

        private void ExecuteDropCommand(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null)
            {
                return;
            }

            foreach (var file in files)
            {
                this.FinishOpeningFile(file);
            }
        }

        private void ExecuteOpenDocumentCommand()
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

        public EditorTabViewModel OpenPartTab(OfficePartViewModel part = null)
        {
            if (part == null)
            {
                part = this.SelectedItem as OfficePartViewModel;
            }

            if (part == null)
            {
                return null;
            }

            var tab = this.OpenTabs.OfType<EditorTabViewModel>().FirstOrDefault(x => x.Part == part);
            if (tab == null)
            {
                var doc = (OfficeDocumentViewModel) part.Parent;
                tab = new EditorTabViewModel
                {
                    Part = part,
                    MainWindow = this,
                    Title = $"{doc.Name} - {part.Name}",
                };
                this.OpenTabs.Add(tab);
            }

            this.SelectedTab = tab;
            return tab;
        }

        public IconTabViewModel OpenIconTab(IconViewModel icon = null)
        {
            if (icon == null)
            {
                icon = this.SelectedItem as IconViewModel;
            }

            if (icon == null)
            {
                return null;
            }

            var tab = this.OpenTabs.OfType<IconTabViewModel>().FirstOrDefault(x => x.Icon == icon);
            if (tab == null)
            {
                var part = (OfficePartViewModel) icon.Parent;
                var doc = (OfficeDocumentViewModel) part.Parent;
                tab = new IconTabViewModel
                {
                    Icon = icon,
                    MainWindow = this,
                    Title = $"{doc.Name} - {part.Name} - {icon.Id}",
                };
                this.OpenTabs.Add(tab);
            }

            this.SelectedTab = tab;
            return tab;
        }

        private void ExecuteSaveCommand()
        {
            if (this.CurrentDocument == null)
            {
                return;
            }
            
            foreach (var tab in this.OpenTabs)
            {
                tab.ApplyChanges();
            }

            try
            {
                this.CurrentDocument.Save(this.ReloadOnSave, preserveAttributes: Settings.Default.PreserveAttributes);
            }
            catch (Exception ex)
            {
                this.messageBoxService.Show(ex.Message, "Error saving Office document", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSaveAllCommand()
        {
            foreach (var tab in this.OpenTabs)
            {
                tab.ApplyChanges();
            }

            foreach (var doc in this.DocumentList)
            {
                doc.Save(this.ReloadOnSave, preserveAttributes: Settings.Default.PreserveAttributes);
            }
        }

        private void ExecuteSaveAsCommand()
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

                doc.Save(this.reloadOnSave, fileName, Settings.Default.PreserveAttributes);
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

        /// <summary>
        /// Inserts an XML sample to the selected document in the tree
        /// </summary>
        /// <param name="resourceName"></param>
        private void ExecuteInsertXmlSampleCommand(string resourceName)
        {
            // TODO: This command should be clearer with its target
            // Right now, it is the selected item, but users might find it more intuitive to insert the sample in
            // the already opened tab. To fix this, the easiest thing to do would be to have a separate command for
            // each (i.e. a context menu action for the tree view and a menu action for the editor)

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

            var tab = this.OpenTabs.OfType<EditorTabViewModel>().FirstOrDefault(x => x.Part == part) ?? this.OpenPartTab(part);

            try
            {
                var data = XmlSampleViewModel.ReadContents(resourceName);

                // Make sure the xml schema is not for the wrong part type
                if (this.customUiSchemas[part.Part.PartType] is XmlSchema thisSchema && this.customUiSchemas[part.Part.PartType == XmlParts.RibbonX12 ? XmlParts.RibbonX14 : XmlParts.RibbonX12] is XmlSchema otherSchema)
                {
                    data = data.Replace(otherSchema.TargetNamespace, thisSchema.TargetNamespace);
                }

                // Event might be raised too soon (when the view still does not exist). Hence, update part as well
                part.Contents = data;
                tab.RaiseUpdateEditor(new EditorChangeEventArgs {Start = -1, End = -1, NewText = data});
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
                this.messageBoxService.Show(ex.Message, "Error inserting XML sample");
            }
        }

        private bool ValidateXml(bool showValidMessage)
        {
            if (!(this.SelectedTab is EditorTabViewModel tab))
            {
                return false;
            }

            tab.ApplyChanges();
            var part = tab.Part;

            // Test to see if text is XML first
            try
            {
                if (!(this.customUiSchemas[part.Part.PartType] is XmlSchema targetSchema))
                {
                    return false;
                }

                var xmlDoc = XDocument.Parse(
                    part.Contents, 
                    LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
                
                var schemaSet = new XmlSchemaSet();
                schemaSet.Add(targetSchema);

                var errorList = new List<XmlError>();

                var ns = xmlDoc.Root?.GetDefaultNamespace().ToString();
                if (ns != targetSchema.TargetNamespace)
                {
                    errorList.Add(new XmlError
                    {
                        LineNumber = 1,
                        LinePosition = 1,
                        Message = $"Unknown namespace \"{ns}\". Custom UI XML namespace must be \"{targetSchema.TargetNamespace}\"",
                    });
                }

                void ValidateHandler(object o, ValidationEventArgs e)
                {
                    errorList.Add(new XmlError
                    {
                        LineNumber = e.Exception.LineNumber,
                        LinePosition = e.Exception.LinePosition,
                        Message = e.Message,
                    });
                }

                xmlDoc.Validate(schemaSet, ValidateHandler);

                if (!errorList.Any())
                {
                    if (showValidMessage)
                    {
                        this.messageBoxService.Show(
                            StringsResource.idsValidXml,
                            "XML is Valid",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }

                    return true;
                }

                tab.RaiseShowResults(new ResultsEventArgs(new XmlErrorResults(errorList)));
                return false;
            }
            catch (XmlException ex)
            {
                var errorList = new[]
                {
                    new XmlError
                    {
                        LineNumber = ex.LineNumber,
                        LinePosition = ex.LinePosition,
                        Message = ex.Message,
                    }
                };

                tab.RaiseShowResults(new ResultsEventArgs(new XmlErrorResults(errorList)));
                return false;
            }
        }
        
        private void ExecuteGenerateCallbacksCommand()
        {
            // TODO: Check whether any text is selected, and generate callbacks only for that text
            if (this.SelectedTab == null)
            {
                return;
            }

            this.SelectedTab.ApplyChanges();
            
            if (!(this.SelectedTab is EditorTabViewModel tab))
            {
                return;
            }

            var part = tab.Part;

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
                
                this.LaunchDialog<CallbackDialogViewModel, string>(callbacks.ToString());
            }
            catch (Exception ex)
            {
                this.messageBoxService.Show(ex.Message, "Error Generating Callbacks", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteGoToCommand()
        {
            if (!(this.SelectedTab is EditorTabViewModel tab))
            {
                return;
            }

            var lexer = tab.Lexer;
            if (lexer == null)
            {
                return;
            }

            this.LaunchDialog<GoToDialogViewModel, ScintillaLexer>(lexer);
        }

        private void ExecuteToggleCommentCommand()
        {
            if (!(this.SelectedTab is EditorTabViewModel tab))
            {
                return;
            }

            var data = tab.EditorInfo;
            if (data == null)
            {
                return;
            }

            // Extend the selection to pick full lines
            const string NewLine = "\n";
            var start = data.Text.LastIndexOf(NewLine, data.Selection.Item1, StringComparison.Ordinal) + 1;

            var end = data.Text.IndexOf(NewLine, data.Selection.Item2, StringComparison.Ordinal);
            if (end < 0)
            {
                end = data.Text.Length;
            }

            if (end < start)
            {
                // This should only happen in blank lines, which do not need to be toggled
                return;
            }

            // TODO: Use a StringBuilder
            var lines = data.Text.Substring(start, end - start).Split(new[] { NewLine }, StringSplitOptions.None);
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

            // Update the selected item's current contents to that, and notify the editor
            tab.RaiseUpdateEditor(new EditorChangeEventArgs { Start = start, End = end, NewText = combined, UpdateSelection = true });
        }

        private async void CheckVersionAsync(IVersionChecker versionChecker)
        {
            this.NewerVersion = await versionChecker.CheckVersionAsync();
        }

        private void ExecuteNewerVersionCommand()
        {
            var result = this.messageBoxService.Show(
                $"Release version {this.newerVersion} is now available. Do you want to download it?", 
                "Newer Version Available", 
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            Process.Start("https://github.com/fernandreu/office-ribbonx-editor/releases/latest");
        }
    }
}
