using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace tulo.ResourcesWpfLib.Utilities;

public class CustomTabStopControl : DependencyObject
{
    public static bool GetIsCustomTabControlEnabled(DependencyObject obj)
    {
        return (bool)obj.GetValue(CustomTabControl);
    }

    public static void SetIsCustomTabControlEnabled(DependencyObject obj, bool value)
    {
        obj.SetValue(CustomTabControl, value);
    }

    public static readonly DependencyProperty CustomTabControl = DependencyProperty.RegisterAttached("CustomTabControlEnabled", typeof(bool), typeof(CustomTabStopControl), new UIPropertyMetadata(false, OnIsCustomTabControlEnabled));

    private static void OnIsCustomTabControlEnabled(DependencyObject dO, DependencyPropertyChangedEventArgs e)
    {
        if (dO is UIElement targetElement)
        {
            if ((bool)e.NewValue)
            {
                targetElement.PreviewKeyDown += PreviewKeyDown;
            }
            else if ((bool)e.OldValue)
            {
                targetElement.PreviewKeyDown -= PreviewKeyDown;
                KeyboardNavigation.SetTabNavigation(targetElement, KeyboardNavigationMode.Continue);
            }
        }
    }

    private static void PreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Tab when Keyboard.Modifiers == ModifierKeys.Shift:
                MoveToPreviousElement(sender, e);
                break;
            case Key.Tab:
            case Key.Enter:
                MoveToNextElement(sender, e);
                break;
            default:
                if (IsLetterKey(e.Key))
                {
                    if (sender is TextBox tb && tb.CaretIndex < tb.MaxLength)
                    {
                        string charRegex = TextBoxProperties.GetAllowedCharactersRegex(tb);
                        if (!Regex.IsMatch(e.Key.ToString(), charRegex))
                        {
                            e.Handled = true;
                        }
                        else if (tb.SelectedText != string.Empty)
                        {
                            SelectionOverwrite(sender);
                        }
                        else if (IsTextBoxFilled(tb))
                        {
                            int helperCaretIndexRememberer = tb.CaretIndex;
                            tb.Select(tb.SelectionStart, 1);
                            tb.Text = string.Concat(tb.Text.AsSpan(0, tb.SelectionStart), e.Key.ToString(), tb.Text.AsSpan(tb.SelectionStart + tb.SelectionLength));
                            helperCaretIndexRememberer++;

                            tb.CaretIndex = helperCaretIndexRememberer;
                            e.Handled = true;
                        }
                        else
                        {
                            MoveToNextElement(sender, e);
                        }
                    }
                }
                else if (IsNumberKey(e.Key))
                {
                    if (sender is TextBox tb)
                    {
                        if (tb.CaretIndex < tb.MaxLength)
                        {
                            int keyValue = KeyToInt(e.Key);
                            string charRegex = TextBoxProperties.GetAllowedCharactersRegex(tb);
                            if (!Regex.IsMatch(keyValue.ToString(), charRegex))
                            {
                                e.Handled = true;
                            }
                            else if (tb.SelectedText != string.Empty)
                            {
                                SelectionOverwrite(sender);
                            }
                            else if (IsTextBoxFilled(tb))
                            {
                                int helperCaretIndexRememberer = tb.CaretIndex;
                                tb.Select(tb.SelectionStart, 1);
                                tb.Text = string.Concat(tb.Text.AsSpan(0, tb.SelectionStart), keyValue.ToString(), tb.Text.AsSpan(tb.SelectionStart + tb.SelectionLength));
                                helperCaretIndexRememberer++;

                                tb.CaretIndex = helperCaretIndexRememberer;
                                e.Handled = true;
                            }
                        }
                        else
                        {
                            MoveToNextElement(sender, e);
                        }
                    }
                }
                else if (e.Key == Key.Oem2)
                {
                    e.Handled = true;
                }
                break;

        }
    }

    private static void MoveToNextElement(object sender, KeyEventArgs e)
    {
        if (IsTextBoxFilled(sender) && sender is UIElement currentElement)
        {
            currentElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            e.Handled = true;
            if (Keyboard.FocusedElement is TextBox textBox)
            {
                textBox.CaretIndex = 0;

                if (IsLetterKey(e.Key) || IsNumberKey(e.Key))
                {
                    string key = e.Key.ToString();
                    if (IsNumberKey(e.Key))
                        key = KeyToInt(e.Key).ToString();

                    if (!string.IsNullOrEmpty(textBox.Text))
                        textBox.Text = string.Concat(textBox.Text.AsSpan(0, textBox.SelectionStart), key, textBox.Text.AsSpan(textBox.SelectionStart + 1));
                    else
                        textBox.Text = key;

                    textBox.CaretIndex = 1;
                }

                if (GetIsCustomTabControlEnabled(textBox) && (e.Key == Key.Tab || e.Key == Key.Enter))
                    textBox.SelectAll();
            }
        }
    }

    private static void MoveToPreviousElement(object sender, KeyEventArgs e)
    {
        if (sender is UIElement currentElement && Keyboard.FocusedElement is TextBox textBox)
        {
            textBox.CaretIndex = 0;

            currentElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));

            if (GetIsCustomTabControlEnabled(textBox))
                textBox.SelectAll();
            e.Handled = true;
        }
    }

    private static bool IsTextBoxFilled(object element)
    {
        if (element is TextBox textBox)
        {
            return textBox.Text.Length >= textBox.MaxLength;
        }
        return false;
    }

    private static bool IsLetterKey(Key key)
    {
        return key >= Key.A && key <= Key.Z;
    }

    private static bool IsNumberKey(Key key)
    {
        return key >= Key.D0 && key <= Key.D9 || key >= Key.NumPad0 && key <= Key.NumPad9;
    }

    private static bool IsFunctionalityKey(Key key)
    {
        return key == Key.Escape || key == Key.Scroll || key == Key.End || key == Key.Home || key == Key.LWin || key == Key.RWin || key == Key.System ||
               key == Key.CapsLock || key == Key.Insert || key == Key.Pause || key == Key.PageUp || key == Key.PageDown || key == Key.PrintScreen;
    }
    private static bool IsModifierKey(Key key)
    {
        return key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftAlt || key == Key.RightAlt;
    }
    private static int KeyToInt(Key key)
    {
        if (key >= Key.D0 && key <= Key.D9)
        {
            return key - Key.D0;
        }
        else if (key >= Key.NumPad0 && key <= Key.NumPad9)
        {
            return key - Key.NumPad0;
        }

        return default;
    }

    private static bool SelectionOverwrite(object element)
    {
        if (element is TextBox tb)
        {
            if (tb.SelectionLength == tb.MaxLength)
            {
                tb.Text = string.Empty;
            }
            else
            {
                tb.SelectedText = string.Empty;
            }
        }
        return false;
    }
}