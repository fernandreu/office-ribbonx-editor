using System;

namespace OfficeRibbonXEditor.Events
{
    public class HighlightEventArgs : EventArgs
    {
        public HighlightEventArgs()
        {
        }

        public HighlightEventArgs(int lineNumber, int linePosition)
        {
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

        public int LineNumber { get; set; }

        public int LinePosition { get; set; }
    }
}
