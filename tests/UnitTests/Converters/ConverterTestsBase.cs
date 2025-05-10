using System.Globalization;
using System.Windows.Data;
using NUnit.Framework;

namespace OfficeRibbonXEditor.UnitTests.Converters;

public abstract class ConverterTestsBase<TConverter, TFrom, TTo> where TConverter : IValueConverter, new()
{
    [Test]
    public void HasValueConversionAttribute()
    {
        // Arrange / act
        var attributes = typeof(TConverter).GetCustomAttributes(typeof(ValueConversionAttribute), false);

        // Assert
        Assert.That(attributes, Is.Not.Empty);
    }

    protected virtual object? Convert(object? original, object? parameter = null)
    {
        // Arrange
        var converter = new TConverter();

        // Act
        var actual = converter.Convert(original, typeof(TTo), parameter, CultureInfo.CurrentCulture);
        return actual;
    }

    protected virtual object? ConvertBack(object? original, object? parameter = null)
    {
        // Arrange
        var converter = new TConverter();

        // Act
        var actual = converter.ConvertBack(original, typeof(TFrom), parameter, CultureInfo.CurrentCulture);
        return actual;
    }
}