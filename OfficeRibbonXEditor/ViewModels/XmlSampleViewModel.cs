using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using GalaSoft.MvvmLight;

namespace OfficeRibbonXEditor.ViewModels
{
    public class XmlSampleViewModel : ViewModelBase
    {
        private static readonly string SamplesNamespace = $"{nameof(OfficeRibbonXEditor)}.{nameof(Resources)}.Samples";

        public string ResourceName { get; set; }

        public string Name => this.ResourceName?.Substring(SamplesNamespace.Length + 1);

        public static IEnumerable<XmlSampleViewModel> GetFromAssembly()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly
                .GetManifestResourceNames()
                .Where(r => r.StartsWith(SamplesNamespace) && r.EndsWith(".xml"))
                .Select(r => new XmlSampleViewModel { ResourceName = r });
        }

        public static string ReadContents(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            // ReSharper disable once AssignNullToNotNullAttribute
            using (var sr = new StreamReader(assembly.GetManifestResourceStream(resourceName)))
            {
                return sr.ReadToEnd();
            }
        }

        public string ReadContents()
        {
            return ReadContents(this.ResourceName);
        }
    }
}
