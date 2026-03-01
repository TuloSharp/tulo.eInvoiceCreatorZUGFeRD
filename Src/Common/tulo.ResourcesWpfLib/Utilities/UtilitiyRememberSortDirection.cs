using System.Collections.Generic;
using System.Linq;

namespace tulo.ResourcesWpfLib.Utilities;

public class SortDirectionInfo
{
    public string PropertyName { get; set; }
    public string Direction { get; set; }
}

public static class UtilitiyRememberSortDirection
{
    private static readonly Dictionary<string, List<SortDirectionInfo>> _viewSortDirections = new();

    public static void AddSortInfo(string viewName, string propertyName, string direction)
    {
        if (!_viewSortDirections.ContainsKey(viewName))
        {
            _viewSortDirections[viewName] = new List<SortDirectionInfo>();
        }
        else
        {
            _viewSortDirections[viewName].Clear();
        }

        _viewSortDirections[viewName].Add(new SortDirectionInfo
        {
            PropertyName = propertyName,
            Direction = direction
        });
    }

    public static SortDirectionInfo GetSortInfo(string viewName)
    {
        if (_viewSortDirections.ContainsKey(viewName))
        {
            return _viewSortDirections[viewName].FirstOrDefault();
        }

        return null;
    }

}
