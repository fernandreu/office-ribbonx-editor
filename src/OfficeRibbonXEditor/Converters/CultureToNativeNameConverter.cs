using System;
using System.Globalization;
using System.Windows.Data;

namespace OfficeRibbonXEditor.Converters;

[ValueConversion(typeof(string), typeof(string))]
public class CultureToNativeNameConverter : IValueConverter
{
#pragma warning disable CA1031 // Do not catch general exception types
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            var cultureInfo = new CultureInfo((string) parameter);
            return cultureInfo.TextInfo.ToTitleCase(cultureInfo.NativeName);
        }
        catch
        {
            return parameter;
        }
    }
#pragma warning restore CA1031 // Do not catch general exception types

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new InvalidOperationException($"{nameof(DataToTypeConverter)} can only be used OneWay.");
    }
}