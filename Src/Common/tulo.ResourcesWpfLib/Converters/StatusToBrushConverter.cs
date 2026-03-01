using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace tulo.ResourcesWpfLib.Converters;
public class StatusToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush _presentBrush = CreateFrozenBrush(Color.FromRgb(0x2E, 0xCC, 0x71));   // grün
    private static readonly SolidColorBrush _absentBrush = CreateFrozenBrush(Color.FromRgb(0x95, 0xA5, 0xA6));   // grau
    private static readonly SolidColorBrush _sickBrush = CreateFrozenBrush(Color.FromRgb(0xF1, 0x7A, 0x2A));   // orange
    private static readonly SolidColorBrush _vacationBrush = CreateFrozenBrush(Color.FromRgb(0x4A, 0x90, 0xE2));   // blau
    private static readonly SolidColorBrush _adjustHoursBrush = CreateFrozenBrush(Color.FromRgb(0xE7, 0x4C, 0x3C));// rot-ish
    private static readonly SolidColorBrush _homeOfficeBrush = CreateFrozenBrush(Color.FromRgb(0x3F, 0x51, 0xB5)); // indigo
    private static readonly SolidColorBrush _bussinesTripBrush = CreateFrozenBrush(Color.FromRgb(0x16, 0xA0, 0x9F)); // teal

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var status = (value as string)?.Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(status))
            return _homeOfficeBrush;

        // Normalisierungen / Aliase (gleich wie bei Geometry-Converter)
        if (status == "anwesend") status = "present";
        if (status == "abwesend" || status == "away") status = "absent";
        if (status == "krank") status = "sick";
        if (status == "urlaub") status = "vacation";
        if (status == "stundenabgleich") status = "adjusthours";
        if (status == "homeoffice") status = "homeoffice";
        if (status == "bussinestrip") status = "dienstreise";

        return status switch
        {
            "present" => _presentBrush,
            "absent" => _absentBrush,
            "sick" => _sickBrush,
            "childsick" => _sickBrush,
            "vacation" => _vacationBrush,
            "stundenabgleich" => _adjustHoursBrush,
            "homeoffice" => _homeOfficeBrush,
            "bussinestrip" => _bussinesTripBrush,
            _ => _absentBrush //fallback
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => null;

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
