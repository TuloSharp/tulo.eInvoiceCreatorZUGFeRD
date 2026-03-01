using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace tulo.ResourcesWpfLib.Utilities;

public static class BehaviorHeaderClicked
{
    public static readonly DependencyProperty AttachBehaviorProperty = DependencyProperty.RegisterAttached("AttachBehavior", typeof(bool), typeof(BehaviorHeaderClicked), new PropertyMetadata(false, OnAttachBehaviorChanged));

    public static bool GetAttachBehavior(DependencyObject obj)
    {
        return (bool)obj.GetValue(AttachBehaviorProperty);
    }

    public static void SetAttachBehavior(DependencyObject obj, bool value)
    {
        obj.SetValue(AttachBehaviorProperty, value);
    }

    private static void OnAttachBehaviorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        GridViewColumnHeader header = d as GridViewColumnHeader;
        if (header != null && (bool)e.NewValue)
        {
            var behaviors = Interaction.GetBehaviors(header);
            var behavior = new HeaderCheckBoxBehavior
            {
                MethodName = "HeaderClicked" // Replace with the method you want to call
            };
            behaviors.Add(behavior);
        }
    }
}