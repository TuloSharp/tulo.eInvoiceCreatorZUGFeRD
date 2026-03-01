using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace tulo.ResourcesWpfLib.Converters;

public class DebuggerConverter : IValueConverter
{
    public string Tag { get; set; } = "BINDING";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var name = parameter?.ToString() ?? "<no-parameter>";
        var type = value?.GetType().Name ?? "null";
        var text = value?.ToString() ?? "null";

        Debug.WriteLine($"[{Tag}] Convert: {name} = {text} (type: {type})  @ {DateTime.Now:HH:mm:ss.fff}");

        // Important: Do NOT change the value
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var name = parameter?.ToString() ?? "<no-parameter>";
        var type = value?.GetType().Name ?? "null";
        var text = value?.ToString() ?? "null";

        Debug.WriteLine($"[{Tag}] ConvertBack: {name} = {text} (type: {type})  @ {DateTime.Now:HH:mm:ss.fff}");

        return value;
    }
}
