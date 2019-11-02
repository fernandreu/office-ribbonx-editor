using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeRibbonXEditor.ViewModels;

namespace OfficeRibbonXEditor.Extensions
{
    public static class TreeViewExtensions
    {
        public static IEnumerable<TreeViewItemViewModel> FindItemsByName(this IEnumerable<TreeViewItemViewModel> list, TreeViewItemViewModel reference, bool excludeSelf = true)
        {
            foreach (var item in list)
            {
                if (excludeSelf && ReferenceEquals(item, reference))
                {
                    continue;
                }

                if (item.GetType() == reference.GetType())
                {
                    if (item.Name == reference.Name)
                    {
                        yield return item;
                    }

                    // The way the tool works, there won't be nested items of the same type
                    continue;
                }

                foreach (var result in item.Children.FindItemsByName(reference, excludeSelf))
                {
                    yield return result;
                }
            }
        }
    }
}
