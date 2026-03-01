using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace tulo.ResourcesWpfLib.Converters;

public class InputErrorToBorderBrushConverter : IMultiValueConverter
{
    public Brush DefaultBrush { get; set; } = (SolidColorBrush)new BrushConverter().ConvertFrom("#B0B2B8");

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2)
            return DefaultBrush;

        if (values.Any(v => v == DependencyProperty.UnsetValue))
            return DependencyProperty.UnsetValue;

        string text = values[0]?.ToString() ?? string.Empty;
        bool isError = values[1] is bool b && b;

        string type = (parameter as string) ?? string.Empty;

        // TextBox (also DecimalTextBox, custom TextBoxes, etc.) or ComboBox
        bool isTextOrCombo = type.EndsWith("TextBox", StringComparison.OrdinalIgnoreCase) || type.Equals("ComboBox", StringComparison.OrdinalIgnoreCase);

        // 1) TextBox / ComboBox rules
        if (isTextOrCombo)
        {
            bool isEmpty = string.IsNullOrWhiteSpace(text);

            // Special case: DecimalTextBox -> 0 / 0,00 counts as "empty"
            if (!isEmpty && type.EndsWith("DecimalTextBox", StringComparison.OrdinalIgnoreCase))
            {
                isEmpty = IsZeroDecimal(text, culture);
            }

            if (isError && isEmpty)
                return Brushes.Red;

            return DefaultBrush;
        }

        // 2) DatePicker rules
        if (type.Equals("DatePicker", StringComparison.OrdinalIgnoreCase))
        {
            if (!isError)
                return DefaultBrush;

            // If empty and error -> red
            if (string.IsNullOrWhiteSpace(text))
                return Brushes.Red;

            DateTime minValue = new DateTime(2000, 1, 1);
            DateTime maxValue = DateTime.Now.AddYears(1);

            if (DateTime.TryParseExact(text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                if (result < minValue || result > maxValue)
                    return Brushes.Red;
            }

            return DefaultBrush;
        }

        // Fallback
        return DefaultBrush;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => null;

    private static bool IsZeroDecimal(string s, CultureInfo culture)
    {
        // Parse using current culture (e.g. de-DE: "0,00")
        if (decimal.TryParse(s, NumberStyles.Number, culture, out var d))
            return d == 0m;

        // Fallback parse (e.g. "0.00")
        if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out d))
            return d == 0m;

        // If it cannot be parsed, treat it as invalid (red)
        return true;
    }
}
