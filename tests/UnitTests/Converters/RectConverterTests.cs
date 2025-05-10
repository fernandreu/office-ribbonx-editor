using System;
using System.Collections.Generic;
using System.Windows;
using NUnit.Framework;
using RectConverter = OfficeRibbonXEditor.Converters.RectConverter;

namespace OfficeRibbonXEditor.UnitTests.Converters;

public class RectConverterTests : MultiConverterTestsBase<RectConverter>
{
    protected override List<Type> FromTypes { get; } = new List<Type> {typeof(double), typeof(double)};

    protected override Type ToType { get; } = typeof(Rect);

    [Test]
    [TestCase(64, 64)]
    [TestCase(0, 0)]
    public void ConvertTest(double width, double height)
    {
        // Act
        var rect = Convert(new object[] {width, height});
        Assert.That(rect, Is.Not.Null);
        Assert.That(rect?.GetType() == ToType, Is.True);
    }

    [Test]
    [TestCase(null)]
    [TestCase(true)]
    public void ShouldNotConvertBack(object? original)
    {
        Assert.Throws<InvalidOperationException>(() => ConvertBack(original));
    }
}