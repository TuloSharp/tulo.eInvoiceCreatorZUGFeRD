using System.Windows;
using System.Windows.Media;

namespace tulo.ResourcesWpfLib.Utilities;

public static class FindParrentUtility
{
    public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
    {
        var parentObject = VisualTreeHelper.GetParent(child);

        // we've reached the end of the tree
        if (parentObject == null) return null;

        // check if the parent matches the type we're looking for
        if (parentObject is T parent)
            return parent;
        else
            return parentObject.FindParent<T>();
    }
}
