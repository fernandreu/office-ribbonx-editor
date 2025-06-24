using System.Windows;
using NUnit.Framework;
using OfficeRibbonXEditor.Converters;

namespace OfficeRibbonXEditor.UnitTests.Converters;

public class InverseBooleanToVisibilityConverterTests : ConverterTestsBase<InverseBooleanToVisibilityConverter, bool, Visibility>
{
    [Test]
    [TestCase(true, ExpectedResult = Visibility.Collapsed)]
    [TestCase(false, ExpectedResult = Visibility.Visible)]
    [TestCase(null, ExpectedResult = Visibility.Visible)]
    [TestCase("wrong type", ExpectedResult = Visibility.Visible)]
    public object? ConvertTest(object? original)
    {
        return Convert(original);
    }

    [Test]
    [TestCase(Visibility.Visible, ExpectedResult = false)]
    [TestCase(Visibility.Hidden, ExpectedResult = true)]
    [TestCase(Visibility.Collapsed, ExpectedResult = true)]
    [TestCase(null, ExpectedResult = false)]
    [TestCase("wrong type", ExpectedResult = false)]
    public object? ConvertBackTest(object? original)
    {
        return ConvertBack(original);
    }
}