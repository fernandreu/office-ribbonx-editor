using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OfficeRibbonXEditor.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var flag = false;
        if (value is bool b)
        {
            flag = b;
        }

        return flag ? Visibility.Collapsed : Visibility.Visible;
    }
        
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility != Visibility.Visible;
        }

        return false;
    }
}