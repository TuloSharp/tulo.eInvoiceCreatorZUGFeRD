using tulo.CoreLib.Components.ResultPattern;
using tulo.eInvoice.eInvoiceApp.DTOs;

namespace tulo.eInvoice.eInvoiceApp.Services;

public interface IInvoicePositionService
{
    bool IsLoaded { get; }
    bool IsCreated { get; }
    bool IsUpdated { get; }
    bool IsDeleted { get; }
    bool AreRequiredFieldsFilled { get; }
    string StatusMessage { get; }
    int TotalCount { get; }

    event Action<List<InvoicePositionDetailsDTO>>? InvoicePositionsLoaded;
    event Action<InvoicePositionDetailsDTO>? InvoicePositionCreated;
    event Action<InvoicePositionDetailsDTO>? InvoicePositionUpdated;
    event Action<Guid>? InvoicePositionDeleted;

    int SuggestNextPositionNo();

    Task<OperationResult<List<InvoicePositionDetailsDTO>>> LoadAllInvoicePositionsAsync();

    Task<OperationResult<Guid>> AddInvoicePositionAsync(InvoicePositionDetailsDTO invPos, int? desiredPositionNo = null);
    Task<OperationResult<Guid>> UpdateInvoicePositionAsync(Guid id, InvoicePositionDetailsDTO invPos);
    Task<OperationResult<Guid>> DeleteInvoicePositionAsync(Guid id);

    Task<OperationResult<List<InvoicePositionDetailsDTO>>> SetInvoicePositionNoAsync(Guid id, int newPositionNo);
}

