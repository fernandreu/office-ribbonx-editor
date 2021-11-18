using OfficeRibbonXEditor.ViewModels.Documents;
using System.Collections;

namespace OfficeRibbonXEditor.Helpers
{
    public class CustomTreeViewItemSorter : IComparer
    {
        public int Compare(object? x, object? y)
        {
            if (x is not TreeViewItemViewModel viewModelX || y is not TreeViewItemViewModel viewModelY)
            {
                // Throwing might make more sense
                return 0;
            }

            return viewModelX.Name?.CompareTo(viewModelY.Name) ?? 0;
        }
    }
}
