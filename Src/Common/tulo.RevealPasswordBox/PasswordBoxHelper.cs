using System.Windows;
using System.Windows.Controls;

namespace tulo.RevealPasswordBox;
public static class PasswordBoxHelper
{
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword", typeof(string), typeof(PasswordBoxHelper),
            new FrameworkPropertyMetadata(string.Empty, OnBoundPasswordChanged));

    public static readonly DependencyProperty BindPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BindPassword", typeof(bool), typeof(PasswordBoxHelper),
            new PropertyMetadata(false, OnBindPasswordChanged));

    private static readonly DependencyProperty _isUpdatingProperty =
        DependencyProperty.RegisterAttached(
            "IsUpdating", typeof(bool), typeof(PasswordBoxHelper),
            new PropertyMetadata(false));

    public static void SetBoundPassword(DependencyObject dp, string value)
        => dp.SetValue(BoundPasswordProperty, value);

    public static string GetBoundPassword(DependencyObject dp)
        => (string)dp.GetValue(BoundPasswordProperty);

    public static void SetBindPassword(DependencyObject dp, bool value)
        => dp.SetValue(BindPasswordProperty, value);

    public static bool GetBindPassword(DependencyObject dp)
        => (bool)dp.GetValue(BindPasswordProperty);

    private static void SetIsUpdating(DependencyObject dp, bool value)
        => dp.SetValue(_isUpdatingProperty, value);

    private static bool GetIsUpdating(DependencyObject dp)
        => (bool)dp.GetValue(_isUpdatingProperty);

    private static void OnBindPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox pb) return;

        if ((bool)e.OldValue)
            pb.PasswordChanged -= HandlePasswordChanged;

        if ((bool)e.NewValue)
            pb.PasswordChanged += HandlePasswordChanged;
    }

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox pb) return;

        pb.PasswordChanged -= HandlePasswordChanged;

        if (!GetIsUpdating(pb))
            pb.Password = e.NewValue?.ToString() ?? string.Empty;

        pb.PasswordChanged += HandlePasswordChanged;
    }

    private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox pb) return;

        SetIsUpdating(pb, true);
        SetBoundPassword(pb, pb.Password);
        SetIsUpdating(pb, false);
    }
}
