using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace tulo.SlideToComfirmControl;
public class SlideToConfirm : Control
{
    static SlideToConfirm()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SlideToConfirm), new FrameworkPropertyMetadata(typeof(SlideToConfirm)));
    }

    private Thumb? _thumb;
    private FrameworkElement? _track;
    private FrameworkElement? _fill;
    private TranslateTransform? _thumbTransform;

    private double _maxX; // Maximum thumb position (px)

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // Detach old handlers (important when template is re-applied)
        if (_thumb != null)
        {
            _thumb.DragStarted -= Thumb_DragStarted;
            _thumb.DragDelta -= Thumb_DragDelta;
            _thumb.DragCompleted -= Thumb_DragCompleted;
        }

        _thumb = GetTemplateChild("PART_Thumb") as Thumb;
        _track = GetTemplateChild("PART_Track") as FrameworkElement;
        _fill = GetTemplateChild("PART_Fill") as FrameworkElement;

        // Ensure the thumb always has a transform we can move
        _thumbTransform = new TranslateTransform(0, 0);
        if (_thumb != null)
            _thumb.RenderTransform = _thumbTransform;

        // Attach handlers
        if (_thumb != null)
        {
            _thumb.DragStarted += Thumb_DragStarted;
            _thumb.DragDelta += Thumb_DragDelta;
            _thumb.DragCompleted += Thumb_DragCompleted;
        }

        // Recompute layout metrics when the control size changes
        SizeChanged -= SlideToConfirm_SizeChanged;
        SizeChanged += SlideToConfirm_SizeChanged;

        UpdateLayoutMetrics();

        // Initialize visuals
        if (IsConfirmed)
        {
            SetThumbX(_maxX);
            SetVisualByProgress(1.0);
        }
        else
        {
            SetThumbX(_maxX <= 0 ? 0 : _maxX * Progress);
            SetVisualByProgress(Progress);
        }
    }

    private void SlideToConfirm_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateLayoutMetrics();

        if (IsConfirmed)
        {
            SetThumbX(_maxX);
            SetVisualByProgress(1.0);
        }
        else
        {
            // Recompute thumb position from Progress after resize
            SetThumbX(_maxX <= 0 ? 0 : _maxX * Progress);
            SetVisualByProgress(Progress);
        }
    }

    private void UpdateLayoutMetrics()
    {
        if (_thumb == null || _track == null) return;

        // Track width minus thumb width = maximum travel distance
        _maxX = Math.Max(0, _track.ActualWidth - _thumb.ActualWidth);
    }

    // ===================== FIX: Stop active animation cleanly =====================

    private void StopThumbAnimationKeepCurrent()
    {
        if (_thumbTransform == null) return;

        // Read the current rendered value (includes animation)
        double current = _thumbTransform.Value.OffsetX;

        // Remove the animation clock (otherwise it overrides manual X assignments)
        _thumbTransform.BeginAnimation(TranslateTransform.XProperty, null);

        // Persist the current value as the new base value to avoid snapping
        _thumbTransform.X = current;
    }

    private void SetThumbX(double x)
    {
        if (_thumbTransform == null) return;

        // Ensure no animation is currently controlling X
        StopThumbAnimationKeepCurrent();

        _thumbTransform.X = x;
    }

    // ===================== Drag handling =====================

    private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
    {
        if (IsConfirmed) return;

        // If a "return" animation is running, stop it so dragging works immediately
        StopThumbAnimationKeepCurrent();
        UpdateLayoutMetrics();
    }

    private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (IsConfirmed) return;
        if (_thumbTransform == null) return;

        // Extra safety: in case DragStarted didn't fire, stop animation here too
        StopThumbAnimationKeepCurrent();

        UpdateLayoutMetrics();

        var newX = _thumbTransform.X + e.HorizontalChange;
        newX = Math.Max(0, Math.Min(_maxX, newX));

        _thumbTransform.X = newX;

        Progress = _maxX <= 0 ? 0 : (newX / _maxX);
        SetVisualByProgress(Progress);
    }

    private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        if (IsConfirmed) return;

        if (Progress >= Threshold)
        {
            Confirm();
        }
        else
        {
            AnimateBack();
        }
    }

    // ===================== Confirm / Reset =====================
    private void Confirm()
    {
        IsConfirmed = true;

        // Force thumb and fill to the final state
        UpdateLayoutMetrics();
        SetThumbX(_maxX);
        Progress = 1.0;
        SetVisualByProgress(1.0);

        if (ConfirmCommand?.CanExecute(ConfirmCommandParameter) == true)
            ConfirmCommand.Execute(ConfirmCommandParameter);

        Confirmed?.Invoke(this, EventArgs.Empty);
    }

    public void Reset()
    {
        IsConfirmed = false;
        Progress = 0;
        AnimateBack();
    }

    private void AnimateBack()
    {
        if (_thumbTransform == null) return;

        // Stop any existing animation and keep the current position as the start
        StopThumbAnimationKeepCurrent();

        var from = _thumbTransform.X;

        var anim = new DoubleAnimation
        {
            From = from,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(220),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            // Do not keep the animation "in control" after completion
            FillBehavior = FillBehavior.Stop
        };

        anim.Completed += (_, __) =>
        {
            // Because FillBehavior.Stop reverts to the base value, set it explicitly
            _thumbTransform.X = 0;

            Progress = 0;
            SetVisualByProgress(0);
        };

        _thumbTransform.BeginAnimation(TranslateTransform.XProperty, anim);
    }

    private void SetVisualByProgress(double p)
    {
        if (_fill == null || _track == null) return;

        p = Math.Max(0, Math.Min(1, p));
        _fill.Width = _track.ActualWidth * p;
    }

    // ===================== Dependency Properties =====================
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(SlideToConfirm),
            new PropertyMetadata("Confirm"));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty ConfirmedTextProperty =
        DependencyProperty.Register(nameof(ConfirmedText), typeof(string), typeof(SlideToConfirm),
            new PropertyMetadata("Confirmed"));

    public string ConfirmedText
    {
        get => (string)GetValue(ConfirmedTextProperty);
        set => SetValue(ConfirmedTextProperty, value);
    }

    public static readonly DependencyProperty ThresholdProperty =
        DependencyProperty.Register(nameof(Threshold), typeof(double), typeof(SlideToConfirm),
            new PropertyMetadata(0.92));

    public double Threshold
    {
        get => (double)GetValue(ThresholdProperty);
        set => SetValue(ThresholdProperty, value);
    }

    public static readonly DependencyProperty ProgressProperty =
        DependencyProperty.Register(nameof(Progress), typeof(double), typeof(SlideToConfirm),
            new PropertyMetadata(0.0));

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        private set => SetValue(ProgressProperty, value);
    }

    public static readonly DependencyProperty IsConfirmedProperty =
        DependencyProperty.Register(nameof(IsConfirmed), typeof(bool), typeof(SlideToConfirm),
            new PropertyMetadata(false));

    public bool IsConfirmed
    {
        get => (bool)GetValue(IsConfirmedProperty);
        private set => SetValue(IsConfirmedProperty, value);
    }

    public static readonly DependencyProperty FillBrushProperty =
        DependencyProperty.Register(nameof(FillBrush), typeof(Brush), typeof(SlideToConfirm),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x71)))); // green

    public Brush FillBrush
    {
        get => (Brush)GetValue(FillBrushProperty);
        set => SetValue(FillBrushProperty, value);
    }

    public static readonly DependencyProperty TrackBrushProperty =
        DependencyProperty.Register(nameof(TrackBrush), typeof(Brush), typeof(SlideToConfirm),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33))));

    public Brush TrackBrush
    {
        get => (Brush)GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    public static readonly DependencyProperty ThumbBrushProperty =
        DependencyProperty.Register(nameof(ThumbBrush), typeof(Brush), typeof(SlideToConfirm),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0))));

    public Brush ThumbBrush
    {
        get => (Brush)GetValue(ThumbBrushProperty);
        set => SetValue(ThumbBrushProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(SlideToConfirm),
            new PropertyMetadata(new CornerRadius(999)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty ConfirmCommandProperty =
        DependencyProperty.Register(nameof(ConfirmCommand), typeof(ICommand), typeof(SlideToConfirm),
            new PropertyMetadata(null));

    public ICommand? ConfirmCommand
    {
        get => (ICommand?)GetValue(ConfirmCommandProperty);
        set => SetValue(ConfirmCommandProperty, value);
    }

    public static readonly DependencyProperty ConfirmCommandParameterProperty =
        DependencyProperty.Register(nameof(ConfirmCommandParameter), typeof(object), typeof(SlideToConfirm), new PropertyMetadata(null));

    public object? ConfirmCommandParameter
    {
        get => GetValue(ConfirmCommandParameterProperty);
        set => SetValue(ConfirmCommandParameterProperty, value);
    }

    public event EventHandler? Confirmed;

    public static readonly DependencyProperty ResetSignalProperty =
    DependencyProperty.Register(nameof(ResetSignal), typeof(long), typeof(SlideToConfirm), new PropertyMetadata(0L, OnResetSignalChanged));

    public long ResetSignal
    {
        get => (long)GetValue(ResetSignalProperty);
        set => SetValue(ResetSignalProperty, value);
    }

    public static readonly DependencyProperty ThumbHoverBrushProperty =
    DependencyProperty.Register(
        nameof(ThumbHoverBrush),
        typeof(Brush),
        typeof(SlideToConfirm),
        new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xFF, 0xD1, 0x66))));

    public Brush ThumbHoverBrush
    {
        get => (Brush)GetValue(ThumbHoverBrushProperty);
        set => SetValue(ThumbHoverBrushProperty, value);
    }

    public static readonly DependencyProperty TrackHoverBrushProperty =
        DependencyProperty.Register(
            nameof(TrackHoverBrush),
            typeof(Brush),
            typeof(SlideToConfirm),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44))));

    public Brush TrackHoverBrush
    {
        get => (Brush)GetValue(TrackHoverBrushProperty);
        set => SetValue(TrackHoverBrushProperty, value);
    }

    public static readonly DependencyProperty TextBrushProperty =
    DependencyProperty.Register(
        nameof(TextBrush),
        typeof(Brush),
        typeof(SlideToConfirm),
        new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xF2, 0xF2, 0xF2))));

    public Brush TextBrush
    {
        get => (Brush)GetValue(TextBrushProperty);
        set => SetValue(TextBrushProperty, value);
    }

    public static readonly DependencyProperty TextHoverBrushProperty =
        DependencyProperty.Register(
            nameof(TextHoverBrush),
            typeof(Brush),
            typeof(SlideToConfirm),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xFF, 0xD1, 0x66))));

    public Brush TextHoverBrush
    {
        get => (Brush)GetValue(TextHoverBrushProperty);
        set => SetValue(TextHoverBrushProperty, value);
    }

    private static void OnResetSignalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var c = (SlideToConfirm)d;

        // Only reset if already confirmed (otherwise it would also reset at startup)
        if (c.IsConfirmed)
            c.Reset();
    }
}
