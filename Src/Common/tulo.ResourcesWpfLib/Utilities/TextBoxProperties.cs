using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace tulo.ResourcesWpfLib.Utilities;

public class TextBoxProperties : TextBox
{
    /// <summary>
    /// RegexPattern to check the characters allowed as input in a TextBox
    /// </summary>
    public static readonly DependencyProperty AllowedCharactersRegexProperty = DependencyProperty.RegisterAttached("AllowedCharactersRegex", typeof(string), typeof(TextBoxProperties), new UIPropertyMetadata(null, AllowedCharactersRegexChanged));

    /// <summary>
    /// DependencyProperty for SpecialTextBox where No Space is allowed
    /// </summary>
    public static readonly DependencyProperty IsNoSpaceTextBoxProperty = DependencyProperty.RegisterAttached("IsNoSpaceTextBox", typeof(bool), typeof(TextBoxProperties), new UIPropertyMetadata(false));

    public static string GetAllowedCharactersRegex(DependencyObject obj)
    {
        return (string)obj.GetValue(AllowedCharactersRegexProperty);
    }
    public static void SetAllowedCharactersRegex(DependencyObject obj, string value)
    {
        obj.SetValue(AllowedCharactersRegexProperty, value);
    }

    public static bool GetIsNoSpaceTextBox(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsNoSpaceTextBoxProperty);
    }
    public static void SetIsNoSpaceTextBox(DependencyObject obj, bool value)
    {
        obj.SetValue(IsNoSpaceTextBoxProperty, value);
    }

    // Events for Regex
    public static void AllowedCharactersRegexChanged(DependencyObject obj, DependencyPropertyChangedEventArgs eventArgs)
    {
        TextBox textBox = obj as TextBox;
        if (textBox != null)
        {
            if (eventArgs.NewValue != null)
            {
                textBox.PreviewTextInput += Textbox_PreviewTextChanged;
                DataObject.AddPastingHandler(textBox, TextBox_OnPaste);

                if (GetIsNoSpaceTextBox(textBox))
                {
                    textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
                }
                else
                {
                    textBox.PreviewKeyDown -= TextBox_PreviewKeyDown;
                }
            }
            else
            {
                textBox.PreviewTextInput -= Textbox_PreviewTextChanged;
                DataObject.RemovePastingHandler(textBox, TextBox_OnPaste);
                textBox.PreviewKeyDown -= TextBox_PreviewKeyDown;
            }
        }
    }

    public static void TextBox_OnPaste(object sender, DataObjectPastingEventArgs eventArgs)
    {
        TextBox textBox = sender as TextBox;

        bool isText = eventArgs.SourceDataObject.GetDataPresent(DataFormats.Text, true);
        if (!isText) return;

        string newText = eventArgs.SourceDataObject.GetData(DataFormats.Text) as string;
        string regEx = GetAllowedCharactersRegex(textBox);
        regEx = string.Format("{0}", regEx);

        if (!Regex.IsMatch(newText.Trim(), regEx, RegexOptions.IgnoreCase))
        {
            eventArgs.CancelCommand();
        }
    }

    public static void Textbox_PreviewTextChanged(object sender, TextCompositionEventArgs eventArgs)
    {
        TextBox textBox = sender as TextBox;
        if (textBox != null)
        {
            string regEx = GetAllowedCharactersRegex(textBox);
            regEx = string.Format("{0}", regEx);

            bool isMatch = Regex.IsMatch(eventArgs.Text, regEx);
            eventArgs.Handled = isMatch ? false : true;
        }
    }

    // Events for KeyDown
    public static void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        TextBox textBox = sender as TextBox;

        if (textBox != null)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
    }
}