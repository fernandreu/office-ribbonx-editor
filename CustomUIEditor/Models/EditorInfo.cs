// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorInfo.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the EditorInfo type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Models
{
    using System;

    public class EditorInfo
    {
        /// <summary>
        /// Gets or sets the current text shown in the editor
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the current selection in the editor, as a (start, end) tuple containing the corresponding indices
        /// </summary>
        public Tuple<int, int> Selection { get; set; }
    }
}
