using System;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.Events
{
    public class LaunchDialogEventArgs : EventArgs
    {
        public LaunchDialogEventArgs(IContentDialogBase content, bool showDialog = true)
        {
            Content = content;
            ShowDialog = showDialog;
        }

        public IContentDialogBase Content { get; }

        public bool ShowDialog { get; }
    }
}
