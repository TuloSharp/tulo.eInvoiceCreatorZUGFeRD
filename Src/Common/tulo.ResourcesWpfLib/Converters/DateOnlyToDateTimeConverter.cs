using System;
using System.Globalization;
using System.Windows.Data;

namespace tulo.ResourcesWpfLib.Converters;
public class DateOnlyToDateTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is DateOnly dateOnly ? dateOnly.ToDateTime(TimeOnly.MinValue) : null!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is DateTime dt ? DateOnly.FromDateTime(dt) : null!;
    }
}
