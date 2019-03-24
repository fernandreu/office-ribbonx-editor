// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TreeViewItemViewModel.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Base class for all ViewModel classes displayed by TreeViewItems.
//   This acts as an adapter between a raw data object and a TreeViewItem.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OfficeRibbonXEditor.ViewModels
{
    using System.Collections.ObjectModel;

    using GalaSoft.MvvmLight;

    /// <summary>
    /// Base class for all ViewModel classes displayed by TreeViewItems.  
    /// This acts as an adapter between a raw data object and a TreeViewItem.
    /// </summary>
    public class TreeViewItemViewModel : ViewModelBase
    {
        #region Data

        private static readonly TreeViewItemViewModel DummyChild = new TreeViewItemViewModel();

        #endregion // Data

        #region Fields
        
        private bool isExpanded;
        
        private bool isSelected;
        
        private bool canHaveContents;
        
        private string contents;

        #endregion // Fields

        #region Constructors

        protected TreeViewItemViewModel(TreeViewItemViewModel parent, bool lazyLoadChildren, bool canHaveContents = true, string contents = null)
        {
            this.Parent = parent;

            this.Children = new ObservableCollection<TreeViewItemViewModel>();

            this.CanHaveContents = canHaveContents;

            this.Contents = contents;

            if (lazyLoadChildren)
            {
                this.Children.Add(DummyChild);
            }
        }

        // This is used to create the DummyChild instance.
        private TreeViewItemViewModel()
        {
        }

        #endregion // Constructors
        
        #region Presentation Members
        
        /// <summary>
        /// Gets the logical child items of this object.
        /// </summary>
        public ObservableCollection<TreeViewItemViewModel> Children { get; }
        
        /// <summary>
        /// Returns true if this object's Children have not yet been populated.
        /// </summary>
        public bool HasDummyChild => this.Children.Count == 1 && this.Children[0] == DummyChild;
        
        /// <summary>
        /// Gets or sets a value indicating whether the TreeViewItem associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get => this.isExpanded;
            set
            {
                if (!this.Set(ref this.isExpanded, value))
                {
                    return;
                }

                // Expand all the way up to the root.
                if (this.isExpanded && this.Parent != null)
                {
                    this.Parent.IsExpanded = true;
                }

                // Lazy load the child items, if necessary.
                if (this.HasDummyChild)
                {
                    this.Children.Remove(DummyChild);
                    this.LoadChildren();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the TreeViewItem associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                if (this.Set(ref this.isSelected, value) && this.isSelected)
                {
                    this.IsExpanded = true; // To select something, you should be able to see it
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this TreeViewItem can have contents edited in the code control
        /// </summary>
        public bool CanHaveContents
        {
            get => this.canHaveContents;
            set => this.Set(ref this.canHaveContents, value);
        }

        public string Contents
        {
            get => this.contents;
            set => this.Set(ref this.contents, value);
        }
        
        public TreeViewItemViewModel Parent { get; }

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
        
        #endregion // Presentation Members
    }
}
