using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.Services;

[Export(typeof(IVersionChecker))]
public class VersionChecker : IVersionChecker
{
    [SuppressMessage("SonarLint", "S1075", Justification = "This warning is due to the hard-coded url. If this ever change (e.g. repo is moved), the code will need changes overall anyway")]
    private const string CheckUrl = "https://api.github.com/repos/fernandreu/office-ribbonx-editor/releases/latest";

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public async Task<string?> CheckToolVersionAsync(CancellationToken cancelToken = default)
    {
        try
        {
            var latestVersion = await GetVersionAsync(cancelToken).ConfigureAwait(false);
            if (latestVersion == null)
            {
                return null;
            }

            var current = Assembly.GetExecutingAssembly().GetName().Version;
            Debug.WriteLine($"Latest version: {latestVersion}; current: {current}");
            return latestVersion > current ? latestVersion.ToString() : null;
        }
        catch (Exception e)
        {
            Debug.Fail(e.Message); 
            return null;
        }
    }

    private static async Task<Version?> GetVersionAsync(CancellationToken cancelToken)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        httpClient.DefaultRequestHeaders.Add("User-Agent", "request");

        var uri = new Uri(CheckUrl);
        using var response = await httpClient.GetAsync(uri, cancelToken).ConfigureAwait(false);
        var contentString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        // We just need the "tag_name" field from the response. We could deserialize everything with
        // JSON.Net and obtain that field, but that adds an extra (mid-size) library just for one
        // field. Hence, using RegEx instead, which should do just fine.
        var match = Regex.Match(contentString, "\\\"tag_name\\\".*?:.*?\\\"v(.*?)\\\"");
        return match.Success ? new Version(match.Groups[1].Value) : null;
    }

    public RedistributableDetails CheckRedistributableVersion()
    {
        // Latest version of Scintilla was built using v14.16 build tools (v141 / VS2017)

        var neededVersion = new Version(14, 16);

        var result = new RedistributableDetails(
            $"v{neededVersion.ToString(2)}",
            RuntimeInformation.ProcessArchitecture.ToString().ToLower(CultureInfo.CurrentUICulture),
            new Uri(@$"https://aka.ms/vs/17/release/vc_redist.{RuntimeInformation.ProcessArchitecture}.exe"));

        var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\" + RuntimeInformation.ProcessArchitecture, false);
        key ??= Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\14.0\VC\Runtiems\" + RuntimeInformation.ProcessArchitecture, false);
        if (key == null)
        {
            return result;
        }

        var isInstalledObject = key.GetValue("Installed");
        if (isInstalledObject is not int isInstalled || isInstalled != 1)
        {
            return result;
        }

        var versionObject = key.GetValue("Version");
        if (versionObject is not string versionString)
        {
            return result;
        }

        // Registry value will be such as v14.30.30704.00

        if (!Version.TryParse(versionString[1..], out var version))
        {
            return result;
        }

        result.InstalledVersion = versionString;
        result.NeedsDownload = version < neededVersion;
        return result;
    }
}