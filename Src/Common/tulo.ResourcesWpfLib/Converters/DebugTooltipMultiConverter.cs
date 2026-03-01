using System;
using System.Globalization;
using System.Windows.Data;

namespace tulo.ResourcesWpfLib.Converters;
public class DebugTooltipMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        string name = parameter?.ToString() ?? "IsEnableToSaveData";

        string vm = values.Length > 0 ? (values[0]?.ToString() ?? "null") : "missing";
        string btn = values.Length > 1 ? (values[1]?.ToString() ?? "null") : "missing";

        return $"{name} (VM)={vm} | Button.IsEnabled={btn}";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

//code for xaml
  //<Button.ToolTip>
  //                          <MultiBinding Converter = "{StaticResource DebugTooltipMultiConverter}" ConverterParameter="IsEnableToSaveData">
  //                              <Binding Path = "IsEnableToSaveData" />
  //                              < Binding RelativeSource="{RelativeSource Self}" Path="IsEnabled"/>
  //                          </MultiBinding>
  //                      </Button.ToolTip>
