using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace OfficeRibbonXEditor.ViewModels
{
    public class RecentListViewModel<T> : ViewModelBase
    {
        public ObservableCollection<T> Values { get; } = new ObservableCollection<T>();

        public void Add(T item)
        {
            if (this.Values.Contains(item))
            {
                this.Values.Remove(item);
            }

            this.Values.Insert(0, item);
        }
    }
}
