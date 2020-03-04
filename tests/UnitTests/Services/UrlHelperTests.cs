using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using OfficeRibbonXEditor.Services;

namespace OfficeRibbonXEditor.UnitTests.Services
{
    public static class UrlHelperTests
    {
        [TestCase("https://www.google.com")]
        [SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "It is just a test, not an API")]
        public static void CanOpenUrls(string url)
        {
            // Arrange
            var helper = new UrlHelper();

            // Act
            var process = helper.OpenUrl(new Uri(url));
            try
            {
                process?.Kill();
            }
            catch (InvalidOperationException)
            {
                // The process finished too quickly. This probably means that the tab was
                // merged with an already opened browsed. Not much can be done then, other
                // than keeping it open
            }

            // The assert is implicit: this should not throw in .NET Core anymore [#88]
        }
    }
}
