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
using System.Xml.Schema;
using AsyncAwaitBestPractices;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Generators;
using OfficeRibbonXEditor.Documents;
using OfficeRibbonXEditor.Events;
using OfficeRibbonXEditor.Extensions;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Helpers.Xml;
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
    [Export]
    public partial class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private readonly IMessageBoxService _messageBoxService;

        private readonly IFileDialogService _fileDialogService;

        private readonly IVersionChecker _versionChecker;

        private readonly IDialogProvider _dialogProvider;

        private readonly IUrlHelper _urlHelper;

        private readonly Dictionary<Type, IContentDialogBase> _dialogs = new Dictionary<Type, IContentDialogBase>();

        /// <summary>
        /// Whether documents should be reloaded right before being saved.
        /// </summary>
        private bool _reloadOnSave = true;

        /// <summary>
        /// Whether the editor should make the whitespace / EOL characters visible.
        /// </summary>
        private bool _showWhitespaces;

        /// <summary>
        /// The version string of a newer release, if available
        /// </summary>
        private string? _newerVersion;

        private readonly Hashtable? _customUiSchemas;

        private TreeViewItemViewModel? _selectedItem;

        private bool _disposed;

        public MainWindowViewModel(
            IMessageBoxService messageBoxService, 
            IFileDialogService fileDialogService, 
            IVersionChecker versionChecker, 
            IDialogProvider dialogProvider, 
            IUrlHelper urlHelper)
        {
            _messageBoxService = messageBoxService;
            _fileDialogService = fileDialogService;
            _versionChecker = versionChecker;
            _dialogProvider = dialogProvider;
            _urlHelper = urlHelper;

            DocumentList.CollectionChanged += OnTreeViewItemCollectionChanged;

#if DEBUG
            if (IsInDesignMode)
            {
                return;
            }
#endif
            _customUiSchemas = LoadXmlSchemas();
            XmlSamples = LoadXmlSamples();
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

        private SampleFolderViewModel? _xmlSamples;

        public SampleFolderViewModel? XmlSamples
        {
            get => _xmlSamples;
            set => Set(ref _xmlSamples, value);
        }

        public ObservableCollection<ITabItemViewModel> OpenTabs { get; } = new ObservableCollection<ITabItemViewModel>();

        private ITabItemViewModel? _selectedTab;

        public ITabItemViewModel? SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (!Set(ref _selectedTab, value))
                {
                    return;
                }

                if (value != null)
                {
                    // This should help the user locate the item in the tree view
                    SelectedItem = value.Item;
                }

                RaisePropertyChanged(nameof(IsEditorTabSelected));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether documents should be reloaded right before being saved.
        /// </summary>
        public bool ReloadOnSave
        {
            get => _reloadOnSave;
            set => Set(ref _reloadOnSave, value);
        }

        public bool ShowWhitespaces
        {
            get => _showWhitespaces;
            set
            {
                if (!Set(ref _showWhitespaces, value))
                {
                    return;
                }

                Settings.Default.ShowWhitespace = value;
                foreach (var tab in OpenTabs.OfType<EditorTabViewModel>())
                {
                    tab.Lexer?.Update();
                }
            }
        }

        public string? NewerVersion
        {
            get => _newerVersion;
            set => Set(ref _newerVersion, value);
        }

        public TreeViewItemViewModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                var previousItem = _selectedItem;
                if (!Set(ref _selectedItem, value))
                {
                    return;
                }

                if (previousItem is IconViewModel icon)
                {
                    // Stop showing the editing textbox when the focus changes to something else.
                    // See: https://github.com/fernandreu/office-ribbonx-editor/issues/32
                    icon.CommitIdChange();
                }

                if (SelectedItem != null)
                {
                    SelectedItem.IsSelected = true;
                }

                RaisePropertyChanged(nameof(CurrentDocument));
                RaisePropertyChanged(nameof(IsDocumentSelected));
                RaisePropertyChanged(nameof(IsPartSelected));
                RaisePropertyChanged(nameof(IsIconSelected));
                RaisePropertyChanged(nameof(CanInsertXml12Part));
                RaisePropertyChanged(nameof(CanInsertXml14Part));
            }
        }

        public bool IsDocumentSelected => SelectedItem is OfficeDocumentViewModel;

        public bool IsPartSelected => SelectedItem is OfficePartViewModel;

        public bool IsEditorTabSelected => SelectedTab != null;

        public bool IsIconSelected => SelectedItem is IconViewModel;
        
        public bool CanInsertXml12Part => (SelectedItem is OfficeDocumentViewModel model) && model.Document.RetrieveCustomPart(XmlPart.RibbonX12) == null;

        public bool CanInsertXml14Part => (SelectedItem is OfficeDocumentViewModel model) && model.Document.RetrieveCustomPart(XmlPart.RibbonX14) == null;

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
                var elem = SelectedItem;
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

                FinishOpeningFile(file);
            }

            CheckVersionAsync(_versionChecker).SafeFireAndForget();
        }

        public IContentDialogBase LaunchDialog<TDialog>(bool showDialog = false) where TDialog : IContentDialogBase
        {
            if (!_dialogs.TryGetValue(typeof(TDialog), out var content) || content.IsClosed)
            {
                // Resolve a new dialog, as any potentially existing one is not suitable
                content = _dialogProvider.ResolveDialog<TDialog>();
            }
            
            LaunchingDialog?.Invoke(this, new LaunchDialogEventArgs(content, showDialog));
            if (content.IsUnique)
            {
                // Keep track of the new content
                _dialogs[typeof(TDialog)] = content;
            }

            return content;
        }

        public IContentDialog<TPayload> LaunchDialog<TDialog, TPayload>(TPayload payload, bool showDialog = false) where TDialog : IContentDialog<TPayload>
        {
            if (!_dialogs.TryGetValue(typeof(TDialog), out var baseContent) || baseContent.IsClosed)
            {
                // Resolve a new dialog, as any potentially existing one is not suitable
                baseContent = _dialogProvider.ResolveDialog<TDialog>();
            }
            
            if (baseContent.IsUnique)
            {
                // Keep track of the new content
                _dialogs[typeof(TDialog)] = baseContent;
            }

            var content = (TDialog) baseContent;
            if (!content.OnLoaded(payload))
            {
                // This might happen if the dialog has an associated instant action for which it doesn't need to stay open, e.g. 'Find Next' in a FindReplaceDialog
                return content;
            }
            
            LaunchingDialog?.Invoke(this, new LaunchDialogEventArgs(content, showDialog));

            return content;
        }

        [GenerateCommand]
        private void ExecuteFindCommand() => PerformFindReplaceAction(FindReplaceAction.Find);

        [GenerateCommand]
        private void ExecuteFindNextCommand() => PerformFindReplaceAction(FindReplaceAction.FindNext);

        [GenerateCommand]
        private void ExecuteFindPreviousCommand() => PerformFindReplaceAction(FindReplaceAction.FindPrevious);

        [GenerateCommand]
        private void ExecuteReplaceCommand() => PerformFindReplaceAction(FindReplaceAction.Replace);

        [GenerateCommand]
        private void ExecuteIncrementalSearchCommand() => PerformFindReplaceAction(FindReplaceAction.IncrementalSearch);

        public void PerformFindReplaceAction(FindReplaceAction action)
        {
            if (!(SelectedTab is EditorTabViewModel tab))
            {
                return;
            }

            var lexer = tab.Lexer;
            if (lexer?.Editor == null)
            {
                return;
            }

            LaunchDialog<FindReplaceDialogViewModel, (Scintilla, FindReplaceAction, FindReplace.FindAllResultsEventHandler)>((
                lexer.Editor.Scintilla,
                action,
                (o, e) => tab.OnShowResults(e)));
        }

        [GenerateCommand]
        private void ExecuteCloseDocumentCommand()
        {
            var doc = CurrentDocument;
            if (doc == null)
            {
                // Nothing to close
                return;
            }

            if (doc.HasUnsavedChanges)
            {
                var result = _messageBoxService.Show(string.Format(CultureInfo.InvariantCulture, Strings.Message_CloseUnsavedDoc_Text, doc.Name), Strings.Message_CloseUnsavedDoc_Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    SaveCommand.Execute(doc);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            var tabs = OpenTabs.OfType<EditorTabViewModel>().Where(x => doc.Children.Contains(x.Part)).ToList();
            foreach (var tab in tabs)
            {
                ExecuteCloseTabCommand(tab);
            }

            doc.Document.Dispose();
            DocumentList.Remove(doc);
        }

        [GenerateCommand]
        private void ExecuteOpenTabCommand(TreeViewItemViewModel? viewModel = null)
        {
            if (viewModel == null)
            {
                viewModel = SelectedItem;
            }

            if (viewModel is OfficePartViewModel part)
            {
                OpenPartTab(part);
            }
            else if (viewModel is IconViewModel icon)
            {
                OpenIconTab(icon);
            }
        }

        [GenerateCommand]
        public void ExecuteCloseTabCommand(ITabItemViewModel? tab = null)
        {
            tab ??= _selectedTab;
            if (tab == null)
            {
                return;
            }

            var index = OpenTabs.IndexOf(tab);
            if (index == -1)
            {
                return;
            }

            tab.ApplyChanges();

            OpenTabs.RemoveAt(index);
            if (SelectedTab == tab)
            {
                var count = OpenTabs.Count;
                SelectedTab = count > 0 ? OpenTabs[Math.Max(index, count - 1)] : null;
            }
        }

        [GenerateCommand]
        private void ExecuteInsertIconsCommand()
        {
            if (!(SelectedItem is OfficePartViewModel))
            {
                return;
            }

            _fileDialogService.OpenFilesDialog(Strings.OpenDialog_Icons_Title, Strings.Filter_Icons + "|" + Strings.Filter_All, FinishInsertingIcons);
        }

        /// <summary>
        /// This method does not change the icon Id per se, just enables the possibility of doing so in the view
        /// </summary>
        [GenerateCommand]
        private void ExecuteChangeIconIdCommand()
        {
            if (!(SelectedItem is IconViewModel icon))
            {
                return;
            }

            icon.IsEditingId = true;
        }

        private void FinishInsertingIcons(IEnumerable<string> filePaths)
        {
            if (!(SelectedItem is OfficePartViewModel part))
            {
                // If OpenFileDialog opens modally, this should not happen
                return;
            }

            bool AlreadyExistingAction(string? existingId, string? newId)
            {
                var result = _messageBoxService.Show(
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

        [GenerateCommand]
        private void ExecuteRemoveCommand()
        {
            if (SelectedItem is OfficePartViewModel part)
            {
                var result = _messageBoxService.Show(
                    Strings.Message_RemovePart_Text, 
                    Strings.Message_RemovePart_Title, 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                foreach (var tab in OpenTabs.OfType<EditorTabViewModel>())
                {
                    if (tab.Part == part)
                    {
                        ExecuteCloseTabCommand(tab);
                        break;
                    }
                }

                foreach (var tab in OpenTabs.OfType<IconTabViewModel>().ToList())
                {
                    if (part.Children.Any(x => x == tab.Icon))
                    {
                        ExecuteCloseTabCommand(tab);
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

            if (SelectedItem is IconViewModel icon)
            {
                var result = _messageBoxService.Show(
                    Strings.Message_RemoveIcon_Text, 
                    Strings.Message_RemoveIcon_Title, 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                foreach (var tab in OpenTabs.OfType<IconTabViewModel>())
                {
                    if (tab.Icon == icon)
                    {
                        ExecuteCloseTabCommand(tab);
                        break;
                    }
                }

                var parent = icon.Parent as OfficePartViewModel;
                parent?.RemoveIcon(icon.Name);
            }
        }

        /// <summary>
        /// Handles the (cancellable) closing of the entire application, getting typically triggered by the view
        /// </summary>
        [GenerateCommand]
        private void ExecuteClosingCommand(CancelEventArgs e)
        {
            foreach (var tab in OpenTabs)
            {
                tab.ApplyChanges();
            }

            foreach (var doc in DocumentList)
            {
                if (doc.HasUnsavedChanges)
                {
                    var result = _messageBoxService.Show(
                        string.Format(CultureInfo.InvariantCulture, Strings.Message_CloseUnsavedDoc_Text, doc.Name), 
                        Strings.Message_CloseUnsavedDoc_Title,
                        MessageBoxButton.YesNoCancel, 
                        MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        SaveCommand.Execute(doc);
                    }
                    else if (result == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

            // Now that it is clear we can leave the program, dispose all documents (i.e. delete the temporary unzipped files)
            foreach (var doc in DocumentList)
            {
                doc.Document.Dispose();
            }
        }

        /// <summary>
        /// Triggers the closing of the view. If linked with the view, this will also trigger the ClosingCommand,
        /// and hence no checks of whether documents should be saved first will be done.
        /// </summary>
        [GenerateCommand]
        private void ExecuteCloseCommand()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// starts the drag / drop action for opening files
        /// </summary>
        [GenerateCommand]
        private void ExecutePreviewDragEnterCommand(DragData data)
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

        [GenerateCommand]
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
                FinishOpeningFile(file);
            }

            data.Handled = true;
        }

        [GenerateCommand]
        private void ExecuteOpenDocumentCommand()
        {
            string[] filters =
                {
                    Strings.Filter_AllOfficeDocuments,
                    Strings.Filter_WordDocuments,
                    Strings.Filter_ExcelDocuments,
                    Strings.Filter_PowerPointDocuments,
                    Strings.Filter_VisioDocuments,
                    Strings.Filter_All,
                };

            _fileDialogService.OpenFileDialog(
                Strings.OpenDialog_Document_Title, 
                string.Join("|", filters), 
                FinishOpeningFile);
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Resource deallocation handled in VM's Dispose() already")]
        [GenerateCommand(Name = "RecentFileClickCommand")]
        private void FinishOpeningFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            var normalized = Path.GetFullPath(fileName);
            var existing = DocumentList.FirstOrDefault(x => Path.GetFullPath(x.Document.Name).Equals(normalized, StringComparison.InvariantCultureIgnoreCase));
            if (existing != null)
            {
                var result = _messageBoxService.Show(
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
                _messageBoxService.Show(ex.Message, Strings.Message_OpenError_Title, image: MessageBoxImage.Error);
                return;
            }

            DocumentList.Add(model);
            InsertRecentFile?.Invoke(this, new DataEventArgs<string> { Data = fileName });

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
                ExecuteOpenTabCommand(model.Children[0]);
            }
            else if (model.Children.Count > 0)
            {
                // Select the document on the TreeView
                SelectedItem = model;
            }
        }

        public EditorTabViewModel? OpenPartTab(OfficePartViewModel? part = null)
        {
            part ??= SelectedItem as OfficePartViewModel;
            if (part == null)
            {
                return null;
            }

            var tab = OpenTabs.OfType<EditorTabViewModel>().FirstOrDefault(x => x.Part == part);
            if (tab == null)
            {
                tab = new EditorTabViewModel(part, this);
                OpenTabs.Add(tab);
                AdjustTabTitle(tab);
            }

            SelectedTab = tab;
            return tab;
        }

        public IconTabViewModel? OpenIconTab(IconViewModel? icon = null)
        {
            icon ??= SelectedItem as IconViewModel;
            if (icon == null)
            {
                return null;
            }

            var tab = OpenTabs.OfType<IconTabViewModel>().FirstOrDefault(x => x.Icon == icon);
            if (tab == null)
            {
                tab = new IconTabViewModel(icon, this);
                OpenTabs.Add(tab);
                AdjustTabTitle(tab);
            }

            SelectedTab = tab;
            return tab;
        }

        public void AdjustTabTitles()
        {
            foreach (var tab in OpenTabs)
            {
                AdjustTabTitle(tab);
            }
        }

        public void AdjustTabTitle(ITabItemViewModel tab)
        {
            if (tab == null)
            {
                return;
            }

            var result = tab.Item.Name;
            var targets = DocumentList.FindItemsByName(tab.Item).ToList();
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

        [GenerateCommand]
        private void ExecuteSaveCommand(OfficeDocumentViewModel? document)
        {
            document ??= CurrentDocument;
            if (document == null)
            {
                return;
            }
            
            foreach (var tab in OpenTabs)
            {
                tab.ApplyChanges();
            }

            try
            {
                using (new CursorOverride(this, Cursors.Wait))
                {
                    document.Save(ReloadOnSave, preserveAttributes: Settings.Default.PreserveAttributes);
                }
            }
            catch (Exception ex)
            {
                _messageBoxService.Show(ex.Message, Strings.Message_SaveError_Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [GenerateCommand]
        private void ExecuteSaveAllCommand()
        {
            foreach (var tab in OpenTabs)
            {
                tab.ApplyChanges();
            }

            try
            {
                foreach (var doc in DocumentList)
                {
                    doc.Save(ReloadOnSave, preserveAttributes: Settings.Default.PreserveAttributes);
                }
            }
            catch (Exception ex)
            {
                _messageBoxService.Show(ex.Message, Strings.Message_SaveError_Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [GenerateCommand]
        private void ExecuteSaveAsCommand() => SaveAs(true);

        [GenerateCommand]
        private void ExecuteSaveACopyAsCommand() => SaveAs(false);

        private void SaveAs(bool renameCurrent)
        {
            var doc = CurrentDocument;
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

            _fileDialogService.SaveFileDialog(
                Strings.SaveDialog_SaveAs_Title, 
                string.Join("|", filters),
                path => FinishSavingFile(path, renameCurrent), 
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
            var doc = CurrentDocument;
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
                    doc.Save(_reloadOnSave, fileName, Settings.Default.PreserveAttributes);
                }
            }
            catch (Exception ex)
            {
                _messageBoxService.Show(ex.Message, Strings.Message_SaveError_Title, image: MessageBoxImage.Error);
                return;
            }
            
            InsertRecentFile?.Invoke(this, new DataEventArgs<string> { Data = fileName });

            if (renameCurrent)
            {
                doc.Document.Name = fileName;

                // Ensure name is updated in the TreeView
                doc.RaisePropertyChanged(nameof(doc.Name));
            }
        }

        private static Hashtable LoadXmlSchemas()
        {
            var missing = new List<XmlPart>();

            var schema12 = Schema.Load(XmlPart.RibbonX12);
            if (schema12 == null)
            {
                missing.Add(XmlPart.RibbonX12);
            }

            var schema14 = Schema.Load(XmlPart.RibbonX14);
            if (schema14 == null)
            {
                missing.Add(XmlPart.RibbonX14);
            }

            if (missing.Count > 0)
            {
                throw new InvalidOperationException($"The following schemas are invalid: {string.Join(", ", missing)}. The tool might be corrupted");
            }

            return new Hashtable(2)
            {
                {XmlPart.RibbonX12, schema12},
                {XmlPart.RibbonX14, schema14},
            };
        }

        private static SampleFolderViewModel? LoadXmlSamples()
        {
            return SampleUtils.LoadXmlSamples(
                Settings.Default.CustomSamples.Split('\n'), 
                Settings.Default.ShowDefaultSamples);
        }

        [GenerateCommand]
        private void ExecuteInsertXml14Command() => CurrentDocument?.InsertPart(XmlPart.RibbonX14);

        [GenerateCommand]
        private void ExecuteInsertXml12Command() => CurrentDocument?.InsertPart(XmlPart.RibbonX12);

        /// <summary>
        /// Inserts an XML sample to the selected document in the tree
        /// </summary>
        [GenerateCommand]
        private void ExecuteInsertXmlSampleCommand(XmlSampleViewModel sample)
        {

            // TODO: This command should be clearer with its target
            // Right now, it is the selected item, but users might find it more intuitive to insert the sample in
            // the already opened tab. To fix this, the easiest thing to do would be to have a separate command for
            // each (i.e. a context menu action for the tree view and a menu action for the editor)

            var newPart = false;
            
            if (SelectedItem is OfficeDocumentViewModel doc)
            {
                // See if there is already a part, and otherwise insert one
                if (doc.Children.Count == 0)
                {
                    doc.InsertPart(XmlPart.RibbonX12);
                    newPart = true;
                }

                SelectedItem = doc.Children[0];
            }
            
            if (!(SelectedItem is OfficePartViewModel part))
            {
                return;
            }
            
            // Show message box for confirmation
            if (!newPart)
            {
                var result = _messageBoxService.Show(
                    Strings.Message_InsertSample_Text, 
                    Strings.Message_InsertSample_Title, 
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            var tab = OpenTabs.OfType<EditorTabViewModel>().FirstOrDefault(x => x.Part == part) ?? OpenPartTab(part);
            if (tab == null)
            {
                throw new InvalidOperationException("Could not find / create a tab for inserting the XML sample");
            }

            try
            {
                var data = sample.ReadContents();

                // Make sure the xml schema is not for the wrong part type
                if (_customUiSchemas != null &&
                    part.Part != null &&
                    _customUiSchemas[part.Part.PartType] is XmlSchema thisSchema && 
                    _customUiSchemas[part.Part.PartType == XmlPart.RibbonX12 ? XmlPart.RibbonX14 : XmlPart.RibbonX12] is XmlSchema otherSchema &&
                    otherSchema.TargetNamespace != null)
                {
                    data = data.Replace(otherSchema.TargetNamespace, thisSchema.TargetNamespace, StringComparison.OrdinalIgnoreCase);
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
                _messageBoxService.Show(ex.Message, Strings.Message_InsertSampleError_Title);
            }
        }

        [GenerateCommand]
        private void ExecuteValidateCommand() => ValidateXml(true);

        private bool ValidateXml(bool showValidMessage)
        {
            if (!(SelectedTab is EditorTabViewModel tab))
            {
                return false;
            }

            tab.ApplyChanges();
            var part = tab.Part;

            // Test to see if text is XML first
            if (_customUiSchemas == null || part.Part == null || !(_customUiSchemas[part.Part.PartType] is XmlSchema targetSchema))
            {
                return false;
            }

            var errorList = XmlValidation.Validate(part?.Contents, targetSchema);

            tab.OnShowResults(new ResultsEventArgs(new XmlErrorResults(errorList)));

            if (errorList.Count == 0)
            {
                if (showValidMessage)
                {
                    _messageBoxService.Show(
                        Strings.Message_ValidXml_Text,
                        Strings.Message_ValidXml_Title,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                return true;
            }

            return false;
        }

        [GenerateCommand]
        private void ExecuteGenerateCallbacksCommand()
        {
            // TODO: Check whether any text is selected, and generate callbacks only for that text
            if (SelectedTab == null)
            {
                return;
            }

            SelectedTab.ApplyChanges();
            
            if (!(SelectedTab is EditorTabViewModel tab))
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
                    _messageBoxService.Show(Strings.Message_NoCallbacks_Text, Strings.Message_NoCallbacks_Title, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                LaunchDialog<CallbackDialogViewModel, string?>(callbacks.ToString());
            }
            catch (Exception ex)
            {
                _messageBoxService.Show(ex.Message, Strings.Message_CallbackError_Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [GenerateCommand]
        private void ExecuteGoToCommand()
        {
            if (!(SelectedTab is EditorTabViewModel tab))
            {
                return;
            }

            var lexer = tab.Lexer;
            if (lexer == null)
            {
                return;
            }

            LaunchDialog<GoToDialogViewModel, ScintillaLexer>(lexer);
        }

        [GenerateCommand]
        private void ExecuteToggleCommentCommand()
        {
            if (!(SelectedTab is EditorTabViewModel tab))
            {
                return;
            }

            var data = tab.EditorInfo;
            if (data == null)
            {
                return;
            }

            // Extend the selection to pick full lines
            const string newLine = "\n";
            var start = data.Text.LastIndexOf(newLine, data.Selection.Item1, StringComparison.Ordinal) + 1;

            var end = data.Text.IndexOf(newLine, data.Selection.Item2, StringComparison.Ordinal);
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
            var lines = data.Text.Substring(start, end - start).Split(new[] { newLine }, StringSplitOptions.None);
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
            var combined = string.Join(newLine, lines);

            // Update the selected item's current contents to that, and notify the editor
            tab.OnUpdateEditor(new EditorChangeEventArgs { Start = start, End = end, NewText = combined, UpdateSelection = true });
        }

        private async Task CheckVersionAsync(IVersionChecker versionChecker)
        {
            NewerVersion = await versionChecker.CheckVersionAsync().ConfigureAwait(false);
        }

        [GenerateCommand]
        private void ExecuteNewerVersionCommand()
        {
            var result = _messageBoxService.Show(
                string.Format(CultureInfo.InvariantCulture, Strings.Message_NewVersion_Text, _newerVersion),
                Strings.Message_NewVersion_Title, 
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            _urlHelper.OpenRelease();
        }

        [GenerateCommand]
        private void ExecuteOpenHelpLinkCommand(string url)
        {
            _urlHelper.OpenExternal(new Uri(url));
        }

        [GenerateCommand]
        private void ExecuteShowSettingsCommand()
        {
            var dialog = LaunchDialog<SettingsDialogViewModel, ICollection<ITabItemViewModel>>(OpenTabs);
            dialog.Closed += (o, e) =>
            {
                if (dialog.IsCancelled)
                {
                    return;
                }

                XmlSamples = LoadXmlSamples();
            };
        }

        [GenerateCommand]
        private void ExecuteShowAboutCommand() => LaunchDialog<AboutDialogViewModel>(true);

        private void OnTreeViewItemCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                return;
            }

            AdjustTabTitles();

            if (e.NewItems != null)
            {
                foreach (TreeViewItemViewModel? item in e.NewItems)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    AddNotifyEvent(item);
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

                    RemoveNotifyEvent(item);
                }
            }
        }

        private void AddNotifyEvent(TreeViewItemViewModel item)
        {
            if (item.Children == null)
            {
                return;
            }

            item.Children.CollectionChanged += OnTreeViewItemCollectionChanged;
            foreach (var child in item.Children)
            {
                AddNotifyEvent(child);
            }
        }

        private void RemoveNotifyEvent(TreeViewItemViewModel item)
        {
            if (item.Children == null)
            {
                return;
            }

            item.Children.CollectionChanged -= OnTreeViewItemCollectionChanged;
            foreach (var child in item.Children)
            {
                AddNotifyEvent(child);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var doc in DocumentList)
                {
                    doc.Dispose();
                }
            }

            _disposed = true;
        }

        /// <summary>
        /// Manages temporary cursors (such as the wait one) via the disposable pattern.
        /// Adapted from: https://stackoverflow.com/a/675686/1712861
        /// </summary>
        private class CursorOverride : IDisposable
        {
            private readonly MainWindowViewModel _viewModel;

            private static readonly Stack<Cursor> Stack = new Stack<Cursor>();

            public CursorOverride(MainWindowViewModel viewModel, Cursor cursor)
            {
                _viewModel = viewModel;

                var current = Stack.Count > 0 ? Stack.Peek() : null;
                Stack.Push(cursor);

                if (cursor != current)
                {
                    _viewModel.SetGlobalCursor?.Invoke(_viewModel, new DataEventArgs<Cursor>(cursor));
                }
            }

            public void Dispose()
            {
                var current = Stack.Pop();

                var cursor = Stack.Count > 0 ? Stack.Peek() : null;

                if (cursor != current)
                {
                    _viewModel.SetGlobalCursor?.Invoke(_viewModel, new DataEventArgs<Cursor>(cursor));
                }
            }
        }
    }
}
