// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VersionChecker.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the VersionChecker type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Services
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    public class VersionChecker : IVersionChecker
    {
        private const string CheckUrl = "https://raw.githubusercontent.com/fernandreu/wpf-custom-ui-editor/info/RELEASE-VERSION";

        public async Task<string> CheckVersionAsync(CancellationToken cancelToken = default(CancellationToken))
        {
            string latest;
            using (var httpClient = new HttpClient())
            {
                var uri = new Uri(CheckUrl);
                var response = await httpClient.GetAsync(uri, cancelToken);
                latest = await response.Content.ReadAsStringAsync();
            }

            if (!Regex.IsMatch(latest, @"^\d+\.\d+\.\d+\.\d+$"))
            {
                Debug.Fail($"Version '{latest}' has an unrecognized format");
                return null;
            }

            Version latestVersion;
            try
            {
                latestVersion = new Version(latest);
            }
            catch (FormatException e)
            {
                Debug.Fail(e.Message);
                return null;
            }

            var current = Assembly.GetExecutingAssembly().GetName().Version;

            // Now convert the latest string into something comparable to current
            return latestVersion > current ? latestVersion.ToString(3) : null;
        }
    }
}
