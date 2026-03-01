using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace tulo.ResourcesWpfLib.Utilities;

public class HeaderCheckBoxBehavior : Behavior<FrameworkElement>
{
    public string MethodName { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Loaded += AssociatedObject_Loaded;
    }

    private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
    {
        // Find the CheckBox in the GridViewColumnHeader's template
        var checkBox = FindVisualChild<CheckBox>(AssociatedObject);

        if (checkBox != null)
        {
            checkBox.Click += OnCheckBoxClicked;
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        var checkBox = FindVisualChild<CheckBox>(AssociatedObject);
        if (checkBox != null)
        {
            checkBox.Click -= OnCheckBoxClicked;
        }
    }

    private void OnCheckBoxClicked(object sender, RoutedEventArgs e)
    {
        var dataContext = AssociatedObject.DataContext;

        if (dataContext != null && !string.IsNullOrEmpty(MethodName))
        {
            // Get the parent GridViewColumnHeader (where the CheckBox is located)
            var header = AssociatedObject as GridViewColumnHeader;

            if (header != null)
            {
                // Extract the header content (i.e., "Column 1")
                var headerText = header.Content?.ToString() ?? "Unknown";

                // Get the parent ListView (that holds the GridView)
                var listView = FindAncestor<ListView>(header);

                // Find the corresponding GridViewColumn for this header
                var gridView = listView.View as GridView;
                var columnIndex = GetColumnIndex(gridView, header);

                if (columnIndex >= 0)
                {
                    // Get the GridViewColumn for this index
                    var column = gridView.Columns[columnIndex];

                    // Get the DisplayMemberBinding of this column
                    var displayMemberBinding = column.DisplayMemberBinding as Binding;

                    // Get the property path (binding path)
                    var propertyPath = displayMemberBinding?.Path?.Path ?? "Unknown";

                    // Get the checkbox state (checked/unchecked)
                    var checkBox = sender as CheckBox;
                    bool? isChecked = checkBox?.IsChecked;

                    // Invoke the method with the header text, DisplayMemberBinding, and checkbox state as parameters
                    var methodInfo = dataContext.GetType().GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public);
                    if (methodInfo != null)
                    {
                        methodInfo.Invoke(dataContext, new object[] { headerText, propertyPath, isChecked });
                    }
                }
            }
        }
    }

    // Helper method to find the CheckBox within the control template
    private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(obj, i);
            if (child != null && child is T t)
            {
                return t;
            }

            T childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null)
            {
                return childOfChild;
            }
        }
        return null;
    }

    // Helper method to find an ancestor of a given type
    private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        while (current != null && !(current is T))
        {
            current = VisualTreeHelper.GetParent(current);
        }
        return current as T;
    }

    // Helper method to get the index of the GridViewColumn from the header
    private static int GetColumnIndex(GridView gridView, GridViewColumnHeader header)
    {
        for (int i = 0; i < gridView.Columns.Count; i++)
        {
            if (gridView.Columns[i].Header == header.Content)
            {
                return i;
            }
        }
        return -1; // Column not found
    }
}

