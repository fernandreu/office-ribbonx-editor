using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using OfficeRibbonXEditor.Common;
using OfficeRibbonXEditor.Helpers.Xml;
using SmartFormat;

namespace OfficeRibbonXEditor.UnitTests.Xml;

public class ValidationTests
{
    private const string Namespace12 = "2006/01";

    private const string Namespace14 = "2009/07";

    private const string BasicContent = """
        <customUI xmlns="http://schemas.microsoft.com/office/{Namespace}/customui" onLoad="MyOnLoadMethod">
            <ribbon {RibbonAttributes}>
                <tabs>
                    <tab {TabAttributes}>
                        <group {GroupAttributes}>
                            <button {ButtonAttributes} />
                        </group>
                    </tab>
                </tabs>
            </ribbon>
        </customUI>
        """; 
        
    private const string PrefixedContent = """
        <mso:customUI xmlns:mso="http://schemas.microsoft.com/office/{Namespace}/customui" onLoad="MyOnLoadMethod">
            <mso:ribbon {RibbonAttributes}>
                <mso:tabs>
                    <mso:tab {TabAttributes}>
                        <mso:group {GroupAttributes}>
                            <mso:button {ButtonAttributes} />
                        </mso:group>
                    </mso:tab>
                </mso:tabs>
            </mso:ribbon>
        </mso:customUI>
        """; 
        
    [TestCase(XmlPart.RibbonX12, ExpectedResult = false)]
    [TestCase(XmlPart.RibbonX14, ExpectedResult = false)]
    public bool TestEmpty(XmlPart partType)
    {
        // Arrange
        var content = string.Empty;
        var schema = Schema.Load(partType);
        Assert.That(schema, Is.Not.Null);

        // Act
        var errors = XmlValidation.Validate(content, schema!);

        return !errors.Any();
    }

    [TestCase(Namespace12, XmlPart.RibbonX12, ExpectedResult = true)]
    [TestCase(Namespace14, XmlPart.RibbonX14, ExpectedResult = true)]
    public bool TestNamespace(string xmlNamespace, XmlPart schema)
        => ValidateInternal(new ValidationOptions
        {
            Namespace = xmlNamespace,
            Schema = schema,
        });

    [TestCase(Namespace12, XmlPart.RibbonX12, ExpectedResult = true)]
    [TestCase(Namespace14, XmlPart.RibbonX12, ExpectedResult = false)]
    [TestCase(Namespace12, XmlPart.RibbonX14, ExpectedResult = false)]
    [TestCase(Namespace14, XmlPart.RibbonX14, ExpectedResult = true)]
    public bool TestWithPrefix(string xmlNamespace, XmlPart schema)
        => ValidateInternal(new ValidationOptions
        {
            Content = PrefixedContent,
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
        Assert.That(schema, Is.Not.Null);

        var xml = Smart.Format(options.Content ?? string.Empty, options);

        // Act
        var errors = XmlValidation.Validate(xml, schema!);

        return !errors.Any();
    }

    private class ValidationOptions
    {
        [UsedImplicitly]
        public string? Content { get; set; } = BasicContent;

        [UsedImplicitly]
        public XmlPart Schema { get; set; } = XmlPart.RibbonX12;

        [UsedImplicitly]
        public string? Namespace { get; set; } = Namespace12;
        
        [UsedImplicitly]
        public string? RibbonAttributes { get; set; }

        [UsedImplicitly]
        public string? TabAttributes { get; set; }

        [UsedImplicitly]
        public string? GroupAttributes { get; set; }

        [UsedImplicitly]
        public string? ButtonAttributes { get; set; }
    }
}