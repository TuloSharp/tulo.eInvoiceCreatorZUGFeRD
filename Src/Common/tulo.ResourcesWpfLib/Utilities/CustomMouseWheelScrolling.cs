using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace tulo.ResourcesWpfLib.Utilities;

public static class CustomMouseWheelScrolling
{
    public static readonly DependencyProperty HandleMouseWheelProperty =
        DependencyProperty.RegisterAttached("HandleMouseWheel", typeof(bool), typeof(CustomMouseWheelScrolling), new PropertyMetadata(false, OnHandleMouseWheelChanged));

    public static bool GetHandleMouseWheel(DependencyObject obj)
    {
        return (bool)obj.GetValue(HandleMouseWheelProperty);
    }

    public static void SetHandleMouseWheel(DependencyObject obj, bool value)
    {
        obj.SetValue(HandleMouseWheelProperty, value);
    }

    private static void OnHandleMouseWheelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            if ((bool)e.NewValue)
            {
                element.PreviewMouseWheel += OnPreviewMouseWheel;
            }
            else
            {
                element.PreviewMouseWheel -= OnPreviewMouseWheel;
            }
        }
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is UIElement element)
        {
            ScrollViewer scrollViewer = FindScrollViewer(element);
            if (scrollViewer != null)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta / 3);
                }
                else
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
                }
                e.Handled = true;
            }
        }
    }

    private static ScrollViewer FindScrollViewer(DependencyObject element)
    {
        if (element is ScrollViewer scrollViewer)
        {
            return scrollViewer;
        }

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
            var child = VisualTreeHelper.GetChild(element, i);
            var result = FindScrollViewer(child);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}