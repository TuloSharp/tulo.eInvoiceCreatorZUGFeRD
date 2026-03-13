using System.Globalization;

namespace tulo.eInvoice.eInvoiceApp.Services;
public interface ICultureService
{
    CultureInfo CurrentCulture { get; }
    IReadOnlyList<CultureInfo> SupportedCultures { get; }
    CultureInfo ResolveStartupCulture();
    void ApplyCulture(string cultureName);
    void ApplyCulture(CultureInfo culture);
}
