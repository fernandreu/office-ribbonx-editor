using System;
using System.Globalization;
using System.Windows.Data;

namespace OfficeRibbonXEditor.Converters
{
    public class DataTypeConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.GetType();
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException($"{nameof(DataTypeConverter)} can only be used OneWay.");
        }
    }
}
