using System;
using System.Diagnostics;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.Services
{
    [Export(typeof(IUrlHelper))]
    public class UrlHelper : IUrlHelper
    {
        private const string BaseUrl = "https://github.com/fernandreu/office-ribbonx-editor";

        public Process? OpenIssue()
        {
            return OpenExternal(new Uri($"{BaseUrl}/issues/new/choose"));
        }

        public Process? OpenBug(string title, string body)
        {
            title = Uri.EscapeUriString(title);
            body = Uri.EscapeUriString(body);
            return OpenExternal(new Uri($"{BaseUrl}/issues/new?assignees=&labels=bug&title={title}&body={body}"));
        }

        public Process? OpenRelease(string version = "latest")
        {
            return OpenExternal(new Uri($"{BaseUrl}/releases/{version}"));
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
