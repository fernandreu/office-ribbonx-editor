using System;

namespace OfficeRibbonXEditor.Events
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
