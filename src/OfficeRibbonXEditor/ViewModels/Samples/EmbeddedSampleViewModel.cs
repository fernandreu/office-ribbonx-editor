using System.IO;
using System.Reflection;

namespace OfficeRibbonXEditor.ViewModels.Samples;

public class EmbeddedSampleViewModel(string resourceName) : XmlSampleViewModel
{
    public string ResourceName { get; } = resourceName;

    private static readonly string SamplesNamespace = $"{nameof(OfficeRibbonXEditor)}.{nameof(Resources)}.Samples";

    // The name will not contain the .xml extension
    public override string Name => ResourceName.Substring(SamplesNamespace.Length + 1, ResourceName.Length - SamplesNamespace.Length - 5);

    public static IEnumerable<XmlSampleViewModel> GetFromAssembly()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly
            .GetManifestResourceNames()
            .Where(r => r.StartsWith(SamplesNamespace, StringComparison.OrdinalIgnoreCase) && r.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            .Select(r => new EmbeddedSampleViewModel(r));
    }
        
    private static string ReadContents(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            return string.Empty;
        }

        using var sr = new StreamReader(stream);
        return sr.ReadToEnd();
    }

    public override string ReadContents()
    {
        return ReadContents(ResourceName);
    }
}