using System;
using System.IO;

namespace OfficeRibbonXEditor.ViewModels.Samples
{
    public class FileSampleViewModel : XmlSampleViewModel
    {
        public FileSampleViewModel(string path)
        {
            Path = path;
        }

        public override string Name => System.IO.Path.GetFileNameWithoutExtension(this.Path);

        public string Path { get; }

        public override string ReadContents()
        {
            try
            {
                return File.ReadAllText(this.Path);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            return string.Empty;
        }
    }
}
