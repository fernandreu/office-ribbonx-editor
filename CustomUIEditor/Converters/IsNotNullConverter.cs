// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IsNotNullConverter.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the IsNotNullConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class IsNotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException($"{nameof(IsNotNullConverter)} can only be used OneWay.");
        }
    }
}
