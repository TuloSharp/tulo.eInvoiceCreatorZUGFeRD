using tulo.CommonMVVM.Collector;
using tulo.CoreLib.Components.ResultPattern;
using tulo.CoreLib.Translators;
using tulo.eInvoice.eInvoiceApp.DTOs;
using tulo.eInvoice.eInvoiceApp.Services;
using tulo.eInvoice.eInvoiceApp.Stores.Invoices;

namespace tulo.eInvoiceAppTests.Fakes;

// FakeInvoicePositionService.cs
public class FakeInvoicePositionService : IInvoicePositionService
{
    // Simple writable properties so tests can inspect or manipulate service state
    public bool IsLoaded { get; set; }
    public bool IsCreated { get; set; }
    public bool IsUpdated { get; set; }
    public bool IsDeleted { get; set; }
    public bool AreRequiredFieldsFilled { get; set; } = true;
    public string StatusMessage { get; set; } = string.Empty;
    public int TotalCount { get; set; }

    public event Action<List<InvoicePositionDetailsDTO>>? InvoicePositionsLoaded;
    public event Action<InvoicePositionDetailsDTO>? InvoicePositionCreated;
    public event Action<InvoicePositionDetailsDTO>? InvoicePositionUpdated;
    public event Action<Guid>? InvoicePositionDeleted;

    // Helper methods so tests can manually raise events
    public void RaiseLoaded(List<InvoicePositionDetailsDTO> list)
        => InvoicePositionsLoaded?.Invoke(list);

    public void RaiseCreated(InvoicePositionDetailsDTO dto)
        => InvoicePositionCreated?.Invoke(dto);

    public void RaiseUpdated(InvoicePositionDetailsDTO dto)
        => InvoicePositionUpdated?.Invoke(dto);

    public void RaiseDeleted(Guid id)
        => InvoicePositionDeleted?.Invoke(id);

    // Simple in-memory counter for SuggestNextPositionNo()
    private int _nextPositionNo = 1;
    public int SuggestNextPositionNo() => _nextPositionNo++;

    public Exception? ExceptionToThrow { get; set; }

    public Task<OperationResult<List<InvoicePositionDetailsDTO>>> LoadAllInvoicePositionsAsync()
    {
        if (ExceptionToThrow is not null)
            // Return a faulted Task instead of throwing synchronously
            // This correctly mimics how a real async service would fail
            return Task.FromException<OperationResult<List<InvoicePositionDetailsDTO>>>(ExceptionToThrow);

        IsLoaded = true;

        // For ViewModel tests we don't need a real DB; return an empty successful result
        var result = OperationResult<List<InvoicePositionDetailsDTO>>.Ok(new List<InvoicePositionDetailsDTO>());
        return Task.FromResult(result);
    }

    public Task<OperationResult<Guid>> AddInvoicePositionAsync(InvoicePositionDetailsDTO invPos, int? desiredPositionNo = null)
    {
        IsCreated = true;
        TotalCount++;

        var id = invPos.Id == Guid.Empty ? Guid.NewGuid() : invPos.Id;
        invPos.Id = id;

        // In production this would persist to the DB;
        // for tests we just raise the event so the ViewModel can react.
        InvoicePositionCreated?.Invoke(invPos);

        var result = OperationResult<Guid>.Ok(id);
        return Task.FromResult(result);
    }

    public Task<OperationResult<Guid>> UpdateInvoicePositionAsync(Guid id, InvoicePositionDetailsDTO invPos)
    {
        IsUpdated = true;

        invPos.Id = id;
        InvoicePositionUpdated?.Invoke(invPos);

        var result = OperationResult<Guid>.Ok(id);
        return Task.FromResult(result);
    }

    public Task<OperationResult<Guid>> DeleteInvoicePositionAsync(Guid id)
    {
        IsDeleted = true;
        TotalCount = Math.Max(0, TotalCount - 1);

        InvoicePositionDeleted?.Invoke(id);

        var result = OperationResult<Guid>.Ok(id);
        return Task.FromResult(result);
    }

    public Task<OperationResult<List<InvoicePositionDetailsDTO>>> SetInvoicePositionNoAsync(Guid id, int newPositionNo)
    {
        // For ViewModel tests, we typically do not need real reordering logic.
        // We just return a successful result to keep the VM logic moving.
        var result = OperationResult<List<InvoicePositionDetailsDTO>>.Ok(new List<InvoicePositionDetailsDTO>());
        return Task.FromResult(result);
    }
}

