using System;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;

namespace OfficeRibbonXEditor.UITests.Extensions
{
    public static class WindowExtensions
    {
        public static Window? FindFirstModalWindow(this Window self, TimeSpan? timeout = null)
        {
            return Retry.WhileNull(
                () => self?.ModalWindows.FirstOrDefault(),
                timeout).Result;
        }
    }
}
