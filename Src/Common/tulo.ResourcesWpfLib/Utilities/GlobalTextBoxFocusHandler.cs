using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace tulo.ResourcesWpfLib.Utilities;

public static class GlobalTextBoxFocusHandler
{
    public static void Enable()
    {
        EventManager.RegisterClassHandler(typeof(TextBox),
            UIElement.GotFocusEvent,
            new RoutedEventHandler(TextBox_GotFocus));

        EventManager.RegisterClassHandler(typeof(TextBox),
            UIElement.PreviewMouseLeftButtonDownEvent,
            new MouseButtonEventHandler(TextBox_PreviewMouseLeftButtonDown));
    }

    public static readonly DependencyProperty MarkFirstFieldOnLoadProperty =
        DependencyProperty.RegisterAttached(
            "MarkFirstFieldOnLoad",
            typeof(bool),
            typeof(GlobalTextBoxFocusHandler),
            new PropertyMetadata(false, OnMarkFirstFieldOnLoadChanged));

    public static readonly DependencyProperty DisableAutoFocusProperty =
        DependencyProperty.RegisterAttached(
            "DisableAutoFocus",
            typeof(bool),
            typeof(GlobalTextBoxFocusHandler),
            new PropertyMetadata(false));

    public static bool GetMarkFirstFieldOnLoad(DependencyObject obj)
    {
        return (bool)obj.GetValue(MarkFirstFieldOnLoadProperty);
    }

    public static void SetMarkFirstFieldOnLoad(DependencyObject obj, bool value)
    {
        obj.SetValue(MarkFirstFieldOnLoadProperty, value);
    }

    public static bool GetDisableAutoFocus(DependencyObject obj)
    {
        return (bool)obj.GetValue(DisableAutoFocusProperty);
    }

    public static void SetDisableAutoFocus(DependencyObject obj, bool value)
    {
        obj.SetValue(DisableAutoFocusProperty, value);
    }

    private static void OnMarkFirstFieldOnLoadChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue)
        {
            d.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                switch (d)
                {
                    case TextBox textBox:
                        textBox.Focus();
                        textBox.SelectAll();
                        break;

                    case DatePicker datePicker:
                        var datePickerTextBox = FindVisualChild<DatePickerTextBox>(datePicker);
                        if (datePickerTextBox != null)
                        {
                            datePickerTextBox.Focus();
                            datePickerTextBox.SelectAll();
                        }
                        break;
                }
            }));
        }
    }

    private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && !GetDisableAutoFocus(textBox))
        {
            textBox.SelectAll();
        }
    }

    private static void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBox textBox && !textBox.IsKeyboardFocusWithin && !GetDisableAutoFocus(textBox))
        {
            textBox.Focus();
            e.Handled = true;
        }
    }

    private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T tChild)
                return tChild;

            var result = FindVisualChild<T>(child);
            if (result != null)
                return result;
        }
        return null;
    }
}
