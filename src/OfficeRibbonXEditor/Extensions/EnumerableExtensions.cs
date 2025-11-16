namespace OfficeRibbonXEditor.Extensions;

public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> self)
    {
        public IEnumerable<(T item, int index)> Enumerated()
        {
            return self.Select((x, i) => (x, i));
        }
    }
}