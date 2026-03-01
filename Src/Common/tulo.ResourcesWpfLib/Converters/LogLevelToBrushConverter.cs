using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace tulo.ResourcesWpfLib.Converters;

public class LogLevelToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return new SolidColorBrush(Colors.White);

        string raw = value.ToString()!.Trim();

        string level = raw
            .Replace("[", "")
            .Replace("]", "")
            .Trim()
            .ToUpperInvariant();

        return level switch
        {
            "INFORMATION" => Hex("#007ACC"),    // Blue
            "INFO" => Hex("#007ACC"),

            "DEBUG" => Hex("#1E90FF"),    // DeepSkyBlue
            "WARNING" => Hex("#FFD700"),    // Gold
            "WARN" => Hex("#FFD700"),

            "ERROR" => Hex("#FF4500"),    // OrangeRed
            "FATAL" => Hex("#FF0000"),    // Rod

            "VERBOSE" => Hex("#2D2D2D"),    // dark grey

            _ => Hex("#FFFFFF")     // white
        };
    }

    private SolidColorBrush Hex(string hex)
    {
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
    } 

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}
