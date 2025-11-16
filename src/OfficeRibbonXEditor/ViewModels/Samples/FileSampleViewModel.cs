using System.IO;

namespace OfficeRibbonXEditor.ViewModels.Samples;

public class FileSampleViewModel(string path) : XmlSampleViewModel
{
    public override string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

    public string Path { get; } = path;

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