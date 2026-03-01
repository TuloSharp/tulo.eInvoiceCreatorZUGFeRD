using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace tulo.ResourcesWpfLib.Utilities;
public static class SetRequestedFocusExtension
{
    public static readonly DependencyProperty RequestFocusProperty = DependencyProperty.RegisterAttached("RequestFocus", typeof(bool), typeof(SetRequestedFocusExtension), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRequestFocusChanged));

    public static void SetRequestFocus(DependencyObject element, bool value)
        => element.SetValue(RequestFocusProperty, value);

    public static bool GetRequestFocus(DependencyObject element)
        => (bool)element.GetValue(RequestFocusProperty);

    private static void OnRequestFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement fe)
            return;

        if (e.NewValue is not bool request || request == false)
            return;

        if (!fe.IsLoaded)
        {
            RoutedEventHandler loadedHandler = null;
            loadedHandler = (_, __) =>
            {
                fe.Loaded -= loadedHandler!;
                FocusNow(fe);
            };
            fe.Loaded += loadedHandler;
        }
        else
        {
            FocusNow(fe);
        }
    }

    private static void FocusNow(FrameworkElement fe)
    {
        fe.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (fe is Control c)
            {
                c.ApplyTemplate();
                c.UpdateLayout();
            }

            if (fe is TextBox tb)
            {
                FocusTextBox(tb);
                SetRequestFocus(fe, false);
                return;
            }

            if (fe is PasswordBox pb)
            {
                FocusPasswordBox(pb);
                SetRequestFocus(fe, false);
                return;
            }

            //CustomControl
            var part = FindTemplatePart(fe,
                "PART_Password",
                "PART_Text",
                "PasswordBox",
                "PlainTextBox");

            if (part is TextBox partTb)
            {
                FocusTextBox(partTb);
                SetRequestFocus(fe, false);
                return;
            }

            if (part is PasswordBox partPb)
            {
                FocusPasswordBox(partPb);
                SetRequestFocus(fe, false);
                return;
            }

            //Fallback
            var visualTb = FindVisualChild<TextBox>(fe);
            if (visualTb != null)
            {
                FocusTextBox(visualTb);
                SetRequestFocus(fe, false);
                return;
            }

            var visualPb = FindVisualChild<PasswordBox>(fe);
            if (visualPb != null)
            {
                FocusPasswordBox(visualPb);
                SetRequestFocus(fe, false);
                return;
            }

            // 5) last Fallback
            fe.Focus();
            Keyboard.Focus(fe);

            SetRequestFocus(fe, false);

        }), DispatcherPriority.Loaded);
    }

    private static void FocusTextBox(TextBox tb)
    {
        tb.Focus();
        Keyboard.Focus(tb);
        tb.SelectAll();
    }

    private static void FocusPasswordBox(PasswordBox pb)
    {
        pb.Focus();
        Keyboard.Focus(pb);
    }

    private static object FindTemplatePart(FrameworkElement fe, params string[] names)
    {
        // UserControl / FrameworkElement NameScope
        foreach (var name in names)
        {
            var named = fe.FindName(name);
            if (named != null)
                return named;
        }

        // ControlTemplate NameScope
        if (fe is Control ctrl && ctrl.Template != null)
        {
            foreach (var name in names)
            {
                var t = ctrl.Template.FindName(name, ctrl);
                if (t != null)
                    return t;
            }
        }

        return null;
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
