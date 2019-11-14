using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels
{
    public class AboutDialogViewModel : DialogBase
    {
        private readonly IMessageBoxService messageBoxService;

        public AboutDialogViewModel(IMessageBoxService messageBoxService)
        {
            this.messageBoxService = messageBoxService;
            this.SubmitIssueCommand = new RelayCommand(ExecuteSubmitIssueCommand);
            this.CopyInfoCommand = new RelayCommand(this.ExecuteCopyInfoCommand);
        }

        public string AssemblyTitle
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (!string.IsNullOrEmpty(titleAttribute.Title))
                    {
                        return titleAttribute.Title;
                    }
                }

                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public string AssemblyDescription
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }

                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }

                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }

                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }

                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        public string RuntimeVersion => RuntimeInformation.FrameworkDescription;

        public string OperatingSystemVersion => RuntimeInformation.OSDescription;

        public RelayCommand SubmitIssueCommand { get; }

        public RelayCommand CopyInfoCommand { get; }

        private static void ExecuteSubmitIssueCommand()
        {
            Process.Start("https://github.com/fernandreu/office-ribbonx-editor/issues/new/choose");
        }

        private void ExecuteCopyInfoCommand()
        {
            Clipboard.SetText(
                $"Version: {this.AssemblyVersion}\n" +
                $"Runtime:\n {this.RuntimeVersion}\n " +
                $"Operating System: {this.OperatingSystemVersion}");

            this.messageBoxService.Show(
                "The version information has been copied to the clipboard.",
                "Version Information Copied",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
