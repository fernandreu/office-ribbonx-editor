using System.Collections.ObjectModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using OfficeRibbonXEditor.Helpers;

namespace OfficeRibbonXEditor.ViewModels.Documents;

/// <summary>
/// Base class for all ViewModel classes displayed by TreeViewItems.  
/// This acts as an adapter between a raw data object and a TreeViewItem.
/// </summary>
public partial class TreeViewItemViewModel : ObservableObject
{
    private static readonly TreeViewItemViewModel DummyChild = new TreeViewItemViewModel();

    public virtual string Name { get; set; } = string.Empty;

    protected TreeViewItemViewModel(TreeViewItemViewModel? parent, bool lazyLoadChildren, bool canHaveContents = true, string? contents = null)
        : this()
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
        SortedChildren = (ListCollectionView)CollectionViewSource.GetDefaultView(Children);
        SortedChildren.CustomSort = new CustomTreeViewItemSorter();
    }

    /// <summary>
    /// Gets the logical child items of this object.
    /// </summary>
    public ObservableCollection<TreeViewItemViewModel> Children { get; } = new ObservableCollection<TreeViewItemViewModel>();
        
    public ListCollectionView SortedChildren { get; }

    /// <summary>
    /// Returns true if this object's Children have not yet been populated.
    /// </summary>
    public bool HasDummyChild => Children.Count == 1 && Children[0] == DummyChild;
        
    /// <summary>
    /// Gets or sets a value indicating whether the TreeViewItem associated with this object is expanded.
    /// </summary>
    [ObservableProperty]
    private bool _isExpanded;

    partial void OnIsExpandedChanged(bool value)
    {
        // Expand all the way up to the root.
        if (value && Parent != null)
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
        
    /// <summary>
    /// Gets or sets a value indicating whether the TreeViewItem associated with this object is selected.
    /// </summary>
    [ObservableProperty]
    private bool _isSelected;

    partial void OnIsSelectedChanged(bool value)
    {
        if (value)
        {
            IsExpanded = true;
        }
    }
        
    /// <summary>
    /// Gets or sets a value indicating whether this TreeViewItem can have contents edited in the code control
    /// </summary>
    [ObservableProperty]
    private bool _canHaveContents;

    [ObservableProperty]
    private string? _contents;
        
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