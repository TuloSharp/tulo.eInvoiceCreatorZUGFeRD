using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using tulo.ResourcesWpfLib.Utilities;

namespace tulo.ResourcesWpfLib.Converters;

public class EnumConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string result = string.Empty;
        if (value == null || value is string str && string.IsNullOrEmpty(str)) return DependencyProperty.UnsetValue;
       
        try
        {
            result = EnumGetDesctiptionUtility.GetDescription((Enum)value);
        }
        catch { } //chatch exception when the combobox is empty
        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}
