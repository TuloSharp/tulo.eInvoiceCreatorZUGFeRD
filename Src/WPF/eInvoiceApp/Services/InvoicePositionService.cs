using tulo.CoreLib.Components.ResultPattern;
using tulo.eInvoice.eInvoiceApp.DTOs;
using tulo.eInvoice.eInvoiceApp.Stores.Invoices;

namespace tulo.eInvoice.eInvoiceApp.Services;

public sealed class InvoicePositionService(IInvoicePositionStore invoicePositionStore) : IInvoicePositionService
{
    private readonly IInvoicePositionStore _invoicePositionStore = invoicePositionStore;

    public bool IsLoaded { get; private set; }
    public bool IsCreated { get; private set; }
    public bool IsUpdated { get; private set; }
    public bool IsDeleted { get; private set; }
    public bool AreRequiredFieldsFilled { get; private set; }
    public string StatusMessage { get; private set; } = string.Empty;
    public int TotalCount { get; private set; }

    public event Action<List<InvoicePositionDetailsDTO>>? InvoicePositionsLoaded;
    public event Action<Guid, InvoicePositionDetailsDTO>? InvoicePositionCreated;
    public event Action<Guid, InvoicePositionDetailsDTO>? InvoicePositionUpdated;
    public event Action<Guid>? InvoicePositionDeleted;

    public int SuggestNextPositionNo() => _invoicePositionStore.SuggestNextPositionNo();

    public async Task<OperationResult<List<InvoicePositionDetailsDTO>>> LoadAllInvoicePositionsAsync()
    {
        ResetLoadFlags();

        var res = await _invoicePositionStore.GetAllWithIdAsync();
        if (!res.Success)
        {
            StatusMessage = res.Message;
            return OperationResult<List<InvoicePositionDetailsDTO>>.Fail(StatusMessage);
        }

        var list = BuildNumberedCalculatedList(res.Data ?? []);
        TotalCount = list.Count;
        IsLoaded = true;
        StatusMessage = $"Loaded {TotalCount} invoice positions.";

        InvoicePositionsLoaded?.Invoke(list);
        return OperationResult<List<InvoicePositionDetailsDTO>>.Ok(list, StatusMessage);
    }

    public async Task<OperationResult<List<(Guid Id, InvoicePositionDetailsDTO invoicePositions)>>> GetAllInvoicePositionsWithIdAsync()
    {
        ResetLoadFlags();

        var res = await _invoicePositionStore.GetAllWithIdAsync();
        if (!res.Success)
        {
            StatusMessage = res.Message;
            return OperationResult<List<(Guid, InvoicePositionDetailsDTO)>>.Fail(StatusMessage);
        }

        var numbered = BuildNumberedCalculatedWithIdList(res.Data ?? []);
        TotalCount = numbered.Count;
        IsLoaded = true;
        StatusMessage = $"Loaded {TotalCount} invoice positions.";

        InvoicePositionsLoaded?.Invoke(numbered.Select(x => x.invoicePositions).ToList());

        return OperationResult<List<(Guid, InvoicePositionDetailsDTO)>>.Ok(numbered, StatusMessage);
    }

    public async Task<OperationResult<Guid>> AddInvoicePositionAsync(InvoicePositionDetailsDTO invPos, int? desiredPositionNo = null)
    {
        ResetMutationFlags();

        var isValid = Validate(invPos);
        AreRequiredFieldsFilled = isValid;
        if (!isValid)
        {
            StatusMessage = string.Empty;
            return OperationResult<Guid>.Fail(StatusMessage);
        }

        Recalculate(invPos);

        var res = await _invoicePositionStore.AddAsync(invPos, desiredPositionNo);
        if (!res.Success)
        {
            StatusMessage = res.Message;
            return OperationResult<Guid>.Fail(StatusMessage);
        }

        IsCreated = true;
        StatusMessage = "Invoice position created successfully.";

        InvoicePositionCreated?.Invoke(res.Data, invPos);

        return OperationResult<Guid>.Ok(res.Data, StatusMessage);
    }

    public async Task<OperationResult<Guid>> UpdateInvoicePositionAsync(Guid id, InvoicePositionDetailsDTO invPos)
    {
        ResetMutationFlags();

        var isValid = Validate(invPos);
        AreRequiredFieldsFilled = isValid;
        if (!isValid)
        {
            StatusMessage = string.Empty;
            return OperationResult<Guid>.Fail(StatusMessage);
        }

        Recalculate(invPos);

        var res = await _invoicePositionStore.UpdateAsync(id, invPos);
        if (!res.Success)
        {
            StatusMessage = res.Message;
            return OperationResult<Guid>.Fail(StatusMessage);
        }

        IsUpdated = true;
        StatusMessage = "Invoice position updated successfully.";

        InvoicePositionUpdated?.Invoke(id, invPos);

        return OperationResult<Guid>.Ok(id, StatusMessage);
    }

    public async Task<OperationResult<Guid>> DeleteInvoicePositionAsync(Guid id)
    {
        ResetMutationFlags();

        var res = await _invoicePositionStore.DeleteAsync(id);
        if (!res.Success)
        {
            StatusMessage = res.Message;
            return OperationResult<Guid>.Fail(StatusMessage);
        }

        IsDeleted = true;
        StatusMessage = "Invoice position deleted successfully.";

        InvoicePositionDeleted?.Invoke(id);
        return OperationResult<Guid>.Ok(id, StatusMessage);
    }

    public async Task<OperationResult<List<InvoicePositionDetailsDTO>>> SetInvoicePositionNoAsync(Guid id, int newPositionNo)
    {
        ResetMutationFlags();

        var res = await _invoicePositionStore.SetPositionNoAsync(id, newPositionNo);
        if (!res.Success)
        {
            StatusMessage = res.Message;
            return OperationResult<List<InvoicePositionDetailsDTO>>.Fail(StatusMessage);
        }

        IsUpdated = true;
        StatusMessage = "Position order updated.";

        var list = BuildNumberedCalculatedList(res.Data ?? []);
        TotalCount = list.Count;
        IsLoaded = true;

        InvoicePositionsLoaded?.Invoke(list);
        return OperationResult<List<InvoicePositionDetailsDTO>>.Ok(list, StatusMessage);
    }

    private void ResetMutationFlags()
    {
        IsCreated = false;
        IsUpdated = false;
        IsDeleted = false;
        StatusMessage = string.Empty;
    }

    private void ResetLoadFlags()
    {
        IsLoaded = false;
        StatusMessage = string.Empty;
    }

    private static List<InvoicePositionDetailsDTO> BuildNumberedCalculatedList(
        List<(Guid Id, InvoicePositionDetailsDTO InvoicePosition)> raw)
    {
        var list = new List<InvoicePositionDetailsDTO>(raw.Count);
        for (int i = 0; i < raw.Count; i++)
        {
            var dto = raw[i].InvoicePosition;
            dto.InvoicePositionNr = i + 1;
            Recalculate(dto);
            list.Add(dto);
        }
        return list;
    }

    private static List<(Guid Id, InvoicePositionDetailsDTO invoicePositions)> BuildNumberedCalculatedWithIdList(
        List<(Guid Id, InvoicePositionDetailsDTO InvoicePosition)> raw)
    {
        var list = new List<(Guid, InvoicePositionDetailsDTO)>(raw.Count);
        for (int i = 0; i < raw.Count; i++)
        {
            var dto = raw[i].InvoicePosition;
            dto.InvoicePositionNr = i + 1;
            Recalculate(dto);
            list.Add((raw[i].Id, dto));
        }
        return list;
    }

    private static bool Validate(InvoicePositionDetailsDTO invPos)
    {
        if (string.IsNullOrWhiteSpace(invPos.InvoicePositionDescription)) return false;
        if (invPos.InvoicePositionQuantity <= 0) return false;
        if (invPos.InvoicePositionUnitPrice < 0) return false;
        if (invPos.InvoicePositionVatRate < 0) return false;
        return true;
    }

    private static void Recalculate(InvoicePositionDetailsDTO invPos)
    {
        invPos.InvoicePositionNetAmount = invPos.InvoicePositionQuantity * invPos.InvoicePositionUnitPrice;

        var netAfterDiscount = invPos.InvoicePositionNetAmount - invPos.InvoicePositionDiscountNetAmount;
        if (netAfterDiscount < 0) netAfterDiscount = 0;
        invPos.InvoicePositionNetAmountAfterDiscount = netAfterDiscount;

        var vatFactor = 1m + (invPos.InvoicePositionVatRate / 100m);
        invPos.InvoicePositionGrossAmount = (decimal)invPos.InvoicePositionNetAmountAfterDiscount * vatFactor;
    }
}