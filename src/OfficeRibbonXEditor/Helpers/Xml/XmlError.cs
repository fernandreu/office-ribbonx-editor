namespace OfficeRibbonXEditor.Helpers.Xml;

public class XmlError(int lineNumber, int linePosition, string message)
{
    public int LineNumber { get; } = lineNumber;

    public int LinePosition { get; } = linePosition;

    public string Message { get; } = message;
}