using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace CustomUIEditor.Data
{
    /// <summary>
    /// Base class for all ViewModel classes displayed by TreeViewItems.  
    /// This acts as an adapter between a raw data object and a TreeViewItem.
    /// </summary>
    public class TreeViewItemViewModel : INotifyPropertyChanged
    {
        #region Data

        static readonly TreeViewItemViewModel DummyChild = new TreeViewItemViewModel();
        bool _isExpanded;
        bool _isSelected;
        bool _canHaveContents;
        string _contents;

        #endregion // Data

        #region Constructors

        protected TreeViewItemViewModel(TreeViewItemViewModel parent, bool lazyLoadChildren, bool canHaveContents = true, string contents = null)
        {
            Parent = parent;

            Children = new ObservableCollection<TreeViewItemViewModel>();

            CanHaveContents = canHaveContents;

            Contents = contents;

            if (lazyLoadChildren)
                Children.Add(DummyChild);
        }

        // This is used to create the DummyChild instance.
        private TreeViewItemViewModel()
        {
        }

        #endregion // Constructors

        #region Presentation Members

        #region Children

        /// <summary>
        /// Returns the logical child items of this object.
        /// </summary>
        public ObservableCollection<TreeViewItemViewModel> Children { get; }

        #endregion // Children

        #region HasLoadedChildren

        /// <summary>
        /// Returns true if this object's Children have not yet been populated.
        /// </summary>
        public bool HasDummyChild => Children.Count == 1 && Children[0] == DummyChild;

        #endregion // HasLoadedChildren

        #region IsExpanded

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }

                // Expand all the way up to the root.
                if (_isExpanded && Parent != null)
                    Parent.IsExpanded = true;

                // Lazy load the child items, if necessary.
                if (HasDummyChild)
                {
                    Children.Remove(DummyChild);
                    LoadChildren();
                }
            }
        }

        #endregion // IsExpanded

        #region IsSelected

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value == _isSelected) return;
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                if (_isSelected) IsExpanded = true;  // To select something, you should be able to see it
            }
        }

        #endregion // IsSelected

        #region CanHaveContents

        /// <summary>
        /// Gets/sets whether this TreeViewItem can have contents edited in the code control
        /// </summary>
        public bool CanHaveContents
        {
            get => _canHaveContents;
            set
            {
                if (value == _canHaveContents) return;
                _canHaveContents = value;
                OnPropertyChanged(nameof(CanHaveContents));
            }
        }

        /// <summary>
        /// The exact opposite of CanHaveContents, in case it is needed from xaml (if only needed 
        /// once, this is more concise than having a easier than having a bool converter)
        /// </summary>
        public bool CannotHaveContents
        {
            get => !CanHaveContents;
            set => CanHaveContents = !value;
        }

        #endregion // CanHaveContents

        #region Contents

        public string Contents
        {
            get => _contents;
            set
            {
                if (value == _contents) return;
                _contents = value;
                OnPropertyChanged(nameof(Contents));
            }
        }

        #endregion // Contents

        #region LoadChildren

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

        #endregion // LoadChildren

        #region Parent

        public TreeViewItemViewModel Parent { get; }

        #endregion // Parent

        #endregion // Presentation Members

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members
    }
}
