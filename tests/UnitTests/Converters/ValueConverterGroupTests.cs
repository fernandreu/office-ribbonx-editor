using System.Globalization;
using NUnit.Framework;
using OfficeRibbonXEditor.Converters;

namespace OfficeRibbonXEditor.UnitTests.Converters;

public class ValueConverterGroupTests
{
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TautologyTest(bool? input)
    {
        // Arrange
        var group = new ValueConverterGroup();
        group.Converters.Add(new InverseBooleanConverter());
        group.Converters.Add(new InverseBooleanConverter());

        // Act
        var result = group.Convert(input, typeof(bool), null, CultureInfo.CurrentCulture);
        var resultBack = group.ConvertBack(result, typeof(bool), null, CultureInfo.CurrentCulture);
            
        // Assert
        Assert.That(result, Is.EqualTo(input));
        Assert.That(resultBack, Is.EqualTo(input));
    }
}