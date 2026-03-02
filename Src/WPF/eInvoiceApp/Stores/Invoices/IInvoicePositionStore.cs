using tulo.CoreLib.Components.ResultPattern;
using tulo.eInvoice.eInvoiceApp.DTOs;

namespace tulo.eInvoice.eInvoiceApp.Stores.Invoices;
public interface IInvoicePositionStore
{
    int SuggestNextPositionNo();

    Task<OperationResult<Guid>> AddAsync(InvoicePositionDetailsDTO dto, int? desiredPositionNo = null);
    Task<OperationResult<Guid>> UpdateAsync(Guid id, InvoicePositionDetailsDTO dto);
    Task<OperationResult<Guid>> DeleteAsync(Guid id);

    Task<OperationResult<List<InvoicePositionDetailsDTO>>> GetAllAsync();
    Task<OperationResult<List<InvoicePositionDetailsDTO>>> SetPositionNoAsync(Guid id, int newPositionNo);
}
