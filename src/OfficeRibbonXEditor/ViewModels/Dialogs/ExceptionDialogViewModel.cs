using System;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{
    [Export]
    public class ExceptionDialogViewModel : DialogBase, IContentDialog<Exception?>
    {
        private readonly IToolInfo info;

        private readonly IUrlHelper urlHelper;

        public ExceptionDialogViewModel(IToolInfo info, IUrlHelper urlHelper)
        {
            this.info = info;
            this.urlHelper = urlHelper;
            this.ShutdownCommand = new RelayCommand(this.ExecuteShutdownCommand);
            this.SubmitBugCommand = new RelayCommand(this.ExecuteSubmitBugCommand);
        }

        private Exception? exception;

        public Exception? Exception
        {
            get => this.exception;
            set => this.Set(ref this.exception, value);
        }

        public RelayCommand ShutdownCommand { get; }

        public RelayCommand SubmitBugCommand { get; }

        public bool OnLoaded(Exception? payload)
        {
            this.Exception = payload;
            return payload != null;
        }

        private void ExecuteShutdownCommand()
        {
            Application.Current.MainWindow?.Close();
        }

        private void ExecuteSubmitBugCommand()
        {
            var title = Uri.EscapeDataString($"{this.exception?.GetType().Name}: {this.Exception?.Message}");
            var body = Uri.EscapeDataString(
                "**Describe the bug**\n\n" +
                $"```\n{this.exception}\n```\n\n" +
                "**To Reproduce**\n" +
                "Please describe the steps that lead to the unhandled exception here, including example files if relevant.\n\n" +
                "**Screenshots**\n" +
                "If applicable, add screenshots to help explain how the unhandled exception occurred.\n\n" +
                "**Additional context**\n\n" +
                $"- Version: {this.info.AssemblyVersion}\n" +
                $"- Runtime: {this.info.RuntimeVersion}\n" +
                $"- Operating System: {this.info.OperatingSystemVersion}\n");
            this.urlHelper.OpenBug(title, body);
        }
    }
}
