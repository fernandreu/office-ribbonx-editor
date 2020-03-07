using System.Collections.Generic;
using System.Linq;

namespace OfficeRibbonXEditor.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T item, int index)> Enumerated<T>(this IEnumerable<T> self)
        {
            return self.Select((x, i) => (x, i));
        }
    }
}
