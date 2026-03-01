using tulo.eInvoice.eInvoiceApp.DTOs;

namespace tulo.eInvoice.eInvoiceApp.Models;
public class InvoicePositionEntry
{
    public Guid Id { get; init; }
    public InvoicePositionDetailsDTO InvoicePositions { get; set; } = new();
}
