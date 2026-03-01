using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace tulo.ResourcesWpfLib.Converters;
public class BoolToGridLengthConverter : IValueConverter
{
    public GridLength TrueLength { get; set; } = new GridLength(1, GridUnitType.Star);
    public GridLength FalseLength { get; set; } = new GridLength(0, GridUnitType.Pixel);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var b = value is bool bb && bb;
        return b ? TrueLength : FalseLength;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
