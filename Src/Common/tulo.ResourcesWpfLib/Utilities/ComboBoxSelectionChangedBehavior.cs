using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace tulo.ResourcesWpfLib.Utilities;

public class ComboBoxSelectionChangedBehavior : Behavior<ComboBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SelectionChanged += OnSelectionChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.SelectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AssociatedObject.IsKeyboardFocusWithin || AssociatedObject.IsMouseOver)
        {
            // Hier können Sie den Befehl oder eine Methode auslösen
            var command = Command;
            if (command != null && command.CanExecute(CommandParameter))
            {
                command.Execute(CommandParameter);
            }
        }
    }

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(System.Windows.Input.ICommand), typeof(ComboBoxSelectionChangedBehavior));

    public System.Windows.Input.ICommand Command
    {
        get => (System.Windows.Input.ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(ComboBoxSelectionChangedBehavior));

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
}
