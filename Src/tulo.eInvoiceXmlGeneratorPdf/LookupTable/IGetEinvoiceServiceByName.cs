using tulo.XMLeInvoiceToPdf.Services;

namespace tulo.XMLeInvoiceToPdf.LookupTable;
/// <summary>
/// Provides a factory lookup to resolve a specific <see cref="IPdfGeneratorFromInvoice"/>
/// implementation by its registered name.
/// Enables dynamic selection of the appropriate PDF generator
/// based on the e-invoicing standard or profile required
/// (e.g. ZUGFeRD, Factur-X, XRechnung).
/// </summary>
public interface IGetEinvoiceServiceByName
{
    /// <summary>
    /// Resolves and returns the <see cref="IPdfGeneratorFromInvoice"/> implementation
    /// registered under the specified name.
    /// </summary>
    /// <param name="name">
    /// The registered name of the PDF generator service to resolve
    /// (e.g. <c>"ZUGFeRD"</c>, <c>"FacturX"</c>, <c>"XRechnung"</c>).
    /// Must not be <see langword="null"/> or empty.
    /// </param>
    /// <returns>
    /// The matching <see cref="IPdfGeneratorFromInvoice"/> implementation,
    /// or <see langword="null"/> if no service is registered under the given name.
    /// </returns>
    IPdfGeneratorFromInvoice? GetServiceByName(string name);
}
