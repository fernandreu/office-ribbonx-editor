using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Exceptions;
using FlaUI.UIA3;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OfficeRibbonXEditor.UITests.Extensions;

namespace OfficeRibbonXEditor.UITests.Helpers
{
    public class AppManager : IDisposable
    {
        private readonly string _exePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "OfficeRibbonXEditor.exe");

        private bool _disposed;

        public Application? App { get; private set; }

        public AutomationBase? Automation { get; private set; }

        public Window? Window { get; private set; }

        public Menu? FileMenu  => Window?.FindFirstDescendant(x => x.ByText("File"), TimeSpan.FromSeconds(1)).AsMenu();

        public Menu? HelpMenu => Window?.FindFirstDescendant(x => x.ByText("Help"), TimeSpan.FromSeconds(1)).AsMenu();

        public void Launch(params string[] arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = _exePath,
                Arguments = string.Join(" ", arguments.Select(x => $"\"{x}\"")),
                WorkingDirectory = TestContext.CurrentContext.TestDirectory,
            };

            App = Application.Launch(psi);
            Automation = new UIA3Automation();
            App.WaitWhileMainHandleIsMissing(TimeSpan.FromSeconds(10));
            Window = App.GetMainWindow(Automation, TimeSpan.FromSeconds(10));
            Assume.That(Window, Is.Not.Null, "Cannot find main window");
        }

        public Window[] GetTopLevelWindows()
        {
            if (App == null || Automation == null)
            {
                return Array.Empty<Window>();
            }

            return App.GetAllTopLevelWindows(Automation);
        }

        /// <summary>
        /// Attempts to close the application gracefully by closing any unsaved changes prompt that appears when closing the
        /// main window. Otherwise, killing the application process will result in no code coverage.
        /// </summary>
        private void Close()
        {
            if (App == null)
            {
                return;
            }

            var status = TestContext.CurrentContext.Result.Outcome.Status;
            if (status == TestStatus.Failed)
            {
                Window?.TestCapture("MainWindow.png", "Main Window status when the test failed");

                // TODO: Capture all modal windows and top-level windows
            }

            while (Window?.ModalWindows.Any() ?? false)
            {
                Window.ModalWindows.First().Close();
                App.WaitWhileBusy();
            }

            Window?.Close();

            try
            {
                // TODO: Loop might not be needed if WaitWhileBusy() is really working
                for (var attempts = 0; attempts < 10 && !App.HasExited; ++attempts)
                {
                    App.WaitWhileBusy();
                    var dialog = Window?.ModalWindows.FirstOrDefault();
                    dialog?.FindFirstChild(x => x.ByName("No")).Click();
                }
            }
            catch (NoClickablePointException)
            {
            }
            catch (InvalidOperationException)
            {
            }

            Automation?.Dispose();
            App.Close();
        }

        /// <summary>Disposes the application.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Disposes the application.</summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Close();
                App?.Dispose();
                Automation?.Dispose();
            }

            _disposed = true;
        }
    }
}
