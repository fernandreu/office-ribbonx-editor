using System;
using System.Globalization;
using System.Windows.Data;

namespace OfficeRibbonXEditor.Converters
{
    [ValueConversion(typeof(double), typeof(double), ParameterType = typeof(double))]
    public class PowerConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var exponent = ((IConvertible) value).ToDouble(null);

            var baseValue = parameter is IConvertible convertible ? convertible.ToDouble(CultureInfo.InvariantCulture) : 2.0;
            return Math.Pow(baseValue, exponent);
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var result = ((IConvertible) value).ToDouble(null);

            var baseValue = parameter is IConvertible convertible ? convertible.ToDouble(CultureInfo.InvariantCulture) : 2.0;
            return Math.Log(result) / Math.Log(baseValue);
        }
    }
}
