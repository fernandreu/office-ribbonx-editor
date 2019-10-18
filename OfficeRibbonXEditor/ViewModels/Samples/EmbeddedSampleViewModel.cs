using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OfficeRibbonXEditor.ViewModels.Samples
{
    public class EmbeddedSampleViewModel : XmlSampleViewModel
    {
        public string ResourceName { get; set; }

        private static readonly string samplesNamespace = $"{nameof(OfficeRibbonXEditor)}.{nameof(Resources)}.Samples";

        public override string Name => this.ResourceName?.Substring(samplesNamespace.Length + 1);

        public static IEnumerable<XmlSampleViewModel> GetFromAssembly()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly
                .GetManifestResourceNames()
                .Where(r => r.StartsWith(samplesNamespace) && r.EndsWith(".xml"))
                .Select(r => new EmbeddedSampleViewModel { ResourceName = r });
        }

        private static string ReadContents(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            // ReSharper disable once AssignNullToNotNullAttribute
            using (var sr = new StreamReader(assembly.GetManifestResourceStream(resourceName)))
            {
                return sr.ReadToEnd();
            }
        }

        public override string ReadContents()
        {
            return ReadContents(this.ResourceName);
        }
    }
}
