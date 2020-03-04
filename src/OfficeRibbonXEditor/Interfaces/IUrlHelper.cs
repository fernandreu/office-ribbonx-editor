using System;
using System.Diagnostics;

namespace OfficeRibbonXEditor.Interfaces
{
    public interface IUrlHelper
    {
        Process? OpenIssue();

        Process? OpenBug(string title, string body);

        Process? OpenRelease(string version = "latest");

        Process? OpenExternal(Uri url);
    }
}
