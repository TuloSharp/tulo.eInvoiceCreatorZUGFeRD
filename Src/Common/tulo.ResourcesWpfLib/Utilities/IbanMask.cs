using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace tulo.ResourcesWpfLib.Utilities;
 public static class IbanMask
    {
        // Enable switch
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled", typeof(bool), typeof(IbanMask),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);
        public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

        // Optional: bind raw IBAN (without spaces) to VM
        public static readonly DependencyProperty IbanProperty =
            DependencyProperty.RegisterAttached(
                "Iban", typeof(string), typeof(IbanMask),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIbanChanged));

        public static void SetIban(DependencyObject element, string value) => element.SetValue(IbanProperty, value);
        public static string GetIban(DependencyObject element) => (string)element.GetValue(IbanProperty);

        // internal flag to avoid recursion
        private static readonly DependencyProperty _isInternalUpdateProperty =
            DependencyProperty.RegisterAttached("IsInternalUpdate", typeof(bool), typeof(IbanMask), new PropertyMetadata(false));

        private static void SetIsInternalUpdate(DependencyObject d, bool v) => d.SetValue(_isInternalUpdateProperty, v);
        private static bool GetIsInternalUpdate(DependencyObject d) => (bool)d.GetValue(_isInternalUpdateProperty);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox tb) return;

            if ((bool)e.NewValue)
            {
                tb.PreviewTextInput += OnPreviewTextInput;
                tb.TextChanged += OnTextChanged;
                DataObject.AddPastingHandler(tb, OnPaste);
                tb.LostFocus += OnLostFocusFormat;
            }
            else
            {
                tb.PreviewTextInput -= OnPreviewTextInput;
                tb.TextChanged -= OnTextChanged;
                DataObject.RemovePastingHandler(tb, OnPaste);
                tb.LostFocus -= OnLostFocusFormat;
            }
        }

        private static void OnIbanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox tb) return;
            if (GetIsInternalUpdate(tb)) return;

            // VM -> UI
            var raw = NormalizeRaw(e.NewValue as string ?? "");
            var formatted = Format(raw);

            SetIsInternalUpdate(tb, true);
            tb.Text = formatted;
            tb.CaretIndex = tb.Text.Length;
            SetIsInternalUpdate(tb, false);
        }

        private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextBox tb) return;

            // allow only letters/digits/spaces, but we will normalize anyway
            if (!e.Text.All(ch => char.IsLetterOrDigit(ch) || ch == ' '))
                e.Handled = true;
        }

        private static void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is not TextBox tb) return;
            if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true)) return;

            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string ?? "";
            if (text.Any(ch => !(char.IsLetterOrDigit(ch) || ch == ' ')))
            {
                e.CancelCommand();
                return;
            }
        }

        private static void OnLostFocusFormat(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                // ensure final formatting
                ApplyFormatKeepingCaret(tb, forceCaretToEnd: false);
            }
        }

        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox tb) return;
            if (GetIsInternalUpdate(tb)) return;

            ApplyFormatKeepingCaret(tb, forceCaretToEnd: false);
        }

        private static void ApplyFormatKeepingCaret(TextBox tb, bool forceCaretToEnd)
        {
            var oldText = tb.Text ?? "";
            var oldCaret = tb.CaretIndex;

            // raw index (how many raw chars are before caret)
            int rawIndex = GetRawIndexFromCaret(oldText, oldCaret);

            // normalize & format
            var raw = NormalizeRaw(oldText);
            var formatted = Format(raw);

            // compute new caret from raw index
            int newCaret = GetCaretFromRawIndex(formatted, rawIndex);

            SetIsInternalUpdate(tb, true);
            tb.Text = formatted;

            tb.CaretIndex = forceCaretToEnd ? formatted.Length : Math.Max(0, Math.Min(formatted.Length, newCaret));
            SetIsInternalUpdate(tb, false);

            // update VM binding value (raw)
            SetIsInternalUpdate(tb, true);
            SetIban(tb, raw);
            SetIsInternalUpdate(tb, false);
        }

        // Remove spaces, uppercase, keep only letters/digits
        private static string NormalizeRaw(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (char.IsLetterOrDigit(ch))
                    sb.Append(char.ToUpperInvariant(ch));
            }
            return sb.ToString();
        }

        // IBAN grouping: DE34 0000 2345 ...
        private static string Format(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";

            // group by 4 from start
            var sb = new StringBuilder(raw.Length + raw.Length / 4);
            for (int i = 0; i < raw.Length; i++)
            {
                if (i > 0 && i % 4 == 0) sb.Append(' ');
                sb.Append(raw[i]);
            }
            return sb.ToString();
        }

        // Count non-space chars before caret
        private static int GetRawIndexFromCaret(string formattedText, int caretIndex)
        {
            int count = 0;
            int limit = Math.Max(0, Math.Min(formattedText.Length, caretIndex));
            for (int i = 0; i < limit; i++)
            {
                if (formattedText[i] != ' ')
                    count++;
            }
            return count;
        }

        // Find caret position such that there are rawIndex non-space chars before caret
        private static int GetCaretFromRawIndex(string formattedText, int rawIndex)
        {
            if (rawIndex <= 0) return 0;

            int count = 0;
            for (int i = 0; i < formattedText.Length; i++)
            {
                if (formattedText[i] != ' ')
                    count++;

                if (count >= rawIndex)
                    return i + 1;
            }
            return formattedText.Length;
        }
    }
