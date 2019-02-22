// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlSampleViewModel.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the XmlSampleViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.ViewModels
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using GalaSoft.MvvmLight;
    
    public class XmlSampleViewModel : ViewModelBase
    {
        private const string SamplesNamespace = "CustomUIEditor.Resources.Samples";

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
