using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace tulo.ResourcesWpfLib.Utilities;
public enum MoneyInputMode
{
    ShiftCents,   // Calculator/cash register: 1 -> 0,01 -> 0,12 -> 1,23 ...
    Manual        // optional (not further developed here)
}

public static class MoneyInput
{
    private static readonly CultureInfo _de = new("de-DE");

    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(MoneyInput), new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);
    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    // NON-nullable decimal
    public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached("Value", typeof(decimal), typeof(MoneyInput), new FrameworkPropertyMetadata(0m, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static void SetValue(DependencyObject element, decimal value) => element.SetValue(ValueProperty, value);
    public static decimal GetValue(DependencyObject element) => (decimal)element.GetValue(ValueProperty);

    // Placeholder-Mode
    public static readonly DependencyProperty ShowZeroWhenEmptyProperty = DependencyProperty.RegisterAttached("ShowZeroWhenEmpty", typeof(bool), typeof(MoneyInput), new PropertyMetadata(false));

    public static void SetShowZeroWhenEmpty(DependencyObject element, bool value) => element.SetValue(ShowZeroWhenEmptyProperty, value);
    public static bool GetShowZeroWhenEmpty(DependencyObject element) => (bool)element.GetValue(ShowZeroWhenEmptyProperty);

    // Mode
    public static readonly DependencyProperty ModeProperty = DependencyProperty.RegisterAttached("Mode", typeof(MoneyInputMode), typeof(MoneyInput), new PropertyMetadata(MoneyInputMode.ShiftCents));

    public static void SetMode(DependencyObject element, MoneyInputMode value) => element.SetValue(ModeProperty, value);
    public static MoneyInputMode GetMode(DependencyObject element) => (MoneyInputMode)element.GetValue(ModeProperty);

    // Internal guard
    private static readonly DependencyProperty _internalUpdateProperty = DependencyProperty.RegisterAttached("InternalUpdate", typeof(bool), typeof(MoneyInput), new PropertyMetadata(false));

    private static void SetInternalUpdate(DependencyObject d, bool v) => d.SetValue(_internalUpdateProperty, v);
    private static bool GetInternalUpdate(DependencyObject d) => (bool)d.GetValue(_internalUpdateProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb) return;

        if ((bool)e.NewValue)
        {
            tb.PreviewTextInput += TbOnPreviewTextInput;
            tb.PreviewKeyDown += TbOnPreviewKeyDown;
            tb.GotKeyboardFocus += TbOnGotKeyboardFocus;
            tb.LostKeyboardFocus += TbOnLostKeyboardFocus;
            DataObject.AddPastingHandler(tb, TbOnPaste);

            ApplyValueToText(tb);
        }
        else
        {
            tb.PreviewTextInput -= TbOnPreviewTextInput;
            tb.PreviewKeyDown -= TbOnPreviewKeyDown;
            tb.GotKeyboardFocus -= TbOnGotKeyboardFocus;
            tb.LostKeyboardFocus -= TbOnLostKeyboardFocus;
            DataObject.RemovePastingHandler(tb, TbOnPaste);
        }
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb) return;
        if (!GetIsEnabled(tb)) return;
        if (GetInternalUpdate(tb)) return;

        ApplyValueToText(tb);
    }

    private static void TbOnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is not TextBox tb) return;

        // Do not overwrite placeholder: if empty, leave empty.
        // If text is already there -> normalize
        if (!string.IsNullOrWhiteSpace(tb.Text))
        {
            SetTextSafely(tb, NormalizeToF2(tb.Text));
            // Move cursor to end (useful for ShiftCents)
            tb.CaretIndex = tb.Text.Length;
            tb.SelectionLength = 0;
        }
    }

    private static void TbOnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is not TextBox tb) return;

        var text = (tb.Text ?? "").Trim();

        // empty => Value = 0, placeholder remains
        if (string.IsNullOrWhiteSpace(text))
        {
            SetInternalUpdate(tb, true);
            try { SetValue(tb, 0m); }
            finally { SetInternalUpdate(tb, false); }
            return;
        }

        text = NormalizeToF2(text);

        if (decimal.TryParse(text, NumberStyles.Number, _de, out var dec))
        {
            dec = Math.Round(dec, 2, MidpointRounding.AwayFromZero);

            SetInternalUpdate(tb, true);
            try { SetValue(tb, dec); }
            finally { SetInternalUpdate(tb, false); }

            // If 0 and placeholder desired: Clear text
            if (!GetShowZeroWhenEmpty(tb) && dec == 0m)
                SetTextSafely(tb, "");
            else
                SetTextSafely(tb, dec.ToString("F2", _de));
        }
    }

    private static void TbOnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox tb) return;

        // Comma/period: with ShiftCents, we just leave it at the end (the comma is there anyway)
        if (e.Key == Key.OemComma || e.Key == Key.OemPeriod || e.Key == Key.Decimal)
        {
            if (GetMode(tb) == MoneyInputMode.ShiftCents)
            {
                EnsureHasMoneyText(tb);
                tb.CaretIndex = tb.Text.Length; // Ende
                tb.SelectionLength = 0;
                e.Handled = true;
            }
        }

        // Backspace im ShiftCents: “von rechts” eine Ziffer entfernen (cents/10)
        if (e.Key == Key.Back && GetMode(tb) == MoneyInputMode.ShiftCents)
        {
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                e.Handled = true;
                return;
            }

            EnsureHasMoneyText(tb);

            var dec = ParseMoney(tb.Text);
            var cents = decimal.Truncate(dec * 100m);
            cents = decimal.Truncate(cents / 10m);          // drop last digit
            var newDec = cents / 100m;

            string formatted = newDec.ToString("F2", _de);

            // Placeholder at 0?
            if (!GetShowZeroWhenEmpty(tb) && newDec == 0m)
                SetTextSafely(tb, "");
            else
                SetTextSafely(tb, formatted);

            SetInternalUpdate(tb, true);
            try { SetValue(tb, newDec); }
            finally { SetInternalUpdate(tb, false); }

            tb.CaretIndex = tb.Text.Length;
            tb.SelectionLength = 0;
            e.Handled = true;
        }
    }

    private static void TbOnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not TextBox tb) return;
        if (string.IsNullOrEmpty(e.Text) || e.Text.Length != 1) { e.Handled = true; return; }

        char ch = e.Text[0];

        if (GetMode(tb) == MoneyInputMode.ShiftCents)
        {
            if (char.IsDigit(ch))
            {
                // placeholder friendly: only generate when first digit is "0.00"
                EnsureHasMoneyText(tb);

                var dec = ParseMoney(tb.Text);
                var cents = decimal.Truncate(dec * 100m);

                cents = cents * 10m + (ch - '0');     // shift left + new digit
                var newDec = cents / 100m;
                var formatted = newDec.ToString("F2", _de);

                // MaxLength strict (all characters count)
                if (tb.MaxLength > 0 && formatted.Length > tb.MaxLength)
                {
                    e.Handled = true;
                    return; // block
                }

                SetTextSafely(tb, formatted);

                SetInternalUpdate(tb, true);
                try { SetValue(tb, newDec); }
                finally { SetInternalUpdate(tb, false); }

                tb.CaretIndex = tb.Text.Length;
                tb.SelectionLength = 0;

                e.Handled = true;
                return;
            }

            // We ignore ',' or '.' (the comma is always there) – or simply put it at the end
            if (ch == ',' || ch == '.')
            {
                EnsureHasMoneyText(tb);
                tb.CaretIndex = tb.Text.Length;
                tb.SelectionLength = 0;
                e.Handled = true;
                return;
            }

            e.Handled = true;
            return;
        }

        //Manual Mode: here you could implement your old "cursor jumps to decimal places" feature
        e.Handled = true;
    }

    private static void TbOnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (sender is not TextBox tb) return;

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

        // accept digits + ',' + '.'
        var cleaned = new string(paste.Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray())
            .Replace('.', ',');

        if (!decimal.TryParse(cleaned, NumberStyles.Number, _de, out var dec))
        {
            e.CancelCommand();
            return;
        }

        dec = Math.Round(dec, 2, MidpointRounding.AwayFromZero);
        var formatted = dec.ToString("F2", _de);

        if (tb.MaxLength > 0 && formatted.Length > tb.MaxLength)
        {
            e.CancelCommand();
            return;
        }

        e.CancelCommand();

        if (!GetShowZeroWhenEmpty(tb) && dec == 0m)
            SetTextSafely(tb, "");
        else
            SetTextSafely(tb, formatted);

        SetInternalUpdate(tb, true);
        try { SetValue(tb, dec); }
        finally { SetInternalUpdate(tb, false); }

        if (!string.IsNullOrEmpty(tb.Text))
            tb.CaretIndex = tb.Text.Length;
    }

    // ---------- utilities ----------
    private static void ApplyValueToText(TextBox tb)
    {
        var v = GetValue(tb);

        if (!GetShowZeroWhenEmpty(tb) && v == 0m && !tb.IsKeyboardFocusWithin)
        {
            SetTextSafely(tb, "");
            return;
        }

        var formatted = Math.Round(v, 2, MidpointRounding.AwayFromZero).ToString("F2", _de);

        if (tb.MaxLength > 0 && formatted.Length > tb.MaxLength)
        {
            // clamp to maximum displayable: (MaxLength - 3) integer digits
            int intDigits = Math.Max(1, tb.MaxLength - 3);
            var maxText = new string('9', intDigits) + ",99";
            SetTextSafely(tb, maxText);

            if (decimal.TryParse(maxText, NumberStyles.Number, _de, out var maxDec))
            {
                SetInternalUpdate(tb, true);
                try { SetValue(tb, maxDec); }
                finally { SetInternalUpdate(tb, false); }
            }
            return;
        }

        SetTextSafely(tb, formatted);
    }

    private static void EnsureHasMoneyText(TextBox tb)
    {
        if (string.IsNullOrWhiteSpace(tb.Text))
            SetTextSafely(tb, "0,00");
        else
            SetTextSafely(tb, NormalizeToF2(tb.Text));
    }

    private static string NormalizeToF2(string input)
    {
        // digits + separators only, '.' -> ','
        var t = new string(input.Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray())
            .Replace('.', ',');

        // if parse fails, fallback to 0,00
        if (!decimal.TryParse(t, NumberStyles.Number, _de, out var dec))
            dec = 0m;

        dec = Math.Round(dec, 2, MidpointRounding.AwayFromZero);
        return dec.ToString("F2", _de);
    }

    private static decimal ParseMoney(string text)
    {
        var t = (text ?? "").Replace('.', ',');
        if (decimal.TryParse(t, NumberStyles.Number, _de, out var dec))
            return Math.Round(dec, 2, MidpointRounding.AwayFromZero);
        return 0m;
    }

    private static void SetTextSafely(TextBox tb, string text)
    {
        if (GetInternalUpdate(tb)) return;

        SetInternalUpdate(tb, true);
        try { tb.Text = text; }
        finally { SetInternalUpdate(tb, false); }
    }
}
