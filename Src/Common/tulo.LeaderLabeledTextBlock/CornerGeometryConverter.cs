using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace tulo.LeaderLabeledTextBlock;
public class CornerGeometryConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        double w = values.Length > 0 && values[0] is double dw ? dw : 0;
        double h = values.Length > 1 && values[1] is double dh ? dh : 0;
        double t = values.Length > 2 && values[2] is double dt ? dt : 1;

        double half = Math.Max(0, t / 2.0);

        // Endpunkte so setzen, dass die Ecke sauber zusammenkommt
        double x = Math.Max(0, w - half);
        double y = Math.Max(0, h - half);

        var fig = new PathFigure
        {
            StartPoint = new Point(0, y),
            IsClosed = false,
            IsFilled = false
        };

        fig.Segments.Add(new LineSegment(new Point(x, y), true));
        fig.Segments.Add(new LineSegment(new Point(x, 0), true));

        var geo = new PathGeometry();
        geo.Figures.Add(fig);
        return geo;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
