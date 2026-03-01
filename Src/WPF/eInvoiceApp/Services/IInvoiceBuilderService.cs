using tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;
using tulo.eInvoiceXmlGeneratorCii.Models;

namespace tulo.eInvoice.eInvoiceApp.Services;

public interface IInvoiceBuilderService
{
    // Builds/fills the Invoice model using data from the ViewModel, position store and appsettings.
    // Returns a fully prepared Invoice object ready for further processing (XML/PDF/etc.).
    Task<Invoice> BuildAsync(InvoiceViewModel vm, CancellationToken ct = default);
}