// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileDialogService.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the FileDialogService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Services
{
    using System;

    using Microsoft.Win32;

    public class FileDialogService : IFileDialogService
    {
        public bool? OpenFileDialog(string title, string filter, Action<string> completedAction, string fileName = null, int filterIndex = 0)
        {
            var ofd = new OpenFileDialog
                          {
                              Title = title,
                              Filter = filter,
                              FilterIndex = filterIndex,
                              RestoreDirectory = true,
                              FileName = fileName ?? string.Empty
                          };
            ofd.FileOk += (o, e) => completedAction(((OpenFileDialog)o).FileName);
            return ofd.ShowDialog();
        }
        
        public bool? SaveFileDialog(string title, string filter, Action<string> completedAction, string fileName = null, int filterIndex = 0)
        {
            var sfd = new SaveFileDialog { Title = title, Filter = filter, FilterIndex = filterIndex, FileName = fileName ?? string.Empty };
            sfd.FileOk += (o, e) => completedAction(((SaveFileDialog)o).FileName);
            return sfd.ShowDialog();
        }
    }
}
