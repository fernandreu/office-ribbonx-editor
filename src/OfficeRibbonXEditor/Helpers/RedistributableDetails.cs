using System;
using System.Runtime.InteropServices;

namespace OfficeRibbonXEditor.Helpers
{
    public class RedistributableDetails
    {
        public RedistributableDetails(string neededVersion, string processArchitecture, Uri downloadLink)
        {
            NeededVersion = neededVersion;
            ProcessArchitecture = processArchitecture;
            DownloadLink = downloadLink;
        }

        public string NeededVersion { get; }

        public string ProcessArchitecture { get; }

        public Uri DownloadLink { get; }

        public string? InstalledVersion { get; set; }

        public bool NeedsDownload { get; set; } = true;
    }
}
