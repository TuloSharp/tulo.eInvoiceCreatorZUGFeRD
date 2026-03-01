using System;
using System.Globalization;
using System.Windows.Data;

namespace tulo.ResourcesWpfLib.Converters;

public class CheckDefaultDateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateStr && dateStr == DateTime.MinValue)
            return string.Empty;

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}
