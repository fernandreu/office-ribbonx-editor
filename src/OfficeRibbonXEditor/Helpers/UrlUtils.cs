using System;
using System.Diagnostics;

namespace OfficeRibbonXEditor.Helpers
{
    public static class UrlUtils
    {
        public static void OpenUrl(string url)
        {
            var psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            };

            var process = Process.Start(psi);
            if (!Sandbox.IsEnabled)
            {
                return;
            }

            try
            {
                process?.Kill();
            }
            catch (InvalidOperationException)
            {
                // The process finished too quickly. This probably means that the tab was
                // merged with an already opened browsed
            }
        }
    }
}
