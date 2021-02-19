using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using NUnit.Framework;

namespace OfficeRibbonXEditor.UnitTests.Converters
{
    public abstract class MultiConverterTestsBase<TConverter> where TConverter : IMultiValueConverter, new()
    {
        protected abstract List<Type> FromTypes { get; }

        protected abstract Type ToType { get; }

        [Test]
        public void HasValueConversionAttribute()
        {
            // Arrange / act
            var attributes = typeof(TConverter).GetCustomAttributes(typeof(ValueConversionAttribute), false);

            // Assert
            Assert.IsNotEmpty(attributes);
        }

        protected virtual object? Convert(object[] original, object? parameter = null)
        {
            // Arrange
            var converter = new TConverter();

            // Act
            var actual = converter.Convert(original, ToType, parameter, CultureInfo.CurrentCulture);
            return actual;
        }

        protected virtual object? ConvertBack(object? original, object? parameter = null)
        {
            // Arrange
            var converter = new TConverter();

            // Act
            var actual = converter.ConvertBack(original, FromTypes.ToArray(), parameter, CultureInfo.CurrentCulture);
            return actual;
        }
    }
}
