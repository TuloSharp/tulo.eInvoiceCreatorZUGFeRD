using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace tulo.ResourcesWpfLib.Utilities;

public class ListGridViewSortByHeader
{
    public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached("Command", typeof(ICommand),
            typeof(ListGridViewSortByHeader), new UIPropertyMetadata(null, (dpObj, dpObjEvent) =>
            {
                var listView = dpObj as ItemsControl;
                if (listView != null)
                {
                    if (!GetAutoSort(listView)) // Don't change click handler if AutoSort enabled
                    {
                        if (dpObjEvent.OldValue != null && dpObjEvent.NewValue == null)
                        {
                            listView.RemoveHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, new RoutedEventHandler(OnColumnHeaderClicked));
                        }
                        if (dpObjEvent.OldValue == null && dpObjEvent.NewValue != null)
                        {
                            listView.AddHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, new RoutedEventHandler(OnColumnHeaderClicked));
                        }
                    }
                }
            }));

    public static ICommand GetCommand(DependencyObject obj)
    {
        return (ICommand)obj.GetValue(CommandProperty);
    }

    public static void SetCommand(DependencyObject obj, ICommand value)
    {
        obj.SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty AutoSortProperty = DependencyProperty.RegisterAttached("AutoSort", typeof(bool),
            typeof(ListGridViewSortByHeader), new UIPropertyMetadata(false, (dpObj, dpObjEvent) =>
            {
                var listView = dpObj as ListView;
                if (listView != null)
                {
                    if (GetCommand(listView) == null) // Don't change click handler if a command is set
                    {
                        bool oldValue = (bool)dpObjEvent.OldValue;
                        bool newValue = (bool)dpObjEvent.NewValue;

                        RestoreSort(listView);

                        if (oldValue && !newValue)
                        {
                            listView.RemoveHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, new RoutedEventHandler(OnColumnHeaderClicked));
                        }
                        if (!oldValue && newValue)
                        {
                            listView.AddHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, new RoutedEventHandler(OnColumnHeaderClicked));
                        }
                    }
                }
            }));

    public static bool GetAutoSort(DependencyObject obj)
    {
        return (bool)obj.GetValue(AutoSortProperty);
    }

    public static void SetAutoSort(DependencyObject obj, bool value)
    {
        obj.SetValue(AutoSortProperty, value);
    }

    public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.RegisterAttached("PropertyName", typeof(string),
         typeof(ListGridViewSortByHeader), new UIPropertyMetadata(null));
    public static string GetPropertyName(DependencyObject obj)
    {
        return (string)obj.GetValue(PropertyNameProperty);
    }

    public static void SetPropertyName(DependencyObject obj, string value)
    {
        obj.SetValue(PropertyNameProperty, value);
    }

    public static readonly DependencyProperty ShowSortGlyphProperty = DependencyProperty.RegisterAttached("ShowSortGlyph",
        typeof(bool), typeof(ListGridViewSortByHeader), new UIPropertyMetadata(true));
    public static bool GetShowSortGlyph(DependencyObject obj)
    {
        return (bool)obj.GetValue(ShowSortGlyphProperty);
    }

    public static void SetShowSortGlyph(DependencyObject obj, bool value)
    {
        obj.SetValue(ShowSortGlyphProperty, value);
    }

    public static readonly DependencyProperty SortGlyphAscendingProperty = DependencyProperty.RegisterAttached("SortGlyphAscending",
            typeof(ImageSource), typeof(ListGridViewSortByHeader), new UIPropertyMetadata(null));
    public static ImageSource GetSortGlyphAscending(DependencyObject obj)
    {
        return (ImageSource)obj.GetValue(SortGlyphAscendingProperty);
    }

    public static void SetSortGlyphAscending(DependencyObject obj, ImageSource value)
    {
        obj.SetValue(SortGlyphAscendingProperty, value);
    }

    public static readonly DependencyProperty SortGlyphDescendingProperty = DependencyProperty.RegisterAttached("SortGlyphDescending",
        typeof(ImageSource), typeof(ListGridViewSortByHeader), new UIPropertyMetadata(null));
    public static ImageSource GetSortGlyphDescending(DependencyObject obj)
    {
        return (ImageSource)obj.GetValue(SortGlyphDescendingProperty);
    }

    public static void SetSortGlyphDescending(DependencyObject obj, ImageSource value)
    {
        obj.SetValue(SortGlyphDescendingProperty, value);
    }

    private static readonly DependencyProperty SortedColumnHeaderProperty = DependencyProperty.RegisterAttached("SortedColumnHeader",
        typeof(GridViewColumnHeader), typeof(ListGridViewSortByHeader), new UIPropertyMetadata(null));

    private static GridViewColumnHeader GetSortedColumnHeader(DependencyObject obj)
    {
        return (GridViewColumnHeader)obj.GetValue(SortedColumnHeaderProperty);
    }

    private static void SetSortedColumnHeader(DependencyObject obj, GridViewColumnHeader value)
    {
        obj.SetValue(SortedColumnHeaderProperty, value);
    }

    private static void OnColumnHeaderClicked(object sender, RoutedEventArgs e)
    {
        var headerClicked = e.OriginalSource as GridViewColumnHeader;
        if (headerClicked != null && headerClicked.Column != null)
        {
            string propertyName = GetPropertyName(headerClicked.Column);
            if (!string.IsNullOrEmpty(propertyName))
            {
                ListView listView = GetAncestor<ListView>(headerClicked);
                if (listView != null)
                {
                    ICommand command = GetCommand(listView);
                    if (command != null)
                    {
                        if (command.CanExecute(propertyName))
                        {
                            command.Execute(propertyName);
                        }
                    }
                    else if (GetAutoSort(listView))
                    {
                        ApplySort(listView.Items, propertyName, listView, headerClicked);
                    }
                }
            }
        }
    }

    public static T GetAncestor<T>(DependencyObject reference) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(reference);
        while (!(parent is T))
        {
            parent = VisualTreeHelper.GetParent(parent);
        }
        if (parent != null)
            return (T)parent;
        else
            return null;
    }

    public static void ApplySort(ICollectionView view, string propertyName, ListView listView, GridViewColumnHeader sortedColumnHeader)
    {
        ListSortDirection direction = ListSortDirection.Ascending;
        if (view.SortDescriptions.Count > 0)
        {
            var currentSort = view.SortDescriptions[0];
            if (currentSort.PropertyName == propertyName)
            {
                if (currentSort.Direction == ListSortDirection.Ascending)
                    direction = ListSortDirection.Descending;
                else
                    direction = ListSortDirection.Ascending;
            }

            UtilitiyRememberSortDirection.AddSortInfo(listView.Name, propertyName, direction.ToString());

            view.SortDescriptions.Clear();

            var currentSortedColumnHeader = GetSortedColumnHeader(listView);
            if (currentSortedColumnHeader != null)
            {
                RemoveSortGlyph(currentSortedColumnHeader);
            }
        }
        if (!string.IsNullOrEmpty(propertyName))
        {
            view.SortDescriptions.Add(new SortDescription(propertyName, direction));
            if (GetShowSortGlyph(listView))
                AddSortGlyph(sortedColumnHeader, direction, direction == ListSortDirection.Ascending ? GetSortGlyphAscending(listView) : GetSortGlyphDescending(listView));
            SetSortedColumnHeader(listView, sortedColumnHeader);
        }
    }

    private static void AddSortGlyph(GridViewColumnHeader columnHeader, ListSortDirection direction, ImageSource sortGlyph)
    {
        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(columnHeader);
        adornerLayer.Add(new SortGlyphAdorner(columnHeader, direction, sortGlyph));
    }

    private static void RemoveSortGlyph(GridViewColumnHeader columnHeader)
    {
        var adornerLayer = AdornerLayer.GetAdornerLayer(columnHeader);
        Adorner[] adorners = adornerLayer.GetAdorners(columnHeader);
        if (adorners != null)
        {
            foreach (Adorner adorner in adorners)
            {
                if (adorner is SortGlyphAdorner)
                    adornerLayer.Remove(adorner);
            }
        }
    }

    public static void RemoveSortGlyph(ListView listView)
    {
        if (listView == null) return;

        var currentSortedColumnHeader = GetSortedColumnHeader(listView);
        if (currentSortedColumnHeader != null)
        {
            RemoveSortGlyph(currentSortedColumnHeader);
        }
    }

    public static void RestoreSort(ListView listView)
    {
        SortDirectionInfo SortInfo = new();

        SortInfo = UtilitiyRememberSortDirection.GetSortInfo(listView.Name);

        string propertyName = SortInfo?.PropertyName;
        string directionStr = SortInfo?.Direction;

        if (!string.IsNullOrEmpty(propertyName) && !string.IsNullOrEmpty(directionStr))
        {
            ListSortDirection direction = (ListSortDirection)Enum.Parse(typeof(ListSortDirection), directionStr);
            ICollectionView view = CollectionViewSource.GetDefaultView(listView.Items);

            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription(propertyName, direction));
        }
    }

    private class SortGlyphAdorner : Adorner
    {
        private GridViewColumnHeader _columnHeader;
        private ListSortDirection _direction;
        private ImageSource _sortGlyph;

        public SortGlyphAdorner(GridViewColumnHeader columnHeader, ListSortDirection direction, ImageSource sortGlyph)
            : base(columnHeader)
        {
            _columnHeader = columnHeader;
            _direction = direction;
            _sortGlyph = sortGlyph;
        }

        private Geometry GetDefaultGlyph()
        {
            double x1 = _columnHeader.ActualWidth - 13;
            double x2 = x1 + 10;
            double x3 = x1 + 5;
            double y1 = _columnHeader.ActualHeight / 2 - 3;
            double y2 = y1 + 5;

            if (_direction == ListSortDirection.Ascending)
            {
                double tmp = y1;
                y1 = y2;
                y2 = tmp;
            }

            PathSegmentCollection pathSegmentCollection = new PathSegmentCollection();
            pathSegmentCollection.Add(new LineSegment(new Point(x2, y1), true));
            pathSegmentCollection.Add(new LineSegment(new Point(x3, y2), true));

            PathFigure pathFigure = new PathFigure(
                new Point(x1, y1),
                pathSegmentCollection,
                true);

            PathFigureCollection pathFigureCollection = new PathFigureCollection();
            pathFigureCollection.Add(pathFigure);

            PathGeometry pathGeometry = new PathGeometry(pathFigureCollection);
            return pathGeometry;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_sortGlyph != null)
            {
                double x = _columnHeader.ActualWidth - 13;
                double y = _columnHeader.ActualHeight / 2 - 5;
                Rect rect = new Rect(x, y, 10, 10);
                drawingContext.DrawImage(_sortGlyph, rect);
            }
            else
            {
                drawingContext.DrawGeometry(Brushes.LightGray, new Pen(Brushes.Gray, 1.0), GetDefaultGlyph());
            }
        }
    }
}
