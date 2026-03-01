using System.Windows;
using System.Windows.Controls;

namespace tulo.FontSizePicker;

public class FontSizePicker : Control
{
    static FontSizePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FontSizePicker),
            new FrameworkPropertyMetadata(typeof(FontSizePicker)));
    }

    // Value (TwoWay bindable)
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FontSizePicker),
            new FrameworkPropertyMetadata(
                12.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    // Minimum
    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(FontSizePicker),
            new PropertyMetadata(12.0));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    // Maximum
    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(FontSizePicker),
            new PropertyMetadata(14.0));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    // Step (TickFrequency)
    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(
            nameof(Step),
            typeof(double),
            typeof(FontSizePicker),
            new PropertyMetadata(1.0));

    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    // Optional: A-Buttons (only Optic)
    public static readonly DependencyProperty SmallALabelSizeProperty =
        DependencyProperty.Register(
            nameof(SmallALabelSize),
            typeof(double),
            typeof(FontSizePicker),
            new PropertyMetadata(12.0));

    public double SmallALabelSize
    {
        get => (double)GetValue(SmallALabelSizeProperty);
        set => SetValue(SmallALabelSizeProperty, value);
    }

    public static readonly DependencyProperty LargeALabelSizeProperty =
        DependencyProperty.Register(
            nameof(LargeALabelSize),
            typeof(double),
            typeof(FontSizePicker),
            new PropertyMetadata(18.0));

    public double LargeALabelSize
    {
        get => (double)GetValue(LargeALabelSizeProperty);
        set => SetValue(LargeALabelSizeProperty, value);
    }
}