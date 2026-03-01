using System;
using System.Globalization;
using System.Windows.Data;

namespace tulo.ResourcesWpfLib.Converters;

public class PercentageConverter : IValueConverter
{
    public double Factor { get; set; } = 1.0;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d)
            return d * Factor;
        return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value;
}
