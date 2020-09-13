using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using AsyncAwaitBestPractices;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Documents;
using OfficeRibbonXEditor.Events;
using OfficeRibbonXEditor.Extensions;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Lexers;
using OfficeRibbonXEditor.Properties;
using OfficeRibbonXEditor.Resources;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using OfficeRibbonXEditor.ViewModels.Documents;
using OfficeRibbonXEditor.ViewModels.Samples;
using OfficeRibbonXEditor.ViewModels.Tabs;
using ScintillaNET;

namespace OfficeRibbonXEditor.ViewModels.Windows
{
    using ResultsEventArgs = DataEventArgs<IResultCollection>;

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Shown in a message box anyway")]
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private readonly IMessageBoxService messageBoxService;

        private readonly IFileDialogService fileDialogService;

        private readonly IVersionChecker versionChecker;

        private readonly IDialogProvider dialogProvider;

        private readonly IUrlHelper urlHelper;

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
        private string? newerVersion = null;

        private readonly Hashtable? customUiSchemas;

        private TreeViewItemViewModel? selectedItem = null;

        private bool disposed;

        public MainWindowViewModel(
            IMessageBoxService messageBoxService, 
            IFileDialogService fileDialogService, 
            IVersionChecker versionChecker, 
            IDialogProvider dialogProvider, 
            IUrlHelper urlHelper)
        {
            this.messageBoxService = messageBoxService;
            this.fileDialogService = fileDialogService;
            this.versionChecker = versionChecker;
            this.dialogProvider = dialogProvider;
            this.urlHelper = urlHelper;

            this.OpenDocumentCommand = new RelayCommand(this.ExecuteOpenDocumentCommand);
            this.OpenTabCommand = new RelayCommand<TreeViewItemViewModel>(this.ExecuteOpenTabCommand);
            this.SaveCommand = new RelayCommand(this.ExecuteSaveCommand);
            this.SaveAllCommand = new RelayCommand(this.ExecuteSaveAllCommand);
            this.SaveAsCommand = new RelayCommand(() => this.ExecuteSaveAsCommand(true));
            this.SaveACopyAsCommand = new RelayCommand(() => this.ExecuteSaveAsCommand(false));
            this.CloseDocumentCommand = new RelayCommand(this.ExecuteCloseDocumentCommand);
            this.InsertXml14Command = new RelayCommand(() => this.CurrentDocument?.InsertPart(XmlPart.RibbonX14));
            this.InsertXml12Command = new RelayCommand(() => this.CurrentDocument?.InsertPart(XmlPart.RibbonX12));
            this.InsertXmlSampleCommand = new RelayCommand<XmlSampleViewModel>(this.ExecuteInsertXmlSampleCommand);
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
            
            this.ShowSettingsCommand = new RelayCommand(this.ExecuteShowSettingsCommand);
            this.ShowAboutCommand = new RelayCommand(() => this.LaunchDialog<AboutDialogViewModel>(true));
            this.RecentFileClickCommand = new RelayCommand<string>(this.FinishOpeningFile);
            this.ClosingCommand = new RelayCommand<CancelEventArgs>(this.ExecuteClosingCommand);
            this.CloseCommand = new RelayCommand(this.ExecuteCloseCommand);
            this.CloseTabCommand = new RelayCommand<ITabItemViewModel>(this.ExecuteCloseTabCommand);
            this.PreviewDragEnterCommand = new RelayCommand<DragData>(this.ExecutePreviewDragCommand);
            this.DropCommand = new RelayCommand<DragData>(this.ExecuteDropCommand);
            this.NewerVersionCommand = new RelayCommand(this.ExecuteNewerVersionCommand);
            this.OpenHelpLinkCommand = new RelayCommand<string>(this.ExecuteOpenHelpLinkCommand);

            this.DocumentList.CollectionChanged += this.OnTreeViewItemCollectionChanged;

#if DEBUG
            if (this.IsInDesignMode)
            {
                return;
            }
#endif
            this.customUiSchemas = LoadXmlSchemas();
            this.XmlSamples = LoadXmlSamples();
        }

        /// <summary>
        /// This gets raised when there is a closed event originated from the ViewModel (e.g. programmatically)
        /// </summary>
        public event EventHandler? Closed;

        public event EventHandler<LaunchDialogEventArgs>? LaunchingDialog;

        /// <summary>
        /// This event will be fired when a file needs to be added to the recent list. The argument will be the path to the file itself.
        /// </summary>
        public event EventHandler<DataEventArgs<string>>? InsertRecentFile;

        public event EventHandler<DataEventArgs<Cursor>>? SetGlobalCursor; 

        public ObservableCollection<OfficeDocumentViewModel> DocumentList { get; } = new ObservableCollection<OfficeDocumentViewModel>();

        private SampleFolderViewModel? xmlSamples;

        public SampleFolderViewModel? XmlSamples
        {
            get => this.xmlSamples;
            set => this.Set(ref this.xmlSamples, value);
        }

        public ObservableCollection<ITabItemViewModel> OpenTabs { get; } = new ObservableCollection<ITabItemViewModel>();

        private ITabItemViewModel? selectedTab;

        public ITabItemViewModel? SelectedTab
        {
            get => this.selectedTab;
            set
            {
                if (!this.Set(ref this.selectedTab, value))
                {
                    return;
                }

                if (value != null)
                {
                    // This should help the user locate the item in the tree view
                    this.SelectedItem = value.Item;
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

                Settings.Default.ShowWhitespace = value;
                foreach (var tab in this.OpenTabs.OfType<EditorTabViewModel>())
                {
                    tab.Lexer?.Update();
                }
            }
        }

        public string? NewerVersion
        {
            get => this.newerVersion;
            set => this.Set(ref this.newerVersion, value);
        }

        public TreeViewItemViewModel? SelectedItem
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
        
        public bool CanInsertXml12Part => (this.SelectedItem is OfficeDocumentViewModel model) && model.Document.RetrieveCustomPart(XmlPart.RibbonX12) == null;

        public bool CanInsertXml14Part => (this.SelectedItem is OfficeDocumentViewModel model) && model.Document.RetrieveCustomPart(XmlPart.RibbonX14) == null;

        public RelayCommand OpenDocumentCommand { get; }

        public RelayCommand<TreeViewItemViewModel> OpenTabCommand { get; }

        public RelayCommand SaveCommand { get; }

        public RelayCommand SaveAllCommand { get; }

        public RelayCommand SaveAsCommand { get; }

        public RelayCommand SaveACopyAsCommand { get; }

        public RelayCommand CloseDocumentCommand { get; }

        public RelayCommand InsertXml14Command { get; }
        
        public RelayCommand InsertXml12Command { get; }

        public RelayCommand<XmlSampleViewModel> InsertXmlSampleCommand { get; set; }

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
        public RelayCommand<DragData> PreviewDragEnterCommand { get; }

        /// <summary>
        /// Gets the command that finishes the drag / drop action for opening files
        /// </summary>
        public RelayCommand<DragData> DropCommand { get; }

        public RelayCommand<string> OpenHelpLinkCommand { get; }

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
        public OfficeDocumentViewModel? CurrentDocument
        {
            get
            {
                // Get currently active document
                var elem = this.SelectedItem;
                if (elem == null)
                {
                    return null;
                }

                // Find the root document
                if (elem is IconViewModel)
                {
                    return elem.Parent?.Parent as OfficeDocumentViewModel;
                }

                if (elem is OfficePartViewModel)
                {
                    return elem.Parent as OfficeDocumentViewModel;
                }

                if (elem is OfficeDocumentViewModel viewModel)
                {
                    return viewModel;
                }

                return null;
            }
        }

        /// <summary>
        /// Called by the application to perform any action that might depend on the window having been
        /// set up already (usually because they depend on events listened in the window).
        /// </summary>
        public void OnLoaded()
        {
            foreach (var file in Environment.GetCommandLineArgs().Skip(1))
            {
                if (!File.Exists(file))
                {
                    continue;
                }

                this.FinishOpeningFile(file);
            }

            this.CheckVersionAsync(this.versionChecker).SafeFireAndForget();
        }

        public IContentDialogBase LaunchDialog<TDialog>(bool showDialog = false) where TDialog : IContentDialogBase
        {
            if (!this.dialogs.TryGetValue(typeof(TDialog), out var content) || content.IsClosed)
            {
                // Resolve a new dialog, as any potentially existing one is not suitable
                content = this.dialogProvider.ResolveDialog<TDialog>();
            }
            
            this.LaunchingDialog?.Invoke(this, new LaunchDialogEventArgs(content, showDialog));
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
            
            this.LaunchingDialog?.Invoke(this, new LaunchDialogEventArgs(content, showDialog));

            return content;
        }

        public void PerformFindReplaceAction(FindReplaceAction action)
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

            this.LaunchDialog<FindReplaceDialogViewModel, (Scintilla, FindReplaceAction, FindReplace.FindAllResultsEventHandler)>((
                lexer.Editor.Scintilla,
                action,
                (o, e) => tab.OnShowResults(e)));
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
                var result = this.messageBoxService.Show(string.Format(CultureInfo.InvariantCulture, Strings.Message_CloseUnsavedDoc_Text, doc.Name), Strings.Message_CloseUnsavedDoc_Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
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

        private void ExecuteOpenTabCommand(TreeViewItemViewModel? viewModel = null)
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

        public void ExecuteCloseTabCommand(ITabItemViewModel? tab = null)
        {
            tab ??= this.selectedTab;
            if (tab == null)
            {
                return;
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
                var count = this.OpenTabs.Count;
                this.SelectedTab = count > 0 ? this.OpenTabs[Math.Max(index, count - 1)] : null;
            }
        }

        private void ExecuteInsertIconsCommand()
        {
            if (!(this.SelectedItem is OfficePartViewModel))
            {
                return;
            }

            this.fileDialogService.OpenFilesDialog(Strings.OpenDialog_Icons_Title, Strings.Filter_Icons + "|" + Strings.Filter_All, this.FinishInsertingIcons);
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

            bool AlreadyExistingAction(string? existingId, string? newId)
            {
                var result = this.messageBoxService.Show(
                    string.Format(CultureInfo.InvariantCulture, Strings.Message_IconExists_Text, existingId, newId),
                    Strings.Message_IconExists_Title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);

                return result == MessageBoxResult.Yes;
            }

            foreach (var path in filePaths)
            {
                part.InsertIcon(path, alreadyExistingAction: AlreadyExistingAction);
            }
        }

        private void ExecuteRemoveItemCommand()
        {
            if (this.SelectedItem is OfficePartViewModel part)
            {
                var result = this.messageBoxService.Show(
                    Strings.Message_RemovePart_Text, 
                    Strings.Message_RemovePart_Title, 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

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

                var doc = part.Parent as OfficeDocumentViewModel;
                var type = part.Part?.PartType;
                if (type != null)
                {
                    doc?.RemovePart(type.Value);
                }

                return;
            }

            if (this.SelectedItem is IconViewModel icon)
            {
                var result = this.messageBoxService.Show(
                    Strings.Message_RemoveIcon_Text, 
                    Strings.Message_RemoveIcon_Title, 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                foreach (var tab in this.OpenTabs.OfType<IconTabViewModel>())
                {
                    if (tab.Icon == icon)
                    {
                        this.ExecuteCloseTabCommand(tab);
                        break;
                    }
                }

                var parent = icon.Parent as OfficePartViewModel;
                parent?.RemoveIcon(icon.Name);
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
                        string.Format(CultureInfo.InvariantCulture, Strings.Message_CloseUnsavedDoc_Text, doc.Name), 
                        Strings.Message_CloseUnsavedDoc_Title,
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

        private void ExecutePreviewDragCommand(DragData data)
        {
            if (!data.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            if (!(data.Data.GetData(DataFormats.FileDrop) is string[] files))
            {
                return;
            }

            if (!files.Any(File.Exists))
            {
                return;
            }

            data.Handled = true;
        }

        private void ExecuteDropCommand(DragData data)
        {
            if (!data.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            if (!(data.Data.GetData(DataFormats.FileDrop) is string[] files))
            {
                return;
            }

            foreach (var file in files)
            {
                this.FinishOpeningFile(file);
            }

            data.Handled = true;
        }

        private void ExecuteOpenDocumentCommand()
        {
            string[] filters =
                {
                    Strings.Filter_AllOfficeDocuments,
                    Strings.Filter_WordDocuments,
                    Strings.Filter_ExcelDocuments,
                    Strings.Filter_PowerPointDocuments,
                    Strings.Filter_All,
                };

            this.fileDialogService.OpenFileDialog(
                Strings.OpenDialog_Document_Title, 
                string.Join("|", filters), 
                this.FinishOpeningFile);
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Resource deallocation handled in VM's Dispose() already")]
        private void FinishOpeningFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            var normalized = Path.GetFullPath(fileName);
            var existing = this.DocumentList.FirstOrDefault(x => Path.GetFullPath(x.Document.Name).Equals(normalized, StringComparison.InvariantCultureIgnoreCase));
            if (existing != null)
            {
                var result = this.messageBoxService.Show(
                    string.Format(CultureInfo.InvariantCulture, Strings.Message_AlreadyOpen_Text, existing.Name),
                    Strings.Message_AlreadyOpen_Title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            OfficeDocumentViewModel model;

            try
            {
                Debug.WriteLine("Opening " + fileName + "...");

                var doc = new OfficeDocument(fileName);
                model = new OfficeDocumentViewModel(doc);
            }
            catch (Exception ex)
            {
                this.messageBoxService.Show(ex.Message, Strings.Message_OpenError_Title, image: MessageBoxImage.Error);
                return;
            }

            this.DocumentList.Add(model);
            this.InsertRecentFile?.Invoke(this, new DataEventArgs<string> { Data = fileName });

            // Expand the tree view
            static void Expand(TreeViewItemViewModel vm)
            {
                vm.IsExpanded = true;
                foreach (var child in vm.Children)
                {
                    Expand(child);
                }
            }

            Expand(model);

            if (model.Children.Count == 1 && model.Children[0].Children.Count == 0)
            {
                // A document with a single custom UI xml file and no icons: open it automatically. This will also select it on the TreeView
                this.ExecuteOpenTabCommand(model.Children[0]);
            }
            else if (model.Children.Count > 0)
            {
                // Select the document on the TreeView
                this.SelectedItem = model;
            }
        }

        public EditorTabViewModel? OpenPartTab(OfficePartViewModel? part = null)
        {
            part ??= this.SelectedItem as OfficePartViewModel;
            if (part == null)
            {
                return null;
            }

            var tab = this.OpenTabs.OfType<EditorTabViewModel>().FirstOrDefault(x => x.Part == part);
            if (tab == null)
            {
                tab = new EditorTabViewModel(part, this);
                this.OpenTabs.Add(tab);
                this.AdjustTabTitle(tab);
            }

            this.SelectedTab = tab;
            return tab;
        }

        public IconTabViewModel? OpenIconTab(IconViewModel? icon = null)
        {
            icon ??= this.SelectedItem as IconViewModel;
            if (icon == null)
            {
                return null;
            }

            var tab = this.OpenTabs.OfType<IconTabViewModel>().FirstOrDefault(x => x.Icon == icon);
            if (tab == null)
            {
                tab = new IconTabViewModel(icon, this);
                this.OpenTabs.Add(tab);
                this.AdjustTabTitle(tab);
            }

            this.SelectedTab = tab;
            return tab;
        }

        public void AdjustTabTitles()
        {
            foreach (var tab in this.OpenTabs)
            {
                this.AdjustTabTitle(tab);
            }
        }

        public void AdjustTabTitle(ITabItemViewModel tab)
        {
            if (tab == null)
            {
                return;
            }

            var result = tab.Item.Name;
            var targets = this.DocumentList.FindItemsByName(tab.Item).ToList();
            if (targets.Count == 0)
            {
                tab.Title = result;
                return;
            }

            // Keep adding their parent names until there is no match or there is no parent
            for (var target = tab.Item.Parent; target != null; target = target.Parent)
            {
                result = $"{target.Name}\\{result}";
                targets = targets
                    .Where(x => x.Parent != null && x.Parent.Name == target.Name)
                    .Select(x => x.Parent!)
                    .ToList();

                if (targets.Count == 0)
                {
                    // No other items with the same name at this level: we can stop traversing the tree upwards
                    break;
                }
            }

            tab.Title = result;
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
                using (new CursorOverride(this, Cursors.Wait))
                {
                    this.CurrentDocument.Save(this.ReloadOnSave, preserveAttributes: Settings.Default.PreserveAttributes);
                }
            }
            catch (Exception ex)
            {
                this.messageBoxService.Show(ex.Message, Strings.Message_SaveError_Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSaveAllCommand()
        {
            foreach (var tab in this.OpenTabs)
            {
                tab.ApplyChanges();
            }

            try
            {
                foreach (var doc in this.DocumentList)
                {
                    doc.Save(this.ReloadOnSave, preserveAttributes: Settings.Default.PreserveAttributes);
                }
            }
            catch (Exception ex)
            {
                this.messageBoxService.Show(ex.Message, Strings.Message_SaveError_Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSaveAsCommand(bool renameCurrent)
        {
            var doc = this.CurrentDocument;
            if (doc == null)
            {
                return;
            }
            
            var filters = new List<string>();
            while (true)
            {
                var filter = Strings.ResourceManager.GetString("Filter.SaveAs" + filters.Count, CultureInfo.CurrentCulture);
                if (filter == null)
                {
                    break;
                }

                filters.Add(filter);
            }

            filters.Add(Strings.Filter_All);
            
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
                Strings.SaveDialog_SaveAs_Title, 
                string.Join("|", filters),
                path => this.FinishSavingFile(path, renameCurrent), 
                doc.Name, 
                i + 1);
        }

        private void FinishSavingFile(string fileName, bool renameCurrent)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }
            
            // Note: We are assuming that no UI events happen between the SaveFileDialog was
            // shown and this is called. Otherwise, selection might have changed
            var doc = this.CurrentDocument;
            if (doc == null)
            {
                throw new InvalidOperationException("Selected document seems to have changed between showing file dialog and closing it");
            }
            
            if (!Path.HasExtension(fileName))
            {
                fileName = Path.ChangeExtension(fileName, Path.GetExtension(doc.Name));
            }

            Debug.WriteLine("Saving " + fileName + "...");

            try
            {
                using (new CursorOverride(this, Cursors.Wait))
                {
                    doc.Save(this.reloadOnSave, fileName, Settings.Default.PreserveAttributes);
                }
            }
            catch (Exception ex)
            {
                this.messageBoxService.Show(ex.Message, Strings.Message_SaveError_Title, image: MessageBoxImage.Error);
                return;
            }
            
            this.InsertRecentFile?.Invoke(this, new DataEventArgs<string> { Data = fileName });

            if (renameCurrent)
            {
                doc.Document.Name = fileName;

                // Ensure name is updated in the TreeView
                doc.RaisePropertyChanged(nameof(doc.Name));
            }
        }

        private static Hashtable LoadXmlSchemas()
        {
            var result = new Hashtable(2);

            using (var stringReader = new StringReader(SchemasResource.customUI))
            using (var reader = XmlReader.Create(stringReader, new XmlReaderSettings { XmlResolver = null }))
            {
                result.Add(XmlPart.RibbonX12, XmlSchema.Read(reader, null));
            }
                
            using (var stringReader = new StringReader(SchemasResource.customui14))
            using (var reader = XmlReader.Create(stringReader, new XmlReaderSettings { XmlResolver = null }))
            {
                result.Add(XmlPart.RibbonX14, XmlSchema.Read(reader, null));
            }

            return result;
        }

        private static SampleFolderViewModel? LoadXmlSamples()
        {
            return SampleUtils.LoadXmlSamples(
                Settings.Default.CustomSamples.Split('\n'), 
                Settings.Default.ShowDefaultSamples);
        }

        /// <summary>
        /// Inserts an XML sample to the selected document in the tree
        /// </summary>
        private void ExecuteInsertXmlSampleCommand(XmlSampleViewModel sample)
        {
            // TODO: This command should be clearer with its target
            // Right now, it is the selected item, but users might find it more intuitive to insert the sample in
            // the already opened tab. To fix this, the easiest thing to do would be to have a separate command for
            // each (i.e. a context menu action for the tree view and a menu action for the editor)

            var newPart = false;
            
            if (this.SelectedItem is OfficeDocumentViewModel doc)
            {
                // See if there is already a part, and otherwise insert one
                if (doc.Children.Count == 0)
                {
                    doc.InsertPart(XmlPart.RibbonX12);
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
                    Strings.Message_InsertSample_Text, 
                    Strings.Message_InsertSample_Title, 
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            var tab = this.OpenTabs.OfType<EditorTabViewModel>().FirstOrDefault(x => x.Part == part) ?? this.OpenPartTab(part);
            if (tab == null)
            {
                throw new InvalidOperationException("Could not find / create a tab for inserting the XML sample");
            }

            try
            {
                var data = sample.ReadContents();

                // Make sure the xml schema is not for the wrong part type
                if (this.customUiSchemas != null &&
                    part.Part != null &&
                    this.customUiSchemas[part.Part.PartType] is XmlSchema thisSchema && 
                    this.customUiSchemas[part.Part.PartType == XmlPart.RibbonX12 ? XmlPart.RibbonX14 : XmlPart.RibbonX12] is XmlSchema otherSchema)
                {
#pragma warning disable CA1307 // Specify StringComparison (this option is not available in .NET Framework 4.6.1)
                    data = data.Replace(otherSchema.TargetNamespace, thisSchema.TargetNamespace);
#pragma warning restore CA1307 // Specify StringComparison
                }

                // Event might be raised too soon (when the view still does not exist). Hence, update part as well
                var info = tab.EditorInfo;
                if (info == null)
                {
                    part.Contents = data;
                    tab.OnUpdateEditor(new EditorChangeEventArgs {Start = -1, End = -1, NewText = data});
                }
                else
                {
                    tab.OnUpdateEditor(new EditorChangeEventArgs {Start = info.Selection.Item1, End = info.Selection.Item2, NewText = data});
                }
            }
            catch (Exception ex)
            {
                this.messageBoxService.Show(ex.Message, Strings.Message_InsertSampleError_Title);
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
                if (this.customUiSchemas == null || part.Part == null || !(this.customUiSchemas[part.Part.PartType] is XmlSchema targetSchema))
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
                    errorList.Add(new XmlError(
                        1,
                        1,
                        $"Unknown namespace \"{ns}\". Custom UI XML namespace must be \"{targetSchema.TargetNamespace}\""));
                }

                void ValidateHandler(object o, ValidationEventArgs e)
                {
                    errorList.Add(new XmlError(
                        e.Exception.LineNumber,
                        e.Exception.LinePosition,
                        e.Message));
                }

                xmlDoc.Validate(schemaSet, ValidateHandler);
                
                tab.OnShowResults(new ResultsEventArgs(new XmlErrorResults(errorList)));

                if (!errorList.Any())
                {
                    if (showValidMessage)
                    {
                        this.messageBoxService.Show(
                            Strings.Message_ValidXml_Text,
                            Strings.Message_ValidXml_Title,
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }

                    return true;
                }

                return false;
            }
            catch (XmlException ex)
            {
                var errorList = new[]
                {
                    new XmlError(
                        ex.LineNumber,
                        ex.LinePosition,
                        ex.Message),
                };

                tab.OnShowResults(new ResultsEventArgs(new XmlErrorResults(errorList)));
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
                var customUi = new XmlDocument { XmlResolver = null };

                using (var stringReader = new StringReader(part.Contents ?? string.Empty))
                using (var reader = XmlReader.Create(stringReader, new XmlReaderSettings { XmlResolver = null }))
                {
                    customUi.Load(reader);
                }

                var callbacks = CallbacksBuilder.GenerateCallback(customUi);
                if (callbacks == null || callbacks.Length == 0)
                {
                    this.messageBoxService.Show(Strings.Message_NoCallbacks_Text, Strings.Message_NoCallbacks_Title, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                this.LaunchDialog<CallbackDialogViewModel, string?>(callbacks.ToString());
            }
            catch (Exception ex)
            {
                this.messageBoxService.Show(ex.Message, Strings.Message_CallbackError_Title, MessageBoxButton.OK, MessageBoxImage.Error);
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
                if (trimmed.StartsWith("<!--", StringComparison.OrdinalIgnoreCase) && trimmed.EndsWith("-->", StringComparison.OrdinalIgnoreCase))
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
            tab.OnUpdateEditor(new EditorChangeEventArgs { Start = start, End = end, NewText = combined, UpdateSelection = true });
        }

        private async Task CheckVersionAsync(IVersionChecker versionChecker)
        {
            this.NewerVersion = await versionChecker.CheckVersionAsync().ConfigureAwait(false);
        }

        private void ExecuteNewerVersionCommand()
        {
            var result = this.messageBoxService.Show(
                string.Format(CultureInfo.InvariantCulture, Strings.Message_NewVersion_Text, this.newerVersion),
                Strings.Message_NewVersion_Title, 
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            this.urlHelper.OpenRelease();
        }

        private void ExecuteOpenHelpLinkCommand(string url)
        {
            this.urlHelper.OpenExternal(new Uri(url));
        }

        private void ExecuteShowSettingsCommand()
        {
            var dialog = this.LaunchDialog<SettingsDialogViewModel, ICollection<ITabItemViewModel>>(this.OpenTabs);
            dialog.Closed += (o, e) =>
            {
                if (dialog.IsCancelled)
                {
                    return;
                }

                this.XmlSamples = LoadXmlSamples();
            };
        }

        private void OnTreeViewItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                return;
            }

            this.AdjustTabTitles();

            if (e.NewItems != null)
            {
                foreach (TreeViewItemViewModel? item in e.NewItems)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    this.AddNotifyEvent(item);
                }
            }

            if (e.OldItems != null)
            {
                foreach (TreeViewItemViewModel? item in e.OldItems)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    this.RemoveNotifyEvent(item);
                }
            }
        }

        private void AddNotifyEvent(TreeViewItemViewModel item)
        {
            if (item.Children == null)
            {
                return;
            }

            item.Children.CollectionChanged += this.OnTreeViewItemCollectionChanged;
            foreach (var child in item.Children)
            {
                this.AddNotifyEvent(child);
            }
        }

        private void RemoveNotifyEvent(TreeViewItemViewModel item)
        {
            if (item.Children == null)
            {
                return;
            }

            item.Children.CollectionChanged -= this.OnTreeViewItemCollectionChanged;
            foreach (var child in item.Children)
            {
                this.AddNotifyEvent(child);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var doc in this.DocumentList)
                {
                    doc.Dispose();
                }
            }

            this.disposed = true;
        }

        /// <summary>
        /// Manages temporary cursors (such as the wait one) via the disposable pattern.
        /// Adapted from: https://stackoverflow.com/a/675686/1712861
        /// </summary>
        private class CursorOverride : IDisposable
        {
            private readonly MainWindowViewModel viewModel;

            private static readonly Stack<Cursor> stack = new Stack<Cursor>();

            public CursorOverride(MainWindowViewModel viewModel, Cursor cursor)
            {
                this.viewModel = viewModel;

                var current = stack.Count > 0 ? stack.Peek() : null;
                stack.Push(cursor);

                if (cursor != current)
                {
                    this.viewModel.SetGlobalCursor?.Invoke(this.viewModel, new DataEventArgs<Cursor>(cursor));
                }
            }

            public void Dispose()
            {
                var current = stack.Pop();

                var cursor = stack.Count > 0 ? stack.Peek() : null;

                if (cursor != current)
                {
                    this.viewModel.SetGlobalCursor?.Invoke(this.viewModel, new DataEventArgs<Cursor>(cursor));
                }
            }
        }
    }
}
