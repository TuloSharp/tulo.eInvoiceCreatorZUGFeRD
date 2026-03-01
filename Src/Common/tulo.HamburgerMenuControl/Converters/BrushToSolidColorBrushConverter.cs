using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace tulo.HamburgerMenuControl.Converters;
public class BrushToSolidColorBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush scb)
            return scb;

        return new SolidColorBrush(Colors.Transparent); // Fallback
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}