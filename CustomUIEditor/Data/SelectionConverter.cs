// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectionConverter.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   A value converter that gets the line / column number of the current selection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Data
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;

    public class SelectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TextBox control))
            {
                Debug.Print("This should only be used with a TextBox");
                return null;
            }

            var pos = control.SelectionStart;

            Debug.Print("Getting line / col for position: " + pos);

            var txt = control.Text;
            var lineCount = 0;
            var colCount = 0;
            for (var i = 0; i < txt.Length; i++)
            {

                if (i == pos)
                {
                    break;
                }

                if (txt[i] == '\n')
                {
                    colCount = -1;
                    lineCount++;
                }

                colCount++;
            }

            return $"Line {lineCount}, Col {colCount}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
