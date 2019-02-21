// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataTypeConverter.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   A value converter that gets the type of that value. This might be useful for EventTriggers and such that only need
//   to occur when selection is of a given type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class DataTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.GetType();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException($"{nameof(DataTypeConverter)} can only be used OneWay.");
        }
    }
}
