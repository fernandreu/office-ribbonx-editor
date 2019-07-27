using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.Services
{
    public class VersionChecker : IVersionChecker
    {
        private const string CheckUrl = "https://raw.githubusercontent.com/fernandreu/office-ribbonx-editor/info/RELEASE-VERSION";

        public async Task<string> CheckVersionAsync(CancellationToken cancelToken = default(CancellationToken))
        {
            string latest;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var uri = new Uri(CheckUrl);
                    var response = await httpClient.GetAsync(uri, cancelToken);
                    latest = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message); 
                return null;
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

            Debug.WriteLine($"Latest version: {latestVersion}; current: {current}");

            // Now convert the latest string into something comparable to current
            return latestVersion > current ? latestVersion.ToString(3) : null;
        }
    }
}
