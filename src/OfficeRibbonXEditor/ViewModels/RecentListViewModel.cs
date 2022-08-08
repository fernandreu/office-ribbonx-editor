using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OfficeRibbonXEditor.ViewModels
{
    public class RecentListViewModel<T> : ObservableObject
    {
        public ObservableCollection<T> Values { get; } = new ObservableCollection<T>();

        public void Add(T item)
        {
            if (Values.Contains(item))
            {
                Values.Remove(item);
            }

            Values.Insert(0, item);
        }
    }
}
