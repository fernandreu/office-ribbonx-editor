using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OfficeRibbonXEditor.UITests.Extensions;

namespace OfficeRibbonXEditor.UITests.Helpers
{
    public class AppManager : IDisposable
    {
        private readonly string exePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "OfficeRibbonXEditor.exe");

        private bool disposed;

        public Application? App { get; private set; }

        public AutomationBase? Automation { get; private set; }

        public Window? Window { get; private set; }

        public void Launch(params string[] arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = string.Join(" ", arguments.Select(x => $"\"{x}\"")),
                WorkingDirectory = TestContext.CurrentContext.TestDirectory,
            };

            this.App = Application.Launch(psi);
            this.Automation = new UIA3Automation();
            this.App.WaitWhileMainHandleIsMissing(TimeSpan.FromSeconds(10));
            for (var i = 0; i < 100; ++i)
            {
                this.Window = this.App.GetAllTopLevelWindows(this.Automation).FirstOrDefault();
                if (this.Window != null)
                {
                    return;
                }

                Thread.Sleep(100);
            }

            Assert.Ignore("Cannot find main window");
        }

        /// <summary>
        /// Attempts to close the application gracefully by closing any unsaved changes prompt that appears when closing the
        /// main window. Otherwise, killing the application process will result in no code coverage.
        /// </summary>
        private void Close()
        {
            if (this.App == null)
            {
                return;
            }

            var status = TestContext.CurrentContext.Result.Outcome.Status;
            if (status == TestStatus.Failed)
            {
                this.Window?.TestCapture("MainWindow.png", "Main Window status when the test failed");
            }

            this.Window?.Close();

            // TODO: Loop might not be needed if WaitWhileBusy() is really working
            for (var attempts = 0; attempts < 10 && !this.App.HasExited; ++attempts)
            {
                this.App.WaitWhileBusy();
                var dialog = this.Window?.ModalWindows.FirstOrDefault();
                dialog?.FindFirstChild(x => x.ByName("No")).Click();
            }

            this.Automation?.Dispose();
            this.App.Close();
        }

        /// <summary>Disposes the application.</summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Disposes the application.</summary>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.Close();
                this.App?.Dispose();
                this.Automation?.Dispose();
            }

            this.disposed = true;
        }
    }
}
