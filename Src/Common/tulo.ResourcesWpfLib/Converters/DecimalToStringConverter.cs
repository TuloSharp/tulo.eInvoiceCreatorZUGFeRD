using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace tulo.ResourcesWpfLib.Converters;

public class DecimalToStringConverter : IValueConverter
{
    private static readonly CultureInfo _de = new CultureInfo("de-DE");
    // decimal -> string
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;

        if (value is decimal d)
        {
            if (d == 0m) return string.Empty;
            return d.ToString("N2", _de);
        }

        return value.ToString()!;
    }

    // string -> decimal
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = value as string;
        if (string.IsNullOrWhiteSpace(s))
        {
            return 0m;
        }

        if (decimal.TryParse(s, NumberStyles.Number, _de, out decimal parsed))
        {
            return Math.Round(parsed, 2, MidpointRounding.AwayFromZero);
        }

        return DependencyProperty.UnsetValue;
    }
}
