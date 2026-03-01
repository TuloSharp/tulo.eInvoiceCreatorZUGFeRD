using System.Windows;
using System.Windows.Controls;

namespace tulo.ResourcesWpfLib.Utilities;

public static class SelectAllTextOnFocus
{
    public static readonly DependencyProperty SelectAllTextOnFocusEnabledProperty = DependencyProperty.RegisterAttached("SelectAllTextOnFocusEnabled", typeof(bool), typeof(SelectAllTextOnFocus), new UIPropertyMetadata(false, OnSelectAllTextOnFocusEnabledChanged));

    public static bool GetSelectAllTextOnFocusEnabled(DependencyObject obj)
    {
        return (bool)obj.GetValue(SelectAllTextOnFocusEnabledProperty);
    }

    public static void SetSelectAllTextOnFocusEnabled(DependencyObject obj, bool value)
    {
        obj.SetValue(SelectAllTextOnFocusEnabledProperty, value);
    }

    private static void OnSelectAllTextOnFocusEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((bool)e.NewValue)
            {
                textBox.GotFocus += TextBox_GotFocus;
            }
        }
    }

    private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }
}
