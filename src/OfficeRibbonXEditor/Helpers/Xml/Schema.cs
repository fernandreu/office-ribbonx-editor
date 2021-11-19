using System.IO;
using System.Xml;
using System.Xml.Schema;
using OfficeRibbonXEditor.Documents;
using OfficeRibbonXEditor.Resources;

namespace OfficeRibbonXEditor.Helpers.Xml
{
    public static class Schema
    {
        public static XmlSchema? Load(XmlPart partType)
        {
            var resource = partType == XmlPart.RibbonX12 ? SchemasResource.customUI : SchemasResource.customui14;
            using var stringReader = new StringReader(resource);
            using var reader = XmlReader.Create(stringReader, new XmlReaderSettings {XmlResolver = null});
            return XmlSchema.Read(reader, null);
        }
    }
}
