using tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;
using tulo.eInvoiceXmlGeneratorCii.Models;

namespace tulo.eInvoice.eInvoiceApp.Services;

/// <summary>
/// Provides functionality to assemble a complete <see cref="Invoice"/> model
/// from ViewModel data, position store entries, and application settings.
/// The resulting invoice is ready for further processing such as XML generation or PDF rendering.
/// </summary>
public interface IInvoiceBuilderService
{
    /// <summary>
    /// Asynchronously builds a fully prepared <see cref="Invoice"/> model
    /// by aggregating data from the provided <see cref="InvoiceViewModel"/>,
    /// the current invoice position store, and application settings.
    /// </summary>
    /// <param name="vm">
    /// The <see cref="InvoiceViewModel"/> containing the invoice header data
    /// such as buyer, seller, payment terms, and document metadata.
    /// </param>
    /// <param name="ct">
    /// An optional <see cref="CancellationToken"/> to cancel the build operation.
    /// Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to a fully populated
    /// <see cref="Invoice"/> object ready for XML generation, PDF rendering,
    /// or any other downstream processing.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via <paramref name="ct"/>.
    /// </exception>
    Task<Invoice> BuildAsync(InvoiceViewModel vm, CancellationToken ct = default);
}
