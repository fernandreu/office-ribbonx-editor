using System;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.Models.Events
{
    public class LaunchDialogEventArgs : EventArgs
    {
        public LaunchDialogEventArgs(IContentDialogBase content, bool showDialog = true)
        {
            this.Content = content;
            this.ShowDialog = showDialog;
        }

        public IContentDialogBase Content { get; }

        public bool ShowDialog { get; }
    }
}
