using System;
using System.Globalization;
using System.Windows.Data;

namespace tulo.ResourcesWpfLib.Converters;

public class FieldFocusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string focusFieldName && parameter is string fieldName)
            return focusFieldName == fieldName;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
