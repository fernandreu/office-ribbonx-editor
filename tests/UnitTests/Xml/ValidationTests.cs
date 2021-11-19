using System.Linq;
using NUnit.Framework;
using OfficeRibbonXEditor.Documents;
using OfficeRibbonXEditor.Helpers.Xml;
using SmartFormat;

namespace OfficeRibbonXEditor.UnitTests.Xml
{
    public class ValidationTests
    {
        private const string Namespace12 = "2006/01";

        private const string Namespace14 = "2009/07";

        private const string BasicContent = @"
        <customUI xmlns=""http://schemas.microsoft.com/office/{Namespace}/customui"">
            <ribbon {RibbonAttributes}>
                <tabs>
                    <tab {TabAttributes}>
                        <group {GroupAttributes}>
                            <button {ButtonAttributes} />
                        </group>
                    </tab>
                </tabs>
            </ribbon>
        </customUI>"; 
        
        [TestCase(XmlPart.RibbonX12, ExpectedResult = false)]
        [TestCase(XmlPart.RibbonX14, ExpectedResult = false)]
        public bool TestEmpty(XmlPart partType)
        {
            // Arrange
            var content = string.Empty;
            var schema = Schema.Load(partType);
            Assert.IsNotNull(schema);

            // Act
            var errors = XmlValidation.Validate(content, schema!);

            return !errors.Any();
        }

        [TestCase(Namespace12, XmlPart.RibbonX12, ExpectedResult = true)]
        [TestCase(Namespace14, XmlPart.RibbonX12, ExpectedResult = false)]
        [TestCase(Namespace12, XmlPart.RibbonX14, ExpectedResult = false)]
        [TestCase(Namespace14, XmlPart.RibbonX14, ExpectedResult = true)]
        public bool TestNamespace(string xmlNamespace, XmlPart schema)
            => ValidateInternal(new ValidationOptions
            {
                Namespace = xmlNamespace,
                Schema = schema,
            });

        [TestCase("title=\"ABC\"", "getTitle=\"DEF\"", ExpectedResult = false)]
        [TestCase("insertBeforeMso=\"ABC\"", "insertAfterMso=\"DEF\"", ExpectedResult = false)]
        [TestCase("supertip=\"ABC\"", "getScreentip=\"DEF\"", ExpectedResult = true)]
        public bool TestMutuallyExclusive(params string[] attributes)
            => ValidateInternal(new ValidationOptions
            {
                ButtonAttributes = string.Join(" ", attributes),
            });

        private static bool ValidateInternal(ValidationOptions options)
        {
            // Arrange
            var schema = Schema.Load(options.Schema);
            Assert.NotNull(schema);

            var xml = Smart.Format(options.Content ?? string.Empty, options);

            // Act
            var errors = XmlValidation.Validate(xml, schema!);

            return !errors.Any();
        }

        private class ValidationOptions
        {
            public string? Content { get; set; } = BasicContent;

            public XmlPart Schema { get; set; } = XmlPart.RibbonX12;

            public string? Namespace { get; set; } = Namespace12;

            public string? RibbonAttributes { get; set; }

            public string? TabAttributes { get; set; }

            public string? GroupAttributes { get; set; }

            public string? ButtonAttributes { get; set; }
        }
    }
}
