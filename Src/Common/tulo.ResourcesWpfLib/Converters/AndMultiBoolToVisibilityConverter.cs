using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace tulo.ResourcesWpfLib.Converters;

public class AndMultiBoolToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is bool && values[1] is bool)
        {

            bool value1 = (bool)values[0];
            bool value2 = (bool)values[1];

            return value1 && value2 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
