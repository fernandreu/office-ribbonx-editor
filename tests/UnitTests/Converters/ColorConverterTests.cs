using System.Collections;
using System.Linq;
using System.Windows.Media;
using NUnit.Framework;
using ColorConverter = OfficeRibbonXEditor.Converters.ColorConverter;
using DataType = System.ValueTuple<System.Drawing.Color?, System.Windows.Media.Color?>;

namespace OfficeRibbonXEditor.UnitTests.Converters;

public class ColorConverterTests : ConverterTestsBase<ColorConverter, System.Drawing.Color, Color>
{
    private static readonly System.Collections.Generic.List<DataType> Data = new System.Collections.Generic.List<DataType>
    {
        (null, null),
        (System.Drawing.Color.FromArgb(200, 150, 100, 50), Color.FromArgb(200, 150, 100, 50)),
    };

    public static IEnumerable ConvertCases => Data.Select(x => new TestCaseData(x.Item1).Returns(x.Item2));

    public static IEnumerable ConvertBackCases => Data.Select(x => new TestCaseData(x.Item2).Returns(x.Item1));

    [Test, TestCaseSource(nameof(ConvertCases))]
    [TestCase("Incorrect type", ExpectedResult = null)]
    public object? ConvertTest(object? original)
    {
        return Convert(original);
    }

    [Test, TestCaseSource(nameof(ConvertBackCases))]
    [TestCase("Incorrect type", ExpectedResult = null)]
    public object? ConvertBackTest(object? original)
    {
        return ConvertBack(original);
    }
}