using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace tulo.RevealPasswordBox;

public class RevealPasswordBox : Control
{
    static RevealPasswordBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RevealPasswordBox),
            new FrameworkPropertyMetadata(typeof(RevealPasswordBox)));
    }

    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.Register(nameof(Password), typeof(string), typeof(RevealPasswordBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string Password
    {
        get => (string)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    public static readonly DependencyProperty IsPasswordVisibleProperty =
        DependencyProperty.Register(nameof(IsPasswordVisible), typeof(bool), typeof(RevealPasswordBox),
            new PropertyMetadata(false));

    public bool IsPasswordVisible
    {
        get => (bool)GetValue(IsPasswordVisibleProperty);
        set => SetValue(IsPasswordVisibleProperty, value);
    }

    // ✅ Geometry Icons
    public static readonly DependencyProperty EyeGeometryProperty =
        DependencyProperty.Register(nameof(EyeGeometry), typeof(Geometry), typeof(RevealPasswordBox));

    public Geometry EyeGeometry
    {
        get => (Geometry)GetValue(EyeGeometryProperty);
        set => SetValue(EyeGeometryProperty, value);
    }

    public static readonly DependencyProperty EyeOffGeometryProperty =
        DependencyProperty.Register(nameof(EyeOffGeometry), typeof(Geometry), typeof(RevealPasswordBox));

    public Geometry EyeOffGeometry
    {
        get => (Geometry)GetValue(EyeOffGeometryProperty);
        set => SetValue(EyeOffGeometryProperty, value);
    }
}