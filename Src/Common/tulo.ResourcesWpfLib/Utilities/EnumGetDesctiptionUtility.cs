using System;
using System.ComponentModel;
using System.Reflection;

namespace tulo.ResourcesWpfLib.Utilities;

public static class EnumGetDesctiptionUtility
{
    public static string GetDescription(Enum _enum)
    {
        Type type = _enum.GetType();
        MemberInfo[] memberInfo = type.GetMember(_enum.ToString());
        if (memberInfo != null && memberInfo.Length > 0)
        {
            object[] attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                return ((DescriptionAttribute)attributes[0]).Description;
            }
        }
        return _enum.ToString();
    }
}
