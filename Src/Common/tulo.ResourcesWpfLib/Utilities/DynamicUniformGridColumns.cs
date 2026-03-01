using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace tulo.ResourcesWpfLib.Utilities;

public static class DynamicUniformGridColumns
{
    public static readonly DependencyProperty MinItemWidthProperty =
        DependencyProperty.RegisterAttached(
            "MinItemWidth",
            typeof(double),
            typeof(DynamicUniformGridColumns),
            new PropertyMetadata(260.0, OnMinItemWidthChanged));

    public static void SetMinItemWidth(DependencyObject element, double value) =>
        element.SetValue(MinItemWidthProperty, value);

    public static double GetMinItemWidth(DependencyObject element) =>
        (double)element.GetValue(MinItemWidthProperty);

    public static readonly DependencyProperty MaxItemHeightProperty =
        DependencyProperty.RegisterAttached(
            "MaxItemHeight",
            typeof(double),
            typeof(DynamicUniformGridColumns),
            new PropertyMetadata(double.PositiveInfinity));

    public static void SetMaxItemHeight(DependencyObject element, double value) =>
        element.SetValue(MaxItemHeightProperty, value);

    public static double GetMaxItemHeight(DependencyObject element) =>
        (double)element.GetValue(MaxItemHeightProperty);

    private static void OnMinItemWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemsControl ic)
        {
            ic.Loaded -= OnLoaded;
            ic.Loaded += OnLoaded;

            ic.SizeChanged -= OnSizeChanged;
            ic.SizeChanged += OnSizeChanged;
        }
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is ItemsControl ic)
            ic.Dispatcher.BeginInvoke(() => UpdateLayout(ic), DispatcherPriority.Background);
    }

    private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is ItemsControl ic)
            ic.Dispatcher.BeginInvoke(() => UpdateLayout(ic), DispatcherPriority.Background);
    }

    private static void UpdateLayout(ItemsControl ic)
    {
        double minItemWidth = GetMinItemWidth(ic);
        if (minItemWidth <= 0) minItemWidth = 260;

        double maxItemHeight = GetMaxItemHeight(ic);

        // found UniformGrid
        var ug = FindVisualChild<UniformGrid>(ic);
        if (ug == null) return;

        // determine usable width
        double usableWidth = 0;
        var sv = FindVisualChild<ScrollViewer>(ic);
        if (sv != null && sv.ViewportWidth > 0)
        {
            usableWidth = sv.ViewportWidth;
        }
        else
        {
            usableWidth = ic.ActualWidth;
        }

        if (double.IsNaN(usableWidth) || usableWidth <= 0) return;

        const double guard = 24.0;
        usableWidth = Math.Max(0, usableWidth - guard);

        int columns = Math.Max(1, (int)Math.Floor(usableWidth / minItemWidth));
        if (ic.Items.Count > 0)
            columns = Math.Min(columns, Math.Max(1, ic.Items.Count));

        if (ug.Columns != columns)
            ug.Columns = columns;

        // MaxHeight set for all Items
        foreach (var item in ic.Items)
        {
            if (ic.ItemContainerGenerator.ContainerFromItem(item) is ListViewItem lvi)
            {
                lvi.MaxHeight = maxItemHeight;
            }
        }
    }

    private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) return null;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t) return t;
            var result = FindVisualChild<T>(child);
            if (result != null) return result;
        }
        return null;
    }
}
