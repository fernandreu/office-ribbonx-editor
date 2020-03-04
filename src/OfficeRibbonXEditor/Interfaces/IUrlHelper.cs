using System;
using System.Diagnostics;

namespace OfficeRibbonXEditor.Interfaces
{
    public interface IUrlHelper
    {
        Process? OpenUrl(Uri url);
    }
}
