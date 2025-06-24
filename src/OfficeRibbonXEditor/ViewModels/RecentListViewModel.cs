using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OfficeRibbonXEditor.ViewModels;

public class RecentListViewModel<T> : ObservableObject
{
    public ObservableCollection<T> Values { get; } = [];

    public void Add(T item)
    {
        Values.Remove(item);
        Values.Insert(0, item);
    }
}