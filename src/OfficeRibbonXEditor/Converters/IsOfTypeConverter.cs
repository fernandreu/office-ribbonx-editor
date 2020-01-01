using System;
using System.Globalization;
using System.Windows.Data;

namespace OfficeRibbonXEditor.Converters
{
    [ValueConversion(typeof(object), typeof(bool), ParameterType = typeof(Type))]
    public class IsOfTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(parameter is Type type))
            {
                return false;
            }

            return type.IsInstanceOfType(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException($"{nameof(IsOfTypeConverter)} can only be used OneWay.");
        }
    }
}
