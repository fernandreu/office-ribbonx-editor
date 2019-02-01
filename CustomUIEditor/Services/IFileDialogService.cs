// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileDialogService.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the IFileDialogService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Services
{
    using System;
    using System.Collections.Generic;

    public interface IFileDialogService
    {
        bool? OpenFileDialog(string title, string filter, Action<string> completedAction, string fileName = null, int filterIndex = 0);

        bool? OpenFilesDialog(string title, string filter, Action<IEnumerable<string>> completedAction, string fileName = null, int filterIndex = 0);

        bool? SaveFileDialog(string title, string filter, Action<string> completedAction, string fileName = null, int filterIndex = 0);
    }
}
