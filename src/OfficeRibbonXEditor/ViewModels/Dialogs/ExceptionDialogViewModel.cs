﻿using System;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{
    [Export]
    public class ExceptionDialogViewModel : DialogBase, IContentDialog<Exception?>
    {
        private readonly IToolInfo _info;

        private readonly IUrlHelper _urlHelper;

        public ExceptionDialogViewModel(IToolInfo info, IUrlHelper urlHelper)
        {
            _info = info;
            _urlHelper = urlHelper;
        }

        private Exception? _exception;
        public Exception? Exception
        {
            get => _exception;
            set => Set(ref _exception, value);
        }

        private RelayCommand? _shutdownCommand;
        public RelayCommand ShutdownCommand => _shutdownCommand ??= new RelayCommand(ExecuteShutdownCommand);

        private RelayCommand? _submitBugCommand;
        public RelayCommand SubmitBugCommand => _submitBugCommand ??= new RelayCommand(ExecuteSubmitBugCommand);

        public bool OnLoaded(Exception? payload)
        {
            Exception = payload;
            return payload != null;
        }

        private void ExecuteShutdownCommand()
        {
            Application.Current.MainWindow?.Close();
        }

        private void ExecuteSubmitBugCommand()
        {
            var title =$"{_exception?.GetType().Name}: {Exception?.Message}";
            var body = 
               "**Describe the bug**\n\n" +
               $"```\n{_exception}\n```\n\n" +
               "**To Reproduce**\n" +
               "Please describe the steps that lead to the unhandled exception here, including example files if relevant.\n\n" +
               "**Screenshots**\n" +
               "If applicable, add screenshots to help explain how the unhandled exception occurred.\n\n" +
               "**Additional context**\n\n" +
               $"- Version: {_info.AssemblyVersion}\n" +
               $"- Runtime: {_info.RuntimeVersion}\n" +
               $"- Operating System: {_info.OperatingSystemVersion}\n";
            _urlHelper.OpenBug(title, body);
        }
    }
}
