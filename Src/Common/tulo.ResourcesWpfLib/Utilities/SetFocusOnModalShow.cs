using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace tulo.ResourcesWpfLib.Utilities;
public class SetFocusOnModalShow
{
    public static bool GetEnabled(DependencyObject obj) => (bool)obj.GetValue(EnabledProperty);

    public static void SetEnabled(DependencyObject obj, bool value) => obj.SetValue(EnabledProperty, value);

    public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached("Enabled", typeof(bool),typeof(SetFocusOnModalShow),new PropertyMetadata(false, OnEnabledChanged));

    private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement fe) return;
        if (e.NewValue is not bool enabled || !enabled) return;

        fe.Loaded += (_, __) =>
        {
            fe.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                fe.Focus(); // Root Fokus
                fe.MoveFocus(new TraversalRequest(FocusNavigationDirection.First)); // first field
            }));
        };
    }
}
