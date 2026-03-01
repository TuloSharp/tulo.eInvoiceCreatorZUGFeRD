using System;

namespace tulo.ResourcesWpfLib.Attributes
{
    /// <summary>
    /// OrderAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class OrderAttribute : Attribute
    {
        /// <summary>
        /// Order (Priority) of Enum-Values
        /// </summary>
        public int _priority;

        /// <summary>
        /// set priority of Enum-Values
        /// </summary>
        /// <param name="priority">priority as int</param>
        public OrderAttribute(int priority)
        {
            _priority = priority;
        }
    }
}
