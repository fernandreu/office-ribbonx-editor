using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Win32;
using NUnit.Framework;
using OfficeRibbonXEditor.Views.Controls;

namespace OfficeRibbonXEditor.UnitTests.Views.Controls;

[Apartment(ApartmentState.STA)]
public abstract class RecentFileListTests
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. Always defined in SetUp
    protected RecentFileList Control { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    [Test]
    public void CanAddFiles()
    {
        // Arrange
        using var stream = TempFile();
        Assume.That(Control.RecentFiles, Is.Empty);

        // Act
        Control.InsertFile(stream.Name);

        // Assert
        Assert.That(Control.RecentFiles, Does.Contain(stream.Name));
    }

    [Test]
    public void CanRemoveFiles()
    {
        // Arrange
        using var stream = TempFile();
        Assume.That(Control.RecentFiles, Is.Empty);

        // Act
        Control.InsertFile(stream.Name);
        Control.RemoveFile(stream.Name);

        // Assert
        Assert.That(Control.RecentFiles, Is.Empty);
    }

    [TestCase(1)]
    [TestCase(5)]
    [TestCase(10)]
    public void CannotPassMaxLimit(int maxFiles)
    {
        // Arrange
        Control.MaxNumberOfFiles = maxFiles;
        var collection = new List<FileStream>(maxFiles + 5);
        for (var i = 0; i < maxFiles + 5; ++i)
        {
            collection.Add(TempFile());
        }

        // Act
        foreach (var stream in collection)
        {
            Control.InsertFile(stream.Name);
        }

        // Assert
        try
        {
            Assert.That(Control.RecentFiles.Count, Is.EqualTo(maxFiles));
        }
        finally
        {
            foreach (var stream in collection)
            {
                stream.Close();
            }
        }
    }

    private static FileStream TempFile()
    {
        return new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
    }
}

public class RecentFileTestsWithFilePersister : RecentFileListTests
{
    private string? _filePath;

    [SetUp]
    public void SetUp()
    {
        do
        {
            _filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        } while (File.Exists(_filePath));

        Control = new RecentFileList();
        Control.UseXmlPersister(_filePath);
    }

    [TearDown]
    public void TearDown()
    {
        if (_filePath != null && File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }
}

public class RecentFileTestsWithRegistryPersister : RecentFileListTests
{
    private string? _registryKey;

    [SetUp]
    public void SetUp()
    {
        do
        {
            _registryKey = "Software\\" + Path.GetRandomFileName();
        } while (Registry.CurrentUser.OpenSubKey(_registryKey) != null);

        Control = new RecentFileList();
        Control.UseRegistryPersister(_registryKey);
    }

    [TearDown]
    public void TearDown()
    {
        if (_registryKey != null && Registry.CurrentUser.OpenSubKey(_registryKey) != null)
        {
            Registry.CurrentUser.DeleteSubKey(_registryKey);
        }
    }
}