using System;
using System.Globalization;
using System.Windows.Data;

namespace OfficeRibbonXEditor.Converters;

[ValueConversion(typeof(object), typeof(Type))]
public class DataToTypeConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.GetType();
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new InvalidOperationException($"{nameof(DataToTypeConverter)} can only be used OneWay.");
    }
}