using System;
using System.Diagnostics;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.Services
{
    public class UrlHelper : IUrlHelper
    {
        public Process? OpenUrl(Uri url)
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
