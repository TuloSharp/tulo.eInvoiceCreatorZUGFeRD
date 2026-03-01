using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace tulo.LeaderLabeledTextBlock;

public class LeaderLabeledTextBlock : Control
{
    static LeaderLabeledTextBlock()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(LeaderLabeledTextBlock),
            new FrameworkPropertyMetadata(typeof(LeaderLabeledTextBlock)));
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string),
            typeof(LeaderLabeledTextBlock), new PropertyMetadata(string.Empty));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string),
            typeof(LeaderLabeledTextBlock), new PropertyMetadata(string.Empty));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty TextFontSizeProperty =
        DependencyProperty.Register(nameof(TextFontSize), typeof(double),
            typeof(LeaderLabeledTextBlock), new PropertyMetadata(72.0));

    public double TextFontSize
    {
        get => (double)GetValue(TextFontSizeProperty);
        set => SetValue(TextFontSizeProperty, value);
    }

    public static readonly DependencyProperty LabelFontSizeProperty =
        DependencyProperty.Register(nameof(LabelFontSize), typeof(double),
            typeof(LeaderLabeledTextBlock), new PropertyMetadata(44.0));

    public double LabelFontSize
    {
        get => (double)GetValue(LabelFontSizeProperty);
        set => SetValue(LabelFontSizeProperty, value);
    }

    public static readonly DependencyProperty TextForegroundProperty =
        DependencyProperty.Register(nameof(TextForeground), typeof(Brush),
            typeof(LeaderLabeledTextBlock), new PropertyMetadata(Brushes.Black));

    public Brush TextForeground
    {
        get => (Brush)GetValue(TextForegroundProperty);
        set => SetValue(TextForegroundProperty, value);
    }

    public static readonly DependencyProperty LabelForegroundProperty =
        DependencyProperty.Register(nameof(LabelForeground), typeof(Brush),
            typeof(LeaderLabeledTextBlock), new PropertyMetadata(Brushes.Black));

    public Brush LabelForeground
    {
        get => (Brush)GetValue(LabelForegroundProperty);
        set => SetValue(LabelForegroundProperty, value);
    }

    public static readonly DependencyProperty TextAlignmentProperty =
        DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment),
            typeof(LeaderLabeledTextBlock), new PropertyMetadata(TextAlignment.Left));

    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public static readonly DependencyProperty TextWrappingProperty =
        DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping),
            typeof(LeaderLabeledTextBlock), new PropertyMetadata(TextWrapping.NoWrap));

    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    public static readonly DependencyProperty TextTrimmingProperty =
        DependencyProperty.Register(nameof(TextTrimming), typeof(TextTrimming),
            typeof(LeaderLabeledTextBlock), new PropertyMetadata(TextTrimming.None));

    public TextTrimming TextTrimming
    {
        get => (TextTrimming)GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }

    public static readonly DependencyProperty StrokeProperty =
        DependencyProperty.Register(nameof(Stroke), typeof(Brush),
            typeof(LeaderLabeledTextBlock), new PropertyMetadata(Brushes.Black));

    public Brush Stroke
    {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(nameof(StrokeThickness), typeof(double),
            typeof(LeaderLabeledTextBlock), new PropertyMetadata(6.0));

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public static readonly DependencyProperty CornerWidthProperty =
        DependencyProperty.Register(nameof(CornerWidth), typeof(double),
            typeof(LeaderLabeledTextBlock), new PropertyMetadata(60.0));

    public double CornerWidth
    {
        get => (double)GetValue(CornerWidthProperty);
        set => SetValue(CornerWidthProperty, value);
    }

    public static readonly DependencyProperty CornerHeightProperty =
        DependencyProperty.Register(nameof(CornerHeight), typeof(double),
            typeof(LeaderLabeledTextBlock), new PropertyMetadata(35.0));

    public double CornerHeight
    {
        get => (double)GetValue(CornerHeightProperty);
        set => SetValue(CornerHeightProperty, value);
    }

    // Abstand zwischen Text und Label (dein Wunsch: klein, z.B. 2px)
    public static readonly DependencyProperty LabelTopGapProperty =
        DependencyProperty.Register(nameof(LabelTopGap), typeof(double),
            typeof(LeaderLabeledTextBlock), new PropertyMetadata(2.0));

    public double LabelTopGap
    {
        get => (double)GetValue(LabelTopGapProperty);
        set => SetValue(LabelTopGapProperty, value);
    }

    public static readonly DependencyProperty CornerShiftXProperty =
    DependencyProperty.Register(nameof(CornerShiftX), typeof(double),
        typeof(LeaderLabeledTextBlock), new PropertyMetadata(-6.0));

    public double CornerShiftX
    {
        get => (double)GetValue(CornerShiftXProperty);
        set => SetValue(CornerShiftXProperty, value);
    }

    public static readonly DependencyProperty LabelAlignToCornerProperty =
    DependencyProperty.Register(nameof(LabelAlignToCorner), typeof(bool),
        typeof(LeaderLabeledTextBlock), new PropertyMetadata(false));

    public bool LabelAlignToCorner
    {
        get => (bool)GetValue(LabelAlignToCornerProperty);
        set => SetValue(LabelAlignToCornerProperty, value);
    }
}
