using System;
using NUnit.Framework;
using OfficeRibbonXEditor.Converters;

namespace OfficeRibbonXEditor.UnitTests.Converters;

public class InverseBooleanConverterTests : ConverterTestsBase<InverseBooleanConverter, bool, bool>
{
    [Test]
    [TestCase(true, ExpectedResult = false)]
    [TestCase(false, ExpectedResult = true)]
    public object? ConvertTest(object? original)
    {
        return Convert(original);
    }

    [Test]
    [TestCase(true, ExpectedResult = false)]
    [TestCase(false, ExpectedResult = true)]
    public object? ConvertBackTest(object? original)
    {
        return ConvertBack(original);
    }

    [Test]
    [TestCase(null, typeof(NullReferenceException))]
    [TestCase("wrong type", typeof(InvalidCastException))]
    public void InvalidTypesShouldThrow(object? original, Type exceptionType)
    {
        Assert.Throws(exceptionType, () => Convert(original));
        Assert.Throws(exceptionType, () => ConvertBack(original));
    }
}