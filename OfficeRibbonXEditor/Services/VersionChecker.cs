using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.Services
{
    public class VersionChecker : IVersionChecker
    {
        private const string CheckUrl = "https://api.github.com/repos/fernandreu/office-ribbonx-editor/releases/latest";

        public async Task<string> CheckVersionAsync(CancellationToken cancelToken = default(CancellationToken))
        {
            try
            {
                var latestVersion = await this.GetVersionAsync(cancelToken);
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

        private async Task<Version> GetVersionAsync(CancellationToken cancelToken)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "request");
                var uri = new Uri(CheckUrl);
                var response = await httpClient.GetAsync(uri, cancelToken);
                var contentString = await response.Content.ReadAsStringAsync();
                var content = JsonConvert.DeserializeObject<dynamic>(contentString);
                string tag = content.tag_name;
                return new Version(tag.Substring(1));
            }
        }
    }
}
