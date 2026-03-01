using System.Windows;
using System.Windows.Input;

namespace tulo.ResourcesWpfLib.Utilities;

public static class ExtendedFocusByEnter
{
    public static bool GetExtendedByEnterKey(DependencyObject obj)
    {
        return (bool)obj.GetValue(ExtendedByEnterKeyProperty);
    }

    public static void SetExtendedByEnterKey(DependencyObject obj, bool value)
    {
        obj.SetValue(ExtendedByEnterKeyProperty, value);
    }

    public static readonly DependencyProperty ExtendedByEnterKeyProperty = DependencyProperty.RegisterAttached("ExtendedByEnterKey", typeof(bool),
            typeof(ExtendedFocusByEnter),new PropertyMetadata(false, OnExtendedByEnterKeyChanged));

    private static void OnExtendedByEnterKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            if ((bool)e.NewValue)
            {
                element.PreviewKeyDown += HandlePreviewKeyDown;
            }
            else
            {
                element.PreviewKeyDown -= HandlePreviewKeyDown;
            }
        }
    }

    private static void HandlePreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;

            if (Keyboard.FocusedElement is UIElement focused)
            {
                focused.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
    }
}
