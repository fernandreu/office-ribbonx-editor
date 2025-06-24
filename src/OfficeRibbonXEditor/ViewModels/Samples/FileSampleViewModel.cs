using System;
using System.IO;

namespace OfficeRibbonXEditor.ViewModels.Samples;

public class FileSampleViewModel : XmlSampleViewModel
{
    public FileSampleViewModel(string path)
    {
        Path = path;
    }

    public override string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

    public string Path { get; }

    public override string ReadContents()
    {
        try
        {
            return File.ReadAllText(Path);
        }
        catch (IOException)
        {
            // Files probably does not exist
        }
        catch (UnauthorizedAccessException)
        {
            // User does not have access to the file / folder
        }

        return string.Empty;
    }
}