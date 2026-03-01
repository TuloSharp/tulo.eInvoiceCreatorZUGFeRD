using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace tulo.ResourcesWpfLib.Converters;
public class ButtonStateDiagnosticConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // values:
        // 0 = VM IsEnableToSaveData
        // 1 = Button (self)
        // 2 = Command
        // 3 = CommandParameter

        var vmValue = values.Length > 0 ? values[0] : null;
        var btn = values.Length > 1 ? values[1] as ButtonBase : null;
        var cmd = values.Length > 2 ? values[2] as ICommand : null;
        var cmdParam = values.Length > 3 ? values[3] : null;

        bool effectiveEnabled = btn?.IsEnabled ?? false;

        // Parent check: find first disabled parent element
        string disabledByParent = "none";
        if (btn != null)
        {
            DependencyObject current = btn;
            while (true)
            {
                current = VisualTreeHelper.GetParent(current);
                if (current == null) break;

                if (current is UIElement uie && uie.IsEnabled == false)
                {
                    disabledByParent = current.GetType().Name;
                    break;
                }
                if (current is ContentElement ce && ce.IsEnabled == false)
                {
                    disabledByParent = current.GetType().Name;
                    break;
                }
            }
        }

        // Command CanExecute check
        string canExecuteText = "n/a";
        if (cmd != null)
        {
            bool canExecute = cmd.CanExecute(cmdParam);
            canExecuteText = canExecute ? "True" : "False";
        }

        return $"VM={vmValue ?? "null"} | Button.IsEnabled={effectiveEnabled} | CanExecute={canExecuteText} | DisabledParent={disabledByParent}";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

//code for xaml
 //<Button.ToolTip>
 //                           <MultiBinding Converter = "{StaticResource ButtonStateDiagnosticConverter}" >
 //                               < Binding Path="IsEnableToSaveData"/>
 //                               <Binding RelativeSource = "{RelativeSource Self}" />
 //                               < Binding RelativeSource="{RelativeSource Self}" Path="Command"/>
 //                               <Binding RelativeSource = "{RelativeSource Self}" Path="CommandParameter"/>
 //                           </MultiBinding>
 //                       </Button.ToolTip>