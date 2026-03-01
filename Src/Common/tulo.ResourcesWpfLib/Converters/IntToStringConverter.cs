using System;
using System.Globalization;
using System.Windows.Data;

namespace tulo.ResourcesWpfLib.Converters;

public class IntToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;

        // int -> string (hide 0)
        if (value is int i)
            return i == 0 ? string.Empty : i.ToString(culture);

        // decimal -> string (hide 0.0)
        if (value is decimal d)
        {
            if (d == 0m) return string.Empty;
            return ((int)d).ToString(culture); // or Math.Round(d) if needed
        }

        // fallback
        return value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = value as string;

        if (string.IsNullOrWhiteSpace(s))
            return 0;

        // string -> int
        if (int.TryParse(s, NumberStyles.Integer, culture, out var i))
            return i;

        // string -> decimal -> int (e.g. "19,0" / "19.0")
        if (decimal.TryParse(s, NumberStyles.Number, culture, out var d))
            return (int)d; // or (int)Math.Round(d)

        return 0;
    }
}
