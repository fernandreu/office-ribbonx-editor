using System;
using NUnit.Framework;
using OfficeRibbonXEditor.Converters;

namespace OfficeRibbonXEditor.UnitTests.Converters;

public class IsNotNullConverterTests : ConverterTestsBase<IsNotNullConverter, object, bool>
{
    [Test]
    [TestCase("string", ExpectedResult = true)]
    [TestCase(null, ExpectedResult = false)]
    [TestCase(42, ExpectedResult = true)]
    public object? ConvertTest(object? original)
    {
        return Convert(original);
    }

    [Test]
    [TestCase(null)]
    [TestCase(true)]
    public void ShouldNotConvertBack(object? original)
    {
        Assert.Throws<InvalidOperationException>(() => ConvertBack(original));
    }
}