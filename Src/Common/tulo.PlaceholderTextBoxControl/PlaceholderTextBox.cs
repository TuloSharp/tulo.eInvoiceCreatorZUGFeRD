using System.Windows;
using System.Windows.Controls;

namespace tulo.PlaceholderTextBoxControl;

public class PlaceholderTextBox : TextBox
{
    public string Placeholder
    {
        get { return (string)GetValue(PlaceholderProperty); }
        set { SetValue(PlaceholderProperty, value); }
    }

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register("Placeholder", typeof(string), typeof(PlaceholderTextBox), new PropertyMetadata(string.Empty));

    public bool IsEmpty
    {
        get { return (bool)GetValue(IsEmptyProperty); }
        private set { SetValue(_isEmptyPropertyKey, value); }
    }

    private static readonly DependencyPropertyKey _isEmptyPropertyKey =
        DependencyProperty.RegisterReadOnly("IsEmpty", typeof(bool), typeof(PlaceholderTextBox), new PropertyMetadata(true));


    public static readonly DependencyProperty IsEmptyProperty = _isEmptyPropertyKey.DependencyProperty;

    static PlaceholderTextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PlaceholderTextBox), new FrameworkPropertyMetadata(typeof(PlaceholderTextBox)));
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        IsEmpty = string.IsNullOrEmpty(Text);

        base.OnTextChanged(e);
    }
}
