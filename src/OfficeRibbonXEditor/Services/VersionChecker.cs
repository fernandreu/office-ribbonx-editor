using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.Services
{
    [Export(typeof(IVersionChecker))]
    public class VersionChecker : IVersionChecker
    {
        [SuppressMessage("SonarLint", "S1075", Justification = "This warning is due to the hard-coded url. If this ever change (e.g. repo is moved), the code will need changes overall anyway")]
        private const string CheckUrl = "https://api.github.com/repos/fernandreu/office-ribbonx-editor/releases/latest";

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public async Task<string?> CheckVersionAsync(CancellationToken cancelToken = default)
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
    }
}
