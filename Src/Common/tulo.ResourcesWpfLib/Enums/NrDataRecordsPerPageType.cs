using System.ComponentModel;
using tulo.ResourcesWpfLib.Attributes;

namespace tulo.ResourcesWpfLib.Enums
{
    public enum NrDataRecordsPerPageType
    {
        [Description("all"), Order(0)]
        All = 0,
        [Description("50"), Order(1)]
        Fifty = 1,
        [Description("100"), Order(2)]
        OneHundred = 2,
    }
}
