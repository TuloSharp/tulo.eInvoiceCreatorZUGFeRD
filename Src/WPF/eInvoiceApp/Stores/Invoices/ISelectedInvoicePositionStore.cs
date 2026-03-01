using tulo.eInvoice.eInvoiceApp.DTOs;

namespace tulo.eInvoice.eInvoiceApp.Stores.Invoices;
public interface ISelectedInvoicePositionStore
{
    Guid? SelectedInvoicePositionId { get; set; }
    /// <summary>
    /// the current selected dataset is stored
    /// </summary>
    InvoicePositionDetailsDTO SelectedInvoicePosition { get; set; }

    /// <summary>
    /// Action to update the selected dataset into the store
    /// </summary>
    event Action SelectedInvoicePositionChanged;
}