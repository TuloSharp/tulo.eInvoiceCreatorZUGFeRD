using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace tulo.ResourcesWpfLib.Utilities;

public class MouseWheelScrollBehavior : Behavior<TabControl>
{
    private ScrollViewer _scrollViewer;
    private TabPanel _tabPanel;

    protected override void OnAttached()
    {
        AssociatedObject.Loaded += OnLoaded;
        AssociatedObject.PreviewMouseWheel += OnPreviewMouseWheel;
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= OnLoaded;
        AssociatedObject.PreviewMouseWheel -= OnPreviewMouseWheel;
        base.OnDetaching();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _scrollViewer = FindChild<ScrollViewer>(AssociatedObject);
        _tabPanel = FindChild<TabPanel>(AssociatedObject);
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_scrollViewer != null && _tabPanel != null)
        {
            if (IsMouseOverElement(_tabPanel))
            {
                _scrollViewer.ScrollToHorizontalOffset(_scrollViewer.HorizontalOffset - e.Delta / 2.0);
                e.Handled = true;
            }
        }
    }

    private static T FindChild<T>(DependencyObject obj) where T : DependencyObject
    {
        if (obj is T child) return child;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            var foundChild = FindChild<T>(VisualTreeHelper.GetChild(obj, i));
            if (foundChild != null) return foundChild;
        }

        return null;
    }

    private static bool IsMouseOverElement(UIElement element)
    {
        if (element == null) return false;
        var position = Mouse.GetPosition(element);
        return position.X >= 0 && position.X <= element.RenderSize.Width &&
               position.Y >= 0 && position.Y <= element.RenderSize.Height;
    }
}