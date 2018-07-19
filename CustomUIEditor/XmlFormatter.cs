namespace CustomUIEditor
{
    using System;
    using System.IO;
    using System.Text;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Markup;

    using Xceed.Wpf.Toolkit;

    public class XmlFormatter : ITextFormatter
    {
        
        public string GetText(FlowDocument document)
        {
            var tr = new TextRange(document.ContentStart, document.ContentEnd);
            using (var ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Rtf);
                return Encoding.Default.GetString(ms.ToArray());
            }
        }

        public void SetText(FlowDocument document, string text)
        {
            try
            {
                // if the text is null/empty clear the contents of the RTB. If you were to pass a null/empty string
                // to the TextRange.Load method an exception would occur.
                if (string.IsNullOrEmpty(text))
                {
                    document.Blocks.Clear();
                }
                else
                {
                    var tr = new TextRange(document.ContentStart, document.ContentEnd);

                    var rtf = XmlColorizer.Colorize(text);

                    using (var ms = new MemoryStream(Encoding.ASCII.GetBytes(rtf)))
                    {
                        tr.Load(ms, DataFormats.Rtf);
                    }
                }
            }
            catch
            {
                ////throw new InvalidDataException("Data provided is not in the correct Xaml format.");
                new TextRange(document.ContentStart, document.ContentEnd).Text = text;
            }
        }
    }
}
