using System.Windows;
using System.Windows.Input;

namespace tulo.ResourcesWpfLib.Utilities;

public static class MarkdownBehavior
{
    public static readonly DependencyProperty LinkCommandProperty = DependencyProperty.RegisterAttached("LinkCommand", typeof(ICommand), typeof(MarkdownBehavior), new PropertyMetadata(null, OnLinkCommandChanged));

    public static void SetLinkCommand(DependencyObject d, ICommand value) => d.SetValue(LinkCommandProperty, value);
    public static ICommand GetLinkCommand(DependencyObject d) => (ICommand)d.GetValue(LinkCommandProperty);

    private static void OnLinkCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement el && e.NewValue is ICommand cmd)
        {
            el.CommandBindings.Add(new CommandBinding(Markdig.Wpf.Commands.Hyperlink, (s, args) =>
            {
                if (cmd.CanExecute(args.Parameter)) cmd.Execute(args.Parameter);
            }));
        }
    }
}
