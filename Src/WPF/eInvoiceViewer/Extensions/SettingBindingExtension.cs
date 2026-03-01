using System.Windows.Data;
using tulo.eInvoice.eInvoiceViewer.Properties;

namespace tulo.eInvoice.eInvoiceViewer.Extensions;

public class SettingBindingExtension : Binding
{
    public SettingBindingExtension()
    {
        Initialize();
    }

    public SettingBindingExtension(string path) : base(path)
    {
        Initialize();
    }

    private void Initialize()
    {
        Source = Settings.Default;
        Mode = BindingMode.TwoWay;
    }
}
