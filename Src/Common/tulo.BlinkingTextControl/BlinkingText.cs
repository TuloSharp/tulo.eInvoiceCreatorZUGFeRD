using System.Windows;
using System.Windows.Controls;

namespace tulo.BlinkingTextControl;

public class BlinkingText : Control
{
    static BlinkingText()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BlinkingText), new FrameworkPropertyMetadata(typeof(BlinkingText)));
    }

    public bool HasToBlink
    {
        get => (bool)GetValue(HasToBlinkProperty);
        set => SetValue(HasToBlinkProperty, value);
    }

    public static readonly DependencyProperty HasToBlinkProperty =
        DependencyProperty.Register(
            nameof(HasToBlink),
            typeof(bool),
            typeof(BlinkingText),
            new PropertyMetadata(false));

    public string BlinkedText
    {
        get => (string)GetValue(BlinkedTextProperty);
        set => SetValue(BlinkedTextProperty, value);
    }

    public static readonly DependencyProperty BlinkedTextProperty =
        DependencyProperty.Register(
            nameof(BlinkedText),
            typeof(string),
            typeof(BlinkingText),
            new PropertyMetadata(string.Empty));
}
