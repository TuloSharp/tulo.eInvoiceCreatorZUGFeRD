using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace tulo.ResourcesWpfLib.Utilities;

public static class ScrollToItemUtilitiy
{
   
    public static ListView TargetListView { get; private set; }

    public static void SaveContext(ListView listView)
    {
        TargetListView = listView;
    }
   
    public static void ClearContext()
    {
        TargetListView = null;
    }

    private static void ScrollToItem(ListView listView, int selectedIndex)
    {
        var scrollPosition = Math.Max(0, Math.Min(selectedIndex - (selectedIndex < listView.Items.Count / 2 ? 1 : -1), listView.Items.Count - 1)) * 15; // selectedIndex (+1 if in second half, or -1 if in first half) * ItemHeight

        if (listView != null)
        {
            var scrollViewer = GetScrollViewer(listView);
            scrollViewer?.ScrollToVerticalOffset(scrollPosition);
        }
    }

    private static ScrollViewer GetScrollViewer(DependencyObject element)
    {
        if (element is ScrollViewer)
            return (ScrollViewer)element;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
            var child = VisualTreeHelper.GetChild(element, i);
            var scrollViewer = GetScrollViewer(child);
            if (scrollViewer != null)
                return scrollViewer;
        }

        return null;
    }

    public static void TriggerScroll()
    {
        if (TargetListView == null || TargetListView.Items.Count == 0)
            return;

        TargetListView.Dispatcher.InvokeAsync(() =>
        {
            if (TargetListView == null || TargetListView.Items.Count == 0)
                return;

            if (TargetListView.Items.Count > 0)
            {
                var selectedItem = TargetListView.SelectedItem ?? TargetListView.Items[TargetListView.Items.Count - 1];
                int selectedIndex = TargetListView.Items.IndexOf(selectedItem);

                if (selectedIndex != -1)
                {
                    ScrollToItem(TargetListView, selectedIndex);
                }
                ClearContext();
            }
        });
    }
}
