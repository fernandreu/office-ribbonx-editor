using System;

namespace OfficeRibbonXEditor.Models
{
    public class HighlightEventArgs : EventArgs
    {
        public HighlightEventArgs()
        {
        }

        public HighlightEventArgs(int lineNumber, int linePosition)
        {
            this.LineNumber = lineNumber;
            this.LinePosition = linePosition;
        }

        public int LineNumber { get; set; }

        public int LinePosition { get; set; }
    }
}
