using System.Collections.Generic;

namespace OfficeRibbonXEditor.Interfaces;

/// <summary>
/// Holds the list of recent files used by the corresponding user control, with the storage method used left up
/// to the implementation to decide (e.g. registry, XML in app data)
/// </summary>
public interface IPersist
{
    List<string> RecentFiles(int max);

    void InsertFile(string filepath, int max);

    void RemoveFile(string filepath, int max);
}