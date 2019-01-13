// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColorConverter.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the ColorConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Data
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is System.Drawing.Color color))
            {
                return null;
            }

            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Color color))
            {
                return null;
            }

            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
