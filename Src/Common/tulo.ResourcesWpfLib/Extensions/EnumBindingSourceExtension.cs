using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using tulo.ResourcesWpfLib.Attributes;

namespace tulo.ResourcesWpfLib.Extensions;

public class EnumBindingSourceExtension : MarkupExtension
{
    public Type EnumType { get; private set; }

    public EnumBindingSourceExtension(Type enumType)
    {
        if (enumType is null || !enumType.IsEnum)
        {
            throw new Exception($"{nameof(EnumType)} must not be null and of type Enum");
        }
        EnumType = enumType;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        //make sure Enum EnumType has an OrderAtribute attached to it to prevent any visualbrush.visual errors
        var values = Enum.GetValues(EnumType).OfType<Enum>();
        var result = values.OrderBy(v => v, new EnumSorter());
        return result;
    }

    public class EnumSorter : IComparer<Enum>
    {
        public int Compare(Enum x, Enum y)
        {
            var xOrderAttribute = x.GetType().GetField(x.ToString()).GetCustomAttribute<OrderAttribute>();
            var yOrderAttribute = y.GetType().GetField(y.ToString()).GetCustomAttribute<OrderAttribute>();
            if (xOrderAttribute == null || yOrderAttribute == null)
            {
                return 0;
            }
            return xOrderAttribute._priority.CompareTo(yOrderAttribute._priority);
        }
    }
}
