using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace tulo.ResourcesWpfLib.Converters;

public sealed class DateFormatConverter : IValueConverter
{
    private static readonly DateTime _emptyDate = new(1, 1, 1);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DateTime date || date == _emptyDate)
            return null;

        var format = GetDisplayFormat(culture);
        return date.ToString(format, culture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
            return _emptyDate;

        if (value is not string input || string.IsNullOrWhiteSpace(input))
            return _emptyDate;

        input = input.Trim();

        var format = GetInputFormat(culture);

        if (culture.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase))
        {
            var digitsOnly = Regex.Replace(input, @"\D", "");

            if (DateTime.TryParseExact(digitsOnly, format, culture, DateTimeStyles.None, out var usDate))
                return usDate;

            return null;
        }

        if (DateTime.TryParseExact(input, format, culture, DateTimeStyles.None, out var parsedDate))
            return parsedDate;

        return null;
    }

    private static string GetDisplayFormat(CultureInfo culture)
    {
        return culture.Name switch
        {
            "en-US" => "MMddyyyy",
            "es-ES" => "dd/MM/yyyy",
            _ => "dd.MM.yyyy"
        };
    }

    private static string GetInputFormat(CultureInfo culture)
    {
        return culture.Name switch
        {
            "en-US" => "MMddyyyy",
            "es-ES" => "dd/MM/yyyy",
            _ => "dd.MM.yyyy"
        };
    }
}