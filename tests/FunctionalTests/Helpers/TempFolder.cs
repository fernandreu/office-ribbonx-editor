using System;
using System.IO;

namespace OfficeRibbonXEditor.FunctionalTests.Helpers;

public class TempFolder : IDisposable
{
    public TempFolder()
    {
        var folder = Path.GetTempPath();
        do
        {
            Name = Path.GetRandomFileName();
            FullName = Path.Combine(folder, Name);
        }
        while (Directory.Exists(FullName));

        Directory.CreateDirectory(FullName);
    }

    public string Name { get; }

    public string FullName { get; }

    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // We are treating the temporary folder as an unmanaged resource which has
                // to be deleted even if we forget to call Dispose()
            }

            Directory.Delete(FullName, true);
            _disposedValue = true;
        }
    }

    ~TempFolder()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}