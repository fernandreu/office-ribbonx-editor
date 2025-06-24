using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using OfficeRibbonXEditor.Services;

namespace OfficeRibbonXEditor.UnitTests.Services;

[SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "It is just a test, not an API")]
public static class UrlHelperTests
{
    [Test]
    public static void CanOpenIssue() => CallAndKill(x => x.OpenIssue());

    [Test]
    public static void CanOpenBug() => CallAndKill(x => x.OpenBug("N/A", "N/A"));

    [Test]
    public static void CanOpenRelease() => CallAndKill(x => x.OpenRelease());

    [TestCase("https://www.google.com")]
    public static void CanOpenExternal(string url)
    {
        CallAndKill(x => x.OpenExternal(new Uri(url)));
    }

    private static void CallAndKill(Func<UrlHelper, Process?> method)
    {
        // Arrange
        var helper = new UrlHelper();

        // Act
        var process = method(helper);
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
        catch (Exception ex)
        {
            Assert.Fail($"Expected no exceptions, but got: {ex}");
        }
    }
}