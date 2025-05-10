﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OfficeRibbonXEditor.Converters;

[ValueConversion(typeof(string), typeof(Visibility))]
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.IsNullOrEmpty((string)value) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new InvalidOperationException($"{nameof(StringToVisibilityConverter)} can only be used OneWay.");
    }
}