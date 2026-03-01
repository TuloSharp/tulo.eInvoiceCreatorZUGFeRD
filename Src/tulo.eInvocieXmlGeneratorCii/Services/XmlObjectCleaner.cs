using System.Collections;
using System.Reflection;

namespace tulo.eInvoiceXmlGeneratorCii.Services;
public class XmlObjectCleaner : IXmlObjectCleaner
{
    public void RemoveEmptyNodes(object root)
    {
        if (root == null) return;
        CleanNode(root, isRoot: true);
    }

    private bool CleanNode(object node, bool isRoot)
    {
        if (node == null) return false;

        var type = node.GetType();
        if (type == typeof(string))
            return !string.IsNullOrWhiteSpace((string)node);
        if (type.IsValueType)
            return true;

        bool hasData = false;

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;
            if (prop.GetIndexParameters().Length > 0) continue;

            var value = prop.GetValue(node);
            if (value == null) continue;

            var propType = prop.PropertyType;

            if (propType == typeof(string))
            {
                if (string.IsNullOrWhiteSpace((string)value))
                    prop.SetValue(node, null);
                else
                    hasData = true;
            }
            else if (typeof(IEnumerable).IsAssignableFrom(propType) && propType != typeof(string))
            {
                var newItems = new List<object>();
                foreach (var item in (IEnumerable)value)
                {
                    if (item != null && CleanNode(item, false))
                        newItems.Add(item);
                }

                if (newItems.Count == 0)
                    prop.SetValue(node, null);
                else if (value is Array)
                {
                    var elemType = propType.GetElementType() ?? typeof(object);
                    var arr = Array.CreateInstance(elemType, newItems.Count);
                    for (int i = 0; i < newItems.Count; i++)
                        arr.SetValue(newItems[i], i);
                    prop.SetValue(node, arr);
                    hasData = true;
                }
                else if (value is IList list)
                {
                    list.Clear();
                    foreach (var item in newItems)
                        list.Add(item);
                    hasData = true;
                }
            }
            else
            {
                if (!CleanNode(value, false))
                    prop.SetValue(node, null);
                else
                    hasData = true;
            }
        }

        return isRoot || hasData;
    }
}
