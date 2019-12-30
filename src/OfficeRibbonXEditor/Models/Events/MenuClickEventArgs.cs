using System;

namespace OfficeRibbonXEditor.Models.Events
{

    public class MenuClickEventArgs : EventArgs
    {
        public MenuClickEventArgs(string filepath)
        {
            this.Filepath = filepath;
        }

        public string Filepath { get; }
    }
}
