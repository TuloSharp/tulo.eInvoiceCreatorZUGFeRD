using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace tulo.ResourcesWpfLib.Utilities;
public class MoneyTextBox : TextBox
{
    private static readonly CultureInfo _de = new("de-DE");
    private bool _internal;

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(decimal), typeof(MoneyTextBox), new FrameworkPropertyMetadata(0m, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) => ((MoneyTextBox)d).SyncTextFromValue()));

    public decimal Value
    {
        get => (decimal)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public MoneyTextBox()
    {
        Loaded += (_, __) =>
        {
            EnsureMinMaxLength();
            SetFromDecimal(Value);
        };

        GotKeyboardFocus += (_, __) =>
        {
            EnsureMinMaxLength();

            if (string.IsNullOrWhiteSpace(Text))
                Text = "0,00";
            else
                Text = NormalizeToF2(Text);

            MoveCaretToEnd();
        };

        // prevents the first click from placing the cursor anywhere
        PreviewMouseLeftButtonDown += (s, e) =>
        {
            if (!IsKeyboardFocusWithin)
            {
                e.Handled = true;
                Focus();
            }
        };

        PreviewTextInput += OnPreviewTextInput;
        PreviewKeyDown += OnPreviewKeyDown;
        DataObject.AddPastingHandler(this, OnPaste);

        LostKeyboardFocus += (_, __) =>
        {
            EnsureMinMaxLength();

            // Format cleanly when leaving
            Text = NormalizeToF2(Text);
            SyncValueFromText();
        };
    }

    private void EnsureMinMaxLength()
    {
        // At least "0.00"
        if (MaxLength > 0 && MaxLength < 4)
            MaxLength = 4;
    }

    // MaxLength = intDigits + 1 + 2 => intDigits = MaxLength - 3
    private int AllowedIntegerDigits =>
        MaxLength > 0 ? Math.Max(1, MaxLength - 3) : int.MaxValue;

    private void MoveCaretToEnd()
    {
        CaretIndex = Text?.Length ?? 0;
        SelectionLength = 0;
    }

    private void SyncTextFromValue()
    {
        if (_internal) return;

        EnsureMinMaxLength();
        SetFromDecimal(Value);
    }

    private void SyncValueFromText()
    {
        if (_internal) return;

        var t = NormalizeToF2(Text);
        if (decimal.TryParse(t, NumberStyles.Number, _de, out var dec))
        {
            dec = Math.Round(dec, 2, MidpointRounding.AwayFromZero);
            _internal = true;
            try { Value = dec; }
            finally { _internal = false; }
        }
    }

    private void SetFromDecimal(decimal dec)
    {
        dec = Math.Round(dec, 2, MidpointRounding.AwayFromZero);
        var formatted = dec.ToString("F2", _de);

        // Strictly adhere to MaxLength: if too long, clamp to "999..,99"
        if (MaxLength > 0 && formatted.Length > MaxLength)
        {
            formatted = new string('9', AllowedIntegerDigits) + ",99";
            dec = decimal.Parse(formatted, NumberStyles.Number, _de);
        }

        _internal = true;
        try
        {
            Text = formatted;
            Value = dec;
        }
        finally { _internal = false; }
    }

    private string NormalizeToF2(string input)
    {
        var t = (input ?? "").Trim();
        if (string.IsNullOrWhiteSpace(t))
            return "0,00";

        // only digits + separators, '.' -> ','
        t = new string(t.Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray())
             .Replace('.', ',');

        // if parsable -> F2, otherwise 0.00
        if (!decimal.TryParse(t, NumberStyles.Number, _de, out var dec))
            dec = 0m;

        dec = Math.Round(dec, 2, MidpointRounding.AwayFromZero);
        var s = dec.ToString("F2", _de);

        // Limit integer part (if MaxLength is very small)
        if (MaxLength > 0 && s.Length > MaxLength)
            s = new string('9', AllowedIntegerDigits) + ",99";

        return s;
    }

    private decimal GetCurrentDecimalSafe()
    {
        var t = NormalizeToF2(Text);
        if (decimal.TryParse(t, NumberStyles.Number, _de, out var dec))
            return Math.Round(dec, 2, MidpointRounding.AwayFromZero);
        return 0m;
    }

    private void ShiftAppendDigit(char digit)
    {
        EnsureMinMaxLength();

        if (string.IsNullOrWhiteSpace(Text))
            Text = "0,00";
        else
            Text = NormalizeToF2(Text);

        decimal dec = GetCurrentDecimalSafe();
        decimal cents = decimal.Truncate(dec * 100m);

        cents = cents * 10m + (digit - '0');
        decimal newDec = cents / 100m;

        string formatted = newDec.ToString("F2", _de);

        // MaxLength strict: block if too long
        if (MaxLength > 0 && formatted.Length > MaxLength)
            return;

        _internal = true;
        try
        {
            Text = formatted;
            Value = newDec;
        }
        finally { _internal = false; }

        MoveCaretToEnd();
    }

    private void ShiftBackspace()
    {
        EnsureMinMaxLength();

        if (string.IsNullOrWhiteSpace(Text))
            return;

        Text = NormalizeToF2(Text);

        decimal dec = GetCurrentDecimalSafe();
        decimal cents = decimal.Truncate(dec * 100m);

        cents = decimal.Truncate(cents / 10m); // last digit omitted
        decimal newDec = cents / 100m;

        _internal = true;
        try
        {
            Text = newDec.ToString("F2", _de);
            Value = newDec;
        }
        finally { _internal = false; }

        MoveCaretToEnd();
    }

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (_internal) { e.Handled = true; return; }
        if (string.IsNullOrEmpty(e.Text) || e.Text.Length != 1) { e.Handled = true; return; }

        char ch = e.Text[0];

        if (char.IsDigit(ch))
        {
            ShiftAppendDigit(ch);
            e.Handled = true;
            return;
        }

        // ',' or '.' -> simply add to the end (comma is already in the format)
        if (ch == ',' || ch == '.')
        {
            if (string.IsNullOrWhiteSpace(Text))
                Text = "0,00";
            else
                Text = NormalizeToF2(Text);

            MoveCaretToEnd();
            e.Handled = true;
            return;
        }

        e.Handled = true;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_internal) return;

        if (e.Key == Key.Space)
        {
            e.Handled = true;
            return;
        }

        // Backspace = Delete right shift
        if (e.Key == Key.Back)
        {
            ShiftBackspace();
            e.Handled = true;
            return;
        }

        // Delete optional: treat as backspace
        if (e.Key == Key.Delete)
        {
            ShiftBackspace();
            e.Handled = true;
            return;
        }

        // Comma/period keys: to the end
        if (e.Key == Key.OemComma || e.Key == Key.OemPeriod || e.Key == Key.Decimal)
        {
            if (string.IsNullOrWhiteSpace(Text))
                Text = "0,00";
            else
                Text = NormalizeToF2(Text);

            MoveCaretToEnd();
            e.Handled = true;
            return;
        }
    }

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.SourceDataObject.GetDataPresent(DataFormats.Text, true))
        {
            e.CancelCommand();
            return;
        }

        var paste = (e.SourceDataObject.GetData(DataFormats.Text) as string ?? "").Trim();
        if (string.IsNullOrEmpty(paste))
        {
            e.CancelCommand();
            return;
        }

        // allow: digits + ',' + '.'; '.' -> ','
        var cleaned = new string(paste.Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray())
            .Replace('.', ',');

        if (!decimal.TryParse(cleaned, NumberStyles.Number, _de, out var dec))
        {
            e.CancelCommand();
            return;
        }

        dec = Math.Round(dec, 2, MidpointRounding.AwayFromZero);
        var formatted = dec.ToString("F2", _de);

        EnsureMinMaxLength();
        if (MaxLength > 0 && formatted.Length > MaxLength)
        {
            e.CancelCommand();
            return;
        }

        e.CancelCommand();

        _internal = true;
        try
        {
            Text = formatted;
            Value = dec;
        }
        finally { _internal = false; }

        MoveCaretToEnd();
    }
}
