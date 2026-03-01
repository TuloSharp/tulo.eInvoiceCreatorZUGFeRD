using Microsoft.Xaml.Behaviors;
using System.Collections.Specialized;
using System.Windows.Controls;

namespace tulo.ResourcesWpfLib.Utilities;

public class AutoScrollBehavior : Behavior<ListBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject.Items is INotifyCollectionChanged notify)
        {
            notify.CollectionChanged += (_, __) =>
            {
                if (AssociatedObject.Items.Count > 0)
                    AssociatedObject.ScrollIntoView(AssociatedObject.Items[AssociatedObject.Items.Count - 1]);
            };
        }
    }
}
