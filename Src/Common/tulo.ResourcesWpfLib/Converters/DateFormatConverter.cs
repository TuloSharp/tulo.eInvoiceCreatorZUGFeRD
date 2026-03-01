using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace tulo.ResourcesWpfLib.Converters;

public class DateFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            if (date == new DateTime(1, 1, 1))
                return null;
            return date;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string input)
        {
            string digitsOnly = Regex.Replace(input, @"\D", "");

            if (digitsOnly.Length == 8)
            {
                try
                {
                    string formattedDate = $"{digitsOnly.Substring(0, 2)}.{digitsOnly.Substring(2, 2)}.{digitsOnly.Substring(4, 4)}";
                    var parsedDate = DateTime.ParseExact(formattedDate, "dd.MM.yyyy", culture);

                    return parsedDate;
                }
                catch { return null; }
            }
        }

        // fallback 01.01.0001
        if (value == null)
            return new DateTime(1, 1, 1);

        return value;
    }
}
