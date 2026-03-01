using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;

namespace tulo.ResourcesWpfLib.Utilities;

public class StoreListViewSelection : Behavior<ListView>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject != null)
        {
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }
    }

    private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView grid)
        {
            if (grid?.SelectedItem != null)
            {
                ScrollToItemUtilitiy.SaveContext(grid);
            }
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject != null)
        {
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }
    }
}
