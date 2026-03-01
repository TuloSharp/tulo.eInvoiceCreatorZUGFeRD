using System;
using System.ComponentModel;

namespace tulo.ResourcesWpfLib.Utilities;

public static class GetEnumFromDescription
{
    public static T ParsedEnum<T>(string description) where T : Enum
    {
        foreach (var fieldInfo in typeof(T).GetFields())
        {
            if (Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                if (attribute.Description == description)
                    return (T)fieldInfo.GetValue(null);
            }
            else
            {
                if (fieldInfo.Name == description)
                    return (T)fieldInfo.GetValue(null);
            }
        }

        return default;
    }
}
