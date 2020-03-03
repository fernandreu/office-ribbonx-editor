using System;
using System.Collections.Generic;
using System.IO;
using OfficeRibbonXEditor.Properties;
using OfficeRibbonXEditor.ViewModels.Samples;

namespace OfficeRibbonXEditor.Helpers
{
    public static class SampleUtils
    {

        private static SampleFolderViewModel? ScanSampleFolder(string path)
        {
            var result = new SampleFolderViewModel
            {
                Name = new DirectoryInfo(path).Name,
            };

            foreach (var directory in Directory.GetDirectories(path))
            {
                var folder = ScanSampleFolder(directory);
                if (folder == null)
                {
                    continue;
                }

                result.Items.Add(folder);
            }

            foreach (var file in Directory.GetFiles(path, "*.xml"))
            {
                result.Items.Add(new FileSampleViewModel(file));
            }

            return result.Items.Count > 0 ? result : null;
        }

        public static SampleFolderViewModel? LoadXmlSamples(IEnumerable<string> sources, bool showDefault)
        {
            var root = new SampleFolderViewModel
            {
                Name = "XML Sample",
            };

            foreach (var source in sources)
            {
                if (string.IsNullOrWhiteSpace(source))
                {
                    continue;
                }

                var trimmed = source.Trim();

                if (trimmed.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase) && File.Exists(trimmed))
                {
                    root.Items.Add(new FileSampleViewModel(trimmed));
                }

                if (!Directory.Exists(trimmed))
                {
                    continue;
                }

                var folder = ScanSampleFolder(trimmed);
                if (folder == null)
                {
                    continue;
                }

                root.Items.Add(folder);
            }

            if (showDefault)
            {
                foreach (var sample in EmbeddedSampleViewModel.GetFromAssembly())
                {
                    root.Items.Add(sample);
                }
            }

            return root.Items.Count > 0 ? root : null;
        }
    }
}
