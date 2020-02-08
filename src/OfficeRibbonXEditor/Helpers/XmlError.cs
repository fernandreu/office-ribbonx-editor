namespace OfficeRibbonXEditor.Helpers
{
    public class XmlError
    {
        public XmlError(int lineNumber, int linePosition, string message)
        {
            LineNumber = lineNumber;
            LinePosition = linePosition;
            Message = message;
        }

        public int LineNumber { get; }

        public int LinePosition { get; }

        public string Message { get; }
    }
}
