using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace tulo.ResourcesWpfLib.Converters;

public class InactiveSinceToBackgroundColorConverter : IValueConverter
{
    DateTime _defaultDate = DateTime.MinValue;
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime inactiveSince)
        {
            if (inactiveSince.Date < DateTime.Now.Date && inactiveSince != _defaultDate)
                return new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
        }

        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
       return value;
    }
}
