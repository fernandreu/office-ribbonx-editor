using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Capturing;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using NUnit.Framework;

namespace OfficeRibbonXEditor.UITests.Main
{
    [Apartment(ApartmentState.STA)]
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown")]
    public class MainWindowTests
    {
        private readonly string exePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
            "OfficeRibbonXEditor.exe");

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. Initialized in SetUp
        private Application app;

        private AutomationBase automation;

        private Window mainWindow;
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        [SetUp]
        public void SetUp()
        {
            this.app = Application.Launch(exePath);
            this.automation = new UIA3Automation();
            this.app.WaitWhileMainHandleIsMissing(TimeSpan.FromSeconds(5));
            var window = app.GetAllTopLevelWindows(this.automation).First();
            Assert.NotNull(window, "Cannot find main window");
            this.mainWindow = window;
        }

        [TearDown]
        public void TearDown()
        {
            this.automation.Dispose();
            this.app.Close();
        }

        [Test]
        public void CanOpenWindow()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "MainWindow.png");
            var image = Capture.Element(this.mainWindow);
            image.ToFile(path);
            TestContext.AddTestAttachment(path, "The main window of the tool");
            Assert.AreEqual("Office RibbonX Editor", this.mainWindow.Title);
        }
    }
}
