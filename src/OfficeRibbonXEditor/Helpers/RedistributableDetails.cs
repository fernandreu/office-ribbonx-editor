namespace OfficeRibbonXEditor.Helpers;

public class RedistributableDetails(string neededVersion, string processArchitecture, Uri downloadLink)
{
    public string NeededVersion { get; } = neededVersion;

    public string ProcessArchitecture { get; } = processArchitecture;

    public Uri DownloadLink { get; } = downloadLink;

    public string? InstalledVersion { get; set; }

    public bool NeedsDownload { get; set; } = true;
}