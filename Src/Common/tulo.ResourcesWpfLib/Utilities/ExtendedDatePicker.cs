using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace tulo.ResourcesWpfLib.Utilities;

public class ExtendedDatePicker : DatePicker
{
    private TextBox _textBox;
    private bool _internalUpdate;
    private string _lastUserText = string.Empty;
    private static readonly CultureInfo _de = CultureInfo.GetCultureInfo("de-DE");

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_textBox != null)
        {
            _textBox.TextChanged -= TextBox_TextChanged;
            _textBox.LostKeyboardFocus -= TextBox_LostKeyboardFocus;
            _textBox.PreviewTextInput -= TextBox_PreviewTextInput;
            DataObject.RemovePastingHandler(_textBox, TextBox_Paste);
        }

        _textBox = GetTemplateChild("PART_TextBox") as TextBox;
        if (_textBox == null) return;

        BindingOperations.ClearBinding(_textBox, TextBox.TextProperty);
        BindingOperations.SetBinding(_textBox, TextBox.TextProperty,
            new Binding(nameof(Text))
            {
                Source = this,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.LostFocus
            });

        _textBox.TextChanged += TextBox_TextChanged;
        _textBox.PreviewTextInput += TextBox_PreviewTextInput;
        _textBox.LostKeyboardFocus += TextBox_LostKeyboardFocus;
        DataObject.AddPastingHandler(_textBox, TextBox_Paste);
    }

    private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Only digits and dot
        if (!Regex.IsMatch(e.Text, @"^[0-9.]$"))
        {
            e.Handled = true;
            return;
        }

        if (_textBox == null) return;

        // Maximum 2 points
        if (e.Text == "." && _textBox.Text.Count(c => c == '.') >= 2)
        {
            e.Handled = true;
            return;
        }

        // Optional: max length 10 (dd.MM.yyyy)
        if (_textBox.Text.Length >= 10 && _textBox.SelectionLength == 0)
            e.Handled = true;
    }

    private void TextBox_Paste(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(DataFormats.UnicodeText))
        {
            e.CancelCommand();
            return;
        }

        var paste = e.DataObject.GetData(DataFormats.UnicodeText) as string ?? "";
        if (paste.Length == 0) return;

        // If paste is completely “wrong”, block
        if (!paste.All(ch => char.IsDigit(ch) || ch == '.' || char.IsWhiteSpace(ch)))
            e.CancelCommand();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_internalUpdate) return;
        if (_textBox == null) return;

        _internalUpdate = true;
        try
        {
            int caret = _textBox.CaretIndex;
            string text = _textBox.Text;

            // only numbers and dots
            text = Regex.Replace(text, @"[^0-9.]", "");

            // max 2 points: remove everything from the 3rd point onwards
            int firstDot = text.IndexOf('.');
            int secondDot = firstDot >= 0 ? text.IndexOf('.', firstDot + 1) : -1;
            if (secondDot != -1)
            {
                int thirdDot = text.IndexOf('.', secondDot + 1);
                while (thirdDot != -1)
                {
                    text = text.Remove(thirdDot, 1);
                    thirdDot = text.IndexOf('.', secondDot + 1);
                }
            }

            // Insert points automatically (dd.MM.yyyy)
            if (text.Length > 2 && firstDot == -1)
            {
                text = text.Insert(2, ".");
                caret++;
            }

            firstDot = text.IndexOf('.');
            secondDot = firstDot >= 0 ? text.IndexOf('.', firstDot + 1) : -1;

            if (text.Length > 5 && secondDot == -1)
            {
                text = text.Insert(5, ".");
                caret++;
            }

            // max 10 characters (dd.MM.yyyy)
            if (text.Length > 10) text = text.Substring(0, 10);

            if (_textBox.Text != text)
                _textBox.SetCurrentValue(TextBox.TextProperty, text);

            _textBox.CaretIndex = Math.Min(caret, _textBox.Text.Length);
            _lastUserText = text;
        }
        finally
        {
            _internalUpdate = false;
        }
    }

    private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (_internalUpdate) return;
        if (_textBox == null) return;

        var typed = _lastUserText;
        if (string.IsNullOrWhiteSpace(typed)) return;

        bool valid = typed.Length == 10 && DateTime.TryParseExact(typed, "dd.MM.yyyy", _de, DateTimeStyles.None, out _);

        if (valid) return;

        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (_textBox == null) return;

            _internalUpdate = true;
            try
            {
                _textBox.SetCurrentValue(TextBox.TextProperty, typed);
                SetCurrentValue(TextProperty, typed);

                _textBox.CaretIndex = _textBox.Text.Length;
            }
            finally { _internalUpdate = false; }
            GetBindingExpression(TextProperty)?.UpdateSource();
        }), DispatcherPriority.DataBind);
    }
}

