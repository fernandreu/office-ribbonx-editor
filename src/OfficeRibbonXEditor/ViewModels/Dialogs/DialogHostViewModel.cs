﻿using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{
    [Export]
    public partial class DialogHostViewModel : ObservableObject
    {
        public event EventHandler? ShowingDialog;

        public event EventHandler? Closed;
        
        [ObservableProperty]
        private IContentDialogBase? _content;

        [RelayCommand]
        private void Closing(CancelEventArgs args)
        {
            _content?.ClosingCommand.Execute(args);
        }

        public void ShowDialog()
        {
            ShowingDialog?.Invoke(this, EventArgs.Empty);
        }

        public void Close()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}
