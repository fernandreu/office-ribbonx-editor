using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using OfficeRibbonXEditor.Extensions;
using OfficeRibbonXEditor.Resources;

namespace OfficeRibbonXEditor.Helpers.Xml
{
    public static class XmlValidation
    {
        private static readonly IList<HashSet<string>> MutuallyExclusiveAttributes = new List<HashSet<string>>
        {
            new HashSet<string> {"title", "getTitle"},
            new HashSet<string> {"enabled", "getEnabled"},
            new HashSet<string> {"visible", "getVisible"},
            new HashSet<string> {"label", "getLabel"},
            new HashSet<string> {"keytip", "getKeytip"},
            new HashSet<string> {"screentip", "getScreentip"},
            new HashSet<string> {"supertip", "getSupertip"},
            new HashSet<string> {"description", "getDescription"},
            new HashSet<string> {"altText", "getAltText"},
            new HashSet<string> {"showLabel", "getShowLabel"},
            new HashSet<string> {"helperText", "getHelperText"},
            new HashSet<string> {"showImage", "getShowImage"},
            new HashSet<string> {"size", "getSize"},
            new HashSet<string> {"id", "idMso", "idQ"},
            new HashSet<string> {"image", "imageMso", "getImage"},
            new HashSet<string> {"insertBeforeMso", "insertAfterMso", "insertBeforeQ", "insertAfterQ"},
        };

        public static IList<XmlError> Validate(string? xml, XmlSchema targetSchema)
        {
            var errorList = new List<XmlError>();

            XDocument xmlDoc;
            try
            {
                xmlDoc = XDocument.Parse(xml ?? string.Empty, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
            }
            catch (XmlException ex)
            {
                errorList.Add(new XmlError(
                    ex.LineNumber,
                    ex.LinePosition,
                    ex.Message));
                return errorList;
            }

            var schemaSet = new XmlSchemaSet();
            schemaSet.Add(targetSchema);

            var ns = xmlDoc.Root?.GetDefaultNamespace().ToString();
            if (ns != targetSchema.TargetNamespace)
            {
                errorList.Add(new XmlError(
                    1,
                    1,
                    string.Format(CultureInfo.InvariantCulture, Strings.Validation_WrongNamespace, ns, targetSchema.TargetNamespace)));
            }

            void ValidateHandler(object o, ValidationEventArgs e)
            {
                errorList?.Add(new XmlError(
                    e.Exception.LineNumber,
                    e.Exception.LinePosition,
                    e.Message));
            }

            try
            {
                xmlDoc.Validate(schemaSet, ValidateHandler);
            }
            catch (XmlException ex)
            {
                errorList.Add(new XmlError(
                    ex.LineNumber,
                    ex.LinePosition,
                    ex.Message));
            }

            if (errorList.Count != 0)
            {
                return errorList;
            }

            // The document passes the validation against the schema, but there might still be mutually exclusive attributes defined
            // in the same element, which the schema does not validate. Let's do that ourselves
            foreach (var element in xmlDoc.Elements())
            {
                ValidateFurther(element, errorList);
            }

            return errorList;
        }

        private static void ValidateFurther(XElement element, ICollection<XmlError> errorList)
        {
            // Note: when this gets executed, we already know that the attributes are defined in an element that
            // allows them, so there is no need to check that here
            if (element.HasAttributes)
            {
                var occurrences = Enumerable.Repeat(0, MutuallyExclusiveAttributes.Count).ToArray();
                foreach (var attribute in element.Attributes())
                {
                    foreach (var (item, index) in MutuallyExclusiveAttributes.Enumerated())
                    {
                        if (!item.Contains(attribute.Name.LocalName))
                        {
                            continue;
                        }

                        ++occurrences[index];
                        if (occurrences[index] != 2)
                        {
                            continue;
                        }

                        // Only showing errors at the second occurrence. If there are more, the same error already shown will be enough
                        var info = attribute as IXmlLineInfo;
                        var message = string.Format(CultureInfo.InvariantCulture, Strings.Validation_MutuallyExclusive, string.Join(", ", MutuallyExclusiveAttributes[index]));
                        errorList.Add(new XmlError(info?.LineNumber ?? 1, info?.LinePosition ?? 1, message));
                    }
                }
            }

            if (!element.HasElements)
            {
                return;
            }
            
            foreach (var nested in element.Elements())
            {
                ValidateFurther(nested, errorList);
            }
        }
    }
}
