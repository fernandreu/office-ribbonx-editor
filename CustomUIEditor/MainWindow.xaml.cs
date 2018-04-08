using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CustomUIEditor.Data;
using Microsoft.Win32;

namespace CustomUIEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<OfficeDocumentViewModel> DocumentList { get; } = new ObservableCollection<OfficeDocumentViewModel>();

        public CodeViewModel Code { get; } = new CodeViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void OpenFile()
        {
            var ofd = new OpenFileDialog();

            #region Initializing Open Document Dialog
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
            #endregion

            ofd.FileOk += FinishOpeningFile;
            ofd.ShowDialog(this);
        }
        
        private void FinishOpeningFile(object sender, CancelEventArgs e)
        {
            try
            {
                //Debug.Assert(this.package == null);
                //if (this.package != null)
                //{
                //    this.package.Dispose();
                //    this.PackageClosed();
                //}

                string fileName = ((OpenFileDialog) sender).FileName;
                if (string.IsNullOrEmpty(fileName)) return;

                Debug.WriteLine("Opening " + fileName + "...");

                var doc = new OfficeDocument(fileName);
                var model = new OfficeDocumentViewModel(doc);
                if (model.Children.Count > 0) model.Children[0].IsSelected = true;
                DocumentList.Add(model);

                //this.package = new OfficeDocument(fileName);
                //this.PackageLoaded();
                //this.PackageTreeView();

                //UndoRedo
                //_commands = new UndoRedo.Control.Commands(rtbCustomUI.Rtf);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Title);
            }
        }

        #region Disable horizontal scrolling when selecting TreeView item
    
        bool _suppressRequestBringIntoView;

        private void TreeViewItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            // Ignore re-entrant calls
            if (_suppressRequestBringIntoView)
                return;

            // Cancel the current scroll attempt
            e.Handled = true;

            // Call BringIntoView using a rectangle that extends into "negative space" to the left of our
            // actual control. This allows the vertical scrolling behaviour to operate without adversely
            // affecting the current horizontal scroll position.
            _suppressRequestBringIntoView = true;

            if (sender is TreeViewItem item)
            {
                var newTargetRect = new Rect(-1000, 0, item.ActualWidth + 1000, item.ActualHeight);
                item.BringIntoView(newTargetRect);
            }

            _suppressRequestBringIntoView = false;
        }

        // The call to BringIntoView() in TreeViewItem_OnSelected is also important

        #endregion
        
        private void TreeViewItem_OnSelected(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;

            // Correctly handle horizontally scrolling for programmatically selected items
            item.BringIntoView();
            e.Handled = true;

            if (item.DataContext is OfficePartViewModel partModel)
            {
                Code.RawText = partModel.Part.ReadContent();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void ShowHelp_CustomizeTheRibbon(object sender, RoutedEventArgs e)
        {
            Process.Start("https://msdn.microsoft.com/en-us/library/aa338202(v=office.14).aspx");
        }

        private void ShowHelp_CustomizeTheOustpace(object sender, RoutedEventArgs e)
        {
            Process.Start("https://msdn.microsoft.com/en-us/library/ee691833(office.14).aspx");
        }

        private void ShowHelp_CommandIdentifiers(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/OfficeDev/office-fluent-ui-command-identifiers");
        }

        private void ShowHelp_VSTO(object sender, RoutedEventArgs e)
        {
            Process.Start("https://msdn.microsoft.com/en-us/library/jj620922.aspx");
        }

        private void ShowHelp_OfficeDevCenter(object sender, RoutedEventArgs e)
        {
            Process.Start("https://dev.office.com/");
        }

        private void ShowHelp_RepurposingControls(object sender, RoutedEventArgs e)
        {
            Process.Start("https://blogs.technet.microsoft.com/the_microsoft_excel_support_team_blog/2012/06/18/how-to-repurpose-a-button-in-excel-2007-or-2010/");
        }

        void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            // TODO: Warning

            foreach (var doc in DocumentList)
            {
                doc.Document.Dispose();
            }
        }
    }
}