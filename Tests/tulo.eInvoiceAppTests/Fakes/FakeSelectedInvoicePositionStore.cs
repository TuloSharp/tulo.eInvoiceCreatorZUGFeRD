using tulo.eInvoice.eInvoiceApp.DTOs;
using tulo.eInvoice.eInvoiceApp.Stores.Invoices;

namespace tulo.eInvoiceAppTests.Fakes;

public class FakeSelectedInvoicePositionStore : ISelectedInvoicePositionStore
{
    public event Action? SelectedInvoicePositionChanged;

    public Guid? SelectedInvoicePositionId { get; set; }
    public InvoicePositionDetailsDTO? SelectedInvoicePosition { get; set; }

    public void RaiseSelectionChanged() => SelectedInvoicePositionChanged?.Invoke();
}
