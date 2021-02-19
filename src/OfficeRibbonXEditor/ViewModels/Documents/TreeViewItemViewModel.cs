using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace OfficeRibbonXEditor.ViewModels.Documents
{
    /// <summary>
    /// Base class for all ViewModel classes displayed by TreeViewItems.  
    /// This acts as an adapter between a raw data object and a TreeViewItem.
    /// </summary>
    public class TreeViewItemViewModel : ViewModelBase
    {
        private static readonly TreeViewItemViewModel DummyChild = new TreeViewItemViewModel();

        public virtual string Name { get; set; } = string.Empty;

        protected TreeViewItemViewModel(TreeViewItemViewModel? parent, bool lazyLoadChildren, bool canHaveContents = true, string? contents = null)
        {
            Parent = parent;

            CanHaveContents = canHaveContents;

            Contents = contents;

            if (lazyLoadChildren)
            {
                Children.Add(DummyChild);
            }
        }

        // This is used to create the DummyChild instance.
        private TreeViewItemViewModel()
        {
        }

        /// <summary>
        /// Gets the logical child items of this object.
        /// </summary>
        public ObservableCollection<TreeViewItemViewModel> Children { get; } = new ObservableCollection<TreeViewItemViewModel>();
        
        /// <summary>
        /// Returns true if this object's Children have not yet been populated.
        /// </summary>
        public bool HasDummyChild => Children.Count == 1 && Children[0] == DummyChild;
        
        private bool _isExpanded;
        /// <summary>
        /// Gets or sets a value indicating whether the TreeViewItem associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (!Set(ref _isExpanded, value))
                {
                    return;
                }

                // Expand all the way up to the root.
                if (_isExpanded && Parent != null)
                {
                    Parent.IsExpanded = true;
                }

                // Lazy load the child items, if necessary.
                if (HasDummyChild)
                {
                    Children.Remove(DummyChild);
                    LoadChildren();
                }
            }
        }

        private bool _isSelected;
        /// <summary>
        /// Gets or sets a value indicating whether the TreeViewItem associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (Set(ref _isSelected, value) && _isSelected)
                {
                    IsExpanded = true; // To select something, you should be able to see it
                }
            }
        }

        private bool _canHaveContents;
        /// <summary>
        /// Gets or sets a value indicating whether this TreeViewItem can have contents edited in the code control
        /// </summary>
        public bool CanHaveContents
        {
            get => _canHaveContents;
            set => Set(ref _canHaveContents, value);
        }

        private string? _contents;
        public string? Contents
        {
            get => _contents;
            set => Set(ref _contents, value);
        }
        
        public TreeViewItemViewModel? Parent { get; }

        /// <summary>
        /// Invoked when the child items need to be loaded on demand.
        /// Subclasses can override this to populate the Children collection.
        /// </summary>
        protected virtual void LoadChildren()
        {
        }

        /// <summary>
        /// Invoked when IsSelected turns to false, in case derived classes want to perform
        /// actions such as saving the current code being shown
        /// </summary>
        protected virtual void SelectionLost()
        {
        }
    }
}
