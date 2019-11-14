using System;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.Models
{
    public class LaunchDialogEventArgs : EventArgs
    {
        public IContentDialogBase Content { get; set; }

        public bool ShowDialog { get; set; } = true;
    }
}
