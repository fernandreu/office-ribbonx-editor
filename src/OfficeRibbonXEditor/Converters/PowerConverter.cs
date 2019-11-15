using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OfficeRibbonXEditor.Converters
{
    [ValueConversion(typeof(double), typeof(double), ParameterType = typeof(double))]
    public class PowerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var exponent = ((IConvertible) value).ToDouble(null);

            var baseValue = parameter == null ? 2.0 : ((IConvertible) parameter).ToDouble(null);
            return Math.Pow(baseValue, exponent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
