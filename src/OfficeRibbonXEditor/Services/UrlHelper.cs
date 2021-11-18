using System;
using System.Diagnostics;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.Services
{
    [Export(typeof(IUrlHelper))]
    public class UrlHelper : IUrlHelper
    {
        private const string DefaultBaseUrl = "https://github.com/fernandreu/office-ribbonx-editor";

        private readonly string _baseUrl;

        public UrlHelper(string? baseUrl = null)
        {
            _baseUrl = baseUrl ?? DefaultBaseUrl;
        }

        public Process? OpenIssue()
        {
            return OpenExternal(new Uri($"{_baseUrl}/issues/new/choose"));
        }

        public Process? OpenBug(string title, string body)
        {
            title = Uri.EscapeDataString(title);
            body = Uri.EscapeDataString(body);
            return OpenExternal(new Uri($"{_baseUrl}/issues/new?assignees=&labels=bug&title={title}&body={body}"));
        }

        public Process? OpenRelease(string version = "latest")
        {
            return OpenExternal(new Uri($"{_baseUrl}/releases/{version}"));
        }

        public Process? OpenExternal(Uri url)
        {
            var psi = new ProcessStartInfo
            {
                FileName = url.ToString(),
                UseShellExecute = true,
            };

            return Process.Start(psi);
        }
    }
}
